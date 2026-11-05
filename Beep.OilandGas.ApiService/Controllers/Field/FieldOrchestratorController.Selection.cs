using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Field;

// Existing controller/resource; selection is stored by the canonical self-profile owner and never grants FIELD access.
[ApiController]
[Authorize]
[Route("api/field")]
public sealed partial class FieldOrchestratorController(ICompanyFieldSelectionService selection) : ControllerBase
{
    [HttpGet("fields")]
    public async Task<ActionResult<List<FieldListItem>>> GetAllFields([FromQuery] string? connectionName = null)
        => Ok(await selection.GetAvailableFieldsAsync(connectionName));
    [HttpGet("current")]
    public async Task<ActionResult<FieldResponse>> GetCurrentField()
        => Ok(await selection.GetCurrentFieldSelectionAsync());
    [HttpPost("set-active")]
    public async Task<ActionResult<SetActiveFieldResponse>> SetActiveField([FromBody] SetActiveFieldRequest? request)
        => Ok(await selection.SetCurrentFieldSelectionAsync(request ?? throw RefusalException.Invalid("A reviewed FIELD selection body is required.")));
}
