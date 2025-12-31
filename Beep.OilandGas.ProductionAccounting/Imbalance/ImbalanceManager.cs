namespace Beep.OilandGas.ProductionAccounting.Imbalance
{
    /// <summary>
    /// Manages oil imbalance calculations and reconciliation.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
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
        public async Task<ProductionAvails> CreateProductionAvailsAsync(
            DateTime periodStart,
            DateTime periodEnd,
            decimal estimatedProduction,
            string userId = "system",
            string? connectionName = null)
        {
            var avails = new ProductionAvails
            {
                AvailsId = Guid.NewGuid().ToString(),
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                EstimatedProduction = estimatedProduction,
                AvailableForDelivery = estimatedProduction // Simplified - would subtract inventory, etc.
            };

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(PRODUCTION_AVAILS_TABLE, avails);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create production avails {AvailsId}: {Error}", avails.AvailsId, errorMessage);
                throw new InvalidOperationException($"Failed to save production avails: {errorMessage}");
            }

            _logger?.LogDebug("Created production avails {AvailsId} in database", avails.AvailsId);
            return avails;
        }

        /// <summary>
        /// Creates a production avails estimate (synchronous wrapper).
        /// </summary>
        public ProductionAvails CreateProductionAvails(
            DateTime periodStart,
            DateTime periodEnd,
            decimal estimatedProduction)
        {
            return CreateProductionAvailsAsync(periodStart, periodEnd, estimatedProduction).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates a nomination.
        /// </summary>
        public async Task<Nomination> CreateNominationAsync(
            DateTime periodStart,
            DateTime periodEnd,
            decimal nominatedVolume,
            List<string> deliveryPoints,
            string userId = "system",
            string? connectionName = null)
        {
            var nomination = new Nomination
            {
                NominationId = Guid.NewGuid().ToString(),
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                NominatedVolume = nominatedVolume,
                DeliveryPoints = deliveryPoints,
                Status = NominationStatus.Pending
            };

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(NOMINATION_TABLE, nomination);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create nomination {NominationId}: {Error}", nomination.NominationId, errorMessage);
                throw new InvalidOperationException($"Failed to save nomination: {errorMessage}");
            }

            _logger?.LogDebug("Created nomination {NominationId} in database", nomination.NominationId);
            return nomination;
        }

        /// <summary>
        /// Creates a nomination (synchronous wrapper).
        /// </summary>
        public Nomination CreateNomination(
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
            var nominationData = results?.FirstOrDefault();
            if (nominationData == null)
                throw new ArgumentException($"Nomination {nominationId} not found.", nameof(nominationId));

            var nomination = nominationData as Nomination;
            if (nomination == null)
                throw new ArgumentException($"Nomination {nominationId} not found.", nameof(nominationId));

            nomination.Status = NominationStatus.Approved;
            nomination.ApprovalDate = DateTime.Now;
            nomination.ApprovedBy = approvedBy;

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
        public async Task RecordActualDeliveryAsync(
            DateTime deliveryDate,
            decimal actualVolume,
            string deliveryPoint,
            string allocationMethod,
            string? runTicketNumber = null,
            string userId = "system",
            string? connectionName = null)
        {
            var delivery = new ActualDelivery
            {
                DeliveryId = Guid.NewGuid().ToString(),
                DeliveryDate = deliveryDate,
                ActualVolume = actualVolume,
                DeliveryPoint = deliveryPoint,
                AllocationMethod = allocationMethod,
                RunTicketNumber = runTicketNumber
            };

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(ACTUAL_DELIVERY_TABLE, delivery);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record actual delivery {DeliveryId}: {Error}", delivery.DeliveryId, errorMessage);
                throw new InvalidOperationException($"Failed to save actual delivery: {errorMessage}");
            }

            _logger?.LogDebug("Recorded actual delivery {DeliveryId} in database", delivery.DeliveryId);
        }

        /// <summary>
        /// Records an actual delivery (synchronous wrapper).
        /// </summary>
        public void RecordActualDelivery(
            DateTime deliveryDate,
            decimal actualVolume,
            string deliveryPoint,
            string allocationMethod,
            string? runTicketNumber = null)
        {
            RecordActualDeliveryAsync(deliveryDate, actualVolume, deliveryPoint, allocationMethod, runTicketNumber).GetAwaiter().GetResult();
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

            var nomination = nominationData as Nomination;
            if (nomination == null)
                throw new ArgumentException($"Nomination {nominationId} not found.", nameof(nominationId));

            // Get period deliveries
            var deliveryFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DELIVERY_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "DELIVERY_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd")}
            };
            var deliveryResults = await dataSource.GetEntityAsync(ACTUAL_DELIVERY_TABLE, deliveryFilters);
            var periodDeliveries = deliveryResults?.Cast<ActualDelivery>().Where(d => d != null).ToList() ?? new List<ActualDelivery>();

            decimal actualVolume = periodDeliveries.Sum(d => d.ActualVolume);

            var imbalance = new OilImbalance
            {
                ImbalanceId = Guid.NewGuid().ToString(),
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                NominatedVolume = nomination.NominatedVolume,
                ActualVolume = actualVolume,
                TolerancePercentage = tolerancePercentage
            };

            // Determine status
            if (imbalance.IsWithinTolerance)
                imbalance.Status = ImbalanceStatus.Balanced;
            else if (imbalance.ImbalanceAmount > 0)
                imbalance.Status = ImbalanceStatus.OverDelivered;
            else
                imbalance.Status = ImbalanceStatus.UnderDelivered;

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(OIL_IMBALANCE_TABLE, imbalance);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to calculate imbalance {ImbalanceId}: {Error}", imbalance.ImbalanceId, errorMessage);
                throw new InvalidOperationException($"Failed to save imbalance: {errorMessage}");
            }

            _logger?.LogDebug("Calculated imbalance {ImbalanceId} in database", imbalance.ImbalanceId);
            return imbalance;
        }

        /// <summary>
        /// Calculates imbalance for a period (synchronous wrapper).
        /// </summary>
        public OilImbalance CalculateImbalance(
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
        public async Task<ImbalanceStatement> GenerateStatementAsync(
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
            var periodNominations = nominationResults?.Cast<Nomination>().Where(n => n != null).ToList() ?? new List<Nomination>();

            // Get deliveries
            var deliveryFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "DELIVERY_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "DELIVERY_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") }
            };
            var deliveryResults = await dataSource.GetEntityAsync(ACTUAL_DELIVERY_TABLE, deliveryFilters);
            var periodDeliveries = deliveryResults?.Cast<ActualDelivery>().Where(d => d != null).ToList() ?? new List<ActualDelivery>();

            // Get imbalances
            var imbalanceFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "PERIOD_START", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "PERIOD_END", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") }
            };
            var imbalanceResults = await dataSource.GetEntityAsync(OIL_IMBALANCE_TABLE, imbalanceFilters);
            var periodImbalances = imbalanceResults?.Cast<OilImbalance>().Where(i => i != null).ToList() ?? new List<OilImbalance>();

            var statement = new ImbalanceStatement
            {
                StatementId = Guid.NewGuid().ToString(),
                StatementPeriodStart = periodStart,
                StatementPeriodEnd = periodEnd,
                Nominations = new ImbalanceSummary
                {
                    TotalVolume = periodNominations.Sum(n => n.NominatedVolume),
                    TransactionCount = periodNominations.Count,
                    AverageDailyVolume = periodNominations.Count > 0
                        ? periodNominations.Sum(n => n.NominatedVolume) / (periodEnd - periodStart).Days
                        : 0
                },
                Actuals = new ImbalanceSummary
                {
                    TotalVolume = periodDeliveries.Sum(d => d.ActualVolume),
                    TransactionCount = periodDeliveries.Count,
                    AverageDailyVolume = periodDeliveries.Count > 0
                        ? periodDeliveries.Sum(d => d.ActualVolume) / (periodEnd - periodStart).Days
                        : 0
                },
                Imbalances = periodImbalances
            };

            // Save to database
            var result = dataSource.InsertEntity(IMBALANCE_STATEMENT_TABLE, statement);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to generate imbalance statement {StatementId}: {Error}", statement.StatementId, errorMessage);
                throw new InvalidOperationException($"Failed to save imbalance statement: {errorMessage}");
            }

            _logger?.LogDebug("Generated imbalance statement {StatementId} in database", statement.StatementId);
            return statement;
        }

        /// <summary>
        /// Generates an imbalance statement (synchronous wrapper).
        /// </summary>
        public ImbalanceStatement GenerateStatement(
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

            var imbalance = imbalanceData as OilImbalance;
            if (imbalance == null)
                throw new ArgumentException($"Imbalance {imbalanceId} not found.", nameof(imbalanceId));

            var reconciliation = new ImbalanceReconciliation
            {
                ReconciliationId = Guid.NewGuid().ToString(),
                ReconciliationDate = DateTime.Now,
                TotalImbalanceBefore = imbalance.ImbalanceAmount,
                Adjustments = adjustments,
                ReconciledBy = reconciledBy,
                Notes = notes
            };

            // Update imbalance status
            if (reconciliation.TotalImbalanceAfter == 0 || Math.Abs(reconciliation.TotalImbalanceAfter) <= imbalance.TolerancePercentage / 100m * imbalance.NominatedVolume)
                imbalance.Status = ImbalanceStatus.Reconciled;
            else
                imbalance.Status = ImbalanceStatus.PendingReconciliation;

            // Update imbalance in database
            dataSource.UpdateEntity(OIL_IMBALANCE_TABLE, imbalance);

            _logger?.LogDebug("Reconciled imbalance {ImbalanceId}", imbalanceId);
            return reconciliation;
        }

        /// <summary>
        /// Reconciles an imbalance (synchronous wrapper).
        /// </summary>
        public ImbalanceReconciliation ReconcileImbalance(
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
        public async Task<IEnumerable<OilImbalance>> GetImbalancesByStatusAsync(ImbalanceStatus status, string? connectionName = null)
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
                return Enumerable.Empty<OilImbalance>();

            return results.Cast<OilImbalance>().Where(i => i != null)!;
        }

        /// <summary>
        /// Gets imbalances by status (synchronous wrapper).
        /// </summary>
        public IEnumerable<OilImbalance> GetImbalancesByStatus(ImbalanceStatus status)
        {
            return GetImbalancesByStatusAsync(status).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets all imbalances requiring reconciliation.
        /// </summary>
        public async Task<IEnumerable<OilImbalance>> GetImbalancesRequiringReconciliationAsync(string? connectionName = null)
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
                return Enumerable.Empty<OilImbalance>();

            return results.Cast<OilImbalance>()
                .Where(i => i != null && (i.Status == ImbalanceStatus.PendingReconciliation || (!i.IsWithinTolerance && i.Status != ImbalanceStatus.Reconciled)))!;
        }

        /// <summary>
        /// Gets all imbalances requiring reconciliation (synchronous wrapper).
        /// </summary>
        public IEnumerable<OilImbalance> GetImbalancesRequiringReconciliation()
        {
            return GetImbalancesRequiringReconciliationAsync().GetAwaiter().GetResult();
        }

        #region Helper Methods - Model to Dictionary Conversion

        private Dictionary<string, object> ConvertProductionAvailsToDictionary(ProductionAvails avails)
        {
            return new Dictionary<string, object>
            {
                { "AVAILS_ID", avails.AvailsId },
                { "PERIOD_START", avails.PeriodStart },
                { "PERIOD_END", avails.PeriodEnd },
                { "ESTIMATED_PRODUCTION", avails.EstimatedProduction },
                { "AVAILABLE_FOR_DELIVERY", avails.AvailableForDelivery }
            };
        }

        private ProductionAvails? ConvertDictionaryToProductionAvails(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("AVAILS_ID"))
                return null;

            return new ProductionAvails
            {
                AvailsId = dict["AVAILS_ID"]?.ToString() ?? string.Empty,
                PeriodStart = dict.ContainsKey("PERIOD_START") && dict["PERIOD_START"] != DBNull.Value
                    ? Convert.ToDateTime(dict["PERIOD_START"])
                    : DateTime.MinValue,
                PeriodEnd = dict.ContainsKey("PERIOD_END") && dict["PERIOD_END"] != DBNull.Value
                    ? Convert.ToDateTime(dict["PERIOD_END"])
                    : DateTime.MinValue,
                EstimatedProduction = dict.ContainsKey("ESTIMATED_PRODUCTION") ? Convert.ToDecimal(dict["ESTIMATED_PRODUCTION"]) : 0m,
                AvailableForDelivery = dict.ContainsKey("AVAILABLE_FOR_DELIVERY") ? Convert.ToDecimal(dict["AVAILABLE_FOR_DELIVERY"]) : 0m
            };
        }

        private Dictionary<string, object> ConvertNominationToDictionary(Nomination nomination)
        {
            return new Dictionary<string, object>
            {
                { "NOMINATION_ID", nomination.NominationId },
                { "PERIOD_START", nomination.PeriodStart },
                { "PERIOD_END", nomination.PeriodEnd },
                { "NOMINATED_VOLUME", nomination.NominatedVolume },
                { "STATUS", nomination.Status.ToString() },
                { "APPROVAL_DATE", nomination.ApprovalDate ?? (object)DBNull.Value },
                { "APPROVED_BY", nomination.ApprovedBy ?? string.Empty }
            };
        }

        private Nomination? ConvertDictionaryToNomination(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("NOMINATION_ID"))
                return null;

            var nomination = new Nomination
            {
                NominationId = dict["NOMINATION_ID"]?.ToString() ?? string.Empty,
                PeriodStart = dict.ContainsKey("PERIOD_START") && dict["PERIOD_START"] != DBNull.Value
                    ? Convert.ToDateTime(dict["PERIOD_START"])
                    : DateTime.MinValue,
                PeriodEnd = dict.ContainsKey("PERIOD_END") && dict["PERIOD_END"] != DBNull.Value
                    ? Convert.ToDateTime(dict["PERIOD_END"])
                    : DateTime.MinValue,
                NominatedVolume = dict.ContainsKey("NOMINATED_VOLUME") ? Convert.ToDecimal(dict["NOMINATED_VOLUME"]) : 0m,
                ApprovalDate = dict.ContainsKey("APPROVAL_DATE") && dict["APPROVAL_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["APPROVAL_DATE"])
                    : null,
                ApprovedBy = dict.ContainsKey("APPROVED_BY") ? dict["APPROVED_BY"]?.ToString() : null
            };

            if (dict.ContainsKey("STATUS") && Enum.TryParse<NominationStatus>(dict["STATUS"]?.ToString(), out var status))
                nomination.Status = status;

            return nomination;
        }

        private Dictionary<string, object> ConvertActualDeliveryToDictionary(ActualDelivery delivery)
        {
            return new Dictionary<string, object>
            {
                { "DELIVERY_ID", delivery.DeliveryId },
                { "DELIVERY_DATE", delivery.DeliveryDate },
                { "ACTUAL_VOLUME", delivery.ActualVolume },
                { "DELIVERY_POINT", delivery.DeliveryPoint ?? string.Empty },
                { "ALLOCATION_METHOD", delivery.AllocationMethod ?? string.Empty },
                { "RUN_TICKET_NUMBER", delivery.RunTicketNumber ?? string.Empty }
            };
        }

        private ActualDelivery? ConvertDictionaryToActualDelivery(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("DELIVERY_ID"))
                return null;

            return new ActualDelivery
            {
                DeliveryId = dict["DELIVERY_ID"]?.ToString() ?? string.Empty,
                DeliveryDate = dict.ContainsKey("DELIVERY_DATE") && dict["DELIVERY_DATE"] != DBNull.Value
                    ? Convert.ToDateTime(dict["DELIVERY_DATE"])
                    : DateTime.MinValue,
                ActualVolume = dict.ContainsKey("ACTUAL_VOLUME") ? Convert.ToDecimal(dict["ACTUAL_VOLUME"]) : 0m,
                DeliveryPoint = dict.ContainsKey("DELIVERY_POINT") ? dict["DELIVERY_POINT"]?.ToString() : null,
                AllocationMethod = dict.ContainsKey("ALLOCATION_METHOD") ? dict["ALLOCATION_METHOD"]?.ToString() : null,
                RunTicketNumber = dict.ContainsKey("RUN_TICKET_NUMBER") ? dict["RUN_TICKET_NUMBER"]?.ToString() : null
            };
        }

        private Dictionary<string, object> ConvertOilImbalanceToDictionary(OilImbalance imbalance)
        {
            return new Dictionary<string, object>
            {
                { "IMBALANCE_ID", imbalance.ImbalanceId },
                { "PERIOD_START", imbalance.PeriodStart },
                { "PERIOD_END", imbalance.PeriodEnd },
                { "NOMINATED_VOLUME", imbalance.NominatedVolume },
                { "ACTUAL_VOLUME", imbalance.ActualVolume },
                { "TOLERANCE_PERCENTAGE", imbalance.TolerancePercentage },
                { "STATUS", imbalance.Status.ToString() }
            };
        }

        private OilImbalance? ConvertDictionaryToOilImbalance(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("IMBALANCE_ID"))
                return null;

            var imbalance = new OilImbalance
            {
                ImbalanceId = dict["IMBALANCE_ID"]?.ToString() ?? string.Empty,
                PeriodStart = dict.ContainsKey("PERIOD_START") && dict["PERIOD_START"] != DBNull.Value
                    ? Convert.ToDateTime(dict["PERIOD_START"])
                    : DateTime.MinValue,
                PeriodEnd = dict.ContainsKey("PERIOD_END") && dict["PERIOD_END"] != DBNull.Value
                    ? Convert.ToDateTime(dict["PERIOD_END"])
                    : DateTime.MinValue,
                NominatedVolume = dict.ContainsKey("NOMINATED_VOLUME") ? Convert.ToDecimal(dict["NOMINATED_VOLUME"]) : 0m,
                ActualVolume = dict.ContainsKey("ACTUAL_VOLUME") ? Convert.ToDecimal(dict["ACTUAL_VOLUME"]) : 0m,
                TolerancePercentage = dict.ContainsKey("TOLERANCE_PERCENTAGE") ? Convert.ToDecimal(dict["TOLERANCE_PERCENTAGE"]) : 2.0m
            };

            if (dict.ContainsKey("STATUS") && Enum.TryParse<ImbalanceStatus>(dict["STATUS"]?.ToString(), out var status))
                imbalance.Status = status;

            return imbalance;
        }

        private Dictionary<string, object> ConvertImbalanceStatementToDictionary(ImbalanceStatement statement)
        {
            return new Dictionary<string, object>
            {
                { "STATEMENT_ID", statement.StatementId },
                { "STATEMENT_PERIOD_START", statement.StatementPeriodStart },
                { "STATEMENT_PERIOD_END", statement.StatementPeriodEnd }
            };
        }

        private ImbalanceStatement? ConvertDictionaryToImbalanceStatement(Dictionary<string, object> dict)
        {
            if (dict == null || !dict.ContainsKey("STATEMENT_ID"))
                return null;

            return new ImbalanceStatement
            {
                StatementId = dict["STATEMENT_ID"]?.ToString() ?? string.Empty,
                StatementPeriodStart = dict.ContainsKey("STATEMENT_PERIOD_START") && dict["STATEMENT_PERIOD_START"] != DBNull.Value
                    ? Convert.ToDateTime(dict["STATEMENT_PERIOD_START"])
                    : DateTime.MinValue,
                StatementPeriodEnd = dict.ContainsKey("STATEMENT_PERIOD_END") && dict["STATEMENT_PERIOD_END"] != DBNull.Value
                    ? Convert.ToDateTime(dict["STATEMENT_PERIOD_END"])
                    : DateTime.MinValue
            };
        }

        #endregion
    }
}
