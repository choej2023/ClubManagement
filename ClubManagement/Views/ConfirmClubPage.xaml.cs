using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClubManagement.Model;
using ClubManagement.Models;

namespace ClubManagement.Views
{
    public partial class ConfirmClubPage : Page
    {
        private readonly int sid;
        private readonly HttpClient _httpClient;

        public ConfirmClubPage(int sId)
        {
            InitializeComponent();
            sid = sId;
            _httpClient = new HttpClient();
            LoadClubApplications();
        }

        private async void LoadClubApplications()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/api/otherclubstatus", null);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var clubApplicationForms = JsonSerializer.Deserialize<List<ClubApplicationForm>>(responseBody);

                DisplayClubApplications(clubApplicationForms);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }
        }
        private void DisplayClubApplications(List<ClubApplicationForm> clubApplicationForms)
        {
            ClubApplicationStackPanel.Children.Clear();
            foreach (var applicationForm in clubApplicationForms)
            {
                var applicationGrid = new Grid { Margin = new Thickness(5) };
                applicationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                applicationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                applicationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                applicationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                applicationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                applicationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                applicationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var nameTextBlock = new TextBlock
                {
                    Text = applicationForm.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5)
                };

                var applicantNameTextBlock = new TextBlock
                {
                    Text = $"신청자: {applicationForm.ApplicantName}",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 12,
                    Margin = new Thickness(5)
                };

                var descriptionTextBlock = new TextBlock
                {
                    Text = $"학과: {applicationForm.Department}, 학년: {applicationForm.Year}",
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 12,
                    Margin = new Thickness(5)
                };

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var approveButton = new Button
                {
                    Content = "승인",
                    Margin = new Thickness(5),
                    Tag = applicationForm
                };
                approveButton.Click += ApproveButton_Click;

                var rejectButton = new Button
                {
                    Content = "거부",
                    Margin = new Thickness(5),
                    Tag = applicationForm
                };
                rejectButton.Click += RejectButton_Click;

                buttonPanel.Children.Add(approveButton);
                buttonPanel.Children.Add(rejectButton);

                // Grid에 요소 추가
                Grid.SetColumn(nameTextBlock, 0);
                Grid.SetRow(nameTextBlock, 0);
                Grid.SetColumnSpan(nameTextBlock, 3);

                Grid.SetColumn(applicantNameTextBlock, 0);
                Grid.SetRow(applicantNameTextBlock, 1);
                Grid.SetColumnSpan(applicantNameTextBlock, 3);

                Grid.SetColumn(descriptionTextBlock, 0);
                Grid.SetRow(descriptionTextBlock, 2);
                Grid.SetColumnSpan(descriptionTextBlock, 3);

                Grid.SetColumn(buttonPanel, 3);
                Grid.SetRow(buttonPanel, 0);
                Grid.SetRowSpan(buttonPanel, 3);

                applicationGrid.Children.Add(nameTextBlock);
                applicationGrid.Children.Add(applicantNameTextBlock);
                applicationGrid.Children.Add(descriptionTextBlock);
                applicationGrid.Children.Add(buttonPanel);

                var border = new Border()
                {
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    Child = applicationGrid,
                    Margin = new Thickness(1),
                    Cursor = Cursors.Hand // 커서 변경
                };

                border.MouseLeftButtonUp += (sender, e) => Border_Click(sender, e, applicationForm);
                border.MouseLeftButtonDown += (sender, e) => Border_MouseLeftButtonDown(sender, e, applicationForm); // 더블 클릭 이벤트 추가

                // 메인 스택패널에 동아리 신청서 패널 추가
                ClubApplicationStackPanel.Children.Add(border);
            }
        }

        private void Border_Click(object sender, MouseButtonEventArgs e, ClubApplicationForm applicationForm)
        {
            // 클릭 이벤트 핸들러 로직 (현재는 비어 있음)
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e, ClubApplicationForm applicationForm)
        {
            if (e.ClickCount == 2) // 더블 클릭인 경우
            {
                var detailsWindow = new ClubApplicationFormDetails(applicationForm);
                detailsWindow.ShowDialog();
            }
        }

        private async void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var applicationForm = button.Tag as ClubApplicationForm;

            if (applicationForm.StudentID == sid)
            {
                MessageBox.Show("자신의 동아리 신청서를 승인하거나 거절할 수 없습니다.");
                return;
            }

            try
            {
                var response = await _httpClient.PutAsync($"{Properties.Settings.Default.serverUrl}/api/approvals/{applicationForm.StudentID}/{applicationForm.ClubID}/approve", null);
                response.EnsureSuccessStatusCode();

                LoadClubApplications();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var applicationForm = button.Tag as ClubApplicationForm;

            if (applicationForm.StudentID == sid)
            {
                MessageBox.Show("자신의 동아리 신청서를 승인하거나 거절할 수 없습니다.");
                return;
            }

            try
            {
                var response = await _httpClient.PutAsync($"{Properties.Settings.Default.serverUrl}/api/approvals/{applicationForm.StudentID}/{applicationForm.ClubID}/reject", null);
                response.EnsureSuccessStatusCode();

                LoadClubApplications();
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            if (mainWindow.MainFrame.CanGoBack)
            {
                mainWindow.MainFrame.GoBack();
            }
        }

    }

}

