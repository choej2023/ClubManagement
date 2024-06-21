using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using ClubManagement.Model;
using ClubManagement.Models;

namespace ClubManagement.Views
{
    public partial class FinanceManagementWindow : Window
    {
        private ObservableCollection<ClubFinance> financeRecords;
        private readonly HttpClient _httpClient;
        private Club club; // ClubID는 생성자나 초기화 방법에 따라 설정
        private int sid;

        public FinanceManagementWindow(Club club, int sid)
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            financeRecords = new ObservableCollection<ClubFinance>();
            FinanceListView.ItemsSource = financeRecords;
            this.club = club;
            this.sid = sid;
            LoadFinanceRecords();
        }

        private async void LoadFinanceRecords()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{Properties.Settings.Default.serverUrl}/api/clubfinance/{club.ClubID}");
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var records = JsonSerializer.Deserialize<List<ClubFinance>>(responseBody, options);
            foreach (var record in records)
            {
                financeRecords.Add(record);
            }
        }

        private async void AddFinance_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다!");
                return;
            }
            var addFinanceWindow = new AddFinanceWindow();
            if (addFinanceWindow.ShowDialog() == true)
            {
                var newFinance = addFinanceWindow.NewFinance;
                newFinance.ClubID = club.ClubID; // 클럽 ID 설정

                // 서버에 새 회계 기록 전송
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                var json = JsonSerializer.Serialize(newFinance, options);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await _httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/api/clubfinance", content);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<FinanceResponse>(responseContent);

                    if (result != null)
                    {
                        newFinance.FinanceID = result.FinanceID;
                        // 새 회계 기록을 로컬 컬렉션에 추가
                        financeRecords.Add(newFinance);
                    }
                    else
                    {
                        MessageBox.Show("회계 기록을 추가하는 데 실패했습니다.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Request error: {ex.Message}");
                }
            }
        }

        public class FinanceResponse
        {
            public bool Success { get; set; }
            public int FinanceID { get; set; }
        }

        private async void EditFinance_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다!");
                return;
            }
            var button = sender as Button;
            var financeRecord = button.DataContext as ClubFinance;

            var editFinanceWindow = new AddFinanceWindow(financeRecord);
            if (editFinanceWindow.ShowDialog() == true)
            {
                var updatedFinance = editFinanceWindow.NewFinance;

                // 서버에 수정된 회계 기록 전송
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase // 서버가 camelCase를 기대하는 경우
                };

                // 날짜를 문자열 형식으로 변환
                updatedFinance.Date = DateTime.SpecifyKind(updatedFinance.Date, DateTimeKind.Utc);

                var json = JsonSerializer.Serialize(updatedFinance, options);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PutAsync($"{Properties.Settings.Default.serverUrl}/api/clubfinance/{updatedFinance.FinanceID}", content);

                if (!response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to update finance record: {responseBody}");
                    return;
                }

                // 로컬 컬렉션에서 수정된 회계 기록 업데이트
                var index = financeRecords.IndexOf(financeRecord);
                if (index >= 0)
                {
                    financeRecords[index] = updatedFinance;
                }
            }
        }


        private async void DeleteFinance_Click(object sender, RoutedEventArgs e)
        {
            if (club.StudentID != sid)
            {
                MessageBox.Show("접근 권한이 없습니다!");
                return;
            }
            var button = sender as Button;
            var financeRecord = button.DataContext as ClubFinance;

            // 회계 삭제 로직
            HttpResponseMessage response = await _httpClient.DeleteAsync($"{Properties.Settings.Default.serverUrl}/api/clubfinance/{financeRecord.FinanceID}");
            response.EnsureSuccessStatusCode();

            financeRecords.Remove(financeRecord);
        }
    }
}
