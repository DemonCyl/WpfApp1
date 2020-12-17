using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Entity
{
    public class ProductConfig
    {
        public int FInterID { get; set; }
        public string FZCType { get; set; }

        public int FXingHao { get; set; }

        public string FDianJiCodeRule { get; set; }

        public string FQianGuanCodeRule { get; set; }
        public int FStatus1 { get; set; }
        public string FLXingCodeRule { get; set; }
        public int FStatus2 { get; set; }
        public string FCeBanCodeRule { get; set; }
        public int FStatus3 { get; set; }

        public int FCodeSum { get; set; }
        public string FGWItem { get; set; }
        public DateTime FDate { get; set; }
    }
}
