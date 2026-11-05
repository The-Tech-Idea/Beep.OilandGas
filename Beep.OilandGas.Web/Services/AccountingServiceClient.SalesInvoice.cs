using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Web.Services;

public sealed partial class AccountingServiceClient
{
    private const string CompanySalesInvoiceRoute="api/accounting/traditional/company-invoices";
    public async Task<CompanySalesInvoiceQueueReview> GetCompanySalesInvoiceQueueAsync(string fieldId)
    {
        if(string.IsNullOrWhiteSpace(fieldId) || fieldId.Length>128 || fieldId.Any(char.IsControl))throw new ArgumentException("Select a bounded original FIELD.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original company and account.");
        var review=await _apiClient.GetAsync<CompanySalesInvoiceQueueReview>(CompanySalesInvoiceRoute+"/queue/"+Uri.EscapeDataString(fieldId)) ??throw new InvalidOperationException("The complete native invoice queue is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during invoice queue review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,fieldId);return review;
    }
    public async Task<SALES_INVOICE_REVIEW_INTENT?> GetCompanySalesInvoiceReviewIntentAsync(string invoiceId)
    {
        if(!Guid.TryParseExact(invoiceId,"N",out var id) || id==Guid.Empty)throw new InvalidOperationException("The original invoice identity is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original reviewer company and account.");
        var intent=await _apiClient.GetAsync<SALES_INVOICE_REVIEW_INTENT>(CompanySalesInvoiceRoute+"/review-intents/"+Uri.EscapeDataString(invoiceId));
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or reviewer account changed during command recovery.");
        if(intent is not null && intent.Restore(original.Selection.Scope,original.Selection.UserId).InvoiceId!=invoiceId)throw new InvalidOperationException("The retained reviewer command belongs to another invoice.");return intent;
    }
    public async Task<CompanySalesInvoiceDraftOutcome> RecordCompanySalesInvoiceDecisionAsync(CompanySalesInvoiceDecisionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null || original.Selection.Scope!=command.Company || original.Selection.UserId!=command.ActorUserId)throw new InvalidOperationException("Return to the original reviewer company and account to record or reconcile.");
        var result=await _apiClient.PostAsync<CompanySalesInvoiceDecisionCommand,CompanySalesInvoiceDraftOutcome>(CompanySalesInvoiceRoute+"/decisions",command)??throw new InvalidOperationException("Invoice disposition is unconfirmed. Retain the original reviewer command.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or reviewer account changed during disposition. Retain the original command.");
        result.Require(command.Review.Original.Intent.Restore(command.Company,command.Review.Original.Intent.ROW_CREATED_BY));
        if(System.Text.Json.JsonSerializer.Serialize(CompanySalesInvoiceDecisionProof.Require(command.Company,result.Invoice).Intent.Restore(command.Company,command.ActorUserId))!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("The returned disposition differs from the original reviewer command.");return result;
    }
    public async Task<CompanySalesInvoiceReviewSource> GetCompanySalesInvoiceReviewAsync(string invoiceId)
    {
        if(string.IsNullOrWhiteSpace(invoiceId) || invoiceId.Length>128 || invoiceId.Any(char.IsControl))throw new InvalidOperationException("The bounded original invoice identity is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original invoice company and reviewer account.");
        var review=await _apiClient.GetAsync<CompanySalesInvoiceReviewSource>(CompanySalesInvoiceRoute+"/review/"+Uri.EscapeDataString(invoiceId)) ??throw new InvalidOperationException("Complete invoice review evidence is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or reviewer account changed during invoice review.");review.Require(original.Selection.Scope,original.Selection.UserId,invoiceId);return review;
    }
    public async Task<CompanySalesInvoiceWorkspaceReview> GetCompanySalesInvoiceWorkspaceAsync(string ticketId)
    {
        if(string.IsNullOrWhiteSpace(ticketId) || ticketId.Length>128 || ticketId.Any(char.IsControl))throw new InvalidOperationException("The bounded original delivery identity is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original invoice company and account.");
        var review=await _apiClient.GetAsync<CompanySalesInvoiceWorkspaceReview>(CompanySalesInvoiceRoute+"/delivery/"+Uri.EscapeDataString(ticketId)) ??throw new InvalidOperationException("The complete invoice workspace is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during invoice review.");
        review.Require(original.Selection.Scope,original.Selection.UserId,ticketId);return review;
    }
    public async Task<CompanySalesInvoiceWorkspaceReview> GetCompanySalesInvoiceIntentAsync(string intentId)
    {
        if(!Guid.TryParseExact(intentId,"N",out var intent) || intent==Guid.Empty)throw new InvalidOperationException("The original invoice intent identity is required.");
        var original=_apiClient.CompanySnapshot;if(original.Selection is null)throw new InvalidOperationException("Select the original invoice company and account.");
        var review=await _apiClient.GetAsync<CompanySalesInvoiceWorkspaceReview>(CompanySalesInvoiceRoute+"/intents/"+Uri.EscapeDataString(intentId)) ??throw new InvalidOperationException("The original invoice intent is unavailable.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during invoice recovery.");
        review.Require(original.Selection.Scope,original.Selection.UserId,review.Source.TicketId);
        if(review.Retained?.SALES_INVOICE_INTENT_ID!=intentId)throw new InvalidOperationException("The returned invoice command differs from the original intent.");return review;
    }
    public async Task<CompanySalesInvoiceDraftOutcome> PrepareCompanySalesInvoiceAsync(CompanySalesInvoiceDraftCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var original=_apiClient.CompanySnapshot;
        if(original.Selection is null || original.Selection.Scope!=command.Review.Company || original.Selection.UserId!=command.Review.ActorUserId)
            throw new InvalidOperationException("Return to the original invoice company and account to submit or reconcile.");
        var result=await _apiClient.PostAsync<CompanySalesInvoiceDraftCommand,CompanySalesInvoiceDraftOutcome>(CompanySalesInvoiceRoute+"/drafts",command) ??throw new InvalidOperationException("Invoice draft outcome is unconfirmed. Retain the original intent.");
        if(original!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during invoice preparation. Retain the original intent.");result.Require(command);return result;
    }
}
