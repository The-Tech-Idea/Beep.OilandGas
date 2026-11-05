using Beep.OilandGas.Accounting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Data.Accounting;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

/// <summary>Retired companyless AR routes; reviewed company source commands own receivable creation and financial continuation.</summary>
[ApiController]
[Authorize]
[Route("api/accounting/traditional/ar")]
public sealed class ARController:ControllerBase
{
    private object CompanyWorkflowRequired()=>new
    {
        code="company_receivable_workflow_required",
        error="Receivable access requires the selected company, independently reviewed sales invoice and original source-bound command. The companyless AR path is unavailable."
    };
    [HttpGet("{id}")]
    public ActionResult<object> GetARInvoice(string id,[FromQuery]string? connectionName=null)=>Conflict(CompanyWorkflowRequired());
    [HttpPost]
    public Task<ActionResult<object>> CreateARInvoice([FromBody]CreateARInvoiceRequest request,[FromQuery]string? connectionName=null)=>
        Task.FromResult<ActionResult<object>>(Conflict(CompanyWorkflowRequired()));
}
