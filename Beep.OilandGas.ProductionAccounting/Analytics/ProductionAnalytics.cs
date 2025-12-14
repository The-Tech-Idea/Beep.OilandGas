using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.ProductionAccounting.Calculations;

namespace Beep.OilandGas.ProductionAccounting.Analytics
{
    /// <summary>
    /// Provides production analytics and insights.
    /// </summary>
    public class ProductionAnalytics
    {
        /// <summary>
        /// Analyzes production trends.
        /// </summary>
        public static ProductionTrendAnalysis AnalyzeProductionTrend(
            List<RunTicket> runTickets,
            DateTime startDate,
            DateTime endDate)
        {
            if (runTickets == null || runTickets.Count == 0)
                throw new ArgumentException("Run tickets list cannot be null or empty.", nameof(runTickets));

            var periodTickets = runTickets
                .Where(t => t.TicketDateTime >= startDate && t.TicketDateTime <= endDate)
                .OrderBy(t => t.TicketDateTime)
                .ToList();

            if (periodTickets.Count == 0)
                throw new ArgumentException("No tickets found in the specified period.", nameof(startDate));

            var analysis = new ProductionTrendAnalysis
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalProduction = periodTickets.Sum(t => t.NetVolume),
                AverageDailyProduction = periodTickets.Sum(t => t.NetVolume) / (endDate - startDate).Days,
                PeakProduction = periodTickets.Max(t => t.NetVolume),
                MinimumProduction = periodTickets.Min(t => t.NetVolume),
                ProductionDays = periodTickets.Select(t => t.TicketDateTime.Date).Distinct().Count()
            };

            // Calculate decline rate if enough data
            if (periodTickets.Count >= 2)
            {
                var firstMonth = periodTickets.Take(periodTickets.Count / 2).Sum(t => t.NetVolume);
                var secondMonth = periodTickets.Skip(periodTickets.Count / 2).Sum(t => t.NetVolume);
                
                if (firstMonth > 0)
                {
                    analysis.DeclineRate = ProductionCalculations.CalculateProductionDeclinePercentage(
                        firstMonth, secondMonth);
                }
            }

            return analysis;
        }

        /// <summary>
        /// Analyzes profitability.
        /// </summary>
        public static ProfitabilityAnalysis AnalyzeProfitability(
            List<SalesTransaction> transactions,
            DateTime startDate,
            DateTime endDate)
        {
            if (transactions == null || transactions.Count == 0)
                throw new ArgumentException("Transactions list cannot be null or empty.", nameof(transactions));

            var periodTransactions = transactions
                .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToList();

            if (periodTransactions.Count == 0)
                throw new ArgumentException("No transactions found in the specified period.", nameof(startDate));

            decimal totalRevenue = periodTransactions.Sum(t => t.TotalValue);
            decimal totalCosts = periodTransactions.Sum(t => t.Costs.TotalCosts + t.Taxes.Sum(tax => tax.Amount));
            decimal totalVolume = periodTransactions.Sum(t => t.NetVolume);

            var analysis = new ProfitabilityAnalysis
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                TotalCosts = totalCosts,
                NetProfit = totalRevenue - totalCosts,
                ProfitMargin = ProductionCalculations.CalculateProfitMargin(totalRevenue, totalCosts),
                RevenuePerBarrel = totalVolume > 0 ? totalRevenue / totalVolume : 0,
                CostPerBarrel = totalVolume > 0 ? totalCosts / totalVolume : 0,
                BreakEvenPrice = ProductionCalculations.CalculateBreakEvenPrice(totalCosts, totalVolume)
            };

            return analysis;
        }

        /// <summary>
        /// Analyzes allocation efficiency.
        /// </summary>
        public static AllocationEfficiencyAnalysis AnalyzeAllocationEfficiency(
            List<Allocation.AllocationResult> allocations)
        {
            if (allocations == null || allocations.Count == 0)
                throw new ArgumentException("Allocations list cannot be null or empty.", nameof(allocations));

            var analysis = new AllocationEfficiencyAnalysis
            {
                TotalAllocations = allocations.Count,
                TotalAllocatedVolume = allocations.Sum(a => a.TotalVolume),
                AverageAllocationVariance = allocations.Average(a => Math.Abs(a.AllocationVariance)),
                AllocationMethodsUsed = allocations.Select(a => a.Method).Distinct().ToList()
            };

            // Calculate efficiency (lower variance = higher efficiency)
            analysis.EfficiencyScore = analysis.AverageAllocationVariance > 0
                ? Math.Max(0, 100 - (analysis.AverageAllocationVariance / analysis.TotalAllocatedVolume * 100))
                : 100;

            return analysis;
        }
    }

    /// <summary>
    /// Represents production trend analysis results.
    /// </summary>
    public class ProductionTrendAnalysis
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalProduction { get; set; }
        public decimal AverageDailyProduction { get; set; }
        public decimal PeakProduction { get; set; }
        public decimal MinimumProduction { get; set; }
        public int ProductionDays { get; set; }
        public decimal DeclineRate { get; set; }
    }

    /// <summary>
    /// Represents profitability analysis results.
    /// </summary>
    public class ProfitabilityAnalysis
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal RevenuePerBarrel { get; set; }
        public decimal CostPerBarrel { get; set; }
        public decimal BreakEvenPrice { get; set; }
    }

    /// <summary>
    /// Represents allocation efficiency analysis results.
    /// </summary>
    public class AllocationEfficiencyAnalysis
    {
        public int TotalAllocations { get; set; }
        public decimal TotalAllocatedVolume { get; set; }
        public decimal AverageAllocationVariance { get; set; }
        public List<Allocation.AllocationMethod> AllocationMethodsUsed { get; set; } = new();
        public decimal EfficiencyScore { get; set; }
    }
}

