using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Security;
namespace Beep.OilandGas.ApiService.Services;

/// <summary>Purpose-authorized reads and draft commands through the existing native PRODUCTION owners.</summary>
public sealed partial class CompanySalesReceivableReads(IARService receivables,IInvoiceService invoices,CompanySalesInvoiceReads invoiceReads,ICompanyAccountingReadContext context,IAccessControlService assets)
{
    private async Task<(CompanyScope Company,string Actor,string Binding,Func<Task> Guard)> Authority(ClaimsPrincipal user,bool prepare=false,CompanySalesReceivableSource? expected=null,bool write=false)
    {
        var original=await context.CaptureAsync(user);var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var grants=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task Guard()
        {
            var current=await context.CaptureAsync(user);
            if(!APInvoiceReadChecks.CanRead(user,original.Access.Scope,current.Access) || current.Actor!=original.Actor || current.Binding!=original.Binding ||
                !roles.SequenceEqual(current.Access.Roles.Order(StringComparer.Ordinal)) || !grants.SequenceEqual(current.Access.Permissions.Order(StringComparer.Ordinal)) ||
                prepare && !current.Access.Permissions.Contains(PermissionConstants.Accounting.PrepareSalesReceivable,StringComparer.Ordinal) ||
                expected is not null && (expected.Company!=current.Access.Scope || expected.ActorUserId!=current.Actor || expected.BindingRevision!=current.Binding))
                throw new UnauthorizedAccessException("The original company/account receivable authority changed.");
            if(!ProcessDefinitionRevision.IsValid(current.Binding))throw new InvalidOperationException("The original production binding is unconfirmed.");
            if(expected is not null && (!(await assets.CheckAssetAccessAsync(current.Actor,expected.FieldId,"FIELD","READ")).HasAccess ||
                write && !(await assets.CheckAssetAccessAsync(current.Actor,expected.FieldId,"FIELD","WRITE")).HasAccess))throw new UnauthorizedAccessException("Current original invoice FIELD authority is required.");
        }
        await Guard();return(original.Access.Scope,original.Actor,original.Binding,Guard);
    }
    private static async Task<T> Stable<T>(Func<Task<T>> capture,Func<Task> guard)
    {
        var first=await capture();var closing=await capture();await guard();
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete original invoice and receivable evidence changed during read-back.");
        return CompanyInventorySourceReview.Copy(closing);
    }
    public async Task<CompanySalesReceivableStatusReview> ReadStatusAsync(ClaimsPrincipal user,string invoice)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        if(!Guid.TryParseExact(invoice,"N",out var invoiceGuid) || invoiceGuid==Guid.Empty)throw new ArgumentException("The original invoice identity is required.");var authority=await Authority(user);string? field=null;
        async Task Guard(){await authority.Guard();if(field is not null && !(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current invoice FIELD READ authority is required.");}
        async Task Field(string value){if(field is not null && field!=value)throw new InvalidOperationException("The original invoice FIELD changed.");field=value;await Guard();}
        async Task<CompanySalesReceivableStatusReview> Capture()
        {
            await Guard();var original=await invoices.GetCompanyInvoiceDraftForReviewAsync(authority.Company,invoice,Guard,Field);
            var rows=await receivables.GetCompanySalesReceivablesAsync(authority.Company,field!,invoice,Guard);var outcomes=new List<CompanySalesReceivableOutcome>();
            foreach(var row in rows)
            {
                var outcome=await receivables.GetCompanySalesReceivableOutcomeAsync(authority.Company,row.ROW_CREATED_BY,invoice,Guard)??throw new InvalidOperationException("The recorded receivable receipt is missing.");
                if(JsonSerializer.Serialize(outcome.Receivable)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The recorded native receivable differs from its original outcome.");outcomes.Add(outcome);
            }
            var result=CompanySalesReceivableStatusReview.Create(authority.Company,authority.Actor,authority.Binding,original,outcomes);await Guard();return result;
        }
        return await Stable(Capture,Guard);
    }
    public async Task<CompanySalesReceivableRecovery> ReadRecoveryAsync(ClaimsPrincipal user,string invoice)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        if(!Guid.TryParseExact(invoice,"N",out var invoiceGuid) || invoiceGuid==Guid.Empty)throw new ArgumentException("The original invoice identity is required.");var authority=await Authority(user);string? field=null;string? parent=null;
        async Task Guard(){await authority.Guard();if(field is not null && !(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current invoice FIELD READ authority is required.");}
        async Task Field(string value){if(field is not null && field!=value)throw new InvalidOperationException("The original invoice FIELD changed.");field=value;await Guard();}
        async Task<CompanySalesReceivableRecovery> Capture()
        {
            await Guard();var original=await invoices.GetCompanyInvoiceDraftForReviewAsync(authority.Company,invoice,Guard,Field);var current=JsonSerializer.Serialize(original);
            if(parent is not null && parent!=current)throw new InvalidOperationException("The original invoice changed during recovery.");parent=current;
            var retained=await receivables.GetCompanySalesReceivableIntentAsync(authority.Company,authority.Actor,invoice,Guard);
            var outcome=await receivables.GetCompanySalesReceivableOutcomeAsync(authority.Company,authority.Actor,invoice,Guard);
            var result=new CompanySalesReceivableRecovery(authority.Company,authority.Actor,invoice,retained,outcome);result.Require(authority.Company,authority.Actor,invoice);await Guard();return result;
        }
        return await Stable(Capture,Guard);
    }
    public async Task<CompanySalesReceivableWorkspaceReview> ReadSourceAsync(ClaimsPrincipal user,string invoice)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        if(!Guid.TryParseExact(invoice,"N",out var invoiceGuid) || invoiceGuid==Guid.Empty)throw new ArgumentException("The original invoice identity is required.");var authority=await Authority(user,true);string? field=null;
        async Task Guard(){await authority.Guard();if(field is not null && !(await assets.CheckAssetAccessAsync(authority.Actor,field,"FIELD","READ")).HasAccess)throw new UnauthorizedAccessException("Current invoice FIELD READ authority is required.");}
        async Task<CompanySalesReceivableWorkspaceReview> Capture()
        {
            await Guard();var original=await invoiceReads.ReadReceivableSourceAsync(user,invoice);
            if(field is not null && field!=original.Current.FieldId)throw new InvalidOperationException("The original receivable source FIELD changed.");field=original.Current.FieldId;await Guard();
            var rows=await receivables.GetCompanySalesReceivablesAsync(authority.Company,field,invoice,Guard);
            var result=new CompanySalesReceivableWorkspaceReview(CompanySalesReceivableSource.Create(original,rows),await ReadRecoveryAsync(user,invoice));
            result.Require(authority.Company,authority.Actor,invoice);await Guard();return result;
        }
        return await Stable(Capture,Guard);
    }
    public async Task<CompanySalesReceivableOutcome> PrepareAsync(ClaimsPrincipal user,CompanySalesReceivableCommand command)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();
        ArgumentNullException.ThrowIfNull(command);command=CompanyInventorySourceReview.Copy(command);command.Require();
        var authority=await Authority(user,true,command.Source,true);
        async Task<CompanySalesReceivableSource> Current(){await authority.Guard();var loaded=await ReadSourceAsync(user,command.Source.InvoiceId);await authority.Guard();return loaded.Source;}
        var result=await receivables.PrepareCompanySalesReceivableAsync(command,authority.Guard,Current);await authority.Guard();result.Require(command);return result;
    }
}
