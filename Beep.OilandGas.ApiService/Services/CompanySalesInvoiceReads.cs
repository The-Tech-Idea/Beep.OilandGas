using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Company/field-authorized billing reads and commands through the existing PRODUCTION owners.</summary>
public sealed partial class CompanySalesInvoiceReads(IInvoiceService invoices,AccountingPolicyService policies,CompanyFieldRevenueReads revenue,ICompanyAccountingReadContext context,IAccessControlService assets)
{
    private async Task<(CompanyScope Company,string Actor,string Binding,Func<Task> Guard)> Authority(ClaimsPrincipal user,bool write,CompanyScope? company=null,string? actor=null,string? binding=null,string? field=null)
    {
        var original=await context.CaptureAsync(user);var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var grants=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task Guard()
        {
            var current=await context.CaptureAsync(user);
            if(!APInvoiceReadChecks.CanRead(user,original.Access.Scope,current.Access) || current.Actor!=original.Actor || current.Binding!=original.Binding ||
                company is not null && company!=current.Access.Scope || actor is not null && actor!=current.Actor || binding is not null && binding!=current.Binding ||
                !roles.SequenceEqual(current.Access.Roles.Order(StringComparer.Ordinal)) || !grants.SequenceEqual(current.Access.Permissions.Order(StringComparer.Ordinal)) ||
                write && !current.Access.Permissions.Contains(PermissionConstants.Accounting.PrepareSalesInvoice,StringComparer.Ordinal) ||
                field is not null && !(await assets.CheckAssetAccessAsync(current.Actor,field,"FIELD","READ")).HasAccess)
                throw new UnauthorizedAccessException("The original company/account/field invoice authority changed.");
            if(!ProcessDefinitionRevision.IsValid(current.Binding))throw new InvalidOperationException("The original production invoice binding is unconfirmed.");
        }
        await Guard();return(original.Access.Scope,original.Actor,original.Binding,Guard);
    }
    public Task<CompanySalesInvoiceWorkspaceReview> ReadAsync(ClaimsPrincipal user,string ticket)=>ReadCore(user,ticket,false);
    private async Task<CompanySalesInvoiceWorkspaceReview> ReadCore(ClaimsPrincipal user,string ticket,bool write)
    {
        if(string.IsNullOrWhiteSpace(ticket) || ticket.Length>128 || ticket.Any(char.IsControl))throw new ArgumentException("A bounded original delivery identity is required.");
        var authority=await Authority(user,write);string? field=null;
        async Task Guard()
        {
            await authority.Guard();if(field is not null && !(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("The original invoice FIELD access changed.");
        }
        async Task<CompanySalesInvoiceWorkspaceReview> Capture()
        {
            await Guard();var delivery=await revenue.ReadPreparedDeliveryAsync(user,ticket);await Guard();
            var source=delivery.Source.Source.Source;
            if(source.Company!=authority.Company || source.ActorUserId!=authority.Actor || delivery.Source.TicketId!=ticket || field is not null && field!=source.FieldId)
                throw new InvalidOperationException("The complete original invoice delivery source changed.");
            field=source.FieldId;await Guard();
            var history=await policies.GetCompanyBillingPoliciesAsync(authority.Company,authority.Actor,Guard);
            var rows=await invoices.GetCompanySalesInvoicesForDeliveryAsync(authority.Company,field,ticket,Guard);
            var review=CompanySalesInvoiceDraftReview.Create(delivery,history,rows,authority.Binding);
            var retained=await invoices.GetCompanyInvoiceDraftIntentForDeliveryAsync(authority.Company,authority.Actor,field,ticket,Guard);
            var outcome=retained is null?null:await invoices.GetCompanyInvoiceDraftOutcomeAsync(authority.Company,authority.Actor,retained.SALES_INVOICE_INTENT_ID,Guard);
            var result=new CompanySalesInvoiceWorkspaceReview(review,retained,outcome);result.Require(authority.Company,authority.Actor,ticket);await Guard();return result;
        }
        var first=await Capture();var closing=await Capture();await Guard();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete original invoice source, policies, command or outcome changed during read-back.");return CompanyInventorySourceReview.Copy(closing);
    }
    public async Task<CompanySalesInvoiceDraftOutcome> PrepareAsync(ClaimsPrincipal user,CompanySalesInvoiceDraftCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();var source=command.Review;
        var authority=await Authority(user,true,source.Company,source.ActorUserId,source.BindingRevision,source.FieldId);
        async Task<CompanySalesInvoiceDraftReview> Current()
        {await authority.Guard();var review=await ReadCore(user,source.TicketId,true);await authority.Guard();return review.Source;}
        var result=await invoices.ExecuteCompanyInvoiceDraftAsync(command,authority.Guard,Current);await authority.Guard();result.Require(command);return result;
    }
    public async Task<CompanySalesInvoiceWorkspaceReview?> ReadIntentAsync(ClaimsPrincipal user,string intentId)
    {
        var authority=await Authority(user,false);var retained=await invoices.GetCompanyInvoiceDraftIntentAsync(authority.Company,authority.Actor,intentId,authority.Guard);await authority.Guard();
        if(retained is null)return null;var command=retained.Restore(authority.Company,authority.Actor);
        var fieldAuthority=await Authority(user,false,authority.Company,authority.Actor,authority.Binding,command.Review.FieldId);
        var review=await ReadCore(user,command.Review.TicketId,false);await fieldAuthority.Guard();
        if(review.Retained is null || JsonSerializer.Serialize(review.Retained)!=JsonSerializer.Serialize(retained))throw new InvalidOperationException("The original invoice intent differs from its complete current workspace.");return review;
    }
}
