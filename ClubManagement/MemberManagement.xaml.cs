using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ClubManagement.Models;

namespace ClubManagement
{
    public partial class MemberManagement : Window
    {
        private Club club;
        private int sid;

        public ObservableCollection<object> Members { get; set; }
        public MemberManagement(Club club, int sid)
        {
            InitializeComponent();
            DataContext = this;
            this.club = club;
            this.sid = sid;
            LoadMembersFromDatabase();
        }

        private void LoadMembersFromDatabase()
        {
            Members = new ObservableCollection<object>();
            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT s.* FROM student s JOIN clubmember cm ON s.StudentID = cm.StudentID WHERE cm.ClubID = @ClubID;";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClubID", club.ClubID);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["StudentID"]);
                            string name = reader["Name"].ToString();
                            string role = reader["Role"].ToString();
                            string department = reader["Department"].ToString();

                            Members.Add(new { MemberID = id, MemberName = name, MemberRole = role, MemberDepartment = department });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void DeleteMember_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int memberId = (int)button.Tag;

            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
                return;
            }
            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM clubmember WHERE ClubID = @ClubID AND StudentID = @StudentID;";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClubID", club.ClubID);
                    command.Parameters.AddWithValue("@StudentID", memberId);
                    command.ExecuteNonQuery();

                    // Members 컬렉션에서 해당 부원을 제거합니다.
                    var memberToRemove = Members.FirstOrDefault(m => ((dynamic)m).MemberID == memberId);
                    if (memberToRemove != null)
                    {
                        Members.Remove(memberToRemove);
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
    }
}
