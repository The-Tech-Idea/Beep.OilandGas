using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.ProspectIdentification.Services;
using Xunit;

namespace Beep.OilandGas.ProspectIdentification.Tests;

/// <summary>
/// Table-row → API projection mapping used by <see cref="ProspectIdentificationService.GetProspectsAsync"/> and evaluation.
/// </summary>
public class ProspectIdentificationServiceMappingTests
{
    [Fact]
    public void ResolveFieldId_PrefersPrimaryFieldId()
    {
        var row = new PROSPECT { PRIMARY_FIELD_ID = "PF-1", FIELD_ID = "F-legacy" };
        Assert.Equal("PF-1", ProspectIdentificationService.ResolveFieldId(row));
    }

    [Fact]
    public void ResolveFieldId_FallsBackToFieldId()
    {
        var row = new PROSPECT { PRIMARY_FIELD_ID = "", FIELD_ID = "F-2" };
        Assert.Equal("F-2", ProspectIdentificationService.ResolveFieldId(row));
    }

    [Fact]
    public void ResolveStatus_PrefersProspectStatus()
    {
        var row = new PROSPECT { PROSPECT_STATUS = "APPROVED", STATUS = "Old" };
        Assert.Equal("APPROVED", ProspectIdentificationService.ResolveStatus(row));
    }

    [Fact]
    public void ResolveStatus_FallsBackToStatus()
    {
        var row = new PROSPECT { PROSPECT_STATUS = "", STATUS = "Screening" };
        Assert.Equal("Screening", ProspectIdentificationService.ResolveStatus(row));
    }

    [Fact]
    public void ResolveRecommendation_WhenProspectNull_UsesDefault()
    {
        Assert.Equal("Further evaluation recommended", ProspectIdentificationService.ResolveRecommendation(null, 0.5m));
    }

    [Fact]
    public void ResolveRecommendation_ApprovedStatus_Drilling()
    {
        var row = new PROSPECT { PROSPECT_STATUS = "approved" };
        Assert.Equal("Recommend drilling", ProspectIdentificationService.ResolveRecommendation(row, 0.9m));
    }

    [Fact]
    public void ResolveRecommendation_RejectedStatus_NoDrill()
    {
        var row = new PROSPECT { PROSPECT_STATUS = "REJECTED" };
        Assert.Equal("Do not drill", ProspectIdentificationService.ResolveRecommendation(row, 0.1m));
    }

    [Fact]
    public void ResolveRecommendation_UsesRiskScoreWhenNoStatus()
    {
        var row = new PROSPECT { PROSPECT_STATUS = "" };
        Assert.Equal("Recommend drilling", ProspectIdentificationService.ResolveRecommendation(row, 0.2m));
        Assert.Equal("Further evaluation recommended", ProspectIdentificationService.ResolveRecommendation(row, 0.5m));
        Assert.Equal("Detailed study required", ProspectIdentificationService.ResolveRecommendation(row, 0.8m));
    }
}
