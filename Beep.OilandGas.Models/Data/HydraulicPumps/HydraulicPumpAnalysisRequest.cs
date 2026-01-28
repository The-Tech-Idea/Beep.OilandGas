using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class HydraulicPumpAnalysisRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

        } // WELL_EQUIPMENT ROW_ID
        private string AnalysisTypeValue = "PERFORMANCE";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // PERFORMANCE, DESIGN, EFFICIENCY
        
        // Well properties (optional, will be retrieved from WELL if not provided)
        private decimal? WellDepthValue;

        public decimal? WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        } // feet
        private decimal? PumpDepthValue;

        public decimal? PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        } // feet
        private decimal? TubingDiameterValue;

        public decimal? TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        } // inches
        
        // Pump properties
        private decimal? NozzleSizeValue;

        public decimal? NozzleSize

        {

            get { return this.NozzleSizeValue; }

            set { SetProperty(ref NozzleSizeValue, value); }

        } // inches
        private decimal? ThroatSizeValue;

        public decimal? ThroatSize

        {

            get { return this.ThroatSizeValue; }

            set { SetProperty(ref ThroatSizeValue, value); }

        } // inches
        private decimal? PowerFluidPressureValue;

        public decimal? PowerFluidPressure

        {

            get { return this.PowerFluidPressureValue; }

            set { SetProperty(ref PowerFluidPressureValue, value); }

        } // psia
        private decimal? PowerFluidRateValue;

        public decimal? PowerFluidRate

        {

            get { return this.PowerFluidRateValue; }

            set { SetProperty(ref PowerFluidRateValue, value); }

        } // bbl/day
        private decimal? PowerFluidDensityValue;

        public decimal? PowerFluidDensity

        {

            get { return this.PowerFluidDensityValue; }

            set { SetProperty(ref PowerFluidDensityValue, value); }

        } // lb/ft³
        
        // Production parameters
        private decimal? ProductionRateValue;

        public decimal? ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        } // bbl/day
        private decimal? DischargePressureValue;

        public decimal? DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        } // psia
        private decimal? SuctionPressureValue;

        public decimal? SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        } // psia
        
        // Fluid properties
        private decimal? OilGravityValue;

        public decimal? OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        } // API
        private decimal? WaterCutValue;

        public decimal? WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        } // fraction 0-1
        private decimal? GasOilRatioValue;

        public decimal? GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        } // scf/bbl
        
        // Additional parameters
        public HydraulicPumpAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
