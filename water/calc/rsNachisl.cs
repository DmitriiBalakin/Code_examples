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
        public int Lic;
        public short GranServiceID;
        public double Amount;
        public double Norma;
        public double Cube;
        public double Nachisl;
        public rsNachisl(int pId, int pLic, short pGranServiceID, double pAmount, double pNorma, double pCube, double pNachisl)
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
            SqlCommand cmdUpdate = new SqlCommand("UPDATE AbonentNach" + pPerCur + " SET Norma = @Norma, Cube = @Cube, Nachisl = @Nachisl WHERE ID = @ID", conn);
            cmdUpdate.Parameters.Add("@ID", SqlDbType.Int).Value = this.Id;
            cmdUpdate.Parameters.Add("@Norma", SqlDbType.Decimal).Value = this.Norma;
            cmdUpdate.Parameters.Add("@Cube", SqlDbType.Decimal).Value = this.Cube;
            cmdUpdate.Parameters.Add("@Nachisl", SqlDbType.Decimal).Value = this.Nachisl;
            cmdUpdate.ExecuteNonQuery();
            return 1;
        }
    }
}
