using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Route("api/setup/drivers")]
[Authorize(Roles = "Administrator")]
public sealed class SetupDriverController(SetupDriverService drivers, RepositoryRequestAccount account) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken token)
    {
        if (User.IsMachine()) return Forbid();
        await account.RequireAdministratorAsync(token);
        return Ok(drivers.Catalog());
    }
    [HttpPost("{id}/install")]
    public async Task<IActionResult> Install(string id, [FromBody] SetupDriverItem reviewed, CancellationToken token)
    {
        if (User.IsMachine()) return Forbid();
        var actor = await account.RequireAdministratorAsync(token);
        async Task Guard() { if (await account.RequireAdministratorAsync(token) != actor) throw new UnauthorizedAccessException("The administrator changed."); }
        return Ok(await drivers.InstallAsync(id, reviewed, Guard, token));
    }
}
