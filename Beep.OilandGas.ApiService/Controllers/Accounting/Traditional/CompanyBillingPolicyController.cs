using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

[ApiController]
[Authorize]
[Route("api/accounting/traditional/billing-policies")]
public sealed class CompanyBillingPolicyController(AccountingPolicyService policies,ICompanyAccountingReadContext context,ILogger<CompanyBillingPolicyController> logger):ControllerBase
{
    private static string Permission(string action)=>action switch
    {"DRAFT"=>PermissionConstants.Accounting.PrepareBillingPolicy,"APPROVE"=>PermissionConstants.Accounting.ApproveBillingPolicy,_=>throw new InvalidOperationException("An explicit billing policy action is required.")};
    private async Task<Func<Task>> Authority(string? action,CompanyScope? company=null,string? actor=null,string? binding=null)
    {
        var original=await context.CaptureAsync(User);var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var grants=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task Guard()
        {
            var current=await context.CaptureAsync(User);
            if(!APInvoiceReadChecks.CanRead(User,original.Access.Scope,current.Access) || current.Actor!=original.Actor || current.Binding!=original.Binding ||
                company is not null && company!=current.Access.Scope || actor is not null && actor!=current.Actor || binding is not null && binding!=current.Binding ||
                !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(grants) ||
                action is not null && !current.Access.Permissions.Contains(Permission(action),StringComparer.Ordinal))throw new UnauthorizedAccessException("The original company billing policy authority changed.");
            if(!ProcessDefinitionRevision.IsValid(current.Binding))throw new InvalidOperationException("The production binding is unconfirmed.");
        }
        await Guard();return Guard;
    }
    private async Task<CompanyBillingPolicyCommandReview> Capture(string? action)
    {
        var original=await context.CaptureAsync(User);var guard=await Authority(action,original.Access.Scope,original.Actor,original.Binding);
        var initial=await policies.GetCompanyBillingPoliciesAsync(original.Access.Scope,original.Actor,guard);
        var closing=await policies.GetCompanyBillingPoliciesAsync(original.Access.Scope,original.Actor,guard);await guard();
        if(initial.Revision!=closing.Revision)throw new InvalidOperationException("The complete billing policy history changed during review.");
        var review=new CompanyBillingPolicyCommandReview(closing,action ??"DRAFT",original.Binding);review.Require(original.Access.Scope,original.Actor,action ??"DRAFT");return review;
    }
    [HttpGet]
    public Task<ActionResult> Read()=>Execute(async()=>Ok((await Capture(null)).History));
    [HttpGet("command-review")]
    public Task<ActionResult> Review([FromQuery]string action)=>action is not ("DRAFT" or "APPROVE")?Task.FromResult<ActionResult>(BadRequest()):Execute(async()=>Ok(await Capture(action)));
    [HttpPost("drafts")]
    public Task<ActionResult> Prepare([FromBody]CompanyBillingPolicyDraftSubmission submission)
    {
        if(submission?.Command is null || submission.Review?.History is null)return Task.FromResult<ActionResult>(BadRequest());
        return Execute(async()=>
        {
            submission=PurchaseReceiptDecisionProof.Copy(submission);var command=submission.Command;
            var guard=await Authority("DRAFT",command.Company,command.ActorUserId,command.BindingRevision);submission.Review.Require(command);
            var result=new CompanyBillingPolicyResult(await policies.PrepareCompanyBillingPolicyAsync(command,submission.Review.History,guard));await guard();result.Require(submission);return Ok(result);
        });
    }
    [HttpPost("approvals")]
    public Task<ActionResult> Approve([FromBody]CompanyBillingPolicyApprovalSubmission submission)
    {
        if(submission?.Command is null || submission.Review?.History is null)return Task.FromResult<ActionResult>(BadRequest());
        return Execute(async()=>
        {
            submission=PurchaseReceiptDecisionProof.Copy(submission);var command=submission.Command;
            var guard=await Authority("APPROVE",command.Company,command.ActorUserId,command.BindingRevision);submission.Review.Require(command);
            var result=new CompanyBillingPolicyResult(await policies.ApproveCompanyBillingPolicyAsync(command,submission.Review.History,guard));await guard();result.Require(submission);return Ok(result);
        });
    }
    private async Task<ActionResult> Execute(Func<Task<ActionResult>> action)
    {
        try{return await action();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception changed) when(changed is InvalidOperationException or AggregateException)
        {logger.LogWarning(changed,"The original company billing policy evidence or outcome is unconfirmed.");return Conflict(new{error="Billing policy evidence or outcome is unconfirmed. Retain and reconcile the original intent; review company authority, explicit rules and Beep schema before another command."});}
    }
}
