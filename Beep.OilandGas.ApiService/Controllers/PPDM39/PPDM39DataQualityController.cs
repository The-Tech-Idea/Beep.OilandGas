using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 data quality operations
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/quality")]
    public class PPDM39DataQualityController : ControllerBase
    {
        private readonly IPPDMDataQualityService _qualityService;
        private readonly IPPDMDataQualityDashboardService _dashboardService;
        private readonly ILogger<PPDM39DataQualityController> _logger;

        public PPDM39DataQualityController(
            IPPDMDataQualityService qualityService,
            IPPDMDataQualityDashboardService dashboardService,
            ILogger<PPDM39DataQualityController> logger)
        {
            _qualityService = qualityService ?? throw new ArgumentNullException(nameof(qualityService));
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get data quality metrics for a table
        /// </summary>
        [HttpGet("{tableName}/metrics")]
        public async Task<ActionResult<DataQualityResult>> GetTableQualityMetrics(string tableName, [FromQuery] string? connectionName = null)
        {
            try
            {
                _logger.LogInformation("Getting data quality metrics for table {TableName}", tableName);
                var metrics = await _qualityService.CalculateTableQualityMetricsAsync(tableName);
                
                if (metrics == null)
                {
                    return NotFound(new { error = "Metrics not found" });
                }

                var result = new DataQualityResult
                {
                    TableName = tableName,
                    OverallQualityScore = metrics.OverallQualityScore,
                    TotalRows = metrics.TotalRecords,
                    CompleteRows = metrics.CompleteRecords,
                    FieldQualityScores = metrics.FieldMetrics?.ToDictionary(
                        f => f.Key,
                        f => f.Value.Completeness) ?? new System.Collections.Generic.Dictionary<string, double>(),
                    QualityIssues = new System.Collections.Generic.List<string>() // Issues are retrieved separately via FindQualityIssuesAsync
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality metrics for table {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get data quality dashboard for all tables
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<DataQualityDashboardResult>> GetQualityDashboard([FromQuery] string? connectionName = null)
        {
            try
            {
                _logger.LogInformation("Getting data quality dashboard");
                var dashboard = await _dashboardService.GetDashboardDataAsync(null);
                
                if (dashboard == null)
                {
                    return NotFound(new { error = "Dashboard data not found" });
                }

                var result = new DataQualityDashboardResult
                {
                    OverallQualityScore = dashboard.OverallQualityScore,
                    TotalTables = 1, // Single table dashboard
                    TablesWithIssues = dashboard.ActiveAlerts?.Count(a => !a.IsResolved) ?? 0,
                    TableQualityResults = dashboard.CurrentMetrics != null
                        ? new System.Collections.Generic.Dictionary<string, DataQualityResult>
                        {
                            {
                                dashboard.TABLE_NAME ?? tableName ?? "Unknown",
                                new DataQualityResult
                                {
                                    TableName = dashboard.TABLE_NAME ?? tableName ?? "Unknown",
                                    OverallQualityScore = dashboard.CurrentMetrics.OverallQualityScore,
                                    TotalRows = dashboard.CurrentMetrics.TotalRecords,
                                    CompleteRows = dashboard.CurrentMetrics.CompleteRecords,
                                    FieldQualityScores = dashboard.FieldQualityScores ?? new System.Collections.Generic.Dictionary<string, double>(),
                                    QualityIssues = dashboard.ActiveAlerts?.Where(a => !a.IsResolved).Select(a => a.AlertMessage).ToList() ?? new System.Collections.Generic.List<string>()
                                }
                            }
                        }
                        : new System.Collections.Generic.Dictionary<string, DataQualityResult>()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality dashboard");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Find data quality issues in a table
        /// </summary>
        [HttpGet("{tableName}/issues")]
        public async Task<ActionResult> GetQualityIssues(string tableName, [FromQuery] string[]? fields = null, [FromQuery] string? connectionName = null)
        {
            try
            {
                _logger.LogInformation("Finding quality issues for table {TableName}", tableName);
                var fieldList = fields?.ToList();
                var issues = await _qualityService.FindQualityIssuesAsync(tableName, fieldList);
                
                return Ok(issues ?? new System.Collections.Generic.List<DATA_QUALITY_ISSUE>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding quality issues for table {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
