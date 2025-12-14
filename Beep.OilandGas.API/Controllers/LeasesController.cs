using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.LeaseAcquisition.Services;

namespace Beep.OilandGas.API.Controllers
{
    /// <summary>
    /// API controller for lease acquisition and management.
    /// </summary>
    [ApiController]
    [Route("api/v1/leases")]
    [Produces("application/json")]
    public class LeasesController : ControllerBase
    {
        private readonly ILeaseManagementService _leaseService;
        private readonly ILogger<LeasesController> _logger;

        public LeasesController(
            ILeaseManagementService leaseService,
            ILogger<LeasesController> logger)
        {
            _leaseService = leaseService ?? throw new ArgumentNullException(nameof(leaseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all leases.
        /// </summary>
        /// <param name="fieldId">Optional field ID to filter leases.</param>
        /// <returns>List of leases.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<LeaseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LeaseDto>>> GetLeases([FromQuery] string? fieldId = null)
        {
            try
            {
                var leases = await _leaseService.GetLeasesAsync(fieldId);
                return Ok(leases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting leases");
                return StatusCode(500, new { error = "An error occurred while retrieving leases." });
            }
        }

        /// <summary>
        /// Gets a lease by ID.
        /// </summary>
        /// <param name="id">Lease ID.</param>
        /// <returns>Lease details.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LeaseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LeaseDto>> GetLease(string id)
        {
            try
            {
                var lease = await _leaseService.GetLeaseAsync(id);
                if (lease == null)
                    return NotFound(new { error = $"Lease with ID {id} not found." });

                return Ok(lease);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lease {LeaseId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the lease." });
            }
        }

        /// <summary>
        /// Creates a new lease.
        /// </summary>
        /// <param name="createDto">Lease creation data.</param>
        /// <returns>Created lease.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(LeaseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LeaseDto>> CreateLease([FromBody] CreateLeaseDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var lease = await _leaseService.CreateLeaseAsync(createDto);
                return CreatedAtAction(nameof(GetLease), new { id = lease.LeaseId }, lease);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lease");
                return StatusCode(500, new { error = "An error occurred while creating the lease." });
            }
        }

        /// <summary>
        /// Updates a lease.
        /// </summary>
        /// <param name="id">Lease ID.</param>
        /// <param name="updateDto">Lease update data.</param>
        /// <returns>Updated lease.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(LeaseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LeaseDto>> UpdateLease(string id, [FromBody] UpdateLeaseDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var lease = await _leaseService.UpdateLeaseAsync(id, updateDto);
                return Ok(lease);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Lease with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lease {LeaseId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the lease." });
            }
        }

        /// <summary>
        /// Renews a lease.
        /// </summary>
        /// <param name="id">Lease ID.</param>
        /// <param name="newExpirationDate">New expiration date.</param>
        /// <returns>Updated lease.</returns>
        [HttpPost("{id}/renew")]
        [ProducesResponseType(typeof(LeaseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LeaseDto>> RenewLease(string id, [FromBody] DateTime newExpirationDate)
        {
            try
            {
                var lease = await _leaseService.RenewLeaseAsync(id, newExpirationDate);
                return Ok(lease);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = $"Lease with ID {id} not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing lease {LeaseId}", id);
                return StatusCode(500, new { error = "An error occurred while renewing the lease." });
            }
        }

        /// <summary>
        /// Gets leases expiring within specified days.
        /// </summary>
        /// <param name="days">Number of days to look ahead.</param>
        /// <returns>List of expiring leases.</returns>
        [HttpGet("expiring")]
        [ProducesResponseType(typeof(List<LeaseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LeaseDto>>> GetExpiringLeases([FromQuery] int days = 30)
        {
            try
            {
                var leases = await _leaseService.GetExpiringLeasesAsync(days);
                return Ok(leases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring leases");
                return StatusCode(500, new { error = "An error occurred while retrieving expiring leases." });
            }
        }

        /// <summary>
        /// Gets land rights for a lease.
        /// </summary>
        /// <param name="id">Lease ID.</param>
        /// <returns>List of land rights.</returns>
        [HttpGet("{id}/land-rights")]
        [ProducesResponseType(typeof(List<LandRightDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LandRightDto>>> GetLandRights(string id)
        {
            try
            {
                var landRights = await _leaseService.GetLandRightsAsync(id);
                return Ok(landRights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting land rights for lease {LeaseId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving land rights." });
            }
        }

        /// <summary>
        /// Gets mineral rights for a lease.
        /// </summary>
        /// <param name="id">Lease ID.</param>
        /// <returns>List of mineral rights.</returns>
        [HttpGet("{id}/mineral-rights")]
        [ProducesResponseType(typeof(List<MineralRightDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<MineralRightDto>>> GetMineralRights(string id)
        {
            try
            {
                var mineralRights = await _leaseService.GetMineralRightsAsync(id);
                return Ok(mineralRights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mineral rights for lease {LeaseId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving mineral rights." });
            }
        }

        /// <summary>
        /// Gets royalties for a lease.
        /// </summary>
        /// <param name="id">Lease ID.</param>
        /// <returns>List of royalties.</returns>
        [HttpGet("{id}/royalties")]
        [ProducesResponseType(typeof(List<RoyaltyDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RoyaltyDto>>> GetRoyalties(string id)
        {
            try
            {
                var royalties = await _leaseService.GetRoyaltiesAsync(id);
                return Ok(royalties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting royalties for lease {LeaseId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving royalties." });
            }
        }
    }
}

