using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Compliance;

[ApiController]
[Route("api/field/current/compliance/obligation-creation")]
[Authorize]
[RequireCurrentFieldAccess(PermissionConstants.Regulatory.ManageFilings)]
public sealed class FieldObligationCreationController(CompanyFieldObligationAuthority authority, CompanyFieldObligationWriter writer,
    ICompanyFieldSelectionService fields) : ControllerBase
{
    [HttpGet("options")]
    public async Task<ActionResult<FieldObligationCreationOptions>> OptionsAsync()
    {
        var selection = await fields.GetCurrentFieldSelectionAsync();
        if (!selection.SelectionAvailable || selection.SelectionContext is null)
            throw RefusalException.Conflict("Choose an available FIELD before reviewing obligation types.");
        var scope = await authority.CaptureAsync(User, selection.SelectionContext);
        return Ok(await writer.OptionsAsync(scope));
    }

    [HttpPost]
    public async Task<ActionResult<CreatedFieldObligation>> CreateAsync([FromBody] CreateFieldObligation request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Context is null) throw RefusalException.Invalid("The original FIELD review is required.");
        var selected = await fields.GetCurrentFieldSelectionAsync();
        if (!selected.SelectionAvailable || !FieldObligationIdentity.SameTarget(request.Context, selected.SelectionContext))
            throw RefusalException.Forbidden("Return to the original company and FIELD before retrying this request.");
        var scope = await authority.CaptureAsync(User, selected.SelectionContext!);
        return Ok(await writer.CreateAsync(request, scope));
    }
}
