using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClubManagement
{
    /// <summary>
    /// PostCreatePage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PostCreatePage : Window
    {
        private int clubId;
        public PostCreatePage(int clubId)
        {
            InitializeComponent();
            this.clubId = clubId;
        }
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string content = ContentTextBox.Text;

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                MessageBox.Show("제목과 내용을 입력해주세요.");
                return;
            }

            SavePostToDatabase(title, content, clubId);
            MessageBox.Show("게시글이 등록되었습니다.");
            this.Close();
        }

        private void SavePostToDatabase(string title, string content, int clubId)
        {
            string connectionString = "Server=localhost;Database=clubmanagement;User ID=root;Password=root;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO post (ClubID, Title, Content, AuthorName, PostDate) VALUES (@ClubID, @Title, @Content, @AuthorName, @PostDate)";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClubID", clubId);
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Content", content);
                    command.Parameters.AddWithValue("@AuthorName", "회장"); // 작성자를 고정값으로 설정
                    command.Parameters.AddWithValue("@PostDate", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
