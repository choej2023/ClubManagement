using ClubManagement.Models;
using ClubManagement.Views;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MySqlConnector;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClubManagement
{
    public partial class ClubDetailPage : Window
    {
        static string[] Scopes = { CalendarService.Scope.Calendar };
        static string ApplicationName = "Google Calendar Integration";
        CalendarService service;
        private Club club;
        private readonly HttpClient _httpClient;
        private int sid;

        public ClubDetailPage(Club club, int sId)
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            this.club = club;
            this.sid = sId;
            LoadDataFromDatabaseAsync(); // 데이터베이스에서 데이터를 가져와서 Club 객체 생성 및 설정
            InitializeGoogleCalendarService();
        }

        // 데이터베이스로부터 데이터를 읽어와 Club 객체를 생성하고 설정하는 메서드
        private async Task LoadDataFromDatabaseAsync()
        {
            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM club WHERE ClubID = @ClubID"; // 적절한 쿼리로 변경 필요
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClubID", club.ClubID);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read()) // 첫 번째 행만 가져옴
                        {
                            // 데이터베이스에서 가져온 값으로 Club 객체 생성
                            club = new Club
                            {
                                ClubID = reader.GetInt32("ClubID"),
                                StudentID = reader.GetInt32("StudentID"),
                                ClubName = reader["ClubName"].ToString(),
                                ShortDescription = reader["ShortDescription"].ToString(),
                                Description = reader["Description"].ToString(),
                                ImagePath = reader["ImagePath"].ToString()
                            };
                            club.ImageSource = await LoadImageFromServerAsync(club.ImagePath);
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

        private async void InitializeGoogleCalendarService()
        {
            try
            {
                UserCredential credential;
                string credPath = $"token_{club.ClubID}.json";

                // 자격 증명 파일이 존재하는지 확인
                if (!File.Exists(credPath))
                {
                    MessageBox.Show("아직 달력이 구글 캘린더와 연동되지 않았습니다.");
                    if (sid != club.StudentID)
                        return;
                }

                using (var stream = new FileStream("client_secret_921999378493-3mjcj8s7l020j6pfdlmlja5qi3h375ji.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        club.StudentID.ToString(),
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                }

                service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async void EventCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (service == null)
                return;

            var selectedDate = EventCalendar.SelectedDate;
            if (selectedDate == null)
                return;

            DateTime startDate = selectedDate.Value.Date;
            DateTime endDate = startDate.AddDays(1);

            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMinDateTimeOffset = new DateTimeOffset(startDate);
            request.TimeMaxDateTimeOffset = new DateTimeOffset(endDate);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            Events events = await request.ExecuteAsync();
            EventListBox.Items.Clear();

            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    EventListBox.Items.Add(new EventWrapper(eventItem));
                }
            }
            else
            {
                EventListBox.Items.Add("No events found.");
            }
        }

        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
            }
            else 
                AddOrEditEvent(null);
        }

        private void EditEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
            }
            else
            {
                if (EventListBox.SelectedItem is EventWrapper selectedEventWrapper)
                {
                    AddOrEditEvent(selectedEventWrapper.Event);
                }
                else
                {
                    MessageBox.Show("Please select an event to edit.");
                }
            }
        }


        private async void DeleteEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
            }
            else
            {
                if (EventListBox.SelectedItem is EventWrapper selectedEventWrapper)
                {
                    await service.Events.Delete("primary", selectedEventWrapper.Event.Id).ExecuteAsync();
                    MessageBox.Show("Event deleted.");
                    EventCalendar_SelectedDatesChanged(null, null); // Refresh the event list
                }
                else
                {
                    MessageBox.Show("Please select an event to delete.");
                }
            }
        }
        private async void AddOrEditEvent(Event existingEvent)
        {
            EventDialog dialog = new EventDialog();

            if (existingEvent != null)
            {
                dialog.SummaryTextBox.Text = existingEvent.Summary;
                dialog.StartDatePicker.SelectedDate = existingEvent.Start.DateTimeDateTimeOffset?.DateTime ?? DateTime.Parse(existingEvent.Start.Date);
                dialog.StartHourTextBox.Text = (existingEvent.Start.DateTimeDateTimeOffset?.DateTime ?? DateTime.Parse(existingEvent.Start.Date)).Hour.ToString();
                dialog.EndDatePicker.SelectedDate = existingEvent.End.DateTimeDateTimeOffset?.DateTime ?? DateTime.Parse(existingEvent.End.Date);
                dialog.EndHourTextBox.Text = (existingEvent.End.DateTimeDateTimeOffset?.DateTime ?? DateTime.Parse(existingEvent.End.Date)).Hour.ToString();
                dialog.DescriptionTextBox.Text = existingEvent.Description;
                dialog.LocationTextBox.Text = existingEvent.Location;
            }

            if (dialog.ShowDialog() == true)
            {
                Event newEvent = existingEvent ?? new Event();
                newEvent.Summary = dialog.EventSummary;
                newEvent.Start = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(dialog.StartTime) };
                newEvent.End = new EventDateTime() { DateTimeDateTimeOffset = new DateTimeOffset(dialog.EndTime) };
                newEvent.Description = dialog.EventDescription;
                newEvent.Location = dialog.EventLocation;

                if (existingEvent == null)
                {
                    await service.Events.Insert(newEvent, "primary").ExecuteAsync();
                    MessageBox.Show("Event added.");
                }
                else
                {
                    await service.Events.Update(newEvent, "primary", existingEvent.Id).ExecuteAsync();
                    MessageBox.Show("Event updated.");
                }

                EventCalendar_SelectedDatesChanged(null, null); // Refresh the event list
            }
        }

        private void EventListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventListBox.SelectedItem is EventWrapper selectedEventWrapper)
            {
                var eventItem = selectedEventWrapper.Event;
                EventDetailsTextBlock.Text = $"제목: {eventItem.Summary}\n" +
                                             $"시작일: {(eventItem.Start.DateTimeDateTimeOffset.HasValue ? eventItem.Start.DateTimeDateTimeOffset.Value.ToString("g") : eventItem.Start.Date)}\n" +
                                             $"종료일: {(eventItem.End.DateTimeDateTimeOffset.HasValue ? eventItem.End.DateTimeDateTimeOffset.Value.ToString("g") : eventItem.End.Date)}\n" +
                                             $"내용: {eventItem.Description ?? "N/A"}\n" +
                                             $"위치: {eventItem.Location ?? "N/A"}";
            }
            else
            {
                EventDetailsTextBlock.Text = string.Empty;
            }
        }

        private class EventWrapper
        {
            public EventWrapper(Event eventItem)
            {
                Event = eventItem;
            }

            public Event Event { get; }

            public override string ToString()
            {
                return $"{Event.Summary} ({Event.Start.DateTimeDateTimeOffset?.ToString("g") ?? Event.Start.Date})";
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
            }
            else
            {
                // 수정 페이지로 이동
                ModifyPage modifyPage = new ModifyPage((Club)DataContext);
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

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
            }
            else
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

        }

        // 데이터베이스에서 클럽 삭제 메서드
        private void DeleteClubFromDatabase()
        {
            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM club WHERE ClubID = @ClubID";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClubID", club.ClubID); // 예시로 ID 1을 사용하였음, 실제로는 Club의 고유한 ID를 사용해야 함
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
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE club SET ClubName = @ClubName, ShortDescription = @ShortDescription, Description = @Description WHERE ClubID = @ClubID";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClubName", modifiedClub.ClubName);
                    command.Parameters.AddWithValue("@ShortDescription", modifiedClub.ShortDescription);
                    command.Parameters.AddWithValue("@Description", modifiedClub.Description);
                    command.Parameters.AddWithValue("@ClubID", club.ClubID); // 예시로 ID 1을 사용하였음, 실제로는 Club의 고유한 ID를 사용해야 함
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

                if (textBlock != null)
                {
                    string userInput = Microsoft.VisualBasic.Interaction.InputBox("Enter value:", "Input", "");
                    if (!string.IsNullOrEmpty(userInput))
                    {
                        // 사용자 입력을 Rectangle의 채우기 색으로 설정
                        rectangle.Fill = new SolidColorBrush(System.Windows.Media.Colors.Red); // 예시로 빨간색 설정
                        textBlock.Text = userInput;
                    }
                }
                // 입력 창을 표시하고 사용자 입력 처리
            }
        }

        private void ShowPosts_Click(object sender, RoutedEventArgs e)
        {
            ClubBoardPage clubBoardPage = new ClubBoardPage(club, sid);
            clubBoardPage.Show();
        }

        private void ShowMember_Click(object sender, RoutedEventArgs e)
        {
            MemberManagement memberManagement = new MemberManagement(club, sid);
            memberManagement.Show();
        }

        private void Back_Button(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
