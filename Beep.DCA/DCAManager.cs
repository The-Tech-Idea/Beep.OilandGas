using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.DCA
{
    public  class DCAManager
    {
        public DCAManager()
        {
            
        }
        public double qi { get; set; } // initial production rate
        public double b { get; set; } // decline rate
        public double di { get; set; } // initial decline rate
        public DateTime t { get; set; } // time since start of production
        public double q { get; set; } // production rate at time t
        public static double[] GenerateDCA(List<double> productionData, List<DateTime> timeData, double qi=1000, double di=.1)
        {
           
            double[] parameters = DCAGenerator.FitCurve(productionData, timeData, qi, di);
            return parameters;
           // Console.WriteLine("Estimated decline rate: {0}", b);
        }
    }
}
