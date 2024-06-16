using Accessibility;
using ClubManagement.Model;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media;

namespace ClubManagement.Models
{
    public class Club : INotifyPropertyChanged
    {
        private ImageSource _imageSource;
        public int ClubID { get; set; }
        public int StudentID {  get; set; }
        public string ClubName { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public DateTime? PromotionStartDate { get; set; }
        public DateTime? PromotionEndDate { get; set; }
        public int maxCount { get; set; }
        public int count { get; set; }

        public string? ImagePath { get; set; }
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        // Navigation properties
        public Student President { get; set; }
        public ICollection<ClubMember> ClubMembers { get; set; }
        public ICollection<Post> Posts { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
