#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PO_RECEIPT : ModelEntityBase {
#nullable enable
        private decimal? ACCEPTED_QUANTITYValue;
        public decimal? ACCEPTED_QUANTITY { get => ACCEPTED_QUANTITYValue; set => SetProperty(ref ACCEPTED_QUANTITYValue,value); }
        private decimal? REJECTED_QUANTITYValue;
        public decimal? REJECTED_QUANTITY { get => REJECTED_QUANTITYValue; set => SetProperty(ref REJECTED_QUANTITYValue,value); }
        private string? RECEIVER_DECISION_JSONValue;
        [System.ComponentModel.DataAnnotations.MaxLength(16000)]
        public string? RECEIVER_DECISION_JSON { get => RECEIVER_DECISION_JSONValue; set => SetProperty(ref RECEIVER_DECISION_JSONValue,value); }
        private string? QUANTITY_UOMValue;
        [System.ComponentModel.DataAnnotations.MaxLength(64)]
        public string? QUANTITY_UOM
        {
            get => QUANTITY_UOMValue;
            set => SetProperty(ref QUANTITY_UOMValue,value);
        }
#nullable disable

        private System.String PO_RECEIPT_IDValue;
        [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Required]
        public System.String PO_RECEIPT_ID
        {
            get { return this.PO_RECEIPT_IDValue; }
            set { SetProperty(ref PO_RECEIPT_IDValue, value); }
        }

        private System.String PURCHASE_ORDER_IDValue;
        public System.String PURCHASE_ORDER_ID
        {
            get { return this.PURCHASE_ORDER_IDValue; }
            set { SetProperty(ref PURCHASE_ORDER_IDValue, value); }
        }

        private System.String PO_LINE_ITEM_IDValue;
        public System.String PO_LINE_ITEM_ID
        {
            get { return this.PO_LINE_ITEM_IDValue; }
            set { SetProperty(ref PO_LINE_ITEM_IDValue, value); }
        }

        private System.DateTime? RECEIPT_DATEValue;
        public System.DateTime? RECEIPT_DATE
        {
            get { return this.RECEIPT_DATEValue; }
            set { SetProperty(ref RECEIPT_DATEValue, value); }
        }

        private System.Decimal? RECEIVED_QUANTITYValue;
        public System.Decimal? RECEIVED_QUANTITY
        {
            get { return this.RECEIVED_QUANTITYValue; }
            set { SetProperty(ref RECEIVED_QUANTITYValue, value); }
        }

        private System.String RECEIVED_BYValue;
        public System.String RECEIVED_BY
        {
            get { return this.RECEIVED_BYValue; }
            set { SetProperty(ref RECEIVED_BYValue, value); }
        }

        private System.String RECEIPT_STATUSValue;
        public System.String RECEIPT_STATUS
        {
            get { return this.RECEIPT_STATUSValue; }
            set { SetProperty(ref RECEIPT_STATUSValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
