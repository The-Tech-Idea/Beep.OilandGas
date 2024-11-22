using System.Collections.Generic;

namespace Beep.PumpPerformance
{
    public class PumpPerformanceCalc
    {
        public static double[]  HQCalc(double[] flowRate, double[] head, double[] power)
        {
            //double[] flowRate = { 100, 200, 300, 400, 500, 600 }; // Flow rate in barrels per day
            //double[] head = { 100, 90, 75, 60, 45, 30 }; // Head in feet
            //double[] power = { 80, 150, 230, 310, 380, 440 }; // Power in horsepower
            // Calculate the efficiency using the power and head
            double[] efficiency = new double[flowRate.Length];
            for (int i = 0; i < flowRate.Length; i++)
            {
                efficiency[i] = power[i] / (flowRate[i] * head[i] / 3960);
            }
            return efficiency;

            //// Plot the pump performance curve
            //var chart = new Chart();
            //chart.Series.Add(new Series("Performance"));
            //chart.Series[0].ChartType = SeriesChartType.Line;
            //for (int i = 0; i < flowRate.Length; i++)
            //{
            //    chart.Series[0].Points.AddXY(flowRate[i], head[i]);
            //}
        }
        public static List<CfactorOutput> CFactorCalc(double motorInputPower, double[] flowRate,double[] head)
        {
            // Define the input parameters
            //double motorInputPower = 200; // Motor input power in horsepower
            //double[] flowRate = { 100, 200, 300, 400, 500, 600 }; // Flow rate in barrels per day
            //double[] head = { 100, 90, 75, 60, 45, 30 }; // Head in feet

            // Calculate the C-Factor based on the motor input power
            double cFactor = motorInputPower / Math.Pow(flowRate[0], 3);
            List<CfactorOutput> retval = new List<CfactorOutput>();
            // Calculate the pump head and power based on the C-Factor
            double[] pumpHead = new double[flowRate.Length];
            double[] pumpPower = new double[flowRate.Length];
            for (int i = 0; i < flowRate.Length; i++)
            {

                pumpHead[i] = head[0] * Math.Pow(flowRate[i] / flowRate[0], 2);
                pumpPower[i] = cFactor * Math.Pow(flowRate[i], 3);
                retval.Add(new CfactorOutput(pumpHead[i], pumpPower[i]));
            }
            return retval;
            //// Plot the pump performance curve
            //var chart = new Chart();
            //chart.Series.Add(new Series("Performance"));
            //chart.Series[0].ChartType = SeriesChartType.Line;
            //for (int i = 0; i < flowRate.Length; i++)
            //{
            //    chart.Series[0].Points.AddXY(flowRate[i], pumpHead[i]);
            //}
        }

        public class CfactorOutput
        {
            public CfactorOutput()
            {
                
            }
            public double PumpHead { get; set; }
            public double PumpPower { get; set; }
            public CfactorOutput(double pumpHead, double pumpPower)
            {
                PumpHead = pumpHead;
                PumpPower   = pumpPower;
            }
        }

    }
}