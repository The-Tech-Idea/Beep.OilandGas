using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.HSE;

/// <summary>Records or reconciles an exact reviewed incident; workflow creation remains a separate operation.</summary>
[ApiController]
[Route("api/field/current/hse/reports")]
[Authorize(Policy = PermissionConstants.HSE.ReportIncident)]
[RequireCurrentFieldAccess(PermissionConstants.HSE.ReportIncident, true)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class IncidentReportController(ICompanyIncidentCommands commands, ICompanyFieldSelectionService selections,
    RepositoryRequestAccount account) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<IncidentReportReceipt>> RecordAsync([FromBody] IncidentReportIntent original, CancellationToken token)
    {
        if (original is null) throw RefusalException.Invalid("The original incident report is required.");
        try { original.Require(); }
        catch (InvalidOperationException) { throw RefusalException.Invalid("The original incident report is invalid."); }
        var actor = await account.RequireAsync(token);
        if (original.Context.ActorUserId != actor) throw RefusalException.Forbidden("The report belongs to another account.");
        async Task RequireCurrentAsync()
        {
            token.ThrowIfCancellationRequested();
            if (await account.RequireAsync(token) != actor) throw RefusalException.Forbidden("The reporting account changed.");
            var selected = await selections.GetCurrentFieldSelectionAsync();
            try { FieldSelectionReviewValidator.RequireCurrent(selected, original.Context.Company, actor); }
            catch (InvalidOperationException) { throw RefusalException.Conflict("The selected company or FIELD changed. Review the original report."); }
            if (!selected.SelectionAvailable || selected.SelectionContext != original.Context)
                throw RefusalException.Conflict("The selected company or FIELD changed. Review the original report.");
        }
        await RequireCurrentAsync();
        var id = await commands.CreateReportAsync(original);
        await RequireCurrentAsync();
        var receipt = new IncidentReportReceipt(original.Context.Company, actor, original.Context.FieldId!, original.IntentId, id);
        receipt.Require(original);
        return Ok(receipt);
    }
}
