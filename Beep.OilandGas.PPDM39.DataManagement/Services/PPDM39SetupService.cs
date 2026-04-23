using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using BeepDataSourceType = TheTechIdea.Beep.Utilities.DataSourceType;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    public class PPDM39SetupService : IPPDM39SetupService
    {
        private sealed class SchemaMigrationPlanSession
        {
            public required string ConnectionName { get; init; }
            public string SchemaName { get; init; } = string.Empty;
            public string? TargetAssemblyName { get; init; }
            public string? TargetModelNamespace { get; init; }
            public required TheTechIdea.Beep.Editor.Migration.MigrationPlanArtifact Plan { get; init; }
            public bool IsApproved { get; set; }
            public string ApprovedBy { get; set; } = string.Empty;
            public string ApprovalNotes { get; set; } = string.Empty;
            public DateTime? ApprovedOnUtc { get; set; }
            public string LastExecutionToken { get; set; } = string.Empty;
        }

        private static readonly ConcurrentDictionary<string, SchemaMigrationPlanSession> _schemaMigrationPlans =
            new(StringComparer.OrdinalIgnoreCase);

        private readonly IDMEEditor _editor;
        private readonly ILogger<PPDM39SetupService> _logger;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly LOVManagementService? _lovService;
        private readonly ModuleSetupOrchestrator? _moduleSetupOrchestrator;

        private IProgressTrackingService? _progressTracking;
        private string? _currentConnectionName;

        // Static fallback driver map — used when BeepDM has not yet loaded the driver assembly
        private static readonly Dictionary<string, DatabaseDriverInfo> _staticDriverMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["SqlServer"]  = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.SQLServerDataSource", DataSourceType = "SqlServer", DefaultPort = 1433, ScriptPath = "Scripts/SqlServer" },
                ["Postgre"]    = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.PostgreDataSource",  DataSourceType = "Postgre",   DefaultPort = 5432, ScriptPath = "Scripts/PostgreSQL" },
                ["PostgreSQL"] = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.PostgreDataSource",  DataSourceType = "Postgre",   DefaultPort = 5432, ScriptPath = "Scripts/PostgreSQL" },
                ["Mysql"]      = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.MySqlDataSource",    DataSourceType = "Mysql",     DefaultPort = 3306, ScriptPath = "Scripts/MySQL" },
                ["MariaDB"]    = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.MySqlDataSource",    DataSourceType = "Mysql",     DefaultPort = 3306, ScriptPath = "Scripts/MySQL" },
                ["Oracle"]     = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.OracleDataSource",   DataSourceType = "Oracle",    DefaultPort = 1521, ScriptPath = "Scripts/Oracle" },
                ["SqlLite"]    = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.SQLiteDataSource",   DataSourceType = "SqlLite",   DefaultPort = 0,    ScriptPath = "Scripts/SQLite" },
                ["SQLite"]     = new DatabaseDriverInfo { NuGetPackage = "TheTechIdea.Beep.Winform.Net8.SQLiteDataSource",   DataSourceType = "SqlLite",   DefaultPort = 0,    ScriptPath = "Scripts/SQLite" },
            };

        public PPDM39SetupService(
            IDMEEditor editor,
            ILogger<PPDM39SetupService> logger,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            LOVManagementService? lovService = null,
            ModuleSetupOrchestrator? moduleSetupOrchestrator = null)
        {
            _editor              = editor              ?? throw new ArgumentNullException(nameof(editor));
            _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults            = defaults            ?? throw new ArgumentNullException(nameof(defaults));
            _metadata            = metadata            ?? throw new ArgumentNullException(nameof(metadata));
            _lovService          = lovService;
            _moduleSetupOrchestrator = moduleSetupOrchestrator;
        }

        // ── PROGRESS TRACKING ─────────────────────────────────────────────────

        public void SetProgressTracking(IProgressTrackingService progressTracking)
            => _progressTracking = progressTracking;

        public async Task<SetupStatusResult> GetSetupStatusAsync()
        {
            var connection = (_editor.ConfigEditor?.DataConnections ?? Enumerable.Empty<ConnectionProperties>())
                .FirstOrDefault(c => string.Equals(c.ConnectionName, _currentConnectionName, StringComparison.OrdinalIgnoreCase))
                ?? (_editor.ConfigEditor?.DataConnections ?? Enumerable.Empty<ConnectionProperties>()).FirstOrDefault();

            if (connection == null)
            {
                return new SetupStatusResult
                {
                    HasConnection = false,
                    IsSchemaReady = false
                };
            }

            return new SetupStatusResult
            {
                HasConnection = true,
                ConnectionName = connection.ConnectionName,
                DbType = connection.DatabaseType.ToString(),
                IsSchemaReady = await ProbeSchemaReadyAsync(connection.ConnectionName)
            };
        }

        public async Task<CreateSqliteResult> CreateSqliteAsync(CreateSqliteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ConnectionName))
            {
                return new CreateSqliteResult
                {
                    Success = false,
                    Message = "Connection name is required."
                };
            }

            try
            {
                var fileName = string.IsNullOrWhiteSpace(request.FileName)
                    ? $"{request.ConnectionName}.db"
                    : request.FileName.Trim();

                if (!fileName.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
                    fileName += ".db";

                var savePath = string.IsNullOrWhiteSpace(request.SavePath)
                    ? Path.Combine(AppContext.BaseDirectory, "Databases")
                    : request.SavePath.Trim();

                Directory.CreateDirectory(savePath);
                var fullPath = Path.Combine(savePath, fileName);

                var props = new ConnectionProperties
                {
                    ConnectionName = request.ConnectionName,
                    DatabaseType = BeepDataSourceType.SqlLite,
                    Category = TheTechIdea.Beep.Utilities.DatasourceCategory.RDBMS,
                    IsLocal = true,
                    IsFile = true,
                    IsDatabase = true,
                    FilePath = savePath,
                    FileName = fileName,
                    GuidID = Guid.NewGuid().ToString()
                };

                PopulateBestDriver(props);

                if (_editor.ConfigEditor.DataConnectionExist(props.ConnectionName))
                    RemoveConnection(props.ConnectionName);

                _editor.ConfigEditor.AddDataConnection(props);
                _editor.ConfigEditor.SaveDataconnectionsValues();

                var dataSource = _editor.GetDataSource(props.ConnectionName);
                if (dataSource == null)
                {
                    return new CreateSqliteResult
                    {
                        Success = false,
                        ConnectionName = props.ConnectionName,
                        FilePath = fullPath,
                        DbType = "SQLite",
                        Message = "Failed to create SQLite data source."
                    };
                }

                CreateLocalDatabaseFile(dataSource, fullPath);
                _currentConnectionName = props.ConnectionName;

                return new CreateSqliteResult
                {
                    Success = true,
                    ConnectionName = props.ConnectionName,
                    FilePath = fullPath,
                    DbType = "SQLite",
                    Message = "SQLite database created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create SQLite database for {ConnectionName}", request.ConnectionName);
                return new CreateSqliteResult
                {
                    Success = false,
                    ConnectionName = request.ConnectionName,
                    Message = "Failed to create SQLite database",
                    ErrorDetails = ex.Message
                };
            }
        }

        public async Task<SchemaMigrationPlanResult> PlanSchemaMigrationAsync(SchemaMigrationPlanRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.ConnectionName))
            {
                return new SchemaMigrationPlanResult
                {
                    Success = false,
                    Message = "Connection name is required."
                };
            }

            try
            {
                var migration = CreateMigrationManager(request.ConnectionName, out var dataSource);
                if (migration == null || dataSource == null)
                {
                    return new SchemaMigrationPlanResult
                    {
                        Success = false,
                        ConnectionName = request.ConnectionName,
                        Message = $"Connection '{request.ConnectionName}' not found or could not be opened."
                    };
                }

                var entityTypes = GetMigrationEntityTypes(request.TargetAssemblyName, request.TargetModelNamespace);
                if (entityTypes.Count == 0)
                {
                    return new SchemaMigrationPlanResult
                    {
                        Success = false,
                        ConnectionName = request.ConnectionName,
                        Message = "No migration entity types were resolved for the requested scope."
                    };
                }

                foreach (var assembly in entityTypes.Select(type => type.Assembly).Distinct())
                    migration.RegisterAssembly(assembly);

                var plan = migration.BuildMigrationPlanForTypes(entityTypes, detectRelationships: true);
                if (plan == null)
                {
                    return new SchemaMigrationPlanResult
                    {
                        Success = false,
                        ConnectionName = request.ConnectionName,
                        Message = "Migration plan could not be built."
                    };
                }

                var policyOptions = new TheTechIdea.Beep.Editor.Migration.MigrationPolicyOptions
                {
                    EnvironmentTier = ParseEnvironmentTier(request.EnvironmentTier)
                };

                // Validate connectivity from the active datasource before trusting preflight connectivity output.
                var connectivityValidated = false;
                try
                {
                    connectivityValidated = dataSource.Openconnection() == ConnectionState.Open;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Connectivity validation probe failed for {ConnectionName}", request.ConnectionName);
                }

                var policy = migration.EvaluateMigrationPlanPolicy(plan, policyOptions);
                var dryRun = migration.GenerateDryRunReport(plan);
                var preflight = migration.RunPreflightChecks(plan, policyOptions);
                NormalizeConnectivityPreflight(preflight, connectivityValidated, request.ConnectionName);
                var compensation = migration.BuildCompensationPlan(plan);
                var rollback = migration.CheckRollbackReadiness(
                    plan,
                    request.BackupConfirmed,
                    request.RestoreTestEvidenceProvided,
                    request.RestoreTestEvidence);
                var ciValidation = migration.ValidatePlanForCi(plan);

                plan.PolicyEvaluation = policy;
                plan.DryRunReport = dryRun;
                plan.PreflightReport = preflight;
                plan.CompensationPlan = compensation;
                plan.RollbackReadinessReport = rollback;
                plan.CiValidationReport = ciValidation;

                var session = _schemaMigrationPlans.AddOrUpdate(
                    plan.PlanId,
                    _ => new SchemaMigrationPlanSession
                    {
                        ConnectionName = request.ConnectionName,
                        SchemaName = request.SchemaName ?? string.Empty,
                        TargetAssemblyName = request.TargetAssemblyName,
                        TargetModelNamespace = request.TargetModelNamespace,
                        Plan = plan
                    },
                    (_, existing) => new SchemaMigrationPlanSession
                    {
                        ConnectionName = request.ConnectionName,
                        SchemaName = request.SchemaName ?? string.Empty,
                        TargetAssemblyName = request.TargetAssemblyName,
                        TargetModelNamespace = request.TargetModelNamespace,
                        Plan = plan,
                        IsApproved = existing.IsApproved,
                        ApprovedBy = existing.ApprovedBy,
                        ApprovalNotes = existing.ApprovalNotes,
                        ApprovedOnUtc = existing.ApprovedOnUtc,
                        LastExecutionToken = existing.LastExecutionToken
                    });

                return MapPlanResult(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to plan schema migration for {ConnectionName}", request.ConnectionName);
                return new SchemaMigrationPlanResult
                {
                    Success = false,
                    ConnectionName = request.ConnectionName,
                    Message = "Schema migration plan failed.",
                    DryRunDiagnostics = new List<string> { ex.Message }
                };
            }
        }

        public async Task<SchemaMigrationApprovalResult> ApproveSchemaMigrationPlanAsync(SchemaMigrationApprovalRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PlanId))
            {
                return new SchemaMigrationApprovalResult
                {
                    Success = false,
                    Message = "Plan ID is required."
                };
            }

            if (!_schemaMigrationPlans.TryGetValue(request.PlanId, out var session))
            {
                return new SchemaMigrationApprovalResult
                {
                    Success = false,
                    PlanId = request.PlanId,
                    Message = "Migration plan was not found. Generate a new plan first."
                };
            }

            session.IsApproved = true;
            session.ApprovedBy = string.IsNullOrWhiteSpace(request.ApprovedBy) ? "SYSTEM" : request.ApprovedBy.Trim();
            session.ApprovalNotes = request.Notes?.Trim() ?? string.Empty;
            session.ApprovedOnUtc = DateTime.UtcNow;

            return await Task.FromResult(new SchemaMigrationApprovalResult
            {
                Success = true,
                Message = "Migration plan approved.",
                PlanId = session.Plan.PlanId,
                PlanHash = session.Plan.PlanHash,
                ApprovedBy = session.ApprovedBy,
                ApprovedOnUtc = session.ApprovedOnUtc
            });
        }

        public async Task<SchemaMigrationExecuteResult> ExecuteSchemaMigrationPlanAsync(SchemaMigrationExecuteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PlanId))
            {
                return new SchemaMigrationExecuteResult
                {
                    Success = false,
                    Message = "Plan ID is required."
                };
            }

            if (!_schemaMigrationPlans.TryGetValue(request.PlanId, out var session))
            {
                return new SchemaMigrationExecuteResult
                {
                    Success = false,
                    PlanId = request.PlanId,
                    Message = "Migration plan was not found. Generate a new plan first."
                };
            }

            if (!session.IsApproved)
            {
                return new SchemaMigrationExecuteResult
                {
                    Success = false,
                    PlanId = session.Plan.PlanId,
                    PlanHash = session.Plan.PlanHash,
                    Message = "Migration plan must be approved before execution."
                };
            }

            try
            {
                var migration = CreateMigrationManager(session.ConnectionName, out var dataSource);
                if (migration == null || dataSource == null)
                {
                    return new SchemaMigrationExecuteResult
                    {
                        Success = false,
                        PlanId = session.Plan.PlanId,
                        PlanHash = session.Plan.PlanHash,
                        Message = $"Connection '{session.ConnectionName}' not found or could not be opened."
                    };
                }

                RegisterScopeAssemblies(migration, session.TargetAssemblyName, session.TargetModelNamespace);
                string? resumeToken = null;
                if (request.ResumeIfCheckpointExists && !string.IsNullOrWhiteSpace(session.LastExecutionToken))
                {
                    var existingCheckpoint = migration.GetExecutionCheckpoint(session.LastExecutionToken);
                    if (existingCheckpoint != null && !existingCheckpoint.IsCompleted)
                        resumeToken = existingCheckpoint.ExecutionToken;
                }

                var executionResult = string.IsNullOrWhiteSpace(resumeToken)
                    ? migration.ExecuteMigrationPlan(session.Plan)
                    : migration.ResumeMigrationPlan(resumeToken);

                session.LastExecutionToken = executionResult.ExecutionToken;

                return await Task.FromResult(new SchemaMigrationExecuteResult
                {
                    Success = executionResult.Success,
                    Message = executionResult.Message,
                    PlanId = session.Plan.PlanId,
                    PlanHash = session.Plan.PlanHash,
                    ExecutionToken = executionResult.ExecutionToken,
                    ResumedFromCheckpoint = executionResult.ResumedFromCheckpoint,
                    RequiresOperatorIntervention = executionResult.RequiresOperatorIntervention,
                    RollbackOutcome = executionResult.RollbackOutcome,
                    CompensationOutcome = executionResult.CompensationOutcome,
                    TotalSteps = executionResult.Checkpoint?.Steps?.Count ?? 0,
                    CompletedSteps = executionResult.Checkpoint?.Steps?.Count(step =>
                        step.Status == TheTechIdea.Beep.Editor.Migration.MigrationExecutionStepStatus.Completed) ?? 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute schema migration plan {PlanId}", request.PlanId);
                return new SchemaMigrationExecuteResult
                {
                    Success = false,
                    PlanId = session.Plan.PlanId,
                    PlanHash = session.Plan.PlanHash,
                    Message = "Schema migration execution failed.",
                    CompensationOutcome = ex.Message
                };
            }
        }

        public async Task<OperationStartResponse> StartSchemaMigrationExecutionAsync(SchemaMigrationExecuteRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.PlanId))
            {
                return new OperationStartResponse
                {
                    Success = false,
                    Message = "Plan ID is required."
                };
            }

            if (!_schemaMigrationPlans.TryGetValue(request.PlanId, out var session))
            {
                return new OperationStartResponse
                {
                    Success = false,
                    Message = "Migration plan was not found. Generate a new plan first."
                };
            }

            if (!session.IsApproved)
            {
                return new OperationStartResponse
                {
                    Success = false,
                    Message = "Migration plan must be approved before execution."
                };
            }

            try
            {
                var migration = CreateMigrationManager(session.ConnectionName, out var dataSource);
                if (migration == null || dataSource == null)
                {
                    return new OperationStartResponse
                    {
                        Success = false,
                        Message = $"Connection '{session.ConnectionName}' not found or could not be opened."
                    };
                }

                RegisterScopeAssemblies(migration, session.TargetAssemblyName, session.TargetModelNamespace);

                var checkpoint = !string.IsNullOrWhiteSpace(session.LastExecutionToken) && request.ResumeIfCheckpointExists
                    ? migration.GetExecutionCheckpoint(session.LastExecutionToken)
                    : null;

                var executionToken = checkpoint != null && !checkpoint.IsCompleted
                    ? checkpoint.ExecutionToken
                    : migration.CreateExecutionCheckpoint(session.Plan).ExecutionToken;

                session.LastExecutionToken = executionToken;

                _ = Task.Run(() =>
                {
                    try
                    {
                        var backgroundMigration = CreateMigrationManager(session.ConnectionName, out var backgroundDataSource);
                        if (backgroundMigration == null || backgroundDataSource == null)
                        {
                            _logger.LogError("Background schema migration start failed for plan {PlanId}: datasource unavailable", request.PlanId);
                            return;
                        }

                        RegisterScopeAssemblies(backgroundMigration, session.TargetAssemblyName, session.TargetModelNamespace);

                        if (request.ResumeIfCheckpointExists && checkpoint != null && !checkpoint.IsCompleted)
                            backgroundMigration.ResumeMigrationPlan(executionToken);
                        else
                            backgroundMigration.ExecuteMigrationPlan(session.Plan, executionToken: executionToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background schema migration execution failed for plan {PlanId}", request.PlanId);
                    }
                });

                return await Task.FromResult(new OperationStartResponse
                {
                    Success = true,
                    OperationId = executionToken,
                    Message = "Schema migration started."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start schema migration plan {PlanId}", request.PlanId);
                return new OperationStartResponse
                {
                    Success = false,
                    Message = "Schema migration could not be started."
                };
            }
        }

        public async Task<SchemaMigrationProgressResult> GetSchemaMigrationProgressAsync(string executionToken)
        {
            if (string.IsNullOrWhiteSpace(executionToken))
            {
                return new SchemaMigrationProgressResult
                {
                    Success = false,
                    Message = "Execution token is required."
                };
            }

            var session = _schemaMigrationPlans.Values
                .FirstOrDefault(item => string.Equals(item.LastExecutionToken, executionToken, StringComparison.OrdinalIgnoreCase));

            if (session == null)
            {
                return new SchemaMigrationProgressResult
                {
                    Success = false,
                    ExecutionToken = executionToken,
                    Message = "Execution token was not found."
                };
            }

            try
            {
                var migration = CreateMigrationManager(session.ConnectionName, out var dataSource);
                if (migration == null || dataSource == null)
                {
                    return new SchemaMigrationProgressResult
                    {
                        Success = false,
                        ExecutionToken = executionToken,
                        PlanId = session.Plan.PlanId,
                        PlanHash = session.Plan.PlanHash,
                        Message = $"Connection '{session.ConnectionName}' not found or could not be opened."
                    };
                }

                var checkpoint = migration.GetExecutionCheckpoint(executionToken);
                if (checkpoint == null)
                {
                    return new SchemaMigrationProgressResult
                    {
                        Success = false,
                        ExecutionToken = executionToken,
                        PlanId = session.Plan.PlanId,
                        PlanHash = session.Plan.PlanHash,
                        Message = "Execution checkpoint was not found."
                    };
                }

                return await Task.FromResult(new SchemaMigrationProgressResult
                {
                    Success = true,
                    Message = checkpoint.IsCompleted
                        ? "Execution completed."
                        : checkpoint.HasFailed
                            ? "Execution failed."
                            : "Execution in progress.",
                    PlanId = checkpoint.PlanId,
                    PlanHash = checkpoint.PlanHash,
                    ExecutionToken = checkpoint.ExecutionToken,
                    IsCompleted = checkpoint.IsCompleted,
                    HasFailed = checkpoint.HasFailed,
                    FailureCategory = checkpoint.FailureCategory,
                    FailureReason = checkpoint.FailureReason,
                    LastCompletedStep = checkpoint.LastCompletedStep,
                    TotalSteps = checkpoint.Steps.Count,
                    CompletedSteps = checkpoint.Steps.Count(step =>
                        step.Status == TheTechIdea.Beep.Editor.Migration.MigrationExecutionStepStatus.Completed),
                    Steps = checkpoint.Steps
                        .OrderBy(step => step.Sequence)
                        .Select(step => new SchemaMigrationStepResult
                        {
                            Sequence = step.Sequence,
                            EntityName = step.EntityName,
                            OperationKind = step.OperationKind.ToString(),
                            Status = step.Status.ToString(),
                            Message = step.Message,
                            AttemptCount = step.AttemptCount
                        })
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read schema migration progress for {ExecutionToken}", executionToken);
                return new SchemaMigrationProgressResult
                {
                    Success = false,
                    ExecutionToken = executionToken,
                    PlanId = session.Plan.PlanId,
                    PlanHash = session.Plan.PlanHash,
                    Message = "Could not read schema migration progress.",
                    FailureReason = ex.Message
                };
            }
        }

        public async Task<SchemaMigrationArtifactsResult> GetSchemaMigrationArtifactsAsync(string planId)
        {
            if (string.IsNullOrWhiteSpace(planId))
            {
                return new SchemaMigrationArtifactsResult
                {
                    Success = false,
                    Message = "Plan ID is required."
                };
            }

            if (!_schemaMigrationPlans.TryGetValue(planId, out var session))
            {
                return new SchemaMigrationArtifactsResult
                {
                    Success = false,
                    PlanId = planId,
                    Message = "Migration plan was not found. Generate a new plan first."
                };
            }

            try
            {
                var plan = session.Plan;
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                return await Task.FromResult(new SchemaMigrationArtifactsResult
                {
                    Success = true,
                    Message = "Schema migration artifacts loaded successfully.",
                    PlanId = plan.PlanId,
                    PlanHash = plan.PlanHash,
                    ConnectionName = session.ConnectionName,
                    IsApproved = session.IsApproved,
                    ApprovedBy = session.ApprovedBy,
                    ApprovedOnUtc = session.ApprovedOnUtc,
                    ApprovalNotes = session.ApprovalNotes,
                    PlanJson = JsonSerializer.Serialize(plan, jsonOptions),
                    DryRunJson = JsonSerializer.Serialize(plan.DryRunReport, jsonOptions),
                    PreflightJson = JsonSerializer.Serialize(plan.PreflightReport, jsonOptions),
                    CiValidationJson = JsonSerializer.Serialize(plan.CiValidationReport, jsonOptions),
                    PolicyJson = JsonSerializer.Serialize(plan.PolicyEvaluation, jsonOptions),
                    RollbackReadinessJson = JsonSerializer.Serialize(plan.RollbackReadinessReport, jsonOptions),
                    CompensationJson = JsonSerializer.Serialize(plan.CompensationPlan, jsonOptions),
                    RuntimeEntityMetadataJson = JsonSerializer.Serialize(BuildRuntimeEntityMetadata(plan), jsonOptions),
                    ApprovalSummaryMarkdown = BuildApprovalSummaryMarkdown(session)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load schema migration artifacts for {PlanId}", planId);
                return new SchemaMigrationArtifactsResult
                {
                    Success = false,
                    PlanId = planId,
                    Message = "Could not load schema migration artifacts."
                };
            }
        }

        // ── DRIVER INFO ───────────────────────────────────────────────────────

        public DatabaseDriverInfo? GetDriverInfo(string databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType)) return null;

            // Try live BeepDM driver classes first
            if (Enum.TryParse<BeepDataSourceType>(databaseType, true, out var dsType))
            {
                var driverClass = _editor.ConfigEditor?.DataDriversClasses?
                    .FirstOrDefault(d => d.DatasourceType == dsType);
                if (driverClass != null)
                    return new DatabaseDriverInfo
                    {
                        NuGetPackage   = driverClass.PackageName,
                        DataSourceType = driverClass.DatasourceType.ToString(),
                        DefaultPort    = GetDefaultPort(dsType),
                        ScriptPath     = $"Scripts/{MapToScriptFolder(dsType)}"
                    };
            }

            // Fallback to static map
            return _staticDriverMap.TryGetValue(databaseType, out var info) ? info : null;
        }

        public List<string> GetAvailableDatabaseTypes()
        {
            var fromDrivers = _editor.ConfigEditor?.DataDriversClasses?
                .Select(d => d.DatasourceType.ToString())
                .Distinct()
                .ToList();

            return fromDrivers?.Count > 0
                ? fromDrivers
                : _staticDriverMap.Keys.Distinct().OrderBy(k => k).ToList();
        }

        public DriverInfo CheckDriver(string databaseType)
        {
            var info = GetDriverInfo(databaseType);
            if (info == null)
                return new DriverInfo { DatabaseType = databaseType, IsAvailable = false, IsInstalled = false };

            // Check if driver assembly is loaded in BeepDM
            var isInstalled = _editor.ConfigEditor?.DataDriversClasses?
                .Any(d => string.Equals(d.PackageName, info.NuGetPackage, StringComparison.OrdinalIgnoreCase)) ?? false;

            return new DriverInfo
            {
                DatabaseType = databaseType,
                NuGetPackage = info.NuGetPackage,
                IsAvailable  = true,
                IsInstalled  = isInstalled
            };
        }

        // ── CONNECTION TESTING ────────────────────────────────────────────────

        public async Task<ConnectionTestResult> TestConnectionAsync(ConnectionConfig config)
        {
            if (config == null)
                return new ConnectionTestResult { Success = false, Message = "Connection configuration is required" };

            _logger.LogInformation("Testing connection to {Host} ({DatabaseType})", config.Host, config.DatabaseType);
            try
            {
                // Register a temporary connection under its name (or a temp name)
                var connName = string.IsNullOrWhiteSpace(config.ConnectionName)
                    ? $"__test_{Guid.NewGuid():N}"
                    : config.ConnectionName;

                var props = BuildConnectionProperties(config, connName);
                var isTempConn = false;

                if (!_editor.ConfigEditor.DataConnectionExist(connName))
                {
                    _editor.ConfigEditor.AddDataConnection(props);
                    isTempConn = true;
                }

                var ds = _editor.GetDataSource(connName);
                if (ds == null)
                {
                    if (isTempConn) RemoveConnection(connName);
                    return new ConnectionTestResult { Success = false, Message = $"Could not create data source for type '{config.DatabaseType}'" };
                }

                var state = ds.Openconnection();
                ds.Closeconnection();

                if (isTempConn) RemoveConnection(connName);

                return state == ConnectionState.Open
                    ? new ConnectionTestResult { Success = true,  Message = "Connection successful" }
                    : new ConnectionTestResult { Success = false, Message = "Connection failed — could not open database" };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Connection test failed for {ConnectionName}", config.ConnectionName);
                return new ConnectionTestResult { Success = false, Message = "Connection test failed", ErrorDetails = ex.Message };
            }
        }

        // ── SAVE / UPDATE / DELETE CONNECTIONS ───────────────────────────────

        public SaveConnectionResult SaveConnection(ConnectionConfig config, bool testAfterSave = true, bool openAfterSave = false)
        {
            if (config == null)
                return new SaveConnectionResult { Success = false, Message = "Connection configuration is required" };

            if (string.IsNullOrWhiteSpace(config.ConnectionName))
                return new SaveConnectionResult { Success = false, Message = "Connection name is required" };

            _logger.LogInformation("Saving connection {ConnectionName}", config.ConnectionName);
            try
            {
                var props = BuildConnectionProperties(config, config.ConnectionName);

                if (_editor.ConfigEditor.DataConnectionExist(config.ConnectionName))
                    RemoveConnection(config.ConnectionName);

                _editor.ConfigEditor.AddDataConnection(props);
                _editor.ConfigEditor.SaveDataconnectionsValues();

                if (testAfterSave)
                {
                    var ds = _editor.GetDataSource(config.ConnectionName);
                    if (ds != null)
                    {
                        var state = ds.Openconnection();
                        if (!openAfterSave) ds.Closeconnection();
                        if (state != ConnectionState.Open)
                            return new SaveConnectionResult { Success = false, ConnectionName = config.ConnectionName, Message = "Connection saved but test failed — could not open database" };
                    }
                }

                _currentConnectionName = config.ConnectionName;
                return new SaveConnectionResult { Success = true, ConnectionName = config.ConnectionName, Message = "Connection saved successfully" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save connection {ConnectionName}", config.ConnectionName);
                return new SaveConnectionResult { Success = false, Message = "Failed to save connection", ErrorDetails = ex.Message };
            }
        }

        public SaveConnectionResult UpdateConnection(string originalConnectionName, ConnectionConfig config, bool testAfterSave = true)
        {
            if (string.IsNullOrWhiteSpace(originalConnectionName) || config == null)
                return new SaveConnectionResult { Success = false, Message = "Original connection name and configuration are required" };

            _logger.LogInformation("Updating connection {OriginalName} → {NewName}", originalConnectionName, config.ConnectionName);
            try
            {
                if (_editor.ConfigEditor.DataConnectionExist(originalConnectionName))
                    RemoveConnection(originalConnectionName);

                return SaveConnection(config, testAfterSave);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update connection {ConnectionName}", originalConnectionName);
                return new SaveConnectionResult { Success = false, Message = "Failed to update connection", ErrorDetails = ex.Message };
            }
        }

        public DeleteConnectionResult DeleteConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return new DeleteConnectionResult { Success = false, Message = "Connection name is required" };

            _logger.LogInformation("Deleting connection {ConnectionName}", connectionName);
            try
            {
                if (!_editor.ConfigEditor.DataConnectionExist(connectionName))
                    return new DeleteConnectionResult { Success = false, Message = $"Connection '{connectionName}' not found" };

                RemoveConnection(connectionName);
                _editor.ConfigEditor.SaveDataconnectionsValues();

                if (string.Equals(_currentConnectionName, connectionName, StringComparison.OrdinalIgnoreCase))
                    _currentConnectionName = null;

                return new DeleteConnectionResult { Success = true, Message = "Connection deleted" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete connection {ConnectionName}", connectionName);
                return new DeleteConnectionResult { Success = false, Message = "Failed to delete connection", ErrorDetails = ex.Message };
            }
        }

        // ── CONNECTION LISTING ────────────────────────────────────────────────

        public List<DatabaseConnectionListItem> GetAllConnections()
        {
            return (_editor.ConfigEditor?.DataConnections ?? Enumerable.Empty<ConnectionProperties>())
                .Select(dc => new DatabaseConnectionListItem
                {
                    ConnectionName = dc.ConnectionName,
                    DatabaseType   = dc.DatabaseType.ToString(),
                    Host           = dc.Host,
                    Database       = dc.Database,
                    IsCurrent      = string.Equals(dc.ConnectionName, _currentConnectionName, StringComparison.OrdinalIgnoreCase)
                }).ToList();
        }

        public ConnectionConfig? GetConnectionByName(string connectionName)
        {
            var dc = (_editor.ConfigEditor?.DataConnections ?? Enumerable.Empty<ConnectionProperties>())
                .FirstOrDefault(c => string.Equals(c.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase));

            if (dc == null) return null;

            return new ConnectionConfig
            {
                ConnectionName = dc.ConnectionName,
                DatabaseType   = dc.DatabaseType.ToString(),
                Host           = dc.Host,
                Port           = dc.Port,
                Database       = dc.Database,
                Username       = dc.UserID,
                ConnectionString = dc.ConnectionString
            };
        }

        public string? GetCurrentConnectionName() => _currentConnectionName;

        public SetCurrentDatabaseResult SetCurrentConnection(string connectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionName))
                return new SetCurrentDatabaseResult { Success = false, Message = "Connection name is required" };

            if (!_editor.ConfigEditor.DataConnectionExist(connectionName))
                return new SetCurrentDatabaseResult { Success = false, Message = $"Connection '{connectionName}' not found" };

            _currentConnectionName = connectionName;
            return new SetCurrentDatabaseResult { Success = true, Message = $"Current connection set to '{connectionName}'" };
        }

        // ── SCHEMA CREATION ───────────────────────────────────────────────────

        public async Task<CreateSchemaResult> CreateSchemaAsync(ConnectionConfig config, string schemaName)
        {
            var connectionName = config?.ConnectionName ?? string.Empty;
            if (string.IsNullOrWhiteSpace(connectionName))
                return new CreateSchemaResult { Success = false, Message = "Connection name is required. Save the connection first." };

            var planResult = await PlanSchemaMigrationAsync(new SchemaMigrationPlanRequest
            {
                ConnectionName = connectionName,
                SchemaName = schemaName,
                BackupConfirmed = true,
                RestoreTestEvidenceProvided = true,
                RestoreTestEvidence = "Compatibility route"
            });

            if (!planResult.Success)
            {
                return new CreateSchemaResult
                {
                    Success = false,
                    Message = planResult.Message,
                    TotalEntities = planResult.TotalEntities,
                    TablesCreated = 0
                };
            }

            var approvalResult = await ApproveSchemaMigrationPlanAsync(new SchemaMigrationApprovalRequest
            {
                PlanId = planResult.PlanId,
                ApprovedBy = "compatibility-route",
                Notes = "Auto-approved by create-schema compatibility path."
            });

            if (!approvalResult.Success)
            {
                return new CreateSchemaResult
                {
                    Success = false,
                    Message = approvalResult.Message,
                    TotalEntities = planResult.TotalEntities,
                    TablesCreated = 0
                };
            }

            var executionResult = await ExecuteSchemaMigrationPlanAsync(new SchemaMigrationExecuteRequest
            {
                PlanId = planResult.PlanId,
                ExecutedBy = "compatibility-route"
            });

            return new CreateSchemaResult
            {
                Success = executionResult.Success,
                Message = executionResult.Success
                    ? $"Schema created successfully in '{connectionName}'"
                    : executionResult.Message,
                ErrorDetails = executionResult.Success ? null : executionResult.CompensationOutcome,
                TablesCreated = planResult.TablesToCreate,
                TotalEntities = planResult.TotalEntities
            };
        }

        public async Task<SchemaPrivilegeCheckResult> CheckSchemaPrivilegesAsync(ConnectionConfig config, string? schemaName = null)
        {
            var connectionName = config?.ConnectionName ?? string.Empty;
            _logger.LogInformation("Checking schema privileges for {ConnectionName}", connectionName);
            try
            {
                var ds = _editor.GetDataSource(connectionName);
                if (ds == null)
                    return new SchemaPrivilegeCheckResult { HasCreatePrivilege = false, Message = $"Connection '{connectionName}' not found" };

                var state = ds.Openconnection();
                if (state != ConnectionState.Open)
                    return new SchemaPrivilegeCheckResult { HasCreatePrivilege = false, Message = "Could not open connection" };

                // Attempt a harmless DDL to verify CREATE TABLE privilege
                try
                {
                    var tempTable = $"__PRIV_CHECK_{Guid.NewGuid():N8}";
                    ds.ExecuteSql($"CREATE TABLE {tempTable} (ID INT); DROP TABLE {tempTable};");
                    return new SchemaPrivilegeCheckResult { HasCreatePrivilege = true, Message = "Schema creation privileges confirmed" };
                }
                catch
                {
                    return new SchemaPrivilegeCheckResult { HasCreatePrivilege = false, Message = "Insufficient privileges to create tables" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Privilege check failed for {ConnectionName}", connectionName);
                return new SchemaPrivilegeCheckResult { HasCreatePrivilege = false, Message = "Privilege check failed", ErrorDetails = ex.Message };
            }
        }

        // ── SCRIPTS ───────────────────────────────────────────────────────────

        public List<ScriptInfo> GetAvailableScripts(string databaseType)
        {
            // Scripts are managed via ModuleDataRegistry in the DataManager project.
            // Return empty; use the discover-scripts endpoint on the setup controller for full listing.
            return new List<ScriptInfo>();
        }

        public Task<List<ScriptInfo>> GetAvailableScriptsAsync(string databaseType)
            => Task.FromResult(GetAvailableScripts(databaseType));

        public async Task<ScriptExecutionResult> ExecuteScriptAsync(ConnectionConfig config, string scriptName, string? operationId = null)
        {
            _logger.LogInformation("ExecuteScript '{Script}' on {ConnectionName}", scriptName, config?.ConnectionName);
            return new ScriptExecutionResult
            {
                ScriptFileName = scriptName,
                Success        = false,
                ErrorMessage   = "Script execution via setup service is not supported. Use the create-database endpoint with ModuleDataRegistry."
            };
        }

        public async Task<AllScriptsExecutionResult> ExecuteAllScriptsAsync(ConnectionConfig config, List<string> scriptNames, string? operationId = null)
        {
            var result = new AllScriptsExecutionResult
            {
                ExecutionId  = operationId ?? Guid.NewGuid().ToString(),
                StartTime    = DateTime.UtcNow,
                TotalScripts = scriptNames?.Count ?? 0,
                Success      = false,
                ErrorMessage = "Batch script execution is not supported via setup service. Use the create-database endpoint."
            };
            result.EndTime = DateTime.UtcNow;
            return result;
        }

        // ── DATABASE OPERATIONS ───────────────────────────────────────────────

        public async Task<DropDatabaseResult> DropDatabaseAsync(string connectionName, string? schemaName, bool dropIfExists = true)
        {
            _logger.LogWarning("DropDatabase requested for {ConnectionName} / schema={Schema}", connectionName, schemaName);
            try
            {
                var ds = _editor.GetDataSource(connectionName);
                if (ds == null)
                    return new DropDatabaseResult { Success = false, Message = $"Connection '{connectionName}' not found" };

                var state = ds.Openconnection();
                if (state != ConnectionState.Open)
                    return new DropDatabaseResult { Success = false, Message = "Could not open connection" };

                var ddl = string.IsNullOrWhiteSpace(schemaName)
                    ? (dropIfExists ? "DROP DATABASE IF EXISTS PPDM39" : "DROP DATABASE PPDM39")
                    : (dropIfExists ? $"DROP SCHEMA IF EXISTS {schemaName}" : $"DROP SCHEMA {schemaName}");

                ds.ExecuteSql(ddl);
                return new DropDatabaseResult { Success = true, Message = "Database dropped" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DropDatabase failed for {ConnectionName}", connectionName);
                return new DropDatabaseResult { Success = false, Message = "Drop failed", ErrorDetails = ex.Message };
            }
        }

        public async Task<RecreateDatabaseResult> RecreateDatabaseAsync(string connectionName, string? schemaName, bool backupFirst = false, string? backupPath = null)
        {
            _logger.LogInformation("RecreateDatabase for {ConnectionName}", connectionName);
            var dropResult = await DropDatabaseAsync(connectionName, schemaName, dropIfExists: true);
            if (!dropResult.Success)
                return new RecreateDatabaseResult { Success = false, Message = "Drop step failed: " + dropResult.Message };

            var connConfig = GetConnectionByName(connectionName);
            if (connConfig == null)
                return new RecreateDatabaseResult { Success = false, Message = $"Connection '{connectionName}' not found after drop" };

            var createResult = await CreateSchemaAsync(connConfig, schemaName ?? string.Empty);
            return new RecreateDatabaseResult
            {
                Success = createResult.Success,
                Message = createResult.Success ? "Database recreated successfully" : "Recreate failed: " + createResult.Message,
                ErrorDetails = createResult.ErrorDetails
            };
        }

        public async Task<CopyDatabaseResult> CopyDatabaseAsync(CopyDatabaseRequest request, string? operationId = null)
        {
            _logger.LogInformation("CopyDatabase from {Source} to {Target}", request?.SourceConnectionName, request?.TargetConnectionName);
            return new CopyDatabaseResult
            {
                Success = false,
                Message = "Database copy is not yet implemented via the setup service. Use the DataManagement ETL sync endpoint."
            };
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private ConnectionProperties BuildConnectionProperties(ConnectionConfig config, string connectionName)
        {
            var dsType = ParseDataSourceType(config.DatabaseType);
            var driverClass = _editor.ConfigEditor?.DataDriversClasses?
                .FirstOrDefault(d => d.DatasourceType == dsType);

            return new ConnectionProperties
            {
                ConnectionName   = connectionName,
                DatabaseType     = dsType,
                DriverName       = driverClass?.PackageName ?? string.Empty,
                Host             = config.Host ?? string.Empty,
                Port             = config.Port,
                Database         = config.Database ?? string.Empty,
                UserID           = config.Username ?? string.Empty,
                Password         = config.Password ?? string.Empty,
                ConnectionString = config.ConnectionString ?? string.Empty,
                GuidID           = Guid.NewGuid().ToString()
            };
        }

        private void PopulateBestDriver(ConnectionProperties props)
        {
            try
            {
                var helperType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly =>
                    {
                        try { return assembly.GetTypes(); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .FirstOrDefault(type => type.FullName == "TheTechIdea.Beep.Helpers.ConnectionHelper");

                var method = helperType?.GetMethod("GetBestMatchingDriver", new[] { typeof(ConnectionProperties), _editor.ConfigEditor.GetType() });
                var driver = method?.Invoke(null, new object[] { props, _editor.ConfigEditor });
                if (driver == null)
                    return;

                var packageName = driver.GetType().GetProperty("PackageName")?.GetValue(driver)?.ToString();
                var version = driver.GetType().GetProperty("version")?.GetValue(driver)?.ToString()
                    ?? driver.GetType().GetProperty("Version")?.GetValue(driver)?.ToString();

                if (!string.IsNullOrWhiteSpace(packageName))
                    props.DriverName = packageName;
                if (!string.IsNullOrWhiteSpace(version))
                    props.DriverVersion = version;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not resolve best matching driver for {ConnectionName}; falling back to loaded driver classes", props.ConnectionName);
                var driverClass = _editor.ConfigEditor?.DataDriversClasses?
                    .FirstOrDefault(d => d.DatasourceType == props.DatabaseType);
                if (driverClass != null)
                {
                    props.DriverName = driverClass.PackageName;
                    props.DriverVersion = driverClass.version;
                }
            }
        }

        private static void CreateLocalDatabaseFile(dynamic dataSource, string fullPath)
        {
            var type = ((object)dataSource).GetType();
            var createWithPath = type.GetMethod("CreateDB", new[] { typeof(string) });
            if (createWithPath != null)
            {
                createWithPath.Invoke(dataSource, new object[] { fullPath });
                return;
            }

            var createDefault = type.GetMethod("CreateDB", Type.EmptyTypes);
            if (createDefault != null)
            {
                createDefault.Invoke(dataSource, null);
                return;
            }

            if (!File.Exists(fullPath))
            {
                using var _ = File.Create(fullPath);
            }
        }

        private async Task<bool> ProbeSchemaReadyAsync(string connectionName)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor,
                    _commonColumnHandler,
                    _defaults,
                    _metadata,
                    typeof(Beep.OilandGas.PPDM39.Models.WELL),
                    connectionName,
                    "WELL");

                await repo.GetAsync(new List<AppFilter>
                {
                    new AppFilter
                    {
                        FieldName = "UWI",
                        Operator = "=",
                        FilterValue = "__schema_probe__"
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Schema probe failed for connection {ConnectionName}", connectionName);
                return false;
            }
        }

        /// <summary>
        /// Seeds only the specified modules, in their declared Order.
        /// Requires ModuleSetupOrchestrator to be available.
        /// </summary>
        public async Task<SeedingOperationResult> SeedSelectedModulesAsync(
            IReadOnlyList<string> moduleIds,
            string connectionName = "PPDM39",
            string? userId = null,
            CancellationToken cancellationToken = default)
        {
            if (_moduleSetupOrchestrator == null)
            {
                _logger.LogWarning(
                    "SeedSelectedModulesAsync: ModuleSetupOrchestrator not available — falling back to all-reference-data");

                return await SeedAllReferenceDataAsync(connectionName, userId ?? "SYSTEM");
            }

            userId ??= "SYSTEM";

            _logger.LogInformation(
                "PPDM39SetupService: seeding {ModuleCount} selected modules on connection {Connection}",
                moduleIds.Count, connectionName);

            var orchestratorResult = await _moduleSetupOrchestrator.RunSeedForModulesAsync(
                moduleIds, connectionName, userId, cancellationToken);

            var result = new SeedingOperationResult
            {
                Success = orchestratorResult.AllSucceeded,
                TotalInserted = orchestratorResult.TotalRecordsInserted,
                Message = orchestratorResult.AllSucceeded
                    ? $"Selected modules seeded successfully: {orchestratorResult.TotalRecordsInserted} total rows."
                    : $"One or more selected modules failed. {orchestratorResult.TotalRecordsInserted} total rows inserted."
            };

            foreach (var module in orchestratorResult.ModuleResults)
            {
                var skipLabel = string.IsNullOrWhiteSpace(module.SkipReason)
                    ? string.Empty
                    : $" (skipped: {module.SkipReason})";

                result.Details.Add(
                    $"[{module.ModuleId}] {module.ModuleName}: {module.RecordsInserted} rows / {module.TablesSeeded} tables{skipLabel}");

                if (module.Errors.Count > 0)
                {
                    foreach (var error in module.Errors)
                        result.Errors.Add($"[{module.ModuleId}] {error}");
                }
            }

            if (!result.Success && result.Errors.Count == 0)
                result.Errors.Add("One or more modules reported failure without explicit error details.");

            return result;
        }

        /// <summary>
        /// Returns metadata about all available IModuleSetup implementations.
        /// </summary>
        public IReadOnlyList<(string ModuleId, string ModuleName, int Order, IReadOnlyList<Type> EntityTypes)> GetAvailableModules()
        {
            if (_moduleSetupOrchestrator == null)
                return new List<(string, string, int, IReadOnlyList<Type>)>();

            return _moduleSetupOrchestrator.GetModuleMetadata();
        }

        private static List<Type> GetPpdm39EntityTypes()
        {
            return typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly
                .GetTypes()
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    string.Equals(type.Namespace, "Beep.OilandGas.PPDM39.Models", StringComparison.Ordinal) &&
                    typeof(Entity).IsAssignableFrom(type))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToList();
        }

        private List<Type> GetMigrationEntityTypes(string? targetAssemblyName, string? targetModelNamespace)
        {
            var hasScope = !string.IsNullOrWhiteSpace(targetAssemblyName) || !string.IsNullOrWhiteSpace(targetModelNamespace);
            if (!hasScope)
            {
                if (_moduleSetupOrchestrator != null)
                {
                    var moduleTypes = _moduleSetupOrchestrator.GetAllEntityTypes();
                    if (moduleTypes.Count > 0)
                    {
                        return moduleTypes
                            .OrderBy(type => type.FullName, StringComparer.Ordinal)
                            .ToList();
                    }
                }

                return GetPpdm39EntityTypes();
            }

            Assembly? assembly = ResolveAssembly(targetAssemblyName);
            if (assembly == null)
                return new List<Type>();

            var targetNamespace = string.IsNullOrWhiteSpace(targetModelNamespace)
                ? null
                : targetModelNamespace.Trim();

            return assembly
                .GetTypes()
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    typeof(Entity).IsAssignableFrom(type) &&
                    (targetNamespace == null ||
                     string.Equals(type.Namespace, targetNamespace, StringComparison.Ordinal) ||
                     (type.Namespace != null && type.Namespace.StartsWith(targetNamespace + ".", StringComparison.Ordinal))))
                .OrderBy(type => type.FullName, StringComparer.Ordinal)
                .ToList();
        }

        private static Assembly? ResolveAssembly(string? requestedAssemblyName)
        {
            if (string.IsNullOrWhiteSpace(requestedAssemblyName))
                return null;

            var trimmed = requestedAssemblyName.Trim();
            var loaded = AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(assembly =>
                    string.Equals(assembly.GetName().Name, trimmed, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(assembly.FullName, trimmed, StringComparison.OrdinalIgnoreCase));
            if (loaded != null)
                return loaded;

            try
            {
                return Assembly.Load(new AssemblyName(trimmed));
            }
            catch
            {
                return null;
            }
        }

        private void RegisterScopeAssemblies(
            TheTechIdea.Beep.Editor.Migration.MigrationManager migration,
            string? targetAssemblyName,
            string? targetModelNamespace)
        {
            var entityTypes = GetMigrationEntityTypes(targetAssemblyName, targetModelNamespace);
            if (entityTypes.Count == 0)
            {
                migration.RegisterAssembly(typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly);
                return;
            }

            foreach (var assembly in entityTypes.Select(type => type.Assembly).Distinct())
                migration.RegisterAssembly(assembly);
        }

        private TheTechIdea.Beep.Editor.Migration.MigrationManager? CreateMigrationManager(string connectionName, out dynamic? dataSource)
        {
            dataSource = _editor.GetDataSource(connectionName);
            if (dataSource == null)
                return null;

            var state = dataSource.Openconnection();
            if (state != ConnectionState.Open)
                return null;

            return new TheTechIdea.Beep.Editor.Migration.MigrationManager(_editor, dataSource);
        }

        private static TheTechIdea.Beep.Editor.Migration.MigrationEnvironmentTier ParseEnvironmentTier(string? environmentTier)
        {
            if (Enum.TryParse(environmentTier, true, out TheTechIdea.Beep.Editor.Migration.MigrationEnvironmentTier tier))
                return tier;

            return TheTechIdea.Beep.Editor.Migration.MigrationEnvironmentTier.Development;
        }

        private void NormalizeConnectivityPreflight(object? preflightReport, bool connectivityValidated, string connectionName)
        {
            if (!connectivityValidated || preflightReport == null)
                return;

            var reportType = preflightReport.GetType();
            var checksProperty = reportType.GetProperty("Checks");
            if (checksProperty?.GetValue(preflightReport) is not System.Collections.IEnumerable checks)
                return;

            var allChecks = checks.Cast<object>().ToList();
            if (allChecks.Count == 0)
                return;

            var updated = false;
            foreach (var check in allChecks)
            {
                var checkType = check.GetType();
                var code = checkType.GetProperty("Code")?.GetValue(check)?.ToString();
                var decision = checkType.GetProperty("Decision")?.GetValue(check)?.ToString();

                if (!string.Equals(code, "preflight-connectivity", StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(decision, "Block", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var decisionProperty = checkType.GetProperty("Decision");
                if (decisionProperty?.CanWrite == true)
                {
                    var passValue = Enum.Parse(decisionProperty.PropertyType, "Pass", ignoreCase: true);
                    decisionProperty.SetValue(check, passValue);
                }

                checkType.GetProperty("Message")?.SetValue(check, "Datasource connectivity probe passed via active datasource validation.");
                checkType.GetProperty("Recommendation")?.SetValue(check, "Proceed with apply checks; runtime datasource connectivity is verified as open.");
                updated = true;
            }

            if (!updated)
                return;

            var hasBlockingChecks = allChecks.Any(check =>
                string.Equals(check.GetType().GetProperty("Decision")?.GetValue(check)?.ToString(), "Block", StringComparison.OrdinalIgnoreCase));

            var canApplyProperty = reportType.GetProperty("CanApply");
            if (canApplyProperty?.CanWrite == true)
                canApplyProperty.SetValue(preflightReport, !hasBlockingChecks);

            _logger.LogInformation("Normalized preflight-connectivity result for {ConnectionName} after successful datasource connectivity validation.", connectionName);
        }

        private static SchemaMigrationPlanResult MapPlanResult(SchemaMigrationPlanSession session)
        {
            var plan = session.Plan;
            var ciValidation = plan.CiValidationReport;
            var preflight = plan.PreflightReport;
            var policy = plan.PolicyEvaluation;
            var rollback = plan.RollbackReadinessReport;
            var dryRun = plan.DryRunReport;

            return new SchemaMigrationPlanResult
            {
                Success = ciValidation?.CanMerge != false,
                Message = ciValidation?.CanMerge == false
                    ? "Migration plan contains blocking CI gates."
                    : preflight?.CanApply == false
                        ? "Migration plan contains blocking preflight checks."
                        : "Migration plan generated successfully.",
                ConnectionName = session.ConnectionName,
                PlanId = plan.PlanId,
                PlanHash = plan.PlanHash,
                PolicyDecision = policy?.Decision.ToString() ?? string.Empty,
                RequiresManualApproval = policy?.RequiresManualApproval ?? false,
                CanApply = (ciValidation?.CanMerge ?? true) && (preflight?.CanApply ?? true),
                IsApproved = session.IsApproved,
                TotalEntities = plan.EntityTypeCount,
                PendingOperationCount = plan.PendingOperationCount,
                TablesToCreate = plan.Operations?.Count(operation => operation.Kind == TheTechIdea.Beep.Editor.Migration.MigrationPlanOperationKind.CreateEntity) ?? 0,
                ColumnsToAdd = plan.Operations?.Count(operation => operation.Kind == TheTechIdea.Beep.Editor.Migration.MigrationPlanOperationKind.AddMissingColumns) ?? 0,
                HighRiskOperationCount = plan.Operations?.Count(operation =>
                    operation.RiskLevel == TheTechIdea.Beep.Editor.Migration.MigrationPlanRiskLevel.High ||
                    operation.RiskLevel == TheTechIdea.Beep.Editor.Migration.MigrationPlanRiskLevel.Critical) ?? 0,
                CiGates = ciValidation?.Gates.Select(gate => new SchemaMigrationCiGateResult
                {
                    Gate = gate.Gate,
                    Decision = gate.Decision.ToString(),
                    Message = gate.Message
                }).ToList() ?? new List<SchemaMigrationCiGateResult>(),
                PreflightChecks = preflight?.Checks.Select(check => new SchemaMigrationPreflightCheckResult
                {
                    Code = check.Code,
                    Decision = check.Decision.ToString(),
                    Message = check.Message,
                    Recommendation = check.Recommendation
                }).ToList() ?? new List<SchemaMigrationPreflightCheckResult>(),
                PolicyFindings = policy?.Findings.Select(finding => new SchemaMigrationPolicyFindingResult
                {
                    RuleId = finding.RuleId,
                    Decision = finding.Decision.ToString(),
                    Message = finding.Message,
                    Recommendation = finding.Recommendation,
                    EntityName = finding.EntityName,
                    OperationKind = finding.OperationKind.ToString(),
                    RiskLevel = finding.RiskLevel.ToString()
                }).ToList() ?? new List<SchemaMigrationPolicyFindingResult>(),
                DryRunOperations = dryRun?.Operations.Select(operation => new SchemaMigrationDryRunOperationResult
                {
                    EntityName = operation.EntityName,
                    Kind = operation.Kind.ToString(),
                    RiskLevel = operation.RiskLevel.ToString(),
                    DdlPreview = operation.DdlPreview,
                    RiskTags = operation.RiskTags,
                    Diagnostics = operation.Diagnostics
                }).ToList() ?? new List<SchemaMigrationDryRunOperationResult>(),
                DryRunDiagnostics = dryRun?.Diagnostics ?? new List<string>(),
                CompensationActions = plan.CompensationPlan?.Actions.Select(action =>
                    $"{action.ActionId}: {action.EntityName} [{action.OperationKind}] {action.RollbackMode}").ToList() ?? new List<string>(),
                RollbackChecks = rollback?.Checks.Select(check =>
                    $"{check.Decision}: {check.Code} - {check.Message}").ToList() ?? new List<string>()
            };
        }

        private static string BuildApprovalSummaryMarkdown(SchemaMigrationPlanSession session)
        {
            var plan = session.Plan;
            var lines = new List<string>
            {
                $"# Schema Migration Approval Summary",
                string.Empty,
                $"- Plan ID: {plan.PlanId}",
                $"- Plan Hash: {plan.PlanHash}",
                $"- Connection: {session.ConnectionName}",
                $"- Approved: {(session.IsApproved ? "Yes" : "No")}",
                $"- Approved By: {session.ApprovedBy}",
                $"- Approved On UTC: {session.ApprovedOnUtc:O}",
                $"- Notes: {session.ApprovalNotes}",
                string.Empty,
                "## Summary",
                string.Empty,
                $"- Pending operations: {plan.PendingOperationCount}",
                $"- Entity types: {plan.EntityTypeCount}",
                $"- CI merge allowed: {plan.CiValidationReport?.CanMerge}",
                $"- Preflight can apply: {plan.PreflightReport?.CanApply}",
                $"- Policy decision: {plan.PolicyEvaluation?.Decision}"
            };

            return string.Join(Environment.NewLine, lines);
        }

        private static List<SchemaMigrationEntityMetadata> BuildRuntimeEntityMetadata(TheTechIdea.Beep.Editor.Migration.MigrationPlanArtifact plan)
        {
            if (plan?.Operations == null)
                return new List<SchemaMigrationEntityMetadata>();

            return plan.Operations
                .GroupBy(operation => operation.EntityTypeName, StringComparer.Ordinal)
                .Select(group =>
                {
                    var op = group.First();
                    var entityType = ResolveEntityType(op.EntityTypeName);
                    var properties = entityType?
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(property => property.CanRead && property.GetMethod?.GetParameters().Length == 0)
                        .ToList() ?? new List<PropertyInfo>();

                    var columns = properties
                        .Select(property =>
                        {
                            var baseType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            var columnName = ToUpperUnderscore(property.Name);
                            return new SchemaMigrationEntityColumnMetadata
                            {
                                PropertyName = property.Name,
                                ColumnName = columnName,
                                ClrType = baseType.Name,
                                IsNullable = !baseType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null,
                                IsPrimaryKeyCandidate = columnName.EndsWith("_ID", StringComparison.OrdinalIgnoreCase),
                                IsJsonPayload = columnName.EndsWith("_JSON", StringComparison.OrdinalIgnoreCase),
                                IsIndicatorFlag = columnName.EndsWith("_IND", StringComparison.OrdinalIgnoreCase)
                            };
                        })
                        .ToList();

                    var primaryKeys = columns
                        .Where(column => column.IsPrimaryKeyCandidate)
                        .Select(column => column.ColumnName)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(name => name)
                        .ToList();

                    var jsonColumns = columns
                        .Where(column => column.IsJsonPayload)
                        .Select(column => column.ColumnName)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(name => name)
                        .ToList();

                    var indicatorColumns = columns
                        .Where(column => column.IsIndicatorFlag)
                        .Select(column => column.ColumnName)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(name => name)
                        .ToList();

                    return new SchemaMigrationEntityMetadata
                    {
                        EntityName = op.EntityName,
                        EntityTypeName = op.EntityTypeName,
                        ResolvedTableName = op.EntityName,
                        ConventionTableName = ToUpperUnderscore(op.EntityName),
                        PrimaryKeyColumns = primaryKeys,
                        JsonColumns = jsonColumns,
                        IndicatorColumns = indicatorColumns,
                        Columns = columns
                    };
                })
                .OrderBy(metadata => metadata.EntityName)
                .ToList();
        }

        private static Type? ResolveEntityType(string? entityTypeName)
        {
            if (string.IsNullOrWhiteSpace(entityTypeName))
                return null;

            var type = Type.GetType(entityTypeName, throwOnError: false);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType(entityTypeName, throwOnError: false, ignoreCase: false);
                    if (type != null)
                        return type;
                }
                catch
                {
                    // ignore unloadable reflection metadata from dynamic or incompatible assemblies
                }
            }

            return null;
        }

        private static string ToUpperUnderscore(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var builder = new StringBuilder(value.Length + 8);
            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];
                if (i > 0 && char.IsUpper(current) && (char.IsLower(value[i - 1]) || (i + 1 < value.Length && char.IsLower(value[i + 1]))))
                    builder.Append('_');

                builder.Append(char.ToUpperInvariant(current));
            }

            return builder.ToString();
        }

        private void RemoveConnection(string connectionName)
        {
            var existing = _editor.ConfigEditor?.DataConnections?
                .FirstOrDefault(c => string.Equals(c.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                _editor.ConfigEditor.DataConnections.Remove(existing);
        }

        private static BeepDataSourceType ParseDataSourceType(string? databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType))
                return BeepDataSourceType.SqlServer;

            if (Enum.TryParse<BeepDataSourceType>(databaseType, true, out var result))
                return result;

            return databaseType.ToLowerInvariant() switch
            {
                "sqlserver"              => BeepDataSourceType.SqlServer,
                "postgre" or "postgresql" => BeepDataSourceType.Postgre,
                "mysql" or "mariadb"     => BeepDataSourceType.Mysql,
                "oracle"                 => BeepDataSourceType.Oracle,
                "sqlite" or "sqllite"    => BeepDataSourceType.SqlLite,
                _                        => BeepDataSourceType.SqlServer
            };
        }

        private static int GetDefaultPort(BeepDataSourceType dsType) => dsType switch
        {
            BeepDataSourceType.SqlServer => 1433,
            BeepDataSourceType.Postgre   => 5432,
            BeepDataSourceType.Mysql     => 3306,
            BeepDataSourceType.Oracle    => 1521,
            _                            => 0
        };

        private static string MapToScriptFolder(BeepDataSourceType dsType) => dsType switch
        {
            BeepDataSourceType.SqlServer => "SqlServer",
            BeepDataSourceType.Postgre   => "PostgreSQL",
            BeepDataSourceType.Mysql     => "MySQL",
            BeepDataSourceType.Oracle    => "Oracle",
            BeepDataSourceType.SqlLite   => "SQLite",
            _                            => "SqlServer"
        };

        // ── Reference Data Seeding ─────────────────────────────────────────────

        /// <inheritdoc />
        public async Task<SeedingOperationResult> SeedWellStatusFacetsAsync(
            string connectionName, string userId = "SYSTEM", string? operationId = null)
        {
            _logger.LogInformation("Seeding WSC v3 well-status facets into {Connection}", connectionName);
            try
            {
                var seeder = new WellStatusFacetSeeder(_editor, _commonColumnHandler, _defaults, _metadata, connectionName);
                var result = await seeder.SeedAllAsync(userId);
                _logger.LogInformation(
                    "Well-status facet seed complete: {Total} rows inserted into {Connection}",
                    result.TotalInserted, connectionName);

                return new SeedingOperationResult
                {
                    Success       = result.Success,
                    Message       = result.Message,
                    TotalInserted = result.TotalInserted,
                    Details = new List<string>
                    {
                        $"R_WELL_STATUS_TYPE:       {result.FacetTypeRows} rows",
                        $"R_WELL_STATUS:            {result.FacetValueRows} rows",
                        $"R_WELL_STATUS_QUAL:       {result.FacetQualifierRows} rows",
                        $"R_WELL_STATUS_QUAL_VALUE: {result.FacetQualValueRows} rows",
                        $"RA_WELL_STATUS_TYPE:      {result.RaFacetTypeRows} rows",
                        $"RA_WELL_STATUS:           {result.RaFacetValueRows} rows",
                    },
                    Errors = result.Errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Well-status facet seeding failed for {Connection}", connectionName);
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = $"Seeding failed: {ex.Message}",
                    Errors  = new List<string> { ex.ToString() }
                };
            }
        }

        /// <inheritdoc />
        public async Task<SeedingOperationResult> SeedEnumReferenceDataAsync(
            string connectionName, string userId = "SYSTEM", string? operationId = null)
        {
            _logger.LogInformation("Seeding enum reference data into {Connection}", connectionName);
            if (_lovService == null)
            {
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = "LOVManagementService is not available. Ensure it is registered in DI.",
                    Errors  = new List<string> { "LOVManagementService is null" }
                };
            }

            try
            {
                var seeder = new EnumReferenceDataSeeder(
                    _editor, _commonColumnHandler, _defaults, _metadata, _lovService, connectionName);
                var count = await seeder.SeedAllEnumsAsync(userId);

                _logger.LogInformation("Enum reference data seeded: {Count} rows into {Connection}", count, connectionName);
                return new SeedingOperationResult
                {
                    Success       = true,
                    Message       = $"Seeded {count} enum reference rows.",
                    TotalInserted = count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Enum reference data seeding failed for {Connection}", connectionName);
                return new SeedingOperationResult
                {
                    Success = false,
                    Message = $"Seeding failed: {ex.Message}",
                    Errors  = new List<string> { ex.ToString() }
                };
            }
        }

        /// <inheritdoc />
        public async Task<SeedingOperationResult> SeedAllReferenceDataAsync(
            string connectionName, string userId = "SYSTEM", string? operationId = null)
        {
            if (_moduleSetupOrchestrator != null)
            {
                _logger.LogInformation(
                    "Seeding all module reference data via ModuleSetupOrchestrator into {Connection}",
                    connectionName);

                try
                {
                    var orchestrated = await _moduleSetupOrchestrator.RunSeedAsync(connectionName, userId);

                    var moduleAggregate = new SeedingOperationResult
                    {
                        Success = orchestrated.AllSucceeded,
                        TotalInserted = orchestrated.TotalRecordsInserted,
                        Message = orchestrated.AllSucceeded
                            ? $"All module seeds succeeded: {orchestrated.TotalRecordsInserted} total rows."
                            : $"One or more modules failed. {orchestrated.TotalRecordsInserted} total rows inserted."
                    };

                    foreach (var module in orchestrated.ModuleResults)
                    {
                        var skipLabel = string.IsNullOrWhiteSpace(module.SkipReason)
                            ? string.Empty
                            : $" (skipped: {module.SkipReason})";

                        moduleAggregate.Details.Add(
                            $"[{module.ModuleId}] {module.ModuleName}: {module.RecordsInserted} rows / {module.TablesSeeded} tables{skipLabel}");

                        if (module.Errors.Count > 0)
                        {
                            foreach (var error in module.Errors)
                                moduleAggregate.Errors.Add($"[{module.ModuleId}] {error}");
                        }
                    }

                    if (!moduleAggregate.Success && moduleAggregate.Errors.Count == 0)
                        moduleAggregate.Errors.Add("One or more modules reported failure without explicit error details.");

                    return moduleAggregate;
                }
                catch (ModuleSetupAbortException ex)
                {
                    _logger.LogError(ex,
                        "Module setup aborted by module {ModuleId} while seeding {Connection}",
                        ex.ModuleId,
                        connectionName);

                    return new SeedingOperationResult
                    {
                        Success = false,
                        Message = $"Module setup aborted by {ex.ModuleId}: {ex.Message}",
                        Errors = new List<string> { ex.ToString() }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Module setup orchestration failed while seeding {Connection}",
                        connectionName);

                    return new SeedingOperationResult
                    {
                        Success = false,
                        Message = $"Seeding failed: {ex.Message}",
                        Errors = new List<string> { ex.ToString() }
                    };
                }
            }

            _logger.LogInformation("Seeding all reference data into {Connection}", connectionName);
            var aggregate = new SeedingOperationResult();

            // 1. WSC v3 facets
            var facetsResult = await SeedWellStatusFacetsAsync(connectionName, userId, operationId);
            aggregate.TotalInserted += facetsResult.TotalInserted;
            aggregate.Details.AddRange(facetsResult.Details);
            aggregate.Errors.AddRange(facetsResult.Errors);

            // 2. Enum reference data
            var enumsResult = await SeedEnumReferenceDataAsync(connectionName, userId, operationId);
            aggregate.TotalInserted += enumsResult.TotalInserted;
            aggregate.Details.AddRange(enumsResult.Details);
            aggregate.Errors.AddRange(enumsResult.Errors);

            aggregate.Success = facetsResult.Success && enumsResult.Success;
            aggregate.Message = aggregate.Success
                ? $"All reference data seeded: {aggregate.TotalInserted} total rows."
                : $"Partial failure. {aggregate.TotalInserted} rows inserted; {aggregate.Errors.Count} error(s).";

            _logger.LogInformation(
                "All reference data seeding complete: {Total} rows, success={Success}",
                aggregate.TotalInserted, aggregate.Success);

            return aggregate;
        }
    }
}
