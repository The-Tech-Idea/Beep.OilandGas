using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class PipelineCapacityResponse : ModelEntityBase
    {
        private string PipelineIdValue = string.Empty;

        public string PipelineId

        {

            get { return this.PipelineIdValue; }

            set { SetProperty(ref PipelineIdValue, value); }

        }
        private decimal MaximumCapacityValue;

        public decimal MaximumCapacity

        {

            get { return this.MaximumCapacityValue; }

            set { SetProperty(ref MaximumCapacityValue, value); }

        }
        private decimal CurrentUtilizationValue;

        public decimal CurrentUtilization

        {

            get { return this.CurrentUtilizationValue; }

            set { SetProperty(ref CurrentUtilizationValue, value); }

        }
        private decimal AvailableCapacityValue;

        public decimal AvailableCapacity

        {

            get { return this.AvailableCapacityValue; }

            set { SetProperty(ref AvailableCapacityValue, value); }

        }
        private DateTime ReportDateValue;

        public DateTime ReportDate

        {

            get { return this.ReportDateValue; }

            set { SetProperty(ref ReportDateValue, value); }

        }
    }
}
