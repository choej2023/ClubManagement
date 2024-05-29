using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class Post
    {
        public int Index { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime DatePosted { get; set; }
        public string Content { get; set; }
    }
    public class ViewModel
    {
        public ObservableCollection<Post> Posts { get; set; }

        public ViewModel()
        {
            // 예시 데이터 생성
            Posts = new ObservableCollection<Post>
        {
            new Post { Index = 1, Title = "첫 번째 게시물", Author = "사용자1", Content = "안녕하세요, 첫 번째 게시물입니다.", DatePosted = DateTime.Now.AddDays(-3) },
            new Post { Index = 2, Title = "두 번째 게시물", Author = "사용자2", Content = "안녕하세요, 두 번째 게시물입니다.", DatePosted = DateTime.Now.AddDays(-2) },
            new Post { Index = 3, Title = "세 번째 게시물", Author = "사용자3", Content = "안녕하세요, 세 번째 게시물입니다.", DatePosted = DateTime.Now.AddDays(-1) }
        };
        }
    }
    /// <summary>
    /// ClubBoardPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClubBoardPage : Window
    {
        public ClubBoardPage()
        {
            InitializeComponent();
            DataContext = new ViewModel();
        }
    }
}
