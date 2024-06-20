using System.Windows;
using ClubManagement.Model;

namespace ClubManagement.Views
{
    public partial class AddFinanceWindow : Window
    {
        public ClubFinance NewFinance { get; private set; }

        // Default constructor for adding new finance record
        public AddFinanceWindow()
        {
            InitializeComponent();
            NewFinance = new ClubFinance
            {
                Date = DateTime.Today // Default date to today
            };
            DataContext = NewFinance;
        }

        // Overloaded constructor for editing existing finance record
        public AddFinanceWindow(ClubFinance finance) : this()
        {
            NewFinance = finance;
            DataContext = NewFinance;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
