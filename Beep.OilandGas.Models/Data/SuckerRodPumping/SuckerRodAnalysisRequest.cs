using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public class SuckerRodAnalysisRequest : ModelEntityBase
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
        private string AnalysisTypeValue = "LOAD";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // LOAD, POWER, PUMP_CARD, OPTIMIZATION
        
        // Well properties (optional, will be retrieved from WELL if not provided)
        private decimal? WellDepthValue;

        public decimal? WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        } // feet
        private decimal? TubingDiameterValue;

        public decimal? TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        } // inches
        private decimal? RodStringLengthValue;

        public decimal? RodStringLength

        {

            get { return this.RodStringLengthValue; }

            set { SetProperty(ref RodStringLengthValue, value); }

        } // feet
        private decimal? RodStringWeightValue;

        public decimal? RodStringWeight

        {

            get { return this.RodStringWeightValue; }

            set { SetProperty(ref RodStringWeightValue, value); }

        } // lb
        private decimal? PumpDepthValue;

        public decimal? PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        } // feet
        private decimal? PumpDiameterValue;

        public decimal? PumpDiameter

        {

            get { return this.PumpDiameterValue; }

            set { SetProperty(ref PumpDiameterValue, value); }

        } // inches
        private decimal? StrokeLengthValue;

        public decimal? StrokeLength

        {

            get { return this.StrokeLengthValue; }

            set { SetProperty(ref StrokeLengthValue, value); }

        } // inches
        private decimal? StrokeRateValue;

        public decimal? StrokeRate

        {

            get { return this.StrokeRateValue; }

            set { SetProperty(ref StrokeRateValue, value); }

        } // strokes/minute
        
        // Fluid properties
        private decimal? FluidLevelValue;

        public decimal? FluidLevel

        {

            get { return this.FluidLevelValue; }

            set { SetProperty(ref FluidLevelValue, value); }

        } // feet
        private decimal? FluidDensityValue;

        public decimal? FluidDensity

        {

            get { return this.FluidDensityValue; }

            set { SetProperty(ref FluidDensityValue, value); }

        } // lb/ft³
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
        
        // Production parameters
        private decimal? ProductionRateValue;

        public decimal? ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        } // bbl/day
        private decimal? VolumetricEfficiencyValue;

        public decimal? VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        } // fraction 0-1
        
        // Additional parameters
        public SuckerRodAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
