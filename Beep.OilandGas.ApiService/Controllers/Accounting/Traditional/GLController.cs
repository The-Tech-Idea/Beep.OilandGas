using Beep.OilandGas.Accounting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.ProductionAccounting.Services;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional;

/// <summary>Retained route signatures for retired companyless GL operations.</summary>
[ApiController]
[Authorize]
[Route("api/accounting/traditional/gl")]
public class GLController:ControllerBase
{
    // Retiring a route must not activate the global production compatibility facade.
    [ActivatorUtilitiesConstructor]
    public GLController() { }
    public GLController(ProductionAccountingService service,ILogger<GLController> logger)
    {ArgumentNullException.ThrowIfNull(service);}

    private ObjectResult Retired()=>StatusCode(410,new{
        error="This companyless ledger operation is retired. Use recorded company/book/currency account configuration. Company journal operations are currently unavailable.",
        accountConfiguration="api/accounting/traditional/gl/company-accounts",
        workspace="/accounting/ledger-accounts"
    });
    [HttpGet("accounts")]
    public ActionResult<List<GLAccount>> GetAccounts([FromQuery]string connectionName="PPDM39")=>Retired();
    [HttpGet("accounts/{id}")]
    public ActionResult<GLAccount> GetAccount(string id,[FromQuery]string connectionName="PPDM39")=>Retired();
    [HttpPost("accounts")]
    public ActionResult<GLAccount> CreateAccount([FromBody]CreateGLAccountRequest request,[FromQuery]string connectionName="PPDM39")=>Retired();
    [HttpPost("journal-entries")]
    public ActionResult<object> CreateJournalEntry([FromBody]CreateJournalEntryRequest request,[FromQuery]string connectionName="PPDM39")=>Retired();
    [HttpPost("journal-entries/{id}/post")]
    public ActionResult PostJournalEntry(string id,[FromQuery]string connectionName="PPDM39")=>Retired();
    [HttpGet("journal-entries/{id}")]
    public ActionResult<object> GetJournalEntry(string id,[FromQuery]string connectionName="PPDM39")=>Retired();
}
