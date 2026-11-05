#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVENTORY_TRANSACTION : ModelEntityBase {
#nullable enable
        private string? workOrderMaterialUsageJson;
        [System.ComponentModel.DataAnnotations.MaxLength(16384)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? WORK_ORDER_MATERIAL_USAGE_JSON {get=>workOrderMaterialUsageJson;set=>SetProperty(ref workOrderMaterialUsageJson,value);}
        private string? purchaseReceiptStockJson;
        [System.ComponentModel.DataAnnotations.MaxLength(16384)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? PURCHASE_RECEIPT_STOCK_JSON {get=>purchaseReceiptStockJson;set=>SetProperty(ref purchaseReceiptStockJson,value);}
#nullable disable
        private System.String INVENTORY_TRANSACTION_IDValue;
        [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Required]
        public System.String INVENTORY_TRANSACTION_ID
        {
            get { return this.INVENTORY_TRANSACTION_IDValue; }
            set { SetProperty(ref INVENTORY_TRANSACTION_IDValue, value); }
        }

        private System.String INVENTORY_ITEM_IDValue;
        public System.String INVENTORY_ITEM_ID
        {
            get { return this.INVENTORY_ITEM_IDValue; }
            set { SetProperty(ref INVENTORY_ITEM_IDValue, value); }
        }

        private System.String TRANSACTION_TYPEValue;
        public System.String TRANSACTION_TYPE
        {
            get { return this.TRANSACTION_TYPEValue; }
            set { SetProperty(ref TRANSACTION_TYPEValue, value); }
        }

        private System.DateTime? TRANSACTION_DATEValue;
        public System.DateTime? TRANSACTION_DATE
        {
            get { return this.TRANSACTION_DATEValue; }
            set { SetProperty(ref TRANSACTION_DATEValue, value); }
        }

        private System.Decimal? QUANTITYValue;
        public System.Decimal? QUANTITY
        {
            get { return this.QUANTITYValue; }
            set { SetProperty(ref QUANTITYValue, value); }
        }

        private System.Decimal? UNIT_COSTValue;
        public System.Decimal? UNIT_COST
        {
            get { return this.UNIT_COSTValue; }
            set { SetProperty(ref UNIT_COSTValue, value); }
        }

        private System.Decimal? TOTAL_COSTValue;
        public System.Decimal? TOTAL_COST
        {
            get { return this.TOTAL_COSTValue; }
            set { SetProperty(ref TOTAL_COSTValue, value); }
        }

        private System.String REFERENCE_NUMBERValue;
        public System.String REFERENCE_NUMBER
        {
            get { return this.REFERENCE_NUMBERValue; }
            set { SetProperty(ref REFERENCE_NUMBERValue, value); }
        }

         private System.String DESCRIPTIONValue;
         public System.String DESCRIPTION
         {
             get { return this.DESCRIPTIONValue; }
             set { SetProperty(ref DESCRIPTIONValue, value); }
         }

         private System.String NOTESValue;
         public System.String NOTES
         {
             get { return this.NOTESValue; }
             set { SetProperty(ref NOTESValue, value); }
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
