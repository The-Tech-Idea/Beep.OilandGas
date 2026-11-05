using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers;

[ApiController]
[Route("api/company-data-quality")]
[Authorize]
public sealed class CompanyWellQualityController(RepositoryRequestAccount account, CompanyModuleConnection company,
    CompanyWellQualityReviewer reviewer) : ControllerBase
{
    [HttpGet("wells/sample")]
    public async Task<ActionResult<CompanyWellQualitySample>> FindWells([FromServices] CompanyWellQualityDiscovery discovery, CancellationToken token)
    {
        var actor = await account.RequireAsync(token);
        var selected = await company.RefreshCompanyAsync(token); company.RequireActor(actor);
        async Task Guard()
        {
            if (await account.RequireAsync(token) != actor || (await company.RefreshCompanyAsync(token)).Scope != selected.Scope)
                throw RefusalException.Forbidden("Company access changed.");
            company.RequireActor(actor);
        }
        return Ok(await discovery.FindAsync(selected.Scope, Guard, token));
    }

    [HttpPost("wells/review")]
    public async Task<ActionResult<CompanyWellQualityBatchReview>> ReviewWells([FromBody] CompanyWellQualityBatchRequest request, CancellationToken token)
    {
        var actor = await account.RequireAsync(token);
        var selected = await company.RefreshCompanyAsync(token); company.RequireActor(actor);
        async Task Guard()
        {
            if (await account.RequireAsync(token) != actor || (await company.RefreshCompanyAsync(token)).Scope != selected.Scope)
                throw RefusalException.Forbidden("Company access changed.");
            company.RequireActor(actor);
        }
        return Ok(await reviewer.ReviewBatchAsync(selected.Scope, request, Guard, token));
    }
}
