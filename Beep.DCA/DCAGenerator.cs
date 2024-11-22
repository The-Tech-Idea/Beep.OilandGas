using System;
using System.Collections.Generic;
using System.Text;

namespace Beep.DCA
{
    public static class DCAGenerator
    {
        //public static double qi { get; set; } // initial production rate
        //public static double b { get; set; } // decline rate
        //public static double di { get; set; } // initial decline rate
        //public static DateTime t { get; set; } // time since start of production
        //public static double q { get; set; } // production rate at time t
        //public static double B { get; set; } = 0.5;
       // public static double Di { get; set; } = 0.0; //initial decline rate(Di)
        //public  DCAGenerator(double pqi, double pb, double pdi, DateTime pt, bool GasWell = false)
        //{
        //    qi = pqi;
        //    b = pb;
        //    di = pdi;
        //    t = pt;
        //    if (!GasWell)
        //    {
        //        CalculateProductionRate();
        //    }
        //    else
        //        CalculateGasWellProductionRate();

        //}
        // Exponential decline method
        public static double ExponentialDecline(double Qi, double Di, double t)
        {
            return Qi * Math.Exp(-Di * t);
        }
        // Harmonic decline method
        public static double HarmonicDecline(double Qi, double Di, double t)
        {
            return Qi / (1 + Di * t);
        }
        // Hyperbolic (parabolic) decline method
        public static double HyperbolicDecline(double Qi, double Di, double t, double b)
        {
            return Qi / Math.Pow(1 + b * Di * t, 1 / b);
        }
        public static double[] EstimateQiDiB(double B,double[] productionData, DateTime[] timeData)
        {
            double initialProductionRate = productionData[0];
            double cumulativeProduction = 0.0;
            double timeSinceFirstProduction = 0.0;
            double[] qOverQt = new double[productionData.Length];

            for (int i = 0; i < productionData.Length; i++)
            {
                double qi = initialProductionRate;
                double qt = productionData[i];
                double t = (timeData[i] - timeData[0]).TotalDays;

                qOverQt[i] = qt / qi;

                cumulativeProduction += (qi - qt) / B * Math.Pow(qi / qt, (1.0 - B) / B) * (Math.Exp(-B * t) - 1.0);

                timeSinceFirstProduction = t;
            }

            double b = (Math.Log(qOverQt[qOverQt.Length - 1]) - Math.Log(qOverQt[0])) / ((timeSinceFirstProduction - 0.0) / 365.25);

            return new double[] { initialProductionRate, cumulativeProduction, b };
        }
        private static double  CalculateProductionRate(DateTime t,double qi,double b,double di)
        {
            double days = (t - new DateTime(2000, 1, 1)).TotalDays; // convert to days since 1/1/2000
            double q = qi / Math.Pow((1 + b * di * days), (1 / b));
            return q;
        }
        private static double CalculateGasWellProductionRate(DateTime t, double qi, double b, double di)
        {
            double days = (t - new DateTime(2000, 1, 1)).TotalDays; // convert to days since 1/1/2000
            double q = qi / Math.Pow((1 + b * di * days), ((1 - b) / b));
            return q;
        }
        public static double[] FitCurve(List<double> productionData, List<DateTime> timeData, double qi, double di)
        {
            double[] p = new double[] { qi, 1.0 }; // initial guess for the parameters
            Func<DateTime, double>[] basis = new Func<DateTime, double>[] {
            t => 1.0,
            t => Math.Pow((1 + p[1] * di * (t - new DateTime(2000, 1, 1)).TotalDays), (-1.0 / p[1]))
        }; // basis functions for the least-squares regression

            int iterations = 100; // maximum number of iterations
            double tolerance = 1E-8; // convergence tolerance

            // perform the least-squares regression to estimate the parameters
            double[] parameters = NonlinearRegression.Solve(basis, productionData.ToArray(), timeData.ToArray(), p, iterations, tolerance);

            return parameters;
        }
        public static double[] FitCurve(List<double> productionData, List<DateTime> timeData, double[] initialGuess, Func<DateTime, double>[] basis)
        {
            double[] p = initialGuess;  // Initial guess for the parameters
            int iterations = 100;       // Maximum number of iterations
            double tolerance = 1E-8;    // Convergence tolerance

            // Perform the least-squares regression to estimate the parameters
            double[] parameters = NonlinearRegression.Solve(basis, productionData.ToArray(), timeData.ToArray(), p, iterations, tolerance);

            return parameters;
        }

        public static double[] EstimateQiDi(List<double> productionData, List<DateTime> timeData, double b,double di)
        {
            //The b parameter is used in the MBM to calculate the pressure decline in the reservoir, which is then used to estimate the initial production rate and the initial decline rate.
            double[] p = new double[] { 1000, 0.1 }; // initial guess for qi and di
            double[] residuals = new double[productionData.Count];

            int iterations = 100; // maximum number of iterations
            double tolerance = 1E-8; // convergence tolerance

            double[] qLog = productionData.Select(Math.Log10).ToArray();
            double[] tLog = timeData.Select(t => Math.Log10((t - timeData.First()).TotalDays)).ToArray();

            for (int i = 0; i < iterations; i++)
            {
                // calculate the expected decline curve for the current qi and di
                for (int j = 0; j < productionData.Count; j++)
                {
                    residuals[j] = qLog[j] - Math.Log10(FitCurve(productionData.Take(j + 1).ToList(), timeData.Take(j + 1).ToList(), p[0], p[1])[j]);
                }
                    // calculate the Jacobian matrix
                    double[,] jacobian = new double[productionData.Count, 2];
                

                    for (int j = 0; j < productionData.Count; j++)
                    {
                        jacobian[j, 0] = 1.0 / (p[1] * Math.Log(10) * Math.Pow((1 + p[1] * di * (timeData[j] - timeData.First()).TotalDays), ((1 - b) / b)));
                        jacobian[j, 1] = -(Math.Log10((timeData[j] - timeData.First()).TotalDays) / b) * (Math.Log10((timeData[j] - timeData.First()).TotalDays) + Math.Log10(1 + p[1] * di * (timeData[j] - timeData.First()).TotalDays));
                    }
                
                    // solve the linear system to obtain the parameter updates
                    double[] dp = SolveLinearSystem(jacobian, residuals);

                    // update the parameters
                    p[0] = Math.Pow(10.0, Math.Log10(p[0]) + dp[0]);
                    p[1] += dp[1];

                    // check for convergence
                    if (Math.Abs(dp[0]) < tolerance && Math.Abs(dp[1]) < tolerance)
                    {
                        break;
                    }
                }

                if (p[1] <= 0.0)
                {
                    throw new InvalidOperationException("Failed to estimate qi and di.");
                }

                double[] qiDi = new double[] { p[0], p[1] };

                return qiDi;
            
        }
        public static double EstimateQiForGasWell(double reservoirPressure, double depth, double gasGravity, double temperature, double permeability)
        {
            double T = (temperature - 32) / 1.8; // convert to Celsius
            double p = reservoirPressure / 14.7; // convert to psia

            double A = Math.Exp(-3.9 + 0.07 * depth + 1.95 * Math.Log10(gasGravity) - 0.0014 * T - 0.35 * Math.Log10(permeability)); // reservoir area (acres)
            double D = Math.Exp(-7.67 + 0.076 * depth + 2.6 * Math.Log10(gasGravity) - 0.00045 * T - 0.28 * Math.Log10(permeability)); // reservoir thickness (ft)

            double qi = 0.484 * A * D * Math.Sqrt(p) * gasGravity; // initial production rate (MMSCFD)

            return qi;
        }
        private static double[] SolveLinearSystem(double[,] A, double[] b)
        {
            int n = b.Length;
            double[,] Ab = new double[n, n + 1];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Ab[i, j] = A[i, j];
                }

                Ab[i, n] = b[i];
            }

            for (int i = 0; i < n; i++)
            {
                // find pivot row and swap
                int maxIndex = i;

                for (int j = i + 1; j < n; j++)
                {
                    if (Math.Abs(Ab[j, i]) > Math.Abs(Ab[maxIndex, i]))
                    {
                        maxIndex = j;
                    }
                }

                double[] temp = new double[n + 1];

                for (int j = 0; j <= n; j++)
                {
                    temp[j] = Ab[i, j];
                    Ab[i, j] = Ab[maxIndex, j];
                    Ab[maxIndex, j] = temp[j];
                }

                // pivot within rows
                for (int j = i + 1; j < n; j++)
                {
                    double factor = Ab[j, i] / Ab[i, i];

                    for (int k = i + 1; k <= n; k++)
                    {
                        Ab[j, k] -= factor * Ab[i, k];
                    }
                }
            }

            double[] x = new double[n];

            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0.0;

                for (int j = i + 1; j < n; j++)
                {
                    sum += Ab[i, j] * x[j];
                }

                x[i] = (Ab[i, n] - sum) / Ab[i, i];
            }

            return x;

        }
        public static double[] EstimateQiDiMBM(double[] productionData, DateTime[] timeData, double porosity, double thickness, double area, double reservoirPressure, double gasGravity, double oilGravity, double waterSaturation, double formationVolumeFactor, double B, double Di)
        {
            //B is the formation volume factor, and Di is the nominal decline rate, both of which are separate
            // Calculate the average reservoir pressure
            double averagePressure = 0.0;
            for (int i = 0; i < productionData.Length; i++)
            {
                averagePressure += reservoirPressure * (1.0 - waterSaturation);
            }
            averagePressure /= productionData.Length;

            // Calculate the oil formation volume factor
            double oilFormationVolumeFactor = formationVolumeFactor / (1.0 + 0.000025 * (averagePressure - 14.7));

            // Calculate the total produced oil and gas volumes
            double totalOilVolume = 0.0;
            double totalGasVolume = 0.0;
            for (int i = 0; i < productionData.Length; i++)
            {
                double p = reservoirPressure * (1.0 - B * Di * (float)(timeData[i] - timeData[0]).TotalDays);
                double gasFormationVolumeFactor = 0.002303 * gasGravity * (520.0 + 460.0) / (p * oilFormationVolumeFactor);
                double solutionGasOilRatio = 0.00006 * gasGravity * (520.0 + 460.0) * Math.Exp(0.0125 * oilGravity / gasGravity * p / (520.0 + 460.0));
                double gasOilRatio = solutionGasOilRatio * gasFormationVolumeFactor / oilFormationVolumeFactor;
                double oilVolume = productionData[i] / (1.0 - waterSaturation) * oilFormationVolumeFactor;
                double gasVolume = oilVolume * gasOilRatio;
                totalOilVolume += oilVolume;
                totalGasVolume += gasVolume;
            }

            // Calculate the initial hydrocarbon in place
            double initialHydrocarbonInPlace = (7758.0 * area * thickness * porosity * (averagePressure - 14.7) * totalOilVolume) / (oilFormationVolumeFactor * totalGasVolume);

            // Calculate the initial production rate
            double initialProductionRate = totalOilVolume / (timeData[timeData.Length - 1] - timeData[0]).TotalDays;

            // Calculate the initial decline rate
            double initialDeclineRate = Math.Log(initialProductionRate / (initialProductionRate - totalOilVolume / initialHydrocarbonInPlace)) / (timeData[timeData.Length - 1] - timeData[0]).TotalDays;

            return new double[] { initialProductionRate, initialDeclineRate };
        }
        // Estimate initial decline rate using linear regression
        public static double EstimateInitialDeclineRate(double[] time, double[] production)
        {
            // Production data: time (months) and production rate (barrels per month)
          //  double[] time = new double[] { 0, 1, 2, 3, 4, 5, 6 };
           // double[] production = new double[] { 1000, 900, 810, 730, 657, 591, 532 };
            int n = time.Length;

            // Convert production rates to logarithms
            double[] logProduction = production.Select(x => Math.Log(x)).ToArray();

            double sumTime = time.Sum();
            double sumLogProduction = logProduction.Sum();
            double sumTimeLogProduction = 0;
            double sumTimeSquared = 0;

            for (int i = 0; i < n; i++)
            {
                sumTimeLogProduction += time[i] * logProduction[i];
                sumTimeSquared += Math.Pow(time[i], 2);
            }

            double slope = (n * sumTimeLogProduction - sumTime * sumLogProduction) /
                           (n * sumTimeSquared - Math.Pow(sumTime, 2));

            return -slope;
        }
        // Estimate initial decline rate using linear regression
        public static double EstimateInitialDeclineRate(DateTime[] time, double[] production)
        {
            // Convert DateTime array to a double array representing the time difference in days
            DateTime startTime = time[0];
            double[] timeDifference = time.Select(x => (x - startTime).TotalDays).ToArray();
            int n = time.Length;

            // Convert production rates to logarithms
            double[] logProduction = production.Select(x => Math.Log(x)).ToArray();

            double sumTime = timeDifference.Sum();
            double sumLogProduction = logProduction.Sum();
            double sumTimeLogProduction = 0;
            double sumTimeSquared = 0;

            for (int i = 0; i < n; i++)
            {
                sumTimeLogProduction += timeDifference[i] * logProduction[i];
                sumTimeSquared += Math.Pow(timeDifference[i], 2);
            }

            double slope = (n * sumTimeLogProduction - sumTime * sumLogProduction) /
                           (n * sumTimeSquared - Math.Pow(sumTime, 2));

            return -slope;
        }
        
    }

}



