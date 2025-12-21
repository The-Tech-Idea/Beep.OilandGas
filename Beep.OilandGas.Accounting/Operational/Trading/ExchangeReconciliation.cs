using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Accounting.Operational.Trading
{
    /// <summary>
    /// Represents exchange reconciliation results.
    /// </summary>
    public class ExchangeReconciliation
    {
        /// <summary>
        /// Gets or sets the reconciliation identifier.
        /// </summary>
        public string ReconciliationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reconciliation date.
        /// </summary>
        public DateTime ReconciliationDate { get; set; }

        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        public string ContractId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the operator's statement.
        /// </summary>
        public ExchangeStatement? OperatorStatement { get; set; }

        /// <summary>
        /// Gets or sets the counterparty's statement.
        /// </summary>
        public ExchangeStatement? CounterpartyStatement { get; set; }

        /// <summary>
        /// Gets or sets the reconciliation differences.
        /// </summary>
        public ExchangeReconciliationDifferences Differences { get; set; } = new();

        /// <summary>
        /// Gets or sets whether the reconciliation is balanced.
        /// </summary>
        public bool IsBalanced { get; set; }

        /// <summary>
        /// Gets or sets the reconciliation status.
        /// </summary>
        public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Pending;
    }

    /// <summary>
    /// Represents reconciliation differences.
    /// </summary>
    public class ExchangeReconciliationDifferences
    {
        /// <summary>
        /// Gets or sets the volume difference in barrels.
        /// </summary>
        public decimal VolumeDifference { get; set; }

        /// <summary>
        /// Gets or sets the value difference.
        /// </summary>
        public decimal ValueDifference { get; set; }

        /// <summary>
        /// Gets or sets the transaction count difference.
        /// </summary>
        public int TransactionCountDifference { get; set; }

        /// <summary>
        /// Gets or sets the list of unmatched transactions.
        /// </summary>
        public List<string> UnmatchedTransactions { get; set; } = new();
    }

    /// <summary>
    /// Reconciliation status.
    /// </summary>
    public enum ReconciliationStatus
    {
        /// <summary>
        /// Pending reconciliation.
        /// </summary>
        Pending,

        /// <summary>
        /// In progress.
        /// </summary>
        InProgress,

        /// <summary>
        /// Balanced (no differences).
        /// </summary>
        Balanced,

        /// <summary>
        /// Differences found.
        /// </summary>
        DifferencesFound,

        /// <summary>
        /// Resolved.
        /// </summary>
        Resolved
    }

    /// <summary>
    /// Provides exchange reconciliation functionality.
    /// </summary>
    public static class ExchangeReconciliationEngine
    {
        /// <summary>
        /// Performs reconciliation between operator and counterparty statements.
        /// </summary>
        public static ExchangeReconciliation Reconcile(
            ExchangeStatement operatorStatement,
            ExchangeStatement counterpartyStatement)
        {
            if (operatorStatement == null)
                throw new ArgumentNullException(nameof(operatorStatement));

            if (counterpartyStatement == null)
                throw new ArgumentNullException(nameof(counterpartyStatement));

            var reconciliation = new ExchangeReconciliation
            {
                ReconciliationId = Guid.NewGuid().ToString(),
                ReconciliationDate = DateTime.Now,
                ContractId = operatorStatement.ContractId,
                OperatorStatement = operatorStatement,
                CounterpartyStatement = counterpartyStatement
            };

            // Calculate differences
            reconciliation.Differences = new ExchangeReconciliationDifferences
            {
                VolumeDifference = operatorStatement.NetPosition.NetVolume - counterpartyStatement.NetPosition.NetVolume,
                ValueDifference = operatorStatement.NetPosition.NetValue - counterpartyStatement.NetPosition.NetValue,
                TransactionCountDifference = operatorStatement.Transactions.Count - counterpartyStatement.Transactions.Count
            };

            // Check if balanced
            decimal tolerance = 0.01m; // 0.01 barrel tolerance
            reconciliation.IsBalanced = Math.Abs(reconciliation.Differences.VolumeDifference) <= tolerance &&
                                       Math.Abs(reconciliation.Differences.ValueDifference) <= 0.01m;

            // Find unmatched transactions
            reconciliation.Differences.UnmatchedTransactions = FindUnmatchedTransactions(
                operatorStatement.Transactions,
                counterpartyStatement.Transactions);

            // Set status
            reconciliation.Status = reconciliation.IsBalanced 
                ? ReconciliationStatus.Balanced 
                : ReconciliationStatus.DifferencesFound;

            return reconciliation;
        }

        /// <summary>
        /// Finds unmatched transactions between two statement sets.
        /// </summary>
        private static List<string> FindUnmatchedTransactions(
            List<ExchangeTransaction> operatorTransactions,
            List<ExchangeTransaction> counterpartyTransactions)
        {
            var unmatched = new List<string>();

            // Find transactions in operator statement not in counterparty
            foreach (var opTransaction in operatorTransactions)
            {
                bool found = counterpartyTransactions.Any(cp => 
                    cp.TransactionId == opTransaction.TransactionId ||
                    (cp.TransactionDate == opTransaction.TransactionDate &&
                     Math.Abs(cp.ReceiptVolume - opTransaction.ReceiptVolume) < 0.01m &&
                     Math.Abs(cp.DeliveryVolume - opTransaction.DeliveryVolume) < 0.01m));

                if (!found)
                {
                    unmatched.Add($"Operator transaction {opTransaction.TransactionId} not found in counterparty statement");
                }
            }

            // Find transactions in counterparty statement not in operator
            foreach (var cpTransaction in counterpartyTransactions)
            {
                bool found = operatorTransactions.Any(op => 
                    op.TransactionId == cpTransaction.TransactionId ||
                    (op.TransactionDate == cpTransaction.TransactionDate &&
                     Math.Abs(op.ReceiptVolume - cpTransaction.ReceiptVolume) < 0.01m &&
                     Math.Abs(op.DeliveryVolume - cpTransaction.DeliveryVolume) < 0.01m));

                if (!found)
                {
                    unmatched.Add($"Counterparty transaction {cpTransaction.TransactionId} not found in operator statement");
                }
            }

            return unmatched;
        }
    }
}

