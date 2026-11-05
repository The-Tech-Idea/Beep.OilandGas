using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers.HSE;

[ApiController]
[Route("api/field/current/hse/report-options")]
[Authorize(Policy = PermissionConstants.HSE.ReportIncident)]
[RequireCurrentFieldAccess(PermissionConstants.HSE.ReportIncident, true)]
public sealed class IncidentReportOptionsController(ICompanyIncidentCommands commands, ICompanyFieldSelectionService selections,
    RepositoryRequestAccount account) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IncidentReportOptions>> GetAsync(CancellationToken token)
    {
        var actor = await account.RequireAsync(token);
        var selected = await selections.GetCurrentFieldSelectionAsync();
        if (!selected.SelectionAvailable || selected.SelectionContext is not { } review || review.ActorUserId != actor)
            return Conflict(new { error = "The selected company FIELD is unavailable." });
        FieldSelectionReviewValidator.RequireCurrent(selected, review.Company, actor);
        var options = await commands.ReviewReportOptionsAsync(review);
        if (options.Context != review || await account.RequireAsync(token) != actor)
            throw new InvalidOperationException("The incident report options could not be confirmed for the current account and FIELD.");
        var current = await selections.GetCurrentFieldSelectionAsync();
        FieldSelectionReviewValidator.RequireCurrent(current, review.Company, actor);
        if (!current.SelectionAvailable || current.SelectionContext != review)
            throw new InvalidOperationException("The selected FIELD changed while reviewing incident report options.");
        return Ok(options);
    }
}
