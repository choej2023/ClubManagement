using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
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
using ClubManagement.Models;

namespace ClubManagement
{
    /// <summary>
    /// MemberManagement.xaml에 대한 상호 작용 논리
    /// </summary>
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
                // 데이터베이스 연결 및 쿼리 실행
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

                            // 가져온 데이터를 Member 객체로 생성하여 ObservableCollection에 추가
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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ClubDetailPage clubDetailPage = new ClubDetailPage(club, sid);
            clubDetailPage.Show();
            this.Close();
        }
       
    }

    

}
