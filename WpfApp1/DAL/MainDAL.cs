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

        public long QueryBefore4051(string barCode)
        {
            string sql = $"select t.FInterID from ProcessInfo t where (t.FDianJiBarCode = '{barCode}' or t.FQianGuanBarCode = '{barCode}') and t.F40511Status = 1 and (t.F40512Status is null or t.F40512Status != 1) ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.QueryFirstOrDefault<long>(sql);
                return re;
            }
        }

        public long QueryBefore4063(string barCode, int xinghao)
        {
            string sql = $"select t.FInterID from ProcessInfo t where (t.FDianJiBarCode = '{barCode}' ) and t.F4062Status = 1  and t.FXingHao = {xinghao} "; // and (t.FOutStatus is null or t.FOutStatus != 1) 

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.QueryFirstOrDefault<long>(sql);
                return re;
            }
        }

        public long QueryBefore405(string barCode,int gwNo, int xinghao)
        {
            string sql = $"select t.FInterID from ProcessInfo t where (t.FDianJiBarCode = '{barCode}' or t.FQianGuanBarCode = '{barCode}' or t.FLXingBarCode = '{barCode}' or t.FCeBanBarCode = '{barCode}') and t.FXingHao = {xinghao} ";
            if (gwNo == 4052)
            {
                sql += " and t.F40512Status = 1 "; //and (t.F4052Status is null or t.F4052Status != 1) ";
            } else if (gwNo == 4053)
            {
                sql += " and t.F4052Status = 1 "; // and (t.FOutStatus is null or t.FOutStatus != 1) ";
            }

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.QueryFirstOrDefault<long>(sql);
                return re;
            }
        }

        public List<string> GetBarCodeList(long fid)
        {
            string sql = $"select FBarCode from ProcessInfoEntry1 where FProcessInfoID = {fid} ";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.Query<string>(sql).ToList();
                return re;
            }
        }

        public bool SaveItem(ProductConfig info)
        {
            string sql = @" INSERT INTO ProductConfig (FZCType,FXingHao,FDianJiCodeRule,FQianGuanCodeRule,FStatus1,FLXingCodeRule,FStatus2,FCeBanCodeRule,FStatus3,FCodeSum,FGWItem,FDate) VALUES
                            (@F1,@F2,@F4,@F5,@F6,@F7,@F8,@F9,@F10,@F11,@F12, GETDATE())";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = info.FZCType,
                    F2 = info.FXingHao,
                    F4 = info.FDianJiCodeRule,
                    F5 = info.FQianGuanCodeRule,
                    F6 = info.FStatus1,
                    F7 = info.FLXingCodeRule,
                    F8 = info.FStatus2,
                    F9 = info.FCeBanCodeRule,
                    F10 = info.FStatus3,
                    F11 = info.FCodeSum,
                    F12 = info.FGWItem

                }) > 0;
            }
        }

        public bool UpdateItem(ProductConfig info)
        {
            string sql = @" UPDATE ProductConfig SET FZCType=@F1,FXingHao=@F2,FDianJiCodeRule=@F4,FQianGuanCodeRule=@F5,FStatus1=@F6,FLXingCodeRule=@F7,FStatus2=@F8,FCeBanCodeRule=@F9,FStatus3=@F10,FCodeSum=@F11,FGWItem = @F12
                            WHERE FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = info.FZCType,
                    F2 = info.FXingHao,
                    F4 = info.FDianJiCodeRule,
                    F5 = info.FQianGuanCodeRule,
                    F6 = info.FStatus1,
                    F7 = info.FLXingCodeRule,
                    F8 = info.FStatus2,
                    F9 = info.FCeBanCodeRule,
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

        public long SaveBarCode405(int XingHao, string DianJiCode, string QianGuanCode)
        {
            long id = 0;
            string sql = @" INSERT INTO ProcessInfo (FXingHao,FDianJiBarCode,FQianGuanBarCode,FGW,FOnTime40511) values 
                             (@F1,@F2,@F3,@F4, GETDATE());select SCOPE_IDENTITY();";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand(sql, conn, tran);
                    cmd.Parameters.AddWithValue("@F1", XingHao);
                    cmd.Parameters.AddWithValue("@F2", DianJiCode);
                    cmd.Parameters.AddWithValue("@F3", QianGuanCode);
                    cmd.Parameters.AddWithValue("@F4", "405");

                    id = Convert.ToInt64(cmd.ExecuteScalar());

                    tran.Commit();
                    return id;
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    tran.Rollback();
                    return 0;
                }
            }
        }
        public bool UpdateData40511(long fid, List<GDbData> list)
        {
            GDbData data1 = new GDbData();
            GDbData data2 = new GDbData();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            data1 = list[0];
                            break;
                        case 1:
                            data2 = list[1];
                            break;
                    }
                }
            }
            string sql = @"update ProcessInfo set FTorque40511=@F1,FAngle40511=@F2,FTorque40512=@F3,FAngle40512=@F4, FTime4051 = GETDATE(),F40511Status = 1 where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = data1.Torque,
                    F2 = data1.Angle,
                    F3 = data2.Torque,
                    F4 = data2.Angle,
                    Id = fid,
                }) > 0;
            }
        }
        public bool UpdateData40512(long fid)
        {
            string sql = @"update ProcessInfo set F40512Status = 1 where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    Id = fid,
                }) > 0;
            }
        }
        public bool UpdateBarCode40512(long fid,string LXingCode,string CeBanCode)
        {
            string sql = @"update ProcessInfo set FLXingBarCode=@F1, FCeBanBarCode=@F2, FOnTime40512 = GETDATE() where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = LXingCode,
                    F2 = CeBanCode,
                    Id = fid,
                }) > 0;
            }
        }
        public bool UpdateBarCode4052(long fid)
        {
            string sql = @"update ProcessInfo set  FOnTime4052 = GETDATE() where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    Id = fid,
                }) > 0;
            }
        }
        public bool UpdateData4052(long fid, List<GDbData> list)
        {
            GDbData data1 = new GDbData();
            GDbData data2 = new GDbData();
            GDbData data3 = new GDbData();
            GDbData data4 = new GDbData();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            data1 = list[0];
                            break;
                        case 1:
                            data2 = list[1];
                            break;
                        case 2:
                            data3 = list[2];
                            break;
                        case 3:
                            data4 = list[3];
                            break;
                    }
                }
            }
            string sql = @"update ProcessInfo set FTorque40521=@F1,FAngle40521=@F2,FTorque40522=@F3,FAngle40522=@F4,FTorque40523=@F5,FAngle40523=@F6,FTorque40524=@F7,FAngle40524=@F8, FTime4052 = GETDATE(),F4052Status = 1 where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = data1.Torque,
                    F2 = data1.Angle,
                    F3 = data2.Torque,
                    F4 = data2.Angle,
                    F5 = data3.Torque,
                    F6 = data3.Angle,
                    F7 = data4.Torque,
                    F8 = data4.Angle,
                    Id = fid,
                }) > 0;
            }
        }
        public bool UpdateBarCode4053(long fid)
        {
            string sql = @"update ProcessInfo set  FOnTime4053 = GETDATE() where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    Id = fid,
                }) > 0;
            }
        }
        public bool UpdateData4053(long fid)
        {
            string sql = @"update ProcessInfo set FOutStatus = 1 where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    Id = fid,
                }) > 0;
            }
        }

        public long SaveBarCode406(int XingHao, string DianJiCode)
        {
            long id = 0;
            string sql = @" INSERT INTO ProcessInfo (FXingHao,FDianJiBarCode,FGW,FOnTime4061) values 
                             (@F1,@F2,@F3, GETDATE());select SCOPE_IDENTITY();";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();
                try
                {
                    SqlCommand cmd = new SqlCommand(sql, conn, tran);
                    cmd.Parameters.AddWithValue("@F1", XingHao);
                    cmd.Parameters.AddWithValue("@F2", DianJiCode);
                    cmd.Parameters.AddWithValue("@F3", "406");

                    id = Convert.ToInt64(cmd.ExecuteScalar());

                    tran.Commit();
                    return id;
                }
                catch (Exception ex)
                {
                    log.Error(ex.Message);
                    tran.Rollback();
                    return 0;
                }
            }
        }
        public bool UpdateData4061(long fid, List<GDbData> list)
        {
            GDbData data1 = new GDbData();
            GDbData data2 = new GDbData();
            GDbData data3 = new GDbData();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            data1 = list[0];
                            break;
                        case 1:
                            data2 = list[1];
                            break;
                        case 2:
                            data3 = list[2];
                            break;
                    }
                }
            }
            string sql = @"update ProcessInfo set FTorque40611=@F1,FAngle40611=@F2,FTorque40612=@F3,FAngle40612=@F4,FTorque40613=@F5,FAngle40613=@F6, FTime4061 = GETDATE(),F4061Status = 1 where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = data1.Torque,
                    F2 = data1.Angle,
                    F3 = data2.Torque,
                    F4 = data2.Angle,
                    F5 = data3.Torque,
                    F6 = data3.Angle,
                    Id = fid,
                }) > 0;
            }
        }
        public bool UpdateBarCode4062(long fid)
        {
            string sql = @"update ProcessInfo set  FOnTime4062 = GETDATE() where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    Id = fid,
                }) > 0;
            }
        }
        public bool UpdateData4062(long fid, List<GDbData> list)
        {
            GDbData data1 = new GDbData();
            GDbData data2 = new GDbData();
            GDbData data3 = new GDbData();
            GDbData data4 = new GDbData();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            data1 = list[0];
                            break;
                        case 1:
                            data2 = list[1];
                            break;
                        case 2:
                            data3 = list[2];
                            break;
                        case 3:
                            data4 = list[3];
                            break;
                    }
                }
            }
            string sql = @"update ProcessInfo set FTorque40621=@F1,FAngle40621=@F2,FTorque40622=@F3,FAngle40622=@F4,FTorque40623=@F5,FAngle40623=@F6,FTorque40624=@F7,FAngle40624=@F8, FTime4062 = GETDATE(),F4062Status = 1 where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = data1.Torque,
                    F2 = data1.Angle,
                    F3 = data2.Torque,
                    F4 = data2.Angle,
                    F5 = data3.Torque,
                    F6 = data3.Angle,
                    F7 = data4.Torque,
                    F8 = data4.Angle,
                    Id = fid,
                }) > 0;
            }
        }

        public bool UpdateBarCode4063(long fid)
        {
            string sql = @"update ProcessInfo set  FOnTime4063 = GETDATE() where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    Id = fid,
                }) > 0;
            }
        }

        public bool UpdateData4063(long fid, List<GDbData> list)
        {
            GDbData data1 = new GDbData();
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            data1 = list[0];
                            break;
                    }
                }
            }
            string sql = @"update ProcessInfo set FTorque40631=@F1,FAngle40631=@F2, FTime4063 = GETDATE(),FOutStatus = 1 where FInterID = @Id";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                return conn.Execute(sql, new
                {
                    F1 = data1.Torque,
                    F2 = data1.Angle,
                    Id = fid,
                }) > 0;
            }
        }

        public long check406(string barcode,int xinghao)
        {
            string sql = $"select t.FInterID from ProcessInfo t where (t.FDianJiBarCode = '{barcode}' ) and (t.F4061Status is null or t.F4061Status != 1) and t.FGW = '406' and FXingHao = {xinghao}";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.QueryFirstOrDefault<long>(sql);
                return re;
            }
        }

        public long check405(int XingHao, string DianJiCode, string QianGuanCode)
        {
            string sql = $"select t.FInterID from ProcessInfo t where (t.FDianJiBarCode = '{DianJiCode}' and t.FQianGuanBarCode = '{QianGuanCode}') and (t.F40511Status is null or t.F40511Status != 1) and t.FGW = '405' and FXingHao = {XingHao}";

            using (var conn = new DbHelperSQL(config).GetConnection())
            {
                var re = conn.QueryFirstOrDefault<long>(sql);
                return re;
            }
        }
    }
}
