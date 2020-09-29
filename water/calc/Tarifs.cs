using System;
using System.Data;
using System.Data.SqlClient;

namespace CalculateWater
{
    public class Tarifs
    {
        public double TarV;
        public double TarK;
        public double TarP;
        public double TarSebV;
        public double TarSebK;
        public double TarSebVEcO;
        public double TarSebKEcO;
        public double ProcNeKachUslug;
        public Tarifs(string Period, string LastPeriod, string StrCode, SqlConnection conn)
        {
            if (Period == LastPeriod)
            {
                Int16 TarifCode = 1;
                string sql = "SELECT TarifKod FROM Street WHERE cod_yl = @StrCode";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@StrCode", SqlDbType.NVarChar).Value = StrCode;
                using (SqlDataReader rsIn = cmd.ExecuteReader())
                {
                    if (rsIn.HasRows)
                    {
                        while (rsIn.Read())
                        {
                            TarifCode = Convert.ToInt16(rsIn["TarifKod"]);
                        }
                    }
                    rsIn.Close();
                }
                sql = "SELECT * FROM SpTarif WHERE per = @LastPeriod and IdTarif = @IdTarif";
                cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@LastPeriod", SqlDbType.NVarChar).Value = LastPeriod;
                cmd.Parameters.Add("@IdTarif", SqlDbType.SmallInt).Value = TarifCode;
                using (SqlDataReader rsIn = cmd.ExecuteReader())
                {
                    if (rsIn.HasRows)
                    {
                        while (rsIn.Read())
                        {
                            this.TarV = Convert.ToDouble(rsIn["vsum"]);
                            this.TarK = Convert.ToDouble(rsIn["Ksum"]);
                            this.TarP = Convert.ToDouble(rsIn["Psum"]);
                            this.TarSebV = Convert.ToDouble(rsIn["SebestV"]);
                            this.TarSebK = Convert.ToDouble(rsIn["SebestK"]);
                            this.TarSebVEcO = Convert.ToDouble(rsIn["SebestVEc"]);
                            this.TarSebKEcO = Convert.ToDouble(rsIn["SebestKEc"]);
                            this.ProcNeKachUslug = Convert.ToDouble(rsIn["ProcNeKachUslug"]);
                        }
                    }
                    rsIn.Close();
                }
            }
            else
            {
                string sql = "SELECT st.vsum, st.ksum, st.psum, st.SebestV, st.SebestK, st.SebestVEc, st.SebestKEc, st.ProcNeKachUslug " +
                    "FROM  StreetHist AS sh INNER JOIN SpTarif AS st ON sh.Per = st.Per AND sh.TarifKod = st.IdTarif " +
                    "WHERE sh.cod_yl = @StrCode AND st.Per = @Period";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@StrCode", SqlDbType.NVarChar).Value = StrCode;
                cmd.Parameters.Add("@Period", SqlDbType.NVarChar).Value = Period;
                using (SqlDataReader rsIn = cmd.ExecuteReader())
                {
                    if (rsIn.HasRows)
                    {
                        while (rsIn.Read())
                        {
                            this.TarV = Convert.ToDouble(rsIn["vsum"]);
                            this.TarK = Convert.ToDouble(rsIn["Ksum"]);
                            this.TarP = Convert.ToDouble(rsIn["Psum"]);
                            this.TarSebV = Convert.ToDouble(rsIn["SebestV"]);
                            this.TarSebK = Convert.ToDouble(rsIn["SebestK"]);
                            this.TarSebVEcO = Convert.ToDouble(rsIn["SebestVEc"]);
                            this.TarSebKEcO = Convert.ToDouble(rsIn["SebestKEc"]);
                            this.ProcNeKachUslug = Convert.ToDouble(rsIn["ProcNeKachUslug"]);
                        }
                    }
                    rsIn.Close();
                }
            }
        }
    }
}