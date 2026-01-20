using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Beep.OilandGas.PPDM39.DataManagement.Services
{
    /// <summary>
    /// Background service for scheduled cleanup of expired demo databases
    /// Uses a simple timer-based approach (runs daily at configured time)
    /// </summary>
    public class DemoDatabaseCleanupService : BackgroundService
    {
        private readonly DemoDatabaseConfig _config;
        private readonly DemoDatabaseService _demoDatabaseService;
        private readonly ILogger<DemoDatabaseCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Run daily

        public DemoDatabaseCleanupService(
            IOptions<DemoDatabaseConfig> config,
            DemoDatabaseService demoDatabaseService,
            ILogger<DemoDatabaseCleanupService> logger)
        {
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _demoDatabaseService = demoDatabaseService ?? throw new ArgumentNullException(nameof(demoDatabaseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_config.Enabled)
            {
                _logger.LogInformation("Demo database cleanup service is disabled");
                return;
            }

            _logger.LogInformation("Demo database cleanup service started. Will run daily to clean up expired databases.");

            // Wait a bit before first run
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting scheduled cleanup of expired demo databases");
                    var result = await _demoDatabaseService.CleanupExpiredDatabasesAsync();

                    if (result.Success)
                    {
                        _logger.LogInformation("Cleanup completed. Deleted {Count} expired demo databases", result.DeletedCount);
                        if (result.DeletedDatabases.Any())
                        {
                            _logger.LogInformation("Deleted databases: {Databases}", string.Join(", ", result.DeletedDatabases));
                        }
                    }
                    else
                    {
                        _logger.LogError("Cleanup failed: {Message}", result.Message);
                    }

                    // Wait for next cleanup cycle (24 hours)
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Demo database cleanup service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in demo database cleanup service");
                    // Wait a bit before retrying
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Demo database cleanup service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }
}
