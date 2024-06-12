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

namespace ClubManagement
{
    /// <summary>
    /// MemberManagement.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MemberManagement : Window
    {
        public ObservableCollection<object> Members { get; set; }
        public MemberManagement()
        {
            InitializeComponent();
            
            LoadMembersFromDatabase();
            DataContext = this;
        }
        private void LoadMembersFromDatabase()
        {
            Members = new ObservableCollection<object>();
            try
            {
                // 데이터베이스 연결 및 쿼리 실행
                string connectionString = "Server=localhost;Database=club;Uid=club;Pwd=club;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM student";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["student_id"]);
                            string name = reader["name"].ToString();
                            string role = reader["role"].ToString();
                            string department = reader["department"].ToString();

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
            ClubDetailPage clubDetailPage = new ClubDetailPage();
            clubDetailPage.Show();
            this.Close();
        }
       
    }

    

}
