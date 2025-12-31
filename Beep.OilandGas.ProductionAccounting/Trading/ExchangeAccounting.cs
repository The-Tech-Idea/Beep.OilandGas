namespace Beep.OilandGas.ProductionAccounting.Trading
{
    /// <summary>
    /// Represents an exchange accounting entry.
    /// </summary>
    public class ExchangeEntry
    {
        /// <summary>
        /// Gets or sets the entry identifier.
        /// </summary>
        public string EntryId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exchange transaction identifier.
        /// </summary>
        public string ExchangeTransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entry date.
        /// </summary>
        public DateTime EntryDate { get; set; }

        /// <summary>
        /// Gets or sets the account code (debit or credit).
        /// </summary>
        public string AccountCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is a debit entry.
        /// </summary>
        public bool IsDebit { get; set; }
    }

    /// <summary>
    /// Provides exchange accounting functionality.
    /// </summary>
    public static class ExchangeAccounting
    {
        /// <summary>
        /// Creates accounting entries for an exchange transaction.
        /// </summary>
        public static List<ExchangeEntry> CreateExchangeEntries(
            ExchangeTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var entries = new List<ExchangeEntry>();

            // Debit: Crude Oil Inventory (Receipt)
            entries.Add(new ExchangeEntry
            {
                EntryId = Guid.NewGuid().ToString(),
                ExchangeTransactionId = transaction.TransactionId,
                EntryDate = transaction.TransactionDate,
                AccountCode = "CrudeOilInventory",
                Amount = transaction.ReceiptVolume * transaction.ReceiptPrice,
                Description = $"Exchange receipt: {transaction.ReceiptVolume} bbl @ ${transaction.ReceiptPrice}/bbl",
                IsDebit = true
            });

            // Credit: Crude Oil Inventory (Delivery)
            entries.Add(new ExchangeEntry
            {
                EntryId = Guid.NewGuid().ToString(),
                ExchangeTransactionId = transaction.TransactionId,
                EntryDate = transaction.TransactionDate,
                AccountCode = "CrudeOilInventory",
                Amount = transaction.DeliveryVolume * transaction.DeliveryPrice,
                Description = $"Exchange delivery: {transaction.DeliveryVolume} bbl @ ${transaction.DeliveryPrice}/bbl",
                IsDebit = false
            });

            // Calculate gain/loss
            decimal receiptValue = transaction.ReceiptVolume * transaction.ReceiptPrice;
            decimal deliveryValue = transaction.DeliveryVolume * transaction.DeliveryPrice;
            decimal gainLoss = receiptValue - deliveryValue;

            if (gainLoss != 0)
            {
                entries.Add(new ExchangeEntry
                {
                    EntryId = Guid.NewGuid().ToString(),
                    ExchangeTransactionId = transaction.TransactionId,
                    EntryDate = transaction.TransactionDate,
                    AccountCode = gainLoss > 0 ? "ExchangeGain" : "ExchangeLoss",
                    Amount = Math.Abs(gainLoss),
                    Description = $"Exchange {((gainLoss > 0) ? "gain" : "loss")}",
                    IsDebit = gainLoss < 0
                });
            }

            return entries;
        }

        /// <summary>
        /// Calculates exchange valuation.
        /// </summary>
        public static ExchangeValuation CalculateValuation(ExchangeTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            decimal receiptValue = transaction.ReceiptVolume * transaction.ReceiptPrice;
            decimal deliveryValue = transaction.DeliveryVolume * transaction.DeliveryPrice;
            decimal netValue = receiptValue - deliveryValue;

            return new ExchangeValuation
            {
                TransactionId = transaction.TransactionId,
                ReceiptValue = receiptValue,
                DeliveryValue = deliveryValue,
                NetValue = netValue,
                IsGain = netValue > 0,
                GainLossAmount = Math.Abs(netValue)
            };
        }
    }

    /// <summary>
    /// Represents an exchange transaction.
    /// </summary>
    public class ExchangeTransaction
    {
        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        public string ContractId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the receipt volume in barrels.
        /// </summary>
        public decimal ReceiptVolume { get; set; }

        /// <summary>
        /// Gets or sets the receipt price per barrel.
        /// </summary>
        public decimal ReceiptPrice { get; set; }

        /// <summary>
        /// Gets or sets the receipt location.
        /// </summary>
        public string ReceiptLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the delivery volume in barrels.
        /// </summary>
        public decimal DeliveryVolume { get; set; }

        /// <summary>
        /// Gets or sets the delivery price per barrel.
        /// </summary>
        public decimal DeliveryPrice { get; set; }

        /// <summary>
        /// Gets or sets the delivery location.
        /// </summary>
        public string DeliveryLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets the receipt value.
        /// </summary>
        public decimal ReceiptValue => ReceiptVolume * ReceiptPrice;

        /// <summary>
        /// Gets the delivery value.
        /// </summary>
        public decimal DeliveryValue => DeliveryVolume * DeliveryPrice;

        /// <summary>
        /// Gets the net value (receipt - delivery).
        /// </summary>
        public decimal NetValue => ReceiptValue - DeliveryValue;

        public string DeliveryPoint { get; internal set; }
        public object PricePerBarrel { get; internal set; }
        public object TotalValue { get; internal set; }
    }

    /// <summary>
    /// Represents exchange valuation.
    /// </summary>
    public class ExchangeValuation
    {
        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the receipt value.
        /// </summary>
        public decimal ReceiptValue { get; set; }

        /// <summary>
        /// Gets or sets the delivery value.
        /// </summary>
        public decimal DeliveryValue { get; set; }

        /// <summary>
        /// Gets or sets the net value.
        /// </summary>
        public decimal NetValue { get; set; }

        /// <summary>
        /// Gets or sets whether this is a gain.
        /// </summary>
        public bool IsGain { get; set; }

        /// <summary>
        /// Gets or sets the gain/loss amount.
        /// </summary>
        public decimal GainLossAmount { get; set; }
    }
}
