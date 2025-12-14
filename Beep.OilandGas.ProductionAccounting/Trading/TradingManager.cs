using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Trading
{
    /// <summary>
    /// Manages exchange contracts, commitments, and transactions.
    /// </summary>
    public class TradingManager
    {
        private readonly Dictionary<string, ExchangeContract> contracts = new();
        private readonly Dictionary<string, ExchangeCommitment> commitments = new();
        private readonly Dictionary<string, ExchangeTransaction> transactions = new();

        /// <summary>
        /// Registers an exchange contract.
        /// </summary>
        public void RegisterContract(ExchangeContract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (string.IsNullOrEmpty(contract.ContractId))
                throw new ArgumentException("Contract ID cannot be null or empty.", nameof(contract));

            contracts[contract.ContractId] = contract;
        }

        /// <summary>
        /// Gets an exchange contract by ID.
        /// </summary>
        public ExchangeContract? GetContract(string contractId)
        {
            return contracts.TryGetValue(contractId, out var contract) ? contract : null;
        }

        /// <summary>
        /// Creates an exchange commitment.
        /// </summary>
        public ExchangeCommitment CreateCommitment(
            string contractId,
            ExchangeCommitmentType commitmentType,
            decimal committedVolume,
            DateTime deliveryPeriodStart,
            DateTime deliveryPeriodEnd)
        {
            if (string.IsNullOrEmpty(contractId))
                throw new ArgumentException("Contract ID cannot be null or empty.", nameof(contractId));

            if (!contracts.ContainsKey(contractId))
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

            commitments[commitment.CommitmentId] = commitment;
            return commitment;
        }

        /// <summary>
        /// Gets an exchange commitment by ID.
        /// </summary>
        public ExchangeCommitment? GetCommitment(string commitmentId)
        {
            return commitments.TryGetValue(commitmentId, out var commitment) ? commitment : null;
        }

        /// <summary>
        /// Records an exchange transaction.
        /// </summary>
        public void RecordTransaction(ExchangeTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            if (string.IsNullOrEmpty(transaction.TransactionId))
                transaction.TransactionId = Guid.NewGuid().ToString();

            transactions[transaction.TransactionId] = transaction;

            // Update commitment if applicable
            var commitment = commitments.Values
                .FirstOrDefault(c => c.ContractId == transaction.ContractId &&
                                   transaction.TransactionDate >= c.DeliveryPeriodStart &&
                                   transaction.TransactionDate <= c.DeliveryPeriodEnd);

            if (commitment != null)
            {
                commitment.ActualVolumeDelivered += transaction.DeliveryVolume;
                
                if (commitment.ActualVolumeDelivered >= commitment.CommittedVolume)
                    commitment.Status = ExchangeCommitmentStatus.Fulfilled;
                else if (commitment.ActualVolumeDelivered > 0)
                    commitment.Status = ExchangeCommitmentStatus.PartiallyFulfilled;
            }
        }

        /// <summary>
        /// Gets transactions for a contract in a date range.
        /// </summary>
        public IEnumerable<ExchangeTransaction> GetTransactions(
            string contractId,
            DateTime startDate,
            DateTime endDate)
        {
            return transactions.Values
                .Where(t => t.ContractId == contractId &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate <= endDate);
        }

        /// <summary>
        /// Generates an exchange statement.
        /// </summary>
        public ExchangeStatement GenerateStatement(
            string contractId,
            DateTime periodStart,
            DateTime periodEnd)
        {
            var contractTransactions = GetTransactions(contractId, periodStart, periodEnd).ToList();
            return ExchangeStatementGenerator.GenerateStatement(contractId, periodStart, periodEnd, contractTransactions);
        }
    }
}

