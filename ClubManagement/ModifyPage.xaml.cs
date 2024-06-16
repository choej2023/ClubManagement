using ClubManagement.Models;
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

namespace ClubManagement
{
    /// <summary>
    /// ModifyPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModifyPage : Window
    {
        public Club ModifiedClub { get; private set; }

        private Club originalClub;

        public ModifyPage(Club club)
        {
            InitializeComponent();
            originalClub = club;
            // 초기화면에 원래 Club 객체의 정보를 표시
            ClubNameTextBox.Text = club.ClubName;
            ClubDescriptionTextBox.Text = club.ShortDescription;
            ClubDetailsTextBox.Text = club.Description;
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 수정된 Club 객체 생성
            ModifiedClub = new Club
            {
                ClubName = ClubNameTextBox.Text,
                ShortDescription = ClubDescriptionTextBox.Text,
                Description = ClubDetailsTextBox.Text
            };

            // 수정이 완료되었음을 나타내는 이벤트 발생
            OnClubModified();

            // 수정 페이지 닫기
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // 취소 버튼 클릭 시, 수정되지 않은 상태로 수정 페이지 닫기
            ModifiedClub = null;
            Close();
        }

        // 수정 완료 이벤트 정의
        public event EventHandler ClubModified;

        protected virtual void OnClubModified()
        {
            ClubModified?.Invoke(this, EventArgs.Empty);
        }

    }
}
