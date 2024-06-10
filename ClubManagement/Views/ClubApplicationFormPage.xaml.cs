using ClubManagement.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using ClubManagement.Model;

namespace ClubManagement.Views
{
    public partial class ClubApplicationFormPage : Page
    {
        private Club club;
        private readonly HttpClient _httpClient;

        public ClubApplicationFormPage(Club club)
        {
            InitializeComponent();
            this.club = club;
            _httpClient = new HttpClient();
        }

        private async void SubmitApplication_Click(object sender, RoutedEventArgs e)
        {
            var applicationForm = new ClubApplicationForm
            {
                StudentID = club.StudentID,
                Department = DepartmentTextBox.Text,
                Year = int.Parse(YearTextBox.Text),
                StudentNumber = StudentNumberTextBox.Text,
                Name = NameTextBox.Text,
                MemberCount = int.Parse(MemberCountTextBox.Text),
            };

            // 동아리 신설 신청서 제출 로직 (백엔드와 통신하여 데이터 저장)

            try
            {
                // 클럽 정보를 서버에 업로드
                var clubJson = JsonSerializer.Serialize(club);
                var clubContent = new StringContent(clubJson, Encoding.UTF8, "application/json");
                HttpResponseMessage clubResponse = await _httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/api/clubs", clubContent);
                if (clubResponse.IsSuccessStatusCode)
                {
                    string clubResponseBody = await clubResponse.Content.ReadAsStringAsync();
                    var clubPostResponse = JsonSerializer.Deserialize<Club>(clubResponseBody);

                    // 클럽 신청서를 서버에 업로드
                    applicationForm.ClubID = clubPostResponse.ClubID; // 서버에서 반환된 클럽 ID 사용
                    var applicationFormJson = JsonSerializer.Serialize(applicationForm);
                    var applicationFormContent = new StringContent(applicationFormJson, Encoding.UTF8, "application/json");
                    HttpResponseMessage applicationFormResponse = await _httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/api/clubapplicationforms", applicationFormContent);
                    if (applicationFormResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show("동아리 신설 신청서가 제출되었습니다.");
                        MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
                        mainWin.MainFrame.Navigate(new ClubDetailPage(club.StudentID));
                    }
                    else
                    {
                        MessageBox.Show("동아리 신설 신청서 제출에 실패했습니다.");
                    }
                }
                else
                {
                    MessageBox.Show("클럽 제출에 실패했습니다.");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}");
            }
        }
    }
}
