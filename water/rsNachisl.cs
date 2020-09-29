using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;


namespace CalculateWater
{
    public class rsNachisl
    {
        public int Id;
        public string Lic;
        public short GranServiceID;
        public double Amount;
        public double Norma;
        public double Cube;
        public double Nachisl;
        public rsNachisl(int pId, string pLic, short pGranServiceID, double pAmount, double pNorma, double pCube, double pNachisl)
        {
            this.Id = pId;
            this.Lic = pLic;
            this.GranServiceID = pGranServiceID;
            this.Amount = pAmount;
            this.Norma = pNorma;
            this.Cube = pCube;
            this.Nachisl = pNachisl;
        }
        public int SaveNach(string pPerCur, SqlConnection conn)
        {
            if (this.Norma == 0 && this.Cube == 0 && this.Nachisl == 0)
            {
                return 0;
            }
            string Base = this.Lic.Substring(0, 1) == "1" ? "Abon.dbo." : "AbonUK.dbo.";
            string str = " UPDATE " + Base + "AbonentNach" + pPerCur + " SET Norma = " + this.Norma.ToString() +
                             ", Cube = " + this.Cube.ToString() + ", Nachisl = " + this.Nachisl.ToString() +
                             " WHERE ID = " + this.Id.ToString() + "; ";
            SqlCommand cmd = new SqlCommand(str, conn);
            cmd.ExecuteNonQuery();
            return 1;
        }
    }
}
