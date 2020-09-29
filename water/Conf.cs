using System;

namespace CalculateWater
{
    public class Conf
    {
        public string PerCur;
        public string HostName;
        public string LastPer;
        public Conf(string pPerCur, string pHostName, string pLastPer)
        {
            PerCur = pPerCur;
            HostName = pHostName;
            LastPer = pLastPer;
        }
    }
}