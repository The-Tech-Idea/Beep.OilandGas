using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using System.Reflection;
using System.Text.Json;
using System.Linq;

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
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<PPDM39Controller> _logger;
        private const string ConnectionName = "PPDM39";

        public PPDM39Controller(
            IWellComparisonService wellComparisonService,
            IPPDMDataValidationService validationService,
            IPPDMDataQualityService qualityService,
            IPPDMDataQualityDashboardService qualityDashboardService,
            WellRepository wellRepository,
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<PPDM39Controller> logger)
        {
            _wellComparisonService = wellComparisonService;
            _validationService = validationService;
            _qualityService = qualityService;
            _qualityDashboardService = qualityDashboardService;
            _wellRepository = wellRepository;
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
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

        // ============================================
        // GENERIC TABLE CRUD ENDPOINTS
        // ============================================

        /// <summary>
        /// Get all records from a table
        /// GET /api/ppdm39/tables/{tableName}
        /// </summary>
        [HttpGet("tables/{tableName}")]
        public async Task<ActionResult<List<object>>> GetTableRecords(string tableName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                var repository = await GetRepositoryForTable(tableName);
                if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                var records = await repository.GetActiveAsync();
                return Ok(records.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting records for table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a single record by ID
        /// GET /api/ppdm39/tables/{tableName}/{id}
        /// </summary>
        [HttpGet("tables/{tableName}/{id}")]
        public async Task<ActionResult<object>> GetTableRecord(string tableName, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(new { error = "Record ID is required" });

                var repository = await GetRepositoryForTable(tableName);
                if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                // Handle composite keys (comma-separated)
                object recordId = id;
                if (id.Contains(','))
                {
                    recordId = id.Split(',').ToList();
                }

                var record = await repository.GetByIdAsync(recordId);
                if (record == null)
                    return NotFound(new { error = $"Record with ID '{id}' not found in table '{tableName}'" });

                return Ok(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting record {Id} from table: {TableName}", id, tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new record
        /// POST /api/ppdm39/tables/{tableName}
        /// </summary>
        [HttpPost("tables/{tableName}")]
        public async Task<ActionResult<object>> CreateTableRecord(string tableName, [FromBody] JsonElement entityJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                var repository = await GetRepositoryForTable(tableName);
                if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                // Convert JSON to entity object
                var entity = await ConvertJsonToEntity(entityJson, repository.EntityType);
                if (entity == null)
                    return BadRequest(new { error = "Invalid entity data" });

                // Get user ID from request (or use default)
                var userId = GetUserIdFromRequest() ?? "SYSTEM";

                var createdEntity = await repository.InsertAsync(entity, userId);
                return CreatedAtAction(nameof(GetTableRecord), new { tableName, id = GetRecordId(createdEntity, repository) }, createdEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating record in table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create multiple records in batch
        /// POST /api/ppdm39/tables/{tableName}/batch
        /// </summary>
        [HttpPost("tables/{tableName}/batch")]
        public async Task<ActionResult<List<object>>> CreateTableRecordsBatch(string tableName, [FromBody] List<JsonElement> entitiesJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                if (entitiesJson == null || entitiesJson.Count == 0)
                    return BadRequest(new { error = "Entities array is required" });

                var repository = await GetRepositoryForTable(tableName);
                if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                // Convert JSON array to entity objects
                var entities = new List<object>();
                foreach (var json in entitiesJson)
                {
                    var entity = await ConvertJsonToEntity(json, repository.EntityType);
                    if (entity != null)
                        entities.Add(entity);
                }

                if (entities.Count == 0)
                    return BadRequest(new { error = "No valid entities to create" });

                // Get user ID from request (or use default)
                var userId = GetUserIdFromRequest() ?? "SYSTEM";

                var createdEntities = await repository.InsertBatchAsync(entities, userId);
                return Ok(createdEntities.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating batch records in table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing record
        /// PUT /api/ppdm39/tables/{tableName}/{id}
        /// </summary>
        [HttpPut("tables/{tableName}/{id}")]
        public async Task<ActionResult<object>> UpdateTableRecord(string tableName, string id, [FromBody] JsonElement entityJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(new { error = "Record ID is required" });

                var repository = await GetRepositoryForTable(tableName);
                if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                // Convert JSON to entity object
                var entity = await ConvertJsonToEntity(entityJson, repository.EntityType);
                if (entity == null)
                    return BadRequest(new { error = "Invalid entity data" });

                // Get user ID from request (or use default)
                var userId = GetUserIdFromRequest() ?? "SYSTEM";

                var updatedEntity = await repository.UpdateAsync(entity, userId);
                return Ok(updatedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating record {Id} in table: {TableName}", id, tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing record (alternative - sends full object)
        /// PUT /api/ppdm39/tables/{tableName}
        /// </summary>
        [HttpPut("tables/{tableName}")]
        public async Task<ActionResult<object>> UpdateTableRecordFull(string tableName, [FromBody] JsonElement entityJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                var repository = await GetRepositoryForTable(tableName);
                if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                // Convert JSON to entity object
                var entity = await ConvertJsonToEntity(entityJson, repository.EntityType);
                if (entity == null)
                    return BadRequest(new { error = "Invalid entity data" });

                // Get user ID from request (or use default)
                var userId = GetUserIdFromRequest() ?? "SYSTEM";

                var updatedEntity = await repository.UpdateAsync(entity, userId);
                return Ok(updatedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating record in table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a record
        /// DELETE /api/ppdm39/tables/{tableName}/{id}
        /// </summary>
        [HttpDelete("tables/{tableName}/{id}")]
        public async Task<ActionResult> DeleteTableRecord(string tableName, string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(new { error = "Record ID is required" });

                var repository = await GetRepositoryForTable(tableName);
                if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                // Handle composite keys (comma-separated)
                object recordId = id;
                if (id.Contains(','))
                {
                    recordId = id.Split(',').ToList();
                }

                var deleted = await repository.DeleteAsync(recordId);
                if (!deleted)
                    return NotFound(new { error = $"Record with ID '{id}' not found in table '{tableName}'" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting record {Id} from table: {TableName}", id, tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        /// <summary>
        /// Gets a repository instance for a table name
        /// </summary>
        private async Task<PPDMGenericRepository?> GetRepositoryForTable(string tableName)
        {
            try
            {
                // Get table metadata to find entity type
                var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
                if (tableMetadata == null)
                {
                    _logger.LogWarning("Table metadata not found: {TableName}", tableName);
                    return null;
                }

                // Get entity type from metadata
                var entityType = GetEntityTypeForTable(tableMetadata.EntityTypeName, tableName);
                if (entityType == null)
                {
                    _logger.LogWarning("Entity type not found for table: {TableName}, EntityTypeName: {EntityTypeName}", 
                        tableName, tableMetadata.EntityTypeName);
                    return null;
                }

                // Create repository
                return new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    ConnectionName,
                    tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating repository for table: {TableName}", tableName);
                return null;
            }
        }

        /// <summary>
        /// Gets the entity type for a table name
        /// </summary>
        private Type? GetEntityTypeForTable(string entityTypeName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(entityTypeName) && string.IsNullOrWhiteSpace(tableName))
                return null;

            var assembly = typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly;

            // Try using entity type name from metadata first
            if (!string.IsNullOrWhiteSpace(entityTypeName))
            {
                var entityType = assembly.GetType($"Beep.OilandGas.PPDM39.Models.{entityTypeName}");
                if (entityType != null)
                    return entityType;
            }

            // Try using table name directly
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                var entityType = assembly.GetType($"Beep.OilandGas.PPDM39.Models.{tableName}");
                if (entityType != null)
                    return entityType;

                // Try case-insensitive search
                entityType = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) &&
                                        typeof(IPPDMEntity).IsAssignableFrom(t));
                if (entityType != null)
                    return entityType;

                // Try removing underscores (e.g., STRAT_UNIT -> STRATUNIT)
                var normalizedName = tableName.Replace("_", "");
                entityType = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase) &&
                                        typeof(IPPDMEntity).IsAssignableFrom(t));
                if (entityType != null)
                    return entityType;
            }

            return null;
        }

        /// <summary>
        /// Converts JSON element to entity object
        /// </summary>
        private async Task<object?> ConvertJsonToEntity(JsonElement json, Type entityType)
        {
            try
            {
                var jsonString = json.GetRawText();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                return JsonSerializer.Deserialize(jsonString, entityType, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting JSON to entity type: {EntityType}", entityType.Name);
                return null;
            }
        }

        /// <summary>
        /// Gets the record ID from an entity object
        /// </summary>
        private string? GetRecordId(object entity, PPDMGenericRepository repository)
        {
            try
            {
                var tableMetadata = _metadata.GetTableMetadataAsync(repository.TableName).GetAwaiter().GetResult();
                if (tableMetadata?.PrimaryKeyColumn == null)
                    return null;

                // Handle composite primary keys
                if (tableMetadata.PrimaryKeyColumn.Contains(','))
                {
                    var keyParts = tableMetadata.PrimaryKeyColumn.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    var values = new List<string>();

                    foreach (var keyPart in keyParts)
                    {
                        var prop = entity.GetType().GetProperty(keyPart);
                        var value = prop?.GetValue(entity)?.ToString();
                        if (string.IsNullOrEmpty(value))
                            return null;
                        values.Add(value);
                    }

                    return string.Join(",", values);
                }
                else
                {
                    // Single primary key
                    var prop = entity.GetType().GetProperty(tableMetadata.PrimaryKeyColumn);
                    return prop?.GetValue(entity)?.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting record ID from entity");
                return null;
            }
        }

        /// <summary>
        /// Gets user ID from request headers or claims
        /// </summary>
        private string? GetUserIdFromRequest()
        {
            // Try to get from header
            if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
                return userIdHeader.ToString();

            // Try to get from user claims (if authentication is enabled)
            if (User?.Identity?.IsAuthenticated == true)
            {
                return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                       User.FindFirst("sub")?.Value ??
                       User.Identity.Name;
            }

            return null;
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



