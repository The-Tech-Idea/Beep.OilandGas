using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PipelineResponse : ModelEntityBase
    {
        private string PipelineIdValue = string.Empty;

        public string PipelineId

        {

            get { return this.PipelineIdValue; }

            set { SetProperty(ref PipelineIdValue, value); }

        }
        private string PipelineNameValue = string.Empty;

        public string PipelineName

        {

            get { return this.PipelineNameValue; }

            set { SetProperty(ref PipelineNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Pipeline classification
        private string? PipelineTypeValue;

        public string? PipelineType

        {

            get { return this.PipelineTypeValue; }

            set { SetProperty(ref PipelineTypeValue, value); }

        }
        private string? FluidTypeValue;

        public string? FluidType

        {

            get { return this.FluidTypeValue; }

            set { SetProperty(ref FluidTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Route information
        private string? OriginFacilityIdValue;

        public string? OriginFacilityId

        {

            get { return this.OriginFacilityIdValue; }

            set { SetProperty(ref OriginFacilityIdValue, value); }

        }
        private string? DestinationFacilityIdValue;

        public string? DestinationFacilityId

        {

            get { return this.DestinationFacilityIdValue; }

            set { SetProperty(ref DestinationFacilityIdValue, value); }

        }
        private decimal? PipelineLengthValue;

        public decimal? PipelineLength

        {

            get { return this.PipelineLengthValue; }

            set { SetProperty(ref PipelineLengthValue, value); }

        }
        private string? PipelineLengthOuomValue;

        public string? PipelineLengthOuom

        {

            get { return this.PipelineLengthOuomValue; }

            set { SetProperty(ref PipelineLengthOuomValue, value); }

        }
        private decimal? DiameterValue;

        public decimal? Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        }
        private string? DiameterOuomValue;

        public string? DiameterOuom

        {

            get { return this.DiameterOuomValue; }

            set { SetProperty(ref DiameterOuomValue, value); }

        }
        private decimal? WallThicknessValue;

        public decimal? WallThickness

        {

            get { return this.WallThicknessValue; }

            set { SetProperty(ref WallThicknessValue, value); }

        }
        private string? WallThicknessOuomValue;

        public string? WallThicknessOuom

        {

            get { return this.WallThicknessOuomValue; }

            set { SetProperty(ref WallThicknessOuomValue, value); }

        }
        
        // Specifications
        private decimal? DesignPressureValue;

        public decimal? DesignPressure

        {

            get { return this.DesignPressureValue; }

            set { SetProperty(ref DesignPressureValue, value); }

        }
        private string? DesignPressureOuomValue;

        public string? DesignPressureOuom

        {

            get { return this.DesignPressureOuomValue; }

            set { SetProperty(ref DesignPressureOuomValue, value); }

        }
        private decimal? OperatingPressureValue;

        public decimal? OperatingPressure

        {

            get { return this.OperatingPressureValue; }

            set { SetProperty(ref OperatingPressureValue, value); }

        }
        private string? OperatingPressureOuomValue;

        public string? OperatingPressureOuom

        {

            get { return this.OperatingPressureOuomValue; }

            set { SetProperty(ref OperatingPressureOuomValue, value); }

        }
        private decimal? FlowCapacityValue;

        public decimal? FlowCapacity

        {

            get { return this.FlowCapacityValue; }

            set { SetProperty(ref FlowCapacityValue, value); }

        }
        private string? FlowCapacityOuomValue;

        public string? FlowCapacityOuom

        {

            get { return this.FlowCapacityOuomValue; }

            set { SetProperty(ref FlowCapacityOuomValue, value); }

        }
        private string? MaterialValue;

        public string? Material

        {

            get { return this.MaterialValue; }

            set { SetProperty(ref MaterialValue, value); }

        }
        private string? CoatingTypeValue;

        public string? CoatingType

        {

            get { return this.CoatingTypeValue; }

            set { SetProperty(ref CoatingTypeValue, value); }

        }
        
        // Dates
        private DateTime? ConstructionStartDateValue;

        public DateTime? ConstructionStartDate

        {

            get { return this.ConstructionStartDateValue; }

            set { SetProperty(ref ConstructionStartDateValue, value); }

        }
        private DateTime? ConstructionEndDateValue;

        public DateTime? ConstructionEndDate

        {

            get { return this.ConstructionEndDateValue; }

            set { SetProperty(ref ConstructionEndDateValue, value); }

        }
        private DateTime? CommissionDateValue;

        public DateTime? CommissionDate

        {

            get { return this.CommissionDateValue; }

            set { SetProperty(ref CommissionDateValue, value); }

        }
        private DateTime? DecommissionDateValue;

        public DateTime? DecommissionDate

        {

            get { return this.DecommissionDateValue; }

            set { SetProperty(ref DecommissionDateValue, value); }

        }
        
        // Cost information
        private decimal? ConstructionCostValue;

        public decimal? ConstructionCost

        {

            get { return this.ConstructionCostValue; }

            set { SetProperty(ref ConstructionCostValue, value); }

        }
        private string? ConstructionCostCurrencyValue;

        public string? ConstructionCostCurrency

        {

            get { return this.ConstructionCostCurrencyValue; }

            set { SetProperty(ref ConstructionCostCurrencyValue, value); }

        }
        
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
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }
        
        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
    }
}
