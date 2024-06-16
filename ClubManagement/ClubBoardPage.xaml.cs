using ClubManagement.Models;
using ClubManagement.Views;
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
        private Club club;
        private int sid;

        public ObservableCollection<object> Posts { get; set; }

        public ClubBoardPage(Club club, int sid)
        {
            InitializeComponent();
            DataContext = this;
            this.club = club;
            this.sid = sid;
            LoadDataFromDatabase();
        }

        private void LoadDataFromDatabase()
        {
            Posts = new ObservableCollection<object>();
            int i = 1;

            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM post WHERE ClubID = @ClubID";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClubID", club.ClubID);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int index = Convert.ToInt32(reader["ClubID"]);
                            string title = reader["Title"].ToString();
                            string author = reader["Authorname"].ToString();
                            DateTime datePosted = Convert.ToDateTime(reader["PostDate"]);
                            string content = reader["Content"].ToString();

                            // 가져온 데이터를 익명 객체로 생성하여 ObservableCollection에 추가
                            Posts.Add(new { Index = i++, Title = title, Author = author, DatePosted = datePosted, Content = content });
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
            ClubDetailPage clubDetailPage = new ClubDetailPage(club, sid);
            clubDetailPage.Show();
            this.Close();
        }

        private void WritePost_Click(object sender, RoutedEventArgs e)
        {
            PostCreatePage postCreatePage = new PostCreatePage(club.ClubID);
            postCreatePage.ShowDialog();
            // 데이터가 변경되었으므로, 목록을 갱신합니다.
            RefreshPosts();
        }
        private void RefreshPosts()
        {
            Posts.Clear();
            string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Title, AuthorName, PostDate FROM post WHERE ClubID = @ClubID ORDER BY PostDate DESC";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClubID", club.ClubID);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Posts.Add(new Post
                            {
                                Title = reader.GetString("Title"),
                                Author = reader.GetString("AuthorName"),
                                DatePosted = reader.GetDateTime("PostDate")
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
