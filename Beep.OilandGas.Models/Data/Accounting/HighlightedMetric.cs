using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class HighlightedMetric : ModelEntityBase
    {
        private System.String LabelValue = string.Empty;
        /// <summary>
        /// Gets or sets label.
        /// </summary>
        public System.String Label
        {
            get { return this.LabelValue; }
            set { SetProperty(ref LabelValue, value); }
        }

        private System.Decimal ValueValue;
        /// <summary>
        /// Gets or sets value.
        /// </summary>
        public System.Decimal Value
        {
            get { return this.ValueValue; }
            set { SetProperty(ref ValueValue, value); }
        }

        private System.String StatusValue = string.Empty;
        /// <summary>
        /// Gets or sets status.
        /// </summary>
        public System.String Status
        {
            get { return this.StatusValue; }
            set { SetProperty(ref StatusValue, value); }
        }

        private System.String InsightValue = string.Empty;
        /// <summary>
        /// Gets or sets insight.
        /// </summary>
        public System.String Insight
        {
            get { return this.InsightValue; }
            set { SetProperty(ref InsightValue, value); }
        }
    }
}
