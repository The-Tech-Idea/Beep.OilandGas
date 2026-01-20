using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Tools;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.Models.Data;
using System.Reflection;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.SeedData.Services;
using Beep.OilandGas.DataManager.Core.Interfaces;
using Beep.OilandGas.DataManager.Core.Models;
using Beep.OilandGas.DataManager.Core.Registry;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 database setup wizard
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/setup")]
    public class PPDM39SetupController : ControllerBase
    {
        private readonly PPDM39SetupService _setupService;
        private readonly IDMEEditor _editor;
        private readonly ILogger<PPDM39SetupController> _logger;
        private readonly IProgressTrackingService? _progressTracking;
        private readonly IDataManager? _dataManager;
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

        public PPDM39SetupController(
            PPDM39SetupService setupService,
            IDMEEditor editor,
            ILogger<PPDM39SetupController> logger,
            IProgressTrackingService? progressTracking = null,
            IPPDMDatabaseCreatorService? databaseCreatorService = null,
            IDataManager? dataManager = null,
            ICommonColumnHandler? commonColumnHandler = null,
            IPPDM39DefaultsRepository? defaults = null,
            IPPDMMetadataRepository? metadata = null,
            LOVManagementService? lovManagementService = null,
            CSVLOVImporter? csvLovImporter = null,
            PPDMStandardValueImporter? ppdmStandardValueImporter = null,
            IHSStandardValueImporter? ihsStandardValueImporter = null,
           StandardValueMapper? standardValueMapper = null,
            PPDMReferenceDataSeeder? referenceDataSeeder = null,
            PPDMDemoDataSeeder? demoDataSeeder = null)
        {
            _setupService = setupService ?? throw new ArgumentNullException(nameof(setupService));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progressTracking = progressTracking;
            _dataManager = dataManager;
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _seedDataCatalog = new PPDMSeedDataCatalog();
            _lovManagementService = lovManagementService;
            _csvLovImporter = csvLovImporter;
            _ppdmStandardValueImporter = ppdmStandardValueImporter;
            _ihsStandardValueImporter = ihsStandardValueImporter;
            _standardValueMapper = standardValueMapper;
            _referenceDataSeeder = referenceDataSeeder;
            
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
            try
            {
                var types = _setupService.GetAvailableDatabaseTypes();
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database types");
                return StatusCode(500, new { error = "Failed to get database types", details = ex.Message });
            }
        }

        /// <summary>
        /// Get driver information for a database type
        /// </summary>
        [HttpGet("driver-info/{databaseType}")]
        public ActionResult<DriverInfo> GetDriverInfo(string databaseType)
        {
            try
            {
                var driverInfo = _setupService.CheckDriver(databaseType);
                return Ok(driverInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting driver info for {DatabaseType}", databaseType);
                return StatusCode(500, new { error = "Failed to get driver info", details = ex.Message });
            }
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
            try
            {
                if (config == null)
                {
                    return BadRequest(new { error = "Connection configuration is required" });
                }

                var mapped = MapToConnectionConfig(config);

                var result = await _setupService.TestConnectionAsync(mapped);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection");
                return StatusCode(500, new ConnectionTestResult
                {
                    Success = false,
                    Message = "Connection test failed",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Check if driver is available
        /// </summary>
        [HttpPost("check-driver")]
        public ActionResult<DriverInfo> CheckDriver([FromBody] CheckDriverRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.DatabaseType))
                {
                    return BadRequest(new { error = "Database type is required" });
                }

                var driverInfo = _setupService.CheckDriver(request.DatabaseType);
                return Ok(driverInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking driver for {DatabaseType}", request?.DatabaseType);
                return StatusCode(500, new { error = "Failed to check driver", details = ex.Message });
            }
        }

        /// <summary>
        /// Get available scripts for a database type
        /// </summary>
        [HttpGet("scripts/{databaseType}")]
        public ActionResult<List<ModuleScriptInfo>> GetAvailableScripts(string databaseType)
        {
            try
            {
                var scripts = _setupService.GetAvailableScripts(databaseType);
                return Ok(scripts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scripts for {DatabaseType}", databaseType);
                return StatusCode(500, new { error = "Failed to get scripts", details = ex.Message });
            }
        }

        /// <summary>
        /// Execute a single SQL script (accepts ConnectionProperties payload)
        /// </summary>
        [HttpPost("execute-script")]
        public async Task<ActionResult<ScriptExecutionResult>> ExecuteScript([FromBody] ScriptExecPayload payload)
        {
            try
            {
                if (payload == null || payload.Connection == null || string.IsNullOrEmpty(payload.ScriptName))
                {
                    return BadRequest(new { error = "Connection and script name are required" });
                }

                var mapped = MapToConnectionConfig(payload.Connection);

                var result = await _setupService.ExecuteScriptAsync(mapped, payload.ScriptName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing script {ScriptName}", payload?.ScriptName);
                return StatusCode(500, new ScriptExecutionResult
                {
                    ScriptName = payload?.ScriptName ?? "Unknown",
                    Success = false,
                    Message = "Script execution failed",
                    Exception = ex
                });
            }
        }

        /// <summary>
        /// Execute all scripts in order (accepts ConnectionProperties payload)
        /// </summary>
        [HttpPost("execute-all-scripts")]
        public async Task<ActionResult<AllScriptsExecutionResult>> ExecuteAllScripts([FromBody] AllScriptsExecPayload request)
        {
            try
            {
                if (request == null || request.Connection == null)
                {
                    return BadRequest(new { error = "Connection configuration is required" });
                }

                var mapped = MapToConnectionConfig(request.Connection);

                if (request.ExecuteAll)
                {
                    // Get all available scripts
                    var scripts = _setupService.GetAvailableScripts(mapped.DatabaseType);
                    request.ScriptNames = scripts.OrderBy(s => s.ExecutionOrder).Select(s => s.FileName).ToList();
                }

                if (request.ScriptNames == null || !request.ScriptNames.Any())
                {
                    return BadRequest(new { error = "No scripts to execute" });
                }

                var result = await _setupService.ExecuteAllScriptsAsync(mapped, request.ScriptNames);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing all scripts");
                return StatusCode(500, new AllScriptsExecutionResult
                {
                    Results = new List<ScriptExecutionResult>(),
                    TotalScripts = 0,
                    SuccessfulScripts = 0,
                    FailedScripts = 0,
                    AllSucceeded = false
                });
            }
        }

        /// <summary>
        /// Save connection to ConfigEditor
        /// </summary>
        [HttpPost("save-connection")]
        public ActionResult<SaveConnectionResult> SaveConnection([FromBody] SaveConnectionRequest request)
        {
            try
            {
                if (request == null || request.Connection == null)
                {
                    return BadRequest(new { error = "Connection configuration is required" });
                }

                var result = _setupService.SaveConnection(
                    request.Connection, 
                    request.TestAfterSave, 
                    request.OpenAfterSave);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving connection {ConnectionName}", request?.Connection?.ConnectionName);
                return StatusCode(500, new SaveConnectionResult
                {
                    Success = false,
                    Message = "Failed to save connection",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Get all database connections
        /// </summary>
        [HttpGet("connections")]
        public ActionResult<List<DatabaseConnectionListItem>> GetAllConnections()
        {
            try
            {
                var connections = _setupService.GetAllConnections();
                return Ok(connections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all connections");
                return StatusCode(500, new { error = "Failed to get connections", details = ex.Message });
            }
        }

        /// <summary>
        /// Get connection by name
        /// </summary>
        [HttpGet("connection/{connectionName}")]
        public ActionResult<ConnectionConfig> GetConnection(string connectionName)
        {
            try
            {
                var connection = _setupService.GetConnectionByName(connectionName);
                if (connection == null)
                {
                    return NotFound(new { error = $"Connection '{connectionName}' not found" });
                }
                return Ok(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connection {ConnectionName}", connectionName);
                return StatusCode(500, new { error = "Failed to get connection", details = ex.Message });
            }
        }

        /// <summary>
        /// Get current connection name
        /// </summary>
        [HttpGet("current-connection")]
        public ActionResult<string?> GetCurrentConnection()
        {
            try
            {
                var connectionName = _setupService.GetCurrentConnectionName();
                return Ok(new { ConnectionName = connectionName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current connection");
                return StatusCode(500, new { error = "Failed to get current connection", details = ex.Message });
            }
        }

        /// <summary>
        /// Set current database connection
        /// </summary>
        [HttpPost("set-current-connection")]
        public ActionResult<SetCurrentDatabaseResult> SetCurrentConnection([FromBody] SetCurrentDatabaseRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ConnectionName))
                {
                    return BadRequest(new { error = "Connection name is required" });
                }

                var result = _setupService.SetCurrentConnection(request.ConnectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current connection {ConnectionName}", request?.ConnectionName);
                return StatusCode(500, new SetCurrentDatabaseResult
                {
                    Success = false,
                    Message = "Failed to set current connection",
                    ErrorDetails = ex.Message,
                    RequiresLogout = false
                });
            }
        }

        /// <summary>
        /// Update an existing connection
        /// </summary>
        [HttpPut("connection")]
        public ActionResult<SaveConnectionResult> UpdateConnection([FromBody] UpdateConnectionRequest request)
        {
            try
            {
                if (request == null || request.Connection == null || string.IsNullOrEmpty(request.OriginalConnectionName))
                {
                    return BadRequest(new { error = "Original connection name and connection configuration are required" });
                }

                var result = _setupService.UpdateConnection(request.OriginalConnectionName, request.Connection, testAfterSave: true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating connection {ConnectionName}", request?.OriginalConnectionName);
                return StatusCode(500, new SaveConnectionResult
                {
                    Success = false,
                    Message = "Failed to update connection",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Check schema privileges and list existing schemas
        /// </summary>
        [HttpPost("check-schema-privileges")]
        public async Task<ActionResult<SchemaPrivilegeCheckResult>> CheckSchemaPrivileges([FromBody] SchemaPrivilegeCheckRequest request)
        {
            try
            {
                if (request == null || request.Connection == null)
                {
                    return BadRequest(new { error = "Connection configuration is required" });
                }

                var result = await _setupService.CheckSchemaPrivilegesAsync(request.Connection, request.SchemaName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking schema privileges");
                return StatusCode(500, new SchemaPrivilegeCheckResult
                {
                    HasCreatePrivilege = false,
                    Message = "Error checking schema privileges",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a new schema
        /// </summary>
        [HttpPost("create-schema")]
        public async Task<ActionResult<CreateSchemaResult>> CreateSchema([FromBody] CreateSchemaRequest request)
        {
            try
            {
                if (request == null || request.Connection == null || string.IsNullOrEmpty(request.SchemaName))
                {
                    return BadRequest(new { error = "Connection configuration and schema name are required" });
                }

                var result = await _setupService.CreateSchemaAsync(request.Connection, request.SchemaName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schema {SchemaName}", request?.SchemaName);
                return StatusCode(500, new CreateSchemaResult
                {
                    Success = false,
                    Message = "Error creating schema",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Drop a database or schema
        /// </summary>
        [HttpPost("drop-database")]
        public async Task<ActionResult<DropDatabaseResult>> DropDatabase([FromBody] DropDatabaseRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ConnectionName))
                {
                    return BadRequest(new { error = "Connection name is required" });
                }

                var result = await _setupService.DropDatabaseAsync(request.ConnectionName, request.SchemaName, request.DropIfExists);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping database");
                return StatusCode(500, new DropDatabaseResult
                {
                    Success = false,
                    Message = "Error dropping database",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Recreate a database or schema
        /// </summary>
        [HttpPost("recreate-database")]
        public async Task<ActionResult<RecreateDatabaseResult>> RecreateDatabase([FromBody] RecreateDatabaseRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.ConnectionName))
                {
                    return BadRequest(new { error = "Connection name is required" });
                }

                var result = await _setupService.RecreateDatabaseAsync(request.ConnectionName, request.SchemaName, request.BackupFirst, request.BackupPath);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recreating database");
                return StatusCode(500, new RecreateDatabaseResult
                {
                    Success = false,
                    Message = "Error recreating database",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Copy database from source to target (ETL operation) with progress tracking
        /// </summary>
        [HttpPost("copy-database")]
        public async Task<ActionResult<object>> CopyDatabase([FromBody] CopyDatabaseRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.SourceConnectionName) || string.IsNullOrEmpty(request.TargetConnectionName))
                {
                    return BadRequest(new { error = "Source and target connection names are required" });
                }

                // Start progress tracking
                string? operationId = null;
                if (_progressTracking != null)
                {
                    operationId = _progressTracking.StartOperation("CopyDatabase", 
                        $"Copying database from {request.SourceConnectionName} to {request.TargetConnectionName}");
                }

                // Execute copy asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var result = await _setupService.CopyDatabaseAsync(request, operationId);
                        if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                        {
                            _progressTracking.CompleteOperation(operationId, result.Success, result.Message, result.ErrorDetails);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error copying database");
                        if (!string.IsNullOrEmpty(operationId) && _progressTracking != null)
                        {
                            _progressTracking.CompleteOperation(operationId, false, "Database copy failed", ex.Message);
                        }
                    }
                });

                return Ok(new OperationStartResponse { OperationId = operationId, Message = "Database copy started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting database copy");
                return StatusCode(500, new { error = "Failed to start database copy", details = ex.Message });
            }
        }

        /// <summary>
        /// Get progress for an operation
        /// </summary>
        [HttpGet("progress/{operationId}")]
        public ActionResult<ProgressUpdate> GetProgress(string operationId)
        {
            try
            {
                if (_progressTracking == null)
                {
                    return NotFound(new { error = "Progress tracking not available" });
                }

                var progress = _progressTracking.GetProgress(operationId);
                if (progress == null)
                {
                    return NotFound(new { error = $"Progress for operation {operationId} not found" });
                }

                return Ok(progress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting progress for operation {OperationId}", operationId);
                return StatusCode(500, new { error = "Failed to get progress", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a connection
        /// </summary>
        [HttpDelete("connection/{connectionName}")]
        public ActionResult<DeleteConnectionResult> DeleteConnection(string connectionName)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionName))
                {
                    return BadRequest(new { error = "Connection name is required" });
                }

                var result = _setupService.DeleteConnection(connectionName);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting connection {ConnectionName}", connectionName);
                return StatusCode(500, new DeleteConnectionResult
                {
                    Success = false,
                    Message = "Failed to delete connection",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Install driver (placeholder - actual NuGet installation would need to be implemented)
        /// Note: This is a placeholder endpoint. Actual NuGet package installation
        /// requires access to the assembly handler which may need to be handled differently
        /// in a web API context vs. BeepShell console context.
        /// </summary>
        [HttpPost("install-driver")]
        public ActionResult<InstallDriverResult> InstallDriver([FromBody] InstallDriverRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.DatabaseType))
                {
                    return BadRequest(new { error = "Database type is required" });
                }

                // Check driver status
                var driverInfo = _setupService.CheckDriver(request.DatabaseType);
                
                if (driverInfo.IsInstalled)
                {
                    return Ok(new InstallDriverResult
                    {
                        Success = true,
                        Message = $"Driver for {request.DatabaseType} is already installed"
                    });
                }

                // NuGet package installation requires assembly handler access which is not available in the API context
                // Return instructions for manual installation
                return Ok(new InstallDriverResult
                {
                    Success = false,
                    Message = $"Driver installation requires manual setup. Please install the NuGet package '{driverInfo.NuGetPackage}' manually or use BeepShell.",
                    ErrorDetails = "Automatic NuGet package installation requires assembly handler access which is not available in the API context. Use BeepShell 'nuget install' command or install manually via Package Manager Console."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing driver for {DatabaseType}", request?.DatabaseType);
                return StatusCode(500, new InstallDriverResult
                {
                    Success = false,
                    Message = "Driver installation failed",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Discover scripts for a database type
        /// </summary>
        [HttpGet("discover-scripts/{databaseType}")]
        public async Task<ActionResult<List<ScriptInfo>>> DiscoverScripts(string databaseType)
        {
            try
            {
                // Get all modules from registry
                var allModules = ModuleDataRegistry.GetAllModules();
                var allScripts = new List<ModuleScriptInfo>();

                // Get scripts from each module
                foreach (var module in allModules.Values)
                {
                    var moduleScripts = await module.GetScriptsAsync(databaseType);
                    allScripts.AddRange(moduleScripts);
                }

                // Map to DTO
                var result = allScripts.Select(s => new ScriptInfo
                {
                    FileName = s.FileName,
                    FullPath = s.FullPath,
                    RelativePath = s.RelativePath ?? string.Empty,
                    ScriptType = s.ScriptType.ToString(),
                    TableName = s.TableName,
                    Module = s.ModuleName ?? string.Empty,
                    SubjectArea = null, // Not available in ModuleScriptInfo
                    IsConsolidated = s.IsConsolidated,
                    IsMandatory = s.IsRequired,
                    IsOptional = !s.IsRequired,
                    FileSize = s.FileSize,
                    LastModified = s.LastModified,
                    ExecutionOrder = s.ExecutionOrder,
                    Dependencies = s.Dependencies?.ToList() ?? new List<string>(),
                    Category = s.ModuleName
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering scripts for {DatabaseType}", databaseType);
                return StatusCode(500, new { error = "Failed to discover scripts", details = ex.Message });
            }
        }

        /// <summary>
        /// Create database by executing scripts using DataManager
        /// </summary>
        [HttpPost("create-database")]
        public async Task<ActionResult<DatabaseCreationResult>> CreateDatabase([FromBody] CreateDatabaseRequest request)
        {
            try
            {
                if (request == null || request.Connection == null || request.Options == null)
                {
                    return BadRequest(new { error = "Connection and options are required" });
                }

                if (_dataManager == null)
                {
                    // Fallback to old service if DataManager not available
                    if (_databaseCreatorService == null)
                    {
                        return StatusCode(500, new { error = "DataManager and DatabaseCreatorService not available" });
                    }
                    return await CreateDatabaseLegacy(request);
                }

                // Map ConnectionProperties to ConnectionConfig
                var connectionConfig = MapToConnectionConfig(request.Connection);

                // Get data source
                var dataSource = _setupService.GetDataSourceFromConnectionConfig(connectionConfig);
                if (dataSource.Openconnection() != System.Data.ConnectionState.Open)
                {
                    return StatusCode(500, new DatabaseCreationResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to open database connection"
                    });
                }

                // Determine which modules to execute
                var modulesToExecute = new List<IModuleData>();
                
                if (request.Options.Categories != null && request.Options.Categories.Any())
                {
                    // Execute specific modules by category
                    var allModules = ModuleDataRegistry.GetAllModules();
                    foreach (var category in request.Options.Categories)
                    {
                        var module = ModuleDataRegistry.GetModule(category);
                        if (module != null)
                        {
                            modulesToExecute.Add(module);
                        }
                    }
                }
                else
                {
                    // Execute all required modules
                    modulesToExecute = ModuleDataRegistry.GetRequiredModules().ToList();
                }

                if (!modulesToExecute.Any())
                {
                    return BadRequest(new { error = "No modules selected for execution" });
                }

                // Create execution options
                var executionOptions = new ScriptExecutionOptions
                {
                    ContinueOnError = request.Options.ContinueOnError,
                    EnableRollback = request.Options.EnableRollback,
                    ExecuteOptionalScripts = request.Options.ExecuteOptionalScripts,
                    ValidateDependencies = request.Options.ValidateDependencies,
                    EnableParallelExecution = request.Options.EnableParallelExecution,
                    MaxParallelTasks = request.Options.MaxParallelTasks,
                    EnableCheckpointing = !string.IsNullOrEmpty(request.Options.ExecutionId),
                    ExecutionId = request.Options.ExecutionId ?? Guid.NewGuid().ToString(),
                    ValidateBeforeExecution = true,
                    CheckErrorsAfterExecution = true,
                    VerifyObjectsCreated = true,
                    Logger = _logger
                };

                // Filter script types if specified
                if (request.Options.ScriptTypes != null && request.Options.ScriptTypes.Any())
                {
                    var scriptTypes = new List<ScriptType>();
                    foreach (var st in request.Options.ScriptTypes)
                    {
                        if (Enum.TryParse<ScriptType>(st, true, out var scriptType))
                        {
                            scriptTypes.Add(scriptType);
                        }
                    }
                    executionOptions.IncludedScriptTypes = scriptTypes;
                }

                // Execute modules
                var moduleResults = await _dataManager.ExecuteModulesAsync(modulesToExecute, dataSource, executionOptions);

                // Aggregate results
                var totalScripts = moduleResults.Values.Sum(r => r.TotalScripts);
                var successfulScripts = moduleResults.Values.Sum(r => r.SuccessfulScripts);
                var failedScripts = moduleResults.Values.Sum(r => r.FailedScripts);
                var allScriptResults = moduleResults.Values.SelectMany(r => r.ScriptResults).ToList();

                var overallSuccess = moduleResults.Values.All(r => r.Success);
                var startTime = moduleResults.Values.Min(r => r.StartTime);
                var endTime = moduleResults.Values.Max(r => r.EndTime);

                // Map to DTO
                var resultDto = new DatabaseCreationResult
                {
                    ExecutionId = executionOptions.ExecutionId ?? Guid.NewGuid().ToString(),
                    Success = overallSuccess,
                    ErrorMessage = overallSuccess ? null : $"Failed scripts: {failedScripts}",
                    StartTime = startTime,
                    EndTime = endTime,
                    TotalDuration = endTime - startTime,
                    TotalScripts = totalScripts,
                    SuccessfulScripts = successfulScripts,
                    FailedScripts = failedScripts,
                    SkippedScripts = totalScripts - successfulScripts - failedScripts,
                    ScriptResults = allScriptResults.Select(sr => new ScriptExecutionResult
                    {
                        ScriptFileName = sr.ScriptFileName ?? string.Empty,
                        Success = sr.Success,
                        ErrorMessage = sr.ErrorMessage,
                        StartTime = sr.StartTime,
                        EndTime = sr.EndTime,
                        Duration = sr.Duration,
                        RowsAffected = sr.RowsAffected,
                        ExecutionLog = sr.ExecutionLog,
                        Metadata = sr.Metadata ?? new Dictionary<string, object>()
                    }).ToList(),
                    Summary = new Dictionary<string, object>
                    {
                        { "ModulesExecuted", modulesToExecute.Select(m => m.ModuleName).ToList() },
                        { "TotalModules", modulesToExecute.Count },
                        { "SuccessfulModules", moduleResults.Values.Count(r => r.Success) },
                        { "FailedModules", moduleResults.Values.Count(r => !r.Success) }
                    },
                    LogFilePath = request.Options.LogFilePath
                };

                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating database");
                return StatusCode(500, new DatabaseCreationResult
                {
                    Success = false,
                    ErrorMessage = $"Database creation failed: {ex.Message}"
                });
            }
        }


        /// <summary>
        /// Get creation progress
        /// </summary>
        [HttpGet("creation-progress/{executionId}")]
        public async Task<ActionResult<ScriptExecutionProgressInfo>> GetCreationProgress(string executionId)
        {
            try
            {
                if (_dataManager == null)
                {
                    return StatusCode(500, new { error = "DataManager is not available" });
                }

                var executionState = await _dataManager.GetExecutionStateAsync(executionId);
                if (executionState == null)
                {
                    return NotFound(new { error = $"Execution state not found for {executionId}" });
                }

                var totalScripts = executionState.CompletedScripts.Count + executionState.FailedScripts.Count + executionState.PendingScripts.Count;
                var completedScripts = executionState.CompletedScripts.Count;
                var failedScripts = executionState.FailedScripts.Count;
                var skippedScripts = 0; // Not tracked in ExecutionState
                var progressPercentage = totalScripts > 0 ? (decimal)completedScripts * 100 / totalScripts : 0;
                var currentScript = executionState.PendingScripts.FirstOrDefault() ?? string.Empty;

                var progressDto = new ScriptExecutionProgressInfo
                {
                    ExecutionId = executionState.ExecutionId,
                    TotalScripts = totalScripts,
                    CompletedScripts = completedScripts,
                    FailedScripts = failedScripts,
                    SkippedScripts = skippedScripts,
                    ProgressPercentage = progressPercentage,
                    CurrentScript = currentScript,
                    StartTime = executionState.StartTime,
                    EstimatedCompletionTime = null, // Not available in ExecutionState
                    Status = executionState.IsCompleted ? "Completed" : "In Progress"
                };

                return Ok(progressDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting creation progress for {ExecutionId}", executionId);
                return StatusCode(500, new { error = "Failed to get progress", details = ex.Message });
            }
        }

        /// <summary>
        /// Seed PPDM reference tables (R_* tables)
        /// </summary>
        [HttpPost("seed/reference")]
        public async Task<ActionResult<SeedDataResponse>> SeedReferenceData([FromBody] SeedDataRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request is required" });
                }

                var connectionName = request.ConnectionName ?? "PPDM39";
                var seeder = new PPDMReferenceDataSeeder(_editor, _commonColumnHandler, _defaults, _metadata, connectionName);
                
                var result = await seeder.SeedPPDMReferenceTablesAsync(
                    connectionName, 
                    request.TableNames, 
                    request.SkipExisting, 
                    request.UserId ?? "SYSTEM");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding reference data");
                return StatusCode(500, new SeedDataResponse
                {
                    Success = false,
                    Message = $"Failed to seed reference data: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Seed full demo dataset (reference data + sample entities + production data)
        /// </summary>
        [HttpPost("seed/demo-full")]
        public async Task<ActionResult<SeedDataResponse>> SeedFullDemoDataset([FromBody] SeedDataRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request is required" });
                }

                var connectionName = request.ConnectionName ?? "PPDM39";
                
                if (_demoDataSeeder == null)
                {
                    return BadRequest(new { error = "DemoDataSeeder is not available. Please ensure it is registered in dependency injection." });
                }

                var result = await _demoDataSeeder.SeedFullDemoDatasetAsync(request.UserId ?? "SYSTEM");

                return Ok(new SeedDataResponse
                {
                    Success = result.Success,
                    Message = result.Message,
                    TablesSeeded = result.TablesSeeded,
                    RecordsInserted = result.RecordsInserted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding full demo dataset");
                return StatusCode(500, new SeedDataResponse
                {
                    Success = false,
                    Message = $"Failed to seed full demo dataset: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Seed data by category
        /// </summary>
        [HttpPost("seed/category/{category}")]
        public async Task<ActionResult<SeedDataResponse>> SeedByCategory(string category, [FromBody] SeedDataRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request is required" });
                }

                var connectionName = request.ConnectionName ?? "PPDM39";
                var seeder = new PPDMReferenceDataSeeder(_editor, _commonColumnHandler, _defaults, _metadata, connectionName);
                
                var result = await seeder.SeedByCategoryAsync(
                    category, 
                    connectionName, 
                    request.TableNames, 
                    request.SkipExisting, 
                    request.UserId ?? "SYSTEM");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding data for category {Category}", category);
                return StatusCode(500, new SeedDataResponse
                {
                    Success = false,
                    Message = $"Failed to seed data for category {category}: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get available seed data categories
        /// </summary>
        [HttpGet("seed/categories")]
        public ActionResult<List<SeedDataCategory>> GetSeedDataCategories()
        {
            try
            {
                var categories = _seedDataCatalog.GetSeedDataCategories();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seed data categories");
                return StatusCode(500, new { error = "Failed to get seed data categories", details = ex.Message });
            }
        }

        /// <summary>
        /// Validate seed data before insertion
        /// </summary>
        [HttpPost("seed/validate")]
        public async Task<ActionResult<SeedDataResponse>> ValidateSeedData([FromBody] SeedDataValidationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request is required" });
                }

                var connectionName = request.ConnectionName ?? "PPDM39";
                var seeder = new PPDMReferenceDataSeeder(_editor, _commonColumnHandler, _defaults, _metadata, connectionName);
                
                // Validate by attempting to seed with validateOnly flag
                var seedRequest = new SeedDataRequest
                {
                    ConnectionName = connectionName,
                    TableNames = request.TableNames,
                    ValidateOnly = true,
                    SkipExisting = true
                };

                var result = await seeder.SeedByCategoryAsync(
                    request.Category, 
                    connectionName, 
                    request.TableNames, 
                    true, 
                    "SYSTEM");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating seed data for category {Category}", request?.Category);
                return StatusCode(500, new SeedDataResponse
                {
                    Success = false,
                    Message = $"Failed to validate seed data: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Seed data for a specific workflow
        /// </summary>
        [HttpPost("seed/workflow/{workflowName}")]
        public async Task<ActionResult<SeedDataResponse>> SeedWorkflowData(string workflowName, [FromBody] SeedDataRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request is required" });
                }

                if (_demoDataSeeder == null)
                {
                    return StatusCode(500, new { error = "DemoDataSeeder is not available" });
                }
                var workflowRequirement = _demoDataSeeder.GetSeedDataForWorkflow(workflowName);
                if (workflowRequirement == null)
                {
                    return NotFound(new { error = $"Workflow '{workflowName}' not found" });
                }

                var connectionName = request.ConnectionName ?? "PPDM39";
                var seeder = new PPDMReferenceDataSeeder(_editor, _commonColumnHandler, _defaults, _metadata, connectionName);
                
                // Seed by category
                var result = await seeder.SeedByCategoryAsync(
                    workflowRequirement.WorkflowCategory, 
                    connectionName, 
                    request.TableNames ?? workflowRequirement.RequiredTables, 
                    request.SkipExisting, 
                    request.UserId ?? "SYSTEM");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding data for workflow {WorkflowName}", workflowName);
                return StatusCode(500, new SeedDataResponse
                {
                    Success = false,
                    Message = $"Failed to seed data for workflow {workflowName}: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Generate SQL scripts for entity classes
        /// </summary>
        [HttpPost("scripts/generate")]
        public async Task<ActionResult<ScriptGenerationResponse>> GenerateScripts([FromBody] ScriptGenerationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { error = "Request is required" });
                }

                var response = new ScriptGenerationResponse
                {
                    Success = true,
                    Message = "Script generation completed",
                    EntityResults = new List<EntityScriptResult>()
                };

                // Determine output path
                var outputPath = request.OutputPath;
                if (string.IsNullOrEmpty(outputPath))
                {
                    var basePath = AppContext.BaseDirectory;
                    var solutionRoot = Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", ".."));
                    outputPath = Path.Combine(solutionRoot, "Beep.OilandGas.Models", "Scripts");
                }
                outputPath = Path.GetFullPath(outputPath);
                response.OutputPath = outputPath;

                // Get entity types
                var entityAssembly = typeof(AFE).Assembly;
                var allEntityTypes = entityAssembly.GetTypes()
                    .Where(t =>
                        t.Namespace == "Beep.OilandGas.Models.Data" &&
                        t.IsClass &&
                        !t.IsAbstract &&
                        t.BaseType != null &&
                        t.BaseType.Name == "Entity")
                    .OrderBy(t => t.Name)
                    .ToList();

                // Filter by entity names if specified
                var entityTypes = allEntityTypes;
                if (request.EntityNames != null && request.EntityNames.Any())
                {
                    entityTypes = allEntityTypes
                        .Where(t => request.EntityNames.Contains(t.Name, StringComparer.OrdinalIgnoreCase))
                        .ToList();
                }

                if (!entityTypes.Any())
                {
                    response.Success = false;
                    response.Message = "No entity types found matching the criteria";
                    return Ok(response);
                }

                // Determine database types
                var databaseTypes = new List<DatabaseTypeMapper.DatabaseType>();
                if (request.DatabaseTypes != null && request.DatabaseTypes.Any())
                {
                    foreach (var dbTypeStr in request.DatabaseTypes)
                    {
                        if (Enum.TryParse<DatabaseTypeMapper.DatabaseType>(dbTypeStr, true, out var dbType))
                        {
                            databaseTypes.Add(dbType);
                        }
                    }
                }
                else
                {
                    // Default to all database types
                    databaseTypes = new List<DatabaseTypeMapper.DatabaseType>
                    {
                        DatabaseTypeMapper.DatabaseType.SqlServer,
                        DatabaseTypeMapper.DatabaseType.Oracle,
                        DatabaseTypeMapper.DatabaseType.PostgreSQL,
                        DatabaseTypeMapper.DatabaseType.MySQL,
                        DatabaseTypeMapper.DatabaseType.MariaDB,
                        DatabaseTypeMapper.DatabaseType.SQLite
                    };
                }

                // Determine script types
                var scriptTypes = new List<ScriptType>();
                if (request.ScriptTypes != null && request.ScriptTypes.Any())
                {
                    foreach (var scriptTypeStr in request.ScriptTypes)
                    {
                        if (Enum.TryParse<ScriptType>(scriptTypeStr, true, out var scriptType))
                        {
                            scriptTypes.Add(scriptType);
                        }
                    }
                }
                else
                {
                    // Default to TAB, PK, FK, IX
                    scriptTypes = new List<ScriptType> { ScriptType.TAB, ScriptType.PK, ScriptType.FK, ScriptType.IX };
                }

                // Create output directories
                if (request.SaveToFile)
                {
                    foreach (var dbType in databaseTypes)
                    {
                        var dbTypeName = GetDatabaseTypeFolderName(dbType);
                        var dbPath = Path.Combine(outputPath, dbTypeName);
                        Directory.CreateDirectory(dbPath);
                    }
                }

                // Generate metadata repository if available
                IPPDMMetadataRepository? metadata = _metadata;

                // Generate scripts for each entity
                foreach (var entityType in entityTypes)
                {
                    var entityResult = new EntityScriptResult
                    {
                        EntityName = entityType.Name,
                        Success = true,
                        Scripts = new List<ScriptResult>()
                    };

                    try
                    {
                        foreach (var dbType in databaseTypes)
                        {
                            try
                            {
                                var generator = new PPDMScriptGenerator(dbType, metadata);
                                var dbTypeName = GetDatabaseTypeFolderName(dbType);

                                // Generate scripts based on requested types
                                if (scriptTypes.Contains(ScriptType.TAB))
                                {
                                    var tabScript = await generator.GenerateTableScriptAsync(entityType);
                                    if (!string.IsNullOrWhiteSpace(tabScript))
                                    {
                                        var scriptResult = await SaveScriptAsync(
                                            tabScript, 
                                            entityType.Name, 
                                            "TAB", 
                                            dbTypeName, 
                                            outputPath, 
                                            request.SaveToFile);
                                        entityResult.Scripts.Add(scriptResult);
                                        if (scriptResult.Success) response.ScriptsGenerated++;
                                    }
                                }

                                if (scriptTypes.Contains(ScriptType.PK))
                                {
                                    var pkScript = await generator.GeneratePrimaryKeyScriptAsync(entityType);
                                    if (!string.IsNullOrWhiteSpace(pkScript))
                                    {
                                        var scriptResult = await SaveScriptAsync(
                                            pkScript, 
                                            entityType.Name, 
                                            "PK", 
                                            dbTypeName, 
                                            outputPath, 
                                            request.SaveToFile);
                                        entityResult.Scripts.Add(scriptResult);
                                        if (scriptResult.Success) response.ScriptsGenerated++;
                                    }
                                }

                                if (scriptTypes.Contains(ScriptType.FK))
                                {
                                    var fkScript = await generator.GenerateForeignKeyScriptsAsync(entityType);
                                    if (!string.IsNullOrWhiteSpace(fkScript))
                                    {
                                        var scriptResult = await SaveScriptAsync(
                                            fkScript, 
                                            entityType.Name, 
                                            "FK", 
                                            dbTypeName, 
                                            outputPath, 
                                            request.SaveToFile);
                                        entityResult.Scripts.Add(scriptResult);
                                        if (scriptResult.Success) response.ScriptsGenerated++;
                                    }
                                }

                                if (scriptTypes.Contains(ScriptType.IX))
                                {
                                    var ixScript = await generator.GenerateIndexScriptsAsync(entityType);
                                    if (!string.IsNullOrWhiteSpace(ixScript))
                                    {
                                        var scriptResult = await SaveScriptAsync(
                                            ixScript, 
                                            entityType.Name, 
                                            "IX", 
                                            dbTypeName, 
                                            outputPath, 
                                            request.SaveToFile);
                                        entityResult.Scripts.Add(scriptResult);
                                        if (scriptResult.Success) response.ScriptsGenerated++;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error generating scripts for {EntityName} for {DatabaseType}", entityType.Name, dbType);
                                response.Errors++;
                                entityResult.Scripts.Add(new ScriptResult
                                {
                                    ScriptType = "ALL",
                                    DatabaseType = dbType.ToString(),
                                    Success = false,
                                    ErrorMessage = ex.Message
                                });
                            }
                        }

                        entityResult.ScriptsGenerated = entityResult.Scripts.Count(s => s.Success);
                        response.EntitiesProcessed++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing entity {EntityName}", entityType.Name);
                        entityResult.Success = false;
                        entityResult.ErrorMessage = ex.Message;
                        response.Errors++;
                    }

                    response.EntityResults.Add(entityResult);
                }

                response.Message = $"Generated {response.ScriptsGenerated} script(s) for {response.EntitiesProcessed} entity/entities";
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating scripts");
                return StatusCode(500, new ScriptGenerationResponse
                {
                    Success = false,
                    Message = $"Failed to generate scripts: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Save or return a generated script
        /// </summary>
        private async Task<ScriptResult> SaveScriptAsync(string script, string entityName, string scriptType, string dbTypeName, string outputPath, bool saveToFile)
        {
            var result = new ScriptResult
            {
                ScriptType = scriptType,
                DatabaseType = dbTypeName,
                ScriptLength = script.Length,
                Success = true
            };

            if (saveToFile)
            {
                try
                {
                    var fileName = $"{entityName}_{scriptType}.sql";
                    var filePath = Path.Combine(outputPath, dbTypeName, fileName);
                    await File.WriteAllTextAsync(filePath, script);
                    result.FilePath = filePath;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// Get database type folder name
        /// </summary>
        private string GetDatabaseTypeFolderName(DatabaseTypeMapper.DatabaseType dbType)
        {
            return dbType switch
            {
                DatabaseTypeMapper.DatabaseType.SqlServer => "Sqlserver",
                DatabaseTypeMapper.DatabaseType.Oracle => "Oracle",
                DatabaseTypeMapper.DatabaseType.PostgreSQL => "PostgreSQL",
                DatabaseTypeMapper.DatabaseType.MySQL => "MariaDB",
                DatabaseTypeMapper.DatabaseType.MariaDB => "MariaDB",
                DatabaseTypeMapper.DatabaseType.SQLite => "SQLite",
                _ => dbType.ToString()
            };
        }

        /// <summary>
        /// Build connection string from ConnectionConfig
        /// </summary>
        private string BuildConnectionString(ConnectionConfig config)
        {
            return config.DatabaseType.ToLowerInvariant() switch
            {
                "sqlserver" => $"Server={config.Host};Database={config.Database};User Id={config.Username};Password={config.Password};TrustServerCertificate=true;",
                "postgresql" => $"Host={config.Host};Port={config.Port};Database={config.Database};Username={config.Username};Password={config.Password};",
                "mysql" or "mariadb" => $"Server={config.Host};Port={config.Port};Database={config.Database};User Id={config.Username};Password={config.Password};",
                "oracle" => $"Data Source={config.Host}:{config.Port}/{config.Database};User Id={config.Username};Password={config.Password};",
                "sqlite" => $"Data Source={config.Database};",
                _ => config.ToConnectionProperties().ConnectionString ?? string.Empty
            };
        }

        #region LOV Management Endpoints

        /// <summary>
        /// Get all VALUE_TYPEs
        /// </summary>
        [HttpGet("lov/types")]
        public async Task<ActionResult<List<string>>> GetLOVTypes([FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
                }

                var types = await _lovManagementService.GetValueTypesAsync(connectionName ?? "PPDM39");
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LOV types");
                return StatusCode(500, new { error = "Failed to get LOV types", details = ex.Message });
            }
        }

        /// <summary>
        /// Get LOVs by type
        /// </summary>
        [HttpGet("lov/{valueType}")]
        public async Task<ActionResult<LOVResponse>> GetLOVByType(string valueType, [FromQuery] string? category = null, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LOVs by type {ValueType}", valueType);
                return StatusCode(500, new LOVResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get LOVs by category
        /// </summary>
        [HttpGet("lov/category/{category}")]
        public async Task<ActionResult<LOVResponse>> GetLOVByCategory(string category, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LOVs by category {Category}", category);
                return StatusCode(500, new LOVResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get LOVs by module
        /// </summary>
        [HttpGet("lov/module/{module}")]
        public async Task<ActionResult<LOVResponse>> GetLOVByModule(string module, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LOVs by module {Module}", module);
                return StatusCode(500, new LOVResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get LOVs by source
        /// </summary>
        [HttpGet("lov/source/{source}")]
        public async Task<ActionResult<LOVResponse>> GetLOVBySource(string source, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LOVs by source {Source}", source);
                return StatusCode(500, new LOVResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Get hierarchical LOVs
        /// </summary>
        [HttpGet("lov/hierarchical/{valueType}")]
        public async Task<ActionResult<LOVResponse>> GetHierarchicalLOV(string valueType, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hierarchical LOVs for {ValueType}", valueType);
                return StatusCode(500, new LOVResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Search LOVs
        /// </summary>
        [HttpGet("lov/search")]
        public async Task<ActionResult<LOVResponse>> SearchLOVs([FromQuery] string? searchTerm = null, [FromQuery] string? valueType = null, [FromQuery] string? category = null, [FromQuery] string? module = null, [FromQuery] string? source = null, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching LOVs");
                return StatusCode(500, new LOVResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Add new LOV
        /// </summary>
        [HttpPost("lov")]
        public async Task<ActionResult<LOVResponse>> AddLOV([FromBody] ListOfValue lovDto, [FromQuery] string? userId = null, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
                }

                if (lovDto == null)
                {
                    return BadRequest(new { error = "LOV data is required" });
                }

                var lov = new LIST_OF_VALUE
                {
                    LIST_OF_VALUE_ID = lovDto.ListOfValueId,
                    VALUE_TYPE = lovDto.ValueType,
                    VALUE_CODE = lovDto.ValueCode,
                    VALUE_NAME = lovDto.ValueName,
                    DESCRIPTION = lovDto.Description,
                    CATEGORY = lovDto.Category,
                    MODULE = lovDto.Module,
                    SORT_ORDER = lovDto.SortOrder,
                    PARENT_VALUE_ID = lovDto.ParentValueId,
                    IS_DEFAULT = lovDto.IsDefault,
                    ACTIVE_IND = lovDto.ActiveInd,
                    SOURCE = lovDto.Source
                };

                var result = await _lovManagementService.AddLOVAsync(lov, userId ?? "SYSTEM", connectionName ?? "PPDM39");
                return Ok(new LOVResponse
                {
                    Success = true,
                    Message = "LOV added successfully",
                    LOVs = new List<ListOfValue> { lovDto },
                    Count = 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding LOV");
                return StatusCode(500, new LOVResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Update LOV
        /// </summary>
        [HttpPut("lov/{id}")]
        public async Task<ActionResult<LOVResponse>> UpdateLOV(string id, [FromBody] ListOfValue lovDto, [FromQuery] string? userId = null, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
                }

                if (lovDto == null)
                {
                    return BadRequest(new { error = "LOV data is required" });
                }

                var lov = new LIST_OF_VALUE
                {
                    LIST_OF_VALUE_ID = id,
                    VALUE_TYPE = lovDto.ValueType,
                    VALUE_CODE = lovDto.ValueCode,
                    VALUE_NAME = lovDto.ValueName,
                    DESCRIPTION = lovDto.Description,
                    CATEGORY = lovDto.Category,
                    MODULE = lovDto.Module,
                    SORT_ORDER = lovDto.SortOrder,
                    PARENT_VALUE_ID = lovDto.ParentValueId,
                    IS_DEFAULT = lovDto.IsDefault,
                    ACTIVE_IND = lovDto.ActiveInd,
                    SOURCE = lovDto.Source
                };

                var result = await _lovManagementService.UpdateLOVAsync(lov, userId ?? "SYSTEM", connectionName ?? "PPDM39");
                return Ok(new LOVResponse
                {
                    Success = true,
                    Message = "LOV updated successfully",
                    LOVs = new List<ListOfValue> { lovDto },
                    Count = 1
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating LOV {Id}", id);
                return StatusCode(500, new LOVResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Delete LOV (soft delete)
        /// </summary>
        [HttpDelete("lov/{id}")]
        public async Task<ActionResult<LOVResponse>> DeleteLOV(string id, [FromQuery] string? userId = null, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (_lovManagementService == null)
                {
                    return StatusCode(500, new { error = "LOVManagementService is not available" });
                }

                var result = await _lovManagementService.DeleteLOVAsync(id, userId ?? "SYSTEM", connectionName ?? "PPDM39");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting LOV {Id}", id);
                return StatusCode(500, new LOVResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// Import LOVs from CSV/JSON
        /// </summary>
        [HttpPost("lov/import")]
        public async Task<ActionResult<object>> ImportLOVs([FromBody] LOVImportRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.FilePath))
                {
                    return BadRequest(new { error = "FilePath is required" });
                }

                var connectionName = request.ConnectionName ?? "PPDM39";
                if (_csvLovImporter == null)
                {
                    return StatusCode(500, new { error = "CSVLOVImporter is not available" });
                }
                
                var csvImporter = _csvLovImporter;

                Beep.OilandGas.PPDM39.DataManagement.SeedData.Services.ImportResult result;
                if (request.TargetTable == "LIST_OF_VALUE" || string.IsNullOrEmpty(request.TargetTable))
                {
                    result = await csvImporter.ImportToListOfValueAsync(
                        request.FilePath, 
                        request.ColumnMapping, 
                        request.SkipExisting ?? true, 
                        request.UserId ?? "SYSTEM",
                        connectionName);
                }
                else
                {
                    result = await csvImporter.ImportToRATableAsync(
                        request.FilePath, 
                        request.TargetTable, 
                        request.ColumnMapping, 
                        request.SkipExisting ?? true, 
                        request.UserId ?? "SYSTEM",
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing LOVs");
                return StatusCode(500, new { error = "Failed to import LOVs", details = ex.Message });
            }
        }

        #endregion
    

  

        /// <summary>
        /// Extract all RA_* table names from PPDM scripts
        /// </summary>
        [HttpGet("ra-tables/extract")]
        public async Task<ActionResult<RATablesExtractResponse>> ExtractRATables()
        {
            try
            {
                var extractor = new ExtractRATables();
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting RA_* tables");
                return StatusCode(500, new RATablesExtractResponse
                {
                    Success = false,
                    Message = $"Failed to extract RA_* tables: {ex.Message}",
                    ExtractionDate = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get categorized RA_* tables
        /// </summary>
        [HttpGet("ra-tables/categorized")]
        public async Task<ActionResult<RATablesExtractResponse>> GetCategorizedRATables()
        {
            try
            {
                var extractor = new ExtractRATables();
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error categorizing RA_* tables");
                return StatusCode(500, new RATablesExtractResponse
                {
                    Success = false,
                    Message = $"Failed to categorize RA_* tables: {ex.Message}",
                    ExtractionDate = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Export RA_* tables to JSON file
        /// </summary>
        [HttpPost("ra-tables/export")]
        public async Task<ActionResult<RATablesExtractResponse>> ExportRATables([FromBody] RATablesExportRequest? request = null)
        {
            try
            {
                var extractor = new ExtractRATables();
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting RA_* tables");
                return StatusCode(500, new RATablesExtractResponse
                {
                    Success = false,
                    Message = $"Failed to export RA_* tables: {ex.Message}",
                    ExtractionDate = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Gets or creates IDataSource from ConnectionConfig (helper method)
        /// </summary>
        private IDataSource GetDataSourceFromConnectionConfig(ConnectionConfig config)
        {
            var driverInfo = _setupService.GetDriverInfo(config.DatabaseType);
            if (driverInfo == null)
            {
                throw new InvalidOperationException($"Unknown database type: {config.DatabaseType}");
            }

            // Check if connection already exists
            if (_editor.ConfigEditor.DataConnectionExist(config.ConnectionName))
            {
                var dataSource = _editor.GetDataSource(config.ConnectionName);
                if (dataSource != null)
                {
                    return dataSource;
                }
            }

            // Create new connection
            var connectionProperties = new ConnectionProperties
            {
                ConnectionName = config.ConnectionName,
                DatabaseType = ParseDataSourceType(driverInfo.DataSourceType),
                DriverName = driverInfo.NuGetPackage,
                Host = config.Host,
                Port = config.Port > 0 ? config.Port : driverInfo.DefaultPort,
                Database = config.Database,
                UserID = config.Username ?? string.Empty,
                Password = config.Password ?? string.Empty,
                ConnectionString = config.ConnectionString ?? string.Empty,
                GuidID = Guid.NewGuid().ToString()
            };

            if (!_editor.ConfigEditor.DataConnectionExist(config.ConnectionName))
            {
                _editor.ConfigEditor.AddDataConnection(connectionProperties);
                _editor.ConfigEditor.SaveDataconnectionsValues();
            }

            var newDataSource = _editor.GetDataSource(config.ConnectionName);
            if (newDataSource == null)
            {
                throw new InvalidOperationException($"Failed to create data source for connection: {config.ConnectionName}");
            }

            return newDataSource;
        }

        /// <summary>
        /// Helper method to convert string to DataSourceType enum
        /// </summary>
        private static DataSourceType ParseDataSourceType(string dataSourceTypeString)
        {
            if (string.IsNullOrEmpty(dataSourceTypeString))
                return DataSourceType.SqlServer; // Default

            // Try parsing the enum directly
            if (Enum.TryParse<DataSourceType>(dataSourceTypeString, true, out var result))
                return result;

            // Map string values to enum values (case-insensitive)
            return dataSourceTypeString.ToLowerInvariant() switch
            {
                "sqlserver" => DataSourceType.SqlServer,
                "postgre" or "postgresql" => DataSourceType.Postgre,
                "mysql" or "mariadb" => DataSourceType.Mysql,
                "oracle" => DataSourceType.Oracle,
                "sqlite" => DataSourceType.SqlServer, // Sqlite may not be in enum, use SqlServer as fallback
                _ => DataSourceType.SqlServer // Default fallback
            };
        }
    }
}
