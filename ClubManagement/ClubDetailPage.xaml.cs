using ClubManagement.Model;
using ClubManagement.Models;
using ClubManagement.Views;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
                string credPath = $"token_{club.ClubID}";
                string tokenFilePath = Path.Combine(credPath, $"Google.Apis.Auth.OAuth2.Responses.TokenResponse-{club.StudentID}");

                // 인증 토큰 파일이 존재하는지 확인
                if (!File.Exists(tokenFilePath))
                {
                    MessageBox.Show("아직 구글 캘린더와 연동되지 않았습니다.");
                    if (club.StudentID != sid)
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

                // 연동 후 로컬 데이터베이스의 이벤트들을 구글 캘린더로 업로드
                await UploadLocalEventsToGoogleCalendar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async Task UploadLocalEventsToGoogleCalendar()
        {
            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM events WHERE ClubID = @ClubID";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClubID", club.ClubID);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Event newEvent = new Event()
                            {
                                Summary = reader["Summary"].ToString(),
                                Description = reader["Description"].ToString(),
                                Start = new EventDateTime() { DateTime = reader.GetDateTime("Start") },
                                End = new EventDateTime() { DateTime = reader.GetDateTime("End") },
                                Location = reader["Location"].ToString(),
                            };

                            await service.Events.Insert(newEvent, "primary").ExecuteAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading events to Google Calendar: " + ex.Message);
            }
        }

        private async void EventCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (service == null)
            {
                LoadLocalEvents();
                return;
            }

            var selectedDate = EventCalendar.SelectedDate;
            if (selectedDate == null)
                return;

            DateTime startDate = selectedDate.Value.Date;
            DateTime endDate = startDate.AddDays(1);

            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = startDate;
            request.TimeMax = endDate;
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

        private void LoadLocalEvents()
        {
            var selectedDate = EventCalendar.SelectedDate;
            if (selectedDate == null)
                return;

            DateTime startDate = selectedDate.Value.Date;
            DateTime endDate = startDate.AddDays(1);

            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM events WHERE ClubID = @ClubID AND (Start >= @StartDate AND Start < @EndDate) OR (End > @StartDate AND End <= @EndDate) OR (Start <= @StartDate AND End >= @EndDate)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClubID", club.ClubID);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        EventListBox.Items.Clear();
                        while (reader.Read())
                        {
                            var localEvent = new LocalEvent
                            {
                                Id = reader.GetInt32("Id"),
                                ClubID = reader.GetInt32("ClubID"),
                                Summary = reader["Summary"].ToString(),
                                Description = reader["Description"].ToString(),
                                Start = reader.GetDateTime("Start"),
                                End = reader.GetDateTime("End"),
                                Location = reader["Location"].ToString(),
                                GoogleEventId = reader["GoogleEventId"].ToString()
                            };
                            EventListBox.Items.Add(new EventWrapper(localEvent));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading local events: " + ex.Message);
            }
        }

        private void ManageFinance_Click(object sender, RoutedEventArgs e)
        {
            var financeWindow = new FinanceManagementWindow(club);
            financeWindow.Show();
        }


        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
                return;
            }
            AddOrEditEvent(null);
        }

        private async void EditEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
                return;
            }

            if (EventListBox.SelectedItem is EventWrapper selectedEventWrapper)
            {
                if (selectedEventWrapper.IsLocal)
                {
                    await AddOrEditEvent(selectedEventWrapper.LocalEvent);
                }
                else
                {
                    MessageBox.Show("Local event만 수정할 수 있습니다.");
                }
            }
            else
            {
                MessageBox.Show("Please select an event to edit.");
            }
        }


        private async void DeleteEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
                return;
            }

            bool isDeleted = false;

            if (EventListBox.SelectedItem is EventWrapper selectedEventWrapper)
            {
                if (selectedEventWrapper.IsLocal)
                {
                    if (service != null && !string.IsNullOrEmpty(selectedEventWrapper.LocalEvent.GoogleEventId))
                    {
                        await service.Events.Delete("primary", selectedEventWrapper.LocalEvent.GoogleEventId).ExecuteAsync();
                        isDeleted = true;
                    }
                    DeleteLocalEvent(selectedEventWrapper.LocalEvent);
                    isDeleted = true;
                }
                else
                {
                    if (service != null && selectedEventWrapper.GoogleEvent != null)
                    {
                        await service.Events.Delete("primary", selectedEventWrapper.GoogleEvent.Id).ExecuteAsync();
                        isDeleted = true;
                    }
                }

                if (isDeleted)
                {
                    MessageBox.Show("Event deleted.");
                    EventCalendar_SelectedDatesChanged(null, null); // Refresh the event list
                }
            }
            else
            {
                MessageBox.Show("Please select an event to delete.");
            }
        }




        private void DeleteLocalEvent(LocalEvent localEvent)
        {
            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM events WHERE Id = @Id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", localEvent.Id);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Event deleted successfully.");
                    }
                    else
                    {
                        MessageBox.Show("Event deletion failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting local event: " + ex.Message);
            }
        }


        private async 

        Task
AddOrEditEvent(LocalEvent existingEvent)
        {
            EventDialog dialog = new EventDialog();

            if (existingEvent != null)
            {
                dialog.SummaryTextBox.Text = existingEvent.Summary;
                dialog.StartDatePicker.SelectedDate = existingEvent.Start;
                dialog.StartHourTextBox.Text = existingEvent.Start.Hour.ToString();
                dialog.EndDatePicker.SelectedDate = existingEvent.End;
                dialog.EndHourTextBox.Text = existingEvent.End.Hour.ToString();
                dialog.DescriptionTextBox.Text = existingEvent.Description;
                dialog.LocationTextBox.Text = existingEvent.Location;
            }

            if (dialog.ShowDialog() == true)
            {
                LocalEvent newEvent = existingEvent ?? new LocalEvent();
                newEvent.ClubID = club.ClubID;
                newEvent.Summary = dialog.SummaryTextBox.Text;
                DateTime startDate = dialog.StartDatePicker.SelectedDate ?? DateTime.Now;
                DateTime endDate = dialog.EndDatePicker.SelectedDate ?? DateTime.Now;
                newEvent.Start = new DateTime(startDate.Year, startDate.Month, startDate.Day, int.Parse(dialog.StartHourTextBox.Text), 0, 0);
                newEvent.End = new DateTime(endDate.Year, endDate.Month, endDate.Day, int.Parse(dialog.EndHourTextBox.Text), 0, 0);
                newEvent.Description = dialog.DescriptionTextBox.Text;
                newEvent.Location = dialog.LocationTextBox.Text;

                bool isEventAddedOrUpdated = false;

                if (existingEvent == null)
                {
                    SaveLocalEvent(newEvent);
                    isEventAddedOrUpdated = true;
                }
                else
                {
                    UpdateLocalEvent(newEvent);
                    isEventAddedOrUpdated = true;
                }

                if (service != null)
                {
                    Event googleEvent = new Event()
                    {
                        Summary = newEvent.Summary,
                        Description = newEvent.Description,
                        Start = new EventDateTime() { DateTime = newEvent.Start },
                        End = new EventDateTime() { DateTime = newEvent.End },
                        Location = newEvent.Location,
                    };

                    if (existingEvent == null || existingEvent.GoogleEventId == null)
                    {
                        var createdEvent = await service.Events.Insert(googleEvent, "primary").ExecuteAsync();
                        newEvent.GoogleEventId = createdEvent.Id;
                    }
                    else
                    {
                        googleEvent.Id = existingEvent.GoogleEventId;
                        await service.Events.Update(googleEvent, "primary", googleEvent.Id).ExecuteAsync();
                    }

                    UpdateLocalEvent(newEvent); // Update the local event with GoogleEventId
                    isEventAddedOrUpdated = true;
                }

                if (isEventAddedOrUpdated)
                {
                    MessageBox.Show(existingEvent == null ? "Event added." : "Event updated.");
                    EventCalendar_SelectedDatesChanged(null, null); // Refresh the event list
                }
            }
        }




        private void SaveLocalEvent(LocalEvent localEvent)
        {
            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO events (ClubID, Summary, Description, Start, End, Location, GoogleEventId) VALUES (@ClubID, @Summary, @Description, @Start, @End, @Location, @GoogleEventId)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@ClubID", localEvent.ClubID);
                    command.Parameters.AddWithValue("@Summary", localEvent.Summary);
                    command.Parameters.AddWithValue("@Description", localEvent.Description);
                    command.Parameters.AddWithValue("@Start", localEvent.Start);
                    command.Parameters.AddWithValue("@End", localEvent.End);
                    command.Parameters.AddWithValue("@Location", localEvent.Location);
                    command.Parameters.AddWithValue("@GoogleEventId", localEvent.GoogleEventId);
                    command.ExecuteNonQuery();

                    // 삽입된 레코드의 자동 증가 Id 값을 가져와서 설정
                    localEvent.Id = (int)command.LastInsertedId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving local event: " + ex.Message);
            }
        }


        private void UpdateLocalEvent(LocalEvent localEvent)
        {
            try
            {
                string connectionString = "Server=localhost;Database=clubmanagement;Uid=root;Pwd=root;";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE events SET Summary = @Summary, Description = @Description, Start = @Start, End = @End, Location = @Location, GoogleEventId = @GoogleEventId WHERE Id = @Id";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Summary", localEvent.Summary);
                    command.Parameters.AddWithValue("@Description", localEvent.Description);
                    command.Parameters.AddWithValue("@Start", localEvent.Start);
                    command.Parameters.AddWithValue("@End", localEvent.End);
                    command.Parameters.AddWithValue("@Location", localEvent.Location);
                    command.Parameters.AddWithValue("@GoogleEventId", localEvent.GoogleEventId);
                    command.Parameters.AddWithValue("@Id", localEvent.Id);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Event updated successfully.");
                    }
                    else
                    {
                        MessageBox.Show("Event update failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating local event: " + ex.Message);
            }
        }



        private void EventListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventListBox.SelectedItem is EventWrapper selectedEventWrapper)
            {
                if (selectedEventWrapper.IsLocal)
                {
                    var eventItem = selectedEventWrapper.LocalEvent;
                    EventDetailsTextBlock.Text = $"제목: {eventItem.Summary}\n" +
                                                 $"시작일: {eventItem.Start.ToString("g")}\n" +
                                                 $"종료일: {eventItem.End.ToString("g")}\n" +
                                                 $"내용: {eventItem.Description ?? "N/A"}\n" +
                                                 $"위치: {eventItem.Location ?? "N/A"}";
                }
                else
                {
                    var eventItem = selectedEventWrapper.GoogleEvent;
                    EventDetailsTextBlock.Text = $"제목: {eventItem.Summary}\n" +
                                                 $"시작일: {(eventItem.Start.DateTime.HasValue ? eventItem.Start.DateTime.Value.ToString("g") : eventItem.Start.Date)}\n" +
                                                 $"종료일: {(eventItem.End.DateTime.HasValue ? eventItem.End.DateTime.Value.ToString("g") : eventItem.End.Date)}\n" +
                                                 $"내용: {eventItem.Description ?? "N/A"}\n" +
                                                 $"위치: {eventItem.Location ?? "N/A"}";
                }
            }
            else
            {
                EventDetailsTextBlock.Text = string.Empty;
            }
        }


        private class LocalEventWrapper
        {
            public LocalEventWrapper(LocalEvent eventItem)
            {
                Event = eventItem;
            }

            public LocalEvent Event { get; }

            public override string ToString()
            {
                return $"{Event.Summary} ({Event.Start.ToString("g")})";
            }
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
                return;
            }
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다.");
                return;
            }
            // 삭제 확인 메시지 출력
            MessageBoxResult result = MessageBox.Show("클럽을 삭제하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

            // 사용자가 확인을 선택한 경우에만 삭제 수행
            if (result == MessageBoxResult.Yes)
            {
                // 데이터베이스에서 클럽 삭제
                DeleteClubFromDatabase();
            }
        }

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
                    command.Parameters.AddWithValue("@ClubID", club.ClubID);
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
                    command.Parameters.AddWithValue("@ClubID", club.ClubID);
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