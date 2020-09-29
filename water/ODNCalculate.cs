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
        public ODNCalculate(SqlConnection conn, string PerCur, string LastPer, int HouseCode = 0)
        {
            string Lc = "";
            if (HouseCode > 0)
            {
                Lc = " and House_Code = " + HouseCode.ToString();
            }
            string sql = "SELECT House_Code FROM [Common].[dbo].[HousesData] " +
                            "WHERE IsCalcODN = 1 and PerBeg <= @PerCur and " +
                            "(case when PerEnd = 0 then @PerCur else PerEnd end) >= @PerCur" + Lc;
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@PerCur", SqlDbType.NVarChar).Value = PerCur;
            using (SqlDataReader readHouses = cmd.ExecuteReader())
            {
                if (readHouses.HasRows)
                {
                    while (readHouses.Read())
                    {
                        Houses.Add(new ODNHouse(conn, Convert.ToInt32(readHouses["House_Code"].ToString())));
                    }
                }
                readHouses.Close();
            }
            if (Houses.Count > 0)
            {
                System.IO.File.WriteAllText(@"info.log", DateTime.Now.ToString() + " Пересчет начислений\n");
                for (int i = 0; i < Houses.Count; i++)
                {
                    Houses[i].FillHouse(PerCur, LastPer);
                    Houses[i].CalculateODN();
                    Houses[i].Save();
                }
                //System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Вычисляем ОДН.\n");
                //cmd = new SqlCommand("Common.dbo.CalcHouseODN", conn);
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.Add("@DataBase", SqlDbType.NVarChar).Value = "Abon";
                //cmd.Parameters.Add("@PerCurent", SqlDbType.NVarChar).Value = LastPer;
                //cmd.CommandTimeout = 0;
                //cmd.ExecuteNonQuery();
                //System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Начисления ОДН в ABON произведены.\n");
                //cmd = new SqlCommand("Common.dbo.CalcHouseODN", conn);
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Parameters.Add("@DataBase", SqlDbType.NVarChar).Value = "AbonUK";
                //cmd.Parameters.Add("@PerCurent", SqlDbType.NVarChar).Value = LastPer;
                //cmd.ExecuteNonQuery();
                //System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Начисления ОДН в ABONUK произведены.\n");
                System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Начисления ОДН произведены. Вычисляем сальдо\n");
                cmd = new SqlCommand("Abon.dbo.SetAllChargeOnAbonServ", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Per", SqlDbType.NVarChar).Value = LastPer;
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand("AbonUK.dbo.SetAllChargeOnAbonServ", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Per", SqlDbType.NVarChar).Value = LastPer;
                cmd.ExecuteNonQuery();
                System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Вычисление сальдо закончено\n");
                System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Все начисления выполнены!\n");
            }
            Houses = null;
        }
    }
}
