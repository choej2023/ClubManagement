using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubManagement.Model
{
    public class ClubApplicationForm
    {
        public int StudentID { get; set; }
        public int ClubID { get; set; }
        public string StudentNumber { get; set; }
        public string Department { get; set; }
        public int Year { get; set; }
        public string Name { get; set; }
        public int MemberCount { get; set; }
        public bool? IsAccepted { get; set; }

        // Navigation properties
        public Student Student { get; set; }
    }


}
