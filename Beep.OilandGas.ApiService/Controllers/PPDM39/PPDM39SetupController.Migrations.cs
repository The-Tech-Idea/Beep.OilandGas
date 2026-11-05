using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39;

public partial class PPDM39SetupController
{
    [HttpPost("schema/plan")]
    public async Task<ActionResult<SchemaMigrationPlanResult>> PlanSchemaMigration([FromBody] SchemaMigrationPlanRequest request, [FromServices] IRepositoryAccessService access)
    {
        await MigrationAdministratorAuthority.RequireAsync(User, access, HttpContext.RequestAborted);
        var result = await _schemaMigrationService.PlanSchemaMigrationAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("schema/approve")]
    public async Task<ActionResult<SchemaMigrationApprovalResult>> ApproveSchemaMigration([FromBody] SchemaMigrationApprovalRequest request, [FromServices] IRepositoryAccessService access)
    {
        request.ApprovedBy = await MigrationAdministratorAuthority.RequireAsync(User, access, HttpContext.RequestAborted);
        var result = await _schemaMigrationService.ApproveSchemaMigrationPlanAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("schema/execute")]
    public async Task<ActionResult<SchemaMigrationExecuteResult>> ExecuteSchemaMigration([FromBody] SchemaMigrationExecuteRequest request, [FromServices] IRepositoryAccessService access)
    {
        request.ExecutedBy = await MigrationAdministratorAuthority.RequireAsync(User, access, HttpContext.RequestAborted);
        var result = await _schemaMigrationService.ExecuteSchemaMigrationPlanAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("schema/start")]
    public async Task<ActionResult<OperationStartResponse>> StartSchemaMigration([FromBody] SchemaMigrationExecuteRequest request, [FromServices] IRepositoryAccessService access)
    {
        request.ExecutedBy = await MigrationAdministratorAuthority.RequireAsync(User, access, HttpContext.RequestAborted);
        var result = await _schemaMigrationService.StartSchemaMigrationExecutionAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
