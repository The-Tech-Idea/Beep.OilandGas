using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;
public sealed class ProcessInstanceDetailState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly Func<string, Task<ProcessInstance?>> read;
    private readonly Func<string, Task<List<ProcessHistoryEntry>>> history;
    private int generation; private bool disposed;
    public bool IsCompanyAccounting {get;private set;}
    public CompanyScope? Company=>company.Selection?.Scope;
    public bool IsHseIncident => Instance?.EntityType == CompanyHseWorkflowStart.EntityType;
    public string? HseIncidentRoute => !IsCompanyAccounting && IsHseIncident && Instance is { } source &&
        source.ProcessId == CompanyHseWorkflowStart.ProcessId && source.InstanceId.StartsWith(CompanyHseWorkflowStart.InstancePrefix, StringComparison.Ordinal) &&
        LeaseSummaryValidator.Identifier(source.InstanceId) && LeaseSummaryValidator.Identifier(source.EntityId)
        ? "/ppdm39/hse/incident-detail?incidentId=" + Uri.EscapeDataString(source.EntityId) + "&workflowInstanceId=" + Uri.EscapeDataString(source.InstanceId) : null;

    public string? AccountingSourceRoute=>!IsCompanyAccounting || Instance is null?null:Instance.EntityType=="AP_INVOICE"?
        "/accounting/payables/invoices/"+Uri.EscapeDataString(Instance.EntityId):Instance.EntityType=="PURCHASE_ORDER"?
        "/procurement/orders/"+Uri.EscapeDataString(Instance.EntityId):Instance.EntityType=="INVOICE" && company.Selection?.Permissions.Contains("Accounting.ReviewSalesInvoice",StringComparer.Ordinal)==true?
        "/accounting/sales-invoice-review?invoice="+Uri.EscapeDataString(Instance.EntityId):null;
    // Page-lifetime repeat protection only. A returned response is not a durable command receipt.
    private readonly List<(ProcessTransitionReview Review, bool ResponseReturned)> attempts = [];
    public bool HasUnconfirmedAttempt => Instance is { } source && company.Selection is { } selected &&
        attempts.Any(attempt => !attempt.ResponseReturned && attempt.Review.Company == selected.Scope &&
            attempt.Review.ActorUserId == selected.UserId && attempt.Review.FieldId == source.FieldId && attempt.Review.InstanceId == source.InstanceId);
    public bool CanSubmit(ProcessTransitionReview review) => !disposed && !IsCompanyAccounting && !IsHseIncident && ReferenceEquals(review, ActionReview) &&
        Instance is { } source && company.Selection is { } selected && review.Company == selected.Scope &&
        review.ActorUserId == selected.UserId && review.FieldId == source.FieldId && review.InstanceId == source.InstanceId &&
        !attempts.Any(attempt => attempt.Review.Company == review.Company && attempt.Review.ActorUserId == review.ActorUserId &&
            attempt.Review.FieldId == review.FieldId && attempt.Review.InstanceId == review.InstanceId &&
            (!attempt.ResponseReturned || attempt.Review.Revision == review.Revision));
    public bool TryBeginSubmission(ProcessTransitionReview review, ProcessTransitionChoice choice)
    {
        if (!CanSubmit(review) || !review.Choices.Contains(choice)) return false;
        attempts.Add((review, false));
        return true;
    }
    public void MarkResponseReturned(ProcessTransitionReview review)
    {
        var index = attempts.FindIndex(attempt => ReferenceEquals(attempt.Review, review));
        if (index >= 0) attempts[index] = (attempts[index].Review, true);
    }
    public ProcessTransitionReview? ActionReview { get; private set; }
    public ProcessInstance? Instance { get; private set; }
    public IReadOnlyList<ProcessHistoryEntry> History { get; private set; } = [];
    public ProcessInstanceDetailState(CompanyClientContext company, Func<string,Task<ProcessInstance?>> read, Func<string,Task<List<ProcessHistoryEntry>>> history)
    { this.company=company;this.read=read;this.history=history;company.Changed+=Clear; }
    public void Clear() { ++generation;IsCompanyAccounting=false;ActionReview=null;Instance=null;History=[]; }
    public async Task LoadAccountingAsync(string id,Func<string,Task<ProcessInstance?>> readAccounting)
    {
        if(disposed)return;Clear();IsCompanyAccounting=true;var version=generation;var scope=company.Snapshot;
        if(scope.Selection is null)return;
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var source=await readAccounting(id);
        if(disposed || version!=generation || scope!=company.Snapshot)return;
        if(source is null || source.InstanceId!=id || !string.IsNullOrEmpty(source.FieldId) || string.IsNullOrWhiteSpace(source.ProcessId) ||
            !Enum.IsDefined(source.Status) || source.ProcessData?.InstanceId!=id || source.ProcessData.Data is null ||
            !source.ProcessData.Data.TryGetValue(CompanyAccountingProcessBinding.PayloadKey,out var owner) || owner is null)
            throw new InvalidOperationException("The company accounting process source is unavailable or unconfirmed.");
        CompanyAccountingProcessBinding.FromJson(JsonSerializer.Serialize(owner),scope.Selection.Scope,id,source.EntityType,source.EntityId);
        var frozen=JsonSerializer.Deserialize<ProcessInstance>(JsonSerializer.Serialize(source))!;
        if(frozen.History is null || frozen.History.Any(row=>row is null || row.InstanceId!=id || string.IsNullOrWhiteSpace(row.HistoryId)) ||
            frozen.History.Select(row=>row.HistoryId).Distinct(StringComparer.Ordinal).Count()!=frozen.History.Count ||
            frozen.StepInstances is null || frozen.StepInstances.Any(row=>row is null || row.InstanceId!=id || string.IsNullOrWhiteSpace(row.StepInstanceId) ||
                string.IsNullOrWhiteSpace(row.StepId) || !Enum.IsDefined(row.Status)) ||
            frozen.StepInstances.Select(row=>row.StepInstanceId).Distinct(StringComparer.Ordinal).Count()!=frozen.StepInstances.Count)
            throw new InvalidOperationException("The company process steps or history do not resolve to this record.");
        if(frozen.Status==ProcessStatus.COMPLETED && CompanyAccountingSourceTypes.IsNativeDecision(frozen.EntityType))RequireNativeDecisionTerminal(frozen);
        else if(frozen.Status==ProcessStatus.COMPLETED)
        {
            var steps=frozen.StepInstances;var final=steps.Where(step=>step.StepId==frozen.CurrentStepId).ToArray();
            bool Utc(DateTime? value)=>value is {} time && time!=default && time.Kind==DateTimeKind.Utc;
            if(frozen.CurrentState!="COMPLETED" || !Utc(frozen.StartDate) || !Utc(frozen.CompletionDate) || frozen.CompletionDate<frozen.StartDate || string.IsNullOrWhiteSpace(frozen.CompletedBy) ||
                steps.Count==0 || steps.Select(step=>step.StepId).Distinct(StringComparer.Ordinal).Count()!=steps.Count ||
                steps.Select(step=>step.SequenceNumber).Distinct().Count()!=steps.Count || final.Length!=1 ||
                final[0].SequenceNumber!=steps.Max(step=>step.SequenceNumber) || final[0].CompletionDate!=frozen.CompletionDate || final[0].CompletedBy!=frozen.CompletedBy ||
                !frozen.History.Any(row=>row.StepInstanceId==final[0].StepInstanceId && row.Action=="STEP_COMPLETED" && row.PreviousState==final[0].StepId && row.NewState=="COMPLETED") ||
                steps.Any(step=>step.Status!=StepStatus.COMPLETED || !Utc(step.CompletionDate) || step.CompletionDate<frozen.StartDate || step.CompletionDate>frozen.CompletionDate || string.IsNullOrWhiteSpace(step.CompletedBy) || string.IsNullOrWhiteSpace(step.Notes) ||
                    frozen.History.Count(row=>row.StepInstanceId==step.StepInstanceId && row.Action=="STEP_COMPLETED")!=1 ||
                    frozen.History.Count(row=>row.StepInstanceId==step.StepInstanceId && row.Action=="STEP_COMPLETED" && row.Timestamp==step.CompletionDate &&
                        row.PerformedBy==step.CompletedBy && row.Notes==step.Notes)!=1))
                throw new InvalidOperationException("The terminal company workflow completion and recorded step history could not be confirmed.");
        }
        Instance=frozen;History=frozen.History.AsReadOnly();
    }
    private static void RequireNativeDecisionTerminal(ProcessInstance process)
    {
        bool Utc(DateTime? value)=>value is {} time && time!=default && time.Kind==DateTimeKind.Utc;
        var rejection=process.CurrentState=="REJECTION_REVIEWED";
        var steps=process.StepInstances.OrderBy(row=>row.SequenceNumber).ToArray();var final=steps.SingleOrDefault(row=>row.StepId==process.CurrentStepId);
        var start=JsonSerializer.Deserialize<CompanyAccountingProcessStart>(JsonSerializer.Serialize(process.ProcessData.Data[CompanyAccountingProcessStart.PayloadKey]))!;start.Require();
        string Action(ProcessStepInstance row)=>rejection?row==final?CompanyAccountingSourceTypes.RejectionReviewHistory(process.EntityType):CompanyAccountingSourceTypes.RejectionHistory(process.EntityType):"STEP_COMPLETED";
        if(process.CurrentState is not ("COMPLETED" or "REJECTION_REVIEWED") || !Utc(process.StartDate) || !Utc(process.CompletionDate) || process.CompletionDate<process.StartDate ||
            steps.Length is <1 or >2 || final is null || steps.Select(row=>row.SequenceNumber).Distinct().Count()!=steps.Length ||
            final.Status!=StepStatus.COMPLETED || final.CompletedBy!=process.CompletedBy || final.CompletionDate!=process.CompletionDate ||
            rejection && (steps.Length!=2 || final!=steps[1] || final.CompletedBy!=start.SourcePreparerUserId || steps[0].Outcome!="REJECTED" || final.Outcome!="REJECTION_REVIEWED" || steps[0].CompletedBy==start.SourcePreparerUserId || steps[0].CompletedBy==start.ActorUserId) ||
            !rejection && (final!=steps[0] || final.Outcome!="SUCCESS" || final.CompletedBy==start.SourcePreparerUserId || final.CompletedBy==start.ActorUserId || steps.Length==2 && steps[1].Status!=StepStatus.SKIPPED) ||
            steps.Any(row=>!Utc(row.CompletionDate) || row.CompletionDate<process.StartDate || row.CompletionDate>process.CompletionDate || string.IsNullOrWhiteSpace(row.CompletedBy) || string.IsNullOrWhiteSpace(row.Notes) ||
                (row.Status==StepStatus.SKIPPED?!rejection && row!=final && row.Outcome=="APPROVED_NO_REJECTION_REVIEW" && row.CompletedBy==final.CompletedBy && row.CompletionDate==final.CompletionDate
                    ?process.History.Any(history=>history.StepInstanceId==row.StepInstanceId):true
                    :row.Status!=StepStatus.COMPLETED || process.History.Count(history=>history.StepInstanceId==row.StepInstanceId && history.Action==Action(row))!=1 || process.History.Count(history=>history.StepInstanceId==row.StepInstanceId && history.Action==Action(row) && history.Timestamp==row.CompletionDate && history.PerformedBy==row.CompletedBy && history.Notes==row.Notes)!=1)) ||
            !process.History.Any(row=>row.StepInstanceId==final.StepInstanceId && row.Action==Action(final) && row.NewState==process.CurrentState))
            throw new InvalidOperationException("The terminal native decision and recorded workflow history could not be confirmed.");
    }
    public async Task LoadAsync(string id, Func<Task<string?>> resolveField)
    {
        if(disposed)return;Clear();var version=generation;var scope=company.Snapshot;
        if(scope.Selection is null)return;
        var field=await resolveField();
        if(disposed || version!=generation || scope!=company.Snapshot || string.IsNullOrWhiteSpace(field))return;
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var instance=await read(id);
        if(disposed || version!=generation || scope!=company.Snapshot)return;
        if(instance is null || instance.InstanceId!=id || instance.FieldId!=field || string.IsNullOrWhiteSpace(instance.EntityId) || string.IsNullOrWhiteSpace(instance.EntityType) || string.IsNullOrWhiteSpace(instance.ProcessId))
            throw new InvalidOperationException("The source process could not be confirmed for this FIELD.");
        var rows=await history(id);
        if(disposed || version!=generation || scope!=company.Snapshot)return;
        if(rows is null || rows.Any(row=>row is null || row.InstanceId!=id || string.IsNullOrWhiteSpace(row.HistoryId)) || rows.Select(row=>row.HistoryId).Distinct().Count()!=rows.Count)
            throw new InvalidOperationException("The source process history could not be confirmed.");
        Instance=instance;History=rows.AsReadOnly();
    }
    public async Task LoadActionsAsync(Func<string,Task<ProcessTransitionReview>> load)
    {
        ActionReview=null;var source=Instance;var version=generation;var scope=company.Snapshot;
        if(IsHseIncident)throw new InvalidOperationException("Incident investigation, verification and closure require their reviewed incident workflow.");
        if(IsCompanyAccounting)throw new InvalidOperationException("Company accounting decisions require the reviewed company submission and independent approval contract.");
        if(disposed || source is null || scope.Selection is null)return;
        var review=await load(source.InstanceId);
        if(disposed || version!=generation || scope!=company.Snapshot)return;
        if(review is null)throw new InvalidOperationException("The process action review is unavailable.");
        review.Require(scope.Selection.Scope,scope.Selection.UserId,source.InstanceId);
        if(review.FieldId!=source.FieldId || review.CurrentState!=source.CurrentState || review.CurrentStepId!=source.CurrentStepId)
            throw new InvalidOperationException("The action review does not match the recorded source state and step. Reload source details.");
        if(source.EntityType=="JOURNAL_ENTRY" ? review.Journal is null || review.Journal.JournalEntryId!=source.EntityId ||
            review.Journal.RetrievedAtUtc==default || review.Journal.RetrievedAtUtc.Offset!=TimeSpan.Zero : review.Journal is not null)
            throw new InvalidOperationException("The journal review does not match the recorded workflow source.");
        if(source.EntityType=="WORK_ORDER")
        {
            if(review.WorkOrder is null)throw new InvalidOperationException("Recorded maintenance evidence is required before reviewing workflow actions.");
            review.WorkOrder.Require(source.FieldId,source.EntityId);
        }
        else if(review.WorkOrder is not null)throw new InvalidOperationException("Maintenance evidence belongs to another workflow source.");
        ActionReview=review;
    }
    public void Dispose(){disposed=true;company.Changed-=Clear;Clear();}
}
