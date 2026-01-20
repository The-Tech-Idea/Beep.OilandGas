using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 defaults and well status facets
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/defaults")]
    public class PPDM39DefaultsController : ControllerBase
    {
        private readonly IPPDM39DefaultsRepository _defaultsRepository;
        private readonly Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL.WellServices _wellServices;
        private readonly ILogger<PPDM39DefaultsController> _logger;

        public PPDM39DefaultsController(
            IPPDM39DefaultsRepository defaultsRepository,
            Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL.WellServices wellServices,
            ILogger<PPDM39DefaultsController> logger)
        {
            _defaultsRepository = defaultsRepository ?? throw new ArgumentNullException(nameof(defaultsRepository));
            _wellServices = wellServices ?? throw new ArgumentNullException(nameof(wellServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get default values for an entity type
        /// </summary>
        [HttpGet("{entityType}")]
        public async Task<ActionResult<Dictionary<string, object>>> GetDefaults(string entityType, [FromQuery] string? connectionName = null)
        {
            try
            {
                _logger.LogInformation("Getting defaults for entity type {EntityType}", entityType);
                
                var result = new Dictionary<string, object>
                {
                    { "ActiveIndicator", _defaultsRepository.GetActiveIndicatorYes() },
                    { "RowQuality", _defaultsRepository.GetDefaultRowQuality() },
                    { "PreferredIndicator", _defaultsRepository.GetDefaultPreferredIndicator() },
                    { "CertifiedIndicator", _defaultsRepository.GetDefaultCertifiedIndicator() }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting defaults for entity type {EntityType}", entityType);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get well status facets
        /// </summary>
        [HttpGet("well-status/{statusId}/facets")]
        public async Task<ActionResult> GetWellStatusFacets(string statusId, [FromQuery] string? connectionName = null)
        {
            try
            {
                _logger.LogInformation("Getting well status facets for status {StatusId}", statusId);
                var facets = await _wellServices.GetWellStatusFacetsAsync(statusId);
                
                return Ok(facets ?? new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting well status facets for status {StatusId}", statusId);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get active indicator default value
        /// </summary>
        [HttpGet("active-indicator")]
        public ActionResult<string> GetActiveIndicator()
        {
            try
            {
                var activeIndicator = _defaultsRepository.GetActiveIndicatorYes();
                return Ok(new { value = activeIndicator });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active indicator");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get inactive indicator default value
        /// </summary>
        [HttpGet("inactive-indicator")]
        public ActionResult<string> GetInactiveIndicator()
        {
            try
            {
                var inactiveIndicator = _defaultsRepository.GetActiveIndicatorNo();
                return Ok(new { value = inactiveIndicator });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inactive indicator");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
