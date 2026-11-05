#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PO_LINE_ITEM : ModelEntityBase {
#nullable enable
        private string? inventoryLinkHistoryRevision;
        [System.ComponentModel.DataAnnotations.MaxLength(64)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? INVENTORY_LINK_HISTORY_REVISION {get=>inventoryLinkHistoryRevision;set=>SetProperty(ref inventoryLinkHistoryRevision,value);}
        private string? inventoryLinkDecision;
        [System.ComponentModel.DataAnnotations.MaxLength(1048576)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? INVENTORY_LINK_DECISION_JSON {get=>inventoryLinkDecision;set=>SetProperty(ref inventoryLinkDecision,value);}
        private string? inventoryLinkReview;
        [System.ComponentModel.DataAnnotations.MaxLength(262144)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? INVENTORY_LINK_REVIEW_JSON {get=>inventoryLinkReview;set=>SetProperty(ref inventoryLinkReview,value);}
        private string? inventoryItem;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? INVENTORY_ITEM_ID {get=>inventoryItem;set=>SetProperty(ref inventoryItem,value);}
        private string? QUANTITY_UOMValue;
        [System.ComponentModel.DataAnnotations.MaxLength(64)]
        public string? QUANTITY_UOM
        {
            get => QUANTITY_UOMValue;
            set => SetProperty(ref QUANTITY_UOMValue,value);
        }
#nullable disable

        private System.String PO_LINE_ITEM_IDValue;
        [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Required]
        public System.String PO_LINE_ITEM_ID
        {
            get { return this.PO_LINE_ITEM_IDValue; }
            set { SetProperty(ref PO_LINE_ITEM_IDValue, value); }
        }

        private System.String PURCHASE_ORDER_IDValue;
        public System.String PURCHASE_ORDER_ID
        {
            get { return this.PURCHASE_ORDER_IDValue; }
            set { SetProperty(ref PURCHASE_ORDER_IDValue, value); }
        }

        private System.Int32? LINE_NUMBERValue;
        public System.Int32? LINE_NUMBER
        {
            get { return this.LINE_NUMBERValue; }
            set { SetProperty(ref LINE_NUMBERValue, value); }
        }

        private System.String ITEM_DESCRIPTIONValue;
        public System.String ITEM_DESCRIPTION
        {
            get { return this.ITEM_DESCRIPTIONValue; }
            set { SetProperty(ref ITEM_DESCRIPTIONValue, value); }
        }

        private System.Decimal? QUANTITYValue;
        public System.Decimal? QUANTITY
        {
            get { return this.QUANTITYValue; }
            set { SetProperty(ref QUANTITYValue, value); }
        }

        private System.Decimal? UNIT_PRICEValue;
        public System.Decimal? UNIT_PRICE
        {
            get { return this.UNIT_PRICEValue; }
            set { SetProperty(ref UNIT_PRICEValue, value); }
        }

        private System.Decimal? LINE_TOTALValue;
        public System.Decimal? LINE_TOTAL
        {
            get { return this.LINE_TOTALValue; }
            set { SetProperty(ref LINE_TOTALValue, value); }
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
