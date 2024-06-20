using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClubManagement.Model
{
    public class ClubFinance
    {
        public int FinanceID { get; set; }
        public int ClubID { get; set; }
        public DateTime Date { get; set; }
        public FinanceType Type { get; set; }
        public int Amount { get; set; }
        public string Description   { get; set; }
    }
}
