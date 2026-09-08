using System.Security.Claims;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

[ApiController]
[Route("api/identity/roles")]
[Authorize(Roles = "Administrator")]
public sealed class RepositoryRolesController(RepositoryRoleCatalogService catalog, ILogger<RepositoryRolesController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await catalog.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(RepositoryRoleRequest request, CancellationToken cancellationToken)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(actor)) return Forbid();
        try
        {
            var role = await catalog.CreateAsync(request, cancellationToken);
            logger.LogInformation("Role created: Actor={Actor} Role={Role}", actor, role.RoleId);
            return Ok(role);
        }
        catch (ArgumentException) { return BadRequest(new { Error = "Invalid role name or description." }); }
        catch (InvalidOperationException) { return Conflict(new { Error = "The role could not be created. Check for an existing role with that name." }); }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Role creation failed for Actor={Actor}", actor);
            return Conflict(new { Error = "The role could not be saved. Reload the role catalog before retrying." });
        }
    }
}
