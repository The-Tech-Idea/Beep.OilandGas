using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class KPI : ModelEntityBase
    {
        private System.String NameValue = string.Empty;
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public System.String Name
        {
            get { return this.NameValue; }
            set { SetProperty(ref NameValue, value); }
        }

        private System.Decimal ValueValue;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public System.Decimal Value
        {
            get { return this.ValueValue; }
            set { SetProperty(ref ValueValue, value); }
        }

        private System.String FormatValue = string.Empty;
        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        public System.String Format
        {
            get { return this.FormatValue; }
            set { SetProperty(ref FormatValue, value); }
        }

        private System.String StatusValue = string.Empty;
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public System.String Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }

        private System.String TrendValue = string.Empty;
        /// <summary>
        /// Gets or sets the trend.
        /// </summary>
        public System.String Trend
        {
            get { return this.TrendValue; }
            set { SetProperty(ref TrendValue, value); }
        }

        private System.Decimal TargetValue;
        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        public System.Decimal Target
        {
            get { return this.TargetValue; }
            set { SetProperty(ref TargetValue, value); }
        }

        private System.Decimal ThresholdValue;
        /// <summary>
        /// Gets or sets the threshold.
        /// </summary>
        public System.Decimal Threshold
        {
            get { return this.ThresholdValue; }
            set { SetProperty(ref ThresholdValue, value); }
        }
    }
}
