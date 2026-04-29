using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Royalty;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Royalty
{
    /// <summary>
    /// Service-backed royalty dispute endpoints aligned with <see cref="IRoyaltyDisputeService"/>.
    /// </summary>
    [ApiController]
    [Route("api/accounting/royalty/disputes")]
    public class RoyaltyDisputeController : ControllerBase
    {
        private readonly IRoyaltyDisputeService _disputeService;
        private readonly ILogger<RoyaltyDisputeController> _logger;

        public RoyaltyDisputeController(
            IRoyaltyDisputeService disputeService,
            ILogger<RoyaltyDisputeController> logger)
        {
            _disputeService = disputeService ?? throw new ArgumentNullException(nameof(disputeService));
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ROYALTY_DISPUTE>> CreateAsync(
            [FromBody] ROYALTY_DISPUTE dispute,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _disputeService.CreateDisputeAsync(dispute, ResolveUserId(), connectionName ?? "PPDM39"),
                "Error creating royalty dispute");
        }

        [HttpPost("{disputeId}/resolve")]
        public async Task<ActionResult<ROYALTY_DISPUTE>> ResolveAsync(
            string disputeId,
            [FromBody] ResolveRoyaltyDisputeRequest request,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _disputeService.ResolveDisputeAsync(
                    disputeId,
                    request.ResolutionDate ?? DateTime.UtcNow,
                    request.ResolutionNotes ?? string.Empty,
                    ResolveUserId(),
                    connectionName ?? "PPDM39"),
                "Error resolving royalty dispute");
        }

        [HttpGet]
        public async Task<ActionResult<List<ROYALTY_DISPUTE>>> GetAsync(
            [FromQuery] string royaltyOwnerBaId,
            [FromQuery] string? status = null,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _disputeService.GetDisputesAsync(royaltyOwnerBaId, status, connectionName ?? "PPDM39"),
                "Error fetching royalty disputes");
        }

        private async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> action, string logMessage)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await action();
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "{Message}", logMessage);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "{Message}", logMessage);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", logMessage);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string ResolveUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? "system";
        }
    }

}
