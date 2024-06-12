using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class ClubBoardPage : Window
    {
        public ObservableCollection<object> Posts { get; set; }

        public ClubBoardPage()
        {
            InitializeComponent();
            DataContext = this;
            LoadDataFromDatabase();
        }

        private void LoadDataFromDatabase()
        {
            Posts = new ObservableCollection<object>();

            try
            {
                string connectionString = "Server=localhost;Database=club;Uid=club;Pwd=club;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM post";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = Convert.ToInt32(reader["club_id"]);
                            string title = reader["title"].ToString();
                            string author = reader["author_name"].ToString();
                            DateTime datePosted = Convert.ToDateTime(reader["created_date"]);
                            string content = reader["content"].ToString();

                            // 가져온 데이터를 익명 객체로 생성하여 ObservableCollection에 추가
                            Posts.Add(new { Index = index, Title = title, Author = author, DatePosted = datePosted, Content = content });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ClubDetailPage clubDetailPage = new ClubDetailPage();
            clubDetailPage.Show();
            this.Close();
        }

        private void WritePost_Click(object sender, RoutedEventArgs e)
        {
            PostCreatePage postCreatePage = new PostCreatePage();
            postCreatePage.ShowDialog();
            // 데이터가 변경되었으므로, 목록을 갱신합니다.
            RefreshPosts();
        }
        private void RefreshPosts()
        {
            Posts.Clear();
            string connectionString = "Server=localhost;Database=club;User ID=club;Password=club;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT club_id, title, author_name, created_date FROM post ORDER BY created_date DESC";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Posts.Add(new Post
                            {
                                Id = reader.GetInt32("club_id"),
                                Title = reader.GetString("title"),
                                Author = reader.GetString("author_name"),
                                DatePosted = reader.GetDateTime("created_date")
                            });
                        }
                    }
                }
            }
        }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime DatePosted { get; set; }
    }

}
