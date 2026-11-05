using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using TheTechIdea.Beep.Editor;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using ValidationResult = Beep.OilandGas.Models.Data.ValidationResult;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// Generic data management API controller for PPDM39 tables
    /// Provides CRUD operations, validation, and quality operations for any PPDM39 table
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            var repository = await GetRepositoryForTable(tableName);
            if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found." });

            var records = await repository.GetAsync(filters ?? new List<AppFilter>());
            return Ok(records.ToList());
        }

        /// <summary>
        /// Get a single record by ID
        /// GET /api/datamanagement/{tableName}/{id}
        /// </summary>
        [HttpGet("{tableName}/{id}")]
        public async Task<ActionResult<object>> GetTableRecord(string tableName, string id)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Record ID is required." });
            var repository = await GetRepositoryForTable(tableName);
            if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found." });

            // A composite key's values were handed to GetByIdAsync as a list, which refuses a composite key with a fault;
            // the record is found by its whole key, as the delete finds it.
            var record = await FindByKeyAsync(repository, tableName, id);
            if (record == null)
                return NotFound(new { error = $"Record with ID '{id}' not found in table '{tableName}'." });

            return Ok(record);
        }

        /// <summary>
        /// Create a new record
        /// POST /api/datamanagement/{tableName}
        /// </summary>
        [HttpPost("{tableName}")]
        public async Task<ActionResult<object>> CreateTableRecord(string tableName, [FromBody] JsonElement entityJson)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            var repository = await GetRepositoryForTable(tableName);
            if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found." });

            // Convert JSON to entity object
            var entity = ConvertJsonToEntity(entityJson, repository.EntityType);
            if (entity == null)
                return BadRequest(new { error = "Invalid entity data." });

            var createdEntity = await repository.InsertAsync(entity, userId);
            var recordId = PPDMRecordKey.Of(createdEntity, await _metadata.GetTableMetadataAsync(repository.TableName));
            return CreatedAtAction(nameof(GetTableRecord), new { tableName, id = recordId }, createdEntity);
        }

        /// <summary>
        /// Create multiple records in batch
        /// POST /api/datamanagement/{tableName}/batch
        /// </summary>
        [HttpPost("{tableName}/batch")]
        public async Task<ActionResult<List<object>>> CreateTableRecordsBatch(string tableName, [FromBody] List<JsonElement> entitiesJson)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            if (entitiesJson == null || entitiesJson.Count == 0)
                    return BadRequest(new { error = "Entities array is required." });

            var repository = await GetRepositoryForTable(tableName);
            if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found." });

            // Convert JSON array to entity objects
            var entities = new List<object>();
            foreach (var json in entitiesJson)
            {
                var entity = ConvertJsonToEntity(json, repository.EntityType);
                if (entity != null)
                    entities.Add(entity);
            }

            if (entities.Count == 0)
                return BadRequest(new { error = "No valid entities to create." });

            var createdEntities = await repository.InsertBatchAsync(entities, userId);
            return Ok(createdEntities.ToList());
        }

        /// <summary>
        /// Update an existing record by its key (<see cref="PPDMRecordKey"/>: its key columns' values, comma-separated)
        /// PUT /api/datamanagement/{tableName}/{id}
        /// </summary>
        [HttpPut("{tableName}/{id}")]
        public async Task<ActionResult<object>> UpdateTableRecord(string tableName, string id, [FromBody] JsonElement entityJson)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Record ID is required." });
            var repository = await GetRepositoryForTable(tableName);
            if (repository == null)
                return NotFound(new { error = $"Table '{tableName}' not found." });

            // Convert JSON to entity object
            var entity = ConvertJsonToEntity(entityJson, repository.EntityType);
            if (entity == null)
                return BadRequest(new { error = "Invalid entity data." });

            // The address names the record to change and the body must be that record: the update is made by the body's own
            // key, so a body naming another record changed that one under this record's address.
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (PPDMRecordKey.Columns(metadata).Count > 0 && PPDMRecordKey.Of(entity, metadata) != RouteKey(id))
                throw RefusalException.Invalid($"The record sent is not the {tableName} record '{id}': its key does not match.");

            var updatedEntity = await repository.UpdateAsync(entity, userId);
            return Ok(updatedEntity);
        }

        /// <summary>
        /// Update an existing record (alternative - sends full object with ID included)
        /// PUT /api/datamanagement/{tableName}
        /// </summary>
        [HttpPut("{tableName}")]
        public async Task<ActionResult<object>> UpdateTableRecordFull(string tableName, [FromBody] JsonElement entityJson)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            var repository = await GetRepositoryForTable(tableName);
            if (repository == null)
                return NotFound(new { error = $"Table '{tableName}' not found." });

            // Convert JSON to entity object
            var entity = ConvertJsonToEntity(entityJson, repository.EntityType);
            if (entity == null)
                return BadRequest(new { error = "Invalid entity data." });

            var updatedEntity = await repository.UpdateAsync(entity, userId);
            return Ok(updatedEntity);
        }

        /// <summary>
        /// Delete a record (soft delete: ACTIVE_IND = 'N', the row kept), named by its key (<see cref="PPDMRecordKey"/>: its
        /// key columns' values, comma-separated)
        /// DELETE /api/datamanagement/{tableName}/{id}
        /// </summary>
        [HttpDelete("{tableName}/{id}")]
        public async Task<ActionResult> DeleteTableRecord(string tableName, string id)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Record ID is required." });
            var repository = await GetRepositoryForTable(tableName);
            if (repository == null)
                    return NotFound(new { error = $"Table '{tableName}' not found." });

            // The key was handed to DeleteAsync, which takes the record itself, so no delete through this endpoint ever
            // removed anything. The record is found by its whole key and soft-deleted, as this endpoint says and as
            // PPDM39GenericDataService's delete does.
            var record = await FindByKeyAsync(repository, tableName, id);
            if (record == null)
                return NotFound(new { error = $"Record with ID '{id}' not found in table '{tableName}'." });

            _commonColumnHandler.SoftDelete((IPPDMEntity)record, userId);
            await repository.UpdateAsync(record, userId);
            return NoContent();
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
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            var result = await _validationService.ValidateAsync(entity, tableName);
            return Ok(result);
        }

        /// <summary>
        /// Get validation rules for a table
        /// GET /api/datamanagement/validate/{tableName}/rules
        /// </summary>
        [HttpGet("validate/{tableName}/rules")]
        public async Task<ActionResult<List<ValidationRule>>> GetValidationRules(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            var rules = await _validationService.GetValidationRulesAsync(tableName);
            return Ok(rules);
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
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
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
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
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

        // ============================================
        // DATA QUALITY OPERATIONS
        // ============================================

        /// <summary>
        /// Get quality metrics for a table
        /// GET /api/datamanagement/quality/{tableName}/metrics
        /// </summary>
        [HttpGet("quality/{tableName}/metrics")]
        public async Task<ActionResult<DATA_QUALITY_METRICS>> GetQualityMetrics(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            var metrics = await _qualityService.CalculateTableQualityMetricsAsync(tableName);
            return Ok(metrics);
        }

        /// <summary>
        /// Get quality dashboard data for a table
        /// GET /api/datamanagement/quality/{tableName}/dashboard
        /// </summary>
        [HttpGet("quality/{tableName}/dashboard")]
        public async Task<ActionResult<QualityDashboardData>> GetQualityDashboard(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            var dashboard = await _qualityDashboardService.GetDashboardDataAsync(tableName);
            return Ok(dashboard);
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
            var alerts = await _qualityDashboardService.GetQualityAlertsAsync(tableName, severity);
            return Ok(alerts);
        }

        // ============================================
        // METADATA ENDPOINT
        // ============================================

        /// <summary>
        /// Get metadata for a specific table
        /// GET /api/datamanagement/metadata/{tableName}
        /// </summary>
        [HttpGet("metadata/{tableName}")]
        public async Task<ActionResult<PPDMTableMetadata>> GetTableMetadata(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            var meta = await _metadata.GetTableMetadataAsync(tableName);
            if (meta == null)
                    return NotFound(new { error = $"Metadata not found for table '{tableName}'." });

            return Ok(meta);
        }

        /// <summary>
        /// Validate all rows in a table and return aggregated issues.
        /// POST /api/datamanagement/validate-table/{tableName}
        /// </summary>
        [HttpPost("validate-table/{tableName}")]
        public async Task<ActionResult<object>> ValidateTable(
            string tableName,
            [FromBody] TableValidationOptions? options = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required." });
            var rules = await _validationService.GetValidationRulesAsync(tableName);
            var repo  = await GetRepositoryForTable(tableName);
            if (repo == null)
                    return NotFound(new { error = $"Table '{tableName}' not found or has no mapped entity type." });

            var allEntities = (await repo.GetAsync(new List<AppFilter>
            {
                new() { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            })).ToList();

            int rowsChecked  = allEntities.Count;
            var issueGroups  = new Dictionary<string, (int Count, string Severity, string Column, string Rule)>();

            foreach (var entity in allEntities)
            {
                    var result = await _validationService.ValidateAsync(entity, tableName);
                    if (result?.Errors != null)
                    {
                        foreach (var e in result.Errors)
                        {
                            var key = $"Error|{e.FieldName}|{e.ErrorMessage}";
                            if (!issueGroups.TryGetValue(key, out var g))
                                issueGroups[key] = (1, "Error", e.FieldName ?? string.Empty, e.ErrorMessage ?? string.Empty);
                            else
                                issueGroups[key] = (g.Count + 1, g.Severity, g.Column, g.Rule);
                        }
                    }
                    if (result?.Warnings != null)
                    {
                        foreach (var w in result.Warnings)
                        {
                            var key = $"Warning|{w.FieldName}|{w.WarningMessage}";
                            if (!issueGroups.TryGetValue(key, out var g))
                                issueGroups[key] = (1, "Warning", w.FieldName ?? string.Empty, w.WarningMessage ?? string.Empty);
                            else
                                issueGroups[key] = (g.Count + 1, g.Severity, g.Column, g.Rule);
                        }
                    }
            }

            var issues = issueGroups.Values
                .Select(x => new { x.Rule, x.Column, Message = x.Rule, x.Severity, RowCount = x.Count })
                .ToList();

            return Ok(new { RowsChecked = rowsChecked, Issues = issues });
        }

        public class TableValidationOptions
        {
            public bool CheckCompleteness { get; set; } = true;
            public bool CheckConstraints  { get; set; } = true;
            public bool CheckReferences   { get; set; } = false;
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        /// <summary>
        /// Gets a repository instance for a table name
        /// </summary>
        private async Task<PPDMGenericRepository?> GetRepositoryForTable(string tableName)
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

        /// <summary>
        /// Gets the entity type for a table name using metadata — the web's generic table page reads the rows as the same
        /// class, through the same owner.
        /// </summary>
        private async Task<Type?> GetEntityTypeForTableAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return null;

            var tableMetadata = await _metadata.GetTableMetadataAsync(tableName);
            return PPDMEntityTypes.Find(tableMetadata?.EntityTypeName, tableName);
        }

        /// <summary>
        /// The record a key names (<see cref="PPDMRecordKey"/>), or null when there is none. The text columns of the key
        /// narrow the query; every column of the key, compared as the key writes it, decides the match, so a number or a
        /// date in a key is not compared as text by the data source.
        /// </summary>
        private async Task<object?> FindByKeyAsync(PPDMGenericRepository repository, string tableName, string routeKey)
        {
            var key = RouteKey(routeKey);
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            var columns = PPDMRecordKey.Columns(metadata);
            if (columns.Count == 0)
                throw RefusalException.Invalid($"Table '{tableName}' names no key, so a record cannot be named in it.");

            var parts = PPDMRecordKey.Read(key, metadata)
                ?? throw RefusalException.Invalid(
                    $"'{key}' does not name a {tableName} record: give a value for each key column ({string.Join(", ", columns)}), separated by commas.");

            var filters = parts
                .Where(part => repository.EntityType.GetProperty(part.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.PropertyType == typeof(string))
                .Select(part => new AppFilter { FieldName = part.Key, Operator = "=", FilterValue = part.Value })
                .ToList();

            var matches = (await repository.GetAsync(filters))
                .Where(row => PPDMRecordKey.Of(row, metadata) == key)
                .ToList();

            return matches.Count switch
            {
                0 => null,
                1 => matches[0],
                _ => throw RefusalException.Conflict($"More than one {tableName} record has the key '{key}', so none was changed.")
            };
        }

        /// <summary>
        /// A record key as its address carried it. Kestrel decodes a request path except for an escaped slash, which it
        /// leaves as <c>%2F</c> in the route value, so a key holding a slash (a Canadian UWI, <c>100/06-11-033-24W4/00</c>)
        /// would never match its record.
        /// </summary>
        private static string RouteKey(string id) => id.Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Converts JSON element to entity object. JSON that is not a record of the table's entity is refused in this API's
        /// words; System.Text.Json has no question for it, so its exception is the answer and stays inside the refusal.
        /// </summary>
        private static object? ConvertJsonToEntity(JsonElement json, Type entityType)
        {
            var jsonString = json.GetRawText();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            try
            {
                return JsonSerializer.Deserialize(jsonString, entityType, options);
            }
            catch (JsonException notAnEntity)
            {
                throw new RefusalException(RefusalKind.Invalid, $"The data is not a valid {entityType.Name} record.", notAnEntity);
            }
        }
    }
}
