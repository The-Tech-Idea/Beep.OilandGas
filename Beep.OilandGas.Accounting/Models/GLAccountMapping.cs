using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Accounting.Models
{
    /// <summary>
    /// Represents a mapping between a logical account key (e.g., "Cash") and a physical GL account number.
    /// This allows for dynamic configuration of GL accounts without code changes.
    /// Persisted via PPDMGenericRepository to the GL_ACCOUNT_MAPPING table.
    /// </summary>
    public class GLAccountMapping : ModelEntityBase
    {
        private string GL_ACCOUNT_MAPPING_IDValue = string.Empty;
        public string GL_ACCOUNT_MAPPING_ID
        {
            get => GL_ACCOUNT_MAPPING_IDValue;
            set => SetProperty(ref GL_ACCOUNT_MAPPING_IDValue, value);
        }

        private string MAPPING_KEYValue = string.Empty;
        public string MAPPING_KEY
        {
            get => MAPPING_KEYValue;
            set => SetProperty(ref MAPPING_KEYValue, value);
        }

        private string GL_ACCOUNT_NUMBERValue = string.Empty;
        public string GL_ACCOUNT_NUMBER
        {
            get => GL_ACCOUNT_NUMBERValue;
            set => SetProperty(ref GL_ACCOUNT_NUMBERValue, value);
        }

        private string DESCRIPTIONValue = string.Empty;
        public string DESCRIPTION
        {
            get => DESCRIPTIONValue;
            set => SetProperty(ref DESCRIPTIONValue, value);
        }
    }
}

