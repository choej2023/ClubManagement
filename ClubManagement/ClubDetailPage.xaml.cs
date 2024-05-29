using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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

namespace ClubManagement
{
    /// <summary>
    /// ClubDetailPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClubDetailPage : Window, INotifyPropertyChanged
    {
        private DateTime _displayedMonth;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DateModel> Dates { get; set; }

        public DateTime DisplayedMonth
        {
            get => _displayedMonth;
            set
            {
                _displayedMonth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DisplayedMonth"));
            }
        }


        public ClubDetailPage()
        {
            InitializeComponent();
            DataContext = this;
            DisplayedMonth = DateTime.Today;
            Dates = new ObservableCollection<DateModel>();
            PopulateDates();

        }
        private void PopulateDates()
        {
            Dates.Clear();
            int daysInMonth = DateTime.DaysInMonth(DisplayedMonth.Year, DisplayedMonth.Month);
            for (int day = 1; day <= daysInMonth; day++)
            {
                Dates.Add(new DateModel { Day = day });
            }
        }

        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            DisplayedMonth = DisplayedMonth.AddMonths(-1);
            PopulateDates();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            DisplayedMonth = DisplayedMonth.AddMonths(1);
            PopulateDates();
        }
    }

    public class DateModel
    {
        public int Day { get; set; }
        // Add other properties like year, month, or any other data you want to display
    }


}
   


