using ClubManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubManagement.Model
{
    public class Post
    {
        public int PostID { get; set; }
        public int ClubID { get; set; }
        public int StudentID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorName { get; set; }
        public DateTime PostDate { get; set; }
        public string FilePath { get; set; }

        // Navigation properties
        public Club Club { get; set; }
    }


}
