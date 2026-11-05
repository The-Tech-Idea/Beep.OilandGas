using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Repositories;
using System.Reflection;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.SeedData.Services;
using Beep.OilandGas.UserManagement.Contracts.Services;
using TheTechIdea.Beep;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{

    /// <summary>
    /// API controller for PPDM39 database setup wizard
    /// </summary>
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("api/ppdm39/setup")]
    public partial class PPDM39SetupController : ControllerBase
    {
        private readonly PPDM39SetupService _setupService;
        private readonly IPPDM39SchemaMigrationService _schemaMigrationService;
        private readonly IDMEEditor _editor;
        private readonly ILogger<PPDM39SetupController> _logger;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMDemoDataSeeder? _demoDataSeeder;
        private readonly LOVManagementService? _lovManagementService;
        private readonly Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.CSVLOVImporter? _csvLovImporter;
        private readonly Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.PPDMStandardValueImporter? _ppdmStandardValueImporter;
        private readonly Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.IHSStandardValueImporter? _ihsStandardValueImporter;
        private readonly Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.StandardValueMapper? _standardValueMapper;
        private readonly Beep.OilandGas.PPDM39.DataManagement.SeedData.PPDMReferenceDataSeeder? _referenceDataSeeder;
        private readonly SeedDataCatalogCompatibility _seedDataCatalog;
        private readonly Beep.OilandGas.PPDM39.DataManagement.SeedData.WellStatusFacetSeeder? _wellStatusFacetSeeder;
        private readonly IFailureReporter _failures;

        public PPDM39SetupController(
            PPDM39SetupService setupService,
            IPPDM39SchemaMigrationService schemaMigrationService,
            IDMEEditor editor,
            ILogger<PPDM39SetupController> logger,
            IFailureReporter failures,
            IProgressTrackingService? progressTracking = null,
            ICommonColumnHandler? commonColumnHandler = null,
            IPPDM39DefaultsRepository? defaults = null,
            IPPDMMetadataRepository? metadata = null,
            LOVManagementService? lovManagementService = null,
            CSVLOVImporter? csvLovImporter = null,
            PPDMStandardValueImporter? ppdmStandardValueImporter = null,
            IHSStandardValueImporter? ihsStandardValueImporter = null,
           StandardValueMapper? standardValueMapper = null,
            PPDMReferenceDataSeeder? referenceDataSeeder = null,
            PPDMDemoDataSeeder? demoDataSeeder = null,
            Beep.OilandGas.PPDM39.DataManagement.SeedData.WellStatusFacetSeeder? wellStatusFacetSeeder = null)
        {
            _setupService = setupService ?? throw new ArgumentNullException(nameof(setupService));
            _schemaMigrationService = schemaMigrationService ?? (IPPDM39SchemaMigrationService)setupService;
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _seedDataCatalog = new SeedDataCatalogCompatibility();
            _lovManagementService = lovManagementService;
            _csvLovImporter = csvLovImporter;
            _ppdmStandardValueImporter = ppdmStandardValueImporter;
            _ihsStandardValueImporter = ihsStandardValueImporter;
            _standardValueMapper = standardValueMapper;
            _referenceDataSeeder = referenceDataSeeder;
            _demoDataSeeder = demoDataSeeder;
            _wellStatusFacetSeeder = wellStatusFacetSeeder;

            // Pass progress tracking to service if available
            if (progressTracking != null && _setupService is PPDM39SetupService setupSvc)
            {
                setupSvc.SetProgressTracking(progressTracking);
            }
        }

        /// <summary>
        /// Get available database types
        /// </summary>
        [HttpGet("database-types")]
        public ActionResult<List<string>> GetDatabaseTypes()
        {
            var types = _setupService.GetAvailableDatabaseTypes();
            return Ok(types);
        }

        // Helper: map ConnectionProperties (incoming) to ConnectionConfig used by services
        private ConnectionConfig MapToConnectionConfig(ConnectionProperties cp)
        {
            if (cp == null) return new ConnectionConfig();
            return new ConnectionConfig
            {
                ConnectionName = cp.ConnectionName ?? string.Empty,
                DatabaseType = cp.DatabaseType.ToString(),
                Host = cp.Host ?? string.Empty,
                Port = cp.Port,
                Database = cp.Database ?? string.Empty,
                Username = cp.UserID,
                Password = cp.Password,
                ConnectionString = cp.ConnectionString
            };
        }

        /// <summary>
        /// Test database connection (accepts ConnectionProperties from client)
        /// </summary>
        [HttpPost("test-connection")]
        public async Task<ActionResult<ConnectionTestResult>> TestConnection([FromBody] ConnectionProperties config)
        {
            if (config == null)
            {
                    return BadRequest(new { error = "Connection configuration is required." });
            }

            var mapped = MapToConnectionConfig(config);

            var result = await _setupService.TestConnectionAsync(mapped);
            return Ok(result);
        }

        /// <summary>
        /// Save connection to ConfigEditor
        /// </summary>
        [HttpPost("save-connection")]
        public ActionResult<SaveConnectionResult> SaveConnection([FromBody] SaveConnectionRequest request)
        {
            if (request == null || request.Connection == null)
            {
                    return BadRequest(new { error = "Connection configuration is required." });
            }

            var result = _setupService.SaveConnection(
                MapToConnectionConfig(request.Connection), 
                request.TestAfterSave, 
                request.OpenAfterSave);

            return Ok(result);
        }

        /// <summary>
        /// Get all database connections
        /// </summary>
        [HttpGet("connections")]
        public ActionResult<List<DatabaseConnectionListItem>> GetAllConnections()
        {
            var connections = _setupService.GetAllConnections();
            return Ok(connections);
        }

        /// <summary>
        /// Get connection by name
        /// </summary>
        [HttpGet("connection/{connectionName}")]
        public ActionResult<ConnectionConfig> GetConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return BadRequest(new { error = "Connection name is required." });
            var connection = _setupService.GetConnectionByName(connectionName);
            if (connection == null)
            {
                    return NotFound(new { error = $"Connection '{connectionName}' not found." });
            }
            return Ok(connection);
        }

        /// <summary>
        /// Get current connection name
        /// </summary>
        [HttpGet("current-connection")]
        public ActionResult<string?> GetCurrentConnection()
        {
            var connectionName = _setupService.GetCurrentConnectionName();
            return Ok(new { ConnectionName = connectionName });
        }

        /// <summary>
        /// Set current database connection
        /// </summary>
        [HttpPost("set-current-connection")]
        public ActionResult<SetCurrentDatabaseResult> SetCurrentConnection([FromBody] SetCurrentDatabaseRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ConnectionName))
            {
                    return BadRequest(new { error = "Connection name is required." });
            }

            var result = _setupService.SetCurrentConnection(request.ConnectionName);
            return Ok(result);
        }

        /// <summary>
        /// Update an existing connection
        /// </summary>
        [HttpPut("connection")]
        public ActionResult<SaveConnectionResult> UpdateConnection([FromBody] UpdateConnectionRequest request)
        {
            if (request == null || request.Connection == null || string.IsNullOrEmpty(request.OriginalConnectionName))
            {
                    return BadRequest(new { error = "Original connection name and connection configuration are required." });
            }

            var result = _setupService.UpdateConnection(
                request.OriginalConnectionName,
                MapToConnectionConfig(request.Connection),
                testAfterSave: true);
            return Ok(result);
        }

        /// <summary>Database copy is unavailable through the setup service.</summary>
        [HttpPost("copy-database")]
        public async Task<ActionResult<object>> CopyDatabase([FromBody] CopyDatabaseRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SourceConnectionName) ||
                string.IsNullOrWhiteSpace(request.TargetConnectionName))
                return BadRequest(new { error = "Source and target connection names are required." });
            return StatusCode(501, new CopyDatabaseResult
            {
                Success = false,
                Message = "Database copy is not implemented through the setup service."
            });
        }

        /// <summary>
        /// Delete a connection
        /// </summary>
        [HttpDelete("connection/{connectionName}")]
        public ActionResult<DeleteConnectionResult> DeleteConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return BadRequest(new { error = "Connection name is required." });
            var result = _setupService.DeleteConnection(connectionName);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Seed PPDM reference tables (R_* tables)
        /// </summary>
        [HttpPost("seed/reference")]
        public async Task<ActionResult<SeedDataResponse>> SeedReferenceData([FromBody] SeedDataRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null)
            {
                    return BadRequest(new { error = "Request is required." });
            }

            var connectionName = request.ConnectionName ?? "PPDM39";
            if (_referenceDataSeeder == null)
            {
                return StatusCode(500, new SeedDataResponse
                {
                    Success = false,
                    Message = "Reference data seeder is not available"
                });
            }
            
            var result = await _referenceDataSeeder.SeedPPDMReferenceTablesAsync(
                connectionName, 
                request.TableNames, 
                request.SkipExisting, 
                userId);

            return Ok(result);
        }

        /// <summary>
        /// Seed full demo dataset (reference data + sample entities + production data)
        /// </summary>
        [HttpPost("seed/demo-full")]
        public async Task<ActionResult<SeedDataResponse>> SeedFullDemoDataset([FromBody] SeedDataRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null)
            {
                    return BadRequest(new { error = "Request is required." });
            }

            var connectionName = request.ConnectionName ?? "PPDM39";
            
            if (_demoDataSeeder == null)
            {
                return BadRequest(new { error = "DemoDataSeeder is not available. Please ensure it is registered in dependency injection." });
            }

            var result = await _demoDataSeeder.SeedFullDemoDatasetAsync(userId);

            return Ok(new SeedDataResponse
            {
                Success = result.Success,
                Message = result.Message,
                TablesSeeded = result.TablesSeeded,
                RecordsInserted = result.RecordsInserted
            });
        }

        /// <summary>
        /// Seed data by category
        /// </summary>
        [HttpPost("seed/category/{category}")]
        public async Task<ActionResult<SeedDataResponse>> SeedByCategory(string category, [FromBody] SeedDataRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(category)) return BadRequest(new { error = "Category is required." });
            if (request == null)
            {
                    return BadRequest(new { error = "Request is required." });
            }

            var connectionName = request.ConnectionName ?? "PPDM39";
            if (_referenceDataSeeder == null)
            {
                return StatusCode(500, new SeedDataResponse
                {
                    Success = false,
                    Message = "Reference data seeder is not available"
                });
            }
            
            var result = await _referenceDataSeeder.SeedByCategoryAsync(
                category, 
                connectionName, 
                request.TableNames, 
                request.SkipExisting, 
                userId);

            return Ok(result);
        }

        /// <summary>
        /// Get available seed data categories
        /// </summary>
        [HttpGet("seed/categories")]
        public ActionResult<List<SeedDataCategory>> GetSeedDataCategories()
        {
            var categories = _seedDataCatalog.GetSeedDataCategories();
            return Ok(categories);
        }

        /// <summary>
        /// Validate seed data before insertion
        /// </summary>
        [HttpPost("seed/validate")]
        public async Task<ActionResult<SeedDataResponse>> ValidateSeedData([FromBody] SeedDataValidationRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null)
            {
                    return BadRequest(new { error = "Request is required." });
            }

            var connectionName = request.ConnectionName ?? "PPDM39";
            if (_referenceDataSeeder == null)
            {
                return StatusCode(500, new SeedDataResponse
                {
                    Success = false,
                    Message = "Reference data seeder is not available"
                });
            }
            
            // Validate by attempting to seed with validateOnly flag
            var seedRequest = new SeedDataRequest
            {
                ConnectionName = connectionName,
                TableNames = request.TableNames,
                ValidateOnly = true,
                SkipExisting = true
            };

            var result = await _referenceDataSeeder.SeedByCategoryAsync(
                request.Category, 
                connectionName, 
                request.TableNames, 
                true, 
                userId);

            return Ok(result);
        }

        /// <summary>
        /// Seed data for a specific workflow
        /// </summary>
        [HttpPost("seed/workflow/{workflowName}")]
        public async Task<ActionResult<SeedDataResponse>> SeedWorkflowData(string workflowName, [FromBody] SeedDataRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(workflowName)) return BadRequest(new { error = "Workflow name is required." });
            if (request == null)
            {
                    return BadRequest(new { error = "Request is required." });
            }

            if (_demoDataSeeder == null)
            {
                return StatusCode(500, new { error = "DemoDataSeeder is not available." });
            }
            var workflowRequirement = _demoDataSeeder.GetSeedDataForWorkflow(workflowName);
            if (workflowRequirement == null)
            {
                return NotFound(new { error = $"Workflow '{workflowName}' not found." });
            }

            var connectionName = request.ConnectionName ?? "PPDM39";
            if (_referenceDataSeeder == null)
            {
                return StatusCode(500, new SeedDataResponse
                {
                    Success = false,
                    Message = "Reference data seeder is not available"
                });
            }
            
            // Seed by category
            var result = await _referenceDataSeeder.SeedByCategoryAsync(
                workflowRequirement.WorkflowCategory, 
                connectionName, 
                request.TableNames ?? workflowRequirement.RequiredTables, 
                request.SkipExisting, 
                userId);

            return Ok(result);
        }

        #region LOV Management Endpoints

        /// <summary>
        /// Get all VALUE_TYPEs
        /// </summary>
        [HttpGet("lov/types")]
        public async Task<ActionResult<List<string>>> GetLOVTypes([FromQuery] string connectionName = "PPDM39")
        {
            if (_lovManagementService == null)
            {
                return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            var types = await _lovManagementService.GetValueTypesAsync(connectionName ?? "PPDM39");
            return Ok(types);
        }

        /// <summary>
        /// Get LOVs by type
        /// </summary>
        [HttpGet("lov/{valueType}")]
        public async Task<ActionResult<LOVResponse>> GetLOVByType(string valueType, [FromQuery] string? category = null, [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(valueType)) return BadRequest(new { error = "Value type is required." });
            if (_lovManagementService == null)
            {
                return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            var lovs = await _lovManagementService.GetLOVByTypeAsync(valueType, category, connectionName ?? "PPDM39");
            return Ok(new LOVResponse
            {
                Success = true,
                Message = $"Retrieved {lovs.Count} LOV(s)",
                LOVs = lovs,
                Count = lovs.Count
            });
        }

        /// <summary>
        /// Get LOVs by category
        /// </summary>
        [HttpGet("lov/category/{category}")]
        public async Task<ActionResult<LOVResponse>> GetLOVByCategory(string category, [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(category)) return BadRequest(new { error = "Category is required." });
            if (_lovManagementService == null)
            {
                return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            var lovs = await _lovManagementService.GetLOVByCategoryAsync(category, connectionName ?? "PPDM39");
            return Ok(new LOVResponse
            {
                Success = true,
                Message = $"Retrieved {lovs.Count} LOV(s)",
                LOVs = lovs,
                Count = lovs.Count
            });
        }

        /// <summary>
        /// Get LOVs by module
        /// </summary>
        [HttpGet("lov/module/{module}")]
        public async Task<ActionResult<LOVResponse>> GetLOVByModule(string module, [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(module)) return BadRequest(new { error = "Module is required." });
            if (_lovManagementService == null)
            {
                return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            var lovs = await _lovManagementService.GetLOVByModuleAsync(module, connectionName ?? "PPDM39");
            return Ok(new LOVResponse
            {
                Success = true,
                Message = $"Retrieved {lovs.Count} LOV(s)",
                LOVs = lovs,
                Count = lovs.Count
            });
        }

        /// <summary>
        /// Get LOVs by source
        /// </summary>
        [HttpGet("lov/source/{source}")]
        public async Task<ActionResult<LOVResponse>> GetLOVBySource(string source, [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(source)) return BadRequest(new { error = "Source is required." });
            if (_lovManagementService == null)
            {
                return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            var lovs = await _lovManagementService.GetLOVBySourceAsync(source, connectionName ?? "PPDM39");
            return Ok(new LOVResponse
            {
                Success = true,
                Message = $"Retrieved {lovs.Count} LOV(s)",
                LOVs = lovs,
                Count = lovs.Count
            });
        }

        /// <summary>
        /// Get hierarchical LOVs
        /// </summary>
        [HttpGet("lov/hierarchical/{valueType}")]
        public async Task<ActionResult<LOVResponse>> GetHierarchicalLOV(string valueType, [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(valueType)) return BadRequest(new { error = "Value type is required." });
            if (_lovManagementService == null)
            {
                return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            var lovs = await _lovManagementService.GetHierarchicalLOVAsync(valueType, connectionName ?? "PPDM39");
            return Ok(new LOVResponse
            {
                Success = true,
                Message = $"Retrieved {lovs.Count} hierarchical LOV(s)",
                LOVs = lovs,
                Count = lovs.Count
            });
        }

        /// <summary>
        /// Search LOVs
        /// </summary>
        [HttpGet("lov/search")]
        public async Task<ActionResult<LOVResponse>> SearchLOVs([FromQuery] string? searchTerm = null, [FromQuery] string? valueType = null, [FromQuery] string? category = null, [FromQuery] string? module = null, [FromQuery] string? source = null, [FromQuery] string connectionName = "PPDM39")
        {
            if (_lovManagementService == null)
            {
                return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            var filters = new LOVRequest
            {
                ConnectionName = connectionName,
                ValueType = valueType,
                Category = category,
                Module = module,
                Source = source
            };

            var lovs = await _lovManagementService.SearchLOVsAsync(searchTerm ?? string.Empty, filters, connectionName ?? "PPDM39");
            return Ok(new LOVResponse
            {
                Success = true,
                Message = $"Found {lovs.Count} LOV(s)",
                LOVs = lovs,
                Count = lovs.Count
            });
        }

        /// <summary>
        /// Add new LOV
        /// </summary>
        [HttpPost("lov")]
        public async Task<ActionResult<LOVResponse>> AddLOV([FromBody] ListOfValue lovDto, [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (_lovManagementService == null)
            {
                return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            if (lovDto == null)
            {
                return BadRequest(new { error = "LOV data is required." });
            }

            var lov = new LIST_OF_VALUE
            {
                LIST_OF_VALUE_ID = lovDto.ListOfValueId,
                VALUE_TYPE = lovDto.ValueType,
                VALUE_CODE = lovDto.ValueCode,
                VALUE_NAME = lovDto.ValueName,
                DESCRIPTION = lovDto.Description ?? string.Empty,
                CATEGORY = lovDto.Category ?? string.Empty,
                MODULE = lovDto.Module ?? string.Empty,
                SORT_ORDER = lovDto.SortOrder,
                PARENT_VALUE_ID = lovDto.ParentValueId ?? string.Empty,
                IS_DEFAULT = lovDto.IsDefault ?? string.Empty,
                ACTIVE_IND = lovDto.ActiveInd,
                SOURCE = lovDto.Source ?? string.Empty
            };

            var result = await _lovManagementService.AddLOVAsync(lov, userId, connectionName ?? "PPDM39");
            return Ok(new LOVResponse
            {
                Success = true,
                Message = "LOV added successfully",
                LOVs = new List<ListOfValue> { lovDto },
                Count = 1
            });
        }

        /// <summary>
        /// Update LOV
        /// </summary>
        [HttpPut("lov/{id}")]
        public async Task<ActionResult<LOVResponse>> UpdateLOV(string id, [FromBody] ListOfValue lovDto, [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "LOV ID is required." });
            if (_lovManagementService == null)
            {
                return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            if (lovDto == null)
            {
                return BadRequest(new { error = "LOV data is required." });
            }

            var lov = new LIST_OF_VALUE
            {
                LIST_OF_VALUE_ID = id,
                VALUE_TYPE = lovDto.ValueType,
                VALUE_CODE = lovDto.ValueCode,
                VALUE_NAME = lovDto.ValueName,
                DESCRIPTION = lovDto.Description ?? string.Empty,
                CATEGORY = lovDto.Category ?? string.Empty,
                MODULE = lovDto.Module ?? string.Empty,
                SORT_ORDER = lovDto.SortOrder,
                PARENT_VALUE_ID = lovDto.ParentValueId ?? string.Empty,
                IS_DEFAULT = lovDto.IsDefault ?? string.Empty,
                ACTIVE_IND = lovDto.ActiveInd,
                SOURCE = lovDto.Source ?? string.Empty
            };

            var result = await _lovManagementService.UpdateLOVAsync(lov, userId, connectionName ?? "PPDM39");
            return Ok(new LOVResponse
            {
                Success = true,
                Message = "LOV updated successfully",
                LOVs = new List<ListOfValue> { lovDto },
                Count = 1
            });
        }

        /// <summary>
        /// Delete LOV (soft delete)
        /// </summary>
        [HttpDelete("lov/{id}")]
        public async Task<ActionResult<LOVResponse>> DeleteLOV(string id, [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { error = "LOV ID is required." });
            if (_lovManagementService == null)
            {
                    return StatusCode(500, new { error = "LOVManagementService is not available." });
            }

            var result = await _lovManagementService.DeleteLOVAsync(id, userId, connectionName ?? "PPDM39");
            if (result)
            {
                return Ok(new LOVResponse
                {
                    Success = true,
                    Message = "LOV deleted successfully",
                    Count = 0
                });
            }
            else
            {
                return NotFound(new LOVResponse { Success = false, Message = "LOV not found" });
            }
        }

        /// <summary>
        /// Import LOVs from CSV/JSON
        /// </summary>
        [HttpPost("lov/import")]
        public async Task<ActionResult<object>> ImportLOVs([FromBody] LOVImportRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null || string.IsNullOrEmpty(request.FilePath))
            {
                    return BadRequest(new { error = "File path is required." });
            }

            var connectionName = request.ConnectionName ?? "PPDM39";
            if (_csvLovImporter == null)
            {
                return StatusCode(500, new { error = "CSVLOVImporter is not available." });
            }
            
            var csvImporter = _csvLovImporter;

            Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.ImportResult result;
            if (request.TargetTable == "LIST_OF_VALUE" || string.IsNullOrEmpty(request.TargetTable))
            {
                result = await csvImporter.ImportToListOfValueAsync(
                    request.FilePath, 
                    request.ColumnMapping, 
                    request.SkipExisting ?? true, 
                    userId,
                    connectionName);
            }
            else
            {
                result = await csvImporter.ImportToRATableAsync(
                    request.FilePath, 
                    request.TargetTable, 
                    request.ColumnMapping, 
                    request.SkipExisting ?? true, 
                    userId,
                    connectionName);
            }

            return Ok(new
            {
                Success = result.Success,
                RecordsProcessed = result.RecordsProcessed,
                RecordsInserted = result.RecordsInserted,
                RecordsSkipped = result.RecordsSkipped,
                Errors = result.Errors
            });
        }

        #endregion
    

  

        /// <summary>
        /// Extract all RA_* table names from PPDM scripts
        /// </summary>
        [HttpGet("ra-tables/extract")]
        public async Task<ActionResult<RATablesExtractResponse>> ExtractRATables()
        {
            var extractor = new ExtractRATablesCompatibility(_metadata);
            var tableNames = await extractor.ExtractRATableNamesAsync();
            var categories = extractor.CategorizeRATables(tableNames);

            var response = new RATablesExtractResponse
            {
                Success = true,
                Message = $"Successfully extracted {tableNames.Count} RA_* tables",
                TotalTables = tableNames.Count,
                ExtractionDate = DateTime.UtcNow,
                TableNames = tableNames,
                Categories = categories.Select(kvp => new CategoryInfo
                {
                    Category = kvp.Key,
                    Count = kvp.Value.Count,
                    Tables = kvp.Value
                }).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Get categorized RA_* tables
        /// </summary>
        [HttpGet("ra-tables/categorized")]
        public async Task<ActionResult<RATablesExtractResponse>> GetCategorizedRATables()
        {
            var extractor = new ExtractRATablesCompatibility(_metadata);
            var tableNames = await extractor.ExtractRATableNamesAsync();
            var categories = extractor.CategorizeRATables(tableNames);

            var response = new RATablesExtractResponse
            {
                Success = true,
                Message = $"Successfully categorized {tableNames.Count} RA_* tables into {categories.Count} categories",
                TotalTables = tableNames.Count,
                ExtractionDate = DateTime.UtcNow,
                TableNames = tableNames,
                Categories = categories.OrderByDescending(c => c.Value.Count).Select(kvp => new CategoryInfo
                {
                    Category = kvp.Key,
                    Count = kvp.Value.Count,
                    Tables = kvp.Value
                }).ToList()
            };

            return Ok(response);
        }

        /// <summary>
        /// Export RA_* tables to JSON file
        /// </summary>
        [HttpPost("ra-tables/export")]
        public async Task<ActionResult<RATablesExtractResponse>> ExportRATables([FromBody] RATablesExportRequest? request = null)
        {
            var extractor = new ExtractRATablesCompatibility(_metadata);
            var tableNames = await extractor.ExtractRATableNamesAsync();
            var categories = extractor.CategorizeRATables(tableNames);

            var outputPath = request?.OutputPath;
            if (string.IsNullOrEmpty(outputPath))
            {
                var basePath = AppContext.BaseDirectory;
                var solutionRoot = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", ".."));
                outputPath = Path.Combine(solutionRoot, "Beep.OilandGas.PPDM39.DataManagement", "SeedData", "RATablesExport.json");
            }

            var json = await extractor.ExportToJsonAsync(tableNames, categories, outputPath);

            var response = new RATablesExtractResponse
            {
                Success = true,
                Message = $"Successfully exported {tableNames.Count} RA_* tables to {outputPath}",
                TotalTables = tableNames.Count,
                ExtractionDate = DateTime.UtcNow,
                TableNames = tableNames,
                Categories = categories.Select(kvp => new CategoryInfo
                {
                    Category = kvp.Key,
                    Count = kvp.Value.Count,
                    Tables = kvp.Value
                }).ToList()
            };

            return Ok(response);
        }

        private sealed class SeedDataCatalogCompatibility
        {
            public List<SeedDataCategory> GetSeedDataCategories()
            {
                return new List<SeedDataCategory>
                {
                    new SeedDataCategory
                    {
                        CategoryName = "PPDM",
                        Description = "Core PPDM 3.9 reference tables (R_WELL_STATUS, R_FACILITY_TYPE, etc.)",
                        TableNames = new List<string> { "R_WELL_STATUS", "R_FACILITY_TYPE", "R_PPDM_SYSTEM", "R_RECORD_QUALITY" },
                        EstimatedRecords = 500
                    },
                    new SeedDataCategory
                    {
                        CategoryName = "ACCOUNTING",
                        Description = "Accounting reference data (cost codes, chart of accounts, currencies)",
                        TableNames = new List<string> { "R_COST_TYPE", "R_ACCOUNT_TYPE", "R_CURRENCY" },
                        EstimatedRecords = 150
                    },
                    new SeedDataCategory
                    {
                        CategoryName = "LIFECYCLE",
                        Description = "Field lifecycle phase reference data (exploration, development, production, decommissioning)",
                        TableNames = new List<string> { "R_BUSINESS_LIFE_CYCLE", "R_INTENT", "R_OUTCOME" },
                        EstimatedRecords = 80
                    },
                    new SeedDataCategory
                    {
                        CategoryName = "ANALYSIS",
                        Description = "Engineering analysis reference data (fluid types, test types, methods)",
                        TableNames = new List<string> { "R_FLUID_TYPE", "R_TEST_TYPE", "R_ANALYSIS_METHOD" },
                        EstimatedRecords = 200
                    },
                    new SeedDataCategory
                    {
                        CategoryName = "CUSTOM",
                        Description = "Custom list-of-values (LIST_OF_VALUE table)",
                        TableNames = new List<string> { "LIST_OF_VALUE" },
                        EstimatedRecords = 300
                    },
                    new SeedDataCategory
                    {
                        CategoryName = "IHS",
                        Description = "IHS Markit industry standard reference values",
                        TableNames = new List<string>(),
                        EstimatedRecords = 1000
                    },
                    new SeedDataCategory
                    {
                        CategoryName = "INDUSTRYSTANDARDS",
                        Description = "Industry standard codes and classification reference data",
                        TableNames = new List<string>(),
                        EstimatedRecords = 400
                    }
                };
            }
        }

        private sealed class ExtractRATablesCompatibility
        {
            private readonly IPPDMMetadataRepository? _metadata;

            public ExtractRATablesCompatibility(IPPDMMetadataRepository? metadata = null)
            {
                _metadata = metadata;
            }

            public async Task<List<string>> ExtractRATableNamesAsync()
            {
                if (_metadata == null)
                    return new List<string>();

                var tables = await _metadata.GetTablesByPatternAsync("R_*");
                return tables.Select(t => t.TableName).OrderBy(n => n).ToList();
            }

            public Dictionary<string, List<string>> CategorizeRATables(IEnumerable<string> tableNames)
            {
                return new Dictionary<string, List<string>>
                {
                    ["All"] = tableNames.ToList()
                };
            }

            public async Task<string> ExportToJsonAsync(List<string> tableNames, Dictionary<string, List<string>> categories, string outputPath)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    TableNames = tableNames,
                    Categories = categories
                });
                await System.IO.File.WriteAllTextAsync(outputPath, json);
                return json;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // WSC v3 Well-Status Facet Seeding
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/ppdm39/setup/seed/well-status-facets/status
        /// Returns the current row counts for the four R_WELL_STATUS* reference tables.
        /// Use this to determine whether seeding is needed before running the wizard.
        /// </summary>
        [HttpGet("seed/well-status-facets/status")]
        public async Task<ActionResult<Beep.OilandGas.PPDM39.DataManagement.SeedData.FacetSeedStatus>> GetWellStatusFacetSeedStatus()
        {
            if (_wellStatusFacetSeeder == null)
                return StatusCode(503, new { error = "WellStatusFacetSeeder is not available." });
            var status = await _wellStatusFacetSeeder.GetStatusAsync();
            return Ok(status);
        }

        /// <summary>
        /// POST /api/ppdm39/setup/seed/well-status-facets
        /// Seeds R_WELL_STATUS_TYPE, R_WELL_STATUS, R_WELL_STATUS_QUAL, R_WELL_STATUS_QUAL_VALUE
        /// from the embedded WSC v3 (R-3) catalog. Idempotent — skips rows that already exist.
        /// </summary>
        [HttpPost("seed/well-status-facets")]
        public async Task<ActionResult<Beep.OilandGas.PPDM39.DataManagement.SeedData.FacetSeedResult>> SeedWellStatusFacets(
            [FromQuery] string? operationId = null)
        {
            var userId = User.ActingUserId();
            if (_wellStatusFacetSeeder == null)
                return StatusCode(503, new { error = "WellStatusFacetSeeder is not available." });
            _logger.LogInformation("Seeding WSC v3 well-status facets (operationId={OperationId})", operationId);
            var result = await _wellStatusFacetSeeder.SeedAllAsync(userId);
            return result.Success ? Ok(result) : StatusCode(500, result);
        }

        /// <summary>
        /// POST /api/ppdm39/setup/seed/enum-reference-data
        /// Seeds all enum-backed reference tables via <c>EnumReferenceDataSeeder</c>.
        /// </summary>
        [HttpPost("seed/enum-reference-data")]
        public async Task<ActionResult<Beep.OilandGas.Models.Core.Interfaces.SeedingOperationResult>> SeedEnumReferenceData(
            [FromQuery] string? operationId = null)
        {
            var userId = User.ActingUserId();
            _logger.LogInformation("Seeding enum reference data (operationId={OperationId})", operationId);
            var result = await _setupService.SeedEnumReferenceDataAsync(
                "PPDM39", userId, operationId);
            return result.Success ? Ok(result) : StatusCode(500, result);
        }

        /// <summary>
        /// POST /api/ppdm39/setup/seed/all-reference-data
        /// Runs both WSC v3 facet seeding AND enum reference-data seeding in sequence.
        /// </summary>
        [HttpPost("seed/all-reference-data")]
        public async Task<ActionResult<Beep.OilandGas.Models.Core.Interfaces.SeedingOperationResult>> SeedAllReferenceData(
            [FromQuery] string? operationId = null)
        {
            var userId = User.ActingUserId();
            _logger.LogInformation("Seeding all reference data (operationId={OperationId})", operationId);
            var result = await _setupService.SeedAllReferenceDataAsync(
                "PPDM39", userId, operationId);
            return result.Success ? Ok(result) : StatusCode(207, result); // 207 Multi-Status for partial success
        }

        /// <summary>
        /// GET /api/ppdm39/setup/available-modules
        /// Lists all registered IModuleSetup implementations.
        /// </summary>
        [HttpGet("available-modules")]
        public ActionResult<Beep.OilandGas.Models.Core.DTOs.AvailableModulesResponse> GetAvailableModules()
        {
            var metadata = _setupService.GetAvailableModules();
            
            var response = new Beep.OilandGas.Models.Core.DTOs.AvailableModulesResponse
            {
                TotalModules = metadata.Count,
                Modules = metadata
                    .OrderBy(m => m.Order)
                    .Select(m => new Beep.OilandGas.Models.Core.DTOs.ModuleInfo
                    {
                        ModuleId = m.ModuleId,
                        ModuleName = m.ModuleName,
                        Order = m.Order,
                        EntityTypes = m.EntityTypes
                            .Select(t => t.Name)
                            .ToList()
                    })
                    .ToList()
            };

            return Ok(response);
        }


        // ─────────────────────────────────────────────────────────────────────
        // First-run wizard endpoints
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/ppdm39/setup/status
        /// Returns whether a PPDM39 connection already exists.
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<SetupStatusResult>> GetSetupStatus()
        {
            return Ok(await _setupService.GetSetupStatusAsync());
        }

        /// <summary>
        /// POST /api/ppdm39/setup/create-sqlite
        /// Creates a new SQLite database file and registers the connection.
        /// </summary>
        [HttpPost("create-sqlite")]
        public async Task<ActionResult<CreateSqliteResult>> CreateSqliteDatabase([FromBody] CreateSqliteRequest request)
        {
            var result = await _setupService.CreateSqliteAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET /api/ppdm39/setup/schema/ci-validate/{planId}
        /// Returns CI gate, policy, and preflight validation evidence for a generated migration plan.
        /// </summary>
        [HttpGet("schema/ci-validate/{planId}")]
        public async Task<ActionResult<SchemaMigrationCiValidationResult>> ValidateSchemaMigrationForCi(string planId)
        {
            var result = await _schemaMigrationService.ValidateMigrationPlanForCiAsync(planId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// GET /api/ppdm39/setup/schema/progress/{executionToken}
        /// Returns checkpointed execution status for a schema migration run.
        /// </summary>
        [HttpGet("schema/progress/{executionToken}")]
        public async Task<ActionResult<SchemaMigrationProgressResult>> GetSchemaMigrationProgress(string executionToken)
        {
            var result = await _schemaMigrationService.GetSchemaMigrationProgressAsync(executionToken);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// GET /api/ppdm39/setup/schema/artifacts/{planId}
        /// Returns stored plan and evidence artifacts for a schema migration plan.
        /// </summary>
        [HttpGet("schema/artifacts/{planId}")]
        public async Task<ActionResult<SchemaMigrationArtifactsResult>> GetSchemaMigrationArtifacts(string planId)
        {
            var result = await _schemaMigrationService.GetSchemaMigrationArtifactsAsync(planId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ── Generate Demo Data ────────────────────────────────────────────────

        /// <summary>
        /// POST /api/ppdm39/setup/generate-dummy-data
        /// Seeds realistic PPDM39 sample data (fields, wells, production, etc.)
        /// into the currently configured database connection.
        /// </summary>
        [HttpPost("generate-dummy-data")]
        public async Task<ActionResult<GenerateDummyDataResponse>> GenerateDummyDataAsync(
            [FromBody] GenerateDummyDataRequest request)
        {
            var userId = User.ActingUserId();
            _logger.LogInformation("Generate dummy data request: option={Option}, user={User}",
                request.SeedOption, userId);

            // Resolve connection name from first registered connection (wizard already saved it)
            var connectionName = _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName;
            if (string.IsNullOrWhiteSpace(connectionName))
                return BadRequest(new GenerateDummyDataResponse
                {
                    Success = false,
                    Message = "No database connection is configured. Complete the setup wizard first."
                });

            // Build the generator — WellServices is required, create it inline
            var wellServices = new Beep.OilandGas.PPDM39.DataManagement.Repositories.WELL.WellServices(
                _editor, _commonColumnHandler, _defaults, _metadata, connectionName);

            var generator = new Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData.PPDM39DummyDataGenerator(
                _editor, _commonColumnHandler, _defaults, _metadata,
                wellServices, connectionName,
                _logger as Microsoft.Extensions.Logging.ILogger<Beep.OilandGas.PPDM39.DataManagement.SeedData.DummyData.PPDM39DummyDataGenerator>);

            var result = await generator.GenerateAsync(request.SeedOption, userId);

            var response = new GenerateDummyDataResponse
            {
                Success           = result.Success,
                Message           = result.Message,
                SeedOption        = result.SeedOption,
                FieldsCreated     = result.FieldsCreated,
                WellsCreated      = result.WellsCreated,
                ProductionRecords = result.ProductionRecords,
                FacilitiesCreated = result.FacilitiesCreated,
                WellTestsCreated  = result.WellTestsCreated,
                ActivitiesCreated = result.ActivitiesCreated
            };

            return result.Success ? Ok(response) : StatusCode(500, response);
        }

        // ── Dummy Data Status ─────────────────────────────────────────────────

        /// <summary>
        /// GET /api/ppdm39/setup/dummy-data/status
        /// Reports whether demo seed data is already present.
        /// </summary>
        [HttpGet("dummy-data/status")]
        public ActionResult<DummyDataStatusResponse> GetDummyDataStatus()
        {
            var connectionName = _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName;
            if (string.IsNullOrWhiteSpace(connectionName))
                return Ok(new DummyDataStatusResponse { HasDummyData = false, Reason = "No connection configured." });

            var ds = _editor.GetDataSource(connectionName);
            if (ds == null)
                return Ok(new DummyDataStatusResponse { HasDummyData = false, Reason = "Connection not found." });

            // Fetch all FIELD records and check for the sentinel DEMO_FLD_001
            var allFields = ds.GetEntity("FIELD", null);
            var hasDemoField = allFields?.Any(r =>
                r?.GetType().GetProperty("FIELD_ID")?.GetValue(r)?.ToString()
                    ?.StartsWith("DEMO_FLD_", StringComparison.OrdinalIgnoreCase) == true) == true;

            return Ok(new DummyDataStatusResponse
            {
                HasDummyData = hasDemoField,
                Reason = hasDemoField ? "Demo data is present (DEMO_FLD_ records found)." : "No demo data found."
            });
        }

        // ── Delete Dummy Data ─────────────────────────────────────────────────

        /// <summary>
        /// DELETE /api/ppdm39/setup/dummy-data
        /// Removes all DEMO_ prefixed seeded records (fields, wells, production, facilities).
        /// </summary>
        [HttpDelete("dummy-data")]
        public async Task<ActionResult<DummyDataDeleteResponse>> DeleteDummyDataAsync()
        {
            await Task.CompletedTask; // satisfy async signature; all ops are sync on IDataSource
            var connectionName = _editor.ConfigEditor?.DataConnections?.FirstOrDefault()?.ConnectionName;
            if (string.IsNullOrWhiteSpace(connectionName))
                return BadRequest(new DummyDataDeleteResponse { Success = false, Message = "No connection configured." });

            var ds = _editor.GetDataSource(connectionName);
            if (ds == null)
                return BadRequest(new DummyDataDeleteResponse { Success = false, Message = "Connection not found." });

            int deleted = 0;

            // Tables seeded with DEMO_ prefixed PKs — delete in FK-safe order
            var deletePlan = new[]
            {
                ("WELL_ACTIVITY",    "UWI",         "DEMO_"),
                ("WELL_TEST",        "UWI",         "DEMO_"),
                ("PDEN_VOL_SUMMARY", "PDEN_ID",     "PDEN_DEMO_"),
                ("WELL",             "UWI",         "DEMO_"),
                ("FACILITY",         "FACILITY_ID", "DEMO_FAC_"),
                ("FIELD",            "FIELD_ID",    "DEMO_FLD_"),
            };

            // A table that could not be cleared — a record the datasource refused to delete, or a failure reading or deleting —
            // is named in the answer, with the reference its failure is filed under; the other tables are still cleared. The
            // answer said "removed successfully" whatever happened, and a refused delete was counted as removed
            // (OILGAS-CATCH-01).
            var notCleared = new List<string>();
            foreach (var (table, pkCol, prefix) in deletePlan)
            {
                try
                {
                    var rows = ds.GetEntity(table, null);
                    if (rows == null || !rows.Any()) continue;

                    var demoRows = rows
                        .Where(r => r?.GetType().GetProperty(pkCol)?.GetValue(r)?.ToString()
                            ?.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) == true)
                        .ToList();

                    var refused = 0;
                    foreach (var row in demoRows)
                    {
                        var pkVal = row?.GetType().GetProperty(pkCol)?.GetValue(row)?.ToString();
                        if (!string.IsNullOrWhiteSpace(pkVal))
                        {
                            var outcome = ds.DeleteEntity(table, pkVal);
                            if (outcome?.Flag == Errors.Failed) refused++;
                            else deleted++;
                        }
                    }
                    if (refused > 0)
                        notCleared.Add($"{table} ({refused} record(s) the datasource did not delete)");
                }
                catch (Exception tableFailure) when (tableFailure is not OperationCanceledException)
                {
                    var reference = _failures.ReportHandled(tableFailure, $"deleting demo records from {table}",
                        "the table's demo records are not all removed; the answer names the table with this reference");
                    notCleared.Add($"{table} (reference {reference})");
                }
            }

            return Ok(new DummyDataDeleteResponse
            {
                Success = notCleared.Count == 0,
                RecordsDeleted = deleted,
                Message = notCleared.Count == 0
                    ? $"Removed {deleted} demo records successfully."
                    : $"Removed {deleted} demo records; not cleared: {string.Join("; ", notCleared)}."
            });
        }

    }
}
