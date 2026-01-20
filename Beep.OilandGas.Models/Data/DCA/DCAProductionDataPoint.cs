using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.DCA
{
    /// <summary>
    /// Production data point for DCA analysis.
    /// DTO for calculations - Entity class: DCA_PRODUCTION_DATA
    /// </summary>
    public partial class DCAProductionDataPoint : ModelEntityBase {
        /// <summary>
        /// Production rate at this time point.
        /// </summary>
        private decimal _productionRateValue;
        public decimal ProductionRate
        {
            get { return _productionRateValue; }
            set { SetProperty(ref _productionRateValue, value); }
        }

        /// <summary>
        /// Time since start of production.
        /// </summary>
        private decimal _timeSinceStartValue;
        public decimal TimeSinceStart
        {
            get { return _timeSinceStartValue; }
            set { SetProperty(ref _timeSinceStartValue, value); }
        }

        /// <summary>
        /// Date and time of the production data point.
        /// </summary>
        private DateTime? _productionDateTimeValue;
        public DateTime? ProductionDateTime
        {
            get { return _productionDateTimeValue; }
            set { SetProperty(ref _productionDateTimeValue, value); }
        }

        /// <summary>
        /// Well identifier for this production data point.
        /// </summary>
        private string _wellIdValue;
        public string WellId
        {
            get { return _wellIdValue; }
            set { SetProperty(ref _wellIdValue, value); }
        }

        /// <summary>
        /// Field identifier for this production data point.
        /// </summary>
        private string _fieldIdValue;
        public string FieldId
        {
            get { return _fieldIdValue; }
            set { SetProperty(ref _fieldIdValue, value); }
        }

        /// <summary>
        /// Cumulative production at this time point.
        /// </summary>
        private decimal _cumulativeProductionValue;
        public decimal CumulativeProduction
        {
            get { return _cumulativeProductionValue; }
            set { SetProperty(ref _cumulativeProductionValue, value); }
        }

        /// <summary>
        /// Production type (oil, gas, water, etc.).
        /// </summary>
        private string _productionTypeValue;
        public string ProductionType
        {
            get { return _productionTypeValue; }
            set { SetProperty(ref _productionTypeValue, value); }
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

        private string _dcaProductionDataIdValue;
        public string DCA_PRODUCTION_DATA_ID
        {
            get { return _dcaProductionDataIdValue; }
            set { SetProperty(ref _dcaProductionDataIdValue, value); }
        }
    }
}