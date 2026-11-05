using Beep.OilandGas.Accounting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Data.Accounting;
namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

/// <summary>Retired companyless invoice routes. Company billing requires an owned, reviewed source command.</summary>
[ApiController]
[Authorize]
[Route("api/accounting/traditional/invoice")]
public sealed class InvoiceController:ControllerBase
{
    private object CompanyWorkflowRequired()=>new
    {
        code="company_invoice_workflow_required",
        error="Invoice access requires the selected company, recorded delivery, approved billing policy and original reviewed command. The companyless invoice path is unavailable."
    };
    [HttpGet]
    public Task<ActionResult<List<object>>> GetInvoices([FromQuery]string? connectionName=null)=>
        Task.FromResult<ActionResult<List<object>>>(Conflict(CompanyWorkflowRequired()));
    [HttpGet("{id}")]
    public ActionResult<object> GetInvoice(string id)=>Conflict(CompanyWorkflowRequired());
    [HttpPost]
    public Task<ActionResult<object>> CreateInvoice([FromBody]CreateInvoiceRequest request)=>
        Task.FromResult<ActionResult<object>>(Conflict(CompanyWorkflowRequired()));
}
