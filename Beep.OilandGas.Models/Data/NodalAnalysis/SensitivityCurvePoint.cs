using System;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
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

    
        /// <summary>
        /// Default constructor
        /// </summary>
        public SensitivityCurvePoint()
        {
            PPDM_GUID = Guid.NewGuid().ToString();
        }
    }
}


