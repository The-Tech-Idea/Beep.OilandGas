using Beep.OilandGas.ApiService.Attributes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Beep.OilandGas.ApiService.Controllers.Field;

[ApiController]
[Authorize]
[Route("api/fields/{fieldId}/production/dashboard")]
public sealed class ProductionDashboardController(IPPDMProductionService production, ILogger<ProductionDashboardController> logger) : ControllerBase
{
    [HttpGet]
    [RequireAssetAccess("fieldId")]
    public async Task<ActionResult<ProductionDashboardResponse>> Get(string fieldId)
    {
        if (string.IsNullOrWhiteSpace(fieldId)) return BadRequest();
        try
        {
            var summary = await production.GetProductionDashboardSummaryAsync(fieldId);
            if (summary is null || summary.FieldId != fieldId) throw new InvalidOperationException("Field summary mismatch.");
            var wells = await production.GetProductionWellStatusAsync(fieldId)
                ?? throw new InvalidOperationException("Well response unavailable.");
            return Ok(new ProductionDashboardResponse { Summary = summary, Wells = wells });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unable to load production dashboard for field {FieldId}", fieldId);
            return StatusCode(500, new { error = "Production dashboard could not be loaded." });
        }
    }
}
