using ClubManagement.Model;
using System.Security.Cryptography.Pkcs;

namespace ClubManagement.Models
{
    public class ClubApplicationForm
    {
        public int StudentID { get; set; }
        public int ClubID { get; set; }
        public string ClubName { get; set; }
        public string StudentNumber { get; set; }
        public string Department { get; set; }
        public int Year { get; set; }
        public string Name { get; set; }
        public int TotalApprovals { get; set; }
        public int RequiredApprovals { get; set; }
        public DateTime? checkedDate { get; set; }

        // Navigation properties
        public Student Student { get; set; }
    }
}
