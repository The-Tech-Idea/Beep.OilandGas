using System.Linq;
using Beep.OilandGas.DevelopmentPlanning.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class DevelopmentPlanningReferenceSeedCatalogTests
{
    [Fact]
    public void GetAllSeedRows_HasUniqueReferenceSetReferenceCodePairs()
    {
        var rows = DevelopmentPlanningReferenceCodeSeed.GetAllSeedRows().ToList();
        var duplicateKeys = rows
            .GroupBy(r => (r.ReferenceSet, r.ReferenceCode))
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.ReferenceSet}/{g.Key.ReferenceCode}")
            .ToList();

        Assert.Empty(duplicateKeys);
    }

    [Fact]
    public void GetAllSeedRows_ContainsCriticalDevelopmentPlanningFamilies()
    {
        var rows = DevelopmentPlanningReferenceCodeSeed.GetAllSeedRows().ToList();
        Assert.Contains(rows, r => r.ReferenceSet == DevelopmentPlanningReferenceSets.FdpStatus && r.ReferenceCode == DevelopmentPlanningDefaults.Draft);
        Assert.Contains(rows, r => r.ReferenceSet == DevelopmentPlanningReferenceSets.ScheduleStatus && r.ReferenceCode == DevelopmentPlanningDefaults.Planned);
        Assert.Contains(rows, r => r.ReferenceSet == DevelopmentPlanningReferenceSets.MaintenanceStatus && r.ReferenceCode == "SCHEDULED");
        Assert.Contains(rows, r => r.ReferenceSet == DevelopmentPlanningReferenceSets.WellJobType && r.ReferenceCode == "WORKOVER");
        Assert.Contains(rows, r => r.ReferenceSet == DevelopmentPlanningReferenceSets.ServiceJobStatus && r.ReferenceCode == "ASSIGNED");
    }

    [Fact]
    public void GetAllSeedRows_CoversAllPlannedReferenceFamilies()
    {
        var rows = DevelopmentPlanningReferenceCodeSeed.GetAllSeedRows().ToList();
        var sets = rows.Select(r => r.ReferenceSet).ToHashSet();

        Assert.Contains(DevelopmentPlanningReferenceSets.FdpStatus, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.ScheduleStatus, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.WellType, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.FacilityType, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.InvestmentPhase, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.EstimateClass, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.CostCategory, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.CostType, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.CostCurrency, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.MaintenanceType, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.MaintenanceStatus, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.MaintenancePriority, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.MaintenanceFrequency, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.MaintenanceTriggerBasis, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.WellJobType, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.ServiceJobStatus, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.ServiceJobPriority, sets);
        Assert.Contains(DevelopmentPlanningReferenceSets.ServiceJobTriggerBasis, sets);
    }
}
