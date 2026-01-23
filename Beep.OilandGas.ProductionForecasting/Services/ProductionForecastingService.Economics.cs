using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.ProductionForecasting.Services
{
    public partial class ProductionForecastingService
    {
        public async Task<EconomicAnalysis> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ForecastId)) throw new ArgumentNullException(nameof(request.ForecastId));

            var forecast = await GetForecastAsync(request.ForecastId);
            if (forecast == null)
            {
                var stub = new EconomicAnalysis
                {
                    AnalysisId = _defaults.FormatIdForTable("ECON_ANALYSIS", Guid.NewGuid().ToString()),
                    NPV = 0,
                    IRR = 0,
                    DiscountRate = request.DiscountRate,
                    CashFlows = new List<CashFlowPoint>()
                };
                return stub;
            }

            var capSchedule = request.CapitalSchedule?.Select(c => (c.Date, c.Amount)).ToList();
            var cashFlows = BuildCashFlowsFromForecast(forecast, request.OilPrice.GetValueOrDefault(), request.GasPrice.GetValueOrDefault(), request.OperatingCostPerBarrel.GetValueOrDefault(10m), request.FixedOpexPerPeriod.GetValueOrDefault(), capSchedule);
            var npv = ComputeNPV(cashFlows, (decimal)request.DiscountRate);
            var irr = ComputeIRR(cashFlows);

            decimal cumulative = 0m;
            for (int i = 0; i < cashFlows.Count; i++)
            {
                cumulative += cashFlows[i].NetCashFlow;
                cashFlows[i].CumulativeCashFlow = cumulative;
            }

            var result = new EconomicAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("ECON_ANALYSIS", Guid.NewGuid().ToString()),
                NPV = Math.Round(npv, 2),
                IRR = Math.Round(irr * 100m, 4),
                DiscountRate = request.DiscountRate,
                CashFlows = cashFlows
            };

            await Task.CompletedTask;
            return result;
        }

        
        private List<CashFlowPoint> BuildCashFlowsFromForecast(
            ProductionForecastResult forecast,
            decimal oilPrice,
            decimal gasPrice,
            decimal operatingCostPerBarrel = 10m,
            decimal fixedOpexPerPeriod = 0m,
            List<(DateTime Date, decimal Amount)>? capitalSchedule = null)
        {
            var list = new List<CashFlowPoint>();
            if (forecast?.ForecastPoints == null) return list;

            foreach (var p in forecast.ForecastPoints)
            {
                var oilRate = p.OilRate.GetValueOrDefault(0m);
                var gasRate = p.GasRate.GetValueOrDefault(0m);

                // Assume rates are per day; convert to monthly volume (30 days)
                var oilVol = oilRate * 30m;
                var gasVol = gasRate * 30m;

                var revenue = oilVol * oilPrice + gasVol * gasPrice;

                // Operating costs: variable by oil volume + fixed OPEX
                var variableOpex = oilVol * operatingCostPerBarrel;
                var operatingCosts = Math.Round(variableOpex + fixedOpexPerPeriod, 2);

                // Capital costs: check schedule for matching month
                decimal capitalCosts = 0m;
                if (capitalSchedule != null && capitalSchedule.Count > 0)
                {
                    var cap = capitalSchedule.FirstOrDefault(cs => cs.Date.Year == p.Date.Year && cs.Date.Month == p.Date.Month);
                    capitalCosts = cap == default ? 0m : cap.Amount;
                }

                var net = revenue - operatingCosts - capitalCosts;

                var cash = new CashFlowPoint
                {
                    Date = p.Date,
                    Revenue = Math.Round(revenue, 2),
                    OperatingCosts = operatingCosts,
                    CapitalCosts = Math.Round(capitalCosts, 2),
                    NetCashFlow = Math.Round(net, 2),
                    CumulativeCashFlow = 0m
                };
                list.Add(cash);
            }

            return list;
        }

        private decimal ComputeNPV(List<CashFlowPoint> cashFlows, decimal annualRate)
        {
            if (cashFlows == null || cashFlows.Count == 0) return 0m;
            var start = cashFlows.First().Date;
            double r = (double)annualRate;
            decimal sum = 0m;
            for (int i = 0; i < cashFlows.Count; i++)
            {
                var months = (cashFlows[i].Date - start).TotalDays / 30.0;
                var df = Math.Pow(1.0 + r, months / 12.0);
                if (df == 0) continue;
                sum += cashFlows[i].NetCashFlow / (decimal)df;
            }
            return sum;
        }

        private decimal ComputeIRR(List<CashFlowPoint> cashFlows)
        {
            if (cashFlows == null || cashFlows.Count == 0) return 0m;

            var cf = cashFlows.Select(x => (double)x.NetCashFlow).ToArray();
            if (cf.All(v => v >= 0) || cf.All(v => v <= 0)) return 0m;

            double low = -0.9999, high = 10.0;
            double npvLow = Npv(cf, low), npvHigh = Npv(cf, high);
            if (double.IsNaN(npvLow) || double.IsNaN(npvHigh)) return 0m;
            if (Math.Sign(npvLow) == Math.Sign(npvHigh)) return 0m;

            for (int iter = 0; iter < 100; iter++)
            {
                double mid = (low + high) / 2.0;
                double npvMid = Npv(cf, mid);
                if (Math.Abs(npvMid) < 1e-6) return (decimal)mid;
                if (Math.Sign(npvMid) == Math.Sign(npvLow))
                {
                    low = mid; npvLow = npvMid;
                }
                else
                {
                    high = mid; npvHigh = npvMid;
                }
            }

            return (decimal)((low + high) / 2.0);
        }

        private double Npv(double[] cashFlows, double rate)
        {
            double sum = 0.0;
            for (int i = 0; i < cashFlows.Length; i++)
            {
                sum += cashFlows[i] / Math.Pow(1.0 + rate, i + 1);
            }
            return sum;
        }
    }
}
