using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Cost Allocation Service - Allocate overhead and support costs to cost centers
    /// Supports multiple allocation methods (Direct, Step-Down, Reciprocal, ABC)
    /// Critical for accurate product/department costing and profitability analysis
    /// </summary>
    public class CostAllocationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly ILogger<CostAllocationService> _logger;
        private const string ConnectionName = "PPDM39";

        public CostAllocationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<CostAllocationService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Perform cost allocation across cost centers
        /// </summary>
        public async Task<AllocationResult> AllocateCostsAsync(
            List<CostCenter> costCenters,
            List<AllocationBase> allocationBases,
            AllocationMethod method,
            DateTime allocationDate,
            string userId)
        {
            _logger?.LogInformation("Performing cost allocation using {Method}", method);

            try
            {
                if (!costCenters.Any())
                    throw new InvalidOperationException("Must provide at least one cost center");

                var result = new AllocationResult
                {
                    AllocationDate = allocationDate,
                    AllocationMethod = method,
                    PerformedBy = userId,
                    PerformedDate = DateTime.UtcNow,
                    AllocationEntries = new List<AllocationEntry>()
                };

                switch (method)
                {
                    case AllocationMethod.DirectAllocation:
                        await PerformDirectAllocationAsync(costCenters, allocationBases, result);
                        break;

                    case AllocationMethod.StepDown:
                        await PerformStepDownAllocationAsync(costCenters, allocationBases, result);
                        break;

                    case AllocationMethod.Reciprocal:
                        await PerformReciprocalAllocationAsync(costCenters, allocationBases, result);
                        break;

                    case AllocationMethod.ActivityBasedCosting:
                        await PerformActivityBasedAllocationAsync(costCenters, allocationBases, result);
                        break;

                    default:
                        throw new InvalidOperationException($"Unknown allocation method: {method}");
                }

                result.TotalAllocated = result.AllocationEntries.Sum(x => x.AllocationAmount);
                result.AllocationCount = result.AllocationEntries.Count;
                result.Status = "COMPLETED";

                _logger?.LogInformation("Cost allocation completed. Total Allocated: {Total:C}, Entries: {Count}",
                    result.TotalAllocated, result.AllocationCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing cost allocation: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Direct allocation - allocate directly to revenue centers
        /// </summary>
        private async Task PerformDirectAllocationAsync(
            List<CostCenter> costCenters,
            List<AllocationBase> bases,
            AllocationResult result)
        {
            _logger?.LogInformation("Performing direct allocation");

            // Get support cost centers (those with costs to allocate)
            var supportCenters = costCenters.Where(x => x.CostCenterType == "SUPPORT").ToList();
            var revenueCenters = costCenters.Where(x => x.CostCenterType == "REVENUE").ToList();

            foreach (var supportCenter in supportCenters)
            {
                decimal totalBasis = revenueCenters.Sum(x => x.AllocationBasisValue);
                if (totalBasis == 0) continue;

                foreach (var revenueCenter in revenueCenters)
                {
                    decimal allocationPercent = revenueCenter.AllocationBasisValue / totalBasis;
                    decimal allocatedAmount = supportCenter.TotalCost * allocationPercent;

                    result.AllocationEntries.Add(new AllocationEntry
                    {
                        SourceCostCenter = supportCenter.CostCenterId,
                        TargetCostCenter = revenueCenter.CostCenterId,
                        AllocationBasis = supportCenter.AllocationBasisType,
                        AllocationPercent = allocationPercent,
                        AllocationAmount = allocatedAmount,
                        Description = $"Direct allocation from {supportCenter.CostCenterId} to {revenueCenter.CostCenterId}"
                    });
                }
            }
        }

        /// <summary>
        /// Step-down allocation - service departments allocated in sequence
        /// </summary>
        private async Task PerformStepDownAllocationAsync(
            List<CostCenter> costCenters,
            List<AllocationBase> bases,
            AllocationResult result)
        {
            _logger?.LogInformation("Performing step-down allocation");

            var supportCenters = costCenters.Where(x => x.CostCenterType == "SUPPORT").OrderBy(x => x.AllocationSequence).ToList();
            var revenueCenters = costCenters.Where(x => x.CostCenterType == "REVENUE").ToList();
            var allCenters = new List<CostCenter>(costCenters);

            foreach (var supportCenter in supportCenters)
            {
                // Allocate to remaining centers (both revenue and support not yet allocated)
                var remainingCenters = allCenters.Where(x => x.CostCenterId != supportCenter.CostCenterId).ToList();
                decimal totalBasis = remainingCenters.Sum(x => x.AllocationBasisValue);

                if (totalBasis == 0) continue;

                foreach (var targetCenter in remainingCenters)
                {
                    decimal allocationPercent = targetCenter.AllocationBasisValue / totalBasis;
                    decimal allocatedAmount = supportCenter.TotalCost * allocationPercent;

                    result.AllocationEntries.Add(new AllocationEntry
                    {
                        SourceCostCenter = supportCenter.CostCenterId,
                        TargetCostCenter = targetCenter.CostCenterId,
                        AllocationBasis = supportCenter.AllocationBasisType,
                        AllocationPercent = allocationPercent,
                        AllocationAmount = allocatedAmount,
                        Description = $"Step-down allocation from {supportCenter.CostCenterId} to {targetCenter.CostCenterId}"
                    });

                    // Update target cost for next round
                    targetCenter.TotalCost += allocatedAmount;
                }

                // Remove support center from future allocations
                allCenters.Remove(supportCenter);
            }
        }

        /// <summary>
        /// Reciprocal allocation - iterative allocation between support centers
        /// </summary>
        private async Task PerformReciprocalAllocationAsync(
            List<CostCenter> costCenters,
            List<AllocationBase> bases,
            AllocationResult result)
        {
            _logger?.LogInformation("Performing reciprocal allocation");

            var supportCenters = costCenters.Where(x => x.CostCenterType == "SUPPORT").ToList();
            var revenueCenters = costCenters.Where(x => x.CostCenterType == "REVENUE").ToList();

            // Initialize costs
            var costs = supportCenters.ToDictionary(x => x.CostCenterId, x => x.TotalCost);
            int maxIterations = 10;
            decimal tolerance = 0.01m;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                bool converged = true;
                var newCosts = new Dictionary<string, decimal>(costs);

                foreach (var supportCenter in supportCenters)
                {
                    decimal totalBasis = costCenters
                        .Where(x => x.CostCenterId != supportCenter.CostCenterId)
                        .Sum(x => x.AllocationBasisValue);

                    if (totalBasis == 0) continue;

                    foreach (var targetCenter in costCenters.Where(x => x.CostCenterId != supportCenter.CostCenterId))
                    {
                        decimal allocationPercent = targetCenter.AllocationBasisValue / totalBasis;
                        decimal allocatedAmount = costs[supportCenter.CostCenterId] * allocationPercent;

                        if (Math.Abs(allocatedAmount) > tolerance)
                            converged = false;

                        result.AllocationEntries.Add(new AllocationEntry
                        {
                            SourceCostCenter = supportCenter.CostCenterId,
                            TargetCostCenter = targetCenter.CostCenterId,
                            AllocationBasis = supportCenter.AllocationBasisType,
                            AllocationPercent = allocationPercent,
                            AllocationAmount = allocatedAmount,
                            Description = $"Reciprocal allocation iteration {iteration + 1}"
                        });

                        if (targetCenter.CostCenterType == "SUPPORT")
                            newCosts[targetCenter.CostCenterId] += allocatedAmount;
                    }

                    newCosts[supportCenter.CostCenterId] = 0;
                }

                costs = newCosts;

                if (converged)
                {
                    _logger?.LogInformation("Reciprocal allocation converged after {Iterations} iterations", iteration + 1);
                    break;
                }
            }
        }

        /// <summary>
        /// Activity-Based Costing (ABC) - allocate based on actual activities
        /// </summary>
        private async Task PerformActivityBasedAllocationAsync(
            List<CostCenter> costCenters,
            List<AllocationBase> bases,
            AllocationResult result)
        {
            _logger?.LogInformation("Performing activity-based costing allocation");

            var supportCenters = costCenters.Where(x => x.CostCenterType == "SUPPORT").ToList();
            var revenueCenters = costCenters.Where(x => x.CostCenterType == "REVENUE").ToList();

            foreach (var supportCenter in supportCenters)
            {
                // Group costs by activity
                var activities = bases.Where(x => x.CostCenterId == supportCenter.CostCenterId).ToList();
                
                foreach (var activity in activities)
                {
                    decimal costPerActivity = supportCenter.TotalCost * activity.ActivityPercent;
                    decimal totalActivityUnits = revenueCenters.Sum(x => x.ActivityUnits);

                    if (totalActivityUnits == 0) continue;

                    decimal costPerUnit = costPerActivity / totalActivityUnits;

                    foreach (var revenueCenter in revenueCenters)
                    {
                        decimal allocatedAmount = costPerUnit * revenueCenter.ActivityUnits;

                        result.AllocationEntries.Add(new AllocationEntry
                        {
                            SourceCostCenter = supportCenter.CostCenterId,
                            TargetCostCenter = revenueCenter.CostCenterId,
                            AllocationBasis = $"Activity: {activity.ActivityName}",
                            AllocationPercent = (revenueCenter.ActivityUnits / totalActivityUnits),
                            AllocationAmount = allocatedAmount,
                            Description = $"ABC allocation for {activity.ActivityName}"
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Generate departmental profitability report
        /// </summary>
        public async Task<DepartmentalProfitabilityReport> GenerateDepartmentalProfitabilityAsync(
            DateTime periodStart,
            DateTime periodEnd,
            List<CostCenter> costCenters)
        {
            _logger?.LogInformation("Generating departmental profitability report");

            try
            {
                var report = new DepartmentalProfitabilityReport
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    GeneratedDate = DateTime.UtcNow,
                    Departments = new List<DepartmentProfitability>()
                };

                foreach (var costCenter in costCenters.Where(x => x.CostCenterType == "REVENUE"))
                {
                    var dept = new DepartmentProfitability
                    {
                        DepartmentId = costCenter.CostCenterId,
                        DepartmentName = costCenter.CostCenterName,
                        Revenue = costCenter.Revenue,
                        DirectCosts = costCenter.DirectCosts,
                        AllocatedOverhead = costCenter.AllocatedCosts,
                        Contribution = costCenter.Revenue - costCenter.DirectCosts,
                        Profit = costCenter.Revenue - costCenter.DirectCosts - costCenter.AllocatedCosts
                    };

                    dept.ContributionMargin = dept.Revenue > 0 ? (dept.Contribution / dept.Revenue) * 100 : 0;
                    dept.ProfitMargin = dept.Revenue > 0 ? (dept.Profit / dept.Revenue) * 100 : 0;
                    dept.ROI = costCenter.TotalCost > 0 ? (dept.Profit / costCenter.TotalCost) * 100 : 0;

                    report.Departments.Add(dept);
                }

                report.TotalRevenue = report.Departments.Sum(x => x.Revenue);
                report.TotalDirectCosts = report.Departments.Sum(x => x.DirectCosts);
                report.TotalAllocatedOverhead = report.Departments.Sum(x => x.AllocatedOverhead);
                report.TotalProfit = report.Departments.Sum(x => x.Profit);

                _logger?.LogInformation("Departmental profitability report generated. Total Profit: {Profit:C}",
                    report.TotalProfit);

                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating departmental profitability: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export cost allocation as formatted text
        /// </summary>
        public string ExportAllocationResultAsText(AllocationResult result)
        {
            _logger?.LogInformation("Exporting allocation result as text");

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine("                         COST ALLOCATION REPORT");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine($"Allocation Date:     {result.AllocationDate:MMMM dd, yyyy}");
                sb.AppendLine($"Allocation Method:   {result.AllocationMethod}");
                sb.AppendLine($"Performed By:        {result.PerformedBy}");
                sb.AppendLine($"Status:              {result.Status}");
                sb.AppendLine();

                sb.AppendLine("ALLOCATION ENTRIES:");
                sb.AppendLine("Source         | Target         | Basis          | Amount       | %");
                sb.AppendLine("─────────────────────────────────────────────────────────────────");

                foreach (var entry in result.AllocationEntries.OrderBy(x => x.SourceCostCenter))
                {
                    sb.AppendLine($"{entry.SourceCostCenter,-14} | {entry.TargetCostCenter,-14} | {entry.AllocationBasis,-14} | ${entry.AllocationAmount,11:N2} | {(entry.AllocationPercent * 100),6:N1}%");
                }

                sb.AppendLine("═════════════════════════════════════════════════════════════════════");
                sb.AppendLine($"  {"TOTAL ALLOCATED",-38} ${result.TotalAllocated,15:N2}");
                sb.AppendLine($"  {"ALLOCATION ENTRIES",-38} {result.AllocationCount,15:D}");
                sb.AppendLine("═════════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine($"Report Generated: {result.PerformedDate:MMMM dd, yyyy hh:mm:ss tt}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting allocation result: {Message}", ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Cost Center
    /// </summary>
    public class CostCenter
    {
        public string CostCenterId { get; set; }
        public string CostCenterName { get; set; }
        public string CostCenterType { get; set; }  // REVENUE, SUPPORT
        public string AllocationBasisType { get; set; }
        public decimal AllocationBasisValue { get; set; }
        public decimal ActivityUnits { get; set; }
        public decimal TotalCost { get; set; }
        public decimal DirectCosts { get; set; }
        public decimal Revenue { get; set; }
        public decimal AllocatedCosts { get; set; }
        public int AllocationSequence { get; set; }
    }

    /// <summary>
    /// Allocation Base
    /// </summary>
    public class AllocationBase
    {
        public string CostCenterId { get; set; }
        public string ActivityName { get; set; }
        public decimal ActivityPercent { get; set; }
        public decimal ActivityUnits { get; set; }
    }

    /// <summary>
    /// Allocation Result
    /// </summary>
    public class AllocationResult
    {
        public DateTime AllocationDate { get; set; }
        public AllocationMethod AllocationMethod { get; set; }
        public string PerformedBy { get; set; }
        public DateTime PerformedDate { get; set; }
        public List<AllocationEntry> AllocationEntries { get; set; }
        public decimal TotalAllocated { get; set; }
        public int AllocationCount { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Allocation Entry
    /// </summary>
    public class AllocationEntry
    {
        public string SourceCostCenter { get; set; }
        public string TargetCostCenter { get; set; }
        public string AllocationBasis { get; set; }
        public decimal AllocationPercent { get; set; }
        public decimal AllocationAmount { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Departmental Profitability
    /// </summary>
    public class DepartmentProfitability
    {
        public string DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public decimal Revenue { get; set; }
        public decimal DirectCosts { get; set; }
        public decimal AllocatedOverhead { get; set; }
        public decimal Contribution { get; set; }
        public decimal Profit { get; set; }
        public decimal ContributionMargin { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal ROI { get; set; }
    }

    /// <summary>
    /// Departmental Profitability Report
    /// </summary>
    public class DepartmentalProfitabilityReport
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime GeneratedDate { get; set; }
        public List<DepartmentProfitability> Departments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalDirectCosts { get; set; }
        public decimal TotalAllocatedOverhead { get; set; }
        public decimal TotalProfit { get; set; }
    }

    /// <summary>
    /// Allocation Method
    /// </summary>
    public enum AllocationMethod
    {
        DirectAllocation,
        StepDown,
        Reciprocal,
        ActivityBasedCosting
    }
}
