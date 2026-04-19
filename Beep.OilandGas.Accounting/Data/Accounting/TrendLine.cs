using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class TrendLine : ModelEntityBase
    {
        private System.String MetricNameValue = string.Empty;
        /// <summary>
        /// Gets or sets metric name.
        /// </summary>
        public System.String MetricName
        {
            get { return this.MetricNameValue; }
            set { SetProperty(ref MetricNameValue, value); }
        }

        private System.Decimal StartValueValue;
        /// <summary>
        /// Gets or sets start value.
        /// </summary>
        public System.Decimal StartValue
        {
            get { return this.StartValueValue; }
            set { SetProperty(ref StartValueValue, value); }
        }

        private System.Decimal EndValueValue;
        /// <summary>
        /// Gets or sets end value.
        /// </summary>
        public System.Decimal EndValue
        {
            get { return this.EndValueValue; }
            set { SetProperty(ref EndValueValue, value); }
        }

        private System.Decimal ChangeValue;
        /// <summary>
        /// Gets or sets change.
        /// </summary>
        public System.Decimal Change
        {
            get { return this.ChangeValue; }
            set { SetProperty(ref ChangeValue, value); }
        }

        private System.Decimal ChangePercentValue;
        /// <summary>
        /// Gets or sets change percent.
        /// </summary>
        public System.Decimal ChangePercent
        {
            get { return this.ChangePercentValue; }
            set { SetProperty(ref ChangePercentValue, value); }
        }

        private System.String DirectionValue = string.Empty;
        /// <summary>
        /// Gets or sets direction.
        /// </summary>
        public System.String Direction
        {
            get { return this.DirectionValue; }
            set { SetProperty(ref DirectionValue, value); }
        }

        private System.Int32 DataPointsValue;
        /// <summary>
        /// Gets or sets data points.
        /// </summary>
        public System.Int32 DataPoints
        {
            get { return this.DataPointsValue; }
            set { SetProperty(ref DataPointsValue, value); }
        }
    }
}
