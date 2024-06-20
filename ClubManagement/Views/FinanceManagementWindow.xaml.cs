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
        private int ClubID; // ClubID는 생성자나 초기화 방법에 따라 설정

        public FinanceManagementWindow(Club club)
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            financeRecords = new ObservableCollection<ClubFinance>();
            FinanceListView.ItemsSource = financeRecords;
            this.ClubID = club.ClubID;
            LoadFinanceRecords();
        }

        private async void LoadFinanceRecords()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{Properties.Settings.Default.serverUrl}/api/clubfinance/{ClubID}");
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
            var addFinanceWindow = new AddFinanceWindow();
            if (addFinanceWindow.ShowDialog() == true)
            {
                var newFinance = addFinanceWindow.NewFinance;
                newFinance.ClubID = ClubID; // 클럽 ID 설정

                // 서버에 새 회계 기록 전송
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };
                var json = JsonSerializer.Serialize(newFinance, options);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync($"{Properties.Settings.Default.serverUrl}/api/clubfinance", content);
                response.EnsureSuccessStatusCode();

                // 새 회계 기록을 로컬 컬렉션에 추가
                financeRecords.Add(newFinance);
            }
        }

        private async void EditFinance_Click(object sender, RoutedEventArgs e)
        {
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
            var button = sender as Button;
            var financeRecord = button.DataContext as ClubFinance;

            // 회계 삭제 로직
            HttpResponseMessage response = await _httpClient.DeleteAsync($"{Properties.Settings.Default.serverUrl}/api/clubfinance/{financeRecord.FinanceID}");
            response.EnsureSuccessStatusCode();

            financeRecords.Remove(financeRecord);
        }
    }
}
