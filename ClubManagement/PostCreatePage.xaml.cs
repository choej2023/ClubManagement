using Microsoft.Win32;
using MySqlConnector;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace ClubManagement
{
    /// <summary>
    /// PostCreatePage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PostCreatePage : Window
    {
        private int clubId ,sid;
        private string selectedFilePath;

        public PostCreatePage(int clubId, int sid)
        {
            InitializeComponent();
            this.clubId = clubId;
            this.sid = sid;
        }

        private async void Register_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string content = ContentTextBox.Text;

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("제목과 내용을 입력해주세요.");
                return;
            }

            string filePath = null;
            if (!string.IsNullOrEmpty(selectedFilePath))
            {
                filePath = await UploadFileToServer(selectedFilePath);
            }

            SavePostToDatabase(title, content, clubId, filePath);
            MessageBox.Show("게시글이 등록되었습니다.");
            this.Close();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                FilePathTextBlock.Text = selectedFilePath;
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

        private void SavePostToDatabase(string title, string content, int clubId, string filePath)
        {
            string connectionString = "Server=localhost;Database=clubmanagement;User ID=root;Password=root;";
            string authorName = "";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // student 테이블에서 작성자 이름을 검색
                string getAuthorQuery = "SELECT Name FROM student WHERE StudentID = @StudentID";
                using (MySqlCommand getAuthorCommand = new MySqlCommand(getAuthorQuery, connection))
                {
                    getAuthorCommand.Parameters.AddWithValue("@StudentID", sid);
                    using (MySqlDataReader reader = getAuthorCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            authorName = reader.GetString("Name");
                        }
                    }
                }

                // post 테이블에 게시글 저장
                string query = "INSERT INTO post (ClubID, Title, Content, AuthorName, PostDate, FilePath) VALUES (@ClubID, @Title, @Content, @AuthorName, @PostDate, @FilePath)";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClubID", clubId);
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Content", content);
                    command.Parameters.AddWithValue("@AuthorName", authorName); // 검색된 작성자 이름 사용
                    command.Parameters.AddWithValue("@PostDate", DateTime.Now);
                    command.Parameters.AddWithValue("@FilePath", filePath);

                    command.ExecuteNonQuery();
                }
            }
        }

    }
}
