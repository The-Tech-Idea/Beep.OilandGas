using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
namespace Beep.OilandGas.Web.Services;
public sealed partial class AccountingServiceClient
{
    private const string CompanySalesReceivableRoute="api/accounting/traditional/company-receivables";
    private async Task<T> ReadReceivable<T>(string invoice,string purpose,Action<T,CompanyScope,string> require)
    {
        if(!Guid.TryParseExact(invoice,"N",out var invoiceGuid) || invoiceGuid==Guid.Empty)throw new ArgumentException("The original invoice identity is required.");var snapshot=_apiClient.CompanySnapshot;if(snapshot.Selection is null)throw new InvalidOperationException("Select the original company and account.");
        var result=await _apiClient.GetAsync<T>(CompanySalesReceivableRoute+"/"+purpose+"/"+Uri.EscapeDataString(invoice));
        if(result is null || snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Complete receivable evidence or the original company/account is unconfirmed.");
        require(result,snapshot.Selection.Scope,snapshot.Selection.UserId);return CompanyInventorySourceReview.Copy(result);
    }
    public Task<CompanySalesReceivableStatusReview> GetCompanySalesReceivableStatusAsync(string invoiceId)=>ReadReceivable<CompanySalesReceivableStatusReview>(invoiceId,"status",(result,scope,actor)=>result.Require(scope,actor,invoiceId));
    public Task<CompanySalesReceivableRecovery> GetCompanySalesReceivableRecoveryAsync(string invoiceId)=>ReadReceivable<CompanySalesReceivableRecovery>(invoiceId,"recovery",(result,scope,actor)=>result.Require(scope,actor,invoiceId));
    public Task<CompanySalesReceivableWorkspaceReview> GetCompanySalesReceivableSourceAsync(string invoiceId)=>ReadReceivable<CompanySalesReceivableWorkspaceReview>(invoiceId,"source",(result,scope,actor)=>result.Require(scope,actor,invoiceId));
    public Task<CompanySalesReceivableReviewSource> GetCompanySalesReceivableReviewAsync(string receivableId)=>ReadReceivable<CompanySalesReceivableReviewSource>(receivableId,"review",(result,scope,actor)=>result.Require(scope,actor,receivableId));
    public Task<CompanySalesReceivableReviewRecovery> GetCompanySalesReceivableReviewRecoveryAsync(string receivableId)=>ReadReceivable<CompanySalesReceivableReviewRecovery>(receivableId,"review-recovery",(result,scope,actor)=>result.Require(scope,actor,receivableId));
    public async Task<CompanySalesReceivableOutcome> RecordCompanySalesReceivableDecisionAsync(CompanySalesReceivableDecisionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var snapshot=_apiClient.CompanySnapshot;
        if(snapshot.Selection is null || snapshot.Selection.Scope!=command.Company || snapshot.Selection.UserId!=command.ActorUserId)throw new InvalidOperationException("Return to the original reviewer company and account to reconcile the decision.");
        var result=await _apiClient.PostAsync<CompanySalesReceivableDecisionCommand,CompanySalesReceivableOutcome>(CompanySalesReceivableRoute+"/decisions",command)??throw new InvalidOperationException("Receivable disposition is unconfirmed. Retain the original reviewer command.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or reviewer account changed during disposition. Retain the original command.");
        RequireReceivableDecision(result,command);return CompanyInventorySourceReview.Copy(result);
    }
    internal static void RequireReceivableDecision(CompanySalesReceivableOutcome result,CompanySalesReceivableDecisionCommand command)
    {
        result.Require(command.Review.Original.Intent.Restore(command.Company,command.Review.Original.Intent.ROW_CREATED_BY));
        if(System.Text.Json.JsonSerializer.Serialize(CompanySalesReceivableDecisionProof.Require(command.Company,result.Receivable).Intent.Restore(command.Company,command.ActorUserId))!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("The returned receivable disposition differs from the exact original reviewer command.");
    }
    public async Task<CompanySalesReceivableOutcome> PrepareCompanySalesReceivableAsync(CompanySalesReceivableCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var snapshot=_apiClient.CompanySnapshot;
        if(snapshot.Selection is null || snapshot.Selection.Scope!=command.Source.Company || snapshot.Selection.UserId!=command.Source.ActorUserId)throw new InvalidOperationException("Return to the original company and account to reconcile the receivable command.");
        var result=await _apiClient.PostAsync<CompanySalesReceivableCommand,CompanySalesReceivableOutcome>(CompanySalesReceivableRoute+"/drafts",command)??throw new InvalidOperationException("Receivable draft outcome is unconfirmed. Retain the original command.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new InvalidOperationException("Company or account changed during preparation. Retain the original command.");result.Require(command);return CompanyInventorySourceReview.Copy(result);
    }
}
