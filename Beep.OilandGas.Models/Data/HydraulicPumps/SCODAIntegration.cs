using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SCODAIntegration : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private bool IsIntegratedValue;

        public bool IsIntegrated

        {

            get { return this.IsIntegratedValue; }

            set { SetProperty(ref IsIntegratedValue, value); }

        }
        private List<IntegrationParameter> ParametersValue = new();

        public List<IntegrationParameter> Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
        private string IntegrationStatusValue = string.Empty;

        public string IntegrationStatus

        {

            get { return this.IntegrationStatusValue; }

            set { SetProperty(ref IntegrationStatusValue, value); }

        }
    }
}
