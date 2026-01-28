using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.EconomicAnalysis;
using Beep.OilandGas.EconomicAnalysis.Calculations;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using EconomicAnalysisResult = Beep.OilandGas.Models.Data.Calculations.EconomicAnalysisResult;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.LifeCycle.Services.Calculations
{
    public partial class PPDMCalculationService
    {
        /// <summary>
        /// Performs economic analysis (NPV, IRR, etc.) for a well, pool, or field.
        /// </summary>
        /// <param name="request">Economic analysis request containing entity IDs, economic parameters, and optional production forecast</param>
        /// <returns>Economic analysis result with NPV, IRR, payback period, cash flows, and additional metrics</returns>
        /// <exception cref="ArgumentException">Thrown when request validation fails</exception>
        /// <exception cref="InvalidOperationException">Thrown when cash flow data is unavailable or calculation fails</exception>
        public async Task<EconomicAnalysisResult> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.WellId) && string.IsNullOrEmpty(request.PoolId) && 
                    string.IsNullOrEmpty(request.FieldId) && string.IsNullOrEmpty(request.ProjectId))
                {
                    throw new ArgumentException("At least one of WellId, PoolId, FieldId, or ProjectId must be provided");
                }

                _logger?.LogInformation("Starting Economic Analysis for WellId: {WellId}, PoolId: {PoolId}, FieldId: {FieldId}, ProjectId: {ProjectId}",
                    request.WellId, request.PoolId, request.FieldId, request.ProjectId);

                // Step 1: Build cash flows from request or PPDM data
                CashFlow[] cashFlows;
                if (request.PRODUCTION_FORECAST != null && request.PRODUCTION_FORECAST.Count > 0)
                {
                    // Build cash flows from production forecast in request
                    cashFlows = BuildCashFlowsFromProductionForecast(request);
                }
                else
                {
                    // Build cash flows from PPDM data
                    cashFlows = await BuildCashFlowsFromPPDMDataAsync(request);
                }

                if (cashFlows == null || cashFlows.Length == 0)
                {
                    throw new InvalidOperationException("No cash flow data available for economic analysis. Provide PRODUCTION_FORECAST or ensure PPDM data is available.");
                }

                // Step 2: Validate discount rate
                double discountRate = (double)request.DISCOUNT_RATE / 100.0; // Convert percentage to decimal

                if (discountRate < 0 || discountRate > 1)
                {
                    throw new ArgumentException("Discount rate must be between 0 and 100 percent");
                }

                // Step 3: Perform economic analysis
                double financeRate = request.AdditionalParameters?.FinanceRate != null
                    ? request.AdditionalParameters.FinanceRate.Value / 100.0
                    : 0.08;

                double reinvestRate = request.AdditionalParameters?.ReinvestRate != null
                    ? request.AdditionalParameters.ReinvestRate.Value / 100.0
                    : 0.12;

                EconomicResult economicResult;
                try
                {
                    economicResult = EconomicAnalyzer.Analyze(cashFlows, discountRate, financeRate, reinvestRate);
                }
                catch (Exception calcEx)
                {
                    _logger?.LogError(calcEx, "Error in economic calculation");
                    throw new InvalidOperationException($"Economic calculation failed: {calcEx.Message}", calcEx);
                }

                // Step 4: Generate NPV profile if requested
                List<NPV_PROFILE_POINT>? npvProfile = null;
                if (request.AdditionalParameters?.GenerateNpvProfile == true)
                {
                    double minRate = request.AdditionalParameters?.NpvProfileMinRate != null
                        ? request.AdditionalParameters.NpvProfileMinRate.Value / 100.0
                        : 0.0;
                    double maxRate = request.AdditionalParameters?.NpvProfileMaxRate != null
                        ? request.AdditionalParameters.NpvProfileMaxRate.Value / 100.0
                        : 0.5;
                    int points = request.AdditionalParameters?.NpvProfilePoints ?? 50;

                    npvProfile = EconomicAnalyzer.GenerateNPVProfile(cashFlows, minRate, maxRate, points);
                }

                // Step 5: Map EconomicResult to EconomicAnalysisResult DTO
                var result = MapEconomicResultToDTO(economicResult, request, cashFlows, npvProfile);

                // Step 6: Store result in database
                var repository = await GetEconomicResultRepositoryAsync();

                await repository.InsertAsync(result, request.UserId ?? "system");

                _logger?.LogInformation("Economic Analysis calculation completed: {CalculationId}, NPV: {NPV}, IRR: {IRR}%",
                    result.CalculationId, result.NetPresentValue, result.InternalRateOfReturn);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Economic Analysis");

                // Return error result
                var errorResult = new EconomicAnalysisResult
                {
                    CalculationId = Guid.NewGuid().ToString(),
                    WellId = request.WellId,
                    PoolId = request.PoolId,
                    FieldId = request.FieldId,
                    ProjectId = request.ProjectId,
                    AnalysisType = request.AnalysisType,
                    CalculationDate = DateTime.UtcNow,
                    Status = "FAILED",
                    ErrorMessage = ex.Message,
                    UserId = request.UserId,
                    CashFlowPoints = new List<EconomicCashFlowPoint>(),
                    AdditionalResults = new EconomicAnalysisAdditionalResults()
                };

                // Try to store error result
                try
                {
                    var repository = await GetEconomicResultRepositoryAsync();

                    await repository.InsertAsync(errorResult, request.UserId ?? "system");
                }
                catch (Exception storeEx)
                {
                    _logger?.LogError(storeEx, "Error storing Economic Analysis error result");
                }

                throw;
            }
        }

        #region Economic Analysis Helper Methods

        /// <summary>
        /// Builds cash flows from production forecast in request
        /// </summary>
        private CashFlow[] BuildCashFlowsFromProductionForecast(EconomicAnalysisRequest request)
        {
            if (request.PRODUCTION_FORECAST == null || request.PRODUCTION_FORECAST.Count == 0)
            {
                return Array.Empty<CashFlow>();
            }

            var cashFlows = new List<CashFlow>();
            var startDate = request.AnalysisStartDate ?? request.PRODUCTION_FORECAST.First().Date;
            var oilPrice = request.OilPrice ?? 50.0m; // Default $50/bbl
            var gasPrice = request.GasPrice ?? 3.0m; // Default $3/Mscf
            var operatingCostPerUnit = request.OperatingCostPerUnit ?? 10.0m; // Default $10/bbl equivalent
            var royaltyRate = request.RoyaltyRate ?? 0.125m; // Default 12.5%
            var taxRate = request.TaxRate ?? 0.35m; // Default 35%
            var workingInterest = request.WorkingInterest ?? 1.0m; // Default 100%

            // Add initial investment (period 0)
            if (request.CapitalInvestment.HasValue && request.CapitalInvestment.Value != 0)
            {
                cashFlows.Add(new CashFlow
                {
                    Period = 0,
                    Amount = -(double)request.CapitalInvestment.Value,
                    Description = "Initial Capital Investment"
                });
            }

            // Build cash flows from production forecast
            int period = 1;
            foreach (var point in request.PRODUCTION_FORECAST.OrderBy(p => p.Date))
            {
                // Calculate revenue
                decimal revenue = 0;
                if (point.OilVolume.HasValue)
                    revenue += point.OilVolume.Value * oilPrice;
                if (point.GasVolume.HasValue)
                    revenue += point.GasVolume.Value * gasPrice / 1000.0m; // Convert Mscf to Mscf (already in Mscf)

                // Apply working interest
                revenue *= workingInterest;

                // Calculate costs
                decimal operatingCost = 0;
                if (point.OperatingCost.HasValue)
                {
                    operatingCost = point.OperatingCost.Value;
                }
                else
                {
                    // Estimate from volumes
                    decimal totalVolume = (point.OilVolume ?? 0) + (point.GasVolume ?? 0) / 6.0m; // Convert gas to oil equivalent
                    operatingCost = totalVolume * operatingCostPerUnit;
                }

                // Calculate royalties
                decimal royalties = revenue * royaltyRate;

                // Calculate taxes (on net revenue after royalties and costs)
                decimal netRevenue = revenue - royalties - operatingCost;
                decimal taxes = netRevenue > 0 ? netRevenue * taxRate : 0;

                // Net cash flow
                decimal netCashFlow = revenue - royalties - operatingCost - taxes;

                cashFlows.Add(new CashFlow
                {
                    Period = period++,
                    Amount = (double)netCashFlow,
                    Description = $"Period {period - 1} - {point.Date:yyyy-MM-dd}"
                });
            }

            return cashFlows.ToArray();
        }

        /// <summary>
        /// Builds cash flows from PPDM data (well, pool, or field)
        /// </summary>
        private async Task<CashFlow[]> BuildCashFlowsFromPPDMDataAsync(EconomicAnalysisRequest request)
        {
            try
            {
                var cashFlows = new List<CashFlow>();
                var oilPrice = request.OilPrice ?? 50.0m; // Default $50/bbl
                var gasPrice = request.GasPrice ?? 3.0m; // Default $3/Mscf
                var operatingCostPerUnit = request.OperatingCostPerUnit ?? 10.0m; // Default $10/bbl equivalent
                var royaltyRate = request.RoyaltyRate ?? 0.125m; // Default 12.5%
                var taxRate = request.TaxRate ?? 0.35m; // Default 35%
                var workingInterest = request.WorkingInterest ?? 1.0m; // Default 100%

                // Add initial investment (period 0)
                if (request.CapitalInvestment.HasValue && request.CapitalInvestment.Value != 0)
                {
                    cashFlows.Add(new CashFlow
                    {
                        Period = 0,
                        Amount = -(double)request.CapitalInvestment.Value,
                        Description = "Initial Capital Investment"
                    });
                }

                // Retrieve production data from PPDM
                var productionData = await GetProductionDataForEconomicAnalysisAsync(request);
                
                if (productionData.Count == 0)
                {
                    _logger?.LogWarning("No production data found in PPDM for economic analysis. " +
                        "Consider providing PRODUCTION_FORECAST in request.");
                    return cashFlows.ToArray();
                }

                // Group production data by period (monthly or yearly based on request)
                var startDate = request.AnalysisStartDate ?? productionData.Min(p => p.Date);
                var periodMonths = request.AnalysisPeriodYears.HasValue 
                    ? 12 / request.AnalysisPeriodYears.Value 
                    : 1; // Default monthly

                var groupedData = productionData
                    .GroupBy(p => GetPeriodNumber(p.Date, startDate, periodMonths))
                    .OrderBy(g => g.Key)
                    .ToList();

                int period = 1;
                foreach (var group in groupedData)
                {
                    var periodData = group.ToList();
                    var periodDate = periodData.First().Date;

                    // Aggregate production for the period
                    decimal totalOil = periodData.Sum(p => p.OilVolume ?? 0);
                    decimal totalGas = periodData.Sum(p => p.GasVolume ?? 0);

                    // Calculate revenue
                    decimal revenue = (totalOil * oilPrice) + (totalGas * gasPrice / 1000.0m);
                    revenue *= workingInterest;

                    // Calculate costs
                    decimal operatingCost = periodData.Sum(p => p.OperatingCost ?? 0);
                    if (operatingCost == 0)
                    {
                        decimal totalVolume = totalOil + (totalGas / 6.0m); // Convert gas to oil equivalent
                        operatingCost = totalVolume * operatingCostPerUnit;
                    }

                    // Calculate royalties and taxes
                    decimal royalties = revenue * royaltyRate;
                    decimal netRevenue = revenue - royalties - operatingCost;
                    decimal taxes = netRevenue > 0 ? netRevenue * taxRate : 0;

                    // Net cash flow
                    decimal netCashFlow = revenue - royalties - operatingCost - taxes;

                    cashFlows.Add(new CashFlow
                    {
                        Period = period++,
                        Amount = (double)netCashFlow,
                        Description = $"Period {period - 1} - {periodDate:yyyy-MM}"
                    });
                }

                return cashFlows.ToArray();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error building cash flows from PPDM data");
                // Return empty array on error - caller can handle
                return Array.Empty<CashFlow>();
            }
        }

        /// <summary>
        /// Gets production data for economic analysis from PPDM
        /// </summary>
        private async Task<List<EconomicProductionPoint>> GetProductionDataForEconomicAnalysisAsync(EconomicAnalysisRequest request)
        {
            var productionPoints = new List<EconomicProductionPoint>();

            try
            {
                // Use similar approach to GetProductionDataForDCAAsync
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(PDEN_VOL_SUMMARY), _connectionName, "PDEN_VOL_SUMMARY", null);

                var filters = new List<AppFilter>();

                if (!string.IsNullOrEmpty(request.WellId))
                {
                    filters.Add(new AppFilter 
                    { 
                        FieldName = "WELL_ID", 
                        Operator = "=", 
                        FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", request.WellId) 
                    });
                }
                else if (!string.IsNullOrEmpty(request.PoolId))
                {
                    filters.Add(new AppFilter 
                    { 
                        FieldName = "POOL_ID", 
                        Operator = "=", 
                        FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", request.PoolId) 
                    });
                }
                else if (!string.IsNullOrEmpty(request.FieldId))
                {
                    filters.Add(new AppFilter 
                    { 
                        FieldName = "FIELD_ID", 
                        Operator = "=", 
                        FilterValue = _defaults.FormatIdForTable("PDEN_VOL_SUMMARY", request.FieldId) 
                    });
                }

                // Add date filters if provided
                if (request.AnalysisStartDate.HasValue)
                {
                    filters.Add(new AppFilter 
                    { 
                        FieldName = "PRODUCTION_DATE", 
                        Operator = ">=", 
                        FilterValue = request.AnalysisStartDate.Value.ToString("yyyy-MM-dd") 
                    });
                }

                if (request.AnalysisEndDate.HasValue)
                {
                    filters.Add(new AppFilter 
                    { 
                        FieldName = "PRODUCTION_DATE", 
                        Operator = "<=", 
                        FilterValue = request.AnalysisEndDate.Value.ToString("yyyy-MM-dd") 
                    });
                }

                var entities = await repo.GetAsync(filters);
                
                foreach (var entity in entities.OrderBy(e => GetDateValue(e, "PRODUCTION_DATE")))
                {
                    var date = GetDateValue(entity, "PRODUCTION_DATE") ?? DateTime.UtcNow;
                    var oilVol = GetPropertyValueMultiple(entity, "OIL_VOLUME", "OIL_PROD", "OIL_VOL");
                    var gasVol = GetPropertyValueMultiple(entity, "GAS_VOLUME", "GAS_PROD", "GAS_VOL");
                    var waterVol = GetPropertyValueMultiple(entity, "WATER_VOLUME", "WATER_PROD", "WATER_VOL");

                    productionPoints.Add(new EconomicProductionPoint
                    {
                        Date = date,
                        OilVolume = oilVol,
                        GasVolume = gasVol,
                        WaterVolume = waterVol
                    });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving production data for economic analysis");
            }

            return productionPoints;
        }

        /// <summary>
        /// Calculates period number based on date and start date
        /// </summary>
        private int GetPeriodNumber(DateTime date, DateTime startDate, int periodMonths)
        {
            var monthsDiff = ((date.Year - startDate.Year) * 12) + (date.Month - startDate.Month);
            return monthsDiff / periodMonths;
        }

        /// <summary>
        /// Maps EconomicResult from library to EconomicAnalysisResult DTO
        /// </summary>
        private EconomicAnalysisResult MapEconomicResultToDTO(
            EconomicResult economicResult,
            EconomicAnalysisRequest request,
            CashFlow[] cashFlows,
            List<NPV_PROFILE_POINT>? npvProfile)
        {
            var result = new EconomicAnalysisResult
            {
                CalculationId = Guid.NewGuid().ToString(),
                WellId = request.WellId,
                PoolId = request.PoolId,
                FieldId = request.FieldId,
                ProjectId = request.ProjectId,
                AnalysisType = request.AnalysisType,
                CalculationDate = DateTime.UtcNow,
                Status = "SUCCESS",
                UserId = request.UserId,
                NetPresentValue = (decimal)economicResult.NPV,
                InternalRateOfReturn = (decimal)(economicResult.IRR * 100.0), // Convert to percentage
                PaybackPeriod = (decimal)economicResult.PaybackPeriod,
                ReturnOnInvestment = (decimal)economicResult.ROI,
                ProfitabilityIndex = (decimal)economicResult.ProfitabilityIndex,
                CashFlowPoints = new List<EconomicCashFlowPoint>(),
                AdditionalResults = new EconomicAnalysisAdditionalResults()
            };

            // Map cash flows to cash flow points
            decimal cumulativeCashFlow = 0;
            decimal cumulativeDiscountedCashFlow = 0;
            double discountRate = (double)request.DISCOUNT_RATE / 100.0;

            foreach (var cf in cashFlows.OrderBy(c => c.Period))
            {
                decimal discountedCF = (decimal)(cf.Amount / Math.Pow(1 + discountRate, cf.Period));
                cumulativeCashFlow += (decimal)cf.Amount;
                cumulativeDiscountedCashFlow += discountedCF;

                result.CashFlowPoints.Add(new EconomicCashFlowPoint
                {
                    Date = request.AnalysisStartDate?.AddMonths(cf.Period * 12) ?? DateTime.UtcNow.AddMonths(cf.Period * 12),
                    NetCashFlow = (decimal)cf.Amount,
                    CumulativeCashFlow = cumulativeCashFlow,
                    DiscountedCashFlow = discountedCF,
                    CumulativeDiscountedCashFlow = cumulativeDiscountedCashFlow
                });
            }

            // Calculate totals
            var totalRevenue = result.CashFlowPoints.Where(cf => cf.NetCashFlow > 0).Sum(cf => cf.NetCashFlow);
            result.TotalRevenue = totalRevenue;
            var totalOperatingCosts = result.CashFlowPoints.Where(cf => cf.NetCashFlow < 0).Sum(cf => cf.NetCashFlow);
            result.TotalOperatingCosts = totalOperatingCosts;
            result.NetCashFlow = cumulativeCashFlow;
            result.NetPresentValue = cumulativeDiscountedCashFlow;

            // Set additional results
            result.AdditionalResults = new EconomicAnalysisAdditionalResults
            {
                TotalCashFlow = (double)cumulativeCashFlow,
                PresentValue = (double)cumulativeDiscountedCashFlow,
                Mirr = (double?)(economicResult.MIRR * 100.0),
                DiscountedPaybackPeriod = (double?)economicResult.DiscountedPaybackPeriod
            };

            // Add NPV profile to additional results if available
            if (npvProfile != null && npvProfile.Count > 0)
            {
                result.AdditionalResults.NpvProfile = npvProfile;
            }

            // Add MIRR and discounted payback period
            result.AdditionalResults.Mirr = economicResult.MIRR * 100.0;
            result.AdditionalResults.DiscountedPaybackPeriod = economicResult.DiscountedPaybackPeriod;
            result.AdditionalResults.TotalCashFlow = economicResult.TotalCashFlow;
            result.AdditionalResults.PresentValue = economicResult.PresentValue;

            return result;
        }

        #endregion
    }
}
