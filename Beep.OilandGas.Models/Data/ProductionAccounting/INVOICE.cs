#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVOICE : ModelEntityBase {
#nullable enable
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? OWNER_TENANT_ID { get;set; }
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? OWNER_COMPANY_BA_ID { get;set; }
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? FIELD_ID { get;set; }
        [System.ComponentModel.DataAnnotations.MaxLength(4194304)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? BILLING_WORKFLOW_JSON { get;set; }
        [System.ComponentModel.DataAnnotations.MaxLength(67108864)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? REVIEW_WORKFLOW_JSON { get;set; }
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? SOURCE_RUN_TICKET_ID { get;set; }
        [System.ComponentModel.DataAnnotations.MaxLength(64)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? BILLING_SOURCE_KEY { get;set; }
#nullable disable
        private System.String INVOICE_IDValue;
        [System.ComponentModel.DataAnnotations.Key]
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public System.String INVOICE_ID
        {
            get { return this.INVOICE_IDValue; }
            set { SetProperty(ref INVOICE_IDValue, value); }
        }

        private System.String INVOICE_NUMBERValue;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public System.String INVOICE_NUMBER
        {
            get { return this.INVOICE_NUMBERValue; }
            set { SetProperty(ref INVOICE_NUMBERValue, value); }
        }

        private System.String CUSTOMER_BA_IDValue;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public System.String CUSTOMER_BA_ID
        {
            get { return this.CUSTOMER_BA_IDValue; }
            set { SetProperty(ref CUSTOMER_BA_IDValue, value); }
        }

        private System.DateTime? INVOICE_DATEValue;
        public System.DateTime? INVOICE_DATE
        {
            get { return this.INVOICE_DATEValue; }
            set { SetProperty(ref INVOICE_DATEValue, value); }
        }

        private System.DateTime? DUE_DATEValue;
        public System.DateTime? DUE_DATE
        {
            get { return this.DUE_DATEValue; }
            set { SetProperty(ref DUE_DATEValue, value); }
        }

        private System.Decimal  SUBTOTALValue;
        public System.Decimal  SUBTOTAL
        {
            get { return this.SUBTOTALValue; }
            set { SetProperty(ref SUBTOTALValue, value); }
        }

        private System.Decimal  TAX_AMOUNTValue;
        public System.Decimal  TAX_AMOUNT
        {
            get { return this.TAX_AMOUNTValue; }
            set { SetProperty(ref TAX_AMOUNTValue, value); }
        }

        private System.Decimal  TOTAL_AMOUNTValue;
        public System.Decimal  TOTAL_AMOUNT
        {
            get { return this.TOTAL_AMOUNTValue; }
            set { SetProperty(ref TOTAL_AMOUNTValue, value); }
        }

        private System.Decimal  PAID_AMOUNTValue;
        public System.Decimal  PAID_AMOUNT
        {
            get { return this.PAID_AMOUNTValue; }
            set { SetProperty(ref PAID_AMOUNTValue, value); }
        }

        private System.Decimal  BALANCE_DUEValue;
        public System.Decimal  BALANCE_DUE
        {
            get { return this.BALANCE_DUEValue; }
            set { SetProperty(ref BALANCE_DUEValue, value); }
        }

        private System.String STATUSValue;
        [System.ComponentModel.DataAnnotations.MaxLength(32)]
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.String CURRENCY_CODEValue;
        [System.ComponentModel.DataAnnotations.MaxLength(16)]
        public System.String CURRENCY_CODE
        {
            get { return this.CURRENCY_CODEValue; }
            set { SetProperty(ref CURRENCY_CODEValue, value); }
        }

        private System.String DESCRIPTIONValue;
        [System.ComponentModel.DataAnnotations.MaxLength(1000)]
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
