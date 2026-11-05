using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>Complete native company/book/currency/calendar period history, without inferred open defaults.</summary>
public sealed record CompanyLedgerPeriodReview(CompanyScope Company,string ActorUserId,string BookId,string CurrencyCode,string CalendarId,IReadOnlyList<COMPANY_LEDGER_PERIOD> Periods,string Revision)
{
    public const int MaximumPeriods=256;
    public static CompanyLedgerPeriodReview Create(CompanyScope company,string actor,string book,string currency,string calendar,IEnumerable<COMPANY_LEDGER_PERIOD> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);var records=rows.Take(MaximumPeriods+1).ToArray();if(records.Length>MaximumPeriods || records.Any(row=>row is null))throw new InvalidOperationException("The complete bounded company period history is unavailable.");
        var result=new CompanyLedgerPeriodReview(company,actor,book,currency,calendar,records.OrderBy(row=>row.COMPANY_LEDGER_PERIOD_ID,StringComparer.Ordinal).ToArray(),string.Empty);
        result=result with{Revision=Digest(result)};result.Require(company,actor,book,currency,calendar);var frozen=CompanyInventorySourceReview.Copy(result);frozen.Require(company,actor,book,currency,calendar);return frozen;
    }
    public void Require(CompanyScope company,string actor,string book,string currency,string calendar)
    {
        PurchaseOrderOwnership.RequireScope(company);foreach(var value in new[]{actor,book,calendar})BillingPolicyValues.Text(value,128);BillingPolicyValues.Text(currency,16);
        if(Company!=company || ActorUserId!=actor || BookId!=book || CurrencyCode!=currency || CalendarId!=calendar || Periods is null || Periods.Count>MaximumPeriods || Periods.Any(row=>row is null) ||
            Periods.Select(row=>row.COMPANY_LEDGER_PERIOD_ID).Distinct(StringComparer.Ordinal).Count()!=Periods.Count || Periods.Select(row=>row.PERIOD_CODE).Distinct(StringComparer.Ordinal).Count()!=Periods.Count ||
            !Periods.Select(row=>row.COMPANY_LEDGER_PERIOD_ID).SequenceEqual(Periods.Select(row=>row.COMPANY_LEDGER_PERIOD_ID).Order(StringComparer.Ordinal)))throw new InvalidOperationException("The complete original company period review is unconfirmed.");
        foreach(var row in Periods)
        {
            BillingPolicyValues.Intent(row.COMPANY_LEDGER_PERIOD_ID);BillingPolicyValues.Text(row.PERIOD_CODE,128);CompanyGLMappingReview.RequireDate(row.START_DATE);CompanyGLMappingReview.RequireDate(row.END_DATE);CompanyGLAccountReview.RequireAudit(row);
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.BOOK_ID!=book || row.CURRENCY_CODE!=currency || row.CALENDAR_ID!=calendar || row.ACTIVE_IND!="Y" || row.START_DATE>row.END_DATE ||
                row.STATUS is not ("OPEN" or "CLOSED" or "LOCKED"))throw new InvalidOperationException("A period lacks original company/book/currency/calendar ownership, date range or durable state.");
            CompanyLedgerPeriodAuthority.Require(row);
        }
        var ordered=Periods.OrderBy(row=>row.START_DATE).ToArray();for(var index=1;index<ordered.Length;index++)if(ordered[index-1].END_DATE>=ordered[index].START_DATE)throw new InvalidOperationException("Company book period ranges overlap.");
        if(Revision!=Digest(this))throw new InvalidOperationException("The complete native period review changed.");
    }
    public COMPANY_LEDGER_PERIOD RequireOpen(DateTime companyDate)
    {
        Require(Company,ActorUserId,BookId,CurrencyCode,CalendarId);CompanyGLMappingReview.RequireDate(companyDate);
        var period=Periods.SingleOrDefault(row=>row.START_DATE<=companyDate && row.END_DATE>=companyDate) ??throw new InvalidOperationException("A recorded company/book period for the explicit calendar date is unavailable.");
        if(period.STATUS!="OPEN")throw new InvalidOperationException("The original recorded company/book period is not open for posting.");return CompanyInventorySourceReview.Copy(period);
    }
    private static string Digest(CompanyLedgerPeriodReview value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}

/// <summary>Native creation receipt. Controlled state transitions extend this authority history separately.</summary>
public sealed record CompanyLedgerPeriodCreation(CompanyScope Company,string ActorUserId,string BookId,string CurrencyCode,string CalendarId,string PeriodId,string PeriodCode,DateTime From,DateTime Through,string IntentId,string Reason,string SourceRevision,string BindingRevision)
{
    public void Require()
    {PurchaseOrderOwnership.RequireScope(Company);foreach(var value in new[]{ActorUserId,BookId,CalendarId,PeriodCode,Reason})BillingPolicyValues.Text(value,128);BillingPolicyValues.Text(CurrencyCode,16);BillingPolicyValues.Intent(PeriodId);BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(BindingRevision);CompanyGLMappingReview.RequireDate(From);CompanyGLMappingReview.RequireDate(Through);if(Through<From)throw new InvalidOperationException("The explicit company period range is invalid.");}
    public void Require(CompanyLedgerPeriodReview review)
    {Require();review.Require(Company,ActorUserId,BookId,CurrencyCode,CalendarId);if(review.Revision!=SourceRevision || review.Periods.Count>=CompanyLedgerPeriodReview.MaximumPeriods || review.Periods.Any(row=>row.COMPANY_LEDGER_PERIOD_ID==PeriodId || row.PERIOD_CODE==PeriodCode || row.START_DATE<=Through && row.END_DATE>=From))throw new InvalidOperationException("The complete original period history changed, is full, overlaps or reserves the requested period identity/code.");}
    public COMPANY_LEDGER_PERIOD CreateRow()=>new(){COMPANY_LEDGER_PERIOD_ID=PeriodId,OWNER_TENANT_ID=Company.TenantId,OWNER_COMPANY_BA_ID=Company.BusinessAssociateId,BOOK_ID=BookId,CURRENCY_CODE=CurrencyCode,CALENDAR_ID=CalendarId,PERIOD_CODE=PeriodCode,START_DATE=From,END_DATE=Through,STATUS="OPEN",ACTIVE_IND="Y",SOURCE=CompanyLedgerPeriodAuthority.Source,REMARK=Reason,PPDM_GUID=Guid.ParseExact(IntentId,"N").ToString()};
}
public sealed record CompanyLedgerPeriodCreationSubmission(CompanyLedgerPeriodCreation Command,CompanyLedgerPeriodReview? Review);
public sealed record CompanyLedgerPeriodCreationReceipt(CompanyLedgerPeriodCreation Command,string RowRevision);
public static class CompanyLedgerPeriodAuthority
{
    public const string Source="COMPANY_LEDGER_PERIOD_AUTHORITY";
    public const int MaximumLength=65536;
    public static string Create(CompanyLedgerPeriodCreation command,COMPANY_LEDGER_PERIOD row)
    {RequireFacts(command,row);var json=JsonSerializer.Serialize(new CompanyLedgerPeriodCreationReceipt(command,Digest(row)));if(json.Length>MaximumLength)throw new InvalidOperationException("Native period authority exceeds its declared capacity.");return json;}
    public static CompanyLedgerPeriodCreationReceipt Require(COMPANY_LEDGER_PERIOD row)
    {
        if(row.STATE_HISTORY_JSON is not null)row=CompanyLedgerPeriodStateHistory.Require(row).Original;
        if(string.IsNullOrWhiteSpace(row.AUTHORITY_JSON) || row.AUTHORITY_JSON.Length>MaximumLength)throw new InvalidOperationException("The native period authority receipt is unavailable.");var receipt=JsonSerializer.Deserialize<CompanyLedgerPeriodCreationReceipt>(row.AUTHORITY_JSON) ??throw new InvalidOperationException("The native period receipt is malformed.");
        if(receipt.Command is null)throw new InvalidOperationException("The original period creation is unavailable.");RequireFacts(receipt.Command,row);if(receipt.RowRevision!=Digest(row))throw new InvalidOperationException("The complete recorded period authority changed.");return receipt;
    }
    private static void RequireFacts(CompanyLedgerPeriodCreation command,COMPANY_LEDGER_PERIOD row)
    {
        command.Require();CompanyGLAccountReview.RequireAudit(row);var expected=command.CreateRow();expected.ROW_CREATED_BY=row.ROW_CREATED_BY;expected.ROW_CREATED_DATE=row.ROW_CREATED_DATE;expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;
        var actual=CompanyInventorySourceReview.Copy(row);actual.AUTHORITY_JSON=string.Empty;
        if(row.ROW_CREATED_BY!=command.ActorUserId || row.ROW_CHANGED_DATE is not null && row.ROW_CHANGED_BY!=command.ActorUserId || JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(actual))throw new InvalidOperationException("Only the exact created company/book period and native audit are confirmed.");
    }
    private static string Digest(COMPANY_LEDGER_PERIOD row){var copy=CompanyInventorySourceReview.Copy(row);copy.AUTHORITY_JSON=string.Empty;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
