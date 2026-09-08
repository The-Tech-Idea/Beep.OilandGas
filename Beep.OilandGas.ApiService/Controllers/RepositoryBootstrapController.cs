using System.Security.Claims;
using Beep.OilandGas.Repository;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Route("api/setup/repository")]
[Authorize(Policy = RepositoryAuthorization.ExternalAccount)]
public sealed class RepositoryBootstrapController(
    RepositoryBootstrapService bootstrap,
    ILogger<RepositoryBootstrapController> logger) : ControllerBase
{
    [HttpPost("bootstrap")]
    [HttpPost("register")]
    public async Task<IActionResult> Bootstrap(CancellationToken cancellationToken)
    {
        // Both claims come from the API's validated bearer token, never request input.
        var issuer = User.FindFirstValue("iss");
        var subject = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(subject))
            return Forbid();
        try
        {
            var result = await bootstrap.BootstrapAsync(issuer, subject, cancellationToken);
            if (result == BootstrapOutcome.NotAllowed)
                return Forbid();
            logger.LogInformation("Repository bootstrap outcome {Outcome}", result);
            return Ok(new { Status = result.ToString() });
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "Repository bootstrap transaction failed");
            return Conflict(new { Error = "Bootstrap could not complete. Check repository status before retrying." });
        }
    }
}
