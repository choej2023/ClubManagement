using System.Windows;

namespace ClubManagement.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());
        }
    }
}
