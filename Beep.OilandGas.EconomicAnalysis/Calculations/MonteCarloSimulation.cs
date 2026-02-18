using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.EconomicAnalysis;

namespace Beep.OilandGas.EconomicAnalysis.Calculations
{
    /// <summary>
    /// Provides Monte Carlo simulation for risk analysis.
    /// </summary>
    public static class MonteCarloSimulation
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Runs a Monte Carlo simulation on cash flows.
        /// </summary>
        /// <param name="baseCashFlows">Base case cash flows.</param>
        /// <param name="discountRate">Discount rate.</param>
        /// <param name="iterations">Number of iterations (default 1000).</param>
        /// <param name="revenueVolatility">Standard deviation for revenue variation (normalized, e.g. 0.2 for 20%).</param>
        /// <param name="costVolatility">Standard deviation for cost variation (normalized, e.g. 0.1 for 10%).</param>
        /// <returns>Monte Carlo result with probability distribution stats.</returns>
        public static MONTE_CARLO_RESULT RunSimulation(
            CashFlow[] baseCashFlows,
            double discountRate,
            int iterations = 1000,
            double revenueVolatility = 0.2,
            double costVolatility = 0.1)
        {
            if (baseCashFlows == null || baseCashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(baseCashFlows));

            var npvResults = new List<double>(iterations);
            var irrResults = new List<double>(iterations);

            for (int i = 0; i < iterations; i++)
            {
                // Generate a random scenario
                // Apply independent random factors to Revenue and Cost for this iteration
                // Ideally we'd have time-series correlation, but simple global factors are a good start.
                
                double revenueFactor = GenerateLogNormal(1.0, revenueVolatility);
                double costFactor = GenerateLogNormal(1.0, costVolatility);

                var scenarioCashFlows = baseCashFlows.Select(cf =>
                {
                    if (cf.Amount > 0) return new CashFlow(cf.Period, cf.Amount * revenueFactor, cf.Description);
                    if (cf.Amount < 0) return new CashFlow(cf.Period, cf.Amount * costFactor, cf.Description);
                    return cf;
                }).ToArray();

                npvResults.Add(EconomicCalculator.CalculateNPV(scenarioCashFlows, discountRate));
                
                // IRR calculation can fail or be negative, handle gracefully
                try
                {
                    irrResults.Add(EconomicCalculator.CalculateIRR(scenarioCashFlows));
                }
                catch
                {
                    irrResults.Add(0); // or NaN, but 0 safer for stats if failure rare
                }
            }

            npvResults.Sort();
            irrResults.Sort();

            return new MONTE_CARLO_RESULT
            {
                Iterations = iterations,
                MeanNPV = (decimal)npvResults.Average(),
                MedianNPV = (decimal)GetPercentile(npvResults, 0.5),
                P10_NPV = (decimal)GetPercentile(npvResults, 0.1), // 10% probability of being LOWER (Conservative/Pessimistic view usually P90 in reserves, but in finance P10 often low end)
                P90_NPV = (decimal)GetPercentile(npvResults, 0.9),
                
                MeanIRR = (decimal)irrResults.Average(),
                MedianIRR = (decimal)GetPercentile(irrResults, 0.5),
                P10_IRR = (decimal)GetPercentile(irrResults, 0.1),
                P90_IRR = (decimal)GetPercentile(irrResults, 0.9)
            };
        }

        // Helper: Generate LogNormal random variable
        // Used for prices/costs to avoid negative values and represent skew
        private static double GenerateLogNormal(double mean, double stdDev)
        {
            // Box-Muller transform for Normal distribution
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            // Convert Process Mean/StdDev to LogNormal mu/sigma
            double variance = stdDev * stdDev;
            double mu = Math.Log(mean * mean / Math.Sqrt(variance + mean * mean));
            double sigma = Math.Sqrt(Math.Log(variance / (mean * mean) + 1));

            return Math.Exp(mu + sigma * randStdNormal);
        }

        private static double GetPercentile(List<double> sortedData, double percentile)
        {
            int index = (int)(percentile * (sortedData.Count - 1));
            return sortedData[index];
        }
    }

    public class MONTE_CARLO_RESULT
    {
        public int Iterations { get; set; }
        public decimal MeanNPV { get; set; }
        public decimal MedianNPV { get; set; }
        public decimal P10_NPV { get; set; } // Low estimate (estimates exceeded 90% of time in some conventions, or bottom 10% in others. Here: Bottom 10% value)
        public decimal P90_NPV { get; set; } // High estimate
        
        public decimal MeanIRR { get; set; }
        public decimal MedianIRR { get; set; }
        public decimal P10_IRR { get; set; }
        public decimal P90_IRR { get; set; }
    }
}
