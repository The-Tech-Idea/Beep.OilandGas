using System.Collections.Generic;

namespace Beep.OilandGas.LeaseAcquisition.Constants;

public static class LeaseReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        var s = LeaseReferenceSets.LandRightOperationalStatus;
        yield return new(s, LeaseReferenceCodes.Active, "Active / in good standing");
        yield return new(s, LeaseReferenceCodes.Inactive, "Inactive / not currently active");
        yield return new(s, LeaseReferenceCodes.Pending, "Pending / in process");
        yield return new(s, LeaseReferenceCodes.Terminated, "Terminated or expired by action");
    }
}
