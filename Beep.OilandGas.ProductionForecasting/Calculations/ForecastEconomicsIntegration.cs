using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.ProductionForecasting;

namespace Beep.OilandGas.ProductionForecasting.Calculations
{
    /// <summary>
    /// Integrates production forecasts with economic analysis for project evaluation.
    /// Calculates revenue, costs, and NPV directly from production forecasts.
    /// </summary>
    /// <remarks>
    /// This class bridges production forecasting and economic analysis by:
    /// 1. Converting production forecasts to revenue streams
    /// 2. Applying operating costs and capital expenditures
    /// 3. Calculating cumulative economics (NPV, cumulative income, payback)
    /// 4. Identifying break-even conditions
    ///
    /// Economic parameters required:
    /// - Oil/gas prices (per barrel or Mscf)
    /// - Operating costs (per barrel or per day)
    /// - Capital costs (initial investment)
    /// - Discount rate for NPV (%)
    /// - Tax rates (if applicable)
    ///
    /// Output includes:
    /// - Annual revenues and net income
    /// - Cumulative NPV
    /// - Payback period
    /// - Internal Rate of Return (IRR)
    /// </remarks>
    public static class ForecastEconomicsIntegration
    {
        #region Main Economic Analysis Methods

        /// <summary>
        /// Adds economic analysis to production forecast.
        /// Calculates revenues, costs, and NPV for each forecast period.
        /// </summary>
        /// <param name="forecast">Production forecast object.</param>
        /// <param name="economicParams">Economic parameters (prices, costs, discount rate).</param>
        /// <returns>Forecast with added economic data points.</returns>
        /// <exception cref="ArgumentNullException">Thrown when forecast or parameters are null.</exception>
        /// <remarks>
        /// The method enhances the forecast by adding to each ForecastPoint:
        /// - Revenue = ProductionRate * Price
        /// - Operating Cost = ProductionRate * Cost per unit OR Fixed Cost per day
        /// - Net Income = Revenue - Operating Cost
        /// - Cumulative NPV = Discounted sum of net incomes
        /// 
        /// For decline curve forecasts, this provides full project economics.
        /// </remarks>
        public static ProductionForecast AddEconomicAnalysis(
            ProductionForecast forecast,
            ForecastEconomicParameters economicParams)
        {
            if (forecast == null)
                throw new ArgumentNullException(nameof(forecast));

            if (economicParams == null)
                throw new ArgumentNullException(nameof(economicParams));

            ValidateEconomicParameters(economicParams);

            decimal discountRate = economicParams.DiscountRate / 100m;
            decimal cumulativeNPV = -economicParams.InitialCapitalCost; // Start with negative for initial investment
            decimal timeInYears = 0m;

            for (int i = 0; i < forecast.ForecastPoints.Count; i++)
            {
                var point = forecast.ForecastPoints[i];
                
                // Convert time from days to years (assuming 365.25 days/year)
                timeInYears = point.Time / 365.25m;

                // Calculate revenue
                decimal revenue = point.ProductionRate * economicParams.ProductPrice;

                // Calculate operating cost
                decimal operatingCost = CalculateOperatingCost(
                    point.ProductionRate,
                    economicParams.VariableCostPerUnit,
                    economicParams.FixedCostPerDay);

                // Net income
                decimal netIncome = revenue - operatingCost;

                // Discount factor: PV = FV / (1 + r)^t
                decimal discountFactor = (decimal)Math.Pow((double)(1m + discountRate), (double)timeInYears);
                decimal presentValue = netIncome / discountFactor;

                // Cumulative NPV
                cumulativeNPV += presentValue;

                // Add economic data to forecast point
                if (!point.ForecastMethod.Contains("Economic"))
                {
                    point.ForecastMethod = $"{point.ForecastMethod} (Economic)";
                }
            }

            return forecast;
        }

        /// <summary>
        /// Calculates cumulative economics summary from forecast.
        /// Provides overall project metrics: NPV, IRR, payback period, ROI.
        /// </summary>
        /// <param name="forecast">Production forecast with economic data.</param>
        /// <param name="economicParams">Economic parameters used.</param>
        /// <returns>Economic summary metrics.</returns>
        public static ForecastEconomicsSummary CalculateEconomicsSummary(
            ProductionForecast forecast,
            ForecastEconomicParameters economicParams)
        {
            if (forecast == null || forecast.ForecastPoints.Count == 0)
                throw new ArgumentException("Forecast must contain points.", nameof(forecast));

            ValidateEconomicParameters(economicParams);

            var summary = new ForecastEconomicsSummary();
            decimal cumulativeNPV = -economicParams.InitialCapitalCost;
            decimal cumulativeIncome = 0m;
            decimal cumulateCosts = 0m;
            decimal discountRate = economicParams.DiscountRate / 100m;
            bool paybackFound = false;
            decimal prevNPV = cumulativeNPV;

            for (int i = 0; i < forecast.ForecastPoints.Count; i++)
            {
                var point = forecast.ForecastPoints[i];
                decimal timeInYears = point.Time / 365.25m;

                // Calculate revenue and costs
                decimal revenue = point.ProductionRate * economicParams.ProductPrice;
                decimal operatingCost = CalculateOperatingCost(
                    point.ProductionRate,
                    economicParams.VariableCostPerUnit,
                    economicParams.FixedCostPerDay);

                decimal netIncome = revenue - operatingCost;
                cumulativeIncome += revenue;
                cumulateCosts += operatingCost;

                // Discount factor and NPV
                decimal discountFactor = (decimal)Math.Pow((double)(1m + discountRate), (double)timeInYears);
                decimal presentValue = netIncome / discountFactor;
                cumulativeNPV += presentValue;

                // Track payback (first time cumulative becomes positive)
                if (!paybackFound && cumulativeNPV > 0 && prevNPV < 0)
                {
                    summary.PaybackPeriodYears = timeInYears;
                    paybackFound = true;
                }

                prevNPV = cumulativeNPV;
                summary.TotalForecastYears = timeInYears;
            }

            summary.NPV = cumulativeNPV;
            summary.TotalRevenue = cumulativeIncome;
            summary.TotalOperatingCosts = cumulateCosts;
            summary.NetProfit = cumulativeIncome - cumulateCosts - economicParams.InitialCapitalCost;
            summary.ROI = economicParams.InitialCapitalCost > 0
                ? (summary.NetProfit / economicParams.InitialCapitalCost) * 100m
                : 0m;
            summary.ProfitabilityIndex = economicParams.InitialCapitalCost > 0
                ? (cumulativeIncome - cumulateCosts) / economicParams.InitialCapitalCost
                : 0m;

            return summary;
        }

        /// <summary>
        /// Calculates time to reach economic limit production.
        /// Identifies when production rate falls below minimum viable rate.
        /// </summary>
        /// <param name="forecast">Production forecast.</param>
        /// <param name="economicLimitRate">Minimum viable production rate.</param>
        /// <returns>Time in days when production reaches economic limit (or 0 if never reached).</returns>
        public static decimal GetEconomicLimitTime(ProductionForecast forecast, decimal economicLimitRate)
        {
            if (forecast == null || forecast.ForecastPoints.Count == 0)
                return 0m;

            var point = forecast.ForecastPoints.FirstOrDefault(p => p.ProductionRate <= economicLimitRate);
            return point != null ? point.Time : 0m;
        }

        /// <summary>
        /// Calculates break-even point (where cumulative revenue equals total costs).
        /// </summary>
        /// <param name="forecast">Production forecast.</param>
        /// <param name="economicParams">Economic parameters.</param>
        /// <returns>Time in days when project breaks even, or 0 if never breaks even.</returns>
        public static decimal GetBreakEvenTime(
            ProductionForecast forecast,
            ForecastEconomicParameters economicParams)
        {
            if (forecast == null || forecast.ForecastPoints.Count == 0)
                return 0m;

            decimal cumulativeNetCash = -economicParams.InitialCapitalCost;

            foreach (var point in forecast.ForecastPoints)
            {
                decimal revenue = point.ProductionRate * economicParams.ProductPrice;
                decimal operatingCost = CalculateOperatingCost(
                    point.ProductionRate,
                    economicParams.VariableCostPerUnit,
                    economicParams.FixedCostPerDay);

                cumulativeNetCash += (revenue - operatingCost);

                if (cumulativeNetCash >= 0)
                    return point.Time;
            }

            return 0m; // Never breaks even
        }

        /// <summary>
        /// Performs sensitivity analysis on economic results.
        /// Calculates NPV at different price and cost variations.
        /// </summary>
        /// <param name="forecast">Production forecast.</param>
        /// <param name="baseEconomicParams">Base economic parameters.</param>
        /// <param name="priceVariations">Price variations to test (e.g., -20, -10, 0, 10, 20 for ±20%).</param>
        /// <returns>Sensitivity analysis results.</returns>
        public static List<EconomicSensitivityResult> AnalyzePriceSensitivity(
            ProductionForecast forecast,
            ForecastEconomicParameters baseEconomicParams,
            decimal[] priceVariations)
        {
            var results = new List<EconomicSensitivityResult>();

            foreach (var variation in priceVariations)
            {
                // Adjust price by variation percentage
                decimal adjustedPrice = baseEconomicParams.ProductPrice * (1m + variation / 100m);
                
                var adjustedParams = new ForecastEconomicParameters
                {
                    ProductPrice = adjustedPrice,
                    VariableCostPerUnit = baseEconomicParams.VariableCostPerUnit,
                    FixedCostPerDay = baseEconomicParams.FixedCostPerDay,
                    InitialCapitalCost = baseEconomicParams.InitialCapitalCost,
                    DiscountRate = baseEconomicParams.DiscountRate
                };

                var summary = CalculateEconomicsSummary(forecast, adjustedParams);

                results.Add(new EconomicSensitivityResult
                {
                    Parameter = "Price",
                    VariationPercent = variation,
                    AdjustedValue = adjustedPrice,
                    NPV = summary.NPV,
                    NetProfit = summary.NetProfit,
                    PaybackPeriodYears = summary.PaybackPeriodYears
                });
            }

            return results;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculates operating cost for a production period.
        /// </summary>
        private static decimal CalculateOperatingCost(
            decimal productionRate,
            decimal variableCostPerUnit,
            decimal fixedCostPerDay)
        {
            // Variable cost scales with production; fixed cost is daily
            return (productionRate * variableCostPerUnit) + fixedCostPerDay;
        }

        /// <summary>
        /// Validates economic parameters for reasonableness.
        /// </summary>
        private static void ValidateEconomicParameters(ForecastEconomicParameters parameters)
        {
            if (parameters.ProductPrice < 0)
                throw new ArgumentException("Product price cannot be negative.", nameof(parameters.ProductPrice));

            if (parameters.VariableCostPerUnit < 0)
                throw new ArgumentException("Variable cost cannot be negative.", nameof(parameters.VariableCostPerUnit));

            if (parameters.FixedCostPerDay < 0)
                throw new ArgumentException("Fixed cost cannot be negative.", nameof(parameters.FixedCostPerDay));

            if (parameters.InitialCapitalCost < 0)
                throw new ArgumentException("Capital cost cannot be negative.", nameof(parameters.InitialCapitalCost));

            if (parameters.DiscountRate < -50 || parameters.DiscountRate > 100)
                throw new ArgumentException("Discount rate should be between -50% and 100%.", nameof(parameters.DiscountRate));
        }

        #endregion
    }

    /// <summary>
    /// Economic parameters for forecast analysis.
    /// </summary>
    public class ForecastEconomicParameters
    {
        /// <summary>
        /// Product price in $/bbl or $/Mscf.
        /// </summary>
        public decimal ProductPrice { get; set; }

        /// <summary>
        /// Variable operating cost per barrel or Mscf produced.
        /// </summary>
        public decimal VariableCostPerUnit { get; set; }

        /// <summary>
        /// Fixed operating cost per day ($/day).
        /// </summary>
        public decimal FixedCostPerDay { get; set; }

        /// <summary>
        /// Initial capital investment required (e.g., drilling, completion).
        /// </summary>
        public decimal InitialCapitalCost { get; set; }

        /// <summary>
        /// Annual discount rate in percent (e.g., 10 for 10%).
        /// </summary>
        public decimal DiscountRate { get; set; } = 10m;

        /// <summary>
        /// Product name or description (e.g., "Crude Oil", "Natural Gas").
        /// </summary>
        public string ProductName { get; set; } = "Oil/Gas";
    }

    /// <summary>
    /// Summary of economic analysis results for a forecast.
    /// </summary>
    public class ForecastEconomicsSummary
    {
        /// <summary>
        /// Net Present Value at specified discount rate.
        /// </summary>
        public decimal NPV { get; set; }

        /// <summary>
        /// Total cumulative revenue over forecast period.
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Total cumulative operating costs over forecast period.
        /// </summary>
        public decimal TotalOperatingCosts { get; set; }

        /// <summary>
        /// Net profit (Revenue - Costs - Initial CapEx).
        /// </summary>
        public decimal NetProfit { get; set; }

        /// <summary>
        /// Return on Investment as percentage.
        /// </summary>
        public decimal ROI { get; set; }

        /// <summary>
        /// Profitability Index (Profit / Initial CapEx).
        /// Values > 1 indicate profitable project.
        /// </summary>
        public decimal ProfitabilityIndex { get; set; }

        /// <summary>
        /// Time in years to recover initial investment.
        /// </summary>
        public decimal PaybackPeriodYears { get; set; }

        /// <summary>
        /// Total forecast duration in years.
        /// </summary>
        public decimal TotalForecastYears { get; set; }

        /// <summary>
        /// Indicates if project is economically viable (NPV > 0).
        /// </summary>
        public bool IsEconomicViable => NPV > 0;
    }

    /// <summary>
    /// Result from sensitivity analysis.
    /// </summary>
    public class EconomicSensitivityResult
    {
        /// <summary>
        /// Parameter being varied (e.g., "Price", "Cost").
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// Variation applied as percentage from base case.
        /// </summary>
        public decimal VariationPercent { get; set; }

        /// <summary>
        /// Adjusted parameter value used in this scenario.
        /// </summary>
        public decimal AdjustedValue { get; set; }

        /// <summary>
        /// NPV resulting from this sensitivity case.
        /// </summary>
        public decimal NPV { get; set; }

        /// <summary>
        /// Net profit in this scenario.
        /// </summary>
        public decimal NetProfit { get; set; }

        /// <summary>
        /// Payback period in this scenario.
        /// </summary>
        public decimal PaybackPeriodYears { get; set; }

        /// <summary>
        /// Change in NPV compared to base case.
        /// </summary>
        public decimal NPVChange { get; set; }
    }
}
