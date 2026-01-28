using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PipelineAnalysisRequest : ModelEntityBase
    {
        private string? PipelineIdValue;

        public string? PipelineId

        {

            get { return this.PipelineIdValue; }

            set { SetProperty(ref PipelineIdValue, value); }

        }
        private string PipelineTypeValue = "GAS";

        public string PipelineType

        {

            get { return this.PipelineTypeValue; }

            set { SetProperty(ref PipelineTypeValue, value); }

        } // GAS, LIQUID
        private string AnalysisTypeValue = "CAPACITY";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // CAPACITY, FLOW_RATE, PRESSURE_DROP
        
        // Pipeline properties (optional, will be retrieved from PIPELINE if not provided)
        private decimal? LengthValue;

        public decimal? Length

        {

            get { return this.LengthValue; }

            set { SetProperty(ref LengthValue, value); }

        } // miles or feet
        private decimal? DiameterValue;

        public decimal? Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        } // inches
        private decimal? RoughnessValue;

        public decimal? Roughness

        {

            get { return this.RoughnessValue; }

            set { SetProperty(ref RoughnessValue, value); }

        } // inches (absolute roughness)
        private decimal? ElevationChangeValue;

        public decimal? ElevationChange

        {

            get { return this.ElevationChangeValue; }

            set { SetProperty(ref ElevationChangeValue, value); }

        } // feet
        
        // Flow conditions
        private decimal? InletPressureValue;

        public decimal? InletPressure

        {

            get { return this.InletPressureValue; }

            set { SetProperty(ref InletPressureValue, value); }

        } // psia
        private decimal? OutletPressureValue;

        public decimal? OutletPressure

        {

            get { return this.OutletPressureValue; }

            set { SetProperty(ref OutletPressureValue, value); }

        } // psia
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // Mscf/day (gas) or bbl/day (liquid)
        private decimal? TemperatureValue;

        public decimal? Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        } // Rankine
        
        // Product properties
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        } // for gas pipelines
        private decimal? LiquidDensityValue;

        public decimal? LiquidDensity

        {

            get { return this.LiquidDensityValue; }

            set { SetProperty(ref LiquidDensityValue, value); }

        } // lb/ft³ (for liquid pipelines)
        private decimal? LiquidViscosityValue;

        public decimal? LiquidViscosity

        {

            get { return this.LiquidViscosityValue; }

            set { SetProperty(ref LiquidViscosityValue, value); }

        } // cP (for liquid pipelines)
        private decimal? ZFactorValue;

        public decimal? ZFactor

        {

            get { return this.ZFactorValue; }

            set { SetProperty(ref ZFactorValue, value); }

        } // for gas pipelines
        
        // Additional parameters
        public PipelineAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
