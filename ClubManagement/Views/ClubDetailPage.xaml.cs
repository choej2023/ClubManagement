using ClubManagement.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ClubManagement.Views
{
    public partial class ClubDetailPage : Page
    {
        private List<Club> allClubs;
        private List<Club> myClubs;

        public ClubDetailPage()
        {
            InitializeComponent();
            LoadClubs();
        }

        private void LoadClubs()
        {
            // 모든 동아리 정보를 리스트로 저장
            allClubs = new List<Club>
            {
                new Club { Name = "동아리명 1", Description = "한 줄 소개", Status = "가입 가능", ImagePath = "pack://application:,,,/Resources/kumohImg.jpg" },
                new Club { Name = "동아리명 2", Description = "한 줄 소개", Status = "가입 불가", ImagePath = "pack://application:,,,/Resources/kumohImg.jpg" }
            };
            // 사용자의 동아리 정보를 리스트로 저장
            myClubs = new List<Club>
            {
                new Club { Name = "내 동아리명 1", Description = "한 줄 소개", Status = "가입 가능", ImagePath = "pack://application:,,,/Resources/kumohImg.jpg" }
            };
            // 초기에는 모든 동아리를 표시
            DisplayClubs(allClubs);
        }

        private void DisplayClubs(List<Club> clubs)
        {
            ClubStackPanel.Children.Clear();
            foreach (var club in clubs)
            {
                // 각 동아리에 대해 스택패널 생성
                var clubPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                var image = new Image
                {
                    Source = new BitmapImage(new Uri(club.ImagePath)),
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(5)
                };

                var nameTextBlock = new TextBlock
                {
                    Text = club.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                var descriptionTextBlock = new TextBlock
                {
                    Text = club.Description,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                var statusTextBlock = new TextBlock
                {
                    Text = club.Status,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5)
                };

                // 스택패널에 요소 추가
                clubPanel.Children.Add(image);
                clubPanel.Children.Add(nameTextBlock);
                clubPanel.Children.Add(descriptionTextBlock);
                clubPanel.Children.Add(statusTextBlock);

                // 메인 스택패널에 동아리 패널 추가
                ClubStackPanel.Children.Add(clubPanel);
            }
        }

        private void ShowAllClubs(object sender, RoutedEventArgs e)
        {
            DisplayClubs(allClubs);
        }

        private void ShowMyClubs(object sender, RoutedEventArgs e)
        {
            DisplayClubs(myClubs);
        }

        private void ToggleClubMenu(object sender, RoutedEventArgs e)
        {
            ClubMenu.Visibility = ClubMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            // 로그아웃 버튼 클릭 시 메인 윈도우로 돌아가도록 처리
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new LoginPage()); // 또는 로그인 페이지로 이동
        }

        private void ClubMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClubMenu.SelectedItem != null)
            {
                var selectedItem = (ComboBoxItem)ClubMenu.SelectedItem;
                var mainWindow = (MainWindow)Application.Current.MainWindow;

                switch (selectedItem.Content.ToString())
                {
                    case "동아리 신설 신청":
                        mainWindow.MainFrame.Navigate(new MakeClubMenu()); // 동아리 신설 신청 페이지로 이동
                        break;
                    case "신설 신청 현황":
                        mainWindow.MainFrame.Navigate(new MakeClubStatus()); // 신설 신청 현황 페이지로 이동
                        break;
                }
                ClubMenu.Visibility = Visibility.Collapsed; // 드롭다운 메뉴를 숨김
            }
        }
    }
}
