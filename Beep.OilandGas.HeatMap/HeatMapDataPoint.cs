using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.OilandGas.HeatMap
{
    public class HeatMapDataPoint
    {
        public HeatMapDataPoint()
        {
                
        }
        public double X { get; set; }
        public double Y { get; set; }
        public double OriginalX { get; set; }
        public double OriginalY { get; set; }
        public double Value { get; set; }
        public string Label { get; set; }
        public HeatMapDataPoint(double originalx,double originaly,double value,string label)
        {
           
            OriginalX=originalx; 
            OriginalY=originaly; 
            Value=value; 
            Label=label;
        }
    }
}
