namespace Beep.OilandGas.ProductionAccounting.Trading
{
    /// <summary>
    /// Manages exchange contracts, commitments, and transactions.
    /// Uses database access via IDataSource instead of in-memory dictionaries.
    /// </summary>
    public class TradingManager
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<TradingManager>? _logger;
        private readonly string _connectionName;
        private const string EXCHANGE_CONTRACT_TABLE = "EXCHANGE_CONTRACT";
        private const string EXCHANGE_COMMITMENT_TABLE = "EXCHANGE_COMMITMENT";
        private const string EXCHANGE_TRANSACTION_TABLE = "EXCHANGE_TRANSACTION";

        public TradingManager(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<TradingManager>? logger = null,
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
        /// Registers an exchange contract.
        /// </summary>
        public async Task RegisterContractAsync(ExchangeContract contract, string userId = "system", string? connectionName = null)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (string.IsNullOrEmpty(contract.ContractId))
                throw new ArgumentException("Contract ID cannot be null or empty.", nameof(contract));

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(EXCHANGE_CONTRACT_TABLE, contract);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to register exchange contract {ContractId}: {Error}", contract.ContractId, errorMessage);
                throw new InvalidOperationException($"Failed to save exchange contract: {errorMessage}");
            }

            _logger?.LogDebug("Registered exchange contract {ContractId} to database", contract.ContractId);
        }

        /// <summary>
        /// Registers an exchange contract (synchronous wrapper).
        /// </summary>
        public void RegisterContract(ExchangeContract contract)
        {
            RegisterContractAsync(contract).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets an exchange contract by ID.
        /// </summary>
        public async Task<ExchangeContract?> GetContractAsync(string contractId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(contractId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CONTRACT_ID", Operator = "=", FilterValue = contractId }
            };

            var results = await dataSource.GetEntityAsync(EXCHANGE_CONTRACT_TABLE, filters);
            var contractData = results?.FirstOrDefault();
            
            if (contractData == null)
                return null;

            return contractData as ExchangeContract;
        }

        /// <summary>
        /// Gets an exchange contract by ID (synchronous wrapper).
        /// </summary>
        public ExchangeContract? GetContract(string contractId)
        {
            return GetContractAsync(contractId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates an exchange commitment.
        /// </summary>
        public async Task<ExchangeCommitment> CreateCommitmentAsync(
            string contractId,
            ExchangeCommitmentType commitmentType,
            decimal committedVolume,
            DateTime deliveryPeriodStart,
            DateTime deliveryPeriodEnd,
            string userId = "system",
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(contractId))
                throw new ArgumentException("Contract ID cannot be null or empty.", nameof(contractId));

            // Verify contract exists
            var contract = await GetContractAsync(contractId, connectionName);
            if (contract == null)
                throw new ArgumentException($"Contract {contractId} not found.", nameof(contractId));

            var commitment = new ExchangeCommitment
            {
                CommitmentId = Guid.NewGuid().ToString(),
                ContractId = contractId,
                CommitmentType = commitmentType,
                CommittedVolume = committedVolume,
                DeliveryPeriodStart = deliveryPeriodStart,
                DeliveryPeriodEnd = deliveryPeriodEnd,
                Status = ExchangeCommitmentStatus.Pending
            };

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(EXCHANGE_COMMITMENT_TABLE, commitment);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to create exchange commitment {CommitmentId}: {Error}", commitment.CommitmentId, errorMessage);
                throw new InvalidOperationException($"Failed to save exchange commitment: {errorMessage}");
            }

            _logger?.LogDebug("Created exchange commitment {CommitmentId} in database", commitment.CommitmentId);
            return commitment;
        }

        /// <summary>
        /// Creates an exchange commitment (synchronous wrapper).
        /// </summary>
        public ExchangeCommitment CreateCommitment(
            string contractId,
            ExchangeCommitmentType commitmentType,
            decimal committedVolume,
            DateTime deliveryPeriodStart,
            DateTime deliveryPeriodEnd)
        {
            return CreateCommitmentAsync(contractId, commitmentType, committedVolume, deliveryPeriodStart, deliveryPeriodEnd).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets an exchange commitment by ID.
        /// </summary>
        public async Task<ExchangeCommitment?> GetCommitmentAsync(string commitmentId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(commitmentId))
                return null;

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "COMMITMENT_ID", Operator = "=", FilterValue = commitmentId }
            };

            var results = await dataSource.GetEntityAsync(EXCHANGE_COMMITMENT_TABLE, filters);
            var commitmentData = results?.FirstOrDefault();
            
            if (commitmentData == null)
                return null;

            return commitmentData as ExchangeCommitment;
        }

        /// <summary>
        /// Gets an exchange commitment by ID (synchronous wrapper).
        /// </summary>
        public ExchangeCommitment? GetCommitment(string commitmentId)
        {
            return GetCommitmentAsync(commitmentId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Records an exchange transaction.
        /// </summary>
        public async Task RecordTransactionAsync(ExchangeTransaction transaction, string userId = "system", string? connectionName = null)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (string.IsNullOrEmpty(transaction.TransactionId))
                transaction.TransactionId = Guid.NewGuid().ToString();

            // Save to database
            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var result = dataSource.InsertEntity(EXCHANGE_TRANSACTION_TABLE, transaction);
            
            if (result != null && result.Errors != null && result.Errors.Count > 0)
            {
                var errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger?.LogError("Failed to record exchange transaction {TransactionId}: {Error}", transaction.TransactionId, errorMessage);
                throw new InvalidOperationException($"Failed to save exchange transaction: {errorMessage}");
            }

            // Update commitment if applicable
            var commitmentFilters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CONTRACT_ID", Operator = "=", FilterValue = transaction.ContractId },
                new AppFilter { FieldName = "DELIVERY_PERIOD_START", Operator = "<=", FilterValue = transaction.TransactionDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "DELIVERY_PERIOD_END", Operator = ">=", FilterValue = transaction.TransactionDate.ToString("yyyy-MM-dd") }
            };

            var commitments = await dataSource.GetEntityAsync(EXCHANGE_COMMITMENT_TABLE, commitmentFilters);
            if (commitments != null && commitments.Any())
            {
                var commitmentData = commitments.First();
                var commitment = commitmentData as ExchangeCommitment;
                if (commitment != null)
                {
                    commitment.ActualVolumeDelivered += transaction.DeliveryVolume;
                    
                    if (commitment.ActualVolumeDelivered >= commitment.CommittedVolume)
                        commitment.Status = ExchangeCommitmentStatus.Fulfilled;
                    else if (commitment.ActualVolumeDelivered > 0)
                        commitment.Status = ExchangeCommitmentStatus.PartiallyFulfilled;

                    // Update commitment in database
                    var updateFilters = new List<AppFilter>
                    {
                        new AppFilter { FieldName = "COMMITMENT_ID", Operator = "=", FilterValue = commitment.CommitmentId }
                    };
                    dataSource.UpdateEntity(EXCHANGE_COMMITMENT_TABLE, commitment);
                }
            }

            _logger?.LogDebug("Recorded exchange transaction {TransactionId} in database", transaction.TransactionId);
        }

        /// <summary>
        /// Records an exchange transaction (synchronous wrapper).
        /// </summary>
        public void RecordTransaction(ExchangeTransaction transaction)
        {
            RecordTransactionAsync(transaction).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets transactions for a contract in a date range.
        /// </summary>
        public async Task<IEnumerable<ExchangeTransaction>> GetTransactionsAsync(
            string contractId,
            DateTime startDate,
            DateTime endDate,
            string? connectionName = null)
        {
            if (string.IsNullOrEmpty(contractId))
                return Enumerable.Empty<ExchangeTransaction>();

            var connName = connectionName ?? _connectionName;
            var dataSource = _editor.GetDataSource(connName);
            if (dataSource == null)
                throw new InvalidOperationException($"DataSource not found for connection: {connName}");

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "CONTRACT_ID", Operator = "=", FilterValue = contractId },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = startDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = endDate.ToString("yyyy-MM-dd") }
            };

            var results = await dataSource.GetEntityAsync(EXCHANGE_TRANSACTION_TABLE, filters);
            if (results == null || !results.Any())
                return Enumerable.Empty<ExchangeTransaction>();

            return results.Cast<ExchangeTransaction>()
                .Where(t => t != null && t.TransactionDate >= startDate && t.TransactionDate <= endDate)!;
        }

        /// <summary>
        /// Gets transactions for a contract in a date range (synchronous wrapper).
        /// </summary>
        public IEnumerable<ExchangeTransaction> GetTransactions(
            string contractId,
            DateTime startDate,
            DateTime endDate)
        {
            return GetTransactionsAsync(contractId, startDate, endDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generates an exchange statement.
        /// </summary>
        public async Task<ExchangeStatement> GenerateStatementAsync(
            string contractId,
            DateTime periodStart,
            DateTime periodEnd,
            string? connectionName = null)
        {
            var contractTransactions = (await GetTransactionsAsync(contractId, periodStart, periodEnd, connectionName)).ToList();
            return ExchangeStatementGenerator.GenerateStatement(contractId, periodStart, periodEnd, contractTransactions);
        }

        /// <summary>
        /// Generates an exchange statement (synchronous wrapper).
        /// </summary>
        public ExchangeStatement GenerateStatement(
            string contractId,
            DateTime periodStart,
            DateTime periodEnd)
        {
            return GenerateStatementAsync(contractId, periodStart, periodEnd).GetAwaiter().GetResult();
        }

    }
}
