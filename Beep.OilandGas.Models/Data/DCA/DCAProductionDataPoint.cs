using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DCA
{
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

        
    }
}
