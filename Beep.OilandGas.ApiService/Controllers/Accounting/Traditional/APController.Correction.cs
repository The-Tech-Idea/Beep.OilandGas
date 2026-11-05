using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;
public sealed partial class APController
{
    [HttpGet("{id}/correction-decision-review")]
    public async Task<ActionResult<APInvoiceCorrectionDecisionReview>> ReviewRecordedCorrection(string id,[FromQuery]string expectedInvoiceRevision,[FromServices]AccountingPolicyService policies)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(!ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))return BadRequest();
        try
        {
            var original=await RequireReadAsync();string[]? roles=null,permissions=null;
            async Task<(RepositoryCompanyAccess Access,string Binding)> Guard()
            {
                var current=await PurchaseContextAsync(original);var nextRoles=current.Access.Roles.Order(StringComparer.Ordinal).ToArray();var nextPermissions=current.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
                if(!APInvoiceReadChecks.CanRead(User,original.Scope,current.Access) ||
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.ReviewInvoiceVariance,StringComparer.Ordinal) ||
                    roles is not null && (!roles.SequenceEqual(nextRoles) || !permissions!.SequenceEqual(nextPermissions)))
                    throw new UnauthorizedAccessException("Current independent company correction review authority is required.");
                roles??=nextRoles;permissions??=nextPermissions;return current;
            }
            async Task<APInvoiceCorrectionDecisionReview> Capture()
            {
                var financial=await APInvoiceReadChecks.ReadFinancialReviewAsync(User,original.Actor,id,expectedInvoiceRevision,Guard,
                    service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync,policies.GetCompanyMatchingPoliciesAsync,service.GetReceiptConsumptionAsync);
                await Guard();return APInvoiceCorrectionDecisionReview.Create(financial,original.Binding);
            }
            var initial=await Capture();var closing=await Capture();
            if(initial.Revision!=closing.Revision)throw new InvalidOperationException("Current correction source or contributor evidence changed during independent review.");
            await Guard();return Ok(initial);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed)
        {logger.LogWarning(changed,"Independent corrected-invoice review could not be confirmed.");return Conflict(new{error="Independent correction review is unavailable or changed. Reload the latest recorded correction and complete current invoice, receipt and policy evidence."});}
    }
    [HttpPost("{id}/correction-commands")]
    public async Task<ActionResult<APInvoiceCorrectionResult>> RecordCorrection(string id,[FromBody]APInvoiceCorrectionSubmission submission,[FromServices]AccountingPolicyService policies)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(submission?.Command is null || submission.Review?.Current?.Invoice is null)return BadRequest();
        try
        {
            submission=PurchaseReceiptDecisionProof.Copy(submission);var command=submission.Command;command.Require(submission.Review);
            var original=await RequireReadAsync();
            if(command.InvoiceId!=id || command.Company!=original.Scope || command.ActorUserId!=original.Actor || command.BindingRevision!=original.Binding)return Forbid();
            var authority=await PurchaseContextAsync(original);var roles=authority.Access.Roles.Order(StringComparer.Ordinal).ToArray();var permissions=authority.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
            async Task<(RepositoryCompanyAccess Access,string Binding)> Guard()
            {
                var current=await PurchaseContextAsync(original);
                if(!APInvoiceReadChecks.CanRead(User,command.Company,current.Access) || !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) ||
                    !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(permissions) ||
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.CorrectInvoice,StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("Current original accountant correction recording authority changed.");
                return current;
            }
            async Task<APInvoiceFinancialReview> Financial()
            {
                await Guard();var source=await service.GetSourceReviewAsync(command.Company,id,command.ActorUserId)??throw new InvalidOperationException("The original correction invoice is unavailable.");
                return await APInvoiceReadChecks.ReadFinancialReviewAsync(User,command.ActorUserId,id,source.Revision,Guard,
                    service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync,policies.GetCompanyMatchingPoliciesAsync,service.GetReceiptConsumptionAsync);
            }
            await Guard();var result=await service.RecordCorrectionAsync(command,submission.Review,async()=>{await Guard();},Financial);
            await Guard();result.Require(submission);return Ok(result);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception changed) when(changed is InvalidOperationException or AggregateException)
        {logger.LogWarning(changed,"Original accountant correction could not be confirmed.");return Conflict(new{error="The original correction is unconfirmed or changed. Retain and reconcile its exact intent, company and account; reload current source and contributor evidence."});}
    }
    [HttpPost("{id}/correction-review")]
    public async Task<ActionResult<APInvoiceCorrectionReview>> ReviewCorrection(string id,[FromBody]APInvoiceCorrectionDraft draft,[FromServices]AccountingPolicyService policies)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(draft is null)return BadRequest();
        try
        {
            draft=PurchaseReceiptDecisionProof.Copy(draft);draft.RequireShape();var original=await RequireReadAsync();
            string[]? roles=null,permissions=null;
            async Task<(RepositoryCompanyAccess Access,string Binding)> Guard()
            {
                var current=await PurchaseContextAsync(original);var nextRoles=current.Access.Roles.Order(StringComparer.Ordinal).ToArray();var nextPermissions=current.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
                if(!APInvoiceReadChecks.CanRead(User,original.Scope,current.Access) ||
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.CorrectInvoice,StringComparer.Ordinal) ||
                    roles is not null && (!roles.SequenceEqual(nextRoles) || !permissions!.SequenceEqual(nextPermissions)))
                    throw new UnauthorizedAccessException("Current original accountant correction authority is required.");
                roles??=nextRoles;permissions??=nextPermissions;return current;
            }
            async Task<APInvoiceCorrectionReview> Capture()
            {
                var financial=await APInvoiceReadChecks.ReadFinancialReviewAsync(User,original.Actor,id,draft.ExpectedInvoiceRevision,Guard,
                    service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync,policies.GetCompanyMatchingPoliciesAsync,service.GetReceiptConsumptionAsync);
                await Guard();return APInvoiceCorrectionReview.Create(financial,draft,original.Binding);
            }
            var initial=await Capture();var closing=await Capture();
            if(initial.Revision!=closing.Revision)throw new InvalidOperationException("Current correction source, controller request or contributor evidence changed during review.");
            await Guard();return Ok(initial);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed)
        {logger.LogWarning(changed,"Accountant invoice correction review could not be confirmed.");return Conflict(new{error="Correction review is unavailable or changed. Reload the recorded controller request and complete current invoice, receipt and policy evidence; provide exact line edits and totals."});}
    }
}
