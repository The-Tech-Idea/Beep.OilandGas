using Beep.OilandGas.Models.Data.Measurement;
using Beep.OilandGas.Models.DTOs.Measurement;
using Beep.OilandGas.ProductionAccounting.Storage;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Measurement
{
    /// <summary>
    /// Service for managing measurement operations.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class MeasurementService : IMeasurementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<MeasurementService>? _logger;
        private readonly string _connectionName;
        private const string MEASUREMENT_RECORD_TABLE = "MEASUREMENT_RECORD";

        public MeasurementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<MeasurementService>? logger = null,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
            _connectionName = connectionName ?? "PPDM39";
        }

        /// <summary>
        /// Records a manual measurement.
        /// </summary>
        public async Task<MEASUREMENT_RECORD> RecordManualMeasurementAsync(RecordManualMeasurementRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;

            // Get tank if provided (for tank gauging)
            Tank? tank = null;
            if (!string.IsNullOrEmpty(request.TankId))
            {
                tank = new Tank
                {
                    TankNumber = request.TankId,
                    Capacity = 1000m,
                    ApiGravity = request.ApiGravity
                };
            }

            // Use ManualMeasurement static class for calculation
            MeasurementRecord measurementRecord;
            if (tank != null)
            {
                measurementRecord = ManualMeasurement.PerformTankGauging(
                    tank,
                    request.GaugeHeight,
                    request.Temperature,
                    request.BswSample);
            }
            else
            {
                measurementRecord = new MeasurementRecord
                {
                    MeasurementId = Guid.NewGuid().ToString(),
                    MeasurementDateTime = DateTime.UtcNow,
                    Method = MeasurementMethod.Manual,
                    GrossVolume = 0m,
                    BSW = request.BswSample,
                    Temperature = request.Temperature,
                    ApiGravity = request.ApiGravity,
                    MeasurementDevice = "Tank Gauge",
                    Operator = request.Operator,
                    Notes = request.Notes
                };
            }

            // Convert to entity and save
            var entity = ConvertToMeasurementEntity(measurementRecord);
            entity.WELL_ID = request.WellId;
            entity.LEASE_ID = request.LeaseId;
            entity.OPERATOR = request.Operator;
            entity.NOTES = request.Notes;
            entity.VALIDATION_STATUS = "Pending";

            if (entity is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            var repo = await GetMeasurementRepositoryAsync(connName);
            await repo.InsertAsync(entity);

            _logger?.LogDebug("Recorded manual measurement {MeasurementId} for well {WellId}", entity.MEASUREMENT_RECORD_ID, request.WellId);
            return entity;
        }

        /// <summary>
        /// Records an automatic measurement.
        /// </summary>
        public async Task<MEASUREMENT_RECORD> RecordAutomaticMeasurementAsync(RecordAutomaticMeasurementRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Use AutomaticMeasurement static class for calculation
            var measurementRecord = AutomaticMeasurement.PerformAutomaticMetering(
                request.MeterReading,
                request.MeterFactor,
                request.Temperature,
                request.Bsw,
                request.ApiGravity);

            // Convert to entity and save
            var entity = ConvertToMeasurementEntity(measurementRecord);
            entity.WELL_ID = request.WellId;
            entity.LEASE_ID = request.LeaseId;
            entity.MEASUREMENT_DEVICE = request.MeasurementDevice ?? "Flow Meter";
            entity.NOTES = request.Notes;
            entity.VALIDATION_STATUS = "Pending";

            var connName = connectionName ?? _connectionName;
            if (entity is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            var repo = await GetMeasurementRepositoryAsync(connName);
            await repo.InsertAsync(entity);

            _logger?.LogDebug("Recorded automatic measurement {MeasurementId} for well {WellId}", entity.MEASUREMENT_RECORD_ID, request.WellId);
            return entity;
        }

        /// <summary>
        /// Gets a measurement by ID.
        /// </summary>
        public async Task<MEASUREMENT_RECORD?> GetMeasurementAsync(string measurementId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(measurementId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetMeasurementRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "MEASUREMENT_RECORD_ID", Operator = "=", FilterValue = measurementId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<MEASUREMENT_RECORD>().FirstOrDefault();
        }

        /// <summary>
        /// Gets measurements by well.
        /// </summary>
        public async Task<List<MEASUREMENT_RECORD>> GetMeasurementsByWellAsync(string wellId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<MEASUREMENT_RECORD>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetMeasurementRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "MEASUREMENT_DATE_TIME", Operator = ">=", FilterValue = startDate.Value });
            }
            if (endDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "MEASUREMENT_DATE_TIME", Operator = "<=", FilterValue = endDate.Value });
            }

            var results = await repo.GetAsync(filters);
            return results.Cast<MEASUREMENT_RECORD>().OrderByDescending(m => m.MEASUREMENT_DATE_TIME).ToList();
        }

        /// <summary>
        /// Gets measurements by lease.
        /// </summary>
        public async Task<List<MEASUREMENT_RECORD>> GetMeasurementsByLeaseAsync(string leaseId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(leaseId))
                return new List<MEASUREMENT_RECORD>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetMeasurementRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "MEASUREMENT_DATE_TIME", Operator = ">=", FilterValue = startDate.Value });
            }
            if (endDate.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "MEASUREMENT_DATE_TIME", Operator = "<=", FilterValue = endDate.Value });
            }

            var results = await repo.GetAsync(filters);
            return results.Cast<MEASUREMENT_RECORD>().OrderByDescending(m => m.MEASUREMENT_DATE_TIME).ToList();
        }

        /// <summary>
        /// Validates a measurement.
        /// </summary>
        public async Task<MeasurementValidationResult> ValidateMeasurementAsync(string measurementId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(measurementId))
                throw new ArgumentException("Measurement ID is required.", nameof(measurementId));

            var connName = connectionName ?? _connectionName;
            var measurement = await GetMeasurementAsync(measurementId, connName);

            if (measurement == null)
                throw new InvalidOperationException($"Measurement {measurementId} not found.");

            var result = new MeasurementValidationResult
            {
                MeasurementId = measurementId,
                IsValid = true,
                ValidationDate = DateTime.UtcNow
            };

            // Validate volume
            if (measurement.GROSS_VOLUME <= 0)
            {
                result.IsValid = false;
                result.ValidationErrors.Add("Gross volume must be greater than zero.");
            }

            // Validate BSW
            if (measurement.BSW_PERCENTAGE < 0 || measurement.BSW_PERCENTAGE > 100)
            {
                result.IsValid = false;
                result.ValidationErrors.Add("BSW percentage must be between 0 and 100.");
            }

            // Validate temperature (reasonable range)
            if (measurement.TEMPERATURE < -50 || measurement.TEMPERATURE > 200)
            {
                result.ValidationWarnings.Add($"Temperature {measurement.TEMPERATURE}°F is outside normal range.");
            }

            // Validate API gravity (reasonable range)
            if (measurement.API_GRAVITY.HasValue && (measurement.API_GRAVITY < 0 || measurement.API_GRAVITY > 100))
            {
                result.ValidationWarnings.Add($"API gravity {measurement.API_GRAVITY} is outside normal range.");
            }

            // Update validation status
            measurement.VALIDATION_STATUS = result.IsValid ? "Valid" : "Invalid";

            if (measurement is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForUpdateAsync(ppdmEntity, "SYSTEM", connName);
            }

            var repo = await GetMeasurementRepositoryAsync(connName);
            await repo.UpdateAsync(measurement);

            _logger?.LogDebug("Validated measurement {MeasurementId}: {IsValid}", measurementId, result.IsValid);
            return result;
        }

        /// <summary>
        /// Gets measurement history.
        /// </summary>
        public async Task<List<MeasurementHistory>> GetMeasurementHistoryAsync(string wellId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(wellId))
                return new List<MeasurementHistory>();

            var connName = connectionName ?? _connectionName;
            var measurements = await GetMeasurementsByWellAsync(wellId, null, null, connName);

            return measurements.Select(m => new MeasurementHistory
            {
                MeasurementId = m.MEASUREMENT_RECORD_ID ?? string.Empty,
                MeasurementDateTime = m.MEASUREMENT_DATE_TIME ?? DateTime.MinValue,
                MeasurementMethod = m.MEASUREMENT_METHOD ?? string.Empty,
                GrossVolume = m.GROSS_VOLUME ?? 0m,
                NetVolume = m.NET_VOLUME ?? 0m,
                ApiGravity = m.API_GRAVITY,
                ValidationStatus = m.VALIDATION_STATUS ?? "Pending"
            }).ToList();
        }

        /// <summary>
        /// Gets measurement summary.
        /// </summary>
        public async Task<MeasurementSummary> GetMeasurementSummaryAsync(string wellId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(wellId))
                throw new ArgumentException("Well ID is required.", nameof(wellId));

            var connName = connectionName ?? _connectionName;
            var measurements = await GetMeasurementsByWellAsync(wellId, startDate, endDate, connName);

            if (!measurements.Any())
            {
                return new MeasurementSummary
                {
                    WellId = wellId,
                    StartDate = startDate,
                    EndDate = endDate,
                    MeasurementCount = 0
                };
            }

            var validMeasurements = measurements.Where(m => m.VALIDATION_STATUS == "Valid").ToList();
            var apiGravityValues = measurements.Where(m => m.API_GRAVITY.HasValue).Select(m => m.API_GRAVITY!.Value).ToList();
            var bswValues = measurements.Where(m => m.BSW_PERCENTAGE.HasValue).Select(m => m.BSW_PERCENTAGE!.Value).ToList();

            return new MeasurementSummary
            {
                WellId = wellId,
                StartDate = startDate,
                EndDate = endDate,
                MeasurementCount = measurements.Count,
                TotalGrossVolume = measurements.Sum(m => m.GROSS_VOLUME ?? 0m),
                TotalNetVolume = measurements.Sum(m => m.NET_VOLUME ?? 0m),
                AverageApiGravity = apiGravityValues.Any() ? apiGravityValues.Average() : 0m,
                AverageBsw = bswValues.Any() ? bswValues.Average() : 0m,
                ValidatedCount = validMeasurements.Count,
                PendingValidationCount = measurements.Count - validMeasurements.Count
            };
        }

        // Helper methods for conversion
        private MEASUREMENT_RECORD ConvertToMeasurementEntity(MeasurementRecord record)
        {
            return new MEASUREMENT_RECORD
            {
                MEASUREMENT_RECORD_ID = record.MeasurementId,
                MEASUREMENT_DATE_TIME = record.MeasurementDateTime,
                MEASUREMENT_METHOD = record.Method.ToString(),
                MEASUREMENT_STANDARD = record.Standard.ToString(),
                GROSS_VOLUME = record.GrossVolume,
                BSW_PERCENTAGE = record.BSW,
                NET_VOLUME = record.NetVolume,
                TEMPERATURE = record.Temperature,
                PRESSURE = record.Pressure,
                API_GRAVITY = record.ApiGravity,
                SULFUR_CONTENT = record.Properties?.SulfurContent,
                ACCURACY = record.Accuracy,
                MEASUREMENT_DEVICE = record.MeasurementDevice,
                OPERATOR = record.Operator,
                NOTES = record.Notes,
                VALIDATION_STATUS = "Pending",
                ACTIVE_IND = "Y"
            };
        }

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetMeasurementRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(MEASUREMENT_RECORD_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Measurement.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(MEASUREMENT_RECORD);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, MEASUREMENT_RECORD_TABLE,
                null);
        }
    }
}
