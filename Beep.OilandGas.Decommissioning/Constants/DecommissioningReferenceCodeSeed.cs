using System.Collections.Generic;

namespace Beep.OilandGas.Decommissioning.Constants;

public static class DecommissioningReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        yield return new(DecommissioningReferenceSets.AbandonmentStatus, DecommissioningReferenceDefaults.Planned, "Planned");
        yield return new(DecommissioningReferenceSets.AbandonmentStatus, DecommissioningReferenceDefaults.InProgress, "In Progress");
        yield return new(DecommissioningReferenceSets.AbandonmentStatus, DecommissioningReferenceDefaults.Completed, "Completed");
        yield return new(DecommissioningReferenceSets.AbandonmentStatus, DecommissioningReferenceDefaults.Cancelled, "Cancelled");

        yield return new(DecommissioningReferenceSets.DecommissioningStatus, DecommissioningReferenceDefaults.Planned, "Planned");
        yield return new(DecommissioningReferenceSets.DecommissioningStatus, DecommissioningReferenceDefaults.InProgress, "In Progress");
        yield return new(DecommissioningReferenceSets.DecommissioningStatus, DecommissioningReferenceDefaults.Completed, "Completed");
        yield return new(DecommissioningReferenceSets.DecommissioningStatus, DecommissioningReferenceDefaults.Cancelled, "Cancelled");

        yield return new(DecommissioningReferenceSets.RestorationStatus, DecommissioningReferenceDefaults.Planned, "Planned");
        yield return new(DecommissioningReferenceSets.RestorationStatus, DecommissioningReferenceDefaults.InProgress, "In Progress");
        yield return new(DecommissioningReferenceSets.RestorationStatus, DecommissioningReferenceDefaults.Completed, "Completed");
        yield return new(DecommissioningReferenceSets.RestorationStatus, DecommissioningReferenceDefaults.Cancelled, "Cancelled");

        yield return new(DecommissioningReferenceSets.CostStatus, DecommissioningReferenceDefaults.Planned, "Planned");
        yield return new(DecommissioningReferenceSets.CostStatus, DecommissioningReferenceDefaults.InProgress, "In Progress");
        yield return new(DecommissioningReferenceSets.CostStatus, DecommissioningReferenceDefaults.Completed, "Completed");
        yield return new(DecommissioningReferenceSets.CostStatus, DecommissioningReferenceDefaults.Cancelled, "Cancelled");

        yield return new(DecommissioningReferenceSets.WellStatusType, DecommissioningReferenceDefaults.DecommissioningStatusType, "Decommissioning");

        yield return new(DecommissioningReferenceSets.WellStatus, "P_AND_A_PLANNED", "Plug and Abandon Planned");
        yield return new(DecommissioningReferenceSets.WellStatus, "P_AND_A_EXECUTING", "Plug and Abandon Executing");
        yield return new(DecommissioningReferenceSets.WellStatus, "P_AND_A_VERIFIED", "Plug and Abandon Verified");
        yield return new(DecommissioningReferenceSets.WellStatus, "P_AND_A_FAILED", "Plug and Abandon Failed");
    }
}
