using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>Public recorded period facts; native command receipts remain private to configuration and posting owners.</summary>
public sealed record CompanyLedgerPeriodFacts(string PeriodId,string PeriodCode,DateTime From,DateTime Through,string Status,string CreatedBy,DateTime CreatedAt,string? ChangedBy,DateTime? ChangedAt);
public sealed record CompanyLedgerPeriodSource(CompanyScope Company,string ActorUserId,string BookId,string CurrencyCode,string CalendarId,IReadOnlyList<CompanyLedgerPeriodFacts> Periods,string Revision)
{
    public static CompanyLedgerPeriodSource FromNative(CompanyLedgerPeriodReview native)
    {
        native.Require(native.Company,native.ActorUserId,native.BookId,native.CurrencyCode,native.CalendarId);
        var source=new CompanyLedgerPeriodSource(native.Company,native.ActorUserId,native.BookId,native.CurrencyCode,native.CalendarId,native.Periods.Select(row=>new CompanyLedgerPeriodFacts(row.COMPANY_LEDGER_PERIOD_ID,row.PERIOD_CODE,row.START_DATE,row.END_DATE,row.STATUS,row.ROW_CREATED_BY,row.ROW_CREATED_DATE!.Value,row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE)).ToArray(),string.Empty);
        source=source with{Revision=Digest(source)};source.Require(native.Company,native.ActorUserId,native.BookId,native.CurrencyCode,native.CalendarId);return CompanyInventorySourceReview.Copy(source);
    }
    public void Require(CompanyScope company,string actor,string book,string currency,string calendar)
    {
        PurchaseOrderOwnership.RequireScope(company);foreach(var value in new[]{actor,book,calendar})BillingPolicyValues.Text(value,128);BillingPolicyValues.Text(currency,16);
        if(Company!=company || ActorUserId!=actor || BookId!=book || CurrencyCode!=currency || CalendarId!=calendar || Periods is null || Periods.Count>CompanyLedgerPeriodReview.MaximumPeriods || Periods.Any(row=>row is null) ||
            Periods.Select(row=>row.PeriodId).Distinct(StringComparer.Ordinal).Count()!=Periods.Count || Periods.Select(row=>row.PeriodCode).Distinct(StringComparer.Ordinal).Count()!=Periods.Count || !Periods.Select(row=>row.PeriodId).SequenceEqual(Periods.Select(row=>row.PeriodId).Order(StringComparer.Ordinal)))throw new InvalidOperationException("The complete public company/book/calendar period source is unconfirmed.");
        foreach(var row in Periods){BillingPolicyValues.Intent(row.PeriodId);BillingPolicyValues.Text(row.PeriodCode,128);CompanyGLMappingReview.RequireDate(row.From);CompanyGLMappingReview.RequireDate(row.Through);
            CompanyGLAccountReview.RequireAudit(new COMPANY_LEDGER_PERIOD{ROW_CREATED_BY=row.CreatedBy,ROW_CREATED_DATE=row.CreatedAt,ROW_CHANGED_BY=row.ChangedBy!, ROW_CHANGED_DATE=row.ChangedAt});
            if(row.From>row.Through || row.Status is not ("OPEN" or "CLOSED" or "LOCKED"))throw new InvalidOperationException("The recorded public period date range or state is unavailable.");}
        var ordered=Periods.OrderBy(row=>row.From).ToArray();for(var index=1;index<ordered.Length;index++)if(ordered[index-1].Through>=ordered[index].From)throw new InvalidOperationException("The public company/book period ranges overlap.");
        if(Revision!=Digest(this))throw new InvalidOperationException("The complete public period source changed.");
    }
    private static string Digest(CompanyLedgerPeriodSource source)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(source with{Revision=string.Empty}));
}
public sealed record CompanyLedgerPeriodConfigurationReview(CompanyLedgerPeriodReview History,string BindingRevision)
{
    public void Require(CompanyScope company,string actor,string book,string currency,string calendar){ArgumentNullException.ThrowIfNull(History);History.Require(company,actor,book,currency,calendar);BillingPolicyValues.Revision(BindingRevision);}
}

