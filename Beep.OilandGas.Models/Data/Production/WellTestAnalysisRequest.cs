using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellTestAnalysisRequest : ModelEntityBase
    {
        private string? AnalysisIdValue;

        public string? AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        
        // Analysis parameters
        private decimal? PermeabilityValue;

        public decimal? Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private string? PermeabilityOuomValue;

        public string? PermeabilityOuom

        {

            get { return this.PermeabilityOuomValue; }

            set { SetProperty(ref PermeabilityOuomValue, value); }

        } // e.g., "MD", "DARCY"
        private decimal? SkinValue;

        public decimal? Skin

        {

            get { return this.SkinValue; }

            set { SetProperty(ref SkinValue, value); }

        }
        private decimal? ProductivityIndexValue;

        public decimal? ProductivityIndex

        {

            get { return this.ProductivityIndexValue; }

            set { SetProperty(ref ProductivityIndexValue, value); }

        }
        private string? ProductivityIndexOuomValue;

        public string? ProductivityIndexOuom

        {

            get { return this.ProductivityIndexOuomValue; }

            set { SetProperty(ref ProductivityIndexOuomValue, value); }

        }
        
        // Flow potential
        private decimal? AofPotentialValue;

        public decimal? AofPotential

        {

            get { return this.AofPotentialValue; }

            set { SetProperty(ref AofPotentialValue, value); }

        } // Absolute Open Flow
        private string? AofPotentialOuomValue;

        public string? AofPotentialOuom

        {

            get { return this.AofPotentialOuomValue; }

            set { SetProperty(ref AofPotentialOuomValue, value); }

        }
        
        // Wellbore storage
        private decimal? WellboreStorageCoeffValue;

        public decimal? WellboreStorageCoeff

        {

            get { return this.WellboreStorageCoeffValue; }

            set { SetProperty(ref WellboreStorageCoeffValue, value); }

        }
        private string? WellboreStorageOuomValue;

        public string? WellboreStorageOuom

        {

            get { return this.WellboreStorageOuomValue; }

            set { SetProperty(ref WellboreStorageOuomValue, value); }

        }
        
        // Flow efficiency
        private decimal? FlowEfficiencyValue;

        public decimal? FlowEfficiency

        {

            get { return this.FlowEfficiencyValue; }

            set { SetProperty(ref FlowEfficiencyValue, value); }

        } // Percentage
        
        // Analysis method
        private string? AnalysisMethodValue;

        public string? AnalysisMethod

        {

            get { return this.AnalysisMethodValue; }

            set { SetProperty(ref AnalysisMethodValue, value); }

        } // e.g., "HORNER", "MDH", "AGARWAL"
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
    }
}
