#nullable disable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ACCOUNTING_METHOD : ModelEntityBase {
#nullable enable
        private string? OWNER_TENANT_IDValue;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? OWNER_TENANT_ID { get => OWNER_TENANT_IDValue; set => SetProperty(ref OWNER_TENANT_IDValue,value); }
        private string? OWNER_COMPANY_BA_IDValue;
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string? OWNER_COMPANY_BA_ID { get => OWNER_COMPANY_BA_IDValue; set => SetProperty(ref OWNER_COMPANY_BA_IDValue,value); }
        private string? MATCHING_POLICY_COMMAND_JSONValue;
        [System.ComponentModel.DataAnnotations.MaxLength(24000)]
        public string? MATCHING_POLICY_COMMAND_JSON { get => MATCHING_POLICY_COMMAND_JSONValue; set => SetProperty(ref MATCHING_POLICY_COMMAND_JSONValue,value); }
        private string? MATCHING_POLICY_JSONValue;
        [System.ComponentModel.DataAnnotations.MaxLength(4000)]
        public string? MATCHING_POLICY_JSON { get => MATCHING_POLICY_JSONValue; set => SetProperty(ref MATCHING_POLICY_JSONValue,value); }
        private string? MATCHING_POLICY_STATUSValue;
        [System.ComponentModel.DataAnnotations.MaxLength(32)]
        public string? MATCHING_POLICY_STATUS { get => MATCHING_POLICY_STATUSValue; set => SetProperty(ref MATCHING_POLICY_STATUSValue,value); }
#nullable disable
        private System.String ACCOUNTING_METHOD_IDValue;
        [System.ComponentModel.DataAnnotations.Key]
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public System.String ACCOUNTING_METHOD_ID
        {
            get { return this.ACCOUNTING_METHOD_IDValue; }
            set { SetProperty(ref ACCOUNTING_METHOD_IDValue, value); }
        }
        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
        }

        private System.String METHOD_TYPEValue;
        public System.String METHOD_TYPE
        {
            get { return this.METHOD_TYPEValue; }
            set { SetProperty(ref METHOD_TYPEValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

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
