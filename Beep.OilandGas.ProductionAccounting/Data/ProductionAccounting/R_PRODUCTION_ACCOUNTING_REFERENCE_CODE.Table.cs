using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// Project-owned reference code table for production-accounting domains that are
    /// not represented by a suitable PPDM R_/RA_ table.
    /// </summary>
    public partial class R_PRODUCTION_ACCOUNTING_REFERENCE_CODE : ModelEntityBase
    {
        private string _referenceSet = string.Empty;
        public string REFERENCE_SET
        {
            get => _referenceSet;
            set => SetProperty(ref _referenceSet, value);
        }

        private string _referenceCode = string.Empty;
        public string REFERENCE_CODE
        {
            get => _referenceCode;
            set => SetProperty(ref _referenceCode, value);
        }

        private string? _longName;
        public string? LONG_NAME
        {
            get => _longName;
            set => SetProperty(ref _longName, value);
        }

        private string? _shortName;
        public string? SHORT_NAME
        {
            get => _shortName;
            set => SetProperty(ref _shortName, value);
        }

    }
}

