using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.Data.ChokeAnalysis;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ChokeAnalysis
{
    /// <summary>
    /// Represents gas properties for choke calculations
    /// DTO for calculations - Entity class: GAS_CHOKE_PROPERTIES
    /// </summary>
    public partial class GasChokeProperties : ModelEntityBase {
        /// <summary>
        /// Gas specific gravity (relative to air)
        /// </summary>
        private decimal _gasSpecificGravityValue;
        public decimal GasSpecificGravity
        {
            get { return _gasSpecificGravityValue; }
            set { SetProperty(ref _gasSpecificGravityValue, value); }
        }

        /// <summary>
        /// Upstream pressure in psia
        /// </summary>
        private decimal _upstreamPressureValue;
        public decimal UpstreamPressure
        {
            get { return _upstreamPressureValue; }
            set { SetProperty(ref _upstreamPressureValue, value); }
        }

        /// <summary>
        /// Downstream pressure in psia
        /// </summary>
        private decimal _downstreamPressureValue;
        public decimal DownstreamPressure
        {
            get { return _downstreamPressureValue; }
            set { SetProperty(ref _downstreamPressureValue, value); }
        }

        /// <summary>
        /// Temperature in Rankine
        /// </summary>
        private decimal _temperatureValue;
        public decimal Temperature
        {
            get { return _temperatureValue; }
            set { SetProperty(ref _temperatureValue, value); }
        }

        /// <summary>
        /// Z-factor (compressibility factor)
        /// </summary>
        private decimal _zFactorValue;
        public decimal ZFactor
        {
            get { return _zFactorValue; }
            set { SetProperty(ref _zFactorValue, value); }
        }

        /// <summary>
        /// Gas flow rate in Mscf/day
        /// </summary>
        private decimal _flowRateValue;
        public decimal FlowRate
        {
            get { return _flowRateValue; }
            set { SetProperty(ref _flowRateValue, value); }
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
    }
}



