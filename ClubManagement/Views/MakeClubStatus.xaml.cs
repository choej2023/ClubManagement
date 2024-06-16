using ClubManagement.Model;
using ClubManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ClubManagement.Views
{
    public partial class MakeClubStatus : Page
    {
        private HttpClient _httpClient;
        private int sid;
        public MakeClubStatus(int sid)
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            this.sid = sid;
            LoadStatusAsync();
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            if (mainWindow.MainFrame.CanGoBack)
            {
                mainWindow.MainFrame.GoBack();
            }
        }

        private async void LoadStatusAsync()
        {
            try
            {
                var studentJson = JsonSerializer.Serialize(new { sid });
                var studentContent = new StringContent(studentJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/api/clubstatus", studentContent);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                };

                var clubApplicationForms = JsonSerializer.Deserialize<List<ClubApplicationForm>>(responseBody, options);

                if (clubApplicationForms != null)
                {
                    foreach (var clubApplicationForm in clubApplicationForms)
                    {
                        var statusPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                        var clubNameTextBlock = new TextBlock
                        {
                            Text = clubApplicationForm.Name,
                            Width = 150,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5)
                        };

                        var reviewDateTextBlock = new TextBlock
                        {
                            Text = clubApplicationForm.checkedDate?.ToString("yyyy-MM-dd"),
                            Width = 150,
                            VerticalAlignment = VerticalAlignment.Center,
                            Margin = new Thickness(5)
                        };

                        var statusTextBlock = new TextBlock
                        {
                            Text = clubApplicationForm.isAccepted == true ? "승인됨" :
                                   (clubApplicationForm.isAccepted == false ? "거절됨" : "검토 전"),
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
                else
                {
                    MessageBox.Show("Failed to load club application forms.");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"JSON error: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}");
            }
        }
    }
}
