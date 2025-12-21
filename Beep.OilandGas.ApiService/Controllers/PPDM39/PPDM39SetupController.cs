using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Beep.Editor;
using System.Linq;
using Beep.OilandGas.ApiService.Models;

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

        public PPDM39SetupController(
            PPDM39SetupService setupService,
            IDMEEditor editor,
            ILogger<PPDM39SetupController> logger,
            IProgressTrackingService? progressTracking = null)
        {
            _setupService = setupService ?? throw new ArgumentNullException(nameof(setupService));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _progressTracking = progressTracking;
            
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

        /// <summary>
        /// Test database connection
        /// </summary>
        [HttpPost("test-connection")]
        public async Task<ActionResult<ConnectionTestResult>> TestConnection([FromBody] ConnectionConfig config)
        {
            try
            {
                if (config == null)
                {
                    return BadRequest(new { error = "Connection configuration is required" });
                }

                var result = await _setupService.TestConnectionAsync(config);
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
        public ActionResult<List<ScriptInfo>> GetAvailableScripts(string databaseType)
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
        /// Execute a single SQL script
        /// </summary>
        [HttpPost("execute-script")]
        public async Task<ActionResult<ScriptExecutionResult>> ExecuteScript([FromBody] ExecuteScriptRequest request)
        {
            try
            {
                if (request == null || request.Connection == null || string.IsNullOrEmpty(request.ScriptName))
                {
                    return BadRequest(new { error = "Connection and script name are required" });
                }

                var result = await _setupService.ExecuteScriptAsync(request.Connection, request.ScriptName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing script {ScriptName}", request?.ScriptName);
                return StatusCode(500, new ScriptExecutionResult
                {
                    ScriptName = request?.ScriptName ?? "Unknown",
                    Success = false,
                    Message = "Script execution failed",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Execute all scripts in order
        /// </summary>
        [HttpPost("execute-all-scripts")]
        public async Task<ActionResult<AllScriptsExecutionResult>> ExecuteAllScripts([FromBody] ScriptExecutionRequest request)
        {
            try
            {
                if (request == null || request.Connection == null)
                {
                    return BadRequest(new { error = "Connection configuration is required" });
                }

                if (request.ExecuteAll)
                {
                    // Get all available scripts
                    var scripts = _setupService.GetAvailableScripts(request.Connection.DatabaseType);
                    request.ScriptNames = scripts.OrderBy(s => s.ExecutionOrder).Select(s => s.Name).ToList();
                }

                if (request.ScriptNames == null || !request.ScriptNames.Any())
                {
                    return BadRequest(new { error = "No scripts to execute" });
                }

                var result = await _setupService.ExecuteAllScriptsAsync(request.Connection, request.ScriptNames);
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

                // TODO: Implement actual NuGet package installation
                // This would require access to the assembly handler and NuGet package service
                // For now, return a message indicating manual installation is needed
                return Ok(new InstallDriverResult
                {
                    Success = false,
                    Message = $"Driver installation is not yet implemented via API. Please install the NuGet package '{driverInfo.NuGetPackage}' manually or use BeepShell.",
                    ErrorDetails = "Automatic NuGet package installation requires assembly handler access which is not available in the API context. Use BeepShell 'nuget install' command or install manually."
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
    }

    // Request DTOs for controller
    public class CheckDriverRequest
    {
        public string DatabaseType { get; set; } = string.Empty;
    }

    public class ExecuteScriptRequest
    {
        public ConnectionConfig Connection { get; set; } = null!;
        public string ScriptName { get; set; } = string.Empty;
    }
}
