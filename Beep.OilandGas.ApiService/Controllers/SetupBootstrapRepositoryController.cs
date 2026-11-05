using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Route("api/setup/bootstrap/repository")]
[AllowAnonymous]
public sealed class SetupBootstrapRepositoryController(SetupBootstrapAccess access, IServiceProvider services) : ControllerBase
{
    private async Task Guard(CancellationToken token)
    {
        var values = Request.Headers["X-Setup-Key"];
        await access.RequireAsync(values.Count == 1 ? values[0] : null, token);
    }

    [HttpGet("connections")]
    public async Task<IActionResult> Connections(CancellationToken token)
    {
        try { await Guard(token); return Ok(services.GetRequiredService<SetupRepositoryConnectionSelectionService>().Connections()); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }

    [HttpPost("connection")]
    public async Task<IActionResult> Select([FromBody] SetupRepositoryConnectionSelection request, CancellationToken token)
    {
        try
        {
            await Guard(token);
            return Ok(await services.GetRequiredService<SetupRepositoryConnectionSelectionService>().SelectAsync(request.ConnectionName, () => Guard(token), token));
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }

    [HttpPost("connections/test")]
    public async Task<IActionResult> Test([FromBody] SetupRepositoryConnectionSelection request, CancellationToken token)
    {
        try
        {
            await Guard(token);
            return Ok(await services.GetRequiredService<SetupRepositoryConnectionSelectionService>().TestAsync(request.ConnectionName, () => Guard(token), token));
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }

    [HttpPost("connections")]
    public async Task<IActionResult> Create([FromBody] SetupRepositoryConnectionCreate request, CancellationToken token)
    {
        try
        {
            await Guard(token);
            return Ok(await services.GetRequiredService<SetupRepositoryConnectionCreationService>().CreateAsync(request, () => Guard(token), token));
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
    }

    [HttpGet("migration")]
    public async Task<IActionResult> Plan(CancellationToken token)
    {
        try
        {
            await Guard(token);
            return Ok(await services.GetRequiredService<SetupRepositoryMigrationService>().PlanAsync(() => Guard(token), token));
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (PersonaScopeTransitionRequiredException) { return Conflict(new { error = "Existing account-wide persona settings need a reviewed company assignment before migration." }); }
    }

    [HttpPost("migration")]
    public async Task<IActionResult> Apply([FromBody] SetupRepositoryMigrationRequest request, CancellationToken token)
    {
        try
        {
            await Guard(token);
            return Ok(await services.GetRequiredService<SetupRepositoryMigrationService>().ApplyAsync(request, () => Guard(token), token));
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (PersonaScopeTransitionRequiredException) { return Conflict(new { error = "Existing account-wide persona settings need a reviewed company assignment before migration." }); }
    }
}
