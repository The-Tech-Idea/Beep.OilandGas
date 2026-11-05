using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Route("api/setup/bootstrap/drivers")]
[AllowAnonymous]
public sealed class SetupBootstrapDriverController(SetupDriverService drivers, SetupBootstrapAccess access) : ControllerBase
{
    private const string KeyHeader = "X-Setup-Key";

    private async Task Guard(CancellationToken token)
    {
        var values = Request.Headers[KeyHeader];
        await access.RequireAsync(values.Count == 1 ? values[0] : null, token);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken token)
    {
        try { await Guard(token); return Ok(drivers.Catalog()); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }

    [HttpPost("{id}/install")]
    public async Task<IActionResult> Install(string id, [FromBody] SetupDriverItem reviewed, CancellationToken token)
    {
        try { return Ok(await drivers.InstallAsync(id, reviewed, () => Guard(token), token)); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }
}
