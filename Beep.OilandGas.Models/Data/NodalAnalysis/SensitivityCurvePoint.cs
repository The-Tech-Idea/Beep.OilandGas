using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.NodalAnalysis
{
    /// <summary>
    /// Represents a single point on a one-way sensitivity curve.
    /// Shows how operating point varies with a single parameter.
    /// DTO for calculations - Entity class: SENSITIVITY_CURVE_POINT
    /// </summary>
    public partial class SensitivityCurvePoint : ModelEntityBase {
        /// <summary>
        /// Parameter value at this point
        /// </summary>
        private double _parameterValueValue;
        public double ParameterValue
        {
            get { return _parameterValueValue; }
            set { SetProperty(ref _parameterValueValue, value); }
        }

        /// <summary>
        /// Flow rate resulting from this parameter value (bbl/day or equivalent)
        /// </summary>
        private double _flowRateValue;
        public double FlowRate
        {
            get { return _flowRateValue; }
            set { SetProperty(ref _flowRateValue, value); }
        }

        /// <summary>
        /// Percentage change from base case flow rate
        /// </summary>
        private double _percentageChangeValue;
        public double PercentageChange
        {
            get { return _percentageChangeValue; }
            set { SetProperty(ref _percentageChangeValue, value); }
        }

        /// <summary>
        /// Sequence order on the sensitivity curve
        /// </summary>
        private int? _sequenceValue;
        public int? Sequence
        {
            get { return _sequenceValue; }
            set { SetProperty(ref _sequenceValue, value); }
        }

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

        /// <summary>
        /// Default constructor
        /// </summary>
        public SensitivityCurvePoint()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}
