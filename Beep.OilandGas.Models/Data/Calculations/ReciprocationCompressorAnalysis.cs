using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ReciprocationCompressorAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal CylinderCountValue;

        public decimal CylinderCount

        {

            get { return this.CylinderCountValue; }

            set { SetProperty(ref CylinderCountValue, value); }

        }
        private decimal BoreSizeValue;

        public decimal BoreSize

        {

            get { return this.BoreSizeValue; }

            set { SetProperty(ref BoreSizeValue, value); }

        }
        private decimal StrokeLengthValue;

        public decimal StrokeLength

        {

            get { return this.StrokeLengthValue; }

            set { SetProperty(ref StrokeLengthValue, value); }

        }
        private decimal RPMValue;

        public decimal RPM

        {

            get { return this.RPMValue; }

            set { SetProperty(ref RPMValue, value); }

        }
        private decimal VolumetricFlowRateValue;

        public decimal VolumetricFlowRate

        {

            get { return this.VolumetricFlowRateValue; }

            set { SetProperty(ref VolumetricFlowRateValue, value); }

        }
        private decimal DisplacementVolumeValue;

        public decimal DisplacementVolume

        {

            get { return this.DisplacementVolumeValue; }

            set { SetProperty(ref DisplacementVolumeValue, value); }

        }
        private decimal VolumetricEfficiencyValue;

        public decimal VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        }
        private decimal SuctionPressureValue;

        public decimal SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        }
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        }
        private decimal RodLoadValue;

        public decimal RodLoad

        {

            get { return this.RodLoadValue; }

            set { SetProperty(ref RodLoadValue, value); }

        }
    }
}
