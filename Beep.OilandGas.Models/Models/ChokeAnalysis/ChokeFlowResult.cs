using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.Models.ChokeAnalysis;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.ChokeAnalysis
{
    /// <summary>
    /// Represents choke flow calculation results
    /// DTO for calculations - Entity class: CHOKE_FLOW_RESULT
    /// </summary>
    public partial class ChokeFlowResult : Entity, IPPDMEntity
    {
        /// <summary>
        /// Calculated flow rate in Mscf/day
        /// </summary>
        private decimal _flowRateValue;
        public decimal FlowRate
        {
            get { return _flowRateValue; }
            set { SetProperty(ref _flowRateValue, value); }
        }

        /// <summary>
        /// Calculated downstream pressure in psia
        /// </summary>
        private decimal _downstreamPressureValue;
        public decimal DownstreamPressure
        {
            get { return _downstreamPressureValue; }
            set { SetProperty(ref _downstreamPressureValue, value); }
        }

        /// <summary>
        /// Calculated upstream pressure in psia
        /// </summary>
        private decimal _upstreamPressureValue;
        public decimal UpstreamPressure
        {
            get { return _upstreamPressureValue; }
            set { SetProperty(ref _upstreamPressureValue, value); }
        }

        /// <summary>
        /// Pressure ratio (P2/P1)
        /// </summary>
        private decimal _pressureRatioValue;
        public decimal PressureRatio
        {
            get { return _pressureRatioValue; }
            set { SetProperty(ref _pressureRatioValue, value); }
        }

        /// <summary>
        /// Flow regime (subsonic or sonic)
        /// </summary>
        private FlowRegime _flowRegimeValue;
        public FlowRegime FlowRegime
        {
            get { return _flowRegimeValue; }
            set { SetProperty(ref _flowRegimeValue, value); }
        }

        /// <summary>
        /// Critical pressure ratio
        /// </summary>
        private decimal _criticalPressureRatioValue;
        public decimal CriticalPressureRatio
        {
            get { return _criticalPressureRatioValue; }
            set { SetProperty(ref _criticalPressureRatioValue, value); }
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



