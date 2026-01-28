using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FacilityRequest : ModelEntityBase
    {
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Auto-set by service
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Facility classification
        private string? FacilityTypeValue;

        public string? FacilityType

        {

            get { return this.FacilityTypeValue; }

            set { SetProperty(ref FacilityTypeValue, value); }

        } // e.g., "PRODUCTION", "PROCESSING", "STORAGE", "TRANSPORTATION"
        private string? FacilityCategoryValue;

        public string? FacilityCategory

        {

            get { return this.FacilityCategoryValue; }

            set { SetProperty(ref FacilityCategoryValue, value); }

        } // e.g., "PLATFORM", "FPSO", "PIPELINE_TERMINAL"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "ACTIVE", "INACTIVE", "DECOMMISSIONED"
        
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
        
        // Location
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
        
        // Capacity and specifications
        private decimal? ProcessingCapacityValue;

        public decimal? ProcessingCapacity

        {

            get { return this.ProcessingCapacityValue; }

            set { SetProperty(ref ProcessingCapacityValue, value); }

        } // Volume per day
        private string? ProcessingCapacityOuomValue;

        public string? ProcessingCapacityOuom

        {

            get { return this.ProcessingCapacityOuomValue; }

            set { SetProperty(ref ProcessingCapacityOuomValue, value); }

        } // e.g., "BBL/D", "MSCF/D"
        private decimal? StorageCapacityValue;

        public decimal? StorageCapacity

        {

            get { return this.StorageCapacityValue; }

            set { SetProperty(ref StorageCapacityValue, value); }

        }
        private string? StorageCapacityOuomValue;

        public string? StorageCapacityOuom

        {

            get { return this.StorageCapacityOuomValue; }

            set { SetProperty(ref StorageCapacityOuomValue, value); }

        }
        private string? DesignSpecificationsValue;

        public string? DesignSpecifications

        {

            get { return this.DesignSpecificationsValue; }

            set { SetProperty(ref DesignSpecificationsValue, value); }

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
        private decimal? OperatingCostValue;

        public decimal? OperatingCost

        {

            get { return this.OperatingCostValue; }

            set { SetProperty(ref OperatingCostValue, value); }

        } // Per period
        private string? OperatingCostCurrencyValue;

        public string? OperatingCostCurrency

        {

            get { return this.OperatingCostCurrencyValue; }

            set { SetProperty(ref OperatingCostCurrencyValue, value); }

        }
        
        // Operator information
        private string? OperatorIdValue;

        public string? OperatorId

        {

            get { return this.OperatorIdValue; }

            set { SetProperty(ref OperatorIdValue, value); }

        } // BUSINESS_ASSOCIATE_ID
        private string? OwnerIdValue;

        public string? OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        } // BUSINESS_ASSOCIATE_ID
        
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
