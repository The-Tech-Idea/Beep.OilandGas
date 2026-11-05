using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

/// <summary>One retained original command for an ordered independent investment stage.</summary>
public sealed record DevelopmentInvestmentStageDecision(
    [property:JsonRequired]DevelopmentInvestmentAssignedReview Review,
    [property:JsonRequired]Guid IntentId,
    [property:JsonRequired]string Decision,
    [property:JsonRequired]string Rationale)
{
    public const int MaximumBodyLength=4*1024*1024;
    public DevelopmentAssignedReviewScope Scope()=>Review.Source.Current.Source.Review.Scope;
    public bool Terminal()=>Decision=="REJECT" || Review.StageNumber==Review.StageCount;
    public void Require()
    {
        if(Review is null || IntentId==Guid.Empty || Decision is not ("APPROVE" or "REJECT") || string.IsNullOrWhiteSpace(Rationale) ||
            Rationale!=Rationale.Trim() || Rationale.Length>1600 || Rationale.Any(value=>char.IsControl(value) && value is not ('\r' or '\n' or '\t')))
            throw new InvalidOperationException("Retain an explicit decision and rationale for the exact independent investment stage.");
        Review.Require(Scope().Company,Scope().ActorUserId,Review.InstanceId);
        if(Review.Source.Recorded is not null || Review.Source.Blockers.Count!=0 || Review.ReservedDecisionRevision is not null || JsonSerializer.Serialize(this).Length>MaximumBodyLength)
            throw new InvalidOperationException("The exact unreserved stage and unchanged financial request must be ready for its first explicit decision.");
    }
    public string Revision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this));}
    public DevelopmentInvestmentCommand FinancialCommand()
    {
        Require();if(!Terminal())throw new InvalidOperationException("An intermediate approval cannot record a final financial outcome.");
        var result=new DevelopmentInvestmentCommand(Review.Source,IntentId,Decision,Rationale);result.Require();return result;
    }
    public DevelopmentInvestmentFacts RequireFinancial(DevelopmentInvestmentFacts facts)
    {
        Require();facts.Require(Scope().Company,Review.Source.Afe());
        var expected=DevelopmentInvestmentCommand.Facts(FinancialCommand().ToRecord(facts.AtUtc.UtcDateTime));
        if(facts!=expected)throw new InvalidOperationException("The recorded financial outcome differs from the retained stage decision and original actor/intent/source.");return facts;
    }
}

public sealed record DevelopmentInvestmentStageReceipt(
    [property:JsonRequired]string IntentRevision,
    [property:JsonRequired]string InstanceId,
    [property:JsonRequired]string Status,
    [property:JsonRequired]string WorkflowState,
    [property:JsonRequired]string? ApprovalId,
    [property:JsonRequired]string? HistoryId,
    [property:JsonRequired]string? NextStepInstanceId,
    [property:JsonRequired]DevelopmentInvestmentFacts? Financial,
    [property:JsonRequired]string RecordedRevision,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    public void Require(DevelopmentInvestmentStageDecision original)
    {
        original.Require();
        if(IntentRevision!=original.Revision() || InstanceId!=original.Review.InstanceId || Status is not ("ABSENT" or "RESERVED" or "FINANCIAL_RECORDED" or "RECORDED") ||
            !LeaseSummaryValidator.Identifier(WorkflowState) || !ProcessDefinitionRevision.IsValid(RecordedRevision) || CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero ||
            (Status=="RECORDED"? !LeaseSummaryValidator.Identifier(ApprovalId) || !LeaseSummaryValidator.Identifier(HistoryId):ApprovalId is not null || HistoryId is not null || NextStepInstanceId is not null) ||
            NextStepInstanceId is not null && (!LeaseSummaryValidator.Identifier(NextStepInstanceId) || original.Terminal()) ||
            (Status=="FINANCIAL_RECORDED" || Status=="RECORDED" && original.Terminal())!=(Financial is not null))
            throw new InvalidOperationException("The investment stage receipt does not confirm the retained original decision and its precise partial or complete outcome.");
        if(Financial is not null)original.RequireFinancial(Financial);
    }
}

public sealed record DevelopmentInvestmentStageRecoveryInput(
    [property:JsonRequired]CompanyScope Company,
    [property:JsonRequired]string ActorUserId,
    [property:JsonRequired]string InstanceId,
    [property:JsonRequired]DevelopmentInvestmentStageDecision? Original,
    [property:JsonRequired]string NativeRevision)
{
    public void Require(CompanyScope company,string actor,string instanceId)
    {
        if(Company!=company || ActorUserId!=actor || InstanceId!=instanceId || !LeaseSummaryValidator.Identifier(actor) ||
            !LeaseSummaryValidator.Identifier(instanceId) || !instanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal) || !ProcessDefinitionRevision.IsValid(NativeRevision))
            throw new InvalidOperationException("The original stage recovery input belongs to another company, account or assignment.");
        if(Original is not null){Original.Require();if(Original.Scope().Company!=company || Original.Scope().ActorUserId!=actor || Original.Review.InstanceId!=instanceId)throw new InvalidOperationException("The retained original recovery decision differs from its current receiving envelope.");}
    }
}
