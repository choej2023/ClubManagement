using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubManagement.Model
{
    public class LocalEvent
    {
        public int Id { get; set; }
        public int ClubID { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Location { get; set; }
        public string GoogleEventId { get; set; } // 구글 이벤트의 고유 ID
    }

}
