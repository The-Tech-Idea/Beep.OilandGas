using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    public async Task<CompanySalesJournalSourceReview> GetCompanySalesJournalSourceAsync(CompanyScope company,string actor,string receivable,string book,string currency,string calendar,DateTime date,CompanySalesJournalMappings selection,
        ARService ar,AccountMappingService mappings,PeriodClosingService periods,Func<Task<CompanySalesReceivableReviewSource>> readReceivable,Func<Task> requireAuthority,Func<string,Task> requireFieldRead,string? evidenceActor=null,CompanySalesJournalSourceReview? originalSource=null)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(ar);ArgumentNullException.ThrowIfNull(mappings);ArgumentNullException.ThrowIfNull(periods);ArgumentNullException.ThrowIfNull(readReceivable);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldRead);ArgumentNullException.ThrowIfNull(selection);
        _=CompanyLedgerPeriodReview.Create(company,actor,book,currency,calendar,[]);CompanyGLMappingReview.RequireDate(date);if(!Guid.TryParseExact(receivable,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=receivable)throw new ArgumentException("The original native receivable identity is required.");
        if(originalSource is not null)originalSource.Require(company,evidenceActor??actor,receivable,book,currency,calendar,date,selection);
        await requireAuthority();if(_resolveConnection is null)throw new InvalidOperationException("The original request-bound PRODUCTION journal owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original journal source binding is unavailable.");var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original journal source datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original journal source binding changed.");await ar.RequireJournalSourceBindingAsync(connection,source);await mappings.RequireJournalSourceBindingAsync(connection,source);await periods.RequireJournalSourceBindingAsync(connection,source);await _glAccountService.RequireBookSourceBindingAsync(connection,source);}
        async Task<CompanySalesReceivableReviewSource> ReadReceivable()
        {
            await Guard();var current=await readReceivable();await Guard();current.Require(company,actor,receivable);await requireFieldRead(current.FieldId);await Guard();
            // Confirm the callback's original AR outcome against the actual bound owner, including its retained independent reviewer journal.
            await ar.ConfirmCompanyJournalReceivableAsync(current,connection,source,Guard);await Guard();
            return current;
        }
        async Task<CompanySalesJournalSourceReview> Capture(bool closing)
        {
            // Reverse the closing order so a later initial period read cannot lend an earlier account or AR snapshot.
            CompanySalesReceivableReviewSource current;CompanyGLMappingReview accounts;CompanyLedgerPeriodReview history;CompanyLedgerBookReview books;
            books=await _glAccountService.GetCompanyBooksAsync(company,actor,Guard);_=books.RequireBook(book,currency,calendar);await Guard();
            if(closing){history=await periods.GetCompanyPeriodsAsync(company,actor,book,currency,calendar,Guard);await Guard();accounts=await mappings.GetCompanyMappingsAsync(company,actor,book,currency,date,Guard);await Guard();current=await ReadReceivable();}
            else{current=await ReadReceivable();accounts=await mappings.GetCompanyMappingsAsync(company,actor,book,currency,date,Guard);await Guard();history=await periods.GetCompanyPeriodsAsync(company,actor,book,currency,calendar,Guard);await Guard();}
            CompanyFxRateReview? rates=null;if(current.Original.Receivable.CURRENCY_CODE!=currency){rates=originalSource?.Conversion is {} frozen?await _glAccountService.GetOriginalCompanyFxReviewAsync(company,actor,book,current.Original.Receivable.CURRENCY_CODE!,date,frozen.Rates.Window,Guard):await _glAccountService.GetCompanyFxRatesAsync(company,actor,book,current.Original.Receivable.CURRENCY_CODE!,date,Guard);await Guard();}
            // Viewpoint normalization changes read-contract hashes only. All native reads and grants above use the actual actor.
            if(evidenceActor is not null && evidenceActor!=actor){current=CompanySalesReceivableReviewSource.Create(current.Original,current.CurrentInvoice.AsReader(evidenceActor),[current.Original.Receivable]);accounts=CompanyGLMappingReview.Create(CompanyGLAccountReview.Create(company,evidenceActor,book,currency,accounts.Accounts.Accounts),date,accounts.Records);history=CompanyLedgerPeriodReview.Create(company,evidenceActor,book,currency,calendar,history.Periods);}
            if(evidenceActor is not null && evidenceActor!=actor)books=books.AsReader(evidenceActor);
            if(rates is not null && evidenceActor is not null && evidenceActor!=actor)rates=rates.AsReader(evidenceActor);
            var result=CompanySalesJournalSourceReview.Create(current,accounts,history,date,selection,books,rates);await requireFieldRead(result.FieldId);await Guard();return result;
        }
        var initial=await Capture(false);var closing=await Capture(true);await requireFieldRead(closing.FieldId);await Guard();if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete approved receivable/account/mapping/period posting source changed during review.");return CompanyInventorySourceReview.Copy(closing);
    }
}
public partial class ARService
{
    internal async Task RequireJournalSourceBindingAsync(string connection,IDataSource source){var binding=await ReceivableBinding();if(binding.Connection!=connection || !ReferenceEquals(binding.Source,source))throw new InvalidOperationException("The original receivable and journal must use the same PRODUCTION binding.");}
    internal async Task ConfirmCompanyJournalReceivableAsync(CompanySalesReceivableReviewSource expected,string connection,IDataSource source,Func<Task> requireAuthority)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();expected.Require(expected.Company,expected.ActorUserId,expected.ReceivableId);
        if(expected.Original.Receivable.STATUS!="APPROVED")throw new InvalidOperationException("The original independent receivable approval is required before journal source confirmation.");
        async Task Guard(){await requireAuthority();await RequireJournalSourceBindingAsync(connection,source);}
        await Guard();ReceivableSchema(source,false);ReceivableSchema(source,true);ReceivableReceiptSchema(source);ReceivableReviewSchema(source);
        // The complete canonical expected receipts have already been restored and validated. Re-read every actual
        // native artifact and compare its full bytes without repeatedly restoring the same nested command history.
        async Task Match<T>(string table,string ownerColumn,string ownerId,T expectedRow)
        {
            await Guard();var rows=(await OwnedReceivableRepo<T>(connection,table).GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=expected.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=expected.Company.BusinessAssociateId},new(){FieldName=ownerColumn,Operator="=",FilterValue=ownerId}]))?.ToArray();await Guard();
            if(rows is not {Length:1} || rows[0] is not T row || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(expectedRow))throw new InvalidOperationException("The complete original native receivable, preparation, outcome or reviewer artifact is unavailable, ambiguous or changed.");
        }
        var review=CompanySalesReceivableDecisionProof.Require(expected.Company,expected.Original.Receivable);
        async Task Capture()
        {
            await Match("AR_INVOICE","AR_INVOICE_ID",expected.ReceivableId,expected.Original.Receivable);
            await Match("SALES_RECEIVABLE_INTENT","SOURCE_INVOICE_ID",expected.InvoiceId,expected.Original.Intent);
            await Match("SALES_RECEIVABLE_OUTCOME","SOURCE_INVOICE_ID",expected.InvoiceId,expected.Original.Receipt);
            await Match("SALES_RECEIVABLE_REVIEW_INTENT","AR_INVOICE_ID",expected.ReceivableId,review.Intent);
        }
        await Capture();await Capture();expected.Require(expected.Company,expected.ActorUserId,expected.ReceivableId);await Guard();
    }
}
public partial class AccountMappingService
{
    internal async Task RequireJournalSourceBindingAsync(string connection,IDataSource source){if(_resolveConnection is null || await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original account mapping and journal must use the same PRODUCTION binding.");}
}
public partial class PeriodClosingService
{
    internal async Task RequireJournalSourceBindingAsync(string connection,IDataSource source){if(_resolveConnection is null || await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original period and journal must use the same PRODUCTION binding.");}
}

