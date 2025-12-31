using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Imbalance;
using Beep.OilandGas.Models.DTOs.Imbalance;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ProductionAccounting.Imbalance
{
    /// <summary>
    /// Service for managing oil imbalance calculations and reconciliation.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class ImbalanceService : IImbalanceService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ImbalanceService>? _logger;
        private readonly string _connectionName;
        private const string PRODUCTION_AVAIL_TABLE = "PRODUCTION_AVAIL";
        private const string NOMINATION_TABLE = "NOMINATION";
        private const string ACTUAL_DELIVERY_TABLE = "ACTUAL_DELIVERY";
        private const string IMBALANCE_TABLE = "IMBALANCE";

        public ImbalanceService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ImbalanceService>? logger = null,
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
        /// Creates a production avail.
        /// </summary>
        public async Task<PRODUCTION_AVAIL> CreateProductionAvailAsync(
            CreateProductionAvailRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PRODUCTION_AVAIL), connName, PRODUCTION_AVAIL_TABLE, null);

            var avail = new PRODUCTION_AVAIL
            {
                PRODUCTION_AVAIL_ID = Guid.NewGuid().ToString(),
                PROPERTY_ID = request.PropertyId,
                AVAIL_DATE = request.AvailDate,
                ESTIMATED_VOLUME = request.EstimatedVolume,
                AVAILABLE_FOR_DELIVERY = request.AvailableForDelivery ?? request.EstimatedVolume,
                ACTIVE_IND = "Y"
            };

            if (avail is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(avail);

            _logger?.LogDebug("Created production avail for property {PropertyId}", request.PropertyId);
            return avail;
        }

        /// <summary>
        /// Gets a production avail by ID.
        /// </summary>
        public async Task<PRODUCTION_AVAIL?> GetProductionAvailAsync(string availId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(availId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PRODUCTION_AVAIL), connName, PRODUCTION_AVAIL_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PRODUCTION_AVAIL_ID", Operator = "=", FilterValue = availId }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<PRODUCTION_AVAIL>().FirstOrDefault();
        }

        /// <summary>
        /// Creates a nomination.
        /// </summary>
        public async Task<NOMINATION> CreateNominationAsync(
            CreateNominationRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(NOMINATION), connName, NOMINATION_TABLE, null);

            var nomination = new NOMINATION
            {
                NOMINATION_ID = Guid.NewGuid().ToString(),
                PERIOD_START = request.PeriodStart,
                PERIOD_END = request.PeriodEnd,
                NOMINATED_VOLUME = request.NominatedVolume,
                STATUS = "Pending",
                SUBMISSION_DATE = DateTime.UtcNow,
                ACTIVE_IND = "Y"
            };

            if (nomination is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(nomination);

            _logger?.LogDebug("Created nomination {NominationId}", nomination.NOMINATION_ID);
            return nomination;
        }

        /// <summary>
        /// Gets nominations by period.
        /// </summary>
        public async Task<List<NOMINATION>> GetNominationsByPeriodAsync(
            DateTime periodStart,
            DateTime periodEnd,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(NOMINATION), connName, NOMINATION_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERIOD_START", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "PERIOD_END", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<NOMINATION>().OrderByDescending(n => n.PERIOD_START).ToList();
        }

        /// <summary>
        /// Records an actual delivery.
        /// </summary>
        public async Task<ACTUAL_DELIVERY> RecordActualDeliveryAsync(
            CreateActualDeliveryRequest request,
            string userId,
            string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ACTUAL_DELIVERY), connName, ACTUAL_DELIVERY_TABLE, null);

            var delivery = new ACTUAL_DELIVERY
            {
                ACTUAL_DELIVERY_ID = Guid.NewGuid().ToString(),
                NOMINATION_ID = request.NominationId,
                DELIVERY_DATE = request.DeliveryDate,
                ACTUAL_VOLUME = request.ActualVolume,
                DELIVERY_POINT = request.DeliveryPoint,
                ALLOCATION_METHOD = request.AllocationMethod,
                RUN_TICKET_NUMBER = request.RunTicketNumber,
                ACTIVE_IND = "Y"
            };

            if (delivery is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await repo.InsertAsync(delivery);

            _logger?.LogDebug("Recorded actual delivery {DeliveryId}", delivery.ACTUAL_DELIVERY_ID);
            return delivery;
        }

        /// <summary>
        /// Gets deliveries by nomination.
        /// </summary>
        public async Task<List<ACTUAL_DELIVERY>> GetDeliveriesByNominationAsync(
            string nominationId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(nominationId))
                return new List<ACTUAL_DELIVERY>();

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ACTUAL_DELIVERY), connName, ACTUAL_DELIVERY_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "NOMINATION_ID", Operator = "=", FilterValue = nominationId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<ACTUAL_DELIVERY>().OrderBy(d => d.DELIVERY_DATE).ToList();
        }

        /// <summary>
        /// Calculates imbalance for a nomination.
        /// </summary>
        public async Task<IMBALANCE> CalculateImbalanceAsync(
            string nominationId,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(nominationId))
                throw new ArgumentException("Nomination ID is required.", nameof(nominationId));

            var connName = connectionName ?? _connectionName;
            var nominationRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(NOMINATION), connName, NOMINATION_TABLE, null);

            var nominationFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "NOMINATION_ID", Operator = "=", FilterValue = nominationId }
            };
            var nominationResults = await nominationRepo.GetAsync(nominationFilters);
            var nomination = nominationResults.Cast<NOMINATION>().FirstOrDefault();

            if (nomination == null)
                throw new InvalidOperationException($"Nomination {nominationId} not found.");

            var deliveries = await GetDeliveriesByNominationAsync(nominationId, connName);
            var actualVolume = deliveries.Sum(d => d.ACTUAL_VOLUME ?? 0m);
            var nominatedVolume = nomination.NOMINATED_VOLUME ?? 0m;
            var imbalanceAmount = actualVolume - nominatedVolume;

            var tolerancePercentage = 2.0m; // Default tolerance

            // Determine status
            var imbalancePercentage = nominatedVolume > 0 ? Math.Abs(imbalanceAmount / nominatedVolume) * 100m : 0m;
            string status;
            if (imbalancePercentage <= tolerancePercentage)
                status = "Balanced";
            else if (imbalanceAmount > 0)
                status = "OverDelivered";
            else
                status = "UnderDelivered";

            var imbalanceRepo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(IMBALANCE), connName, IMBALANCE_TABLE, null);

            var imbalance = new IMBALANCE
            {
                IMBALANCE_ID = Guid.NewGuid().ToString(),
                NOMINATION_ID = nominationId,
                PERIOD_START = nomination.PERIOD_START,
                PERIOD_END = nomination.PERIOD_END,
                NOMINATED_VOLUME = nominatedVolume,
                ACTUAL_VOLUME = actualVolume,
                IMBALANCE_AMOUNT = imbalanceAmount,
                TOLERANCE_PERCENTAGE = tolerancePercentage,
                STATUS = status,
                ACTIVE_IND = "Y"
            };

            if (imbalance is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }

            await imbalanceRepo.InsertAsync(imbalance);

            _logger?.LogDebug("Calculated imbalance {ImbalanceId} for nomination {NominationId}",
                imbalance.IMBALANCE_ID, nominationId);

            return imbalance;
        }

        /// <summary>
        /// Gets imbalances by period.
        /// </summary>
        public async Task<List<IMBALANCE>> GetImbalancesByPeriodAsync(
            DateTime periodStart,
            DateTime periodEnd,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(IMBALANCE), connName, IMBALANCE_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERIOD_START", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "PERIOD_END", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };
            var results = await repo.GetAsync(filters);
            return results.Cast<IMBALANCE>().OrderByDescending(i => i.PERIOD_START).ToList();
        }

        /// <summary>
        /// Reconciles an imbalance.
        /// </summary>
        public async Task<ImbalanceReconciliationResult> ReconcileImbalanceAsync(
            string imbalanceId,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(imbalanceId))
                throw new ArgumentException("Imbalance ID is required.", nameof(imbalanceId));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(IMBALANCE), connName, IMBALANCE_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "IMBALANCE_ID", Operator = "=", FilterValue = imbalanceId }
            };
            var results = await repo.GetAsync(filters);
            var imbalance = results.Cast<IMBALANCE>().FirstOrDefault();

            if (imbalance == null)
                throw new InvalidOperationException($"Imbalance {imbalanceId} not found.");

            var imbalanceBefore = imbalance.IMBALANCE_AMOUNT ?? 0m;

            // For now, reconciliation just marks as reconciled
            // In a full implementation, adjustments would be applied
            imbalance.STATUS = "Reconciled";

            if (imbalance is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
            }

            await repo.UpdateAsync(imbalance);

            _logger?.LogDebug("Reconciled imbalance {ImbalanceId}", imbalanceId);

            return new ImbalanceReconciliationResult
            {
                ReconciliationId = Guid.NewGuid().ToString(),
                ImbalanceId = imbalanceId,
                ImbalanceBefore = imbalanceBefore,
                ImbalanceAfter = imbalanceBefore, // Would be adjusted in full implementation
                IsReconciled = true,
                ReconciliationDate = DateTime.UtcNow,
                ReconciledBy = userId
            };
        }

        /// <summary>
        /// Settles an imbalance.
        /// </summary>
        public async Task<ImbalanceSettlementResult> SettleImbalanceAsync(
            string imbalanceId,
            DateTime settlementDate,
            string userId,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(imbalanceId))
                throw new ArgumentException("Imbalance ID is required.", nameof(imbalanceId));

            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(IMBALANCE), connName, IMBALANCE_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "IMBALANCE_ID", Operator = "=", FilterValue = imbalanceId }
            };
            var results = await repo.GetAsync(filters);
            var imbalance = results.Cast<IMBALANCE>().FirstOrDefault();

            if (imbalance == null)
                throw new InvalidOperationException($"Imbalance {imbalanceId} not found.");

            imbalance.SETTLEMENT_DATE = settlementDate;
            imbalance.STATUS = "Settled";

            if (imbalance is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForUpdate(ppdmEntity, userId);
            }

            await repo.UpdateAsync(imbalance);

            _logger?.LogDebug("Settled imbalance {ImbalanceId}", imbalanceId);

            return new ImbalanceSettlementResult
            {
                SettlementId = Guid.NewGuid().ToString(),
                ImbalanceId = imbalanceId,
                SettlementDate = settlementDate,
                SettlementAmount = imbalance.IMBALANCE_AMOUNT ?? 0m,
                Status = "Settled",
                SettledBy = userId
            };
        }

        /// <summary>
        /// Gets imbalance summary.
        /// </summary>
        public async Task<List<Beep.OilandGas.Models.DTOs.Imbalance.ImbalanceSummary>> GetImbalanceSummaryAsync(
            DateTime? periodStart,
            DateTime? periodEnd,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(IMBALANCE), connName, IMBALANCE_TABLE, null);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (periodStart.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "PERIOD_START", Operator = ">=", FilterValue = periodStart.Value.ToString("yyyy-MM-dd") });
            }

            if (periodEnd.HasValue)
            {
                filters.Add(new AppFilter { FieldName = "PERIOD_END", Operator = "<=", FilterValue = periodEnd.Value.ToString("yyyy-MM-dd") });
            }

            var results = await repo.GetAsync(filters);
            var imbalances = results.Cast<IMBALANCE>().ToList();

            if (!imbalances.Any())
                return new List<Beep.OilandGas.Models.DTOs.Imbalance.ImbalanceSummary>();

            var summary = new Beep.OilandGas.Models.DTOs.Imbalance.ImbalanceSummary
            {
                PeriodStart = periodStart?.ToString("yyyy-MM-dd") ?? imbalances.Min(i => i.PERIOD_START)?.ToString("yyyy-MM-dd") ?? string.Empty,
                PeriodEnd = periodEnd?.ToString("yyyy-MM-dd") ?? imbalances.Max(i => i.PERIOD_END)?.ToString("yyyy-MM-dd") ?? string.Empty,
                TotalNominatedVolume = imbalances.Sum(i => i.NOMINATED_VOLUME ?? 0m),
                TotalActualVolume = imbalances.Sum(i => i.ACTUAL_VOLUME ?? 0m),
                TotalImbalanceAmount = imbalances.Sum(i => i.IMBALANCE_AMOUNT ?? 0m),
                ImbalanceCount = imbalances.Count,
                BalancedCount = imbalances.Count(i => i.STATUS == "Balanced"),
                OverDeliveredCount = imbalances.Count(i => i.STATUS == "OverDelivered"),
                UnderDeliveredCount = imbalances.Count(i => i.STATUS == "UnderDelivered")
            };

            return new List<Beep.OilandGas.Models.DTOs.Imbalance.ImbalanceSummary> { summary };
        }
    }
}
