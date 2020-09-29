using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;


namespace CalculateWater
{
    class ODNCalculate
    {
        List<ODNHouse> Houses = new List<ODNHouse>();
        public ODNCalculate(SqlConnection conn, string PerCur, string LastPer)
        {
            // -- Открываем соединение с базой -------------------------------------------------------------------
            string sql = "SELECT House_Code FROM [Common].[dbo].[HousesData] " +
                            "WHERE IsCalcODN = 1 and (((PerBeg = 0) and ((PerEnd = 0) or (PerEnd >= @PerCur))) or " +
                            "(PerBeg <= @PerCur and ((PerEnd = 0) or (PerEnd >= @PerCur))))";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@PerCur", SqlDbType.NVarChar).Value = PerCur;
            using (SqlDataReader readHouses = cmd.ExecuteReader())
            {
                if (readHouses.HasRows)
                {
                    while (readHouses.Read())
                    {
                        Houses.Add(new ODNHouse(conn, Convert.ToInt32(readHouses["House_Code"])));
                    }
                }
                readHouses.Close();
            }
            if (Houses.Count > 0)
            {
                System.IO.File.WriteAllText(@"D:\Work\WriteLines.txt", "");
                for (int i = 0; i < Houses.Count; i++)
                {
                    Houses[i].FillHouse(conn, PerCur, LastPer);
                    Houses[i].CalculateODN();
                    Houses[i].Save();
                }
            }
        }
    }
}
