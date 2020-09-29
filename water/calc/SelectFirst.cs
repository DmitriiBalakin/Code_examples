using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;


namespace CalculateWater
{
    public struct SelectFirst
    {
        public string Lic;
        public string Str_code;
        public byte Liver;
        public short Vibilo;
        public DateTime VibiloData;
        public short Vibilo2;
        public DateTime VibiloData2;
        public byte Vvodomer;
        public byte Pvodomer;
        public byte LgKan;
        public double N_VL;
        public double N_Kl;
        public double Nv;
        public double Nk;
        public double Nachisl;
        public double NvFull;
        public double NkFull;
        public int KodVedom;
        public byte Skvagina;
        public double TempNorm;
        public DateTime TempNormDate;
        public byte Sebestoim;
        public double CubeV;
        public double CubeK;
        public double AllN_vl;
        public double AllN_kl;
        public int VodKod;
        public double OverCubeV;
        public double OverCubeK;
        public double OverNV_full;
        public double OverNK_full;
        public double Poliv;
        public byte SkvPol;

        public SelectFirst(string pLic, string pStr_code, byte pLiver, short pVibilo, DateTime pVibiloData, short pVibilo2, DateTime pVibiloData2,
                    byte pVvodomer, byte pPvodomer, byte pLgKan, double pN_VL, double pN_Kl, double pNv, double pNk, 
                    double pNvFull, double pNkFull, int pKodVedom, byte pSkvagina, double pTempNorm, DateTime pTempNormDate,
                    byte pSebestoim, double pAllN_vl, double pAllN_kl, int pVodKod, byte pSkvPol)
        {
            this.Lic = pLic;
            this.Str_code = pStr_code;
            this.Liver = pLiver;
            this.Vibilo = pVibilo;
            this.VibiloData = pVibiloData;
            this.Vibilo2 = pVibilo2;
            this.VibiloData2 = pVibiloData2;
            this.Vvodomer = pVvodomer;
            this.Pvodomer = pPvodomer;
            this.LgKan = pLgKan;
            this.N_VL = pN_VL;
            this.N_Kl = pN_Kl;
            this.Nv = pNv;
            this.Nk = pNk;
            this.Nachisl = 0;
            this.NvFull = pNvFull;
            this.NkFull = pNkFull;
            this.KodVedom = pKodVedom;
            this.Skvagina = pSkvagina;
            this.TempNorm = pTempNorm;
            this.TempNormDate = pTempNormDate;
            this.Sebestoim = pSebestoim;
            this.CubeV = 0;
            this.CubeK = 0;
            this.AllN_vl = pAllN_vl;
            this.AllN_kl = pAllN_kl;
            this.VodKod = pVodKod;
            this.OverCubeV = 0;
            this.OverCubeK = 0;
            this.OverNV_full = 0;
            this.OverNK_full = 0;
            this.Poliv = 0;
            this.SkvPol = pSkvPol;
        }

        public void Save(string Period, SqlConnection conn)
        {
            string sql = "UPDATE Abonent" + Period + " SET N_VL = @N_VL, N_Kl = @N_Kl, Nv = @Nv, Nk = @Nk, " +
                            "Nachisl = @Nachisl, NvFull = @NvFull, NkFull = @NkFull, " +
                            "TempNorm = @TempNorm, CubeV = @CubeV, CubeK = @CubeK, AllN_vl = @AllN_vl, " +
                            "AllN_kl = @AllN_kl, OverCubeV = @OverCubeV, OverCubeK = @OverCubeK, OverNV_full = @OverNV_full, " +
                            "OverNK_full = @OverNK_full, poliv = @poliv " +
                            "WHERE Lic = '" + this.Lic + "'";
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@N_VL", SqlDbType.Decimal).Value = this.N_VL;
            cmd.Parameters.Add("@N_Kl", SqlDbType.Decimal).Value = this.N_Kl;
            cmd.Parameters.Add("@Nv", SqlDbType.Decimal).Value = this.Nv;
            cmd.Parameters.Add("@Nk", SqlDbType.Decimal).Value = this.Nk;
            cmd.Parameters.Add("@Nachisl", SqlDbType.Decimal).Value = this.Nachisl;
            cmd.Parameters.Add("@NvFull", SqlDbType.Decimal).Value = this.NvFull;
            cmd.Parameters.Add("@NkFull", SqlDbType.Decimal).Value = this.NkFull;
            cmd.Parameters.Add("@TempNorm", SqlDbType.Decimal).Value = this.TempNorm;
            cmd.Parameters.Add("@CubeV", SqlDbType.Decimal).Value = this.CubeV;
            cmd.Parameters.Add("@CubeK", SqlDbType.Decimal).Value = this.CubeK;
            cmd.Parameters.Add("@AllN_vl", SqlDbType.Decimal).Value = this.AllN_vl;
            cmd.Parameters.Add("@AllN_kl", SqlDbType.Decimal).Value = this.AllN_kl;
            cmd.Parameters.Add("@OverCubeV", SqlDbType.Decimal).Value = this.OverCubeV;
            cmd.Parameters.Add("@OverCubeK", SqlDbType.Decimal).Value = this.OverCubeK;
            cmd.Parameters.Add("@OverNV_full", SqlDbType.Decimal).Value = this.OverNV_full;
            cmd.Parameters.Add("@OverNK_full", SqlDbType.Decimal).Value = this.OverNK_full;
            cmd.Parameters.Add("@poliv", SqlDbType.Decimal).Value = this.Poliv;
//            cmd.Parameters.Add("@lic", SqlDbType.NVarChar).Value = this.Lic;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch
            {
                string st = Lic + "/" + N_VL + "/" + N_Kl + "/" + Nv + "/" + Nk + "/" + Nachisl + "/" + NvFull + "/" + NkFull +
                     "/" + TempNorm + "/" + CubeV + "/" + CubeK + "/" + AllN_vl + "/" + AllN_kl + "/" + OverCubeV + "/" + OverCubeK +
                     "/" + OverNV_full + "/" + OverNK_full + "/" + Poliv;
                System.IO.File.AppendAllText(@"info.log", DateTime.Now.ToString() + " Ошибка при сохранении данных " + st + "\n\r");
            }
        }

        public void SetNewDate(double pNvFull, double pNkFull, double pNPoliv, double pAllCubeV, double pAllCubeK, double pAllNormaV, double pAllNormaK)
        {
            this.NvFull = pNvFull;
            this.NkFull = pNkFull;
            this.Nv = pNvFull;
            this.Nk = pNkFull;
            this.Nachisl = pNvFull + pNkFull;
            this.Poliv = pNPoliv;
            this.CubeV = pAllCubeV;
            this.CubeK = pAllCubeK;
            this.AllN_vl = pAllNormaV;
            this.AllN_kl = pAllNormaK;
        }



    }
}
