using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

/// <summary>Existing PO routes use the canonical company-bound asynchronous accounting owner.</summary>
[ApiController]
[Authorize]
[Route("api/accounting/traditional/purchase-order")]
public sealed class PurchaseOrderController(PurchaseOrderService service,ICompanyAccountingReadContext context,ILogger<PurchaseOrderController> logger) : ControllerBase
{
    private async Task<(CompanyScope Scope,string Binding,string Actor)> RequireReadAsync(PurchaseOrderSourceReview? source=null)
    {
        var current=await context.CaptureAsync(User);
        if(!PurchaseOrderReadChecks.CanRead(User,current.Access.Scope,current.Access))
            throw new UnauthorizedAccessException("Current selected-company roles and Procurement.View are required.");
        if(source is not null)PurchaseOrderReadChecks.RequireInventoryEvidence(source,current.Access);
        return (current.Access.Scope,current.Binding,current.Actor);
    }
    [HttpGet("{id}/receipts/{receiptId}/decision-review")]
    public async Task<ActionResult<PurchaseReceiptDecisionReview>> GetReceiptDecisionReview(string id,string receiptId)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));receiptId=RecordedSourceRouteIds.Read(Request,nameof(receiptId));}catch(FormatException){return BadRequest();}
        try
        {
            var original=await RequireReadAsync();
            var initial=await service.GetSourceReviewAsync(original.Scope,id,original.Actor) ?? throw new InvalidOperationException("The owned purchase order is unavailable.");
            var closing=await service.GetSourceReviewAsync(original.Scope,id,original.Actor) ?? throw new InvalidOperationException("The owned purchase order is unavailable.");
            if(initial.Revision!=closing.Revision || await RequireReadAsync(closing)!=original)throw new InvalidOperationException("The original receiving source or binding changed.");
            var review=new PurchaseReceiptDecisionReview(closing,receiptId,original.Binding);review.Require(original.Scope,original.Actor,id,receiptId);return Ok(review);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Receiver review could not be confirmed.");return Conflict(new{error="Receiver source or company access changed. Reload the original order and receipt."});}
    }
    [HttpPost("{id}/receipts/{receiptId}/decision")]
    public async Task<ActionResult<PurchaseReceiptDecisionResult>> RecordReceiptDecision(string id,string receiptId,[FromBody]PurchaseReceiptDecisionSubmission submission)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));receiptId=RecordedSourceRouteIds.Read(Request,nameof(receiptId));}catch(FormatException){return BadRequest();}
        if(submission?.Command is null || submission.Review is null || submission.Command.OrderId!=id || submission.Command.ReceiptId!=receiptId)return BadRequest();
        try
        {
            submission=PurchaseReceiptDecisionProof.Copy(submission);var command=submission.Command;
            var original=await context.CaptureAsync(User);var roles=original.Access.Roles.Order(StringComparer.Ordinal).ToArray();var permissions=original.Access.Permissions.Order(StringComparer.Ordinal).ToArray();
            async Task Guard()
            {
                var current=await context.CaptureAsync(User);
                PurchaseOrderReadChecks.RequireInventoryEvidence(submission.Review.Source,current.Access);
                if(current.Access.Scope!=command.Company || current.Actor!=command.ActorUserId || current.Binding!=command.BindingRevision ||
                    current.Access.Scope!=original.Access.Scope || current.Actor!=original.Actor || current.Binding!=original.Binding ||
                    !PurchaseOrderReadChecks.CanRead(User,command.Company,current.Access) ||
                    !current.Access.Permissions.Contains(Beep.OilandGas.Models.Data.Security.PermissionConstants.Procurement.RecordReceiptDecision,StringComparer.Ordinal) ||
                    !current.Access.Roles.Order(StringComparer.Ordinal).SequenceEqual(roles) || !current.Access.Permissions.Order(StringComparer.Ordinal).SequenceEqual(permissions))
                    throw new UnauthorizedAccessException("Original company receiver authority changed or is unavailable.");
            }
            await Guard();submission.Review.Require(command);
            var row=await service.RecordReceiptDecisionAsync(command,submission.Review.Source,Guard);await Guard();
            return Ok(PurchaseReceiptDecisionResult.Create(submission,row));
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Receiver outcome could not be confirmed.");return Conflict(new{error="Receiver outcome is unconfirmed or original evidence changed. Reconcile the original intent before another decision."});}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException refused){logger.LogWarning(refused,"Receiver provider refused the command.");return Conflict(new{error="Receiver outcome is unconfirmed. Reconcile the original intent before another decision."});}
        catch(AggregateException uncertain){logger.LogWarning(uncertain,"Receiver transaction or rollback is unconfirmed.");return Conflict(new{error="Receiver outcome is unconfirmed. Reconcile the original intent before another decision."});}
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseOrderSourceReview>> GetPurchaseOrder(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(id))return BadRequest(new {error="Purchase order ID is required."});
        try
        {
            var original=await RequireReadAsync();var scope=original.Scope;
            var actor=original.Actor;var initial=await service.GetSourceReviewAsync(scope,id,actor);if(initial is null)return NotFound();
            var revision=initial.Revision;
            var closing=await service.GetSourceReviewAsync(scope,id,actor);
            if(closing is null || closing.Revision!=revision ||
                await RequireReadAsync(closing)!=original)return Conflict(new {error="Company, binding or purchase order changed. Reload before continuing."});
            return Ok(closing);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company-owned purchase-order evidence could not be confirmed.");return Conflict(new {error="Purchase-order source is unconfirmed. Review header ownership, recorded line/receipt relationships and the PRODUCTION schema."});}
    }
    [HttpGet]
    public async Task<ActionResult<List<PURCHASE_ORDER>>> GetCompanyOrders([FromQuery]string? vendorId=null)
    {
        if(Request.Query.TryGetValue(nameof(vendorId),out var suppliedVendor) &&
            (suppliedVendor.Count!=1 || string.IsNullOrWhiteSpace(vendorId)))return BadRequest();
        try
        {
            var original=await RequireReadAsync();var scope=original.Scope;var rows=await service.GetCompanyPOsAsync(scope,vendorId);
            if(await RequireReadAsync()!=original)return Conflict(new {error="Company or production binding changed during the order read."});
            return Ok(rows);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company order queue could not be confirmed.");return Conflict(new {error="Purchase-order ownership or schema is unconfirmed. Review the PRODUCTION migration and company assignment."});}
    }
    [HttpGet("vendor/{vendorId}/orders")]
    public async Task<ActionResult<List<PURCHASE_ORDER>>> GetVendorOrders(string vendorId)
    {
        try{vendorId=RecordedSourceRouteIds.Read(Request,nameof(vendorId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(vendorId))return BadRequest();
        try
        {
            var original=await RequireReadAsync();var scope=original.Scope;var rows=await service.GetVendorPOsAsync(scope,vendorId);
            if(await RequireReadAsync()!=original)return Conflict(new {error="Company or production binding changed during the purchase-order read."});
            return Ok(rows);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company-owned purchase-order evidence could not be confirmed.");return Conflict(new {error="Purchase-order ownership or schema is unconfirmed. Review the PRODUCTION migration and company assignment."});}
    }
    // An unreviewed legacy request cannot create company-less PO data through the overlapping traditional owner.
    [HttpPost]
    public IActionResult CreatePurchaseOrder()=>Conflict(new {error="Reviewed company-owned purchase-order creation, duties and command recovery are not implemented. No purchase order was created."});
}
