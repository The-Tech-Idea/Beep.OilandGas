using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class LACTUnit : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the LACT unit identifier.
        /// </summary>
        private string LACTUnitIdValue = string.Empty;

        public string LACTUnitId

        {

            get { return this.LACTUnitIdValue; }

            set { SetProperty(ref LACTUnitIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the LACT unit name.
        /// </summary>
        private string LACTUnitNameValue = string.Empty;

        public string LACTUnitName

        {

            get { return this.LACTUnitNameValue; }

            set { SetProperty(ref LACTUnitNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the meter configuration.
        /// </summary>
        private MeterConfiguration? MeterConfigurationValue;

        public MeterConfiguration? MeterConfiguration

        {

            get { return this.MeterConfigurationValue; }

            set { SetProperty(ref MeterConfigurationValue, value); }

        }

        /// <summary>
        /// Gets or sets the quality measurement system.
        /// </summary>
        private QualityMeasurementSystem? QualityMeasurementSystemValue;

        public QualityMeasurementSystem? QualityMeasurementSystem

        {

            get { return this.QualityMeasurementSystemValue; }

            set { SetProperty(ref QualityMeasurementSystemValue, value); }

        }

        /// <summary>
        /// Gets or sets the transfer records.
        /// </summary>
        private List<LACTTransferRecord> TransferRecordsValue = new();

        public List<LACTTransferRecord> TransferRecords

        {

            get { return this.TransferRecordsValue; }

            set { SetProperty(ref TransferRecordsValue, value); }

        }
    }
}
