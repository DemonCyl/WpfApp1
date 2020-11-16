using Dapper;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Entity;
using static WpfApp1.Common.SQLHelper;

namespace WpfApp1.DAL
{
    public class MainDAL
    {

        private ConfigData config;
        private ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainDAL(ConfigData data)
        {
            this.config = data;
        }

        public List<ProductConfig> QueryItem()
        {
            string sql = "select * from ProductConfig";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.Query<ProductConfig>(sql).ToList();
                return re;
            }
        }

        public bool SaveItem(ProductConfig info)
        {
            string sql = @" INSERT INTO ProductConfig (FZCType,FXingHao,FCodeRule,FCodeRule1,FStatus1,FCodeRule2,FStatus2,FCodeRule3,FStatus3,FCodeSum,FGWItem,FDate) VALUES
                            (@F1,@F2,@F4,@F5,@F6,@F7,@F8,@F9,@F10,@F11,@F12, GETDATE())";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = info.FZCType,
                    F2 = info.FXingHao,
                    F4 = info.FCodeRule,
                    F5 = info.FCodeRule1,
                    F6 = info.FStatus1,
                    F7 = info.FCodeRule2,
                    F8 = info.FStatus2,
                    F9 = info.FCodeRule3,
                    F10 = info.FStatus3,
                    F11 = info.FCodeSum,
                    F12 = info.FGWItem

                }) > 0;
            }
        }

        public bool UpdateItem(ProductConfig info)
        {
            string sql = @" UPDATE ProductConfig SET FZCType=@F1,FXingHao=@F2,FCodeRule=@F4,FCodeRule1=@F5,FStatus1=@F6,FCodeRule2=@F7,FStatus2=@F8,FCodeRule3=@F9,FStatus3=@F10,FCodeSum=@F11,FGWItem = @F12
                            WHERE FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = info.FZCType,
                    F2 = info.FXingHao,
                    F4 = info.FCodeRule,
                    F5 = info.FCodeRule1,
                    F6 = info.FStatus1,
                    F7 = info.FCodeRule2,
                    F8 = info.FStatus2,
                    F9 = info.FCodeRule3,
                    F10 = info.FStatus3,
                    F11 = info.FCodeSum,
                    F12 = info.FGWItem,
                    Id = info.FInterID

                }) > 0;
            }
        }

        public bool DeleteItem(int id)
        {
            string sql = @"delete from ProductConfig where FInterID = @id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new { id = id }) > 0;
            }
        }

        public bool SaveInfo(int id, string process, List<string> barList, List<GDbData> list)
        {
            string sql = @" INSERT INTO ProcessInfo (FProductID,FProcess,FDate) values 
                             (@F1,@F2, GETDATE());select SCOPE_IDENTITY();";

            string sql1 = @" INSERT INTO ProcessInfoEntry (FProcessInfoID,FTorque,FAngle,FStatus) values
                             (@F11,@F21,@F31,@F41);";

            string sql2 = @" INSERT INTO ProcessInfoEntry1 (FProcessInfoID,FBarcode) values
                             (@F13,@F23);";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand(sql, conn, tran);
                    cmd.Parameters.AddWithValue("@F1", id);
                    cmd.Parameters.AddWithValue("@F2", process);

                    int processId = Convert.ToInt32(cmd.ExecuteScalar());
                    if (barList != null)
                    {
                        if (barList.Any())
                        {

                            foreach (var l in barList)
                            {
                                SqlCommand cmd2 = new SqlCommand(sql2, conn, tran);
                                cmd2.Parameters.AddWithValue("@F13", processId);
                                cmd2.Parameters.AddWithValue("@F23", l);

                                cmd2.ExecuteNonQuery();
                            }
                        }
                    }

                    if (list != null)
                    {
                        if (list.Any())
                        {

                            foreach (var l in list)
                            {
                                SqlCommand cmd1 = new SqlCommand(sql1, conn, tran);
                                cmd1.Parameters.AddWithValue("@F11", processId);
                                cmd1.Parameters.AddWithValue("@F21", l.Torque);
                                cmd1.Parameters.AddWithValue("@F31", l.Angle);
                                cmd1.Parameters.AddWithValue("@F41", l.Result);

                                cmd1.ExecuteNonQuery();
                            }
                        }
                    }


                    tran.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    tran.Rollback();
                    return false;
                }
            }
        }

    }
}
