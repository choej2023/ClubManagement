using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ClubManagement.Views
{
    public partial class LoginPage : Page
    {
        private readonly HttpClient _httpClient;
        public LoginPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginRequest = new LoginRequest
            {
                Username = txtID.Text,
                Password = txtPassword.Password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/api/login", content);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseBody);

                    if (loginResponse.Success)
                    {
                        MessageBox.Show("로그인 성공!");
                        MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
                        mainWin.MainFrame.Navigate(new ClubDetailPage(loginResponse.StudentId));
                    }
                    else
                    {
                        MessageBox.Show("Invalid ID or Password");
                    }
                }
                else
                {
                    MessageBox.Show("Invalid ID or Password");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public int StudentId { get; set; }
        public string Message { get; set; }
    }
}