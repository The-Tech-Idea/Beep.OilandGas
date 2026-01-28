using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class AllocationBase : ModelEntityBase
    {
        private System.String CostCenterIdValue = string.Empty;
        /// <summary>
        /// Gets or sets the cost center identifier.
        /// </summary>
        public System.String CostCenterId
        {
            get { return this.CostCenterIdValue; }
            set { SetProperty(ref CostCenterIdValue, value); }
        }

        private System.String ActivityNameValue = string.Empty;
        /// <summary>
        /// Gets or sets the activity name.
        /// </summary>
        public System.String ActivityName
        {
            get { return this.ActivityNameValue; }
            set { SetProperty(ref ActivityNameValue, value); }
        }

        private System.Decimal ActivityPercentValue;
        /// <summary>
        /// Gets or sets the activity percent.
        /// </summary>
        public System.Decimal ActivityPercent
        {
            get { return this.ActivityPercentValue; }
            set { SetProperty(ref ActivityPercentValue, value); }
        }

        private System.Decimal ActivityUnitsValue;
        /// <summary>
        /// Gets or sets the activity units.
        /// </summary>
        public System.Decimal ActivityUnits
        {
            get { return this.ActivityUnitsValue; }
            set { SetProperty(ref ActivityUnitsValue, value); }
        }
    }
}
