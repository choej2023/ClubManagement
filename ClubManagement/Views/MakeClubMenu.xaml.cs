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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClubManagement.Views
{
    /// <summary>
    /// MakeClubMenu.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MakeClubMenu : Page
    {
        public MakeClubMenu()
        {
            InitializeComponent();
        }

        private void Go_Back(object sender, RoutedEventArgs e)
        {
            // 로그아웃 버튼 클릭 시 메인 윈도우로 돌아가도록 처리
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            if (mainWindow.MainFrame.CanGoBack)
            {
                mainWindow.MainFrame.GoBack();
            }
        }
    }
}
