using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("api/setup/system-repository/connection")]
public sealed class SystemSetupConnectionController(SystemSetupConnectionSelectionService selection, RepositoryRequestAccount account) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken token)
    {
        if (User.IsMachine()) return Forbid();
        await account.RequireAdministratorAsync(token);
        return Ok(selection.Connections());
    }

    [HttpPost]
    public async Task<IActionResult> Select(SystemSetupConnectionSelection request, CancellationToken token)
    {
        if (User.IsMachine()) return Forbid();
        var actor = await account.RequireAdministratorAsync(token);
        async Task Guard() { if (await account.RequireAdministratorAsync(token) != actor) throw new UnauthorizedAccessException("The administrator changed."); }
        try { return Ok(await selection.SelectAsync(request.ConnectionName, Guard, token)); }
        catch (InvalidOperationException) { return Conflict(new { error = "The connection could not be selected. Check its status and existing system target." }); }
    }
}
