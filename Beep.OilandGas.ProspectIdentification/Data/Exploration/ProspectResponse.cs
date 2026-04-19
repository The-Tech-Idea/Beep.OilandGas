using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProspectResponse : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string ProspectNameValue = string.Empty;

        public string ProspectName

        {

            get { return this.ProspectNameValue; }

            set { SetProperty(ref ProspectNameValue, value); }

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
        
        // Status and classification
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? RiskLevelValue;

        public string? RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private string? ProspectTypeValue;

        public string? ProspectType

        {

            get { return this.ProspectTypeValue; }

            set { SetProperty(ref ProspectTypeValue, value); }

        }
        private string? PlayTypeValue;

        public string? PlayType

        {

            get { return this.PlayTypeValue; }

            set { SetProperty(ref PlayTypeValue, value); }

        }
        
        // Dates
        private DateTime? DiscoveryDateValue;

        public DateTime? DiscoveryDate

        {

            get { return this.DiscoveryDateValue; }

            set { SetProperty(ref DiscoveryDateValue, value); }

        }
        private DateTime? FirstDrillDateValue;

        public DateTime? FirstDrillDate

        {

            get { return this.FirstDrillDateValue; }

            set { SetProperty(ref FirstDrillDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        
        // Location information
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private decimal? ElevationValue;

        public decimal? Elevation

        {

            get { return this.ElevationValue; }

            set { SetProperty(ref ElevationValue, value); }

        }
        private string? ElevationOuomValue;

        public string? ElevationOuom

        {

            get { return this.ElevationOuomValue; }

            set { SetProperty(ref ElevationOuomValue, value); }

        }
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Depth information
        private decimal? TopDepthValue;

        public decimal? TopDepth

        {

            get { return this.TopDepthValue; }

            set { SetProperty(ref TopDepthValue, value); }

        }
        private decimal? BaseDepthValue;

        public decimal? BaseDepth

        {

            get { return this.BaseDepthValue; }

            set { SetProperty(ref BaseDepthValue, value); }

        }
        private string? DepthOuomValue;

        public string? DepthOuom

        {

            get { return this.DepthOuomValue; }

            set { SetProperty(ref DepthOuomValue, value); }

        }
        
        // Estimated resources
        private decimal? EstimatedOilVolumeValue;

        public decimal? EstimatedOilVolume

        {

            get { return this.EstimatedOilVolumeValue; }

            set { SetProperty(ref EstimatedOilVolumeValue, value); }

        }
        private decimal? EstimatedGasVolumeValue;

        public decimal? EstimatedGasVolume

        {

            get { return this.EstimatedGasVolumeValue; }

            set { SetProperty(ref EstimatedGasVolumeValue, value); }

        }
        private string? EstimatedVolumeOuomValue;

        public string? EstimatedVolumeOuom

        {

            get { return this.EstimatedVolumeOuomValue; }

            set { SetProperty(ref EstimatedVolumeOuomValue, value); }

        }
        private decimal? EstimatedValueValue;

        public decimal? EstimatedValue

        {

            get { return this.EstimatedValueValue; }

            set { SetProperty(ref EstimatedValueValue, value); }

        }
        private string? EstimatedValueCurrencyValue;

        public string? EstimatedValueCurrency

        {

            get { return this.EstimatedValueCurrencyValue; }

            set { SetProperty(ref EstimatedValueCurrencyValue, value); }

        }
        
        // Geological information
        private string? FormationNameValue;

        public string? FormationName

        {

            get { return this.FormationNameValue; }

            set { SetProperty(ref FormationNameValue, value); }

        }
        private string? StratUnitIdValue;

        public string? StratUnitId

        {

            get { return this.StratUnitIdValue; }

            set { SetProperty(ref StratUnitIdValue, value); }

        }
        private decimal? PorosityValue;

        public decimal? Porosity

        {

            get { return this.PorosityValue; }

            set { SetProperty(ref PorosityValue, value); }

        }
        private decimal? PermeabilityValue;

        public decimal? Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private decimal? NetPayValue;

        public decimal? NetPay

        {

            get { return this.NetPayValue; }

            set { SetProperty(ref NetPayValue, value); }

        }
        private string? NetPayOuomValue;

        public string? NetPayOuom

        {

            get { return this.NetPayOuomValue; }

            set { SetProperty(ref NetPayOuomValue, value); }

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
