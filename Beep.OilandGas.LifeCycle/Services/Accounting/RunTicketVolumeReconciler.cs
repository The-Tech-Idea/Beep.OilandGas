using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.LifeCycle.Services.Accounting;

/// <summary>Compares measured oil run-ticket volumes with their active allocation results.</summary>
public static class RunTicketVolumeReconciler
{
    public static VolumeReconciliationResult Reconcile(
        IReadOnlyList<RUN_TICKET> tickets,
        IReadOnlyDictionary<string, List<ALLOCATION_RESULT>> allocations)
    {
        var result = new VolumeReconciliationResult();
        void Issue(string type, string message) => result.Issues.Add(new VolumeReconciliationIssue
            { IssueType = type, Description = message, Severity = "Error" });

        if (tickets.Count == 0) Issue("MissingData", "No active run tickets exist for the requested field and dates.");
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var ticket in tickets)
        {
            var id = ticket.RUN_TICKET_ID;
            if (string.IsNullOrWhiteSpace(id) || !ids.Add(id))
            {
                Issue("AmbiguousTicket", "Run tickets must have unique, nonempty IDs.");
                continue;
            }
            // AllocationService uses the ticket's net volume as input. This endpoint currently
            // supports its oil/barrel workflow; it must not sum gas, rates or unknown units.
            if (!string.Equals(ticket.VOLUME_OUOM, "BBL", StringComparison.OrdinalIgnoreCase) ||
                !ticket.NET_VOLUME.HasValue || ticket.NET_VOLUME < 0)
            {
                Issue("InvalidMeasurement", $"Ticket {id} requires a nonnegative measured net volume in BBL.");
                continue;
            }
            if (!allocations.TryGetValue(id, out var matches) || matches.Count != 1 ||
                matches[0].ALLOCATION_REQUEST_ID != id || !matches[0].ALLOCATED_VOLUME.HasValue ||
                matches[0].ALLOCATED_VOLUME < 0)
            {
                Issue("InvalidAllocation", $"Ticket {id} requires exactly one active allocation with a nonnegative volume.");
                continue;
            }
            var allocated = matches[0].ALLOCATED_VOLUME!.Value;
            result.FieldProductionVolume += ticket.NET_VOLUME.Value;
            result.ALLOCATED_VOLUME += allocated;
            if (ticket.NET_VOLUME.Value != allocated)
                result.Issues.Add(new VolumeReconciliationIssue { IssueType = "VolumeVariance",
                    Description = $"Ticket {id} measured and allocated volumes differ.", Severity = "Warning" });
        }
        if (result.Issues.Any(issue => issue.Severity == "Error"))
        {
            // Do not expose partial totals as a complete reconciliation.
            result.FieldProductionVolume = result.ALLOCATED_VOLUME = 0;
            result.Status = ReconciliationStatus.Error;
            return result;
        }
        result.Discrepancy = result.FieldProductionVolume - result.ALLOCATED_VOLUME;
        result.DiscrepancyPercentage = result.FieldProductionVolume == 0 ? null :
            result.Discrepancy / result.FieldProductionVolume * 100m;
        result.Status = result.Issues.Count == 0 ? ReconciliationStatus.Matched : ReconciliationStatus.VarianceDetected;
        if (result.FieldProductionVolume == 0 && result.ALLOCATED_VOLUME != 0)
            result.Issues.Add(new VolumeReconciliationIssue { IssueType = "UndefinedPercentage",
                Description = "A discrepancy percentage is undefined when measured volume is zero.", Severity = "Warning" });
        result.OilVolume = new VolumeBreakdownResult { FieldVolume = result.FieldProductionVolume,
            ALLOCATED_VOLUME = result.ALLOCATED_VOLUME, Discrepancy = result.Discrepancy };
        return result;
    }
}
