using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Trading;
using Beep.OilandGas.Models.DTOs.Trading;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ProductionAccounting.Trading
{
    /// <summary>
    /// Service for managing exchange contracts, commitments, and transactions.
    /// Uses PPDMGenericRepository for database operations.
    /// </summary>
    public class TradingService : ITradingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly ILogger<TradingService>? _logger;
        private readonly string _connectionName;
        private const string EXCHANGE_CONTRACT_TABLE = "EXCHANGE_CONTRACT";
        private const string EXCHANGE_COMMITMENT_TABLE = "EXCHANGE_COMMITMENT";
        private const string EXCHANGE_TRANSACTION_TABLE = "EXCHANGE_TRANSACTION";

        public TradingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            ILogger<TradingService>? logger = null,
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
        public async Task<EXCHANGE_CONTRACT> RegisterContractAsync(CreateExchangeContractRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var connName = connectionName ?? _connectionName;
            var repo = await GetContractRepositoryAsync(connName);

            var contract = new EXCHANGE_CONTRACT
            {
                EXCHANGE_CONTRACT_ID = Guid.NewGuid().ToString(),
                CONTRACT_NUMBER = request.ContractNumber,
                COUNTERPARTY_BA_ID = request.CounterpartyBaId,
                CONTRACT_TYPE = request.ContractType,
                COMMODITY_TYPE = request.CommodityType,
                CONTRACT_DATE = request.ContractDate,
                EFFECTIVE_DATE = request.EffectiveDate,
                EXPIRY_DATE = request.ExpiryDate,
                STATUS = "Active",
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y"
            };

            if (contract is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await repo.InsertAsync(contract);
            _logger?.LogDebug("Registered exchange contract {ContractNumber}", request.ContractNumber);

            return contract;
        }

        /// <summary>
        /// Gets an exchange contract by ID.
        /// </summary>
        public async Task<EXCHANGE_CONTRACT?> GetContractAsync(string contractId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(contractId))
                return null;

            var connName = connectionName ?? _connectionName;
            var repo = await GetContractRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "EXCHANGE_CONTRACT_ID", Operator = "=", FilterValue = contractId }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<EXCHANGE_CONTRACT>().FirstOrDefault();
        }

        /// <summary>
        /// Gets all exchange contracts.
        /// </summary>
        public async Task<List<EXCHANGE_CONTRACT>> GetContractsAsync(string? connectionName = null)
        {
            var connName = connectionName ?? _connectionName;
            var repo = await GetContractRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<EXCHANGE_CONTRACT>().OrderBy(c => c.CONTRACT_DATE).ToList();
        }

        /// <summary>
        /// Creates an exchange commitment.
        /// </summary>
        public async Task<EXCHANGE_COMMITMENT> CreateCommitmentAsync(CreateExchangeCommitmentRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.ExchangeContractId))
                throw new ArgumentException("Exchange Contract ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var contract = await GetContractAsync(request.ExchangeContractId, connName);

            if (contract == null)
                throw new InvalidOperationException($"Exchange contract {request.ExchangeContractId} not found.");

            var commitmentRepo = await GetCommitmentRepositoryAsync(connName);

            var commitment = new EXCHANGE_COMMITMENT
            {
                EXCHANGE_COMMITMENT_ID = Guid.NewGuid().ToString(),
                EXCHANGE_CONTRACT_ID = request.ExchangeContractId,
                COMMITMENT_TYPE = request.CommitmentType,
                COMMITTED_VOLUME = request.CommittedVolume,
                ACTUAL_VOLUME_DELIVERED = 0m,
                DELIVERY_PERIOD_START = request.DeliveryPeriodStart,
                DELIVERY_PERIOD_END = request.DeliveryPeriodEnd,
                STATUS = "Pending",
                ACTIVE_IND = "Y"
            };

            if (commitment is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await commitmentRepo.InsertAsync(commitment);
            _logger?.LogDebug("Created exchange commitment for contract {ContractId}", request.ExchangeContractId);

            return commitment;
        }

        /// <summary>
        /// Gets commitments by contract.
        /// </summary>
        public async Task<List<EXCHANGE_COMMITMENT>> GetCommitmentsByContractAsync(string contractId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(contractId))
                return new List<EXCHANGE_COMMITMENT>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetCommitmentRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "EXCHANGE_CONTRACT_ID", Operator = "=", FilterValue = contractId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var results = await repo.GetAsync(filters);
            return results.Cast<EXCHANGE_COMMITMENT>().OrderBy(c => c.DELIVERY_PERIOD_START).ToList();
        }

        /// <summary>
        /// Records an exchange transaction.
        /// </summary>
        public async Task<EXCHANGE_TRANSACTION> RecordTransactionAsync(CreateExchangeTransactionRequest request, string userId, string? connectionName = null)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.ExchangeContractId))
                throw new ArgumentException("Exchange Contract ID is required.", nameof(request));

            var connName = connectionName ?? _connectionName;
            var contract = await GetContractAsync(request.ExchangeContractId, connName);

            if (contract == null)
                throw new InvalidOperationException($"Exchange contract {request.ExchangeContractId} not found.");

            var transactionRepo = await GetTransactionRepositoryAsync(connName);

            var transaction = new EXCHANGE_TRANSACTION
            {
                EXCHANGE_TRANSACTION_ID = Guid.NewGuid().ToString(),
                EXCHANGE_CONTRACT_ID = request.ExchangeContractId,
                TRANSACTION_DATE = request.TransactionDate,
                RECEIPT_VOLUME = request.ReceiptVolume,
                RECEIPT_PRICE = request.ReceiptPrice,
                RECEIPT_LOCATION = request.ReceiptLocation,
                DELIVERY_VOLUME = request.DeliveryVolume,
                DELIVERY_PRICE = request.DeliveryPrice,
                DELIVERY_LOCATION = request.DeliveryLocation,
                STATUS = "Recorded",
                DESCRIPTION = request.Description,
                ACTIVE_IND = "Y"
            };

            if (transaction is IPPDMEntity ppdmEntity)
            {
                await _commonColumnHandler.SetCommonColumnsForCreateAsync(ppdmEntity, userId, connName);
            }

            await transactionRepo.InsertAsync(transaction);

            // Update commitment if applicable
            var commitments = await GetCommitmentsByContractAsync(request.ExchangeContractId, connName);
            var applicableCommitment = commitments.FirstOrDefault(c =>
                c.DELIVERY_PERIOD_START <= request.TransactionDate &&
                c.DELIVERY_PERIOD_END >= request.TransactionDate);

            if (applicableCommitment != null)
            {
                applicableCommitment.ACTUAL_VOLUME_DELIVERED = (applicableCommitment.ACTUAL_VOLUME_DELIVERED ?? 0m) + request.DeliveryVolume;

                if (applicableCommitment.ACTUAL_VOLUME_DELIVERED >= applicableCommitment.COMMITTED_VOLUME)
                    applicableCommitment.STATUS = "Fulfilled";
                else if (applicableCommitment.ACTUAL_VOLUME_DELIVERED > 0)
                    applicableCommitment.STATUS = "PartiallyFulfilled";

                if (applicableCommitment is IPPDMEntity commitmentPpdmEntity)
                {
                    await _commonColumnHandler.SetCommonColumnsForUpdateAsync(commitmentPpdmEntity, userId, connName);
                }

                var commitmentRepo = await GetCommitmentRepositoryAsync(connName);
                await commitmentRepo.UpdateAsync(applicableCommitment);
            }

            _logger?.LogDebug("Recorded exchange transaction for contract {ContractId}", request.ExchangeContractId);

            return transaction;
        }

        /// <summary>
        /// Gets transactions by contract.
        /// </summary>
        public async Task<List<EXCHANGE_TRANSACTION>> GetTransactionsByContractAsync(string contractId, DateTime? startDate, DateTime? endDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(contractId))
                return new List<EXCHANGE_TRANSACTION>();

            var connName = connectionName ?? _connectionName;
            var repo = await GetTransactionRepositoryAsync(connName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "EXCHANGE_CONTRACT_ID", Operator = "=", FilterValue = contractId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            if (startDate.HasValue)
                filters.Add(new AppFilter { FieldName = "TRANSACTION_DATE", Operator = ">=", FilterValue = startDate.Value });
            if (endDate.HasValue)
                filters.Add(new AppFilter { FieldName = "TRANSACTION_DATE", Operator = "<=", FilterValue = endDate.Value });

            var results = await repo.GetAsync(filters);
            return results.Cast<EXCHANGE_TRANSACTION>().OrderBy(t => t.TRANSACTION_DATE).ToList();
        }

        /// <summary>
        /// Settles an exchange contract.
        /// </summary>
        public async Task<ExchangeSettlementResult> SettleExchangeAsync(string contractId, DateTime settlementDate, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(contractId))
                throw new ArgumentException("Contract ID is required.", nameof(contractId));

            var connName = connectionName ?? _connectionName;
            var transactions = await GetTransactionsByContractAsync(contractId, null, settlementDate, connName);

            decimal totalReceiptValue = transactions.Sum(t => (t.RECEIPT_VOLUME ?? 0m) * (t.RECEIPT_PRICE ?? 0m));
            decimal totalDeliveryValue = transactions.Sum(t => (t.DELIVERY_VOLUME ?? 0m) * (t.DELIVERY_PRICE ?? 0m));
            decimal netSettlementAmount = totalReceiptValue - totalDeliveryValue;

            return new ExchangeSettlementResult
            {
                ExchangeContractId = contractId,
                SettlementDate = settlementDate,
                TotalReceiptValue = totalReceiptValue,
                TotalDeliveryValue = totalDeliveryValue,
                NetSettlementAmount = netSettlementAmount,
                TransactionCount = transactions.Count,
                IsSettled = true
            };
        }

        /// <summary>
        /// Reconciles an exchange contract.
        /// </summary>
        public async Task<ExchangeReconciliationResult> ReconcileExchangeAsync(string contractId, DateTime reconciliationDate, string userId, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(contractId))
                throw new ArgumentException("Contract ID is required.", nameof(contractId));

            var connName = connectionName ?? _connectionName;
            var transactions = await GetTransactionsByContractAsync(contractId, null, reconciliationDate, connName);

            decimal bookReceiptVolume = transactions.Sum(t => t.RECEIPT_VOLUME ?? 0m);
            decimal bookDeliveryVolume = transactions.Sum(t => t.DELIVERY_VOLUME ?? 0m);

            // For now, physical volumes are same as book (would come from physical inventory in real scenario)
            decimal physicalReceiptVolume = bookReceiptVolume;
            decimal physicalDeliveryVolume = bookDeliveryVolume;

            return new ExchangeReconciliationResult
            {
                ExchangeContractId = contractId,
                ReconciliationDate = reconciliationDate,
                BookReceiptVolume = bookReceiptVolume,
                BookDeliveryVolume = bookDeliveryVolume,
                PhysicalReceiptVolume = physicalReceiptVolume,
                PhysicalDeliveryVolume = physicalDeliveryVolume,
                ReceiptVariance = physicalReceiptVolume - bookReceiptVolume,
                DeliveryVariance = physicalDeliveryVolume - bookDeliveryVolume,
                RequiresAdjustment = Math.Abs(physicalReceiptVolume - bookReceiptVolume) > 0.01m ||
                    Math.Abs(physicalDeliveryVolume - bookDeliveryVolume) > 0.01m
            };
        }

        /// <summary>
        /// Generates an exchange statement.
        /// </summary>
        public async Task<Beep.OilandGas.Models.Data.Trading.ExchangeStatement> GenerateStatementAsync(string contractId, DateTime statementDate, string? connectionName = null)
        {
            if (string.IsNullOrEmpty(contractId))
                throw new ArgumentException("Contract ID is required.", nameof(contractId));

            var connName = connectionName ?? _connectionName;
            var periodStart = new DateTime(statementDate.Year, statementDate.Month, 1);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);

            var transactions = await GetTransactionsByContractAsync(contractId, periodStart, periodEnd, connName);

            // Convert to ExchangeTransaction models for statement generation
            var exchangeTransactions = transactions.Select(t => new Beep.OilandGas.Models.Data.Trading.ExchangeTransaction
            {
                TransactionId = t.EXCHANGE_TRANSACTION_ID ?? string.Empty,
                ContractId = t.EXCHANGE_CONTRACT_ID ?? string.Empty,
                TransactionDate = t.TRANSACTION_DATE ?? DateTime.MinValue,
                ReceiptVolume = t.RECEIPT_VOLUME ?? 0m,
                ReceiptPrice = t.RECEIPT_PRICE ?? 0m,
                ReceiptLocation = t.RECEIPT_LOCATION ?? string.Empty,
                DeliveryVolume = t.DELIVERY_VOLUME ?? 0m,
                DeliveryPrice = t.DELIVERY_PRICE ?? 0m,
                DeliveryLocation = t.DELIVERY_LOCATION ?? string.Empty
            }).ToList();

            return Beep.OilandGas.Models.Data.Trading.ExchangeStatementGenerator.GenerateStatement(contractId, periodStart, periodEnd, exchangeTransactions);
        }

        // Repository helper methods
        private async Task<PPDMGenericRepository> GetContractRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(EXCHANGE_CONTRACT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Trading.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(EXCHANGE_CONTRACT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, EXCHANGE_CONTRACT_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetCommitmentRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(EXCHANGE_COMMITMENT_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Trading.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(EXCHANGE_COMMITMENT);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, EXCHANGE_COMMITMENT_TABLE,
                null);
        }

        private async Task<PPDMGenericRepository> GetTransactionRepositoryAsync(string connectionName)
        {
            var metadata = await _metadata.GetTableMetadataAsync(EXCHANGE_TRANSACTION_TABLE);
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.Trading.{metadata.EntityTypeName}");

            if (entityType == null)
            {
                entityType = typeof(EXCHANGE_TRANSACTION);
            }

            return new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                entityType, connectionName, EXCHANGE_TRANSACTION_TABLE,
                null);
        }
    }
}
