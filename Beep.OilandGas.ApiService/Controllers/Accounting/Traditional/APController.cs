using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

/// <summary>Existing AP routes use the canonical company-bound asynchronous accounting owner.</summary>
[ApiController]
[Authorize]
[Route("api/accounting/traditional/ap")]
public sealed partial class APController(APInvoiceService service,PurchaseOrderService purchases,ICompanyAccountingReadContext context,ILogger<APController> logger) : ControllerBase
{
    private static string PolicyPermission(string action)=>action switch
    {
        "DRAFT"=>Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.PrepareMatchingPolicy,
        "APPROVE"=>Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.ApproveMatchingPolicy,
        _=>throw new InvalidOperationException("An explicit draft or approval action is required.")
    };
    private async Task<CompanyMatchingPolicyCommandReview> CapturePolicyReview(AccountingPolicyService policies,string? action)
    {
        var original=await context.CaptureAsync(User);
        var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var permissions=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
        async Task Guard()
        {
            var current=await context.CaptureAsync(User);
            if(current.Access.Scope!=original.Access.Scope || current.Actor!=original.Actor || current.Binding!=original.Binding || string.IsNullOrWhiteSpace(current.Binding) ||
                !APInvoiceReadChecks.CanRead(User,original.Access.Scope,current.Access) || !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) ||
                !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(permissions) || action is not null && !current.Access.Permissions.Contains(PolicyPermission(action),StringComparer.Ordinal))
                throw new UnauthorizedAccessException("Original company matching-policy authority changed.");
        }
        await Guard();var initial=await policies.GetCompanyMatchingPoliciesAsync(original.Access.Scope,original.Actor);await Guard();
        var closing=await policies.GetCompanyMatchingPoliciesAsync(original.Access.Scope,original.Actor);await Guard();
        closing.Require(original.Access.Scope,original.Actor);
        if(initial.Revision!=closing.Revision)throw new InvalidOperationException("Company matching policies changed during review.");
        var result=new CompanyMatchingPolicyCommandReview(closing,action ?? "DRAFT",original.Binding);
        if(action is not null)result.Require(original.Access.Scope,original.Actor,action);return result;
    }
    [HttpGet("matching-policies")]
    public async Task<ActionResult<CompanyMatchingPolicyReview>> GetMatchingPolicies([FromServices]AccountingPolicyService policies)
    {
        try{return Ok((await CapturePolicyReview(policies,null)).History);}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company matching-policy evidence could not be confirmed.");return Conflict(new{error="Company matching-policy evidence is unavailable or changed. Review company access, explicit rules and the PRODUCTION schema, then reload."});}
    }
    [HttpGet("matching-policies/command-review")]
    public async Task<ActionResult<CompanyMatchingPolicyCommandReview>> ReviewMatchingPolicy([FromServices]AccountingPolicyService policies,[FromQuery]string action)
    {
        if(action is not ("DRAFT" or "APPROVE"))return BadRequest();
        try{return Ok(await CapturePolicyReview(policies,action));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Original policy command review could not be confirmed.");return Conflict(new{error="The original policy action, history or production binding is unavailable or changed. Reload company policy evidence and authority."});}
    }
    [HttpPost("matching-policies/commands")]
    public async Task<ActionResult<CompanyMatchingPolicyResult>> RecordMatchingPolicy([FromServices]AccountingPolicyService policies,[FromBody]CompanyMatchingPolicySubmission submission)
    {
        if(submission?.Command is null || submission.Review?.History is null || submission.Command.Action is not ("DRAFT" or "APPROVE"))return BadRequest();
        try
        {
            submission=PurchaseReceiptDecisionProof.Copy(submission);var command=submission.Command;
            var original=await context.CaptureAsync(User);
            var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var permissions=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
            async Task Guard()
            {
                var current=await context.CaptureAsync(User);
                if(current.Access.Scope!=command.Company || current.Actor!=command.ActorUserId || current.Binding!=command.BindingRevision ||
                    current.Access.Scope!=original.Access.Scope || current.Actor!=original.Actor || current.Binding!=original.Binding ||
                    !APInvoiceReadChecks.CanRead(User,command.Company,current.Access) || !current.Access.Permissions.Contains(PolicyPermission(command.Action),StringComparer.Ordinal) ||
                    !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(permissions))
                    throw new UnauthorizedAccessException("Original company matching-policy command authority changed.");
            }
            await Guard();submission.Review.Require(command);
            var row=await policies.RecordCompanyMatchingPolicyAsync(command,submission.Review.History,Guard);await Guard();
            var result=new CompanyMatchingPolicyResult(command,row);result.Require(submission);return Ok(result);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception changed) when(changed is InvalidOperationException or AggregateException)
        {logger.LogWarning(changed,"The original matching-policy outcome could not be confirmed.");return Conflict(new{error="Policy outcome is unconfirmed or its reviewed evidence changed. Retain and reconcile the original intent before another policy command."});}
    }
    private async Task<(CompanyScope Scope,string Binding,string Actor)> RequireReadAsync()
    {
        var current=await context.CaptureAsync(User);
        if(!APInvoiceReadChecks.CanRead(User,current.Access.Scope,current.Access))
            throw new UnauthorizedAccessException("Current selected-company roles and Accounting.View are required.");
        return (current.Access.Scope,current.Binding,current.Actor);
    }
    private async Task<(RepositoryCompanyAccess Access,string Binding)> PurchaseContextAsync((CompanyScope Scope,string Binding,string Actor) original)
    {
        var current=await context.CaptureAsync(User);
        if(current.Access.Scope!=original.Scope || current.Binding!=original.Binding || current.Actor!=original.Actor)
            throw new UnauthorizedAccessException("Original company, actor or binding changed.");
        return(current.Access,current.Binding);
    }
    [HttpGet("{id}/lines/{lineId}/purchase-source")]
    public async Task<ActionResult<APLinePurchaseSourceReview>> GetLinePurchaseSource(string id,string lineId,[FromQuery]string expectedInvoiceRevision)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(lineId) || !ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))return BadRequest();
        try
        {
            var original=await RequireReadAsync();var actor=original.Actor;
            var review=await APInvoiceReadChecks.ReadPurchaseSourceAsync(User,actor,id,lineId,expectedInvoiceRevision,
                ()=>PurchaseContextAsync(original),
                service.GetSourceReviewAsync,purchases.GetSourceReviewAsync);
            return Ok(review);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"The AP line purchase source could not be confirmed.");return Conflict(new {error="The recorded AP reference does not have confirmed current owned PO source evidence. Reload the invoice and review company access, references and the PRODUCTION schema."});}
    }
    [HttpGet("{id}/financial-review")]
    public async Task<ActionResult<APInvoiceFinancialReview>> GetFinancialReview(string id,[FromQuery]string expectedInvoiceRevision,[FromServices]AccountingPolicyService policies)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(id) || !ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))return BadRequest();
        try
        {
            var original=await RequireReadAsync();
            return Ok(await APInvoiceReadChecks.ReadFinancialReviewAsync(User,original.Actor,id,expectedInvoiceRevision,()=>PurchaseContextAsync(original),
                service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync,policies.GetCompanyMatchingPoliciesAsync,service.GetReceiptConsumptionAsync));
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"The company invoice financial evidence could not be confirmed.");return Conflict(new{error="Invoice financial source evidence is unavailable or changed. Reload the invoice and review owned purchase/allocation sources and company access."});}
    }
    [HttpPost("{id}/variance-proposal-review")]
    public async Task<ActionResult<APInvoiceVarianceProposalReview>> ReviewVarianceProposal(string id,[FromBody]APInvoiceVarianceProposal proposal,[FromServices]AccountingPolicyService policies)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(proposal is null || !ProcessDefinitionRevision.IsValid(proposal.ExpectedInvoiceRevision) || !ProcessDefinitionRevision.IsValid(proposal.ExpectedFinancialRevision))return BadRequest();
        try
        {
            proposal=PurchaseReceiptDecisionProof.Copy(proposal);var original=await RequireReadAsync();
            async Task<(RepositoryCompanyAccess Access,string Binding)> Guard()
            {
                var current=await PurchaseContextAsync(original);
                if(!current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.PrepareInvoiceVariance,StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("Current original accountant variance preparation authority is required.");
                return current;
            }
            var financial=await APInvoiceReadChecks.ReadFinancialReviewAsync(User,original.Actor,id,proposal.ExpectedInvoiceRevision,Guard,
                service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync,policies.GetCompanyMatchingPoliciesAsync,service.GetReceiptConsumptionAsync);
            await Guard();return Ok(APInvoiceVarianceProposalReview.Create(financial,proposal,original.Binding));
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Accountant invoice variance preparation could not be confirmed.");return Conflict(new{error="Variance preparation is unavailable or changed. Reload complete invoice, policy and receipt evidence; explain every recorded difference and confirm current company preparation authority."});}
    }
    [HttpGet("{id}/variance-decision-review")]
    public async Task<ActionResult<APInvoiceVarianceDecisionReview>> ReviewVarianceDecision(string id,[FromQuery]string expectedInvoiceRevision,[FromServices]AccountingPolicyService policies)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(!ProcessDefinitionRevision.IsValid(expectedInvoiceRevision))return BadRequest();
        try
        {
            var original=await RequireReadAsync();
            string[]? roles=null,permissions=null;
            async Task<(RepositoryCompanyAccess Access,string Binding)> Guard()
            {
                var current=await PurchaseContextAsync(original);
                var currentRoles=current.Access.Roles.Order(StringComparer.Ordinal).ToArray();var currentPermissions=current.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
                if(!APInvoiceReadChecks.CanRead(User,original.Scope,current.Access) ||
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.ReviewInvoiceVariance,StringComparer.Ordinal) ||
                    roles is not null && (!roles.SequenceEqual(currentRoles) || !permissions!.SequenceEqual(currentPermissions)))throw new UnauthorizedAccessException("Current independent variance review authority is required.");
                roles??=currentRoles;permissions??=currentPermissions;
                return current;
            }
            var financial=await APInvoiceReadChecks.ReadFinancialReviewAsync(User,original.Actor,id,expectedInvoiceRevision,Guard,
                service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync,policies.GetCompanyMatchingPoliciesAsync,service.GetReceiptConsumptionAsync);
            var proof=APInvoiceVarianceProof.Require(original.Scope,financial.Invoice.Invoice);
            if(original.Actor==proof.Command.ActorUserId || original.Actor==proof.OriginalInvoice.ROW_CREATED_BY)return Forbid();
            await Guard();return Ok(APInvoiceVarianceDecisionReview.Create(financial,original.Binding));
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Independent invoice variance review could not be confirmed.");return Conflict(new{error="Independent variance review is unavailable or changed. Reload current invoice, recorded proposal and full company evidence."});}
    }
    [HttpPost("{id}/variance-decision-commands")]
    public async Task<ActionResult<APInvoiceVarianceDecisionResult>> RecordVarianceDecision(string id,[FromBody]APInvoiceVarianceDecisionSubmission submission,[FromServices]AccountingPolicyService policies)
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
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.ReviewInvoiceVariance,StringComparer.Ordinal) ||
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.RecordInvoiceVarianceDecision,StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("Original company controller recording authority changed.");
                return current;
            }
            async Task<APInvoiceFinancialReview> Financial()
            {
                await Guard();var source=await service.GetSourceReviewAsync(command.Company,id,command.ActorUserId)??throw new InvalidOperationException("The original controller invoice is unavailable.");
                return await APInvoiceReadChecks.ReadFinancialReviewAsync(User,command.ActorUserId,id,source.Revision,Guard,
                    service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync,policies.GetCompanyMatchingPoliciesAsync,service.GetReceiptConsumptionAsync);
            }
            await Guard();var invoice=await service.RecordVarianceDecisionAsync(command,submission.Review,async()=>{await Guard();},Financial);await Guard();
            var result=new APInvoiceVarianceDecisionResult(command,invoice);result.Require(submission);return Ok(result);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception changed) when(changed is InvalidOperationException or AggregateException){logger.LogWarning(changed,"Original controller disposition could not be confirmed.");return Conflict(new{error="The original controller disposition is unconfirmed or changed. Retain and reconcile its exact intent, company and account."});}
    }
    [HttpPost("{id}/variance-proposal-commands")]
    public async Task<ActionResult<APInvoiceVarianceResult>> RecordVarianceProposal(string id,[FromBody]APInvoiceVarianceSubmission submission,[FromServices]AccountingPolicyService policies)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(submission?.Command is null || submission.Review?.Financial?.Invoice is null)return BadRequest();
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
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.PrepareInvoiceVariance,StringComparer.Ordinal) ||
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.RecordInvoiceVariance,StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("Original company variance submission authority changed.");
                return current;
            }
            async Task<APInvoiceFinancialReview> Financial()
            {
                await Guard();var source=await service.GetSourceReviewAsync(command.Company,id,command.ActorUserId)??throw new InvalidOperationException("The original invoice is unavailable.");
                return await APInvoiceReadChecks.ReadFinancialReviewAsync(User,command.ActorUserId,id,source.Revision,Guard,
                    service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync,policies.GetCompanyMatchingPoliciesAsync,service.GetReceiptConsumptionAsync);
            }
            await Guard();var invoice=await service.RecordVarianceProposalAsync(command,submission.Review,async()=>{await Guard();},Financial);await Guard();
            var result=new APInvoiceVarianceResult(command,invoice);result.Require(submission);return Ok(result);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception changed) when(changed is InvalidOperationException or AggregateException){logger.LogWarning(changed,"Original invoice variance submission could not be confirmed.");return Conflict(new{error="The original variance proposal outcome is unconfirmed or changed. Retain and reconcile the original intent, company and account before another submission."});}
    }
    [HttpGet("{id}/lines/{lineId}/receipt-allocations")]
    public async Task<ActionResult<APReceiptAllocationReview>> GetReceiptAllocations(string id,string lineId,
        [FromQuery]string expectedInvoiceRevision,[FromQuery]string expectedOrderRevision)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(lineId) ||
            !ProcessDefinitionRevision.IsValid(expectedInvoiceRevision) || !ProcessDefinitionRevision.IsValid(expectedOrderRevision))return BadRequest();
        try
        {
            var original=await RequireReadAsync();var actor=original.Actor;
            var review=await APInvoiceReadChecks.ReadReceiptAllocationsAsync(User,actor,id,lineId,expectedInvoiceRevision,expectedOrderRevision,
                ()=>PurchaseContextAsync(original),
                service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync);
            return Ok(review);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Recorded receipt allocations could not be confirmed.");return Conflict(new {error="Recorded receipt allocation evidence is unavailable or changed. Reload invoice and order sources and review the PRODUCTION schema and access."});}
    }
    [HttpPost("{id}/lines/{lineId}/receipt-allocation-review")]
    public async Task<ActionResult<APReceiptAllocationProposalReview>> ReviewReceiptAllocation(string id,string lineId,[FromBody]APReceiptAllocationProposal proposal)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(proposal is null || !ProcessDefinitionRevision.IsValid(proposal.ExpectedInvoiceRevision) ||
            !ProcessDefinitionRevision.IsValid(proposal.ExpectedOrderRevision) || !ProcessDefinitionRevision.IsValid(proposal.ExpectedAllocationRevision))return BadRequest();
        try
        {
            var original=await RequireReadAsync();
            async Task<(RepositoryCompanyAccess Access,string Binding)> Current()
            {
                var current=await PurchaseContextAsync(original);
                if(!APInvoiceReadChecks.CanRead(User,original.Scope,current.Access) || !PurchaseOrderReadChecks.CanRead(User,original.Scope,current.Access) ||
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.PrepareReceiptAllocation,StringComparer.Ordinal))
                    throw new UnauthorizedAccessException("Current company receipt-allocation preparation authority is required.");
                return current;
            }
            var source=await APInvoiceReadChecks.ReadReceiptAllocationsAsync(User,original.Actor,id,lineId,proposal.ExpectedInvoiceRevision,proposal.ExpectedOrderRevision,
                Current,service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync);
            proposal.Require(original.Scope,original.Actor,id,lineId,source);
            async Task Guard(){_=await Current();}
            var initialConsumption=await service.GetReceiptConsumptionAsync(original.Scope,original.Actor,source.Source,proposal.ReceiptId,Guard);
            var closingSource=await APInvoiceReadChecks.ReadReceiptAllocationsAsync(User,original.Actor,id,lineId,proposal.ExpectedInvoiceRevision,proposal.ExpectedOrderRevision,
                Current,service.GetSourceReviewAsync,purchases.GetSourceReviewAsync,service.GetReceiptAllocationsForLineAsync);
            proposal.Require(original.Scope,original.Actor,id,lineId,closingSource);
            var closingConsumption=await service.GetReceiptConsumptionAsync(original.Scope,original.Actor,closingSource.Source,proposal.ReceiptId,Guard);
            if(initialConsumption.Revision!=closingConsumption.Revision)throw new InvalidOperationException("Recorded receipt contributors changed during review.");
            return Ok(APReceiptAllocationProposalReview.Create(original.Scope,original.Actor,id,lineId,proposal,closingSource,closingConsumption,original.Binding));
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Allocation preparation source could not be confirmed.");return Conflict(new {error="The allocation proposal does not have confirmed current receipt, quantity, unit or source evidence. Reload the sources and review the proposal."});}
    }
    [HttpGet("{id}/lines/{lineId}/receipt-consumption")]
    public async Task<ActionResult<APReceiptConsumptionReview>> GetReceiptConsumption(string id,string lineId,[FromQuery]string receiptId,
        [FromQuery]string expectedInvoiceRevision,[FromQuery]string expectedOrderRevision)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(receiptId) || receiptId.Length>128 || receiptId.Any(char.IsControl) ||
            !ProcessDefinitionRevision.IsValid(expectedInvoiceRevision) || !ProcessDefinitionRevision.IsValid(expectedOrderRevision))return BadRequest();
        try
        {
            var original=await context.CaptureAsync(User);var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var permissions=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
            async Task<(RepositoryCompanyAccess Access,string Binding)> Current()
            {
                var value=await context.CaptureAsync(User);
                if(value.Access.Scope!=original.Access.Scope || value.Actor!=original.Actor || value.Binding!=original.Binding || string.IsNullOrWhiteSpace(value.Binding) ||
                    !APInvoiceReadChecks.CanRead(User,original.Access.Scope,value.Access) || !PurchaseOrderReadChecks.CanRead(User,original.Access.Scope,value.Access) ||
                    !value.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !value.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(permissions))
                    throw new UnauthorizedAccessException("Original company receipt consumption read authority changed.");
                return(value.Access,value.Binding);
            }
            async Task Guard(){_=await Current();}
            async Task<APReceiptConsumptionReview> Capture()
            {
                var purchase=await APInvoiceReadChecks.ReadPurchaseSourceAsync(User,original.Actor,id,lineId,expectedInvoiceRevision,Current,service.GetSourceReviewAsync,purchases.GetSourceReviewAsync);
                if(purchase.Order.Revision!=expectedOrderRevision)throw new InvalidOperationException("The original purchase source changed.");
                return await service.GetReceiptConsumptionAsync(original.Access.Scope,original.Actor,purchase,receiptId,Guard);
            }
            await Guard();var initial=await Capture();var closing=await Capture();await Guard();
            if(initial.Revision!=closing.Revision)throw new InvalidOperationException("Receipt contributors changed during read-back.");return Ok(closing);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Receipt consumption evidence could not be confirmed.");return Conflict(new{error="Receipt contributors are unavailable or changed. Reload original invoice and purchase sources and company access."});}
    }
    [HttpPost("{id}/lines/{lineId}/receipt-allocations")]
    public async Task<ActionResult<APReceiptAllocationReceipt>> RecordReceiptAllocation(string id,string lineId,[FromBody]APReceiptAllocationSubmission submission)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(submission?.Command is null || submission.Review is null || submission.Command.InvoiceId!=id || submission.Command.LineId!=lineId)return BadRequest();
        try
        {
            // Original body evidence cannot change while upstream authority or providers are awaited.
            submission=System.Text.Json.JsonSerializer.Deserialize<APReceiptAllocationSubmission>(System.Text.Json.JsonSerializer.Serialize(submission))!;
            var command=submission.Command;var review=submission.Review;
            var original=await context.CaptureAsync(User);
            var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();
            var permissions=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
            async Task Guard()
            {
                var current=await context.CaptureAsync(User);
                if(current.Access.Scope!=command.Company || current.Actor!=command.ActorUserId || current.Binding!=command.BindingRevision ||
                    current.Access.Scope!=original.Access.Scope || current.Actor!=original.Actor || current.Binding!=original.Binding ||
                    !APInvoiceReadChecks.CanRead(User,command.Company,current.Access) || !PurchaseOrderReadChecks.CanRead(User,command.Company,current.Access) ||
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Accounting.RecordReceiptAllocation,StringComparer.Ordinal) ||
                    !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(permissions))
                    throw new UnauthorizedAccessException("Original company allocation recording authority changed or is unavailable.");
            }
            await Guard();command.Require(review);
            var row=await service.RecordReceiptAllocationAsync(command,review,Guard,async()=>
            {
                await Guard();var line=review.Source.Source.Invoice.Lines.Single(item=>item.AP_INVOICE_LINE_ID==lineId);
                return await purchases.GetSourceReviewAsync(command.Company,line.PURCHASE_ORDER_ID!,command.ActorUserId)
                    ?? throw new InvalidOperationException("The original owned purchase source is unavailable.");
            });
            await Guard();return Ok(APReceiptAllocationReceipt.Create(command,review,row));
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"The original reviewed allocation outcome could not be confirmed.");return Conflict(new{error="Allocation outcome is unconfirmed or its original evidence changed. Reconcile the original intent and review sources and company access before another allocation."});}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException refused){logger.LogWarning(refused,"The allocation provider refused the original command.");return Conflict(new{error="Allocation outcome could not be confirmed. Reconcile the original intent before another allocation."});}
        catch(AggregateException uncertain){logger.LogWarning(uncertain,"The allocation transaction outcome or rollback was not confirmed.");return Conflict(new{error="Allocation transaction outcome is unconfirmed. Reconcile the original intent before another allocation."});}
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<APInvoiceSourceReview>> GetAPInvoice(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(id))return BadRequest(new {error="AP invoice ID is required."});
        try
        {
            var original=await RequireReadAsync();var scope=original.Scope;
            var actor=original.Actor;var initial=await service.GetSourceReviewAsync(scope,id,actor);if(initial is null)return NotFound();
            var revision=initial.Revision;
            var closing=await service.GetSourceReviewAsync(scope,id,actor);
            if(closing is null || closing.Revision!=revision ||
                await RequireReadAsync()!=original)return Conflict(new {error="Company, binding or AP invoice changed. Reload before continuing."});
            return Ok(closing);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company-owned AP invoice evidence could not be confirmed.");return Conflict(new {error="AP invoice source is unconfirmed. Review header ownership and recorded invoice evidence and the PRODUCTION schema."});}
    }
    [HttpGet]
    public async Task<ActionResult<List<AP_INVOICE>>> GetCompanyInvoices([FromQuery]string? vendorId=null)
    {
        if(Request.Query.TryGetValue(nameof(vendorId),out var suppliedVendor) &&
            (suppliedVendor.Count!=1 || string.IsNullOrWhiteSpace(vendorId)))return BadRequest();
        try
        {
            var original=await RequireReadAsync();var scope=original.Scope;
            var rows=await service.GetCompanyBillsAsync(scope,vendorId);
            if(await RequireReadAsync()!=original)
                return Conflict(new {error="Company or production binding changed during the AP invoice read."});
            return Ok(rows);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company AP queue evidence could not be confirmed.");return Conflict(new {error="AP invoice ownership or schema is unconfirmed. Review the PRODUCTION migration and company assignment."});}
    }
    [HttpGet("vendor/{vendorId}/invoices")]
    public async Task<ActionResult<List<AP_INVOICE>>> GetVendorInvoices(string vendorId)
    {
        try{vendorId=RecordedSourceRouteIds.Read(Request,nameof(vendorId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(vendorId))return BadRequest();
        try
        {
            var original=await RequireReadAsync();var scope=original.Scope;var rows=await service.GetVendorBillsAsync(scope,vendorId);
            if(await RequireReadAsync()!=original)return Conflict(new {error="Company or production binding changed during the AP invoice read."});
            return Ok(rows);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company-owned AP invoice evidence could not be confirmed.");return Conflict(new {error="AP invoice ownership or schema is unconfirmed. Review the PRODUCTION migration and company assignment."});}
    }
    // An unreviewed legacy request cannot create company-less AP data through the overlapping traditional owner.
    [HttpPost]
    public IActionResult CreateAPInvoice()=>Conflict(new {error="Reviewed company-owned AP invoice creation, independent approval/posting and command recovery are not implemented. No AP invoice was created."});
}
