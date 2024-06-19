using ClubManagement.Model;
using ClubManagement.Models;
using ClubManagement.Views;
using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClubManagement
{
    public partial class ClubBoardPage : Window
    {
        private Club club;
        private int sid;

        public ObservableCollection<Post> Posts { get; set; }

        public ClubBoardPage(Club club, int sid)
        {
            InitializeComponent();
            DataContext = this;
            this.club = club;
            this.sid = sid;
            Posts = new ObservableCollection<Post>();
            LoadDataFromDatabase();
        }

        private void LoadDataFromDatabase()
        {
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
                            Posts.Add(new Post
                            {
                                PostID = reader.GetInt32("PostID"),
                                ClubID = reader.GetInt32("ClubID"),
                                Title = reader.GetString("Title"),
                                Content = reader.GetString("Content"),
                                AuthorName = reader.GetString("AuthorName"),
                                PostDate = reader.GetDateTime("PostDate"),
                                FilePath = reader["FilePath"] as string
                            });
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
            this.Close();
        }

        private void WritePost_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID == sid)
            {
                PostCreatePage postCreatePage = new PostCreatePage(club.ClubID, sid);
                postCreatePage.ShowDialog();
                RefreshPosts();
            }
            else
            {
                MessageBox.Show("접근 권한이 없습니다.");
            }
        }

        private void Post_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is Post selectedPost)
            {
                OpenPage openPage = new OpenPage(selectedPost, sid);
                openPage.ShowDialog();
            }
        }

        private void RefreshPosts()
        {
            Posts.Clear();
            LoadDataFromDatabase();
        }
    }
}
