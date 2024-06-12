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
        public PostCreatePage()
        {
            InitializeComponent();
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

            SavePostToDatabase(title, content);
            MessageBox.Show("게시글이 등록되었습니다.");
            this.Close();
        }

        private void SavePostToDatabase(string title, string content)
        {
            string connectionString = "Server=localhost;Database=club;User ID=club;Password=club;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO post (title, content, author_name, created_date) VALUES (@title, @content, @author_name, @created_date)";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@content", content);
                    command.Parameters.AddWithValue("@author_name", "회장"); // 작성자를 고정값으로 설정
                    command.Parameters.AddWithValue("@created_date", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
