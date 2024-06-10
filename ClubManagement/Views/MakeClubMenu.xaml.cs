using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ClubManagement.Models; // 추가된 네임스페이스

namespace ClubManagement.Views
{
    public partial class MakeClubMenu : Page
    {
        private string _uploadedImagePath; // 업로드된 이미지 경로를 저장할 필드
        private int sid;
        public MakeClubMenu(int sId)
        {
            InitializeComponent();
            sid = sId;
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            // 이전 페이지로 이동하는 로직 (MainWindow의 NavigationService 사용)
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private async void NextPage_Click(object sender, RoutedEventArgs e)
        {
            // 이미지를 먼저 업로드
            if (ClubImage.Tag != null)
            {
                _uploadedImagePath = await UploadImageAsync(ClubImage.Tag.ToString());
            }

            // 현재 페이지에서 입력된 데이터를 다음 페이지로 전달
            var club = new Club
            {
                StudentID = this.sid,
                ClubName = ClubNameTextBox.Text,
                ShortDescription = ShortDescriptionTextBox.Text,
                Description = DescriptionTextBox.Text,
                maxCount = int.Parse(MaxCountTextBox.Text),
                ImagePath = _uploadedImagePath,  // 업로드된 이미지 경로 저장
            };

            // 다음 페이지로 이동
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new ClubApplicationFormPage(club));
        }

        private void ImageBorder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void ImageBorder_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void ImageBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    var filePath = files[0];
                    try
                    {
                        var bitmap = new BitmapImage(new Uri(filePath));
                        ClubImage.Source = bitmap;
                        ClubImage.Visibility = Visibility.Visible;
                        ImageTextBlock.Visibility = Visibility.Collapsed;
                        ClubImage.Tag = filePath;  // 이미지 경로를 태그에 저장
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"이미지 로드 중 오류가 발생했습니다: {ex.Message}");
                    }
                }
            }
        }

        private async Task<string> UploadImageAsync(string filePath)
        {
            using (var client = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                content.Add(fileContent, "file", Path.GetFileName(filePath));

                var response = await client.PostAsync($"{Properties.Settings.Default.serverUrl}/{Properties.Settings.Default.upload}", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return result; // 서버에서 반환된 이미지 경로를 그대로 반환
                }
                else
                {
                    MessageBox.Show("이미지 업로드에 실패했습니다.");
                    return null;
                }
            }
        }
    }
}
