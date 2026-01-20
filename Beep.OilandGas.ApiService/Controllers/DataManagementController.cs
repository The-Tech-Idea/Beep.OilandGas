using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using TheTechIdea.Beep.Editor;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// Generic data management API controller for PPDM39 tables
    /// Provides CRUD operations, validation, and quality operations for any PPDM39 table
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataManagementController : ControllerBase
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IPPDMDataValidationService _validationService;
        private readonly IPPDMDataQualityService _qualityService;
        private readonly IPPDMDataQualityDashboardService _qualityDashboardService;
        private readonly ILogger<DataManagementController> _logger;
        private const string ConnectionName = "PPDM39";

        public DataManagementController(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IPPDMDataValidationService validationService,
            IPPDMDataQualityService qualityService,
            IPPDMDataQualityDashboardService qualityDashboardService,
            ILogger<DataManagementController> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _qualityService = qualityService ?? throw new ArgumentNullException(nameof(qualityService));
            _qualityDashboardService = qualityDashboardService ?? throw new ArgumentNullException(nameof(qualityDashboardService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================
        // GENERIC CRUD OPERATIONS
        // ============================================

        /// <summary>
        /// Get all records from a table with optional filters
        /// GET /api/datamanagement/{tableName}
        /// </summary>
        [HttpGet("{tableName}")]
        public async Task<ActionResult<List<object>>> GetTableRecords(string tableName, [FromQuery] List<AppFilter>? filters = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                var repository = await GetRepositoryForTable(tableName);
                if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                var records = await repository.GetAsync(filters ?? new List<AppFilter>());
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
        /// GET /api/datamanagement/{tableName}/{id}
        /// </summary>
        [HttpGet("{tableName}/{id}")]
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
        /// POST /api/datamanagement/{tableName}
        /// </summary>
        [HttpPost("{tableName}")]
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
                var recordId = GetRecordId(createdEntity, repository);
                return CreatedAtAction(nameof(GetTableRecord), new { tableName, id = recordId }, createdEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating record in table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create multiple records in batch
        /// POST /api/datamanagement/{tableName}/batch
        /// </summary>
        [HttpPost("{tableName}/batch")]
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
        /// Update an existing record by ID
        /// PUT /api/datamanagement/{tableName}/{id}
        /// </summary>
        [HttpPut("{tableName}/{id}")]
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
        /// Update an existing record (alternative - sends full object with ID included)
        /// PUT /api/datamanagement/{tableName}
        /// </summary>
        [HttpPut("{tableName}")]
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
        /// Delete a record (soft delete)
        /// DELETE /api/datamanagement/{tableName}/{id}
        /// </summary>
        [HttpDelete("{tableName}/{id}")]
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
        // VALIDATION OPERATIONS
        // ============================================

        /// <summary>
        /// Validate an entity
        /// POST /api/datamanagement/validate/{tableName}
        /// </summary>
        [HttpPost("validate/{tableName}")]
        public async Task<ActionResult<ValidationResult>> ValidateEntity(string tableName, [FromBody] object entity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                var result = await _validationService.ValidateAsync(entity, tableName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating entity for table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get validation rules for a table
        /// GET /api/datamanagement/validate/{tableName}/rules
        /// </summary>
        [HttpGet("validate/{tableName}/rules")]
        public async Task<ActionResult<List<ValidationRule>>> GetValidationRules(string tableName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                var rules = await _validationService.GetValidationRulesAsync(tableName);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting validation rules for table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Validates foreign key values for imported data (batch)
        /// POST /api/datamanagement/validate-foreign-keys/{tableName}
        /// </summary>
        [HttpPost("validate-foreign-keys/{tableName}")]
        public async Task<ActionResult<List<ForeignKeyValidationError>>> ValidateForeignKeys(
            string tableName,
            [FromBody] List<Dictionary<string, string>> rows)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                if (rows == null || rows.Count == 0)
                    return Ok(new List<ForeignKeyValidationError>());

                // Get entity type for the table
                var entityType = await GetEntityTypeForTableAsync(tableName);
                if (entityType == null)
                    return BadRequest(new { error = $"Entity type not found for table: {tableName}" });

                // Create repository for the table
                var repository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    ConnectionName,
                    tableName,
                    null);

                // Validate all rows
                var errors = await repository.ValidateForeignKeyValuesBatchAsync(rows);

                return Ok(errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating foreign keys for table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Validates a single row's foreign key values
        /// POST /api/datamanagement/validate-foreign-keys/{tableName}/row
        /// </summary>
        [HttpPost("validate-foreign-keys/{tableName}/row")]
        public async Task<ActionResult<List<ForeignKeyValidationError>>> ValidateForeignKeysRow(
            string tableName,
            [FromBody] Dictionary<string, string> row,
            [FromQuery] int rowNumber = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                if (row == null || row.Count == 0)
                    return Ok(new List<ForeignKeyValidationError>());

                // Get entity type for the table
                var entityType = await GetEntityTypeForTableAsync(tableName);
                if (entityType == null)
                    return BadRequest(new { error = $"Entity type not found for table: {tableName}" });

                // Create repository for the table
                var repository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    ConnectionName,
                    tableName,
                    null);

                // Validate the row
                var errors = await repository.ValidateForeignKeyValuesAsync(row, rowNumber);

                return Ok(errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating foreign keys for table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ============================================
        // DATA QUALITY OPERATIONS
        // ============================================

        /// <summary>
        /// Get quality metrics for a table
        /// GET /api/datamanagement/quality/{tableName}/metrics
        /// </summary>
        [HttpGet("quality/{tableName}/metrics")]
        public async Task<ActionResult<DataQualityMetrics>> GetQualityMetrics(string tableName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                var metrics = await _qualityService.CalculateTableQualityMetricsAsync(tableName);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality metrics for table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get quality dashboard data for a table
        /// GET /api/datamanagement/quality/{tableName}/dashboard
        /// </summary>
        [HttpGet("quality/{tableName}/dashboard")]
        public async Task<ActionResult<QualityDashboardData>> GetQualityDashboard(string tableName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required" });

                var dashboard = await _qualityDashboardService.GetDashboardDataAsync(tableName);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting quality dashboard for table: {TableName}", tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get quality alerts
        /// GET /api/datamanagement/quality/alerts
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
                var entityType = await GetEntityTypeForTableAsync(tableName);
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
                    tableName,
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating repository for table: {TableName}", tableName);
                return null;
            }
        }

        /// <summary>
        /// Gets the entity type for a table name using metadata
        /// </summary>
        private async Task<Type?> GetEntityTypeForTableAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return null;

            // Try to get metadata first
            var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
            if (tableMetadata != null && !string.IsNullOrWhiteSpace(tableMetadata.EntityTypeName))
            {
                var assembly = typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly;
                var entityType = assembly.GetType($"Beep.OilandGas.PPDM39.Models.{tableMetadata.EntityTypeName}");
                if (entityType != null)
                    return entityType;
            }

            // Fallback to assembly lookup
            var assembly2 = typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly;
            
            // Try exact match
            var entityType2 = assembly2.GetType($"Beep.OilandGas.PPDM39.Models.{tableName}");
            if (entityType2 != null)
                return entityType2;

            // Try case-insensitive search
            entityType2 = assembly2.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) &&
                                    typeof(IPPDMEntity).IsAssignableFrom(t));
            if (entityType2 != null)
                return entityType2;

            // Try removing underscores (e.g., STRAT_UNIT -> STRATUNIT)
            var normalizedName = tableName.Replace("_", "");
            entityType2 = assembly2.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(normalizedName, StringComparison.OrdinalIgnoreCase) &&
                                    typeof(IPPDMEntity).IsAssignableFrom(t));
            
            return entityType2;
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
}
