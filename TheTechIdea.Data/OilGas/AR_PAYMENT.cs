#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class AR_PAYMENT : ModelEntityBase {
        private System.String AR_PAYMENT_IDValue;
        [Key,Required,MaxLength(128)] public System.String AR_PAYMENT_ID
        {
            get { return this.AR_PAYMENT_IDValue; }
            set { SetProperty(ref AR_PAYMENT_IDValue, value); }
        }

        private System.String AR_INVOICE_IDValue;
        [MaxLength(128)] public System.String AR_INVOICE_ID
        {
            get { return this.AR_INVOICE_IDValue; }
            set { SetProperty(ref AR_INVOICE_IDValue, value); }
        }

        private System.String PAYMENT_NUMBERValue;
        [MaxLength(128)] public System.String PAYMENT_NUMBER
        {
            get { return this.PAYMENT_NUMBERValue; }
            set { SetProperty(ref PAYMENT_NUMBERValue, value); }
        }

        private System.DateTime? PAYMENT_DATEValue;
        public System.DateTime? PAYMENT_DATE
        {
            get { return this.PAYMENT_DATEValue; }
            set { SetProperty(ref PAYMENT_DATEValue, value); }
        }

        private System.Decimal? PAYMENT_AMOUNTValue;
        public System.Decimal? PAYMENT_AMOUNT
        {
            get { return this.PAYMENT_AMOUNTValue; }
            set { SetProperty(ref PAYMENT_AMOUNTValue, value); }
        }

        private System.String PAYMENT_METHODValue;
        [MaxLength(32)] public System.String PAYMENT_METHOD
        {
            get { return this.PAYMENT_METHODValue; }
            set { SetProperty(ref PAYMENT_METHODValue, value); }
        }

        private System.String CHECK_NUMBERValue;
        [MaxLength(128)] public System.String CHECK_NUMBER
        {
            get { return this.CHECK_NUMBERValue; }
            set { SetProperty(ref CHECK_NUMBERValue, value); }
        }

        // Standard PPDM columns
        [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string OWNER_TENANT_ID{get;set;}
        [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string OWNER_COMPANY_BA_ID{get;set;}
        [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string BOOK_ID{get;set;}
        [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string FIELD_ID{get;set;}
        [MaxLength(16),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string CURRENCY_CODE{get;set;}
        [MaxLength(128),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string PAYER_BA_ID{get;set;}
        [MaxLength(256),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string EXTERNAL_REFERENCE{get;set;}
        [MaxLength(2000),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string EVIDENCE_REFERENCE{get;set;}
        [MaxLength(32),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string STATUS{get;set;}
        [MaxLength(64),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string SOURCE_REVISION{get;set;}
        [MaxLength(1048576),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string PREPARATION_WORKFLOW_JSON{get;set;}
        [MaxLength(8388608),JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)] public string REVIEW_WORKFLOW_JSON{get;set;}

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
