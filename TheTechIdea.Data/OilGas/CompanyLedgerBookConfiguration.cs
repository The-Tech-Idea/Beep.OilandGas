using System.Text.Json;
namespace TheTechIdea.Data.OilGas;
public sealed record CompanyLedgerBookFacts(string Id,string BookId,string Name,string FunctionalCurrency,int CurrencyDecimals,string RoundingPolicy,string CalendarId,string TimeZoneId,string CreatedBy,DateTime CreatedAtUtc,string NativeRevision);
public sealed record CompanyLedgerBookReview(CompanyScope Company,string ActorUserId,IReadOnlyList<CompanyLedgerBookFacts> Books,string Revision)
{
    public const int MaximumBooks=128;
    public static CompanyLedgerBookReview Create(CompanyScope company,string actor,IEnumerable<COMPANY_LEDGER_BOOK> records)
    {
        var rows=records.Take(MaximumBooks+1).ToArray();if(rows.Length>MaximumBooks || rows.Any(row=>row is null))throw new InvalidOperationException("The complete bounded company book configuration is unavailable.");
        foreach(var row in rows){var command=CompanyLedgerBookConfigurationProof.Require(row).Command;if(command.Company!=company)throw new InvalidOperationException("A book belongs to another original company.");}
        var value=new CompanyLedgerBookReview(company,actor,rows.OrderBy(row=>row.BOOK_ID,StringComparer.Ordinal).Select(row=>new CompanyLedgerBookFacts(row.COMPANY_LEDGER_BOOK_ID,row.BOOK_ID,row.BOOK_NAME,row.FUNCTIONAL_CURRENCY,row.CURRENCY_DECIMALS,row.ROUNDING_POLICY,row.CALENDAR_ID,row.TIME_ZONE_ID,row.ROW_CREATED_BY,row.ROW_CREATED_DATE!.Value,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(row)))).ToArray(),string.Empty);value=value with{Revision=Digest(value)};value.Require(company,actor);return CompanyInventorySourceReview.Copy(value);
    }
    public void Require(CompanyScope company,string actor)
    {
        PurchaseOrderOwnership.RequireScope(company);BillingPolicyValues.Text(actor,128);if(Company!=company || ActorUserId!=actor || Books is null || Books.Count>MaximumBooks || Books.Any(book=>book is null) || Books.Select(book=>book.Id).Distinct(StringComparer.Ordinal).Count()!=Books.Count || Books.Select(book=>book.BookId).Distinct(StringComparer.Ordinal).Count()!=Books.Count || !Books.Select(book=>book.BookId).SequenceEqual(Books.Select(book=>book.BookId).Order(StringComparer.Ordinal)))throw new InvalidOperationException("The complete original company book review is unconfirmed.");
        foreach(var book in Books){BillingPolicyValues.Intent(book.Id);BookValues(book.BookId,book.Name,book.FunctionalCurrency,book.CurrencyDecimals,book.RoundingPolicy,book.CalendarId,book.TimeZoneId);BillingPolicyValues.Text(book.CreatedBy,128);BillingPolicyValues.Revision(book.NativeRevision);if(book.CreatedAtUtc.Kind!=DateTimeKind.Utc || book.CreatedAtUtc==default)throw new InvalidOperationException("The original native book creation audit is required.");}if(Revision!=Digest(this))throw new InvalidOperationException("The complete native company book source changed.");
    }
    public CompanyLedgerBookFacts RequireBook(string book,string currency,string calendar)
    {
        Require(Company,ActorUserId);var found=Books.SingleOrDefault(row=>row.BookId==book) ??throw new InvalidOperationException("The explicitly selected company book has no recorded configuration.");if(found.FunctionalCurrency!=currency || found.CalendarId!=calendar)throw new InvalidOperationException("The selected currency/calendar differs from the recorded company book. Foreign-currency posting requires explicit recorded FX evidence.");return found;
    }
    public CompanyLedgerBookReview AsReader(string actor){Require(Company,ActorUserId);var value=this with{ActorUserId=actor,Revision=string.Empty};value=value with{Revision=Digest(value)};value.Require(Company,actor);return CompanyInventorySourceReview.Copy(value);}
    internal static void BookValues(string book,string name,string currency,int decimals,string rounding,string calendar,string zone)
    {
        BillingPolicyValues.Text(book,128);BillingPolicyValues.Text(name,256);BillingPolicyValues.Text(calendar,128);BillingPolicyValues.Text(zone,128);
        if(currency is null || currency.Length!=3 || currency.Any(value=>value is <'A' or >'Z') || decimals is <0 or >8 || rounding is not ("TO_EVEN" or "AWAY_FROM_ZERO"))throw new InvalidOperationException("Explicit uppercase currency, supported precision and rounding facts are required.");
        try{_=TimeZoneInfo.FindSystemTimeZoneById(zone);}catch(Exception unavailable) when(unavailable is TimeZoneNotFoundException or InvalidTimeZoneException){throw new InvalidOperationException("The explicit company timezone is unavailable on this host.",unavailable);}
    }
    private static string Digest(CompanyLedgerBookReview value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
}
public sealed record CompanyLedgerBookConfigurationCommand(string IntentId,CompanyScope Company,string ActorUserId,string Id,string BookId,string Name,string FunctionalCurrency,int CurrencyDecimals,string RoundingPolicy,string CalendarId,string TimeZoneId,string Reason,string SourceRevision,string BindingRevision)
{
    public void Require(){PurchaseOrderOwnership.RequireScope(Company);BillingPolicyValues.Intent(IntentId);BillingPolicyValues.Intent(Id);BillingPolicyValues.Text(ActorUserId,128);BillingPolicyValues.Text(Reason,128);BillingPolicyValues.Revision(SourceRevision);BillingPolicyValues.Revision(BindingRevision);CompanyLedgerBookReview.BookValues(BookId,Name,FunctionalCurrency,CurrencyDecimals,RoundingPolicy,CalendarId,TimeZoneId);}
    public void Require(CompanyLedgerBookReview review){Require();review.Require(Company,ActorUserId);if(review.Revision!=SourceRevision || review.Books.Count>=CompanyLedgerBookReview.MaximumBooks || review.Books.Any(book=>book.Id==Id || book.BookId==BookId))throw new InvalidOperationException("The complete original book configuration changed, is full or reserves this identity/code.");}
    public COMPANY_LEDGER_BOOK CreateRow()=>new(){COMPANY_LEDGER_BOOK_ID=Id,OWNER_TENANT_ID=Company.TenantId,OWNER_COMPANY_BA_ID=Company.BusinessAssociateId,BOOK_ID=BookId,BOOK_NAME=Name,FUNCTIONAL_CURRENCY=FunctionalCurrency,CURRENCY_DECIMALS=CurrencyDecimals,ROUNDING_POLICY=RoundingPolicy,CALENDAR_ID=CalendarId,TIME_ZONE_ID=TimeZoneId,ACTIVE_IND="Y",SOURCE=CompanyLedgerBookConfigurationProof.Source,REMARK=Reason,PPDM_GUID=Guid.ParseExact(IntentId,"N").ToString()};
}
public sealed record CompanyLedgerBookConfigurationReview(CompanyLedgerBookReview History,string BindingRevision){public void Require(CompanyScope company,string actor){History.Require(company,actor);BillingPolicyValues.Revision(BindingRevision);}}
public sealed record CompanyLedgerBookConfigurationSubmission(CompanyLedgerBookConfigurationCommand Command,CompanyLedgerBookReview? Review);
public sealed record CompanyLedgerBookConfigurationReceipt(CompanyLedgerBookConfigurationCommand Command,string RowRevision);
public static class CompanyLedgerBookConfigurationProof
{
    public const string Source="COMPANY_LEDGER_BOOK_CONFIGURATION";
    public const int MaximumLength=65536;
    public static string Create(CompanyLedgerBookConfigurationCommand command,COMPANY_LEDGER_BOOK row){RequireFacts(command,row);var json=JsonSerializer.Serialize(new CompanyLedgerBookConfigurationReceipt(command,Digest(row)));if(json.Length>MaximumLength)throw new InvalidOperationException("The complete original book receipt exceeds native capacity.");return json;}
    public static CompanyLedgerBookConfigurationReceipt Require(COMPANY_LEDGER_BOOK row){if(string.IsNullOrWhiteSpace(row.CONFIGURATION_JSON) || row.CONFIGURATION_JSON.Length>MaximumLength)throw new InvalidOperationException("The original native book receipt is unavailable.");var receipt=JsonSerializer.Deserialize<CompanyLedgerBookConfigurationReceipt>(row.CONFIGURATION_JSON) ??throw new InvalidOperationException("The book receipt is malformed.");if(receipt.Command is null)throw new InvalidOperationException("The original book command is unavailable.");RequireFacts(receipt.Command,row);if(receipt.RowRevision!=Digest(row))throw new InvalidOperationException("The complete configured company book changed.");return receipt;}
    private static void RequireFacts(CompanyLedgerBookConfigurationCommand command,COMPANY_LEDGER_BOOK row){command.Require();CompanyGLAccountReview.RequireAudit(row);var expected=command.CreateRow();expected.ROW_CREATED_BY=row.ROW_CREATED_BY;expected.ROW_CREATED_DATE=row.ROW_CREATED_DATE;expected.ROW_CHANGED_BY=row.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=row.ROW_CHANGED_DATE;var actual=CompanyInventorySourceReview.Copy(row);actual.CONFIGURATION_JSON=string.Empty;if(row.ROW_CREATED_BY!=command.ActorUserId || row.ROW_CHANGED_DATE is not null && row.ROW_CHANGED_BY!=command.ActorUserId || JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(actual))throw new InvalidOperationException("Only the exact original book configuration and native producer audit are authorized.");}
    private static string Digest(COMPANY_LEDGER_BOOK row){var copy=CompanyInventorySourceReview.Copy(row);copy.CONFIGURATION_JSON=string.Empty;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
}
