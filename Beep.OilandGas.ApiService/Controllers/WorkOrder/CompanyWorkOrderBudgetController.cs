using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Controllers.WorkOrder;

[ApiController][Authorize]
[Route("api/field/current/workorder/{workOrderId}/budget")]
public sealed class CompanyWorkOrderBudgetController(CompanyWorkOrderBudgetService service):ControllerBase
{
    [HttpPost("financial-sources")][RequireCurrentFieldAccess("READ")]
    public Task<ActionResult> FinancialSources(string workOrderId,CompanyWorkOrderFinancialQuery query)=>Run(async()=>{query.Require();if(query.WorkOrderId!=workOrderId)throw new ArgumentException("Original route work order is required.");return Ok(await service.ReadFinancialSourcesAsync(User,query));});
    [HttpPost("material-cost/review")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> MaterialCostReview(string workOrderId,CompanyWorkOrderMaterialCostQuery query)=>Run(async()=>{query.Require();if(query.Budget.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("Original route work order is required.");return Ok(await service.ReviewMaterialCostAsync(User,query));});
    [HttpPost("material-cost")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> MaterialCost(string workOrderId,CompanyWorkOrderMaterialCostCommand command)=>Run(async()=>{command.Require();if(command.Review.Query.Budget.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("Original route work order is required.");return Ok(await service.RecordMaterialCostAsync(User,command));});
    [HttpPost("material-cost/recovery/reference")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> MaterialCostRecovery(string workOrderId,CompanyWorkOrderMaterialCostReference reference)=>Run(async()=>{reference.Require();if(reference.Query.Budget.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("Original route work order is required.");return Ok(await service.RecoverMaterialCostAsync(User,reference));});
    [HttpPost("actual-costs")][RequireCurrentFieldAccess("READ")]
    public Task<ActionResult> ActualCosts(string workOrderId,CompanyWorkOrderBudgetDecisionQuery query)=>Run(async()=>{query.Require();if(query.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("Original route work order is required.");return Ok(await service.ReadActualCostsAsync(User,query));});
    [HttpPost("labor-cost/review")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> LaborCostReview(string workOrderId,CompanyWorkOrderLaborCostQuery query)=>Run(async()=>{query.Require();if(query.Budget.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("Original route work order is required.");return Ok(await service.ReviewLaborCostAsync(User,query));});
    [HttpPost("labor-cost")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> LaborCost(string workOrderId,CompanyWorkOrderLaborCostCommand command)=>Run(async()=>{command.Require();if(command.Review.Query.Budget.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("Original route work order is required.");return Ok(await service.RecordLaborCostAsync(User,command));});
    [HttpPost("labor-cost/recovery/reference")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> LaborCostRecovery(string workOrderId,CompanyWorkOrderLaborCostReference reference)=>Run(async()=>{reference.Require();if(reference.Query.Budget.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("Original route work order is required.");return Ok(await service.RecoverLaborCostAsync(User,reference));});
    [HttpPost("decision/status")][RequireCurrentFieldAccess("READ")]
    public Task<ActionResult> DecisionStatus(string workOrderId,CompanyWorkOrderBudgetDecisionQuery query)=>Run(async()=>{query.Require();if(query.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("The original route work order is required.");return Ok(await service.ReadDecisionStatusAsync(User,query));});
    [HttpPost("decision/review")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> DecisionReview(string workOrderId,CompanyWorkOrderBudgetDecisionQuery query)=>Run(async()=>{query.Require();if(query.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("The original route work order is required.");return Ok(await service.ReviewDecisionAsync(User,query));});
    [HttpPost("decision")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> Decision(string workOrderId,CompanyWorkOrderBudgetDecisionCommand command)=>Run(async()=>{command.Require();if(command.Review.Query.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("The original route work order is required.");return Ok(await service.DecideAsync(User,command));});
    [HttpPost("decision/recovery/reference")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> DecisionRecovery(string workOrderId,CompanyWorkOrderBudgetDecisionReference reference)=>Run(async()=>{reference.Require();if(reference.Query.Budget.WorkOrderId!=workOrderId)throw new ArgumentException("The original route work order is required.");return Ok(await service.RecoverDecisionAsync(User,reference));});
    [HttpPost("review")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> Review(string workOrderId,CompanyWorkOrderBudgetQuery query)=>Run(async()=>{query.Require();if(query.WorkOrderId!=workOrderId)throw new ArgumentException("The original route work order is required.");return Ok(await service.ReviewAsync(User,query));});
    [HttpPost][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> Create(string workOrderId,CompanyWorkOrderBudgetCommand command)=>Run(async()=>{command.Require();if(command.Review.Query.WorkOrderId!=workOrderId)throw new ArgumentException("The original route work order is required.");return Ok(await service.CreateAsync(User,command));});
    [HttpPost("recovery/reference")][RequireCurrentFieldAccess("WRITE")]
    public Task<ActionResult> Recover(string workOrderId,CompanyWorkOrderBudgetReference reference)=>Run(async()=>{reference.Require();if(reference.Query.WorkOrderId!=workOrderId)throw new ArgumentException("The original route work order is required.");return Ok(await service.RecoverAsync(User,reference));});
    private async Task<ActionResult> Run(Func<Task<ActionResult>> action)
    {
        Response.Headers.CacheControl="no-store";try{return await action();}catch(UnauthorizedAccessException){return Forbid();}
        catch(ArgumentException error){return BadRequest(new{error=error.Message});}catch(InvalidOperationException error){return Conflict(new{error=error.Message});}
    }
}
