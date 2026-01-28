using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class EOSPhase : ModelEntityBase
    {
        private string PhaseTypeValue = string.Empty;

        public string PhaseType

        {

            get { return this.PhaseTypeValue; }

            set { SetProperty(ref PhaseTypeValue, value); }

        }
        private decimal MoleFractionValue;

        public decimal MoleFraction

        {

            get { return this.MoleFractionValue; }

            set { SetProperty(ref MoleFractionValue, value); }

        }
        private decimal DensityValue;

        public decimal Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        }
        private decimal CompressibilityValue;

        public decimal Compressibility

        {

            get { return this.CompressibilityValue; }

            set { SetProperty(ref CompressibilityValue, value); }

        }
        private decimal FugacityValue;

        public decimal Fugacity

        {

            get { return this.FugacityValue; }

            set { SetProperty(ref FugacityValue, value); }

        }
        private FluidComposition CompositionValue = new();

        public FluidComposition Composition

        {

            get { return this.CompositionValue; }

            set { SetProperty(ref CompositionValue, value); }

        }
    }
}
