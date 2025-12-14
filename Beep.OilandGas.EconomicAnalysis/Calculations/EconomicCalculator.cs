using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.EconomicAnalysis.Models;

namespace Beep.OilandGas.EconomicAnalysis.Calculations
{
    /// <summary>
    /// Provides economic analysis calculations.
    /// </summary>
    public static class EconomicCalculator
    {
        /// <summary>
        /// Calculates Net Present Value.
        /// </summary>
        public static double CalculateNPV(CashFlow[] cashFlows, double discountRate)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(cashFlows));

            if (discountRate < 0 || discountRate > 1)
                throw new ArgumentException("Discount rate must be between 0 and 1.", nameof(discountRate));

            double npv = 0.0;

            foreach (var cf in cashFlows)
            {
                double pv = cf.Amount / Math.Pow(1 + discountRate, cf.Period);
                npv += pv;
            }

            return npv;
        }

        /// <summary>
        /// Calculates Internal Rate of Return using Newton-Raphson method.
        /// </summary>
        public static double CalculateIRR(CashFlow[] cashFlows, double initialGuess = 0.1, double tolerance = 1e-6, int maxIterations = 100)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(cashFlows));

            double rate = initialGuess;

            for (int i = 0; i < maxIterations; i++)
            {
                double npv = NPVFunction(cashFlows, rate);
                double npvDerivative = NPVDerivative(cashFlows, rate);

                if (Math.Abs(npvDerivative) < 1e-10)
                    break;

                double newRate = rate - npv / npvDerivative;

                if (Math.Abs(newRate - rate) < tolerance)
                    return newRate;

                rate = newRate;

                // Constrain rate to reasonable bounds
                if (rate < -0.99)
                    rate = -0.99;
                if (rate > 10.0)
                    rate = 10.0;
            }

            return rate;
        }

        /// <summary>
        /// NPV function for IRR calculation.
        /// </summary>
        private static double NPVFunction(CashFlow[] cashFlows, double rate)
        {
            double npv = 0.0;
            foreach (var cf in cashFlows)
            {
                npv += cf.Amount / Math.Pow(1 + rate, cf.Period);
            }
            return npv;
        }

        /// <summary>
        /// NPV derivative for IRR calculation.
        /// </summary>
        private static double NPVDerivative(CashFlow[] cashFlows, double rate)
        {
            double derivative = 0.0;
            foreach (var cf in cashFlows)
            {
                if (cf.Period > 0)
                {
                    derivative -= cf.Period * cf.Amount / Math.Pow(1 + rate, cf.Period + 1);
                }
            }
            return derivative;
        }

        /// <summary>
        /// Calculates Modified Internal Rate of Return.
        /// </summary>
        public static double CalculateMIRR(CashFlow[] cashFlows, double financeRate, double reinvestRate)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(cashFlows));

            double positivePV = 0.0;
            double negativeFV = 0.0;

            foreach (var cf in cashFlows)
            {
                if (cf.Amount > 0)
                {
                    // Future value of positive cash flows
                    positivePV += cf.Amount * Math.Pow(1 + reinvestRate, cashFlows.Length - 1 - cf.Period);
                }
                else if (cf.Amount < 0)
                {
                    // Present value of negative cash flows
                    negativeFV += Math.Abs(cf.Amount) * Math.Pow(1 + financeRate, -cf.Period);
                }
            }

            if (negativeFV <= 0)
                return 0.0;

            double mirr = Math.Pow(positivePV / negativeFV, 1.0 / (cashFlows.Length - 1)) - 1.0;

            return mirr;
        }

        /// <summary>
        /// Calculates payback period.
        /// </summary>
        public static double CalculatePaybackPeriod(CashFlow[] cashFlows)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(cashFlows));

            double cumulative = 0.0;
            int lastNegativePeriod = -1;

            foreach (var cf in cashFlows.OrderBy(c => c.Period))
            {
                cumulative += cf.Amount;

                if (cumulative < 0)
                {
                    lastNegativePeriod = cf.Period;
                }
                else if (cumulative >= 0 && lastNegativePeriod >= 0)
                {
                    // Linear interpolation
                    double prevCumulative = cumulative - cf.Amount;
                    double periodFraction = -prevCumulative / cf.Amount;
                    return lastNegativePeriod + periodFraction;
                }
            }

            return double.MaxValue; // Never pays back
        }

        /// <summary>
        /// Calculates discounted payback period.
        /// </summary>
        public static double CalculateDiscountedPaybackPeriod(CashFlow[] cashFlows, double discountRate)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(cashFlows));

            double cumulativePV = 0.0;
            int lastNegativePeriod = -1;

            foreach (var cf in cashFlows.OrderBy(c => c.Period))
            {
                double pv = cf.Amount / Math.Pow(1 + discountRate, cf.Period);
                cumulativePV += pv;

                if (cumulativePV < 0)
                {
                    lastNegativePeriod = cf.Period;
                }
                else if (cumulativePV >= 0 && lastNegativePeriod >= 0)
                {
                    double prevPV = cumulativePV - pv;
                    double periodFraction = -prevPV / pv;
                    return lastNegativePeriod + periodFraction;
                }
            }

            return double.MaxValue;
        }

        /// <summary>
        /// Calculates Profitability Index.
        /// </summary>
        public static double CalculateProfitabilityIndex(CashFlow[] cashFlows, double discountRate)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(cashFlows));

            double pvInflows = 0.0;
            double pvOutflows = 0.0;

            foreach (var cf in cashFlows)
            {
                double pv = cf.Amount / Math.Pow(1 + discountRate, cf.Period);
                if (pv > 0)
                    pvInflows += pv;
                else
                    pvOutflows += Math.Abs(pv);
            }

            if (pvOutflows == 0)
                return double.MaxValue;

            return pvInflows / pvOutflows;
        }

        /// <summary>
        /// Calculates Return on Investment.
        /// </summary>
        public static double CalculateROI(CashFlow[] cashFlows)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(cashFlows));

            double totalReturn = cashFlows.Sum(cf => cf.Amount);
            double initialInvestment = Math.Abs(cashFlows.Where(cf => cf.Amount < 0).Sum(cf => cf.Amount));

            if (initialInvestment == 0)
                return 0.0;

            return (totalReturn / initialInvestment) * 100.0;
        }

        /// <summary>
        /// Generates NPV profile.
        /// </summary>
        public static List<NPVProfilePoint> GenerateNPVProfile(CashFlow[] cashFlows, 
            double minRate = 0.0, double maxRate = 1.0, int points = 50)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(cashFlows));

            var profile = new List<NPVProfilePoint>();

            for (int i = 0; i <= points; i++)
            {
                double rate = minRate + (maxRate - minRate) * i / points;
                double npv = CalculateNPV(cashFlows, rate);
                profile.Add(new NPVProfilePoint(rate, npv));
            }

            return profile;
        }

        /// <summary>
        /// Performs comprehensive economic analysis.
        /// </summary>
        public static EconomicResult Analyze(CashFlow[] cashFlows, double discountRate, 
            double financeRate = 0.1, double reinvestRate = 0.1)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty.", nameof(cashFlows));

            var result = new EconomicResult
            {
                DiscountRate = discountRate
            };

            result.NPV = CalculateNPV(cashFlows, discountRate);
            result.IRR = CalculateIRR(cashFlows);
            result.MIRR = CalculateMIRR(cashFlows, financeRate, reinvestRate);
            result.ProfitabilityIndex = CalculateProfitabilityIndex(cashFlows, discountRate);
            result.PaybackPeriod = CalculatePaybackPeriod(cashFlows);
            result.DiscountedPaybackPeriod = CalculateDiscountedPaybackPeriod(cashFlows, discountRate);
            result.ROI = CalculateROI(cashFlows);
            result.TotalCashFlow = cashFlows.Sum(cf => cf.Amount);
            result.PresentValue = result.NPV;

            return result;
        }
    }
}

