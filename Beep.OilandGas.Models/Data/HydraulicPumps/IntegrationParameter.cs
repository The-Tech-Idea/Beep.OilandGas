using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class IntegrationParameter : ModelEntityBase
    {
        private string ParameterNameValue = string.Empty;

        public string ParameterName

        {

            get { return this.ParameterNameValue; }

            set { SetProperty(ref ParameterNameValue, value); }

        }
        private string DataTypeValue = string.Empty;

        public string DataType

        {

            get { return this.DataTypeValue; }

            set { SetProperty(ref DataTypeValue, value); }

        }
        private string UpdateFrequencyValue = string.Empty;

        public string UpdateFrequency

        {

            get { return this.UpdateFrequencyValue; }

            set { SetProperty(ref UpdateFrequencyValue, value); }

        }
    }
}
