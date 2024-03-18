package main

import (
	"bytes"
	"fmt"
	"image"
	"image/draw"
	"image/jpeg"
	"io/ioutil"
	"net/http"
	"os"
	"strings"
)

func ConvertImageToPixel(source image.Image) (image.Image, string) {
	const (
		H_CELL = 100
		W_CELL = 100
	)

	bounds := source.Bounds()
	result := image.NewRGBA(bounds)
	var colors []string

	draw.Draw(result, bounds, source, image.Point{}, draw.Src)

	for y := 0; y < bounds.Max.Y; y += H_CELL {
		for x := 0; x < bounds.Max.X; x += W_CELL {
			// Отримання кольору пікселя
			pixelColor := source.At(x, y)

			// Заповнення прямокутника кольором пікселя
			for i := 0; i < H_CELL; i++ {
				for j := 0; j < W_CELL; j++ {
					result.Set(x+j, y+i, pixelColor)
				}
			}

			// Конвертування кольору пікселя у рядок і додавання його до списку кольорів
			r, g, b, _ := pixelColor.RGBA()
			color := fmt.Sprintf("%d, %d, %d", r>>8, g>>8, b>>8)
			colors = append(colors, color)
		}
	}

	// Побудова рядка з усіх кольорів
	colorString := strings.Join(colors, " | ")

	return result, colorString
}

func photoInfoHandler(w http.ResponseWriter, r *http.Request) {
	if r.Method != "POST" {
		http.Error(w, "Method not allowed", http.StatusMethodNotAllowed)
		return
	}

	body, err := ioutil.ReadAll(r.Body)
	if err != nil {
		http.Error(w, "Error reading request body", http.StatusInternalServerError)
		return
	}

	// Декодування зображення
	img, _, err := image.Decode(bytes.NewReader(body))
	if err != nil {
		http.Error(w, "Error decoding image", http.StatusInternalServerError)
		return
	}

	// Конвертування та погіршення якості зображення
	imgWithPixels, colorString := ConvertImageToPixel(img)

	// Збереження зображення на локальний комп'ютер
	outputFile, err := os.Create("image.jpg")
	if err != nil {
		http.Error(w, "Error creating output file", http.StatusInternalServerError)
		return
	}
	defer outputFile.Close()

	// Збереження зображення у форматі JPEG з погіршеною якістю
	err = jpeg.Encode(outputFile, imgWithPixels, nil)
	if err != nil {
		http.Error(w, "Error saving image", http.StatusInternalServerError)
		return
	}

	// Вивід інформації про кольори у вигляді рядка
	fmt.Fprintf(w, colorString)
}

func main() {
	http.HandleFunc("/photoinf", photoInfoHandler)

	fmt.Println("Server is listening...")
	http.ListenAndServe("localhost:8181", nil)
}
