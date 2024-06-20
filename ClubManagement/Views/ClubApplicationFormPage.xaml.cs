using ClubManagement.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using ClubManagement.Model;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using System.IO;
using Google.Apis.Calendar.v3;

namespace ClubManagement.Views
{
    public partial class ClubApplicationFormPage : Page
    {
        static string[] Scopes = { CalendarService.Scope.Calendar };
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
                    club.ClubID = applicationForm.ClubID;
                    var applicationFormJson = JsonSerializer.Serialize(applicationForm);
                    var applicationFormContent = new StringContent(applicationFormJson, Encoding.UTF8, "application/json");
                    HttpResponseMessage applicationFormResponse = await _httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/api/clubapplicationforms", applicationFormContent);
                    if (applicationFormResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show("동아리 신설 신청서가 제출되었습니다.");
                        MessageBox.Show("구글 캘린더와의 동기화를 위해 페이지를 이동합니다.");
                        await SaveChairmanCredentialsAsync(club);

                        MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
                        mainWin.MainFrame.Navigate(new ClubMainPage(club.StudentID));
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
        public async Task SaveChairmanCredentialsAsync(Club club)
        {
            try
            {
                UserCredential credential;

                using (var stream = new FileStream("client_secret_921999378493-3mjcj8s7l020j6pfdlmlja5qi3h375ji.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
                {
                    string credPath = $"token_{club.ClubID}";
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        club.StudentID.ToString(),
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
