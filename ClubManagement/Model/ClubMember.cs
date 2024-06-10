using ClubManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubManagement.Model
{
    public class ClubMember
    {
        public int ClubID { get; set; }
        public int StudentID { get; set; }

        // Navigation properties
        public Club Club { get; set; }
        public Student Student { get; set; }
    }


}
