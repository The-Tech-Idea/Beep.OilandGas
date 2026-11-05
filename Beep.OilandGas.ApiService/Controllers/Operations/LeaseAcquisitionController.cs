using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LeaseAcquisition.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Operations;

// One resource owner; FIELD selectors never grant authority. The company gate independently checks current access.
[ApiController]
[Authorize]
[Route("api/fields/{fieldId}/lease-acquisition")]
public sealed class LeaseAcquisitionController(ILeaseAcquisitionService service, ICompanyLeaseDataAccess access) : ControllerBase
{
    [HttpGet("evaluate/{leaseId}")]
    public async Task<ActionResult<LeaseSummary>> EvaluateLease(string fieldId, string leaseId)
        => Ok(await service.EvaluateLeaseAsync(leaseId, fieldId));

    [HttpGet("available")]
    public async Task<ActionResult<List<LeaseSummary>>> GetAvailableLeases(string fieldId, [FromQuery] Dictionary<string, string>? filters = null)
    {
        var selected = filters is null ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(filters, StringComparer.Ordinal);
        if (selected.TryGetValue("FieldId", out var field) && field != fieldId || selected.TryGetValue("PropertyId", out var property) && property != fieldId)
            throw RefusalException.Invalid("The lease FIELD assertions do not match the route.");
        selected["FieldId"] = fieldId;
        return Ok(await service.GetAvailableLeasesAsync(selected));
    }
    [HttpGet("creation-context")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<LeaseCreationContext>> GetCreationContext(string fieldId)
        => Ok(await service.GetCreationContextAsync(fieldId));

    [HttpPost("creation-reconciliation")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<LeaseCreationReceipt>> ReconcileCreation(string fieldId, [FromBody] CreateLeaseAcquisition? request)
    {
        var intent = Intent(fieldId, request);
        return Ok(await service.ReconcileLeaseCreationAsync(intent, await Actor(fieldId)));
    }
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<LeaseCreationReceipt>> CreateLeaseAcquisition(string fieldId, [FromBody] CreateLeaseAcquisition? request)
    {
        var intent = Intent(fieldId, request);
        var id = await service.CreateLeaseAcquisitionAsync(intent, await Actor(fieldId));
        return Ok(new LeaseCreationReceipt(intent.CommandId, "Recorded", id, intent.Context!));
    }
    [HttpPut("{leaseId}/status")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult> UpdateLeaseStatus(string fieldId, string leaseId, [FromBody] UpdateLeaseStatusRequest? request)
    {
        if (request is null) throw RefusalException.Invalid("Request body is required.");
        var status = request.Status;
        await service.UpdateLeaseStatusAsync(leaseId, status, await Actor(fieldId), fieldId);
        return Ok();
    }
    private async Task<string> Actor(string field)
    {
        // The existing gate resolves the request's application account/company and validates live Administrator/FIELD WRITE.
        var scope = await access.ResolveAsync();
        await access.RequireFieldAsync(scope, field, "WRITE", scope.ActorUserId);
        return scope.ActorUserId;
    }
    private static CreateLeaseAcquisition Intent(string field, CreateLeaseAcquisition? request)
    {
        if (request is null) throw RefusalException.Invalid("Request body is required.");
        var intent = request.Snapshot();
        if (intent.PropertyId != field || intent.Context?.FieldId != field) throw RefusalException.Invalid("The creation intent must match the route's FIELD.");
        return intent;
    }
}
