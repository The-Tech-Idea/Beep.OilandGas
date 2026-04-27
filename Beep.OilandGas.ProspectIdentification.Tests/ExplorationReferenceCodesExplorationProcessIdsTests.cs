using Beep.OilandGas.ProspectIdentification;
using Xunit;

namespace Beep.OilandGas.ProspectIdentification.Tests;

/// <summary>
/// Locks seeded exploration process ids and shared <c>ProcessTypeExploration</c> literal.
/// </summary>
public class ExplorationReferenceCodesExplorationProcessIdsTests
{
    [Fact]
    public void Exploration_ProcessIds_MatchLifecycleSeed()
    {
        Assert.Equal("LEAD_TO_PROSPECT", ExplorationReferenceCodes.ProcessIdLeadToProspect);
        Assert.Equal("PROSPECT_TO_DISCOVERY", ExplorationReferenceCodes.ProcessIdProspectToDiscovery);
        Assert.Equal("DISCOVERY_TO_DEVELOPMENT", ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment);
    }

    [Fact]
    public void Exploration_ProcessNames_MatchLifecycleSeed()
    {
        Assert.Equal("LeadToProspect", ExplorationReferenceCodes.ProcessNameLeadToProspect);
        Assert.Equal("ProspectToDiscovery", ExplorationReferenceCodes.ProcessNameProspectToDiscovery);
        Assert.Equal("DiscoveryToDevelopment", ExplorationReferenceCodes.ProcessNameDiscoveryToDevelopment);
    }

    [Fact]
    public void ProcessTypeExploration_AlignsWithCategoryToken()
    {
        Assert.Equal(ExplorationReferenceCodes.ExplorationCategoryToken, ExplorationReferenceCodes.ProcessTypeExploration);
        Assert.Equal("EXPLORATION", ExplorationReferenceCodes.ProcessTypeExploration);
    }

    [Fact]
    public void ProspectToDiscovery_StepIds_MatchSeededChainOrder()
    {
        Assert.Equal("PROSPECT_CREATION", ExplorationReferenceCodes.StepProspectCreation);
        Assert.Equal("RISK_ASSESSMENT", ExplorationReferenceCodes.StepRiskAssessment);
        Assert.Equal("VOLUME_ESTIMATION", ExplorationReferenceCodes.StepVolumeEstimation);
        Assert.Equal("ECONOMIC_EVALUATION", ExplorationReferenceCodes.StepEconomicEvaluation);
        Assert.Equal("DRILLING_DECISION", ExplorationReferenceCodes.StepDrillingDecision);
        Assert.Equal("DISCOVERY_RECORDING", ExplorationReferenceCodes.StepDiscoveryRecording);
    }
}
