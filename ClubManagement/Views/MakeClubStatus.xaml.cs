using ClubManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClubManagement.Views
{
    public partial class MakeClubStatus : Page
    {
        private HttpClient _httpClient;
        public MakeClubStatus(List<Models.Club> myClubs)
        {
            InitializeComponent();
        }

        
        private void Go_Back(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            if (mainWindow.MainFrame.CanGoBack)
            {
                mainWindow.MainFrame.GoBack();
            }
        }
        /*
        private void LoadStatus()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{Properties.Settings.Default.serverUrl}/api/clubstatus");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var clubs = JsonSerializer.Deserialize<List<Club>>(responseBody);

                foreach (var club in clubs)
                {
                    allClubs.Add(club);
                    if (club.StudentID == sId)
                        myClubs.Add(club);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }

            DisplayClubs(allClubs);
            // 예제 데이터를 추가합니다.
            var statusList = new List<(string ClubName, string ReviewDate, string Status)>
            {
                ("동아리명 1", "검토일자 1", "승인됨"),
                ("동아리명 2", "검토일자 2", "거절됨")
                // 더 많은 데이터를 추가합니다.
            };

            foreach (var status in statusList)
            {
                var statusPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5)};


                var clubNameTextBlock = new TextBlock
                {
                    Text = status.ClubName,
                    Width = 150,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                var reviewDateTextBlock = new TextBlock
                {
                    Text = status.ReviewDate,
                    Width = 150,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                var statusTextBlock = new TextBlock
                {
                    Text = status.Status,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                statusPanel.Children.Add(clubNameTextBlock);
                statusPanel.Children.Add(reviewDateTextBlock);
                statusPanel.Children.Add(statusTextBlock);

                var border = new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Child = statusPanel,
                    Margin = new Thickness(1)
                };

                StatusStackPanel.Children.Add(border);
            }

    }
                */
    }
}