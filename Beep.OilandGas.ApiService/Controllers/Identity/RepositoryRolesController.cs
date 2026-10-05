using Beep.OilandGas.ApiService.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

[ApiController]
[Route("api/identity/roles")]
[Authorize(Roles = "Administrator")]
public sealed class RepositoryRolesController(RepositoryRoleCatalogService catalog, ILogger<RepositoryRolesController> logger,
    IFailureReporter failures) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await catalog.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(RepositoryRoleRequest request, CancellationToken cancellationToken)
    {
        var actor = User.ActingUserId();
        try
        {
            var role = await catalog.CreateAsync(request, cancellationToken);
            logger.LogInformation("Role created: Actor={Actor} Role={Role}", actor, role.RoleId);
            return Ok(role);
        }
        // A role of the same name saved by somebody else first is the administrator's to reload; the catalog's own refusals
        // reach the API's handler as refusals, and any other refused save as the failure it is (OILGAS-CATCH-01: every
        // refused save was answered "could not be saved", and the catalog's rules were guessed from exception types).
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate))
        {
            failures.ReportHandled(duplicate, "creating a role", "no role is created; the administrator is told to reload", FailureSeverity.Degraded);
            return Conflict(new { Error = "The role could not be saved. Reload the role catalog before retrying." });
        }
    }
}
