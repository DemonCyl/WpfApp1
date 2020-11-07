using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Entity;

namespace WpfApp1.Common
{
    public class SQLHelper
    {

        public class DbHelperSQL
        {
            //private string connectionString = ConfigurationManager.ConnectionStrings["connJAS"].ConnectionString;
            private string connStr = null;
            private ConfigData configData;

            public DbHelperSQL(ConfigData data)
            {
                this.configData = data;

                this.connStr = new StringBuilder("server=" + configData.DataIpAddress +
                ";database=" + configData.DataBaseName + "; uid=" + configData.Uid + ";pwd=" + configData.Pwd + "").ToString();
            }

            public SqlConnection GetConnection()
            {
                return new SqlConnection(connStr);
            }

            
        }
    }
}
