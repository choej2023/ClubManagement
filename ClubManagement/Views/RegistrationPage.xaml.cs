using ClubManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    /// <summary>
    /// RegistrationPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RegistrationPage : Page
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public RegistrationPage()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {

            // 모든 필드가 입력되었는지 검사
            if (string.IsNullOrWhiteSpace(StudentNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(UserNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(YearTextBox.Text) ||
                string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                RoleComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(DepartmentTextBox.Text))
            {
                MessageBox.Show("모든 필드를 입력하세요.");
                return;
            }

            // 학년 필드가 유효한 숫자인지 검사
            if (!int.TryParse(YearTextBox.Text, out int year))
            {
                MessageBox.Show("학년은 숫자여야 합니다.");
                return;
            }

            var formData = new Student
            {
                StudentNumber = StudentNumberTextBox.Text,
                UserName = UserNameTextBox.Text,
                Password = PasswordBox.Password,
                Year = int.Parse(YearTextBox.Text),
                Name = NameTextBox.Text,
                Role = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString(),
                Department = DepartmentTextBox.Text
            };

            try
            {
                var jsonContent = JsonSerializer.Serialize(formData);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/api/register", httpContent);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                MessageBox.Show("회원가입이 성공적으로 완료되었습니다.");

                MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
                mainWin.MainFrame.Navigate(new LoginPage());
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
