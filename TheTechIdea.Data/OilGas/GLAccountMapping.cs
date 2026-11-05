#nullable disable
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Accounting.Models
{
    /// <summary>
    /// Represents a mapping between a logical account key (e.g., "Cash") and a physical GL account number.
    /// This allows for dynamic configuration of GL accounts without code changes.
    /// Persisted via PPDMGenericRepository to the GL_ACCOUNT_MAPPING table.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.Table("GL_ACCOUNT_MAPPING")]
    public partial class GLAccountMapping : ModelEntityBase
    {
#nullable enable
        [System.ComponentModel.DataAnnotations.MaxLength(196608)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? CONFIGURATION_TRANSITION_JSON {get;set;}
        [System.ComponentModel.DataAnnotations.MaxLength(32768)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? CONFIGURATION_COMMAND_JSON {get;set;}
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? OWNER_TENANT_ID {get;set;}
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? OWNER_COMPANY_BA_ID {get;set;}
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? BOOK_ID {get;set;}
        [System.ComponentModel.DataAnnotations.MaxLength(16)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? CURRENCY_CODE {get;set;}
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public string? GL_ACCOUNT_ID {get;set;}
#nullable disable
        private string GL_ACCOUNT_MAPPING_IDValue = string.Empty;
        [System.ComponentModel.DataAnnotations.Key]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string GL_ACCOUNT_MAPPING_ID
        {
            get => GL_ACCOUNT_MAPPING_IDValue;
            set => SetProperty(ref GL_ACCOUNT_MAPPING_IDValue, value);
        }

        private string MAPPING_KEYValue = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string MAPPING_KEY
        {
            get => MAPPING_KEYValue;
            set => SetProperty(ref MAPPING_KEYValue, value);
        }

        private string GL_ACCOUNT_NUMBERValue = string.Empty;
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MaxLength(128)]
        public string GL_ACCOUNT_NUMBER
        {
            get => GL_ACCOUNT_NUMBERValue;
            set => SetProperty(ref GL_ACCOUNT_NUMBERValue, value);
        }

        private string DESCRIPTIONValue = string.Empty;
        [System.ComponentModel.DataAnnotations.MaxLength(1024)]
        public string DESCRIPTION
        {
            get => DESCRIPTIONValue;
            set => SetProperty(ref DESCRIPTIONValue, value);
        }
    }
}

