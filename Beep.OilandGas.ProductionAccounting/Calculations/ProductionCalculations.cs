using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Accounting;

namespace Beep.OilandGas.ProductionAccounting.Calculations
{
    /// <summary>
    /// Provides advanced production calculations.
    /// </summary>
    public static class ProductionCalculations
    {
        /// <summary>
        /// Calculates decline rate using exponential decline.
        /// </summary>
        /// <param name="initialRate">Initial production rate.</param>
        /// <param name="currentRate">Current production rate.</param>
        /// <param name="timePeriod">Time period in months.</param>
        /// <returns>Decline rate per month (decimal, 0-1).</returns>
        public static decimal CalculateExponentialDeclineRate(
            decimal initialRate,
            decimal currentRate,
            decimal timePeriod)
        {
            if (initialRate <= 0)
                throw new ArgumentException("Initial rate must be greater than zero.", nameof(initialRate));

            if (currentRate <= 0)
                throw new ArgumentException("Current rate must be greater than zero.", nameof(currentRate));

            if (timePeriod <= 0)
                throw new ArgumentException("Time period must be greater than zero.", nameof(timePeriod));

            if (currentRate >= initialRate)
                return 0;

            // Exponential decline: q = qi * e^(-Dt)
            // D = -ln(q/qi) / t
            decimal ratio = currentRate / initialRate;
            return (decimal)(-Math.Log((double)ratio) / (double)timePeriod);
        }

        /// <summary>
        /// Calculates hyperbolic decline rate.
        /// </summary>
        public static (decimal DeclineRate, decimal HyperbolicExponent) CalculateHyperbolicDecline(
            decimal initialRate,
            decimal currentRate,
            decimal timePeriod,
            decimal hyperbolicExponent = 0.5m)
        {
            if (initialRate <= 0 || currentRate <= 0 || timePeriod <= 0)
                throw new ArgumentException("All parameters must be greater than zero.");

            // Hyperbolic decline: q = qi / (1 + b*Di*t)^(1/b)
            // Simplified calculation
            decimal ratio = currentRate / initialRate;
            decimal declineRate = (decimal)(Math.Pow((double)ratio, -(double)hyperbolicExponent) - 1) / (hyperbolicExponent * timePeriod);

            return (declineRate, hyperbolicExponent);
        }

        /// <summary>
        /// Calculates cumulative production from decline curve.
        /// </summary>
        public static decimal CalculateCumulativeProduction(
            decimal initialRate,
            decimal declineRate,
            decimal timePeriod,
            bool isExponential = true)
        {
            if (isExponential)
            {
                // Exponential: Np = (qi - q) / D
                decimal finalRate = initialRate * (decimal)Math.Exp(-(double)(declineRate * timePeriod));
                return (initialRate - finalRate) / declineRate;
            }
            else
            {
                // Hyperbolic (simplified)
                return initialRate * timePeriod * 0.5m; // Simplified
            }
        }

        /// <summary>
        /// Calculates production efficiency.
        /// </summary>
        public static decimal CalculateProductionEfficiency(
            decimal actualProduction,
            decimal theoreticalMaximum)
        {
            if (theoreticalMaximum <= 0)
                throw new ArgumentException("Theoretical maximum must be greater than zero.", nameof(theoreticalMaximum));

            return (actualProduction / theoreticalMaximum) * 100m;
        }

        /// <summary>
        /// Calculates netback price (price minus all costs).
        /// </summary>
        public static decimal CalculateNetbackPrice(
            decimal salesPrice,
            decimal liftingCosts,
            decimal transportationCosts,
            decimal processingCosts,
            decimal taxes)
        {
            return salesPrice - liftingCosts - transportationCosts - processingCosts - taxes;
        }

        /// <summary>
        /// Calculates revenue per barrel.
        /// </summary>
        public static decimal CalculateRevenuePerBarrel(SalesTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (transaction.NetVolume <= 0)
                return 0;

            return transaction.TotalValue / transaction.NetVolume;
        }

        /// <summary>
        /// Calculates profit margin percentage.
        /// </summary>
        public static decimal CalculateProfitMargin(
            decimal revenue,
            decimal costs)
        {
            if (revenue <= 0)
                return 0;

            return ((revenue - costs) / revenue) * 100m;
        }

        /// <summary>
        /// Calculates break-even price.
        /// </summary>
        public static decimal CalculateBreakEvenPrice(
            decimal totalCosts,
            decimal productionVolume)
        {
            if (productionVolume <= 0)
                throw new ArgumentException("Production volume must be greater than zero.", nameof(productionVolume));

            return totalCosts / productionVolume;
        }

        /// <summary>
        /// Calculates reserve-to-production ratio (R/P ratio).
        /// </summary>
        public static decimal CalculateReserveToProductionRatio(
            decimal totalReserves,
            decimal annualProduction)
        {
            if (annualProduction <= 0)
                throw new ArgumentException("Annual production must be greater than zero.", nameof(annualProduction));

            return totalReserves / annualProduction;
        }

        /// <summary>
        /// Calculates production decline percentage.
        /// </summary>
        public static decimal CalculateProductionDeclinePercentage(
            decimal previousProduction,
            decimal currentProduction)
        {
            if (previousProduction <= 0)
                return 0;

            return ((previousProduction - currentProduction) / previousProduction) * 100m;
        }
    }
}

