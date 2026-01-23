using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting;
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
    public class CostAllocationService : ICostAllocationService
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
        public async Task<CostAllocationResult> AllocateCostsAsync(
            List<CostCenter> costCenters,
            List<AllocationBase> allocationBases,
            CostAllocationMethod method,
            DateTime allocationDate,
            string userId)
        {
            _logger?.LogInformation("Performing cost allocation using {Method}", method);

            try
            {
                if (!costCenters.Any())
                    throw new InvalidOperationException("Must provide at least one cost center");

                var result = new CostAllocationResult
                {
                    AllocationDate = allocationDate,
                    AllocationMethod = method,
                    PerformedBy = userId,
                    PerformedDate = DateTime.UtcNow,
                    AllocationEntries = new List<AllocationEntry>()
                };

                switch (method)
                {
                    case CostAllocationMethod.DirectAllocation:
                        await PerformDirectAllocationAsync(costCenters, allocationBases, result);
                        break;

                    case CostAllocationMethod.StepDown:
                        await PerformStepDownAllocationAsync(costCenters, allocationBases, result);
                        break;

                    case CostAllocationMethod.Reciprocal:
                        await PerformReciprocalAllocationAsync(costCenters, allocationBases, result);
                        break;

                    case CostAllocationMethod.ActivityBasedCosting:
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
            CostAllocationResult result)
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
            CostAllocationResult result)
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
            CostAllocationResult result)
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
        /// Allocates a single accounting cost across cost centers and persists COST_ALLOCATION records.
        /// </summary>
        public async Task<List<COST_ALLOCATION>> AllocateCostAsync(
            ACCOUNTING_COST cost,
            List<COST_ALLOCATION> allocations,
            string userId,
            string cn = "PPDM39")
        {
            if (cost == null)
                throw new ArgumentNullException(nameof(cost));
            if (allocations == null || allocations.Count == 0)
                throw new InvalidOperationException("Allocations are required");
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));
            if (cost.AMOUNT <= 0)
                throw new InvalidOperationException("Cost amount must be positive");

            var totalPercentage = allocations.Sum(a => a.ALLOCATION_PERCENTAGE ?? 0m);
            if (Math.Abs(totalPercentage - 100m) > 0.01m)
                throw new InvalidOperationException($"Allocation percentages must sum to 100 (got {totalPercentage})");

            var metadata = await _metadata.GetTableMetadataAsync("COST_ALLOCATION");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(COST_ALLOCATION);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "COST_ALLOCATION");

            var results = new List<COST_ALLOCATION>();
            foreach (var allocation in allocations)
            {
                allocation.COST_ALLOCATION_ID ??= Guid.NewGuid().ToString();
                allocation.COST_TRANSACTION_ID ??= cost.ACCOUNTING_COST_ID;
                allocation.ALLOCATED_AMOUNT = cost.AMOUNT * ((allocation.ALLOCATION_PERCENTAGE ?? 0m) / 100m);
                allocation.ALLOCATION_METHOD ??= "VOLUME_BASED";
                allocation.ACTIVE_IND = _defaults.GetActiveIndicatorYes();
                allocation.PPDM_GUID ??= Guid.NewGuid().ToString();
                allocation.ROW_CREATED_BY = userId;
                allocation.ROW_CREATED_DATE = DateTime.UtcNow;

                await repo.InsertAsync(allocation, userId);
                results.Add(allocation);

                await UpdateCostCenterTotalsAsync(allocation.COST_CENTER_ID, allocation.ALLOCATED_AMOUNT ?? 0m, userId, cn);
            }

            return results;
        }

        private async Task UpdateCostCenterTotalsAsync(string costCenterId, decimal amount, string userId, string cn)
        {
            if (string.IsNullOrWhiteSpace(costCenterId))
                return;

            var metadata = await _metadata.GetTableMetadataAsync("COST_CENTER");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(COST_CENTER);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "COST_CENTER");

            var result = await repo.GetByIdAsync(costCenterId);
            var center = result as COST_CENTER;
            if (center == null)
                return;

            center.TOTAL_CAPITALIZED_COSTS = (center.TOTAL_CAPITALIZED_COSTS ?? 0m) + amount;
            center.ROW_CHANGED_BY = userId;
            center.ROW_CHANGED_DATE = DateTime.UtcNow;

            await repo.UpdateAsync(center, userId);
        }

        /// <summary>
        /// Activity-Based Costing (ABC) - allocate based on actual activities
        /// </summary>
        private async Task PerformActivityBasedAllocationAsync(
            List<CostCenter> costCenters,
            List<AllocationBase> bases,
            CostAllocationResult result)
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
        public string ExportAllocationResultAsText(CostAllocationResult result)
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
}
