using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.OilandGas.ApiService.Data;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

[ApiController]
[Route("api/setup/companies")]
[Authorize(Roles = "Administrator")]
public sealed class InitialCompanyProvisionController(RepositoryRoleAssignmentService assignments) : ControllerBase
{
    [HttpPost("initial/review")]
    public async Task<IActionResult> Review(InitialCompanyProvisionRequest request, CancellationToken cancellationToken)
    {
        if (User.IsMachine()) return Forbid();
        return Ok(await assignments.ReviewInitialCompanyAsync(request, User.ActingUserId(), cancellationToken));
    }

    [HttpPost("initial")]
    public async Task<IActionResult> Create(InitialCompanyProvisionRequest request, CancellationToken cancellationToken)
    {
        if (User.IsMachine()) return Forbid();
        try
        {
            var result = await assignments.CreateInitialCompanyAsync(request.TenantId, request.TenantName,
                request.BusinessAssociateId, request.Reason, request.IntentId, User.ActingUserId(), cancellationToken);
            return Ok(result);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { Error = "Setup changed while provisioning. Retry the same request or review the existing tenant." });
        }
        catch (DbUpdateException error) when (UniqueKeyViolation.Is(error))
        {
            return Conflict(new { Error = "The company or request reference already exists. Retry the same request or review the existing tenant." });
        }
    }
}
