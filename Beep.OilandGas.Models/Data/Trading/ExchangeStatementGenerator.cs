using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public static class ExchangeStatementGenerator
    {
        /// <summary>
        /// Generates an exchange statement for a contract and period.
        /// </summary>
        public static ExchangeStatement GenerateStatement(
            string contractId,
            DateTime periodStart,
            DateTime periodEnd,
            List<EXCHANGE_TRANSACTION> transactions)
        {
            var statement = new ExchangeStatement
            {
                StatementId = Guid.NewGuid().ToString(),
                StatementPeriodStart = periodStart,
                StatementPeriodEnd = periodEnd,
                ContractId = contractId,
                Transactions = transactions.Where(t => 
                    t.TRANSACTION_DATE >= periodStart && 
                    t.TRANSACTION_DATE <= periodEnd).ToList()
            };

            // Calculate receipts summary
            var receiptTransactions = statement.Transactions
                .Where(t => t.RECEIPT_VOLUME > 0)
                .ToList();

            if (receiptTransactions.Count > 0)
            {
                statement.Receipts = new ExchangeSummary
                {
                    TotalVolume = receiptTransactions.Sum(t => t.RECEIPT_VOLUME  ),
                    AveragePrice = receiptTransactions.Sum(t => (t.RECEIPT_VOLUME  ) * (t.RECEIPT_PRICE  )) / receiptTransactions.Sum(t => t.RECEIPT_VOLUME ),
                    TransactionCount = receiptTransactions.Count
                };
            }

            // Calculate deliveries summary
            var deliveryTransactions = statement.Transactions
                .Where(t => (t.DELIVERY_VOLUME  ) > 0)
                .ToList();

            if (deliveryTransactions.Count > 0)
            {
                statement.Deliveries = new ExchangeSummary
                {
                    TotalVolume = deliveryTransactions.Sum(t => t.DELIVERY_VOLUME  ),
                    AveragePrice = deliveryTransactions.Sum(t => (t.DELIVERY_VOLUME  ) * (t.DELIVERY_PRICE  )) / deliveryTransactions.Sum(t => t.DELIVERY_VOLUME),
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
