using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FluidComposition : ModelEntityBase
    {
        private string CompositionIdValue = string.Empty;

        public string CompositionId

        {

            get { return this.CompositionIdValue; }

            set { SetProperty(ref CompositionIdValue, value); }

        }
        private string FluidTypeValue = string.Empty;

        public string FluidType

        {

            get { return this.FluidTypeValue; }

            set { SetProperty(ref FluidTypeValue, value); }

        }
        private decimal MolecularWeightValue;

        public decimal MolecularWeight

        {

            get { return this.MolecularWeightValue; }

            set { SetProperty(ref MolecularWeightValue, value); }

        }
        private decimal SpecificGravityValue;

        public decimal SpecificGravity

        {

            get { return this.SpecificGravityValue; }

            set { SetProperty(ref SpecificGravityValue, value); }

        }
        private decimal APIValue;

        public decimal API

        {

            get { return this.APIValue; }

            set { SetProperty(ref APIValue, value); }

        }
        private List<FluidComponent> ComponentsValue = new();

        public List<FluidComponent> Components

        {

            get { return this.ComponentsValue; }

            set { SetProperty(ref ComponentsValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }
}
