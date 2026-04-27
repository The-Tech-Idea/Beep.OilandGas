using Beep.OilandGas.ProspectIdentification;
using Xunit;

namespace Beep.OilandGas.ProspectIdentification.Tests;

/// <summary>
/// Locks exploration gate strings to the values seeded in <c>ProcessDefinitionInitializer</c> (drift guard).
/// </summary>
public class ExplorationReferenceCodesGateReviewTests
{
    [Fact]
    public void GateExploration_ProcessId_MatchesDocumentedWorkflow()
    {
        Assert.Equal("GATE_EXPLORATION_REVIEW", ExplorationReferenceCodes.ProcessIdGateExplorationReview);
    }

    [Fact]
    public void GateExploration_StepChain_IsSequential()
    {
        Assert.Equal("GATE_EXP_PACKAGE", ExplorationReferenceCodes.StepGateExplorationPackage);
        Assert.Equal("GATE_EXP_RESOURCES", ExplorationReferenceCodes.StepGateExplorationResources);
        Assert.Equal("GATE_EXP_ECONOMICS", ExplorationReferenceCodes.StepGateExplorationEconomics);
        Assert.Equal("GATE_EXP_APPROVAL", ExplorationReferenceCodes.StepGateExplorationApproval);
        Assert.Equal("GATE_EXP_COMMIT", ExplorationReferenceCodes.StepGateExplorationCommit);
    }

    [Fact]
    public void GateExploration_NameTypeAndAnchor_MatchSeed()
    {
        Assert.Equal("ExplorationGateReview", ExplorationReferenceCodes.ProcessNameExplorationGateReview);
        Assert.Equal("GATE_REVIEW", ExplorationReferenceCodes.ProcessTypeGateReview);
        Assert.Equal("POOL", ExplorationReferenceCodes.EntityTypeExplorationGateReview);
    }
}
