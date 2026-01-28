using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class PipelineFlowRequest : ModelEntityBase
    {
        private string PipelineIdValue = string.Empty;

        public string PipelineId

        {

            get { return this.PipelineIdValue; }

            set { SetProperty(ref PipelineIdValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private decimal PressureValue;

        public decimal Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private string? ProductTypeValue;

        public string? ProductType

        {

            get { return this.ProductTypeValue; }

            set { SetProperty(ref ProductTypeValue, value); }

        }
        private DateTime FlowDateValue;

        public DateTime FlowDate

        {

            get { return this.FlowDateValue; }

            set { SetProperty(ref FlowDateValue, value); }

        }
    }
}
