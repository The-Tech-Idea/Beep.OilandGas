using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class MultiphaseChokeAnalysis : ModelEntityBase
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
        private decimal OilFlowRateValue;

        public decimal OilFlowRate

        {

            get { return this.OilFlowRateValue; }

            set { SetProperty(ref OilFlowRateValue, value); }

        } // STB/day
        private decimal WaterFlowRateValue;

        public decimal WaterFlowRate

        {

            get { return this.WaterFlowRateValue; }

            set { SetProperty(ref WaterFlowRateValue, value); }

        } // STB/day
        private decimal GasFlowRateValue;

        public decimal GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        } // Mscf/day
        private decimal OilDensityValue;

        public decimal OilDensity

        {

            get { return this.OilDensityValue; }

            set { SetProperty(ref OilDensityValue, value); }

        } // lb/ft³
        private decimal WaterDensityValue;

        public decimal WaterDensity

        {

            get { return this.WaterDensityValue; }

            set { SetProperty(ref WaterDensityValue, value); }

        } // lb/ft³
        private decimal GasDensityValue;

        public decimal GasDensity

        {

            get { return this.GasDensityValue; }

            set { SetProperty(ref GasDensityValue, value); }

        } // lb/ft³
        private decimal OilViscosityValue;

        public decimal OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        } // cp
        private decimal WaterViscosityValue;

        public decimal WaterViscosity

        {

            get { return this.WaterViscosityValue; }

            set { SetProperty(ref WaterViscosityValue, value); }

        } // cp
        private decimal GasViscosityValue;

        public decimal GasViscosity

        {

            get { return this.GasViscosityValue; }

            set { SetProperty(ref GasViscosityValue, value); }

        } // cp
        private decimal SurfaceTensionValue;

        public decimal SurfaceTension

        {

            get { return this.SurfaceTensionValue; }

            set { SetProperty(ref SurfaceTensionValue, value); }

        } // dyne/cm
        private string FlowPatternValue = string.Empty;

        public string FlowPattern

        {

            get { return this.FlowPatternValue; }

            set { SetProperty(ref FlowPatternValue, value); }

        } // Bubbly, Slug, Annular, Dispersed
        private decimal MixtureDensityValue;

        public decimal MixtureDensity

        {

            get { return this.MixtureDensityValue; }

            set { SetProperty(ref MixtureDensityValue, value); }

        }
        private decimal MixtureViscosityValue;

        public decimal MixtureViscosity

        {

            get { return this.MixtureViscosityValue; }

            set { SetProperty(ref MixtureViscosityValue, value); }

        }
        private decimal HomogeneousVoidFractionValue;

        public decimal HomogeneousVoidFraction

        {

            get { return this.HomogeneousVoidFractionValue; }

            set { SetProperty(ref HomogeneousVoidFractionValue, value); }

        } // Quality
        private decimal TotalPressureDropValue;

        public decimal TotalPressureDrop

        {

            get { return this.TotalPressureDropValue; }

            set { SetProperty(ref TotalPressureDropValue, value); }

        }
        private decimal AccelerationPressureDropValue;

        public decimal AccelerationPressureDrop

        {

            get { return this.AccelerationPressureDropValue; }

            set { SetProperty(ref AccelerationPressureDropValue, value); }

        }
        private decimal FrictionalPressureDropValue;

        public decimal FrictionalPressureDrop

        {

            get { return this.FrictionalPressureDropValue; }

            set { SetProperty(ref FrictionalPressureDropValue, value); }

        }
        private decimal ElevationPressureDropValue;

        public decimal ElevationPressureDrop

        {

            get { return this.ElevationPressureDropValue; }

            set { SetProperty(ref ElevationPressureDropValue, value); }

        }
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        }
    }
}
