using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;


namespace CalculateWater
{
    public class SelectFirst
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
        public double Nvk;
        public double Nk;
        public double Nachisl;
        public double NvFull;
        public double NvkFull;
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
        public double Poliv;
        public byte SkvPol;
        public double OverNV_Full;
        public bool DevCombination;

        public SelectFirst(string pLic, string pStr_code, byte pLiver, short pVibilo, DateTime pVibiloData, short pVibilo2, DateTime pVibiloData2,
                    byte pVvodomer, byte pPvodomer, byte pLgKan, double pN_VL, double pN_Kl, int pKodVedom, byte pSkvagina, double pTempNorm, DateTime pTempNormDate,
                    byte pSebestoim, double pAllN_vl, double pAllN_kl, int pVodKod, byte pSkvPol, double pOverNV_Full, bool devCombination)
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
            this.Nv = 0;
            this.Nvk = 0;
            this.Nk = 0;
            this.Nachisl = 0;
            this.NvFull = 0;
            this.NvkFull = 0;
            this.NkFull = 0;
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
            this.Poliv = 0;
            this.SkvPol = pSkvPol;
            this.OverNV_Full = pOverNV_Full;
            this.DevCombination = devCombination;
        }

        public void Save(string Period, SqlConnection conn)
        {
            string Base = this.Lic.Substring(0, 1) == "1" ? "Abon.dbo." : "AbonUK.dbo.";
            string str = "UPDATE " + Base + "Abonent" + Period + @" SET N_VL = " + this.N_VL.ToString() +
                           ", N_Kl = " + this.N_Kl.ToString() + ", Nv = " + this.Nv.ToString() + ", Nvk = " + this.Nvk.ToString() + ", Nk = " + this.Nk.ToString() + 
                           ", Nachisl = " + this.Nachisl.ToString() + ", NvFull = " + this.NvFull.ToString() + ", NvkFull = " + this.NvkFull.ToString() + ", NkFull = " + this.NkFull.ToString() +
                           ", TempNorm = " + this.TempNorm.ToString() + ", CubeV = " + this.CubeV.ToString() + ", CubeK = " + this.CubeK.ToString() +
                           ", AllN_vl = " + this.AllN_vl.ToString() + ", AllN_kl = " + this.AllN_kl.ToString() + ", poliv = " + this.Poliv.ToString() + " WHERE Lic = " + this.Lic.ToString();
            SqlCommand cmd = new SqlCommand(str, conn);
            cmd.ExecuteNonQuery();
        }

        public void SetNewDate(double pNvFull, double pNvkFull, double pNkFull, double pNPoliv, double pAllCubeV, double pAllCubeK, double pAllNormaV, double pAllNormaK)
        {
            this.NvFull = pNvFull;
            this.NvkFull = pNvkFull;
            this.NkFull = pNkFull;
            this.Nv = pNvFull;
            this.Nvk = pNvkFull;
            this.Nk = pNkFull;
            this.Nachisl = pNvkFull + pNvFull + pNkFull;
            this.Poliv = pNPoliv;
            this.CubeV = pAllCubeV;
            this.CubeK = pAllCubeK;
            this.AllN_vl = pAllNormaV;
            this.AllN_kl = pAllNormaK;
        }
    }
}
