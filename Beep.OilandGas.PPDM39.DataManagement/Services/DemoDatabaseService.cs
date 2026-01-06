using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.DataManagement.Data;
using Beep.OilandGas.Models.DTOs.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Models.DatabaseCreation;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Service for creating and managing demo SQLite databases
    /// </summary>
    public class DemoDatabaseService
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

        public DemoDatabaseService(
            IOptions<DemoDatabaseConfig> config,
            DemoDatabaseRepository repository,
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            IPPDM39SetupService setupService,
            Beep.OilandGas.PPDM39.DataManagement.SeedData.PPDMReferenceDataSeeder? referenceDataSeeder = null,
            ILogger<DemoDatabaseService>? logger = null)
        {
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

                // Get all available scripts for SQLite
                var availableScripts = _setupService.GetAvailableScripts("SQLite");
                var names = availableScripts
                    .OrderBy(s => s.ExecutionOrder)
                    .Select(s => s.Name?.ToString() ?? string.Empty)
                    .ToList();
                
                // Create database schema using setup service
                var scriptsResult = await _setupService.ExecuteAllScriptsAsync(connectionConfig, names);
                if (!scriptsResult.AllSucceeded)
                {
                    // Clean up on failure
                    if (File.Exists(databasePath))
                    {
                        File.Delete(databasePath);
                    }
                    return new CreateDemoDatabaseResponse
                    {
                        Success = false,
                        Message = "Failed to create database schema",
                        ErrorDetails = $"Only {scriptsResult.SuccessfulScripts} of {scriptsResult.TotalScripts} scripts succeeded"
                    };
                }

                // Seed data if requested
                if (request.SeedDataOption != "none")
                {
                    await SeedDemoDatabaseAsync(connectionName, request.SeedDataOption);
                }

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
                    Message = "Demo database created successfully",
                    CreatedDate = createdDate,
                    ExpiryDate = expiryDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating demo database for user {UserId}", request.UserId);
                return new CreateDemoDatabaseResponse
                {
                    Success = false,
                    Message = "Failed to create demo database",
                    ErrorDetails = ex.Message
                };
            }
        }

        /// <summary>
        /// Seed demo database with data based on option
        /// </summary>
        private async Task SeedDemoDatabaseAsync(string connectionName, string seedOption)
        {
            try
            {
                var seedRequest = new SeedDataRequest
                {
                    ConnectionName = connectionName,
                    SkipExisting = true,
                    UserId = "SYSTEM"
                };

                // Seed reference data if seeder is available
                if (_referenceDataSeeder != null)
                {
                    var seedResult = await _referenceDataSeeder.SeedPPDMReferenceTablesAsync(
                        connectionName, 
                        null, // All tables
                        true, // Skip existing
                        "SYSTEM");
                    
                    if (!seedResult.Success)
                    {
                        _logger.LogWarning("Failed to seed reference data for demo database {ConnectionName}: {Message}", 
                            connectionName, seedResult.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Reference data seeder not available for demo database {ConnectionName}", connectionName);
                }

                // Seed additional data based on option
                if (seedOption == "reference-sample" || seedOption == "full-demo")
                {
                    // Seed sample entities (would need additional implementation)
                    _logger.LogInformation("Seeding sample entities for demo database {ConnectionName}", connectionName);
                    // TODO: Implement sample entity seeding
                }

                if (seedOption == "full-demo")
                {
                    // Seed full demo dataset
                    _logger.LogInformation("Seeding full demo dataset for demo database {ConnectionName}", connectionName);
                    // TODO: Implement full demo dataset seeding
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding demo database {ConnectionName}", connectionName);
                // Don't throw - seeding failure shouldn't fail database creation
            }
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

                // Delete database file
                if (File.Exists(metadata.DatabasePath))
                {
                    File.Delete(metadata.DatabasePath);
                }

                // Remove connection from IDMEEditor
                try
                {
                    _editor.ConfigEditor.RemoveConnByName(connectionName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error removing connection {ConnectionName} from IDMEEditor", connectionName);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting demo database {ConnectionName}", connectionName);
                return new DeleteDemoDatabaseResponse
                {
                    Success = false,
                    Message = "Failed to delete demo database",
                    ErrorDetails = ex.Message
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

                foreach (var metadata in expiredDatabases)
                {
                    var result = await DeleteDemoDatabaseAsync(metadata.ConnectionName);
                    if (result.Success)
                    {
                        deletedCount++;
                        deletedNames.Add(metadata.ConnectionName);
                    }
                }

                _logger.LogInformation("Cleaned up {Count} expired demo databases", deletedCount);

                return new CleanupDemoDatabasesResponse
                {
                    Success = true,
                    DeletedCount = deletedCount,
                    DeletedDatabases = deletedNames,
                    Message = $"Cleaned up {deletedCount} expired demo databases"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired demo databases");
                return new CleanupDemoDatabasesResponse
                {
                    Success = false,
                    Message = "Failed to cleanup expired demo databases",
                    ErrorDetails = ex.Message
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
    }
}
