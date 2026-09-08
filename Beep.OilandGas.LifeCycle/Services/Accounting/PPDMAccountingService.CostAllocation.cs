using System.Globalization;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using Microsoft.Extensions.Configuration;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.Accounting;

public partial class PPDMAccountingService
{
    public async Task<CostAllocationComputationResult> AllocateCostsAsync(string fieldId, DateTime startDate,
        DateTime endDate, CostAllocationMethod allocationMethod, string connectionName = "PPDM39")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
        if (fieldId.Contains(':'))
            throw new ArgumentException("Field IDs cannot contain the configuration path separator.", nameof(fieldId));
        if (startDate.Date > endDate.Date || endDate.Date == DateTime.MaxValue.Date)
            throw new ArgumentException("A valid inclusive allocation date range is required.");
        if (!Enum.IsDefined(allocationMethod))
            throw new ArgumentException("Unknown allocation method.", nameof(allocationMethod));
        if (_configuration == null || _costAllocationService == null)
            throw new InvalidOperationException("Field cost allocation services are not configured.");

        var rules = _configuration.GetSection($"CostAllocation:Fields:{fieldId}");
        var configured = rules.GetSection("Centers").Get<List<CostCenter>>() ?? new();
        var bases = rules.GetSection("ActivityBases").Get<List<AllocationBase>>() ?? new();
        if (configured.Count == 0)
            throw new InvalidOperationException($"No cost allocation rules are configured for field {fieldId}.");

        var connection = connectionName ?? _connectionName;
        var centersRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(COST_CENTER), connection, "COST_CENTER");
        var centers = (await centersRepo.GetAsync(new()
        {
            new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
        })).OfType<COST_CENTER>().ToDictionary(c => c.COST_CENTER_ID, StringComparer.OrdinalIgnoreCase);
        if (configured.Any(c => string.IsNullOrWhiteSpace(c.CostCenterId) || !centers.ContainsKey(c.CostCenterId)) ||
            configured.Select(c => c.CostCenterId).Distinct(StringComparer.OrdinalIgnoreCase).Count() != configured.Count)
            throw new InvalidOperationException("Allocation rules must reference distinct active cost centers in the selected field.");
        if (configured.Any(c => c.CostCenterType is not ("SUPPORT" or "REVENUE") || c.AllocationBasisValue < 0 || c.ActivityUnits < 0) ||
            !configured.Any(c => c.CostCenterType == "REVENUE"))
            throw new InvalidOperationException("Allocation rules require valid SUPPORT/REVENUE types, nonnegative bases, and revenue targets.");

        var costRepo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(COST_TRANSACTION), connection, "COST_TRANSACTION");
        var costs = (await costRepo.GetAsync(new()
        {
            new() { FieldName = "FIELD_ID", Operator = "=", FilterValue = fieldId },
            new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" },
            new() { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = startDate.Date.ToString("O", CultureInfo.InvariantCulture) },
            new() { FieldName = "TRANSACTION_DATE", Operator = "<", FilterValue = endDate.Date.AddDays(1).ToString("O", CultureInfo.InvariantCulture) }
        })).OfType<COST_TRANSACTION>().ToList();
        var ids = configured.Select(c => c.CostCenterId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (allocationMethod == CostAllocationMethod.ActivityBasedCosting &&
            (bases.Any(b => !configured.Any(c => c.CostCenterType == "SUPPORT" && c.CostCenterId == b.CostCenterId) ||
                b.ActivityPercent < 0 || b.ActivityPercent > 1) ||
             configured.Where(c => c.CostCenterType == "SUPPORT").Any(c =>
                bases.Where(b => b.CostCenterId == c.CostCenterId).Sum(b => b.ActivityPercent) != 1m)))
            throw new InvalidOperationException("Each support center requires activity fractions between zero and one totaling one.");
        if (costs.Any(c => !c.AMOUNT.HasValue || !ids.Contains(c.COST_CENTER_ID) ||
            (c.IS_CAPITALIZED == "Y") == (c.IS_EXPENSED == "Y")))
            throw new InvalidOperationException("Every cost requires an amount, a configured cost center, and exactly one capital/expense classification.");

        var operating = await CalculateAsync(costs.Where(c => c.IS_EXPENSED == "Y").ToList());
        var capital = await CalculateAsync(costs.Where(c => c.IS_CAPITALIZED == "Y").ToList());
        var total = costs.Sum(c => c.AMOUNT!.Value);
        return new CostAllocationComputationResult
        {
            TotalOperatingCosts = operating.Values.Sum(),
            TotalCapitalCosts = capital.Values.Sum(),
            AllocationDetails = configured.Where(c => c.CostCenterType == "REVENUE").Select(c => new CostAllocationBreakdown
            {
                EntityType = "COST_CENTER", EntityName = centers[c.CostCenterId].COST_CENTER_NAME,
                AllocatedOperatingCost = operating[c.CostCenterId], AllocatedCapitalCost = capital[c.CostCenterId],
                TotalAllocatedCost = operating[c.CostCenterId] + capital[c.CostCenterId],
                AllocationPercentage = total == 0 ? 0 : (operating[c.CostCenterId] + capital[c.CostCenterId]) / total * 100
            }).ToList()
        };

        async Task<Dictionary<string, decimal>> CalculateAsync(List<COST_TRANSACTION> category)
        {
            // The engine mutates some methods' working balances; each category needs fresh inputs.
            var input = configured.Select(c => new CostCenter
            {
                CostCenterId = c.CostCenterId, CostCenterName = c.CostCenterName, CostCenterType = c.CostCenterType,
                AllocationBasisType = c.AllocationBasisType, AllocationBasisValue = c.AllocationBasisValue,
                ActivityUnits = c.ActivityUnits, AllocationSequence = c.AllocationSequence,
                TotalCost = category.Where(t => string.Equals(t.COST_CENTER_ID, c.CostCenterId, StringComparison.OrdinalIgnoreCase)).Sum(t => t.AMOUNT!.Value)
            }).ToList();
            var totals = input.Where(c => c.CostCenterType == "REVENUE")
                .ToDictionary(c => c.CostCenterId, c => c.TotalCost, StringComparer.OrdinalIgnoreCase);
            var allocation = await _costAllocationService.AllocateCostsAsync(input, bases, allocationMethod, endDate.Date, "CALCULATION");
            foreach (var entry in allocation.AllocationEntries.Where(e => totals.ContainsKey(e.TargetCostCenter)))
                totals[entry.TargetCostCenter] += entry.AllocationAmount;
            if (Math.Abs(totals.Values.Sum() - category.Sum(c => c.AMOUNT!.Value)) > 0.01m)
                throw new InvalidOperationException("Configured allocation rules did not fully reconcile the source costs.");
            return totals;
        }
    }
}
