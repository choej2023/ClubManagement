using ClubManagement.Model;

namespace ClubManagement.Models
{
    public class Club
    {
        public int ClubID { get; set; }
        public int StudentID { get; set; }
        public string ClubName { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public DateTime? PromotionStartDate { get; set; }
        public DateTime? PromotionEndDate { get; set; }
        public int maxCount { get; set; }
        public int count { get; set; }

        public string? ImagePath { get; set; }

        // Navigation properties
        public Student President { get; set; }
        public ICollection<ClubMember> ClubMembers { get; set; }
        public ICollection<Post> Posts { get; set; }
    }


}
