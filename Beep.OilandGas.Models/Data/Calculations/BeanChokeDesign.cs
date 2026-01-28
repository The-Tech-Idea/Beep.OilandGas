using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class BeanChokeDesign : ModelEntityBase
    {
        private string DesignIdValue = string.Empty;

        public string DesignId

        {

            get { return this.DesignIdValue; }

            set { SetProperty(ref DesignIdValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private decimal DesiredFlowRateValue;

        public decimal DesiredFlowRate

        {

            get { return this.DesiredFlowRateValue; }

            set { SetProperty(ref DesiredFlowRateValue, value); }

        }
        private decimal UpstreamPressureValue;

        public decimal UpstreamPressure

        {

            get { return this.UpstreamPressureValue; }

            set { SetProperty(ref UpstreamPressureValue, value); }

        }
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        }
        private decimal TemperatureValue;

        public decimal Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private decimal GasSpecificGravityValue;

        public decimal GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private string TrimMaterialValue = string.Empty;

        public string TrimMaterial

        {

            get { return this.TrimMaterialValue; }

            set { SetProperty(ref TrimMaterialValue, value); }

        } // WC, Tungsten Carbide, Steel
        private decimal RecommendedChokeDiameterValue;

        public decimal RecommendedChokeDiameter

        {

            get { return this.RecommendedChokeDiameterValue; }

            set { SetProperty(ref RecommendedChokeDiameterValue, value); }

        }
        private decimal MinimumChokeDiameterValue;

        public decimal MinimumChokeDiameter

        {

            get { return this.MinimumChokeDiameterValue; }

            set { SetProperty(ref MinimumChokeDiameterValue, value); }

        }
        private decimal MaximumChokeDiameterValue;

        public decimal MaximumChokeDiameter

        {

            get { return this.MaximumChokeDiameterValue; }

            set { SetProperty(ref MaximumChokeDiameterValue, value); }

        }
        private decimal DischargeCoefficientValue;

        public decimal DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
        private decimal SurfaceAreaValue;

        public decimal SurfaceArea

        {

            get { return this.SurfaceAreaValue; }

            set { SetProperty(ref SurfaceAreaValue, value); }

        }
        private string RecommendedSeriesValue = string.Empty;

        public string RecommendedSeries

        {

            get { return this.RecommendedSeriesValue; }

            set { SetProperty(ref RecommendedSeriesValue, value); }

        } // AX, BX, CX, DX
        private decimal EstimatedErosionRateValue;

        public decimal EstimatedErosionRate

        {

            get { return this.EstimatedErosionRateValue; }

            set { SetProperty(ref EstimatedErosionRateValue, value); }

        } // mils/year
        private decimal DesignLifeValue;

        public decimal DesignLife

        {

            get { return this.DesignLifeValue; }

            set { SetProperty(ref DesignLifeValue, value); }

        } // years
        private string ManufacturerRecommendationValue = string.Empty;

        public string ManufacturerRecommendation

        {

            get { return this.ManufacturerRecommendationValue; }

            set { SetProperty(ref ManufacturerRecommendationValue, value); }

        }
    }
}
