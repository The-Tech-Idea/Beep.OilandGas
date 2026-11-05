namespace TheTechIdea.Data.OilGas;
public sealed record ProcessTransitionChoice(string TransitionId,string FromStateId,string ToStateId,string Trigger,bool RequiresApproval);
public sealed record ProcessTransitionReview(CompanyScope Company,string ActorUserId,string FieldId,string InstanceId,string CurrentState,string CurrentStepId,string Revision,IReadOnlyList<ProcessTransitionChoice> Choices, JournalSourceReview? Journal = null, WorkOrderSourceReview? WorkOrder = null)
{
    /// <summary>Validate a received action review against its original request scope before exposing choices.</summary>
    public void Require(CompanyScope company,string actor,string instance)
    {
        if(company is null || string.IsNullOrWhiteSpace(company.TenantId) || string.IsNullOrWhiteSpace(company.BusinessAssociateId) || string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(instance) || Company!=company || ActorUserId!=actor || InstanceId!=instance ||
            string.IsNullOrWhiteSpace(FieldId) || CurrentState is null || CurrentStepId is null ||
            Choices is {Count:>0} && (string.IsNullOrWhiteSpace(CurrentState) || string.IsNullOrWhiteSpace(CurrentStepId)) ||
            Revision is not {Length:64} || Revision.Any(value=>!char.IsAsciiHexDigit(value)) || Choices is null ||
            Choices.Any(row=>row is null || string.IsNullOrWhiteSpace(row.TransitionId) || row.FromStateId!=CurrentState || string.IsNullOrWhiteSpace(row.ToStateId) || string.IsNullOrWhiteSpace(row.Trigger)) ||
            Choices.Select(row=>row.TransitionId).Distinct(StringComparer.Ordinal).Count()!=Choices.Count ||
            Choices.Select(row=>(row.FromStateId,row.ToStateId,row.Trigger)).Distinct().Count()!=Choices.Count || Journal is not null && WorkOrder is not null)
            throw new InvalidOperationException("The action review does not match the original company, actor and process request.");
        if(Journal is {} journal && (string.IsNullOrWhiteSpace(journal.JournalEntryId) || journal.RetrievedAtUtc==default || journal.RetrievedAtUtc.Offset!=TimeSpan.Zero ||
            journal.Lines is null || journal.Lines.Any(row=>row is null || string.IsNullOrWhiteSpace(row.LineId) || row.JournalEntryId!=journal.JournalEntryId || string.IsNullOrWhiteSpace(row.AccountId)) ||
            journal.Lines.Select(row=>row.LineId).Distinct(StringComparer.Ordinal).Count()!=journal.Lines.Count))
            throw new InvalidOperationException("The recorded journal review has unconfirmed source identities.");
        if(WorkOrder is {} order)order.Require(FieldId,order.WorkOrderId);
    }
}


/// <summary>Recorded journal header captured with its workflow review; no posting receipt or currency inference.</summary>
public sealed record JournalSourceReview(string JournalEntryId, string? EntryNumber, DateTime? EntryDate,
    string? Status, string? Description, string? PreparerUserId, DateTime? CreatedAt, string? ReferenceNumber,
    string? SourceModule, string? BookId, decimal? TotalDebit, decimal? TotalCredit, DateTimeOffset RetrievedAtUtc, IReadOnlyList<JournalSourceLineReview>? Lines = null);

public sealed record JournalSourceLineReview(string LineId, string JournalEntryId, int? LineNumber, string AccountId,
    decimal? Debit, decimal? Credit, string? CurrencyCode, decimal? ExchangeRate, string? CostCenterId, string? Description);
