using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// API controller for PPDM39 data management operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PPDM39Controller : ControllerBase
    {
        private readonly IWellComparisonService _wellComparisonService;
        private readonly IPPDMDataValidationService _validationService;
        private readonly IPPDMDataQualityService _qualityService;
        private readonly IPPDMDataQualityDashboardService _qualityDashboardService;
        private readonly WellRepository _wellRepository;
        private readonly ILogger<PPDM39Controller> _logger;

        public PPDM39Controller(
            IWellComparisonService wellComparisonService,
            IPPDMDataValidationService validationService,
            IPPDMDataQualityService qualityService,
            IPPDMDataQualityDashboardService qualityDashboardService,
            WellRepository wellRepository,
            ILogger<PPDM39Controller> logger)
        {
            _wellComparisonService = wellComparisonService;
            _validationService = validationService;
            _qualityService = qualityService;
            _qualityDashboardService = qualityDashboardService;
            _wellRepository = wellRepository;
            _logger = logger;
        }

        // ============================================
        // WELL COMPARISON ENDPOINTS
        // ============================================

        /// <summary>
        /// Compare multiple wells
        /// </summary>
        [HttpPost("wells/compare")]
        public async Task<ActionResult<WellComparisonDTO>> CompareWells([FromBody] CompareWellsRequest request)
        {
            try
            {
                var comparison = await _wellComparisonService.CompareWellsAsync(
                    request.WellIdentifiers, 
                    request.FieldNames);
                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing wells");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Compare wells from multiple data sources
        /// </summary>
        [HttpPost("wells/compare-multi-source")]
        public async Task<ActionResult<WellComparisonDTO>> CompareWellsMultiSource([FromBody] CompareWellsMultiSourceRequest request)
        {
            try
            {
                var comparison = await _wellComparisonService.CompareWellsFromMultipleSourcesAsync(
                    request.WellComparisons, 
                    request.FieldNames);
                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing wells from multiple sources");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get available comparison fields
        /// </summary>
        [HttpGet("wells/comparison-fields")]
        public async Task<ActionResult<List<ComparisonField>>> GetComparisonFields()
        {
            try
            {
                var fields = await _wellComparisonService.GetAvailableComparisonFieldsAsync();
                return Ok(fields);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comparison fields");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================
        // DATA VALIDATION ENDPOINTS
        // ============================================

        /// <summary>
        /// Validate an entity
        /// </summary>
        [HttpPost("validate/{tableName}")]
        public async Task<ActionResult<ValidationResult>> ValidateEntity(
            string tableName, 
            [FromBody] object entity)
        {
            try
            {
                var result = await _validationService.ValidateAsync(entity, tableName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating entity");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get validation rules for a table
        /// </summary>
        [HttpGet("validate/{tableName}/rules")]
        public async Task<ActionResult<List<ValidationRule>>> GetValidationRules(string tableName)
        {
            try
            {
                var rules = await _validationService.GetValidationRulesAsync(tableName);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting validation rules");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================
        // DATA QUALITY ENDPOINTS
        // ============================================

        /// <summary>
        /// Get quality metrics for a table
        /// </summary>
        [HttpGet("quality/{tableName}/metrics")]
        public async Task<ActionResult<DataQualityMetrics>> GetQualityMetrics(string tableName)
        {
            try
            {
                var metrics = await _qualityService.CalculateTableQualityMetricsAsync(tableName);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality metrics");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get quality dashboard data
        /// </summary>
        [HttpGet("quality/{tableName}/dashboard")]
        public async Task<ActionResult<QualityDashboardData>> GetQualityDashboard(string tableName)
        {
            try
            {
                var dashboard = await _qualityDashboardService.GetDashboardDataAsync(tableName);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality dashboard");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get quality alerts
        /// </summary>
        [HttpGet("quality/alerts")]
        public async Task<ActionResult<List<QualityAlert>>> GetQualityAlerts(
            [FromQuery] string? tableName = null,
            [FromQuery] QualityAlertSeverity? severity = null)
        {
            try
            {
                var alerts = await _qualityDashboardService.GetQualityAlertsAsync(tableName, severity);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality alerts");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // ============================================
    // REQUEST DTOs
    // ============================================

    public class CompareWellsRequest
    {
        public List<string> WellIdentifiers { get; set; } = new List<string>();
        public List<string>? FieldNames { get; set; }
    }

    public class CompareWellsMultiSourceRequest
    {
        public List<WellSourceMapping> WellComparisons { get; set; } = new List<WellSourceMapping>();
        public List<string>? FieldNames { get; set; }
    }
}


