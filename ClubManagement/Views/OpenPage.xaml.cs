using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MySqlConnector;
using ClubManagement.Model;

namespace ClubManagement.Views
{
    public partial class OpenPage : Window
    {
        private string filePath;
        private Post post;
        private int sid;
        private string selectedFilePath;

        public OpenPage(Post post, int sid)
        {
            InitializeComponent();
            TitleTextBox.Text = post.Title;
            AuthorTextBox.Text = post.AuthorName;
            DateTextBox.Text = post.PostDate.ToString("yyyy-MM-dd");
            ContentTextBox.Text = post.Content;
            filePath = post.FilePath;
            this.post = post;
            this.sid = sid;

            DisplayFile(filePath);
        }

        private void DisplayFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();

            if (fileExtension == ".jpg" || fileExtension == ".jpeg" || fileExtension == ".png" || fileExtension == ".bmp")
            {
                Image image = new Image();
                BitmapImage bitmap = new BitmapImage(new Uri(filePath, UriKind.RelativeOrAbsolute));
                image.Source = bitmap;
                image.MaxHeight = 200;
                FilePreviewBorder.Child = image;
            }
            else if (fileExtension == ".mp4" || fileExtension == ".avi" || fileExtension == ".mov" || fileExtension == ".wmv")
            {
                MediaElement mediaElement = new MediaElement();
                mediaElement.Source = new Uri(filePath, UriKind.RelativeOrAbsolute);
                mediaElement.LoadedBehavior = MediaState.Manual;
                mediaElement.UnloadedBehavior = MediaState.Manual;
                mediaElement.Height = 200;
                mediaElement.Play();
                FilePreviewBorder.Child = mediaElement;
            }
            else
            {
                DownloadButton.Visibility = Visibility.Visible;
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sid != post.StudentID)
            {
                MessageBox.Show("접근 권한이 없습니다!");
                return;
            }
            // TextBox를 편집 가능하게 설정
            TitleTextBox.IsReadOnly = false;
            ContentTextBox.IsReadOnly = false;

            // 파일 선택 버튼 및 텍스트블록 표시
            SelectFileButton.Visibility = Visibility.Visible;
            SelectedFilePathTextBlock.Visibility = Visibility.Visible;

            // Edit 버튼 숨기고 Save 버튼 표시
            EditButton.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // TextBox를 다시 읽기 전용으로 설정
            TitleTextBox.IsReadOnly = true;
            ContentTextBox.IsReadOnly = true;

            // 파일 선택 버튼 및 텍스트블록 숨기기
            SelectFileButton.Visibility = Visibility.Collapsed;
            SelectedFilePathTextBlock.Visibility = Visibility.Collapsed;

            // Save 버튼 숨기고 Edit 버튼 표시
            SaveButton.Visibility = Visibility.Collapsed;
            EditButton.Visibility = Visibility.Visible;

            string uploadedFilePath = filePath;
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                uploadedFilePath = await UploadFileToServer(selectedFilePath);
            }

            // 데이터베이스 업데이트
            UpdatePostInDatabase(post.PostID, TitleTextBox.Text, ContentTextBox.Text, uploadedFilePath);
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                SelectedFilePathTextBlock.Text = selectedFilePath;
            }
        }

        private async Task<string> UploadFileToServer(string filePath)
        {
            using (var httpClient = new HttpClient())
            {
                using (var form = new MultipartFormDataContent())
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var streamContent = new StreamContent(fileStream))
                        {
                            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                            form.Add(streamContent, "file", Path.GetFileName(filePath));
                            var response = await httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/{Properties.Settings.Default.upload}", form);
                            if (response.IsSuccessStatusCode)
                            {
                                var responseContent = await response.Content.ReadAsStringAsync();
                                dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseContent);
                                return result.filePath;
                            }
                            else
                            {
                                MessageBox.Show("파일 업로드에 실패했습니다.");
                                return null;
                            }
                        }
                    }
                }
            }
        }

        private void UpdatePostInDatabase(int postId, string title, string content, string filePath)
        {
            string connectionString = "Server=localhost;Database=clubmanagement;User ID=root;Password=root;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE post SET Title = @Title, Content = @Content, FilePath = @FilePath WHERE PostID = @PostID";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PostID", postId);
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Content", content);
                    command.Parameters.AddWithValue("@FilePath", filePath);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
