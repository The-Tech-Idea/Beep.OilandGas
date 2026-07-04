using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Trading;


namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for trading/exchange operations.
    /// </summary>
    public interface ITradingService
    {
        /// <summary>
        /// Registers an exchange contract.
        /// </summary>
        Task<EXCHANGE_CONTRACT> RegisterContractAsync(CreateExchangeContractRequest request, string userId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Gets an exchange contract by ID.
        /// </summary>
        Task<EXCHANGE_CONTRACT?> GetContractAsync(string contractId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Gets all exchange contracts.
        /// </summary>
        Task<List<EXCHANGE_CONTRACT>> GetContractsAsync(string connectionName = "PPDM39");
        
        /// <summary>
        /// Creates an exchange commitment.
        /// </summary>
        Task<EXCHANGE_COMMITMENT> CreateCommitmentAsync(CreateExchangeCommitmentRequest request, string userId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Gets commitments by contract.
        /// </summary>
        Task<List<EXCHANGE_COMMITMENT>> GetCommitmentsByContractAsync(string contractId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Records an exchange transaction.
        /// </summary>
        Task<EXCHANGE_TRANSACTION> RecordTransactionAsync(CreateExchangeTransactionRequest request, string userId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Gets transactions by contract.
        /// </summary>
        Task<List<EXCHANGE_TRANSACTION>> GetTransactionsByContractAsync(string contractId, DateTime? startDate, DateTime? endDate, string connectionName = "PPDM39");
        
        /// <summary>
        /// Settles an exchange contract.
        /// </summary>
        Task<ExchangeSettlementResult> SettleExchangeAsync(string contractId, DateTime settlementDate, string userId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Reconciles an exchange contract.
        /// </summary>
        Task<ExchangeReconciliationResult> ReconcileExchangeAsync(string contractId, DateTime reconciliationDate, string userId, string connectionName = "PPDM39");
        
        /// <summary>
        /// Generates an exchange statement.
        /// </summary>
        Task<ExchangeStatement> GenerateStatementAsync(string contractId, DateTime statementDate, string connectionName = "PPDM39");
    }
}




