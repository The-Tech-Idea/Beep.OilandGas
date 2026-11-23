using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39;

public partial class PPDM39SetupController
{
    [HttpPost("database-target/review")]
    public async Task<IActionResult> ReviewDatabaseTarget(LocalDbDatabaseTargetRequest request,
        [FromServices] RepositoryRequestAccount account, CancellationToken cancellationToken)
    {
        var actor = await account.RequireAdministratorAsync(cancellationToken);
        var review = _setupService.ReviewLocalDbDatabaseTarget(request, actor);
        await account.RequireAdministratorAsync(cancellationToken);
        return Ok(review);
    }

    [HttpPost("database-target/create")]
    public async Task<IActionResult> CreateDatabaseTarget(LocalDbDatabaseTargetCreate request,
        [FromServices] RepositoryRequestAccount account, CancellationToken cancellationToken)
    {
        var actor = await account.RequireAdministratorAsync(cancellationToken);
        var result = await _setupService.CreateLocalDbDatabaseTargetAsync(request, actor, cancellationToken);
        await account.RequireAdministratorAsync(cancellationToken);
        return Ok(result);
    }
}
