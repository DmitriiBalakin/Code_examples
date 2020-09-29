using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CalculateWater
{
    class ODNFlat
    {
        public string Lic;
        public string FlatNumber;
        public double WaterCube;
        public double Moneys;
        public double Area;
        public double ODNCube;
        public double ODNMoney;
        public byte bUK;

        public ODNFlat(string pLic, string pFlatNumber, double pWaterCube, double pMoneys, double pArea, byte pbUK)
        {
            this.Lic = pLic;
            this.FlatNumber = pFlatNumber;
            this.WaterCube = pWaterCube;
            this.Moneys = pMoneys;
            this.Area = pArea;
            this.ODNCube = 0;
            this.ODNMoney = 0;
            this.bUK = pbUK;
        }
    }
}
