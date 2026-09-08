using System.Security.Claims;
using Beep.OilandGas.Repository;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Route("api/auth/repository")]
[Authorize(Policy = RepositoryAuthorization.ExternalAccount)]
public sealed class RepositoryAccountController(IRepositoryAccessService access) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var issuer = User.FindFirstValue("iss");
        var subject = User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(subject)) return Forbid();
        var result = await access.GetAccessAsync(issuer, subject, cancellationToken);
        if (result is null) return NotFound();
        if (!result.IsActive) return Forbid();
        return Ok(result);
    }
}
