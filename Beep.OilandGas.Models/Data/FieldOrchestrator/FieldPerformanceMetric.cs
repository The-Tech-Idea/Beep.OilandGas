using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FieldPerformanceMetric : ModelEntityBase
    {
        private string MetricNameValue = string.Empty;

        public string MetricName

        {

            get { return this.MetricNameValue; }

            set { SetProperty(ref MetricNameValue, value); }

        }
        private string MetricLabelValue = string.Empty;

        public string MetricLabel

        {

            get { return this.MetricLabelValue; }

            set { SetProperty(ref MetricLabelValue, value); }

        }
        private string MetricTypeValue = string.Empty;

        public string MetricType

        {

            get { return this.MetricTypeValue; }

            set { SetProperty(ref MetricTypeValue, value); }

        } // "count", "volume", "percentage", "currency", "date"
        private object? CurrentValueValue;

        public object? CurrentValue

        {

            get { return this.CurrentValueValue; }

            set { SetProperty(ref CurrentValueValue, value); }

        }
        private object? PreviousValueValue;

        public object? PreviousValue

        {

            get { return this.PreviousValueValue; }

            set { SetProperty(ref PreviousValueValue, value); }

        }
        private object? TargetValueValue;

        public object? TargetValue

        {

            get { return this.TargetValueValue; }

            set { SetProperty(ref TargetValueValue, value); }

        }
        private double? ChangePercentageValue;

        public double? ChangePercentage

        {

            get { return this.ChangePercentageValue; }

            set { SetProperty(ref ChangePercentageValue, value); }

        }
        private string? UnitValue;

        public string? Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }
        private string? PhaseValue;

        public string? Phase

        {

            get { return this.PhaseValue; }

            set { SetProperty(ref PhaseValue, value); }

        } // Exploration, Development, Production, Decommissioning
        private DateTime? AsOfDateValue;

        public DateTime? AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
    }
}
