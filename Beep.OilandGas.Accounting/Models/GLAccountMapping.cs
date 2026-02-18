using System;

namespace Beep.OilandGas.Accounting.Models
{
    /// <summary>
    /// Represents a mapping between a logical account key (e.g., "Cash") and a physical GL account number.
    /// This allows for dynamic configuration of GL accounts without code changes.
    /// </summary>
    public class GLAccountMapping
    {
        public string GL_ACCOUNT_MAPPING_ID { get; set; }
        public string MAPPING_KEY { get; set; }
        public string GL_ACCOUNT_NUMBER { get; set; }
        public string DESCRIPTION { get; set; }
        public string ACTIVE_IND { get; set; }
        public string PPDM_GUID { get; set; }
        public string ROW_CREATED_BY { get; set; }
        public DateTime? ROW_CREATED_DATE { get; set; }
        public string ROW_CHANGED_BY { get; set; }
        public DateTime? ROW_CHANGED_DATE { get; set; }
    }
}
