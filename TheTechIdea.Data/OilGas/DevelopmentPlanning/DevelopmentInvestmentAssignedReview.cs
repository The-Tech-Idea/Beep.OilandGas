using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>Current individual admission to one original AFE assignment, independent of active FIELD preferences.</summary>
public sealed record DevelopmentInvestmentAssignedReview(
    [property:JsonRequired]DevelopmentInvestmentReview Source,
    [property:JsonRequired]string InstanceId,
    [property:JsonRequired]string WorkflowName,
    [property:JsonRequired]string WorkflowVersion,
    [property:JsonRequired]string Sender,
    [property:JsonRequired]string StepId,
    [property:JsonRequired]string StepInstanceId,
    [property:JsonRequired]string StepName,
    [property:JsonRequired]string AssignedRole,
    [property:JsonRequired]string PersonaCode,
    [property:JsonRequired]string TaskId,
    [property:JsonRequired]string DefinitionRevision,
    [property:JsonRequired]string ProcessRevision,
    [property:JsonRequired]string TaskRevision,
    [property:JsonRequired]string Revision,
    [property:JsonRequired]DateTimeOffset CheckedAtUtc)
{
    [JsonRequired]public int StageNumber{get;init;}
    [JsonRequired]public int StageCount{get;init;}
    public string? ReservedDecisionRevision{get;init;}
    public string? ReservedBy{get;init;}
    [JsonRequired]public string FundingRationale{get;init;}=null!;
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{
        Source=Source.Current.Revision,Source.AfeId,Source.Recorded,Source.Blockers,InstanceId,WorkflowName,WorkflowVersion,Sender,
        StepId,StepInstanceId,StepName,AssignedRole,PersonaCode,TaskId,DefinitionRevision,ProcessRevision,TaskRevision,FundingRationale,StageNumber,StageCount,ReservedDecisionRevision,ReservedBy}));
    public void Require(CompanyScope company,string actor,string instance)
    {
        Source.Require();var scope=Source.Current.Source.Review.Scope;
        if(scope.Company!=company || scope.ActorUserId!=actor || InstanceId!=instance || !InstanceId.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal) ||
            new[]{InstanceId,Sender,StepId,StepInstanceId,AssignedRole,PersonaCode,TaskId,WorkflowVersion}.Any(value=>!LeaseSummaryValidator.Identifier(value)) ||
            string.IsNullOrWhiteSpace(FundingRationale) || FundingRationale.Length>4096 || FundingRationale!=FundingRationale.Trim() || FundingRationale.Any(value=>char.IsControl(value) && value is not ('\r' or '\n' or '\t')) ||
            StageCount is <1 or >64 || StageNumber<1 || StageNumber>StageCount || (ReservedDecisionRevision is null)!=(ReservedBy is null) ||
            ReservedDecisionRevision is not null && (!ProcessDefinitionRevision.IsValid(ReservedDecisionRevision) || !LeaseSummaryValidator.Identifier(ReservedBy)) ||
            PersonaCode.Length>64 || actor==Sender || actor==Source.Afe().CreatedBy ||
            new[]{WorkflowName,StepName}.Any(value=>string.IsNullOrWhiteSpace(value) || value.Length>256 || value.Any(char.IsControl)) ||
            new[]{DefinitionRevision,ProcessRevision,TaskRevision,Revision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)) ||
            CheckedAtUtc==default || CheckedAtUtc.Offset!=TimeSpan.Zero || Revision!=ContentRevision())
            throw new InvalidOperationException("The current independent investment assignment and exact AFE source could not be confirmed.");
    }
}
