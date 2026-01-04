using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.ProductionAccounting;
using System.Text.Json;

namespace Beep.OilandGas.ProductionAccounting.Imbalance
{
    /// <summary>
    /// Manages oil imbalance calculations and reconciliation.
    /// Uses Entity classes directly with IDataSource - no dictionary conversions.
    /// </summary>
    public class ImbalanceManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<ImbalanceManager>? _logger;
        private readonly string _connectionName;
        private const string PRODUCTION_AVAILS_TABLE = "PRODUCTION_AVAILS";
        private const string NOMINATION_TABLE = "NOMINATION";
        private const string ACTUAL_DELIVERY_TABLE = "ACTUAL_DELIVERY";
        private const string OIL_IMBALANCE_TABLE = "OIL_IMBALANCE";
        private const string IMBALANCE_STATEMENT_TABLE = "IMBALANCE_STATEMENT";

        public ImbalanceManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<ImbalanceManager>? logger = null,
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
        /// Creates a production avails estimate.
        /// </summary>
        public async Task<PRODUCTION_AVAILS> CreateProductionAvailsAsync(
            DateTime periodStart,
            DateTime periodEnd,
            decimal estimatedProduction,
            string userId = "system",
            string? connectionName = null)
        {
            var avails = new PRODUCTION_AVAILS
            {
                PRODUCTION_AVAILS_ID = Guid.NewGuid().ToString(),
                PERIOD_START = periodStart,
                PERIOD_END = periodEnd,
                ESTIMATED_PRODUCTION = estimatedProduction,
                AVAILABLE_FOR_DELIVERY = estimatedProduction // Simplified - would subtract inventory, etc.
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(avails, userId);
            var result = dataSource.InsertEntity(PRODUCTION_AVAILS_TABLE, avails);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create production avails {AvailsId}: {Error}", avails.PRODUCTION_AVAILS_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save production avails: {errorMessage}");
            }

            _logger?.LogDebug("Created production avails {AvailsId} in database", avails.PRODUCTION_AVAILS_ID);
            return avails;
        }

        /// <summary>
        /// Creates a production avails estimate (synchronous wrapper).
        /// </summary>
        public PRODUCTION_AVAILS CreateProductionAvails(
            DateTime periodStart,
            DateTime periodEnd,
            decimal estimatedProduction)
        {
            return CreateProductionAvailsAsync(periodStart, periodEnd, estimatedProduction).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a nomination.
        /// </summary>
        public async Task<NOMINATION> CreateNominationAsync(
            DateTime periodStart,
            DateTime periodEnd,
            decimal nominatedVolume,
            List<string> deliveryPoints,
            string userId = "system",
            string? connectionName = null)
        {
            var nomination = new NOMINATION
            {
                NOMINATION_ID = Guid.NewGuid().ToString(),
                PERIOD_START = periodStart,
                PERIOD_END = periodEnd,
                NOMINATED_VOLUME = nominatedVolume,
                DELIVERY_POINTS_JSON = JsonSerializer.Serialize(deliveryPoints),
                STATUS = NominationStatus.Pending.ToString()
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(nomination, userId);
            var result = dataSource.InsertEntity(NOMINATION_TABLE, nomination);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create nomination {NominationId}: {Error}", nomination.NOMINATION_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save nomination: {errorMessage}");
            }

            _logger?.LogDebug("Created nomination {NominationId} in database", nomination.NOMINATION_ID);
            return nomination;
        }

        /// <summary>
        /// Creates a nomination (synchronous wrapper).
        /// </summary>
        public NOMINATION CreateNomination(
            DateTime periodStart,
            DateTime periodEnd,
            decimal nominatedVolume,
            List<string> deliveryPoints)
        {
            return CreateNominationAsync(periodStart, periodEnd, nominatedVolume, deliveryPoints).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Approves a nomination.
        /// </summary>
        public async Task ApproveNominationAsync(string nominationId, string approvedBy, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "NOMINATION_ID", Operator = "=", FilterValue = nominationId }
            };

            var results = await dataSource.GetEntityAsync(NOMINATION_TABLE, filters);
            var nomination = results?.FirstOrDefault() as NOMINATION;
            if (nomination == null)
                throw new ArgumentException($"Nomination {nominationId} not found.", nameof(nominationId));

            nomination.STATUS = NominationStatus.Approved.ToString();
            nomination.APPROVAL_DATE = DateTime.Now;
            nomination.APPROVED_BY = approvedBy;

            _commonColumnHandler.PrepareForUpdate(nomination, approvedBy);
            dataSource.UpdateEntity(NOMINATION_TABLE, nomination);

            _logger?.LogDebug("Approved nomination {NominationId}", nominationId);
        }

        /// <summary>
        /// Approves a nomination (synchronous wrapper).
        /// </summary>
        public void ApproveNomination(string nominationId, string approvedBy)
        {
            ApproveNominationAsync(nominationId, approvedBy).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Records an actual delivery.
        /// </summary>
        public async Task<ACTUAL_DELIVERY> RecordActualDeliveryAsync(
            DateTime deliveryDate,
            decimal actualVolume,
            string deliveryPoint,
            string allocationMethod,
            string? runTicketNumber = null,
            string userId = "system",
            string? connectionName = null)
        {
            var delivery = new ACTUAL_DELIVERY
            {
                ACTUAL_DELIVERY_ID = Guid.NewGuid().ToString(),
                DELIVERY_DATE = deliveryDate,
                ACTUAL_VOLUME = actualVolume,
                DELIVERY_POINT = deliveryPoint,
                ALLOCATION_METHOD = allocationMethod,
                RUN_TICKET_NUMBER = runTicketNumber
            };

            // Prepare for insert and save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            _commonColumnHandler.PrepareForInsert(delivery, userId);
            var result = dataSource.InsertEntity(ACTUAL_DELIVERY_TABLE, delivery);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record actual delivery {DeliveryId}: {Error}", delivery.ACTUAL_DELIVERY_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save actual delivery: {errorMessage}");
            }

            _logger?.LogDebug("Recorded actual delivery {DeliveryId} in database", delivery.ACTUAL_DELIVERY_ID);
            return delivery;
        }

        /// <summary>
        /// Records an actual delivery (synchronous wrapper).
        /// </summary>
        public ACTUAL_DELIVERY RecordActualDelivery(
            DateTime deliveryDate,
            decimal actualVolume,
            string deliveryPoint,
            string allocationMethod,
            string? runTicketNumber = null)
        {
            return RecordActualDeliveryAsync(deliveryDate, actualVolume, deliveryPoint, allocationMethod, runTicketNumber).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Calculates imbalance for a period.
        /// </summary>
        public async Task<OilImbalance> CalculateImbalanceAsync(
            string nominationId,
            DateTime periodStart,
            DateTime periodEnd,
            decimal tolerancePercentage = 2.0m,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Get nomination
            var nominationFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "NOMINATION_ID", Operator = "=", FilterValue = nominationId }
            };
            var nominationResults = await dataSource.GetEntityAsync(NOMINATION_TABLE, nominationFilters);
            var nominationData = nominationResults?.FirstOrDefault();
            if (nominationData == null)
                throw new ArgumentException($"Nomination {nominationId} not found.", nameof(nominationId));

            var nomination = nominationData as NOMINATION;
            if (nomination == null)
                throw new ArgumentException($"Nomination {nominationId} not found.", nameof(nominationId));

            // Get period deliveries
            var deliveryFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DELIVERY_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "DELIVERY_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd")}
            };
            var deliveryResults = await dataSource.GetEntityAsync(ACTUAL_DELIVERY_TABLE, deliveryFilters);
            var periodDeliveries = deliveryResults?.Cast<ACTUAL_DELIVERY>().Where(d => d != null).ToList() ?? new List<ACTUAL_DELIVERY>();

            decimal actualVolume = periodDeliveries.Sum(d => d.ACTUAL_VOLUME ?? 0m);
            decimal nominatedVolume = nomination.NOMINATED_VOLUME ?? 0m;
            decimal imbalanceAmount = actualVolume - nominatedVolume;
            decimal imbalancePercentage = nominatedVolume != 0 ? (imbalanceAmount / nominatedVolume) * 100m : 0m;
            bool isWithinTolerance = Math.Abs(imbalancePercentage) <= tolerancePercentage;

            var imbalance = new OIL_IMBALANCE
            {
                IMBALANCE_ID = Guid.NewGuid().ToString(),
                PERIOD_START = periodStart,
                PERIOD_END = periodEnd,
                NOMINATED_VOLUME = nominatedVolume,
                ACTUAL_VOLUME = actualVolume,
                IMBALANCE_AMOUNT = imbalanceAmount,
                IMBALANCE_PERCENTAGE = imbalancePercentage,
                TOLERANCE_PERCENTAGE = tolerancePercentage,
                IS_WITHIN_TOLERANCE_IND = isWithinTolerance ? "Y" : "N"
            };

            // Determine status
            if (isWithinTolerance)
                imbalance.STATUS = ImbalanceStatus.Balanced.ToString();
            else if (imbalanceAmount > 0)
                imbalance.STATUS = ImbalanceStatus.OverDelivered.ToString();
            else
                imbalance.STATUS = ImbalanceStatus.UnderDelivered.ToString();

            // Prepare for insert and save to database
            _commonColumnHandler.PrepareForInsert(imbalance, "system");
            var result = dataSource.InsertEntity(OIL_IMBALANCE_TABLE, imbalance);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to calculate imbalance {ImbalanceId}: {Error}", imbalance.IMBALANCE_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save imbalance: {errorMessage}");
            }

            _logger?.LogDebug("Calculated imbalance {ImbalanceId} in database", imbalance.IMBALANCE_ID);
            return imbalance;
        }

        /// <summary>
        /// Calculates imbalance for a period (synchronous wrapper).
        /// </summary>
        public OIL_IMBALANCE CalculateImbalance(
            string nominationId,
            DateTime periodStart,
            DateTime periodEnd,
            decimal tolerancePercentage = 2.0m)
        {
            return CalculateImbalanceAsync(nominationId, periodStart, periodEnd, tolerancePercentage).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generates an imbalance statement.
        /// </summary>
        public async Task<IMBALANCE_STATEMENT> GenerateStatementAsync(
            DateTime periodStart,
            DateTime periodEnd,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            // Get nominations
            var nominationFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERIOD_START", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "PERIOD_END", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") }
            };
            var nominationResults = await dataSource.GetEntityAsync(NOMINATION_TABLE, nominationFilters);
            var periodNominations = nominationResults?.Cast<NOMINATION>().Where(n => n != null).ToList() ?? new List<NOMINATION>();

            // Get deliveries
            var deliveryFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DELIVERY_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "DELIVERY_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") }
            };
            var deliveryResults = await dataSource.GetEntityAsync(ACTUAL_DELIVERY_TABLE, deliveryFilters);
            var periodDeliveries = deliveryResults?.Cast<ACTUAL_DELIVERY>().Where(d => d != null).ToList() ?? new List<ACTUAL_DELIVERY>();

            // Get imbalances
            var imbalanceFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERIOD_START", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "PERIOD_END", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") }
            };
            var imbalanceResults = await dataSource.GetEntityAsync(OIL_IMBALANCE_TABLE, imbalanceFilters);
            var periodImbalances = imbalanceResults?.Cast<OIL_IMBALANCE>().Where(i => i != null).ToList() ?? new List<OIL_IMBALANCE>();

            var statement = new IMBALANCE_STATEMENT
            {
                IMBALANCE_STATEMENT_ID = Guid.NewGuid().ToString(),
                STATEMENT_PERIOD_START = periodStart,
                STATEMENT_PERIOD_END = periodEnd
            };

            // Prepare for insert and save to database
            _commonColumnHandler.PrepareForInsert(statement, "system");
            var result = dataSource.InsertEntity(IMBALANCE_STATEMENT_TABLE, statement);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to generate imbalance statement {StatementId}: {Error}", statement.IMBALANCE_STATEMENT_ID, errorMessage);
                throw new InvalidOperationException($"Failed to save imbalance statement: {errorMessage}");
            }

            _logger?.LogDebug("Generated imbalance statement {StatementId} in database", statement.IMBALANCE_STATEMENT_ID);
            return statement;
        }

        /// <summary>
        /// Generates an imbalance statement (synchronous wrapper).
        /// </summary>
        public IMBALANCE_STATEMENT GenerateStatement(
            DateTime periodStart,
            DateTime periodEnd)
        {
            return GenerateStatementAsync(periodStart, periodEnd).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reconciles an imbalance.
        /// </summary>
        public async Task<ImbalanceReconciliation> ReconcileImbalanceAsync(
            string imbalanceId,
            List<ImbalanceAdjustment> adjustments,
            string reconciledBy,
            string? notes = null,
            string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "IMBALANCE_ID", Operator = "=", FilterValue = imbalanceId }
            };

            var results = await dataSource.GetEntityAsync(OIL_IMBALANCE_TABLE, filters);
            var imbalanceData = results?.FirstOrDefault();
            if (imbalanceData == null)
                throw new ArgumentException($"Imbalance {imbalanceId} not found.", nameof(imbalanceId));

            var imbalance = imbalanceData as OIL_IMBALANCE;
            if (imbalance == null)
                throw new ArgumentException($"Imbalance {imbalanceId} not found.", nameof(imbalanceId));

            // Calculate reconciliation (simplified - adjustments would be stored separately)
            decimal totalAdjustments = adjustments?.Sum(a => a.AdjustmentAmount) ?? 0m;
            decimal totalImbalanceAfter = (imbalance.IMBALANCE_AMOUNT ?? 0m) - totalAdjustments;
            decimal toleranceAmount = (imbalance.TOLERANCE_PERCENTAGE ?? 0m) / 100m * (imbalance.NOMINATED_VOLUME ?? 0m);

            // Update imbalance status
            if (totalImbalanceAfter == 0 || Math.Abs(totalImbalanceAfter) <= toleranceAmount)
                imbalance.STATUS = ImbalanceStatus.Reconciled.ToString();
            else
                imbalance.STATUS = ImbalanceStatus.PendingReconciliation.ToString();

            // Update imbalance in database
            _commonColumnHandler.PrepareForUpdate(imbalance, reconciledBy);
            dataSource.UpdateEntity(OIL_IMBALANCE_TABLE, imbalance);

            // Note: ImbalanceReconciliation would be a separate Entity class if needed
            var reconciliation = new { ReconciliationId = Guid.NewGuid().ToString(), ReconciliationDate = DateTime.Now, TotalImbalanceBefore = imbalance.IMBALANCE_AMOUNT, Adjustments = adjustments, ReconciledBy = reconciledBy, Notes = notes };

            _logger?.LogDebug("Reconciled imbalance {ImbalanceId}", imbalanceId);
            return reconciliation;
        }

        /// <summary>
        /// Reconciles an imbalance (synchronous wrapper).
        /// </summary>
        public object ReconcileImbalance(
            string imbalanceId,
            List<ImbalanceAdjustment> adjustments,
            string reconciledBy,
            string? notes = null)
        {
            return ReconcileImbalanceAsync(imbalanceId, adjustments, reconciledBy, notes).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets imbalances by status.
        /// </summary>
        public async Task<IEnumerable<OIL_IMBALANCE>> GetImbalancesByStatusAsync(ImbalanceStatus status, string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "=", FilterValue = status.ToString() }
            };

            var results = await dataSource.GetEntityAsync(OIL_IMBALANCE_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<OIL_IMBALANCE>();

            return results.Cast<OIL_IMBALANCE>().Where(i => i != null)!;
        }

        /// <summary>
        /// Gets imbalances by status (synchronous wrapper).
        /// </summary>
        public IEnumerable<OIL_IMBALANCE> GetImbalancesByStatus(ImbalanceStatus status)
        {
            return GetImbalancesByStatusAsync(status).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets all imbalances requiring reconciliation.
        /// </summary>
        public async Task<IEnumerable<OIL_IMBALANCE>> GetImbalancesRequiringReconciliationAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "STATUS", Operator = "IN", FilterValue = "PendingReconciliation,UnderDelivered,OverDelivered" }
            };

            var results = await dataSource.GetEntityAsync(OIL_IMBALANCE_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<OIL_IMBALANCE>();

            return results.Cast<OIL_IMBALANCE>()
                .Where(i => i != null && (i.STATUS == ImbalanceStatus.PendingReconciliation.ToString() || (i.IS_WITHIN_TOLERANCE_IND != "Y" && i.STATUS != ImbalanceStatus.Reconciled.ToString())))!;
        }

        /// <summary>
        /// Gets all imbalances requiring reconciliation (synchronous wrapper).
        /// </summary>
        public IEnumerable<OIL_IMBALANCE> GetImbalancesRequiringReconciliation()
        {
            return GetImbalancesRequiringReconciliationAsync().GetAwaiter().GetResult();
        }
    }
}
