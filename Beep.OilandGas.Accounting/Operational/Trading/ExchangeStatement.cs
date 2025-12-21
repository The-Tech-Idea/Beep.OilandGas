using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Accounting.Operational.Trading
{
    /// <summary>
    /// Represents an exchange statement.
    /// </summary>
    public class ExchangeStatement
    {
        /// <summary>
        /// Gets or sets the statement identifier.
        /// </summary>
        public string StatementId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the statement period start date.
        /// </summary>
        public DateTime StatementPeriodStart { get; set; }

        /// <summary>
        /// Gets or sets the statement period end date.
        /// </summary>
        public DateTime StatementPeriodEnd { get; set; }

        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        public string ContractId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the receipts summary.
        /// </summary>
        public ExchangeSummary Receipts { get; set; } = new();

        /// <summary>
        /// Gets or sets the deliveries summary.
        /// </summary>
        public ExchangeSummary Deliveries { get; set; } = new();

        /// <summary>
        /// Gets or sets the net position.
        /// </summary>
        public ExchangeNetPosition NetPosition { get; set; } = new();

        /// <summary>
        /// Gets or sets the transactions.
        /// </summary>
        public List<ExchangeTransaction> Transactions { get; set; } = new();
    }

    /// <summary>
    /// Represents an exchange summary.
    /// </summary>
    public class ExchangeSummary
    {
        /// <summary>
        /// Gets or sets the total volume in barrels.
        /// </summary>
        public decimal TotalVolume { get; set; }

        /// <summary>
        /// Gets or sets the average price per barrel.
        /// </summary>
        public decimal AveragePrice { get; set; }

        /// <summary>
        /// Gets or sets the total value.
        /// </summary>
        public decimal TotalValue => TotalVolume * AveragePrice;

        /// <summary>
        /// Gets or sets the number of transactions.
        /// </summary>
        public int TransactionCount { get; set; }
    }

    /// <summary>
    /// Represents exchange net position.
    /// </summary>
    public class ExchangeNetPosition
    {
        /// <summary>
        /// Gets or sets the net volume (receipts - deliveries) in barrels.
        /// </summary>
        public decimal NetVolume { get; set; }

        /// <summary>
        /// Gets or sets the net value (receipts - deliveries).
        /// </summary>
        public decimal NetValue { get; set; }

        /// <summary>
        /// Gets whether the position is long (positive net volume).
        /// </summary>
        public bool IsLong => NetVolume > 0;

        /// <summary>
        /// Gets whether the position is short (negative net volume).
        /// </summary>
        public bool IsShort => NetVolume < 0;

        /// <summary>
        /// Gets whether the position is flat (zero net volume).
        /// </summary>
        public bool IsFlat => NetVolume == 0;
    }

    /// <summary>
    /// Provides exchange statement generation.
    /// </summary>
    public static class ExchangeStatementGenerator
    {
        /// <summary>
        /// Generates an exchange statement for a contract and period.
        /// </summary>
        public static ExchangeStatement GenerateStatement(
            string contractId,
            DateTime periodStart,
            DateTime periodEnd,
            List<ExchangeTransaction> transactions)
        {
            var statement = new ExchangeStatement
            {
                StatementId = Guid.NewGuid().ToString(),
                StatementPeriodStart = periodStart,
                StatementPeriodEnd = periodEnd,
                ContractId = contractId,
                Transactions = transactions.Where(t => 
                    t.TransactionDate >= periodStart && 
                    t.TransactionDate <= periodEnd).ToList()
            };

            // Calculate receipts summary
            var receiptTransactions = statement.Transactions
                .Where(t => t.ReceiptVolume > 0)
                .ToList();

            if (receiptTransactions.Count > 0)
            {
                statement.Receipts = new ExchangeSummary
                {
                    TotalVolume = receiptTransactions.Sum(t => t.ReceiptVolume),
                    AveragePrice = receiptTransactions.Sum(t => t.ReceiptValue) / receiptTransactions.Sum(t => t.ReceiptVolume),
                    TransactionCount = receiptTransactions.Count
                };
            }

            // Calculate deliveries summary
            var deliveryTransactions = statement.Transactions
                .Where(t => t.DeliveryVolume > 0)
                .ToList();

            if (deliveryTransactions.Count > 0)
            {
                statement.Deliveries = new ExchangeSummary
                {
                    TotalVolume = deliveryTransactions.Sum(t => t.DeliveryVolume),
                    AveragePrice = deliveryTransactions.Sum(t => t.DeliveryValue) / deliveryTransactions.Sum(t => t.DeliveryVolume),
                    TransactionCount = deliveryTransactions.Count
                };
            }

            // Calculate net position
            statement.NetPosition = new ExchangeNetPosition
            {
                NetVolume = statement.Receipts.TotalVolume - statement.Deliveries.TotalVolume,
                NetValue = statement.Receipts.TotalValue - statement.Deliveries.TotalValue
            };

            return statement;
        }
    }
}

