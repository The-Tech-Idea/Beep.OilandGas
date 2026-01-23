using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Trading
{
    /// <summary>
    /// Represents an exchange statement.
    /// </summary>
    public partial class ExchangeStatement : ModelEntityBase
    {
        private System.String StatementIdValue;
        public System.String StatementId
        {
            get { return this.StatementIdValue; }
            set { SetProperty(ref StatementIdValue, value); }
        }

        private System.DateTime StatementPeriodStartValue;
        public System.DateTime StatementPeriodStart
        {
            get { return this.StatementPeriodStartValue; }
            set { SetProperty(ref StatementPeriodStartValue, value); }
        }

        private System.DateTime StatementPeriodEndValue;
        public System.DateTime StatementPeriodEnd
        {
            get { return this.StatementPeriodEndValue; }
            set { SetProperty(ref StatementPeriodEndValue, value); }
        }

        private System.String ContractIdValue;
        public System.String ContractId
        {
            get { return this.ContractIdValue; }
            set { SetProperty(ref ContractIdValue, value); }
        }

        private List<ExchangeTransaction> TransactionsValue = new List<ExchangeTransaction>();
        public List<ExchangeTransaction> Transactions
        {
            get { return this.TransactionsValue; }
            set { SetProperty(ref TransactionsValue, value); }
        }

        private ExchangeSummary ReceiptsValue = new ExchangeSummary();
        public ExchangeSummary Receipts
        {
            get { return this.ReceiptsValue; }
            set { SetProperty(ref ReceiptsValue, value); }
        }

        private ExchangeSummary DeliveriesValue = new ExchangeSummary();
        public ExchangeSummary Deliveries
        {
            get { return this.DeliveriesValue; }
            set { SetProperty(ref DeliveriesValue, value); }
        }

        private ExchangeNetPosition NetPositionValue = new ExchangeNetPosition();
        public ExchangeNetPosition NetPosition
        {
            get { return this.NetPositionValue; }
            set { SetProperty(ref NetPositionValue, value); }
        }
    }

    /// <summary>
    /// Represents an exchange summary.
    /// </summary>
    public partial class ExchangeSummary : ModelEntityBase
    {
        private System.Decimal TotalVolumeValue;
        public System.Decimal TotalVolume
        {
            get { return this.TotalVolumeValue; }
            set { SetProperty(ref TotalVolumeValue, value); }
        }

        private System.Decimal AveragePriceValue;
        public System.Decimal AveragePrice
        {
            get { return this.AveragePriceValue; }
            set { SetProperty(ref AveragePriceValue, value); }
        }

        public System.Decimal TotalValue => TotalVolume * AveragePrice;

        private System.Int32 TransactionCountValue;
        public System.Int32 TransactionCount
        {
            get { return this.TransactionCountValue; }
            set { SetProperty(ref TransactionCountValue, value); }
        }
    }

    /// <summary>
    /// Represents exchange net position.
    /// </summary>
    public partial class ExchangeNetPosition : ModelEntityBase
    {
        private System.Decimal NetVolumeValue;
        public System.Decimal NetVolume
        {
            get { return this.NetVolumeValue; }
            set { SetProperty(ref NetVolumeValue, value); }
        }

        private System.Decimal NetValueValue;
        public System.Decimal NetValue
        {
            get { return this.NetValueValue; }
            set { SetProperty(ref NetValueValue, value); }
        }

        public bool IsLong => NetVolume > 0;
        public bool IsShort => NetVolume < 0;
        public bool IsFlat => NetVolume == 0;
    }

    /// <summary>
    /// Represents an exchange transaction.
    /// </summary>
    public partial class ExchangeTransaction : ModelEntityBase
    {
        private System.String TransactionIdValue;
        public System.String TransactionId
        {
            get { return this.TransactionIdValue; }
            set { SetProperty(ref TransactionIdValue, value); }
        }

        private System.String ContractIdValue;
        public System.String ContractId
        {
            get { return this.ContractIdValue; }
            set { SetProperty(ref ContractIdValue, value); }
        }

        private System.DateTime TransactionDateValue;
        public System.DateTime TransactionDate
        {
            get { return this.TransactionDateValue; }
            set { SetProperty(ref TransactionDateValue, value); }
        }

        private System.Decimal ReceiptVolumeValue;
        public System.Decimal ReceiptVolume
        {
            get { return this.ReceiptVolumeValue; }
            set { SetProperty(ref ReceiptVolumeValue, value); }
        }

        private System.Decimal ReceiptPriceValue;
        public System.Decimal ReceiptPrice
        {
            get { return this.ReceiptPriceValue; }
            set { SetProperty(ref ReceiptPriceValue, value); }
        }

        private System.String ReceiptLocationValue;
        public System.String ReceiptLocation
        {
            get { return this.ReceiptLocationValue; }
            set { SetProperty(ref ReceiptLocationValue, value); }
        }

        private System.Decimal DeliveryVolumeValue;
        public System.Decimal DeliveryVolume
        {
            get { return this.DeliveryVolumeValue; }
            set { SetProperty(ref DeliveryVolumeValue, value); }
        }

        private System.Decimal DeliveryPriceValue;
        public System.Decimal DeliveryPrice
        {
            get { return this.DeliveryPriceValue; }
            set { SetProperty(ref DeliveryPriceValue, value); }
        }

        private System.String DeliveryLocationValue;
        public System.String DeliveryLocation
        {
            get { return this.DeliveryLocationValue; }
            set { SetProperty(ref DeliveryLocationValue, value); }
        }

        public System.Decimal ReceiptValue => ReceiptVolume * ReceiptPrice;
        public System.Decimal DeliveryValue => DeliveryVolume * DeliveryPrice;
        public System.Decimal NetValue => ReceiptValue - DeliveryValue;
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





