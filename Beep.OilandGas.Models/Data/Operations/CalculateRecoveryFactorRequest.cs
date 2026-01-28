using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Operations
{
    public class CalculateRecoveryFactorRequest : ModelEntityBase
    {
        private string ProjectIdValue = string.Empty;

        public string ProjectId

        {

            get { return this.ProjectIdValue; }

            set { SetProperty(ref ProjectIdValue, value); }

        }
    }
}
