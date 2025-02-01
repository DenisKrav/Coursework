using Coursework.Models;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Coursework
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ImageInf imageInf = new ImageInf();
		static HttpClient httpClient = new HttpClient();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void downloadBtn_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new Microsoft.Win32.OpenFileDialog();
			dialog.FileName = "Imaage"; // Default file name
			dialog.DefaultExt = ".jpg"; // Default file extension
			dialog.Filter = "Text documents (.jpg, .png, .bmp)|*.jpg;*.png;*.bmp"; // Filter files by extension

			// Show open file dialog box
			bool? result = dialog.ShowDialog();

			// Process open file dialog box results
			if (result == true)
			{
				// Open document
				uploadImg.Source = new BitmapImage(new Uri(dialog.FileName));
				imageInf.ImagePath = dialog.FileName;
			}
		}

		private async void processBtn_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(imageInf.ImagePath))
				{
					MessageBox.Show("Завантажте фото.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);

					return;
				}

				byte[] convImg = ConvertImgToByte();

				var socketsHandler = new SocketsHttpHandler
				{
					PooledConnectionLifetime = TimeSpan.FromMinutes(2)
				};
				httpClient = new HttpClient(socketsHandler);

				ByteArrayContent content = new ByteArrayContent(convImg);
				// визначаємо дані запиту
				using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:8181/photoinf");
				// встановлення контенту, для відправки
				request.Content = content;
				// відправка запиту
				using HttpResponseMessage response = await httpClient.SendAsync(request);
				// отримання відповіді
				string responseText = await response.Content.ReadAsStringAsync();

				imageInf.ImageColors = GetListColorsFromString(responseText);
				foreach (var color in imageInf.ImageColors)
				{
					colorsListBox.Items.Add(color);
				}
			}
			catch (HttpRequestException ex)
			{
				MessageBox.Show($"Сервер не доступний. Детальніше про помилку: {ex.Message}.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);

				return;
			}
		}

		public byte[] ConvertImgToByte()
		{
			byte[] bData;
			using (FileStream fs = new FileStream($@"{imageInf.ImagePath}", FileMode.Open, FileAccess.Read))
			{
				bData = new byte[fs.Length];
				fs.Read(bData, 0, (int)fs.Length);
			}

			return bData;
		}

		public List<string> GetListColorsFromString(string colors)
		{
			return colors.Split(" | ").ToList();
		}
	}
}