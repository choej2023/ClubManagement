using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClubManagement.Views
{
    public partial class EventDialog : Window
    {
        public EventDialog()
        {
            InitializeComponent();

            // StartTime과 EndTime의 기본값을 오늘 날짜로 설정
            StartDatePicker.SelectedDate = DateTime.Now;
            EndDatePicker.SelectedDate = DateTime.Now;

            // 기본적으로 9시에서 10시로 설정 (원하는 시간으로 변경 가능)
            StartHourTextBox.Text = "9";
            EndHourTextBox.Text = "10";
        }

        public string EventSummary { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public string EventDescription { get; private set; }
        public string EventLocation { get; private set; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            EventSummary = SummaryTextBox.Text;
            StartTime = StartDatePicker.SelectedDate.Value.AddHours(int.Parse(StartHourTextBox.Text));
            EndTime = EndDatePicker.SelectedDate.Value.AddHours(int.Parse(EndHourTextBox.Text));
            EventDescription = DescriptionTextBox.Text;
            EventLocation = LocationTextBox.Text;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
