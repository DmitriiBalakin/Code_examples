using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace CalculateWater
{
    class EmptyCountersCalculate
    {
        public EmptyCountersCalculate(string PerCur, SqlConnection conn)
        {
            string sql = "SELECT lic from Config where HostName=(Host_Name())";
            SqlCommand cmd = new SqlCommand(sql, conn);
            using (SqlDataReader rsIn = cmd.ExecuteReader())
            {
                if (rsIn.HasRows)
                {
                    while (rsIn.Read())
                    {
//                        DateTime DateCur = (rsIn.IsDBNull(rsIn.GetOrdinal("DateCur")) ? DateTime.MinValue : rsIn.GetDateTime(rsIn.GetOrdinal("DateCur")));
//                        conf = new Conf(rsIn["PerCur"].ToString(), rsIn["PerOld"].ToString(), rsIn["PerNew"].ToString(),
//                            rsIn["HostName"].ToString(), DateCur, rsIn["A_Cur"].ToString(), rsIn["P_Cur"].ToString(),
//                            rsIn["S_Cur"].ToString(), rsIn["LastPer"].ToString());
                    }
                }
                rsIn.Close();
            }
        }


    }
}
