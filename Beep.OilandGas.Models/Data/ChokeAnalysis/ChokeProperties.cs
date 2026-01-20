using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    /// <summary>
    /// Represents choke properties
    /// DTO for calculations - Entity class: CHOKE_PROPERTIES
    /// </summary>
    public partial class ChokeProperties : ModelEntityBase {
        /// <summary>
        /// Choke diameter in inches
        /// </summary>
        private decimal _chokeDiameterValue;
        public decimal ChokeDiameter
        {
            get { return _chokeDiameterValue; }
            set { SetProperty(ref _chokeDiameterValue, value); }
        }

        /// <summary>
        /// Choke type (bean, adjustable, etc.)
        /// </summary>
        private ChokeType _chokeTypeValue;
        public ChokeType ChokeType
        {
            get { return _chokeTypeValue; }
            set { SetProperty(ref _chokeTypeValue, value); }
        }

        /// <summary>
        /// Discharge coefficient
        /// </summary>
        private decimal _dischargeCoefficientValue = 0.85m;
        public decimal DischargeCoefficient
        {
            get { return _dischargeCoefficientValue; }
            set { SetProperty(ref _dischargeCoefficientValue, value); }
        }

        /// <summary>
        /// Choke area in square inches
        /// </summary>
        public decimal ChokeArea => (decimal)Math.PI * ChokeDiameter * ChokeDiameter / 4m;

        // PPDM Entity Properties

        private string _activeIndValue = "Y";
        public string ACTIVE_IND
        {
            get { return _activeIndValue; }
            set { SetProperty(ref _activeIndValue, value); }
        }

        private string _rowCreatedByValue;
        public string ROW_CREATED_BY
        {
            get { return _rowCreatedByValue; }
            set { SetProperty(ref _rowCreatedByValue, value); }
        }

        private DateTime? _rowCreatedDateValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return _rowCreatedDateValue; }
            set { SetProperty(ref _rowCreatedDateValue, value); }
        }

        private string _rowChangedByValue;
        public string ROW_CHANGED_BY
        {
            get { return _rowChangedByValue; }
            set { SetProperty(ref _rowChangedByValue, value); }
        }

        private DateTime? _rowChangedDateValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return _rowChangedDateValue; }
            set { SetProperty(ref _rowChangedDateValue, value); }
        }

        private DateTime? _rowEffectiveDateValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return _rowEffectiveDateValue; }
            set { SetProperty(ref _rowEffectiveDateValue, value); }
        }

        private DateTime? _rowExpiryDateValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return _rowExpiryDateValue; }
            set { SetProperty(ref _rowExpiryDateValue, value); }
        }

        private string _rowQualityValue;
        public string ROW_QUALITY
        {
            get { return _rowQualityValue; }
            set { SetProperty(ref _rowQualityValue, value); }
        }

        private string _ppdmGuidValue;
        public string PPDM_GUID
        {
            get { return _ppdmGuidValue; }
            set { SetProperty(ref _ppdmGuidValue, value); }
        }
    }
}



