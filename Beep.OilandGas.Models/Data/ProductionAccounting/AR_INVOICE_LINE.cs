using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// One line of a sales invoice (ACC-03): what was sold, how much, at what price, the tax on it, and - the point of
    /// AC-3 - the volume record it came from.
    /// </summary>
    /// <remarks>
    /// The AR tables carried an invoice total and a credit memo but no lines, so there was nothing to trace back to a
    /// volume and no place for a line's own tax. This table is the line, and <see cref="VOLUME_REFERENCE"/> is the link the
    /// feature asks for ("traceable from invoice line back to the volume that produced it").
    /// </remarks>
    public class AR_INVOICE_LINE : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string AR_INVOICE_LINE_ID
        {
            get => LineIdValue;
            set => SetProperty(ref LineIdValue, value);
        }

        private string LineIdValue = string.Empty;

        [MaxLength(64)]
        public string AR_INVOICE_ID
        {
            get => InvoiceValue;
            set => SetProperty(ref InvoiceValue, value);
        }

        private string InvoiceValue = string.Empty;

        public int LINE_NUMBER
        {
            get => LineNumberValue;
            set => SetProperty(ref LineNumberValue, value);
        }

        private int LineNumberValue;

        /// <summary>The commodity sold, as the contract names it.</summary>
        [MaxLength(64)]
        public string? COMMODITY
        {
            get => CommodityValue;
            set => SetProperty(ref CommodityValue, value);
        }

        [MaxLength(512)]
        public string? DESCRIPTION
        {
            get => DescriptionValue;
            set => SetProperty(ref DescriptionValue, value);
        }

        public decimal QUANTITY
        {
            get => QuantityValue;
            set => SetProperty(ref QuantityValue, value);
        }

        /// <summary>The unit the quantity is in - bbl, Mscf, boe.</summary>
        [MaxLength(16)]
        public string? QUANTITY_OUOM
        {
            get => UnitValue;
            set => SetProperty(ref UnitValue, value);
        }

        private string? UnitValue;

        public decimal UNIT_PRICE
        {
            get => PriceValue;
            set => SetProperty(ref PriceValue, value);
        }

        private decimal PriceValue;

        [MaxLength(32)]
        public string? PRICE_OUOM
        {
            get => PriceOuomValue;
            set => SetProperty(ref PriceOuomValue, value);
        }

        private string? PriceOuomValue;

        /// <summary>Quantity times price, before tax.</summary>
        public decimal NET_AMOUNT
        {
            get => NetValue;
            set => SetProperty(ref NetValue, value);
        }

        private decimal NetValue;

        /// <summary>The tax the owning service computed for this line (AC-5).</summary>
        public decimal TAX_AMOUNT
        {
            get => TaxValue;
            set => SetProperty(ref TaxValue, value);
        }

        private decimal TaxValue;

        public decimal LINE_TOTAL
        {
            get => TotalValue;
            set => SetProperty(ref TotalValue, value);
        }

        private decimal TotalValue;

        /// <summary>
        /// The volume record that produced this line (AC-3). It is the sold-volume id from the revenue workbench, so an
        /// invoice line can be followed back to the volume - and the test asserts it survives the round trip.
        /// </summary>
        [MaxLength(128)]
        public string? VOLUME_REFERENCE
        {
            get => VolumeValue;
            set => SetProperty(ref VolumeValue, value);
        }

        private string? VolumeValue;
        private string? CommodityValue;
        private string? DescriptionValue;
        private decimal QuantityValue;
}
}
