using ClubManagement.Model;
using ClubManagement.Models;
using System.Windows;
using System.Windows.Controls;

namespace ClubManagement.Views
{
    public partial class ClubApplicationFormPage : Page
    {
        private Club club;
        public ClubApplicationFormPage(Club club)
        {
            InitializeComponent();
            this.club = club;
        }

        private void SubmitApplication_Click(object sender, RoutedEventArgs e)
        {
            var applicationForm = new ClubApplicationForm
            {
                Department = DepartmentTextBox.Text,
                Year = Int32.Parse(YearTextBox.Text),
                StudentNumber = StudentNumberTextBox.Text,
                Name = NameTextBox.Text,
                MemberCount = Int32.Parse(MemberCountTextBox.Text),
            };

            // 동아리 신설 신청서 제출 로직 (백엔드와 통신하여 데이터 저장)
            // 이 부분에 필요한 로직을 추가하세요.

            MessageBox.Show("동아리 신설 신청서가 제출되었습니다.");

            // 메인 페이지로 이동
            NavigationService.Navigate(new ClubDetailPage(club.StudentID));
        }
    }
}
