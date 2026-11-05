using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DevelopmentPlanning.Models;

public sealed record DevelopmentInvestmentStageProgress(
    [property:JsonRequired]int Number,
    [property:JsonRequired]string StepId,
    [property:JsonRequired]string Name,
    [property:JsonRequired]string Role,
    [property:JsonRequired]IReadOnlyList<string> Personas,
    [property:JsonRequired]string Status,
    [property:JsonRequired]string? Reviewer,
    [property:JsonRequired]string? Decision,
    [property:JsonRequired]string? Rationale,
    [property:JsonRequired]DateTimeOffset? AtUtc,
    [property:JsonRequired]string? ApprovalId,
    [property:JsonRequired]string? HistoryId);

/// <summary>Read-only, complete original workflow projection; no command or recovery input.</summary>
public sealed record DevelopmentInvestmentProgress(
    [property:JsonRequired]string InstanceId,
    [property:JsonRequired]string WorkflowVersion,
    [property:JsonRequired]string DefinitionRevision,
    [property:JsonRequired]string State,
    [property:JsonRequired]int PendingTasks,
    [property:JsonRequired]IReadOnlyList<DevelopmentInvestmentStageProgress> Stages,
    [property:JsonRequired]string? ReservedBy,
    [property:JsonRequired]string? ReservationRevision,
    [property:JsonRequired]string? FinancialRevision,
    [property:JsonRequired]string NativeRevision,
    [property:JsonRequired]string Revision)
{
    public string ContentRevision()=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this with{Revision=""}));
    public void Require(DevelopmentInvestmentReview source,string instance,string sender,DateTimeOffset started)
    {
        source.Require();
        if(InstanceId!=instance || !LeaseSummaryValidator.Identifier(WorkflowVersion) || !ProcessDefinitionRevision.IsValid(DefinitionRevision) ||
            !ProcessDefinitionRevision.IsValid(NativeRevision) || Revision!=ContentRevision() || State is not ("IN_REVIEW" or "RESERVED" or "FINANCIAL_RECORDED" or "APPROVED" or "REJECTED" or "RECONCILIATION_REQUIRED") ||
            Stages is null || Stages.Count is <1 or >64 || Stages.Any(stage=>stage is null) || Stages.Select(stage=>stage.StepId).Distinct(StringComparer.Ordinal).Count()!=Stages.Count || PendingTasks is <0 or >128 ||
            (ReservedBy is null)!=(ReservationRevision is null) || ReservedBy is not null && (!LeaseSummaryValidator.Identifier(ReservedBy) || ReservedBy==sender || ReservedBy==source.Afe().CreatedBy || !ProcessDefinitionRevision.IsValid(ReservationRevision)) ||
            (FinancialRevision is null?source.Recorded is not null:FinancialRevision!=source.Recorded?.RecordedRevision))
            throw new InvalidOperationException("The investment progress does not match its complete original workflow and current financial source.");
        DateTimeOffset previous=started;
        for(var index=0;index<Stages.Count;index++)
        {
            var stage=Stages[index];var closed=stage.Status is "APPROVED" or "REJECTED";
            if(stage.Number!=index+1 || !LeaseSummaryValidator.Identifier(stage.StepId) || !LeaseSummaryValidator.Identifier(stage.Role) ||
                string.IsNullOrWhiteSpace(stage.Name) || stage.Name.Length>256 || stage.Name.Any(char.IsControl) ||
                stage.Personas is null || stage.Personas.Count is <1 or >128 || stage.Personas.Any(persona=>!LeaseSummaryValidator.Identifier(persona)) || stage.Personas.Distinct(StringComparer.Ordinal).Count()!=stage.Personas.Count ||
                stage.Status is not ("APPROVED" or "REJECTED" or "PENDING" or "RESERVED" or "WAITING") ||
                (closed? !LeaseSummaryValidator.Identifier(stage.Reviewer) || stage.Reviewer==sender || stage.Reviewer==source.Afe().CreatedBy ||
                    stage.Decision!=(stage.Status=="APPROVED"?"APPROVE":"REJECT") || string.IsNullOrWhiteSpace(stage.Rationale) || stage.Rationale!=stage.Rationale.Trim() || stage.Rationale.Length>1600 || stage.Rationale.Any(c=>char.IsControl(c) && c is not ('\r' or '\n' or '\t')) ||
                    stage.AtUtc is null || stage.AtUtc.Value.Offset!=TimeSpan.Zero || stage.AtUtc<previous || !LeaseSummaryValidator.Identifier(stage.ApprovalId) || !LeaseSummaryValidator.Identifier(stage.HistoryId):
                    stage.Reviewer is not null || stage.Decision is not null || stage.Rationale is not null || stage.AtUtc is not null || stage.ApprovalId is not null || stage.HistoryId is not null))
                throw new InvalidOperationException("The ordered stage audit is incomplete or belongs to another independent review.");
            if(closed)previous=stage.AtUtc!.Value;
        }
        var approved=Stages.TakeWhile(stage=>stage.Status=="APPROVED").Count();var final=approved==Stages.Count?Stages[^1]:Stages[approved];var terminal=approved==Stages.Count || final.Status=="REJECTED";
        if(terminal)
        {
            if(PendingTasks!=0 || ReservedBy is not null || State!=(final.Status=="APPROVED"?"APPROVED":"REJECTED") ||
                Stages.Skip(approved+1).Any(stage=>stage.Status!="WAITING") || source.Recorded is not {} financial || financial.Decision!=final.Decision || financial.Reviewer!=final.Reviewer || financial.AtUtc>final.AtUtc)
                throw new InvalidOperationException("Final investment progress requires its recorded financial outcome and complete native closure.");
        }
        else if(final.Status is not ("PENDING" or "RESERVED") || Stages.Skip(approved+1).Any(stage=>stage.Status!="WAITING") || PendingTasks!=final.Personas.Count ||
            (final.Status=="RESERVED")!=(ReservedBy is not null) ||
            State=="IN_REVIEW" && (ReservedBy is not null || FinancialRevision is not null) ||
            State=="RESERVED" && (ReservedBy is null || FinancialRevision is not null) ||
            State=="FINANCIAL_RECORDED" && (ReservedBy is null || FinancialRevision is null || source.Recorded!.Reviewer!=ReservedBy) ||
            State is "APPROVED" or "REJECTED" || State=="RECONCILIATION_REQUIRED" && FinancialRevision is null)
            throw new InvalidOperationException("The current investment stage, pending work and partial outcome do not agree.");
    }
}
