using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
using Entity;
    public class GetInfoService
    {
        public string GetStaStr(int value)
        {
            string conn = null;
            switch (value)
            {
                case 1:
                    conn = "DB2000.DBW0.0";
                    break;
                case 2:
                    conn = "DB2000.DBW2.0";
                    break;
                case 3:
                    conn = "DB2000.DBW4.0";
                    break;
                case 4:
                    conn = "DB2000.DBW6.0";
                    break;
                case 5:
                    conn = "DB2000.DBW8.0";
                    break;
            }
            return conn;
        }

        public string GetTypeStr(int value)
        {
            string conn = null;
            switch (value)
            {
                case 1:
                    conn = "DB2000.DBW5110.0";
                    break;
                case 2:
                    conn = "DB2000.DBW5112.0";
                    break;
                case 3:
                    conn = "DB2000.DBW5114.0";
                    break;
                case 4:
                    conn = "DB2000.DBW5116.0";
                    break;
                case 5:
                    conn = "DB2000.DBW5118.0";
                    break;
                case 6:
                    conn = "DB2000.DBW5120.0";
                    break;
                case 7:
                    conn = "DB2000.DBW5122.0";
                    break;
                case 8:
                    conn = "DB2000.DBW5124.0";
                    break;
                case 9:
                    conn = "DB2000.DBW5126.0";
                    break;
                case 10:
                    conn = "DB2000.DBW5128.0";
                    break;
            }
            return conn;
        }

        public GDbStr GetGunStr(int value)
        {
            GDbStr str = new GDbStr();
            switch(value)
            {
                case 1:
                    str.TorqueStr = "DB2000.DBD10.0";
                    str.AngleStr = "DB2000.DBD14.0";
                    str.ResultStr = "DB2000.DBX18.0";
                    break;
                case 2:
                    str.TorqueStr = "DB2000.DBD20.0";
                    str.AngleStr = "DB2000.DBD24.0";
                    str.ResultStr = "DB2000.DBX28.0";
                    break;
                case 3:
                    str.TorqueStr = "DB2000.DBD30.0";
                    str.AngleStr = "DB2000.DBD34.0";
                    str.ResultStr = "DB2000.DBX38.0";
                    break;
                case 4:
                    str.TorqueStr = "DB2000.DBD40.0";
                    str.AngleStr = "DB2000.DBD44.0";
                    str.ResultStr = "DB2000.DBX48.0";
                    break;
                case 5:
                    str.TorqueStr = "DB2000.DBD50.0";
                    str.AngleStr = "DB2000.DBD54.0";
                    str.ResultStr = "DB2000.DBX58.0";
                    break;
                case 6:
                    str.TorqueStr = "DB2000.DBD60.0";
                    str.AngleStr = "DB2000.DBD64.0";
                    str.ResultStr = "DB2000.DBX68.0";
                    break;
                case 7:
                    str.TorqueStr = "DB2000.DBD70.0";
                    str.AngleStr = "DB2000.DBD74.0";
                    str.ResultStr = "DB2000.DBX78.0";
                    break;
                case 8:
                    str.TorqueStr = "DB2000.DBD80.0";
                    str.AngleStr = "DB2000.DBD84.0";
                    str.ResultStr = "DB2000.DBX88.0";
                    break;
                case 9:
                    str.TorqueStr = "DB2000.DBD90.0";
                    str.AngleStr = "DB2000.DBD94.0";
                    str.ResultStr = "DB2000.DBX98.0";
                    break;
                case 10:
                    str.TorqueStr = "DB2000.DBD100.0";
                    str.AngleStr = "DB2000.DBD104.0";
                    str.ResultStr = "DB2000.DBX108.0";
                    break;
            }
            return str;
        }

        public BarCodeStr GetBarCodeStr(int value)
        {
            var tmp = value+1;
            BarCodeStr str = new BarCodeStr();
            str.BarStr = value*100+10;
            str.ResultStr = "DB2000.DBX"+tmp+"08.0";
            return str;
        }
    }
}