using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class QualityMeasurementSystem : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the system identifier.
        /// </summary>
        private string SystemIdValue = string.Empty;

        public string SystemId

        {

            get { return this.SystemIdValue; }

            set { SetProperty(ref SystemIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the system type.
        /// </summary>
        private string SystemTypeValue = string.Empty;

        public string SystemType

        {

            get { return this.SystemTypeValue; }

            set { SetProperty(ref SystemTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets whether automatic sampling is enabled.
        /// </summary>
        private bool AutomaticSamplingEnabledValue;

        public bool AutomaticSamplingEnabled

        {

            get { return this.AutomaticSamplingEnabledValue; }

            set { SetProperty(ref AutomaticSamplingEnabledValue, value); }

        }
    }
}
