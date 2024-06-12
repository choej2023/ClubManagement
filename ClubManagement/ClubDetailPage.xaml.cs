using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
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
using static ClubManagement.ClubDetailPage;
using System.Windows.Navigation;
using Microsoft.VisualBasic;

namespace ClubManagement
{
    
    public partial class ClubDetailPage : Window
    {
       
        public ClubDetailPage()
        {
            InitializeComponent();
            LoadDataFromDatabase(); // 데이터베이스에서 데이터를 가져와서 Club 객체 생성 및 설정
            
        }

        // 데이터베이스로부터 데이터를 읽어와 Club 객체를 생성하고 설정하는 메서드
        private void LoadDataFromDatabase()
        {
            try
            {
                string connectionString = "Server=localhost;Database=club;Uid=club;Pwd=club;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM club"; // 적절한 쿼리로 변경 필요
                    MySqlCommand command = new MySqlCommand(query, connection);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) // 첫 번째 행만 가져옴
                        {
                            // 데이터베이스에서 가져온 값으로 Club 객체 생성
                            Club club = new Club
                            {
                                ClubName = reader["name"].ToString(),
                                ClubDescription = reader["short_description"].ToString(),
                                ClubDetails = reader["description"].ToString()
                            };

                            // Club 객체를 DataContext로 설정하여 XAML에서 바인딩 가능하도록 함
                            DataContext = club;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Rectangle_Click(object sender, RoutedEventArgs e)
        {
            // 클릭된 Rectangle 가져오기
            Rectangle rectangle = sender as Rectangle;
            if (rectangle != null)
            {
                // Grid 내의 위치를 가져오기
                int row = Grid.GetRow(rectangle);
                int column = Grid.GetColumn(rectangle);

                // 대응하는 TextBlock 찾기
                string textBlockName = $"TextBlock_{row}_{column}";
                TextBlock textBlock = FindName(textBlockName) as TextBlock;

                if(textBlock != null)
                {
                    string userInput = Interaction.InputBox("Enter value:", "Input", "");
                    if (!string.IsNullOrEmpty(userInput))
                    {
                        // 사용자 입력을 Rectangle의 채우기 색으로 설정
                        rectangle.Fill = new SolidColorBrush(Colors.Red); // 예시로 빨간색 설정
                        textBlock.Text = userInput;
                    }
                }
                // 입력 창을 표시하고 사용자 입력 처리
                
                
            }
        }
        private void ShowPosts_Click(object sender, RoutedEventArgs e)
        {
            ClubBoardPage clubBoardPage = new ClubBoardPage();
            clubBoardPage.Show();
            this.Close();
        }
        private void ShowMember_Click(object sender, RoutedEventArgs e)
        {
            MemberManagement memberManagement= new MemberManagement();
            memberManagement.Show();
            this.Close();
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // 수정 페이지로 이동
            ModifyPage modifyPage= new ModifyPage((Club)DataContext);
            modifyPage.ShowDialog();

            // 수정 페이지에서 수정된 Club 객체를 가져옴
            if (modifyPage.ModifiedClub != null)
            {
                // 수정된 Club 객체로 DataContext 갱신
                DataContext = modifyPage.ModifiedClub;

                // 데이터베이스 업데이트
                UpdateDataInDatabase(modifyPage.ModifiedClub);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // 삭제 확인 메시지 출력
            MessageBoxResult result = MessageBox.Show("클럽을 삭제하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

            // 사용자가 확인을 선택한 경우에만 삭제 수행
            if (result == MessageBoxResult.Yes)
            {
                // 데이터베이스에서 클럽 삭제
                DeleteClubFromDatabase();
            }
        }

        // 데이터베이스에서 클럽 삭제 메서드
        private void DeleteClubFromDatabase()
        {
            try
            {
                string connectionString = "Server=localhost;Database=club;Uid=club;Pwd=club;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM club WHERE club_id = @club_id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    // 여기서 Club의 고유한 ID를 사용하여 삭제합니다. 예시로 ID 1을 사용하였음
                    command.Parameters.AddWithValue("@club_id", 1); // 예시로 ID 1을 사용하였음, 실제로는 Club의 고유한 ID를 사용해야 함
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("클럽이 삭제되었습니다.");
                        // 삭제 성공 시 ClubDetailPage를 닫습니다.
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("클럽 삭제에 실패했습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void UpdateDataInDatabase(Club modifiedClub)
        {
            try
            {
                string connectionString = "Server=localhost;Database=club;Uid=club;Pwd=club;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE club SET name = @name, short_description = @shortDescription, description = @description WHERE club_id = @club_id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@name", modifiedClub.ClubName);
                    command.Parameters.AddWithValue("@shortDescription", modifiedClub.ClubDescription);
                    command.Parameters.AddWithValue("@description", modifiedClub.ClubDetails);
                    // 아래 부분은 Club 객체의 고유한 ID에 맞게 설정해야 함
                    command.Parameters.AddWithValue("@club_id", 1); // 예시로 ID 1을 사용하였음, 실제로는 Club의 고유한 ID를 사용해야 함
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("데이터가 수정되었습니다.");
                    }
                    else
                    {
                        MessageBox.Show("데이터 수정에 실패했습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }



    }


    }

    // Club 클래스 정의
    public class Club
    {
        public string ClubName { get; set; }
        public string ClubDescription { get; set; }
        public string ClubDetails { get; set; }
    }
    




