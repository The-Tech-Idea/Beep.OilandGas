using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Repositories;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Repositories;
using TheTechIdea.Beep.Editor;
using System.Reflection;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// Generic data management API controller for PPDM39 tables
    /// Provides CRUD operations and validation for any PPDM39 table
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataManagementController : ControllerBase
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<DataManagementController> _logger;

        public DataManagementController(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<DataManagementController> logger)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _logger = logger;
        }

        /// <summary>
        /// Validates foreign key values for imported data
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
                var entityType = GetEntityTypeForTable(tableName);
                if (entityType == null)
                    return BadRequest(new { error = $"Entity type not found for table: {tableName}" });

                // Create repository for the table
                var repository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    "PPDM39",
                    tableName);

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
                var entityType = GetEntityTypeForTable(tableName);
                if (entityType == null)
                    return BadRequest(new { error = $"Entity type not found for table: {tableName}" });

                // Create repository for the table
                var repository = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    entityType,
                    "PPDM39",
                    tableName);

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

        /// <summary>
        /// Gets the entity type for a given table name
        /// </summary>
        private Type? GetEntityTypeForTable(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return null;

            // Try to find the entity type in the PPDM39.Models assembly
            var assembly = typeof(Beep.OilandGas.PPDM39.Models.STRAT_UNIT).Assembly;
            
            // Try exact match first
            var entityType = assembly.GetType($"Beep.OilandGas.PPDM39.Models.{tableName}");
            if (entityType != null)
                return entityType;

            // Try case-insensitive search
            entityType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) &&
                                    typeof(Beep.OilandGas.PPDM39.Core.Interfaces.IPPDMEntity).IsAssignableFrom(t));

            return entityType;
        }
    }
}
