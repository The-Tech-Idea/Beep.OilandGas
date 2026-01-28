using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Storage
{
    public class CreateLACTUnitRequest : ModelEntityBase
    {
        private string LactNameValue;

        public string LactName

        {

            get { return this.LactNameValue; }

            set { SetProperty(ref LactNameValue, value); }

        }
        private string ServiceUnitIdValue;

        public string ServiceUnitId

        {

            get { return this.ServiceUnitIdValue; }

            set { SetProperty(ref ServiceUnitIdValue, value); }

        }
        private string MeterTypeValue;

        public string MeterType

        {

            get { return this.MeterTypeValue; }

            set { SetProperty(ref MeterTypeValue, value); }

        }
        private decimal MaximumFlowRateValue;

        public decimal MaximumFlowRate

        {

            get { return this.MaximumFlowRateValue; }

            set { SetProperty(ref MaximumFlowRateValue, value); }

        }
        private decimal MeterFactorValue = 1.0m;

        public decimal MeterFactor

        {

            get { return this.MeterFactorValue; }

            set { SetProperty(ref MeterFactorValue, value); }

        }
    }
}
