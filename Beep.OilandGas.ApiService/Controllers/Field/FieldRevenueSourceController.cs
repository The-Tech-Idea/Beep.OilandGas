using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.Field;
[ApiController][Authorize][Route("api/fields/{fieldId}/accounting/revenue-source")]
public sealed class FieldRevenueSourceController(CompanyFieldRevenueReads reads):ControllerBase
{
    [HttpGet("/api/accounting/delivery-intents")]
    public Task<ActionResult<CompanyDeliverySavedWorkPage>> ReadDeliveryPage(int page=1,int size=8)=>Acceptance(()=>reads.ReadDeliverySavedPageAsync(User,page,size));
    [HttpGet("/api/accounting/delivery-intents/{intentId}")]
    public Task<ActionResult<CompanyDeliverySavedWorkReview>> ReadDeliveryIntent(string intentId)=>Acceptance(()=>reads.ReadDeliveryIntentAsync(User,RecordedSourceRouteIds.Read(Request,nameof(intentId))));
    [HttpGet("delivery/acceptance/saved")]
    public Task<ActionResult<RevenueDeliveryAcceptanceSavedReview>> ReadSaved(string fieldId,DateTime from,DateTime through,string transactionId)=>Acceptance(()=>reads.ReadDeliverySavedAsync(User,RecordedSourceRouteIds.Read(Request,nameof(fieldId)),from,through,transactionId));
    [HttpPost("delivery/acceptance/intent")]
    public Task<ActionResult<DELIVERY_ACCEPTANCE_INTENT>> SaveIntent(string fieldId,[FromBody]RevenueDeliveryAcceptanceIntent command)=>Acceptance(()=>reads.SaveDeliveryIntentAsync(User,RecordedSourceRouteIds.Read(Request,nameof(fieldId)),command));
    [HttpGet("delivery/acceptance")]
    public Task<ActionResult<RevenueDeliveryAcceptanceReadReview>> ReadAcceptance(string fieldId,DateTime from,DateTime through,string transactionId)=>Acceptance(()=>reads.ReadDeliveryAcceptanceAsync(User,RecordedSourceRouteIds.Read(Request,nameof(fieldId)),from,through,transactionId));
    [HttpPost("delivery/acceptance/prepare")]
    public Task<ActionResult<RevenueDeliveryAcceptanceRecord>> PrepareAcceptance(string fieldId,[FromBody]RevenueDeliveryAcceptancePreparation preparation)=>Acceptance(()=>reads.PrepareDeliveryAcceptanceAsync(User,RecordedSourceRouteIds.Read(Request,nameof(fieldId)),preparation));
    [HttpPost("delivery/acceptance/decide")]
    public Task<ActionResult<RevenueDeliveryAcceptanceRecord>> DecideAcceptance(string fieldId,[FromBody]RevenueDeliveryAcceptanceSubmission submission)=>Acceptance(()=>reads.DecideDeliveryAcceptanceAsync(User,RecordedSourceRouteIds.Read(Request,nameof(fieldId)),submission));
    private async Task<ActionResult<T>> Acceptance<T>(Func<Task<T>> action)
    {
        try{return Ok(await action());}catch(ArgumentException){return BadRequest();}catch(FormatException){return BadRequest();}catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(Exception invalid) when(invalid is InvalidOperationException or JsonException){return Conflict(new{error="Original measured delivery or recorded acceptance could not be confirmed. Retain and reconcile the original intent."});}
    }
    [HttpGet("delivery")]
    public async Task<ActionResult<RevenueDeliverySourceReview>> GetDelivery(string fieldId,DateTime from,DateTime through,string transactionId,string sourceRevision)
    {
        try{fieldId=RecordedSourceRouteIds.Read(Request,nameof(fieldId));FieldRevenueSourceReview.RequireWindow(fieldId,from,through);}catch(ArgumentException){return BadRequest();}catch(FormatException){return BadRequest();}
        try{return Ok(await reads.GetDeliveryAsync(User,fieldId,from,through,transactionId,sourceRevision));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="The original company revenue, allocation and FIELD run-ticket evidence could not be confirmed. Reload before reviewing delivery."});}
    }
    [HttpGet("sales-contract")]
    public async Task<ActionResult<RevenueSalesContractReview>> GetSalesContract(string fieldId,DateTime from,DateTime through,string transactionId,string sourceRevision)
    {
        try{fieldId=RecordedSourceRouteIds.Read(Request,nameof(fieldId));FieldRevenueSourceReview.RequireWindow(fieldId,from,through);}catch(ArgumentException){return BadRequest();}catch(FormatException){return BadRequest();}
        try{return Ok(await reads.GetContractAsync(User,fieldId,from,through,transactionId,sourceRevision));}
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="The original revenue source and company-owned sales contract could not be confirmed. Reload before reviewing."});}
    }
    [HttpGet]
    public async Task<ActionResult<FieldRevenueSourceReview>> Get(string fieldId,DateTime from,DateTime through)
    {
        try{fieldId=RecordedSourceRouteIds.Read(Request,nameof(fieldId));FieldRevenueSourceReview.RequireWindow(fieldId,from,through);}catch(ArgumentException){return BadRequest();}catch(FormatException){return BadRequest();}
        try
        {
            return Ok(await reads.GetAsync(User,fieldId,from,through));
        }
        catch(UnauthorizedAccessException){return Forbid();}
        catch(Beep.OilandGas.Models.Core.Refusals.RefusalException denied) when(denied.Kind==Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden){return Forbid();}
        catch(InvalidOperationException){return Conflict(new{error="Recorded revenue source could not be confirmed. Reload the original company, FIELD and date window."});}
    }
}
