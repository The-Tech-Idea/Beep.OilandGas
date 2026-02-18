using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Accounting.Models; // Assuming models exist

namespace Beep.OilandGas.Accounting.Services
{
    public class RatioAnalysisService
    {
        public RatioAnalysisReport CalculateRatios(FinancialDataInputs inputs)
        {
            var report = new RatioAnalysisReport();

            // 1. Liquidity Ratios
            if (inputs.CurrentLiabilities != 0)
            {
                report.CurrentRatio = inputs.CurrentAssets / inputs.CurrentLiabilities;
                report.QuickRatio = (inputs.CurrentAssets - inputs.Inventory) / inputs.CurrentLiabilities;
            }

            // 2. Profitability Ratios
            if (inputs.Revenue != 0)
            {
                report.NetProfitMargin = inputs.NetIncome / inputs.Revenue;
                report.GrossProfitMargin = inputs.GrossProfit / inputs.Revenue;
            }
            
            if (inputs.TotalAssets != 0)
            {
                report.ReturnOnAssets = inputs.NetIncome / inputs.TotalAssets;
            }

            if (inputs.TotalEquity != 0)
            {
                report.ReturnOnEquity = inputs.NetIncome / inputs.TotalEquity;
            }

            // 3. Solvency Ratios
            if (inputs.TotalEquity != 0)
            {
                report.DebtToEquity = inputs.TotalDebt / inputs.TotalEquity;
            }

            return report;
        }
    }

    public class FinancialDataInputs
    {
        public decimal CurrentAssets { get; set; }
        public decimal CurrentLiabilities { get; set; }
        public decimal Inventory { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetIncome { get; set; }
    }

    public class RatioAnalysisReport
    {
        public decimal CurrentRatio { get; set; }
        public decimal QuickRatio { get; set; }
        public decimal NetProfitMargin { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public decimal ReturnOnAssets { get; set; }
        public decimal ReturnOnEquity { get; set; }
        public decimal DebtToEquity { get; set; }
    }
}
