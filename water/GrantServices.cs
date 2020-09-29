using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace CalculateWater
{
    class GrantServices
    {
        public int Id;
        public string PeriodOt;
        public string PeriodDo;
        public int TypeChargeID;
        public double ConsumptionRate;
        public double Coefficient;
        public int TypeServicesID;

        public GrantServices(SqlConnection conn, int pId, byte bUK)

        {
            string Base = (bUK == 0) ? "Abon.dbo." : "AbonUK.dbo.";
            this.Id = 0;
            this.PeriodOt = "";
            this.PeriodDo = "";
            this.TypeChargeID = 0;
            this.ConsumptionRate = 0;
            this.Coefficient = 0;
            this.TypeServicesID = 0;

            string sql = "SELECT Id, PeriodOt, PeriodDo, TypeChargeID, ConsumptionRate, Coefficient, TypeServicesID FROM " + Base + "GrantServices WHERE Id = @Id";
            SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = pId;
            using (SqlDataReader rsIn = cmd.ExecuteReader())
            {
                if (rsIn.HasRows)
                {
                    while (rsIn.Read())
                    {
                        this.Id = Convert.ToInt32(rsIn["Id"]);
                        this.PeriodOt = (rsIn["PeriodOt"] is DBNull) ? "" : rsIn["PeriodOt"].ToString();
                        this.PeriodDo = (rsIn["PeriodDo"] is DBNull) ? "" : rsIn["PeriodDo"].ToString();
                        this.TypeChargeID = Convert.ToInt32(rsIn["TypeChargeID"]);
                        this.ConsumptionRate = (rsIn["ConsumptionRate"] is DBNull) ? 0 : Convert.ToDouble(rsIn["ConsumptionRate"]);
                        this.Coefficient = (rsIn["Coefficient"] is DBNull) ? 0 : Convert.ToDouble(rsIn["Coefficient"]);
                        this.TypeServicesID = Convert.ToInt32(rsIn["TypeServicesID"]);
                    }
                }
                rsIn.Close();
            }

        }
    }
}
