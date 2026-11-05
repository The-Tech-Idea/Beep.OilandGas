#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PURCHASE_ORDER : ModelEntityBase {
#nullable enable
        private string? CURRENCY_CODEValue;
        [System.ComponentModel.DataAnnotations.MaxLength(32)]
        public string? CURRENCY_CODE
        {
            get => CURRENCY_CODEValue;
            set => SetProperty(ref CURRENCY_CODEValue,value);
        }
#nullable disable

#nullable enable
        // Explicit owning company; never inferred from the vendor or an active FIELD.
        private string? ownerTenant;
        private string? ownerCompany;
        [System.ComponentModel.DataAnnotations.MaxLength(64)]
        public string? OWNER_TENANT_ID {get=>ownerTenant;set=>SetProperty(ref ownerTenant,value);}
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? OWNER_COMPANY_BA_ID {get=>ownerCompany;set=>SetProperty(ref ownerCompany,value);}
#nullable disable
        private System.String PURCHASE_ORDER_IDValue;
        [System.ComponentModel.DataAnnotations.Key, System.ComponentModel.DataAnnotations.Required]
        public System.String PURCHASE_ORDER_ID
        {
            get { return this.PURCHASE_ORDER_IDValue; }
            set { SetProperty(ref PURCHASE_ORDER_IDValue, value); }
        }

        private System.String PO_NUMBERValue;
        public System.String PO_NUMBER
        {
            get { return this.PO_NUMBERValue; }
            set { SetProperty(ref PO_NUMBERValue, value); }
        }

        private System.String VENDOR_BA_IDValue;
        public System.String VENDOR_BA_ID
        {
            get { return this.VENDOR_BA_IDValue; }
            set { SetProperty(ref VENDOR_BA_IDValue, value); }
        }

        private System.DateTime? PO_DATEValue;
        public System.DateTime? PO_DATE
        {
            get { return this.PO_DATEValue; }
            set { SetProperty(ref PO_DATEValue, value); }
        }

        private System.DateTime? EXPECTED_DELIVERY_DATEValue;
        public System.DateTime? EXPECTED_DELIVERY_DATE
        {
            get { return this.EXPECTED_DELIVERY_DATEValue; }
            set { SetProperty(ref EXPECTED_DELIVERY_DATEValue, value); }
        }

        private System.Decimal  TOTAL_AMOUNTValue;
        public System.Decimal  TOTAL_AMOUNT
        {
            get { return this.TOTAL_AMOUNTValue; }
            set { SetProperty(ref TOTAL_AMOUNTValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
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
