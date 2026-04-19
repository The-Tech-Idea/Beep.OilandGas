using System;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class SalesTransaction
    {
        public string TransactionId
        {
            get => SALES_TRANSACTION_ID;
            set => SALES_TRANSACTION_ID = value;
        }

        public DateTime? TransactionDate
        {
            get => SALES_DATE;
            set => SALES_DATE = value;
        }

        public decimal NetVolume
        {
            get => NET_VOLUME;
            set => NET_VOLUME = value;
        }

        public decimal PricePerBarrel
        {
            get => PRICE_PER_BARREL;
            set => PRICE_PER_BARREL = value;
        }

        public string Purchaser { get; set; } = string.Empty;
    }
}