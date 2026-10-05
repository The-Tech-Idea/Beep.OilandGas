using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for creating and managing demo SQLite databases
    /// </summary>
    public class DemoDatabaseService : IDemoDatabaseService
    {
        private readonly DemoDatabaseConfig _config;
        private readonly DemoDatabaseRepository _repository;
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IPPDM39SetupService _setupService;
        private readonly Beep.OilandGas.PPDM39.DataManagement.SeedData.PPDMReferenceDataSeeder? _referenceDataSeeder;
        private readonly ILogger<DemoDatabaseService> _logger;
        private readonly IFailureReporter _failures;

        public DemoDatabaseService(
            IOptions<DemoDatabaseConfig> config,
            DemoDatabaseRepository repository,
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IPPDM39SetupService setupService,
            IFailureReporter failures,
            Beep.OilandGas.PPDM39.DataManagement.SeedData.PPDMReferenceDataSeeder? referenceDataSeeder = null,
            ILogger<DemoDatabaseService>? logger = null)
        {
            _failures = failures ?? throw new ArgumentNullException(nameof(failures));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _setupService = setupService ?? throw new ArgumentNullException(nameof(setupService));
            _referenceDataSeeder = referenceDataSeeder;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a demo SQLite database for a user
        /// </summary>
        public async Task<CreateDemoDatabaseResponse> CreateDemoDatabaseAsync(CreateDemoDatabaseRequest request)
        {
            if (!_config.Enabled)
            {
                return new CreateDemoDatabaseResponse
                {
                    Success = false,
                    Message = "Demo database creation is disabled"
                };
            }

            try
            {
                // Generate unique connection name and database path
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var connectionName = request.ConnectionName ?? $"demo_{request.UserId}_{timestamp}";
                var databaseFileName = $"{connectionName}.db";
                var databasePath = Path.Combine(_config.StoragePath, databaseFileName);

                // Ensure storage directory exists
                if (!Directory.Exists(_config.StoragePath))
                {
                    Directory.CreateDirectory(_config.StoragePath);
                }

                // Create SQLite connection configuration
                var connectionConfig = new ConnectionConfig
                {
                    ConnectionName = connectionName,
                    DatabaseType = "SQLite",
                    Host = "localhost",
                    Port = 0,
                    Database = databasePath,
                    Username = null,
                    Password = null
                };

                // Create SQLite database file
                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }

                // Register connection in IDMEEditor
                var saveResult = _setupService.SaveConnection(connectionConfig, false, false);
                if (!saveResult.Success)
                {
                    return new CreateDemoDatabaseResponse
                    {
                        Success = false,
                        Message = "Failed to register connection",
                        ErrorDetails = saveResult.Message
                    };
                }

                var planResult = await _setupService.PlanSchemaMigrationAsync(new SchemaMigrationPlanRequest
                {
                    ConnectionName = connectionName,
                    BackupConfirmed = true,
                    RestoreTestEvidenceProvided = true,
                    RestoreTestEvidence = $"Demo database bootstrap for user {request.UserId}"
                });

        if (!planResult.Success)
                {
                    // Clean up on failure
                    TryCleanupFailedDatabase(connectionName, databasePath);
                    return new CreateDemoDatabaseResponse
                    {
                        Success = false,
                        Message = "Failed to create database schema",
                        ErrorDetails = planResult.Message,
                        SchemaPlanId = planResult.PlanId
                    };
                }

                var approvalResult = await _setupService.ApproveSchemaMigrationPlanAsync(new SchemaMigrationApprovalRequest
                {
                    PlanId = planResult.PlanId,
                    ApprovedBy = $"demo-service:{request.UserId}",
                    Notes = "Auto-approved for demo database bootstrap."
                });

                if (!approvalResult.Success)
                {
                    TryCleanupFailedDatabase(connectionName, databasePath);

                    return new CreateDemoDatabaseResponse
                    {
                        Success = false,
                        Message = "Failed to approve database schema plan",
                        ErrorDetails = approvalResult.Message,
                        SchemaPlanId = planResult.PlanId
                    };
                }

                var startResult = await _setupService.StartSchemaMigrationExecutionAsync(new SchemaMigrationExecuteRequest
                {
                    PlanId = planResult.PlanId,
                    ExecutedBy = $"demo-service:{request.UserId}"
                });

                if (!startResult.Success || string.IsNullOrWhiteSpace(startResult.OperationId))
                {
                    TryCleanupFailedDatabase(connectionName, databasePath);

                    return new CreateDemoDatabaseResponse
                    {
                        Success = false,
                        Message = "Failed to start database schema creation",
                        ErrorDetails = startResult.Message,
                        SchemaPlanId = planResult.PlanId,
                        SchemaExecutionToken = startResult.OperationId
                    };
                }

                SchemaMigrationProgressResult? schemaProgress = null;
                const int maxSchemaPollAttempts = 900;

                for (var attempt = 0; attempt < maxSchemaPollAttempts; attempt++)
                {
                    await Task.Delay(1000);
                    schemaProgress = await _setupService.GetSchemaMigrationProgressAsync(startResult.OperationId);

                    if (!schemaProgress.Success)
                    {
                        continue;
                    }

                    if (schemaProgress.IsCompleted || schemaProgress.HasFailed)
                    {
                        break;
                    }
                }

                if (schemaProgress == null || !schemaProgress.Success)
                {
                    TryCleanupFailedDatabase(connectionName, databasePath);

                    return new CreateDemoDatabaseResponse
                    {
                        Success = false,
                        Message = "Database schema creation did not report completion",
                        ErrorDetails = "Schema migration progress could not be confirmed.",
                        SchemaPlanId = planResult.PlanId,
                        SchemaExecutionToken = startResult.OperationId
                    };
                }

                if (schemaProgress.HasFailed)
                {
                    TryCleanupFailedDatabase(connectionName, databasePath);

                    return new CreateDemoDatabaseResponse
                    {
                        Success = false,
                        Message = "Failed to create database schema",
                        ErrorDetails = schemaProgress.FailureReason,
                        SchemaPlanId = planResult.PlanId,
                        SchemaExecutionToken = startResult.OperationId
                    };
                }

                var artifactsResult = await _setupService.GetSchemaMigrationArtifactsAsync(planResult.PlanId);
                if (!artifactsResult.Success)
                {
                    _logger.LogWarning("Schema artifacts were not available for demo database {ConnectionName}: {Message}", connectionName, artifactsResult.Message);
                }

                // Seed data if requested. A seeding problem does not undo the database, but it is said: the response
                // used to read "created successfully" whatever the seeding did (OILGAS-CATCH-01).
                var seedingProblems = request.SeedDataOption != "none"
                    ? await SeedDemoDatabaseAsync(connectionName, request.SeedDataOption)
                    : new List<string>();

                // Calculate expiry date
                var createdDate = DateTime.UtcNow;
                var expiryDate = createdDate.AddDays(_config.RetentionDays);

                // Store metadata
                var metadata = new DemoDatabaseMetadata
                {
                    ConnectionName = connectionName,
                    UserId = request.UserId,
                    DatabasePath = databasePath,
                    SeedDataOption = request.SeedDataOption,
                    CreatedDate = createdDate,
                    ExpiryDate = expiryDate
                };

                await _repository.AddAsync(metadata);

                _logger.LogInformation("Created demo database {ConnectionName} for user {UserId}", connectionName, request.UserId);

                return new CreateDemoDatabaseResponse
                {
                    Success = true,
                    ConnectionName = connectionName,
                    DatabasePath = databasePath,
                    Message = seedingProblems.Count == 0
                        ? "Demo database created successfully"
                        : "Demo database created, but its sample data was not completely seeded",
                    ErrorDetails = seedingProblems.Count == 0 ? null : string.Join(Environment.NewLine, seedingProblems),
                    CreatedDate = createdDate,
                    ExpiryDate = expiryDate,
                    SchemaPlanId = planResult.PlanId,
                    SchemaExecutionToken = startResult.OperationId
                };
            }
            // Broad: creating a demo database answers every failure of its steps — the file system, the configuration
            // store, the metadata store — in its response, which carried the exception's text.
            catch (Exception ex) when (ex is not RefusalException)
            {
                return new CreateDemoDatabaseResponse
                {
                    Success = false,
                    Message = "Failed to create demo database",
                    ErrorDetails = ReportedFailure.Sentence(_failures, ex,
                        $"creating a demo database for user {request.UserId}", "The demo database was not created.")
                };
            }
        }

        /// <summary>
        /// Seeds the demo database with data based on the option, and answers what did not seed — each stage's own
        /// problems, or a failure's sentence with its reference. Empty when every stage seeded.
        /// </summary>
        /// <remarks>
        /// A seeding failure does not undo the database, which is usable without its sample data. It used to be caught,
        /// logged and dropped — "seeding failure shouldn't fail database creation" — and each stage's own problems only
        /// logged as warnings, so the caller was told the demo database was created successfully either way
        /// (OILGAS-CATCH-01).
        /// </remarks>
        private async Task<List<string>> SeedDemoDatabaseAsync(string connectionName, string seedOption)
        {
            var problems = new List<string>();
            try
            {
                // ── Required stage 1: well-status facets ─────────────────────────────
                var wellStatusSeeder = new Beep.OilandGas.PPDM39.DataManagement.SeedData.WellStatusFacetSeeder(
                    _editor, _commonColumnHandler, _defaults, _metadata, connectionName);
                var facetResult = await wellStatusSeeder.SeedAllAsync("SYSTEM");
                if (!facetResult.Success)
                    problems.Add($"Well-status facets: {facetResult.Message}");
                else
                    _logger.LogInformation("Well-status facet seeding complete for {ConnectionName}", connectionName);

                // ── Required stage 2: reference data ─────────────────────────────────
                if (_referenceDataSeeder != null)
                {
                    var seedResult = await _referenceDataSeeder.SeedPPDMReferenceTablesAsync(
                        connectionName,
                        null, // All tables
                        true, // Skip existing
                        "SYSTEM");

                    if (!seedResult.Success)
                        problems.Add($"Reference data: {seedResult.Message}");
                }
                else
                {
                    problems.Add("Reference data: the reference data seeder is not available on this host, so no reference data was seeded.");
                }

                // Seed additional data based on option
                if ((seedOption == "reference-sample" || seedOption == "full-demo") && _referenceDataSeeder != null)
                {
                    _logger.LogInformation("Seeding sample entities for demo database {ConnectionName}", connectionName);
                    var csvSeeder = new Beep.OilandGas.PPDM39.DataManagement.SeedData.PPDMCSVSeeder(
                        _editor, _commonColumnHandler, _defaults, _metadata, connectionName);
                    var demoSeeder = new Beep.OilandGas.PPDM39.DataManagement.SeedData.PPDMDemoDataSeeder(
                        _editor, _commonColumnHandler, _defaults, _metadata,
                        _referenceDataSeeder, csvSeeder, connectionName, null);
                    var sampleResult = await demoSeeder.SeedFullDemoDatasetAsync("SYSTEM");
                    if (!sampleResult.Success)
                        problems.Add($"Sample data: {sampleResult.Message}");
                    else
                        _logger.LogInformation("Sample entity seeding complete for {ConnectionName}: {Records} records", connectionName, sampleResult.RecordsInserted);
                }
            }
            // Broad: the seeders write many tables through the driver, whatever it throws; the database stays (see the
            // remarks) and the failure is reported and said in the answer.
            catch (Exception ex) when (ex is not RefusalException)
            {
                problems.Add(ReportedFailure.Sentence(_failures, ex,
                    $"seeding demo database '{connectionName}' ({seedOption})",
                    "The demo database's data was not completely seeded."));
            }

            return problems;
        }

        /// <summary>
        /// Delete a demo database
        /// </summary>
        public async Task<DeleteDemoDatabaseResponse> DeleteDemoDatabaseAsync(string connectionName)
        {
            try
            {
                var metadata = _repository.GetByConnectionName(connectionName);
                if (metadata == null)
                {
                    return new DeleteDemoDatabaseResponse
                    {
                        Success = false,
                        Message = $"Demo database '{connectionName}' not found"
                    };
                }

                // Close connection first so the file handle is released before deletion
                try
                {
                    _editor.ConfigEditor.RemoveConnByName(connectionName);
                }
                // Broad: the configuration store throws its own types. The file and the record are still removed —
                // if the handle stays open, deleting the file fails below and the deletion is answered as failed.
                catch (Exception ex) when (ex is not RefusalException)
                {
                    _failures.ReportHandled(ex,
                        $"unregistering the connection of demo database '{connectionName}' before deleting it",
                        "the database file and its record are still deleted; the connection may stay registered until the next restart",
                        FailureSeverity.Degraded);
                }

                // Delete database file
                if (File.Exists(metadata.DatabasePath))
                {
                    File.Delete(metadata.DatabasePath);
                }

                // Remove metadata
                await _repository.DeleteAsync(connectionName);

                _logger.LogInformation("Deleted demo database {ConnectionName}", connectionName);

                return new DeleteDemoDatabaseResponse
                {
                    Success = true,
                    Message = $"Demo database '{connectionName}' deleted successfully"
                };
            }
            // Broad: deleting answers every failure of removing the file and the record in its response, which carried
            // the exception's text.
            catch (Exception ex) when (ex is not RefusalException)
            {
                return new DeleteDemoDatabaseResponse
                {
                    Success = false,
                    Message = "Failed to delete demo database",
                    ErrorDetails = ReportedFailure.Sentence(_failures, ex,
                        $"deleting demo database '{connectionName}'", "The demo database was not deleted.")
                };
            }
        }

        /// <summary>
        /// Cleanup expired demo databases
        /// </summary>
        public async Task<CleanupDemoDatabasesResponse> CleanupExpiredDatabasesAsync()
        {
            try
            {
                var expiredDatabases = _repository.GetExpired();
                var deletedCount = 0;
                var deletedNames = new List<string>();
                var notDeleted = new List<string>();

                foreach (var metadata in expiredDatabases)
                {
                    var result = await DeleteDemoDatabaseAsync(metadata.ConnectionName);
                    if (result.Success)
                    {
                        deletedCount++;
                        deletedNames.Add(metadata.ConnectionName);
                    }
                    else
                    {
                        // Each failure is reported by the deletion itself; the cleanup says which were left.
                        notDeleted.Add($"{metadata.ConnectionName}: {result.ErrorDetails ?? result.Message}");
                    }
                }

                _logger.LogInformation("Cleaned up {Count} expired demo databases; {Left} could not be deleted",
                    deletedCount, notDeleted.Count);

                return new CleanupDemoDatabasesResponse
                {
                    Success = notDeleted.Count == 0,
                    DeletedCount = deletedCount,
                    DeletedDatabases = deletedNames,
                    Message = notDeleted.Count == 0
                        ? $"Cleaned up {deletedCount} expired demo databases"
                        : $"Cleaned up {deletedCount} expired demo databases; {notDeleted.Count} could not be deleted",
                    ErrorDetails = notDeleted.Count == 0 ? null : string.Join(Environment.NewLine, notDeleted)
                };
            }
            // Broad: reading the expired list answers every failure of the metadata store in the response, which
            // carried the exception's text.
            catch (Exception ex) when (ex is not RefusalException)
            {
                return new CleanupDemoDatabasesResponse
                {
                    Success = false,
                    Message = "Failed to cleanup expired demo databases",
                    ErrorDetails = ReportedFailure.Sentence(_failures, ex,
                        "cleaning up expired demo databases", "Expired demo databases were not cleaned up.")
                };
            }
        }

        /// <summary>
        /// Get demo databases for a user
        /// </summary>
        public List<DemoDatabaseMetadata> GetUserDemoDatabases(string userId)
        {
            return _repository.GetByUserId(userId);
        }

        /// <summary>
        /// Get all demo databases (admin only)
        /// </summary>
        public List<DemoDatabaseMetadata> GetAllDemoDatabases()
        {
            return _repository.GetAll();
        }

        /// <summary>
        /// Returns a typed status snapshot for a demo database covering schema completeness,
        /// seeding outcomes, and retention state.
        /// </summary>
        public Task<DemoDatabaseStatusResult> GetDemoDatabaseStatusAsync(string connectionName)
        {
            var metadata = _repository.GetByConnectionName(connectionName);
            if (metadata == null)
            {
                return Task.FromResult(new DemoDatabaseStatusResult
                {
                    Exists = false,
                    ConnectionName = connectionName
                });
            }

            return Task.FromResult(new DemoDatabaseStatusResult
            {
                Exists = true,
                ConnectionName = connectionName,
                SchemaComplete = true, // Metadata presence implies schema creation succeeded
                SeedingComplete = !string.IsNullOrEmpty(metadata.SeedDataOption) && metadata.SeedDataOption != "none",
                SeedDataOption = metadata.SeedDataOption,
                CreatedDate = metadata.CreatedDate,
                ExpiryDate = metadata.ExpiryDate,
                IsExpired = metadata.IsExpired
            });
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Closes and unregisters a partially-created connection then removes the database file.
        /// Called from all failure paths in <see cref="CreateDemoDatabaseAsync"/> so that
        /// the file handle is always released before the file is deleted.
        /// </summary>
        private void TryCleanupFailedDatabase(string connectionName, string databasePath)
        {
            // Best effort, on a path that is already answering a failure: what cannot be cleaned up is reported, and the
            // caller's failure is still the answer. Broad because the configuration store throws its own types.
            try { _editor.ConfigEditor.RemoveConnByName(connectionName); }
            catch (Exception ex) when (ex is not RefusalException)
            {
                _failures.ReportHandled(ex,
                    $"unregistering the connection of a demo database that failed to create ('{connectionName}')",
                    "the connection may stay registered until the next restart; the creation is still answered as failed",
                    FailureSeverity.Degraded);
            }

            try
            {
                if (File.Exists(databasePath))
                    File.Delete(databasePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                _failures.ReportHandled(ex,
                    $"deleting the file of a demo database that failed to create ('{connectionName}')",
                    "the partial database file stays on disk; the creation is still answered as failed",
                    FailureSeverity.Degraded);
            }
        }
    }
}
