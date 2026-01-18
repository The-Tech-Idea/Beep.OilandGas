using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// Measurement Service - Records and validates production measurements.
    /// Converts run tickets to measurement records for accounting.
    /// </summary>
    public class MeasurementService : IMeasurementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<MeasurementService> _logger;
        private const string ConnectionName = "PPDM39";

        public MeasurementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<MeasurementService> logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _logger = logger;
        }

        /// <summary>
        /// Records a production measurement from a run ticket.
        /// </summary>
        public async Task<MEASUREMENT_RECORD> RecordAsync(
            RUN_TICKET ticket,
            string userId,
            string cn = "PPDM39")
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            _logger?.LogInformation("Recording measurement for run ticket {TicketId}", ticket.RUN_TICKET_ID);

            var measurement = new MEASUREMENT_RECORD
            {
                MEASUREMENT_ID = Guid.NewGuid().ToString(),
                MEASUREMENT_DATETIME = ticket.TICKET_DATE_TIME ?? DateTime.UtcNow,
                GROSS_VOLUME = ticket.GROSS_VOLUME,
                NET_VOLUME = ticket.NET_VOLUME,
                MEASUREMENT_METHOD = "AUTOMATED",
                MEASUREMENT_STANDARD = "API",
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CREATED_BY = userId
            };

            var metadata = await _metadata.GetTableMetadataAsync("MEASUREMENT_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(MEASUREMENT_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "MEASUREMENT_RECORD");

            await repo.InsertAsync(measurement, userId);

            _logger?.LogInformation("Measurement recorded: {MeasurementId}",
                measurement.MEASUREMENT_ID);

            return measurement;
        }

        /// <summary>
        /// Gets a measurement record by ID.
        /// </summary>
        public async Task<MEASUREMENT_RECORD?> GetAsync(string measurementId, string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(measurementId))
                throw new ArgumentNullException(nameof(measurementId));

            var metadata = await _metadata.GetTableMetadataAsync("MEASUREMENT_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(MEASUREMENT_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "MEASUREMENT_RECORD");

            var result = await repo.GetByIdAsync(measurementId);
            return result as MEASUREMENT_RECORD;
        }

        /// <summary>
        /// Gets measurements for a well in a date range.
        /// </summary>
        public async Task<List<MEASUREMENT_RECORD>> GetByWellAsync(
            string wellId,
            DateTime start,
            DateTime end,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(wellId))
                throw new ArgumentNullException(nameof(wellId));

            var metadata = await _metadata.GetTableMetadataAsync("MEASUREMENT_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(MEASUREMENT_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "MEASUREMENT_RECORD");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "WELL_ID", Operator = "=", FilterValue = wellId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var records = await repo.GetAsync(filters);
            return records?.Cast<MEASUREMENT_RECORD>().ToList() ?? new List<MEASUREMENT_RECORD>();
        }

        /// <summary>
        /// Gets measurements for a lease in a date range.
        /// </summary>
        public async Task<List<MEASUREMENT_RECORD>> GetByLeaseAsync(
            string leaseId,
            DateTime start,
            DateTime end,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(leaseId))
                throw new ArgumentNullException(nameof(leaseId));

            var metadata = await _metadata.GetTableMetadataAsync("MEASUREMENT_RECORD");
            var entityType = Type.GetType($"Beep.OilandGas.PPDM39.Models.{metadata.EntityTypeName}")
                ?? typeof(MEASUREMENT_RECORD);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, cn, "MEASUREMENT_RECORD");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "LEASE_ID", Operator = "=", FilterValue = leaseId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var records = await repo.GetAsync(filters);
            return records?.Cast<MEASUREMENT_RECORD>().ToList() ?? new List<MEASUREMENT_RECORD>();
        }

        /// <summary>
        /// Validates a measurement record.
        /// </summary>
        public async Task<bool> ValidateAsync(MEASUREMENT_RECORD measurement, string cn = "PPDM39")
        {
            if (measurement == null)
                throw new ArgumentNullException(nameof(measurement));

            _logger?.LogInformation("Validating measurement {MeasurementId}", measurement.MEASUREMENT_ID);

            try
            {
                if (measurement.GROSS_VOLUME == null || measurement.GROSS_VOLUME <= 0)
                    throw new AllocationException($"Gross volume must be positive: {measurement.GROSS_VOLUME}");

                if (measurement.NET_VOLUME.HasValue && measurement.NET_VOLUME < 0)
                    throw new AllocationException($"Net volume cannot be negative: {measurement.NET_VOLUME}");

                if (measurement.NET_VOLUME > measurement.GROSS_VOLUME)
                    throw new AllocationException("Net volume cannot exceed gross volume");

                _logger?.LogInformation("Measurement {MeasurementId} validation passed", measurement.MEASUREMENT_ID);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Measurement validation failed");
                throw;
            }
        }
    }
}
