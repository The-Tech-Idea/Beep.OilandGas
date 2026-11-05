using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyLedgerPeriodTransitionCommand(string IntentId,CompanyScope Company,string ActorUserId,string BookId,string CurrencyCode,string CalendarId,string PeriodId,
    string FromState,string ToState,string ClosingDisposition,string ClosingEvidence,string Reason,string SourceRevision,string BindingRevision)
{
    public void Require()
    {
        PurchaseOrderOwnership.RequireScope(Company);BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Intent(PeriodId);foreach(var value in new[]{ActorUserId,BookId,CalendarId,Reason})BillingPolicyValues.Text(value,128);BillingPolicyValues.Text(CurrencyCode,16);BillingPolicyValues.Text(ClosingEvidence,2000);BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(BindingRevision);
        if(!Allowed(FromState,ToState) || ClosingDisposition!="CONFIRMED")throw new InvalidOperationException("An explicit allowed close, lock or reopen with confirmed control evidence is required.");
    }
    public COMPANY_LEDGER_PERIOD Require(CompanyLedgerPeriodReview review)
    {
        Require();review.Require(Company,ActorUserId,BookId,CurrencyCode,CalendarId);var row=review.Periods.SingleOrDefault(period=>period.COMPANY_LEDGER_PERIOD_ID==PeriodId) ??throw new InvalidOperationException("The original recorded period is unavailable.");
        var previous=row.STATE_HISTORY_JSON is null?row.ROW_CREATED_BY:CompanyLedgerPeriodStateHistory.Require(row).Entries[^1].Command.ActorUserId;
        if(review.Revision!=SourceRevision || row.STATUS!=FromState || ActorUserId==previous)throw new InvalidOperationException("Current period state/source and an actor independent of its previous authority are required.");return CompanyInventorySourceReview.Copy(row);
    }
    public static bool Allowed(string from,string to)=>from=="OPEN" && to=="CLOSED" || from=="CLOSED" && to is "LOCKED" or "OPEN" || from=="LOCKED" && to=="OPEN";
}
public sealed record CompanyLedgerPeriodTransitionSubmission(CompanyLedgerPeriodTransitionCommand Command,CompanyLedgerPeriodReview? Review);
public sealed record CompanyLedgerPeriodStateEntry(CompanyLedgerPeriodTransitionCommand Command,DateTime ChangedAtUtc,string BeforeRevision,string AfterRevision);
public sealed record CompanyLedgerPeriodStateHistory(COMPANY_LEDGER_PERIOD Original,IReadOnlyList<CompanyLedgerPeriodStateEntry> Entries)
{
    public const int MaximumLength=2097152;
    public const int MaximumEntries=64;
    public static CompanyLedgerPeriodStateHistory Append(COMPANY_LEDGER_PERIOD previous,CompanyLedgerPeriodTransitionCommand command,DateTime changed)
    {
        command.Require();var history=previous.STATE_HISTORY_JSON is null?new CompanyLedgerPeriodStateHistory(CompanyInventorySourceReview.Copy(previous),[]):Require(previous);var before=CompanyInventorySourceReview.Copy(previous);before.STATE_HISTORY_JSON=null;var next=Apply(before,command,changed);
        if(history.Entries.Count>=MaximumEntries || history.Entries.Any(entry=>entry.Command.IntentId==command.IntentId))throw new InvalidOperationException("The bounded period history is full or already reserves this intent.");
        var result=new CompanyLedgerPeriodStateHistory(history.Original,history.Entries.Concat([new CompanyLedgerPeriodStateEntry(command,changed,Digest(before),Digest(next))]).ToArray());_=result.Expected();return result;
    }
    public COMPANY_LEDGER_PERIOD Expected()
    {
        if(Original is null || Original.STATE_HISTORY_JSON is not null || Entries is null || Entries.Count is <1 or >MaximumEntries || Entries.Any(entry=>entry is null || entry.Command is null) || Entries.Select(entry=>entry.Command.IntentId).Distinct(StringComparer.Ordinal).Count()!=Entries.Count)throw new InvalidOperationException("The complete ordered original period state history is required.");
        _=CompanyLedgerPeriodAuthority.Require(Original);var row=CompanyInventorySourceReview.Copy(Original);var actor=row.ROW_CREATED_BY;var at=row.ROW_CHANGED_DATE ?? row.ROW_CREATED_DATE!.Value;
        foreach(var entry in Entries){entry.Command.Require();if(entry.Command.ActorUserId==actor || entry.ChangedAtUtc<at || entry.BeforeRevision!=Digest(row))throw new InvalidOperationException("The independent period authority or ordered predecessor evidence changed.");row=Apply(row,entry.Command,entry.ChangedAtUtc);if(entry.AfterRevision!=Digest(row))throw new InvalidOperationException("The recorded period state outcome changed.");actor=entry.Command.ActorUserId;at=entry.ChangedAtUtc;}return row;
    }
    public string Serialize(){_=Expected();var json=JsonSerializer.Serialize(this);if(json.Length>MaximumLength)throw new InvalidOperationException("The original period state history exceeds native capacity.");return json;}
    public static CompanyLedgerPeriodStateHistory Require(COMPANY_LEDGER_PERIOD row)
    {
        if(string.IsNullOrWhiteSpace(row.STATE_HISTORY_JSON) || row.STATE_HISTORY_JSON.Length>MaximumLength)throw new InvalidOperationException("The durable period state history is unavailable.");var history=JsonSerializer.Deserialize<CompanyLedgerPeriodStateHistory>(row.STATE_HISTORY_JSON) ??throw new InvalidOperationException("The native period history is malformed.");var actual=CompanyInventorySourceReview.Copy(row);actual.STATE_HISTORY_JSON=null;
        if(JsonSerializer.Serialize(actual)!=JsonSerializer.Serialize(history.Expected()))throw new InvalidOperationException("Only the exact original period status and transition audit may change.");return history;
    }
    public COMPANY_LEDGER_PERIOD At(string intent)
    {
        _=Expected();var count=Array.FindIndex(Entries.ToArray(),entry=>entry.Command.IntentId==intent)+1;if(count==0)throw new InvalidOperationException("The original transition intent is unavailable.");var prefix=this with{Entries=Entries.Take(count).ToArray()};var row=prefix.Expected();row.STATE_HISTORY_JSON=prefix.Serialize();return row;
    }
    private static COMPANY_LEDGER_PERIOD Apply(COMPANY_LEDGER_PERIOD row,CompanyLedgerPeriodTransitionCommand command,DateTime changed)
    {
        command.Require();if(row.OWNER_TENANT_ID!=command.Company.TenantId || row.OWNER_COMPANY_BA_ID!=command.Company.BusinessAssociateId || row.BOOK_ID!=command.BookId || row.CURRENCY_CODE!=command.CurrencyCode || row.CALENDAR_ID!=command.CalendarId || row.COMPANY_LEDGER_PERIOD_ID!=command.PeriodId || row.STATUS!=command.FromState || changed.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("The exact original period context/state and UTC change audit are required.");
        var next=CompanyInventorySourceReview.Copy(row);next.STATE_HISTORY_JSON=null;next.STATUS=command.ToState;next.ROW_CHANGED_BY=command.ActorUserId;next.ROW_CHANGED_DATE=changed;return next;
    }
    private static string Digest(COMPANY_LEDGER_PERIOD row){var copy=CompanyInventorySourceReview.Copy(row);copy.STATE_HISTORY_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
public sealed record CompanyLedgerPeriodTransitionOutcome(CompanyLedgerPeriodTransitionCommand Command,COMPANY_LEDGER_PERIOD Period)
{
    public void Require(CompanyLedgerPeriodTransitionCommand command)
    {command.Require();if(Command!=command || Period is null)throw new InvalidOperationException("The original period transition is unconfirmed.");var history=CompanyLedgerPeriodStateHistory.Require(Period);if(history.Entries[^1].Command!=command || Period.STATUS!=command.ToState)throw new InvalidOperationException("The exact original transition authority/state differs from its durable receipt.");}
}
