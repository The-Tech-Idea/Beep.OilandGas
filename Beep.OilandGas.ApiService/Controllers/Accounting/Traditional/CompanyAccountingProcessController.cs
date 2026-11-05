using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Processes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

[ApiController]
[Authorize]
[Route("api/accounting/processes")]
public sealed class CompanyAccountingProcessController(CompanyAccountingProcessReads reads,ILogger<CompanyAccountingProcessController> logger):ControllerBase
{
    [HttpGet("{id}/delivery-correction-review")]
    public Task<ActionResult<CompanyDeliveryCorrectionReview>> DeliveryCorrectionReview(string id)=>DeliveryCorrectionResult(()=>reads.ReviewDeliveryCorrectionAsync(User,RecordedSourceRouteIds.Read(Request,nameof(id))));
    [HttpPost("{id}/delivery-correction-intent")]
    public Task<ActionResult<DELIVERY_ACCEPTANCE_INTENT>> SaveDeliveryCorrection(string id,[FromBody]CompanyDeliveryCorrectionSubmission command)=>DeliveryCorrectionResult(()=>reads.SaveDeliveryCorrectionIntentAsync(User,RecordedSourceRouteIds.Read(Request,nameof(id)),command));
    [HttpPost("{id}/delivery-correction")]
    public Task<ActionResult<RevenueDeliveryAcceptanceRecord>> SubmitDeliveryCorrection(string id,[FromBody]CompanyDeliveryCorrectionSubmission command)=>DeliveryCorrectionResult(()=>reads.SubmitDeliveryCorrectionAsync(User,RecordedSourceRouteIds.Read(Request,nameof(id)),command));
    private async Task<ActionResult<T>> DeliveryCorrectionResult<T>(Func<Task<T>> action)
    {
        try{return Ok(await action());}catch(Exception invalid) when(invalid is ArgumentException or FormatException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(Exception changed) when(changed is InvalidOperationException or System.Text.Json.JsonException){logger.LogWarning(changed,"Original receiving correction is unconfirmed.");return Conflict(new{error="The original rejected process, receiving source or correction outcome could not be confirmed. Retain the exact original command and reconcile before another correction."});}
    }
    [HttpGet("commercial-amendment-queue/pages/{page:int}/{size:int}")]
    public async Task<ActionResult<CompanyCommercialAmendmentPage>> AmendmentPage(int page,int size)
    {
        try{return Ok(await reads.ReadCommercialAmendmentPageAsync(User,page,size));}
        catch(ArgumentException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Saved commercial proposal page could not be confirmed.");return Conflict(new{error="The complete saved proposal page, native history or current authority could not be confirmed. Reload its original company account."});}
    }
    [HttpGet("commercial-amendment-queue")]
    public async Task<ActionResult<CompanyCommercialAmendmentQueue>> AmendmentQueue()
    {
        try{return Ok(await reads.ReadCommercialAmendmentQueueAsync(User));}
        catch(ArgumentException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Saved commercial proposal queue could not be confirmed.");return Conflict(new{error="The complete saved proposal queue, native history or current authority could not be confirmed. Reload company access and original process evidence."});}
    }
    [HttpGet("{id}/commercial-amendment-intent")]
    public async Task<ActionResult<CompanyCommercialAmendmentIntentReview>> AmendmentIntent(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{return Ok(await reads.ReadCommercialAmendmentIntentAsync(User,id));}
        catch(ArgumentException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Saved commercial amendment intent could not be confirmed.");return Conflict(new{error="The original saved intent, process or current authority could not be confirmed."});}
    }
    [HttpPost("{id}/commercial-amendment-intent")]
    public async Task<ActionResult<COMMERCIAL_TERMS_INTENT>> SaveAmendmentIntent(string id,[FromBody]CompanyCommercialAmendmentSubmission command)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{return Ok(await reads.SaveCommercialAmendmentIntentAsync(User,id,command));}
        catch(ArgumentException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Commercial amendment intent save is unconfirmed.");return Conflict(new{error="The original reviewed intent could not be confirmed. Retain it and reconcile the original process before another proposal."});}
    }
    [HttpPost("native-handoff-review")]
    public async Task<ActionResult<CompanyNativeDecisionHandoffReview>> NativeHandoff([FromBody]CompanyAccountingProcessStartCatalogQuery query)
    {
        try{return Ok(await reads.DiscoverNativeDecisionHandoffAsync(User,query));}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Native preparation handoff discovery could not be confirmed.");return Conflict(new{error="The exact native preparation handoff, complete candidate set or current authority could not be confirmed. Reload the original source."});}
    }
    [HttpPost("commercial-handoff-review")]
    public async Task<ActionResult<CompanyCommercialHandoffReview>> CommercialHandoff([FromBody]CompanyAccountingProcessStartCatalogQuery query)
    {
        try{return Ok(await reads.DiscoverCommercialHandoffAsync(User,query));}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Commercial handoff discovery could not be confirmed.");return Conflict(new{error="The exact commercial handoff, complete candidate set or current authority could not be confirmed. Reload the original source."});}
    }
    [HttpGet("{id}/commercial-amendment-review")]
    public async Task<ActionResult<CompanyCommercialAmendmentReview>> AmendmentReview(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{return Ok(await reads.ReviewCommercialAmendmentAsync(User,id));}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Commercial amendment review could not be confirmed.");return Conflict(new{error="The closed original rejection review, predecessor or current amendment authority could not be confirmed."});}
    }
    [HttpPost("{id}/commercial-amendment")]
    public async Task<ActionResult<SalesContractTermsRecord>> Amend(string id,[FromBody]CompanyCommercialAmendmentSubmission submission)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{return Ok(await reads.SubmitCommercialAmendmentAsync(User,id,submission));}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Commercial amendment outcome could not be confirmed.");return Conflict(new{error="The amendment outcome is unconfirmed or its original evidence changed. Retain the original intent and reconcile its native outcome."});}
    }
    [HttpGet("{id}/sales-payment-source")]
    public async Task<ActionResult<CompanySalesPaymentProcessSourceReview>> SalesPaymentSource(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{var review=await reads.ReadSalesPaymentProcessSourceAsync(User,id);return review is null?NotFound():Ok(review);}
        catch(ArgumentException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Owned payment workflow source could not be confirmed.");return Conflict(new{error="The original payment draft, native evidence, process or current authority could not be confirmed. Reload its original company account."});}
    }
    [HttpGet("{id}/sales-journal-source")]
    public async Task<ActionResult<CompanySalesJournalProcessSourceReview>> SalesJournalSource(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{var review=await reads.ReadSalesJournalProcessSourceAsync(User,id);return review is null?NotFound():Ok(review);}
        catch(ArgumentException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Owned journal workflow source could not be confirmed.");return Conflict(new{error="The original journal, complete native evidence, process or current authority could not be confirmed. Reload the original company account."});}
    }
    [HttpGet("{id}/sales-receivable-source")]
    public async Task<ActionResult<CompanySalesReceivableProcessSourceReview>> SalesReceivableSource(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{var review=await reads.ReadSalesReceivableProcessSourceAsync(User,id);return review is null?NotFound():Ok(review);}
        catch(ArgumentException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Owned receivable workflow source could not be confirmed.");return Conflict(new{error="The original receivable draft, process or current source authority could not be confirmed. Reload its original company account and evidence."});}
    }
    [HttpGet("{id}/sales-invoice-source")]
    public async Task<ActionResult<CompanySalesInvoiceProcessSourceReview>> SalesInvoiceSource(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{var review=await reads.ReadSalesInvoiceProcessSourceAsync(User,id);return review is null?NotFound():Ok(review);}
        catch(ArgumentException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Owned invoice workflow source could not be confirmed.");return Conflict(new{error="The original invoice preparation, process or current source authority could not be confirmed. Reload the original company account and evidence."});}
    }
    [HttpGet("{id}/commercial-source")]
    public async Task<ActionResult<CompanyCommercialProcessSourceReview>> CommercialSource(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{var review=await reads.ReadCommercialProcessSourceAsync(User,id);return review is null?NotFound():Ok(review);}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Commercial process source could not be confirmed.");return Conflict(new{error="The original company process and commercial source could not be confirmed. Reload access and recorded evidence."});}
    }
    [HttpGet("{id}/delivery-source")]
    public async Task<ActionResult<CompanyDeliveryProcessSourceReview>> DeliverySource(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{var review=await reads.ReadDeliveryProcessSourceAsync(User,id);return review is null?NotFound():Ok(review);}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Delivery process source could not be confirmed.");return Conflict(new{error="The original company process and delivery source could not be confirmed. Reload access and recorded evidence."});}
    }
    [HttpPost("step-complete")]
    public async Task<ActionResult<CompanyAccountingStepReceipt>> CompleteStep([FromBody]CompanyAccountingStepCommand command)
    {
        try{return Ok(await reads.CompleteStepAsync(User,command));}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company step command outcome could not be confirmed.");return Conflict(new{error="The company step outcome is unconfirmed or changed. Retain the original intent and reconcile its recorded process before another submission."});}
    }
    [HttpGet("{id}/step-review")]
    public async Task<ActionResult<CompanyAccountingStepReview>> ReviewStep(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try{var review=await reads.ReviewStepAsync(User,id);return review is null?NotFound():Ok(review);}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company step review could not be confirmed.");return Conflict(new{error="The company process, assigned step or source-maker evidence is unavailable or changed. Reload company access and the recorded source."});}
    }
    [HttpPost("start-catalog")]
    public async Task<ActionResult<CompanyAccountingProcessStartCatalog>> Catalog([FromBody]CompanyAccountingProcessStartCatalogQuery query)
    {
        try{return Ok(await reads.CatalogAsync(User,query));}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company workflow catalog could not be confirmed.");return Conflict(new{error="Recorded company workflow choices or the displayed source changed or are unavailable. Reload company access and the source."});}
    }
    [HttpPost("start-review")]
    public async Task<ActionResult<CompanyAccountingProcessStartReview>> ReviewStart([FromBody]CompanyAccountingProcessStartQuery query)
    {
        try{return Ok(await reads.ReviewStartAsync(User,query));}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company workflow start review could not be confirmed.");return Conflict(new{error="Company workflow/source/preparer evidence is unavailable or changed. Reload company access and review the original source."});}
    }

    [HttpPost("start")]
    public async Task<ActionResult<CompanyAccountingProcessStartReceipt>> Start([FromBody]CompanyAccountingProcessStart intent)
    {
        try{return Ok(await reads.StartAsync(User,intent));}
        catch(ArgumentException){return BadRequest();}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"Company workflow start outcome could not be confirmed.");return Conflict(new{error="The company handoff outcome is unconfirmed. Retain this intent and reconcile its recorded process before another submission."});}
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<ProcessInstance>> Get(string id)
    {
        try{id=RecordedSourceRouteIds.Read(Request,nameof(id));}catch(FormatException){return BadRequest();}
        try
        {
            var review=await reads.ReadAsync(User,id);
            return review is null?NotFound():Ok(review);
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(InvalidOperationException changed){logger.LogWarning(changed,"The company accounting process source could not be confirmed.");
            return Conflict(new{error="Company accounting process ownership, database binding or native source evidence is unavailable or changed. Reload company access and review the original source."});}
    }
}
