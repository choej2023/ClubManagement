using ClubManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubManagement.Model
{
    public class Student
    {
        public int StudentID { get; set; }
        public string StudentNumber { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Year { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }

        // Navigation properties
        public ICollection<ClubMember> ClubMembers { get; set; }
        public ICollection<ClubApplicationForm> ClubApplicationForms { get; set; }
    }


}
