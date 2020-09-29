using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculateWater
{
    class CountersData
    {
		public string PerOpl;
		public int KubH12V;
		public int KubH3V;	
		public int KubGV;
		public int KubH12K;
		public int KubH3K;
		public int KubGK;
		public int Liver;


        public CountersData(string PerOpl, int KubH12V, int KubH3V, int KubGV, int KubH12K, int KubH3K, int KubGK, int Liver)
        {
            this.PerOpl = PerOpl;
            this.KubH12V = KubH12V;
            this.KubH3V = KubH3V;
            this.KubGV = KubGV;
            this.KubH12K = KubH12K;
            this.KubH3K = KubH3K;
            this.KubGK = KubGK;
            this.Liver = Liver;
        }
    }
}
