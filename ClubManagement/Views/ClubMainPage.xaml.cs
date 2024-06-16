using System;
using System.Collections.Generic;
using System.IO;
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
using ClubManagement.Models;

namespace ClubManagement.Views
{
    /// <summary>
    /// ClubMainPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClubMainPage : Page
    {
        private List<Club> allClubs;
        private List<Club> myClubs;
        private int sid;
        private readonly HttpClient _httpClient;

        public ClubMainPage(int sId)
        {
            InitializeComponent();
            allClubs = new List<Club>();
            myClubs = new List<Club>();
            sid = sId;
            _httpClient = new HttpClient();
            LoadClubs(sid);
        }

        private async void LoadClubs(int sId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{Properties.Settings.Default.serverUrl}/api/clubs");
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
        }

        private async Task<BitmapImage> LoadImageFromServerAsync(string imageUrl)
        {
            string uri = $"{Properties.Settings.Default.serverUrl}/api/files/images/{imageUrl}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();

                    var imageData = await response.Content.ReadAsByteArrayAsync();

                    BitmapImage bitmapImage = new BitmapImage();
                    using (var stream = new MemoryStream(imageData))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                    }

                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async void DisplayClubs(List<Club> clubs)
        {
            ClubStackPanel.Children.Clear();
            foreach (var club in clubs)
            {
                var clubGrid = new Grid { Margin = new Thickness(5) };
                clubGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                clubGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                clubGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // 행 정의 추가
                clubGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                clubGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var image = new Image
                {
                    Width = 50,
                    Height = 50,
                    Margin = new Thickness(5)
                };

                if (club.ImagePath != null)
                {
                    var bitmapImage = await LoadImageFromServerAsync(club.ImagePath);
                    if (bitmapImage != null)
                    {
                        image.Source = bitmapImage;
                    }
                }

                var nameTextBlock = new TextBlock
                {
                    Text = club.ClubName,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5)
                };

                var descriptionTextBlock = new TextBlock
                {
                    Text = club.Description,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 12,
                    Margin = new Thickness(5)
                };

                var statusTextBlock = new TextBlock
                {
                    Text = "인원 현황: " + club.count.ToString() + "/" + club.maxCount.ToString(),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(5)
                };

                // Grid에 요소 추가
                Grid.SetColumn(image, 0);
                Grid.SetRowSpan(image, 2); // 이미지가 두 행을 차지하도록 설정
                Grid.SetColumn(nameTextBlock, 1);
                Grid.SetRow(nameTextBlock, 0);
                Grid.SetColumn(descriptionTextBlock, 1);
                Grid.SetRow(descriptionTextBlock, 1);
                Grid.SetColumn(statusTextBlock, 2);
                Grid.SetRowSpan(statusTextBlock, 2); // 상태 텍스트가 두 행을 차지하도록 설정

                clubGrid.Children.Add(image);
                clubGrid.Children.Add(nameTextBlock);
                clubGrid.Children.Add(descriptionTextBlock);
                clubGrid.Children.Add(statusTextBlock);

                var border = new Border()
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Child = clubGrid,
                    Margin = new Thickness(1),
                    Cursor = Cursors.Hand // 커서 변경
                };

                border.MouseLeftButtonUp += (sender, e) => Border_Click(sender, e, club);

                // 메인 스택패널에 동아리 패널 추가
                ClubStackPanel.Children.Add(border);
            }
        }
        private void Border_Click(object sender, MouseButtonEventArgs e, Club club)
        {
            ClubDetailPage clubDetailPage = new ClubDetailPage(club, sid);
            clubDetailPage.Show();
            // 여기서 필요한 작업을 수행할 수 있습니다.
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
                        mainWindow.MainFrame.Navigate(new MakeClubMenu(sid)); // 동아리 신설 신청 페이지로 이동
                        break;
                    case "신설 신청 현황":
                        mainWindow.MainFrame.Navigate(new MakeClubStatus(sid)); // 신설 신청 현황 페이지로 이동
                        break;
                }
                ClubMenu.SelectedItem = null;
                ClubMenu.Visibility = Visibility.Collapsed; // 드롭다운 메뉴를 숨김
            }
        }

        private void ReLoad(object sender, RoutedEventArgs e)
        {
            myClubs.Clear();
            allClubs.Clear();
            LoadClubs(sid);
        }
    }
}
