using System.Windows;
using System.Windows.Controls;

namespace ClubManagement.Views
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // 로그인 로직을 여기에 추가하세요 (예: ID와 Password 확인).
            // 로그인 성공 시 다음 페이지로 이동합니다.
            if (txtID.Text == "admin" && txtPassword.Password == "password") // 예제용
            {
                MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
                mainWin.MainFrame.Navigate(new ClubDetailPage());
            }
            else
            {
                MessageBox.Show("Invalid ID or Password");
            }
        }
    }
}
