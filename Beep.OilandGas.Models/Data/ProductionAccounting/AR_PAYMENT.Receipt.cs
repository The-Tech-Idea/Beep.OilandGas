using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The receipt columns of an AR payment (ACC-04): which receipt this amount belongs to.
    /// </summary>
    /// <remarks>
    /// An <c>AR_PAYMENT</c> is one amount against one invoice, which is exactly what an allocation is - so the payment row is
    /// reused as the allocation rather than duplicated by a second allocation table. The receipt it belongs to is the only
    /// thing it was missing.
    /// </remarks>
    public partial class AR_PAYMENT
    {
        /// <summary>The receipt header this allocation was made by, or null for a payment recorded on its own.</summary>
        [MaxLength(64)]
        public string? RECEIPT_ID
        {
            get => ReceiptValue;
            set => SetProperty(ref ReceiptValue, value);
        }

        private string? ReceiptValue;
    }
}
