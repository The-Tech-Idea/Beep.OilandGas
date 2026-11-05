using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

[ApiController]
[Route("api/accounting/traditional/inventory")]
public partial class InventoryController : ControllerBase
{
    [Authorize]
    [HttpGet("company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-history")]
    public async Task<ActionResult<PurchaseOrderInventoryLinkCycleResult>> GetInventoryLinkHistory(string orderId,string lineId,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(lineId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetLinkHistoryAsync(User,orders,orderId,lineId));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Complete owned cycle history is unconfirmed. Review the original order, history cursor and current company binding."});}
    }
    [Authorize]
    [HttpGet("company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-cycle-review/{cycleAction}")]
    public async Task<ActionResult<PurchaseOrderInventoryLinkCycleReview>> GetInventoryLinkCycleReview(string orderId,string lineId,PurchaseOrderInventoryLinkCycleAction cycleAction,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(lineId) || !Enum.IsDefined(cycleAction))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetLinkCycleReviewAsync(User,orders,orderId,lineId,cycleAction));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Cycle reopening review is unconfirmed. Review the explicit action, original line and complete retained history."});}
    }
    [Authorize]
    [HttpPost("company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-cycle")]
    public async Task<ActionResult<PurchaseOrderInventoryLinkCycleResult>> ReopenInventoryLink(string orderId,string lineId,[FromBody]PurchaseOrderInventoryLinkCycleSubmission submission,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(submission is null || string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(lineId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).ReopenLinkAsync(User,orders,orderId,lineId,submission));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Cycle reopening is unconfirmed. Retain the original action, intent, reason and full review for reconciliation."});}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException){return Conflict(new{error="The provider refused the original cycle. Reconcile the original intent."});}
        catch(AggregateException){return Conflict(new{error="Cycle commit or rollback is unconfirmed. Reconcile the original intent before another cycle."});}
    }
    [Authorize]
    [HttpPost("company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-resolution")]
    public async Task<ActionResult<PurchaseOrderInventoryLinkResolution>> GetInventoryLinkResolution(string orderId,string lineId,[FromBody]PurchaseOrderInventoryLinkProposalSubmission submission,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(submission is null || string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(lineId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetLinkResolutionAsync(User,orders,orderId,lineId,submission));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="The original preparation has no confirmed unchanged independent resolution. Retain its original intent."});}
    }
    [Authorize]
    [HttpGet("company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-decision-review")]
    public async Task<ActionResult<PurchaseOrderInventoryLinkDecisionReview>> GetInventoryLinkDecisionReview(string orderId,string lineId,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(lineId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetLinkDecisionAsync(User,orders,orderId,lineId));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Independent inventory-link review is unconfirmed. Review the original preparer, retained proposal, owned target and current company binding."});}
    }
    [Authorize]
    [HttpPost("company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-decision")]
    public async Task<ActionResult<PO_LINE_ITEM>> RecordInventoryLinkDecision(string orderId,string lineId,[FromBody]PurchaseOrderInventoryLinkDecisionSubmission submission,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(submission is null || string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(lineId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).RecordLinkDecisionAsync(User,orders,orderId,lineId,submission));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Independent inventory-link outcome is unconfirmed. Retain the original decision intent and full review for reconciliation."});}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException){return Conflict(new{error="The provider refused this original decision. Retain the original intent for reconciliation."});}
        catch(AggregateException){return Conflict(new{error="Decision commit or rollback is unconfirmed. Reconcile the original intent before another decision."});}
    }
    [Authorize]
    [HttpGet("company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-preparation/targets/{itemId}")]
    public async Task<ActionResult<PurchaseOrderInventoryLinkProposalReview>> GetInventoryLinkPreparation(string orderId,string lineId,string itemId,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));itemId=RecordedSourceRouteIds.Read(Request,nameof(itemId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(lineId) || string.IsNullOrWhiteSpace(itemId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetLinkPreparationAsync(User,orders,orderId,lineId,itemId));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Original purchase line, target warehouse evidence or preparation authority is unconfirmed."});}
    }
    [Authorize]
    [HttpPost("company/purchase-orders/{orderId}/lines/{lineId}/inventory-link-preparation")]
    public async Task<ActionResult<PO_LINE_ITEM>> PrepareInventoryLink(string orderId,string lineId,[FromBody]PurchaseOrderInventoryLinkProposalSubmission submission,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));lineId=RecordedSourceRouteIds.Read(Request,nameof(lineId));}catch(FormatException){return BadRequest();}
        if(submission is null || string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(lineId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).PrepareLinkAsync(User,orders,orderId,lineId,submission));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Inventory-link preparation is unconfirmed. Retain the original intent and review for reconciliation; no approval or applied link is confirmed."});}
    }
    [Authorize]
    [HttpGet("company/purchase-orders/{orderId}/receipts/{receiptId}")]
    public async Task<ActionResult<PurchaseReceiptInventoryReview>> GetReceiptInventorySource(string orderId,string receiptId,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));receiptId=RecordedSourceRouteIds.Read(Request,nameof(receiptId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(receiptId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetReceiptLinkAsync(User,orders,orderId,receiptId));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Recorded receipt inventory linkage or current source is unconfirmed. Review the original PO line, receiver decision, owned item, warehouse/location and exact units."});}
    }
    [Authorize]
    [HttpGet("company/purchase-orders/{orderId}/receipts/{receiptId}/stock-review")]
    public async Task<ActionResult<PurchaseReceiptStockReview>> GetReceiptStockReview(string orderId,string receiptId,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));receiptId=RecordedSourceRouteIds.Read(Request,nameof(receiptId));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(receiptId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetReceiptStockReviewAsync(User,orders,orderId,receiptId));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Physical stock review is unconfirmed. Review receiver acceptance, current quantity and complete receipt/return provenance."});}
        catch(OverflowException){return Conflict(new{error="Recorded stock quantities cannot establish bounded receipt capacity."});}
    }
    [Authorize]
    [HttpPost("company/purchase-orders/{orderId}/receipts/{receiptId}/stock-movement")]
    public async Task<ActionResult<PurchaseReceiptInventoryReview>> RecordReceiptStock(string orderId,string receiptId,[FromBody]PurchaseReceiptStockSubmission submission,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{orderId=RecordedSourceRouteIds.Read(Request,nameof(orderId));receiptId=RecordedSourceRouteIds.Read(Request,nameof(receiptId));}catch(FormatException){return BadRequest();}
        if(submission is null || string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(receiptId))return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).RecordStockAsync(User,orders,orderId,receiptId,submission));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception error) when(error is InvalidOperationException or OverflowException or AggregateException)
        {return Conflict(new{error="Stock movement is unconfirmed. Retain the original intent and review complete receipt/item/history evidence for reconciliation."});}
    }
    [Authorize]
    [HttpGet("company/stock-intents/pages/{pageNumber}/{pageSize}")]
    public async Task<ActionResult<PurchaseReceiptStockIntentPage>> GetStockIntentPage(int pageNumber,int pageSize,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{PurchaseReceiptStockIntentPage.RequireRequest(pageNumber,pageSize);}catch(InvalidOperationException){return BadRequest();}
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetStockRecoveryPageAsync(User,orders,pageNumber,pageSize));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception error) when(error is InvalidOperationException or OverflowException){return Conflict(new{error="Original stock page, provider metadata or current source is unconfirmed. Reload discovery."});}
    }
    [Authorize]
    [HttpGet("company/stock-intents")]
    public async Task<ActionResult<PurchaseReceiptStockIntentIndex>> GetStockIntentIndex(
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetStockRecoveryIndexAsync(User,orders));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception error) when(error is InvalidOperationException or OverflowException){return Conflict(new{error="Complete saved stock discovery is unconfirmed. Review original account, retained evidence, current sources and the index limit."});}
    }
    [Authorize]
    [HttpGet("company/stock-intents/{intentId}")]
    public async Task<ActionResult<PurchaseReceiptStockRecovery>> GetStockIntentRecovery(string intentId,
        [FromServices]InventoryService inventory,[FromServices]PurchaseOrderService orders,[FromServices]ICompanyAccountingReadContext context)
    {
        try{intentId=RecordedSourceRouteIds.Read(Request,nameof(intentId));}catch(FormatException){return BadRequest();}
        if(!Guid.TryParseExact(intentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=intentId)return BadRequest();
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetStockRecoveryAsync(User,orders,intentId));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception error) when(error is InvalidOperationException or OverflowException)
        {return Conflict(new{error="Original recorded stock evidence or current owned source is unavailable or changed. Keep the original intent; no replay or stock change is confirmed."});}
    }
    [Authorize]
    [HttpGet("company/items")]
    public async Task<ActionResult<List<INVENTORY_ITEM>>> GetCompanyItems([FromServices]InventoryService inventory,[FromServices]ICompanyAccountingReadContext context)
    {
        try{return Ok(await new CompanyInventoryReads(inventory,context).GetQueueAsync(User));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Company inventory source is unconfirmed. Reload company access and review the PRODUCTION Beep migration."});}
    }
    [Authorize]
    [HttpGet("company/items/{id}")]
    public async Task<ActionResult<CompanyInventorySourceReview>> GetCompanyItemSource(string id,[FromServices]InventoryService inventory,[FromServices]ICompanyAccountingReadContext context)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        if(string.IsNullOrWhiteSpace(id))return BadRequest();
        try
        {
            var result=await new CompanyInventoryReads(inventory,context).GetSourceAsync(User,id);
            return result is null?NotFound():Ok(result);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Inventory item or movement evidence changed or is unavailable. Reload the original company item."});}
    }
}
