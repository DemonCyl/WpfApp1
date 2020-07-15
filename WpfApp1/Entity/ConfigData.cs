using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WpfApp1.Entity
{
    class ConfigData
    {


        public string IpAdress { get; set; }

        public int GWNo { get; set; }
        public int StationNo { get; set; }


        public List<StationData> Station { get; set; }
        public int GunNo { get; set; }

        public int ProductNo { get; set; }

        public string ProductType { get; set; }

        public string BarRule { get; set; }

        public int BarNo { get; set; }

        public string ImageUri { get; set; }
    }
}
