using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class VolumetricAnalysisRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private decimal GrossRockVolumeValue;

        public decimal GrossRockVolume

        {

            get { return this.GrossRockVolumeValue; }

            set { SetProperty(ref GrossRockVolumeValue, value); }

        }
        private decimal NetToGrossRatioValue;

        public decimal NetToGrossRatio

        {

            get { return this.NetToGrossRatioValue; }

            set { SetProperty(ref NetToGrossRatioValue, value); }

        }
        private decimal PorosityValue;

        public decimal Porosity

        {

            get { return this.PorosityValue; }

            set { SetProperty(ref PorosityValue, value); }

        }
        private decimal WaterSaturationValue;

        public decimal WaterSaturation

        {

            get { return this.WaterSaturationValue; }

            set { SetProperty(ref WaterSaturationValue, value); }

        }
        private decimal FormationVolumeFactorValue;

        public decimal FormationVolumeFactor

        {

            get { return this.FormationVolumeFactorValue; }

            set { SetProperty(ref FormationVolumeFactorValue, value); }

        }
        private decimal RecoveryFactorValue;

        public decimal RecoveryFactor

        {

            get { return this.RecoveryFactorValue; }

            set { SetProperty(ref RecoveryFactorValue, value); }

        }
        private bool IncludeUncertaintyValue = true;

        public bool IncludeUncertainty

        {

            get { return this.IncludeUncertaintyValue; }

            set { SetProperty(ref IncludeUncertaintyValue, value); }

        }
        private string AnalysisMethodologyValue = "Deterministic";

        public string AnalysisMethodology

        {

            get { return this.AnalysisMethodologyValue; }

            set { SetProperty(ref AnalysisMethodologyValue, value); }

        }
    }
}
