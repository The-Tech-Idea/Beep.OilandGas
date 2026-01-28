using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PumpTypeSelection : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private List<PumpTypeOption> OptionsValue = new();

        public List<PumpTypeOption> Options

        {

            get { return this.OptionsValue; }

            set { SetProperty(ref OptionsValue, value); }

        }
        private string RecommendedTypeValue = string.Empty;

        public string RecommendedType

        {

            get { return this.RecommendedTypeValue; }

            set { SetProperty(ref RecommendedTypeValue, value); }

        }
        private string SelectionRationaleValue = string.Empty;

        public string SelectionRationale

        {

            get { return this.SelectionRationaleValue; }

            set { SetProperty(ref SelectionRationaleValue, value); }

        }
    }
}
