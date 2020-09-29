using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculateWater
{
    class CountersData
    {
		public string lic;
        public string PerOpl;
        public byte Gl;
        public byte H1Another;
        public byte H2Another;
        public byte H3Another;
        public int KubH1V;
        public int KubH2V;
        public int KubH3V;
        public int Id_House;
        public int id_HWorg;
        public byte HotWaterAnother;
        public byte Circulation;
        public byte G1Another;
        public byte G2Another;
        public byte G3Another;
        public int KubGV1;
        public int KubGV2;
        public int KubGV3;
        public byte Hk1Another;
        public byte Hk2Another;
        public byte Hk3Another;
        public byte Gk1Another;
        public byte Gk2Another;
        public byte Gk3Another;
        public int Liver;

        public CountersData(string lic, string PerOpl, byte Gl, byte H1Another, byte H2Another, byte H3Another, int KubH1V,
		        int KubH2V, int KubH3V, int Id_House, int id_HWorg, byte HotWaterAnother, byte Circulation, byte G1Another,
                byte G2Another, byte G3Another, int KubGV1, int KubGV2, int KubGV3, byte Hk1Another, byte Hk2Another, byte Hk3Another,
		        byte Gk1Another, byte Gk2Another, byte Gk3Another, int Liver)
        {
            this.lic = lic;
            this.PerOpl = PerOpl;
            this.Gl= Gl;
            this.H1Another = H1Another;
            this.H2Another = H2Another;
            this.H3Another = H3Another;
            this.KubH1V = KubH1V;
            this.KubH2V = KubH2V;
            this.KubH3V = KubH3V;
            this.Id_House = Id_House;
            this.id_HWorg = id_HWorg;
            this.HotWaterAnother = HotWaterAnother;
            this.Circulation = Circulation;
            this.G1Another = G1Another;
            this.G2Another = G2Another;
            this.G3Another = G3Another;
            this.KubGV1 = KubGV1;
            this.KubGV2 = KubGV2;
            this.KubGV3 = KubGV3;
            this.Hk1Another = Hk1Another;
            this.Hk2Another = Hk2Another;
            this.Hk3Another = Hk3Another;
            this.Gk1Another = Gk1Another;
            this.Gk2Another = Gk2Another;
            this.Gk3Another = Gk3Another;
            this.Liver = Liver;
        }
    }
}
