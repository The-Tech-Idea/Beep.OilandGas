using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Pool DTOs

    /// <summary>
    /// Request for creating or updating a pool (maps to POOL table)
    /// </summary>
    public class PoolRequest : ModelEntityBase
    {
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string PoolNameValue = string.Empty;

        public string PoolName

        {

            get { return this.PoolNameValue; }

            set { SetProperty(ref PoolNameValue, value); }

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
        
        // Pool classification
        private string? PoolTypeValue;

        public string? PoolType

        {

            get { return this.PoolTypeValue; }

            set { SetProperty(ref PoolTypeValue, value); }

        } // e.g., "OIL", "GAS", "OIL_GAS", "WATER"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "ACTIVE", "INACTIVE", "DEPLETED"
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
        
        // Reservoir properties
        private decimal? InitialReservoirPressureValue;

        public decimal? InitialReservoirPressure

        {

            get { return this.InitialReservoirPressureValue; }

            set { SetProperty(ref InitialReservoirPressureValue, value); }

        }
        private string? InitialReservoirPressureOuomValue;

        public string? InitialReservoirPressureOuom

        {

            get { return this.InitialReservoirPressureOuomValue; }

            set { SetProperty(ref InitialReservoirPressureOuomValue, value); }

        } // e.g., "PSI", "KPA"
        private decimal? ReservoirTemperatureValue;

        public decimal? ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private string? ReservoirTemperatureOuomValue;

        public string? ReservoirTemperatureOuom

        {

            get { return this.ReservoirTemperatureOuomValue; }

            set { SetProperty(ref ReservoirTemperatureOuomValue, value); }

        } // e.g., "F", "C"
        private decimal? AveragePorosityValue;

        public decimal? AveragePorosity

        {

            get { return this.AveragePorosityValue; }

            set { SetProperty(ref AveragePorosityValue, value); }

        } // Percentage or fraction
        private decimal? AveragePermeabilityValue;

        public decimal? AveragePermeability

        {

            get { return this.AveragePermeabilityValue; }

            set { SetProperty(ref AveragePermeabilityValue, value); }

        }
        private string? PermeabilityOuomValue;

        public string? PermeabilityOuom

        {

            get { return this.PermeabilityOuomValue; }

            set { SetProperty(ref PermeabilityOuomValue, value); }

        } // e.g., "MD", "DARCY"
        private decimal? AverageThicknessValue;

        public decimal? AverageThickness

        {

            get { return this.AverageThicknessValue; }

            set { SetProperty(ref AverageThicknessValue, value); }

        }
        private string? ThicknessOuomValue;

        public string? ThicknessOuom

        {

            get { return this.ThicknessOuomValue; }

            set { SetProperty(ref ThicknessOuomValue, value); }

        } // e.g., "FT", "M"
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
        
        // Fluid properties
        private decimal? OilGravityValue;

        public decimal? OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        } // API gravity
        private decimal? GasGravityValue;

        public decimal? GasGravity

        {

            get { return this.GasGravityValue; }

            set { SetProperty(ref GasGravityValue, value); }

        }
        private decimal? BubblePointPressureValue;

        public decimal? BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        private string? BubblePointPressureOuomValue;

        public string? BubblePointPressureOuom

        {

            get { return this.BubblePointPressureOuomValue; }

            set { SetProperty(ref BubblePointPressureOuomValue, value); }

        }
        private decimal? OilViscosityValue;

        public decimal? OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        }
        private string? OilViscosityOuomValue;

        public string? OilViscosityOuom

        {

            get { return this.OilViscosityOuomValue; }

            set { SetProperty(ref OilViscosityOuomValue, value); }

        } // e.g., "CP", "MPA_S"
        private decimal? GasViscosityValue;

        public decimal? GasViscosity

        {

            get { return this.GasViscosityValue; }

            set { SetProperty(ref GasViscosityValue, value); }

        }
        private string? GasViscosityOuomValue;

        public string? GasViscosityOuom

        {

            get { return this.GasViscosityOuomValue; }

            set { SetProperty(ref GasViscosityOuomValue, value); }

        }
        private decimal? FormationVolumeFactorValue;

        public decimal? FormationVolumeFactor

        {

            get { return this.FormationVolumeFactorValue; }

            set { SetProperty(ref FormationVolumeFactorValue, value); }

        }
        private decimal? TotalCompressibilityValue;

        public decimal? TotalCompressibility

        {

            get { return this.TotalCompressibilityValue; }

            set { SetProperty(ref TotalCompressibilityValue, value); }

        }
        private string? CompressibilityOuomValue;

        public string? CompressibilityOuom

        {

            get { return this.CompressibilityOuomValue; }

            set { SetProperty(ref CompressibilityOuomValue, value); }

        }
        
        // Reserves estimates
        private decimal? OriginalOilInPlaceValue;

        public decimal? OriginalOilInPlace

        {

            get { return this.OriginalOilInPlaceValue; }

            set { SetProperty(ref OriginalOilInPlaceValue, value); }

        } // OOIP
        private decimal? OriginalGasInPlaceValue;

        public decimal? OriginalGasInPlace

        {

            get { return this.OriginalGasInPlaceValue; }

            set { SetProperty(ref OriginalGasInPlaceValue, value); }

        } // OGIP
        private string? ReservesOuomValue;

        public string? ReservesOuom

        {

            get { return this.ReservesOuomValue; }

            set { SetProperty(ref ReservesOuomValue, value); }

        } // e.g., "BBL", "MSCF"
        private decimal? RecoveryFactorValue;

        public decimal? RecoveryFactor

        {

            get { return this.RecoveryFactorValue; }

            set { SetProperty(ref RecoveryFactorValue, value); }

        } // Percentage
        
        // Drainage information
        private decimal? DrainageAreaValue;

        public decimal? DrainageArea

        {

            get { return this.DrainageAreaValue; }

            set { SetProperty(ref DrainageAreaValue, value); }

        }
        private string? DrainageAreaOuomValue;

        public string? DrainageAreaOuom

        {

            get { return this.DrainageAreaOuomValue; }

            set { SetProperty(ref DrainageAreaOuomValue, value); }

        } // e.g., "ACRE", "KM2"
        private decimal? DrainageRadiusValue;

        public decimal? DrainageRadius

        {

            get { return this.DrainageRadiusValue; }

            set { SetProperty(ref DrainageRadiusValue, value); }

        }
        private string? DrainageRadiusOuomValue;

        public string? DrainageRadiusOuom

        {

            get { return this.DrainageRadiusOuomValue; }

            set { SetProperty(ref DrainageRadiusOuomValue, value); }

        } // e.g., "FT", "M"
        
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

    /// <summary>
    /// Response containing pool data (includes audit fields from POOL table)
    /// </summary>
    public class PoolResponse : ModelEntityBase
    {
        private string PoolIdValue = string.Empty;

        public string PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string PoolNameValue = string.Empty;

        public string PoolName

        {

            get { return this.PoolNameValue; }

            set { SetProperty(ref PoolNameValue, value); }

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
        
        // Pool classification
        private string? PoolTypeValue;

        public string? PoolType

        {

            get { return this.PoolTypeValue; }

            set { SetProperty(ref PoolTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
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
        
        // Reservoir properties
        private decimal? InitialReservoirPressureValue;

        public decimal? InitialReservoirPressure

        {

            get { return this.InitialReservoirPressureValue; }

            set { SetProperty(ref InitialReservoirPressureValue, value); }

        }
        private string? InitialReservoirPressureOuomValue;

        public string? InitialReservoirPressureOuom

        {

            get { return this.InitialReservoirPressureOuomValue; }

            set { SetProperty(ref InitialReservoirPressureOuomValue, value); }

        }
        private decimal? ReservoirTemperatureValue;

        public decimal? ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private string? ReservoirTemperatureOuomValue;

        public string? ReservoirTemperatureOuom

        {

            get { return this.ReservoirTemperatureOuomValue; }

            set { SetProperty(ref ReservoirTemperatureOuomValue, value); }

        }
        private decimal? AveragePorosityValue;

        public decimal? AveragePorosity

        {

            get { return this.AveragePorosityValue; }

            set { SetProperty(ref AveragePorosityValue, value); }

        }
        private decimal? AveragePermeabilityValue;

        public decimal? AveragePermeability

        {

            get { return this.AveragePermeabilityValue; }

            set { SetProperty(ref AveragePermeabilityValue, value); }

        }
        private string? PermeabilityOuomValue;

        public string? PermeabilityOuom

        {

            get { return this.PermeabilityOuomValue; }

            set { SetProperty(ref PermeabilityOuomValue, value); }

        }
        private decimal? AverageThicknessValue;

        public decimal? AverageThickness

        {

            get { return this.AverageThicknessValue; }

            set { SetProperty(ref AverageThicknessValue, value); }

        }
        private string? ThicknessOuomValue;

        public string? ThicknessOuom

        {

            get { return this.ThicknessOuomValue; }

            set { SetProperty(ref ThicknessOuomValue, value); }

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
        
        // Fluid properties
        private decimal? OilGravityValue;

        public decimal? OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        }
        private decimal? GasGravityValue;

        public decimal? GasGravity

        {

            get { return this.GasGravityValue; }

            set { SetProperty(ref GasGravityValue, value); }

        }
        private decimal? BubblePointPressureValue;

        public decimal? BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        private string? BubblePointPressureOuomValue;

        public string? BubblePointPressureOuom

        {

            get { return this.BubblePointPressureOuomValue; }

            set { SetProperty(ref BubblePointPressureOuomValue, value); }

        }
        private decimal? OilViscosityValue;

        public decimal? OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        }
        private string? OilViscosityOuomValue;

        public string? OilViscosityOuom

        {

            get { return this.OilViscosityOuomValue; }

            set { SetProperty(ref OilViscosityOuomValue, value); }

        }
        private decimal? GasViscosityValue;

        public decimal? GasViscosity

        {

            get { return this.GasViscosityValue; }

            set { SetProperty(ref GasViscosityValue, value); }

        }
        private string? GasViscosityOuomValue;

        public string? GasViscosityOuom

        {

            get { return this.GasViscosityOuomValue; }

            set { SetProperty(ref GasViscosityOuomValue, value); }

        }
        private decimal? FormationVolumeFactorValue;

        public decimal? FormationVolumeFactor

        {

            get { return this.FormationVolumeFactorValue; }

            set { SetProperty(ref FormationVolumeFactorValue, value); }

        }
        private decimal? TotalCompressibilityValue;

        public decimal? TotalCompressibility

        {

            get { return this.TotalCompressibilityValue; }

            set { SetProperty(ref TotalCompressibilityValue, value); }

        }
        private string? CompressibilityOuomValue;

        public string? CompressibilityOuom

        {

            get { return this.CompressibilityOuomValue; }

            set { SetProperty(ref CompressibilityOuomValue, value); }

        }
        
        // Reserves estimates
        private decimal? OriginalOilInPlaceValue;

        public decimal? OriginalOilInPlace

        {

            get { return this.OriginalOilInPlaceValue; }

            set { SetProperty(ref OriginalOilInPlaceValue, value); }

        }
        private decimal? OriginalGasInPlaceValue;

        public decimal? OriginalGasInPlace

        {

            get { return this.OriginalGasInPlaceValue; }

            set { SetProperty(ref OriginalGasInPlaceValue, value); }

        }
        private string? ReservesOuomValue;

        public string? ReservesOuom

        {

            get { return this.ReservesOuomValue; }

            set { SetProperty(ref ReservesOuomValue, value); }

        }
        private decimal? RecoveryFactorValue;

        public decimal? RecoveryFactor

        {

            get { return this.RecoveryFactorValue; }

            set { SetProperty(ref RecoveryFactorValue, value); }

        }
        
        // Drainage information
        private decimal? DrainageAreaValue;

        public decimal? DrainageArea

        {

            get { return this.DrainageAreaValue; }

            set { SetProperty(ref DrainageAreaValue, value); }

        }
        private string? DrainageAreaOuomValue;

        public string? DrainageAreaOuom

        {

            get { return this.DrainageAreaOuomValue; }

            set { SetProperty(ref DrainageAreaOuomValue, value); }

        }
        private decimal? DrainageRadiusValue;

        public decimal? DrainageRadius

        {

            get { return this.DrainageRadiusValue; }

            set { SetProperty(ref DrainageRadiusValue, value); }

        }
        private string? DrainageRadiusOuomValue;

        public string? DrainageRadiusOuom

        {

            get { return this.DrainageRadiusOuomValue; }

            set { SetProperty(ref DrainageRadiusOuomValue, value); }

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

    #endregion

    #region Facility DTOs

    /// <summary>
    /// Request for creating or updating a facility (maps to FACILITY table)
    /// </summary>
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

   

    #endregion

    #region Pipeline DTOs

    /// <summary>
    /// Request for creating or updating a pipeline (maps to PIPELINE table)
    /// </summary>
    public class PipelineRequest : ModelEntityBase
    {
        private string? PipelineIdValue;

        public string? PipelineId

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

        } // Auto-set by service
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

        } // e.g., "GATHERING", "TRANSMISSION", "DISTRIBUTION", "EXPORT"
        private string? FluidTypeValue;

        public string? FluidType

        {

            get { return this.FluidTypeValue; }

            set { SetProperty(ref FluidTypeValue, value); }

        } // e.g., "OIL", "GAS", "MULTIPHASE", "WATER"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "ACTIVE", "INACTIVE", "ABANDONED"
        
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

        } // e.g., "KM", "MI"
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

        } // e.g., "IN", "MM"
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

        } // e.g., "PSI", "BAR"
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

        } // e.g., "BBL/D", "MSCF/D"
        private string? MaterialValue;

        public string? Material

        {

            get { return this.MaterialValue; }

            set { SetProperty(ref MaterialValue, value); }

        } // e.g., "STEEL", "COMPOSITE"
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
    }

    /// <summary>
    /// Response containing pipeline data (includes audit fields from PIPELINE table)
    /// </summary>
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

    #endregion

    #region Development Well DTOs

    /// <summary>
    /// Request for creating or updating a development well (maps to WELL table with WELL_TYPE='DEVELOPMENT')
    /// </summary>
    public class DevelopmentWellRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Auto-set by service
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Well classification
        private string WellTypeValue = "DEVELOPMENT";

        public string WellType

        {

            get { return this.WellTypeValue; }

            set { SetProperty(ref WellTypeValue, value); }

        } // Should be "DEVELOPMENT"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "DRILLING", "COMPLETED", "PRODUCING", "SHUT_IN"
        private string? WellClassificationValue;

        public string? WellClassification

        {

            get { return this.WellClassificationValue; }

            set { SetProperty(ref WellClassificationValue, value); }

        } // e.g., "PRODUCER", "INJECTOR", "OBSERVATION"
        private string? CompletionTypeValue;

        public string? CompletionType

        {

            get { return this.CompletionTypeValue; }

            set { SetProperty(ref CompletionTypeValue, value); }

        } // e.g., "OPEN_HOLE", "CASED_HOLE", "FRAC"
        
        // Dates
        private DateTime? SpudDateValue;

        public DateTime? SpudDate

        {

            get { return this.SpudDateValue; }

            set { SetProperty(ref SpudDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private DateTime? FirstProductionDateValue;

        public DateTime? FirstProductionDate

        {

            get { return this.FirstProductionDateValue; }

            set { SetProperty(ref FirstProductionDateValue, value); }

        }
        private DateTime? RigReleaseDateValue;

        public DateTime? RigReleaseDate

        {

            get { return this.RigReleaseDateValue; }

            set { SetProperty(ref RigReleaseDateValue, value); }

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
        private decimal? GroundElevationValue;

        public decimal? GroundElevation

        {

            get { return this.GroundElevationValue; }

            set { SetProperty(ref GroundElevationValue, value); }

        }
        private string? GroundElevationOuomValue;

        public string? GroundElevationOuom

        {

            get { return this.GroundElevationOuomValue; }

            set { SetProperty(ref GroundElevationOuomValue, value); }

        }
        private decimal? KBElevationValue;

        public decimal? KBElevation

        {

            get { return this.KBElevationValue; }

            set { SetProperty(ref KBElevationValue, value); }

        } // Kelly Bushing Elevation
        private string? KBElevationOuomValue;

        public string? KBElevationOuom

        {

            get { return this.KBElevationOuomValue; }

            set { SetProperty(ref KBElevationOuomValue, value); }

        }
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Depth information
        private decimal? TotalDepthValue;

        public decimal? TotalDepth

        {

            get { return this.TotalDepthValue; }

            set { SetProperty(ref TotalDepthValue, value); }

        }
        private string? TotalDepthOuomValue;

        public string? TotalDepthOuom

        {

            get { return this.TotalDepthOuomValue; }

            set { SetProperty(ref TotalDepthOuomValue, value); }

        } // e.g., "FT", "M"
        private decimal? MeasuredDepthValue;

        public decimal? MeasuredDepth

        {

            get { return this.MeasuredDepthValue; }

            set { SetProperty(ref MeasuredDepthValue, value); }

        }
        private string? MeasuredDepthOuomValue;

        public string? MeasuredDepthOuom

        {

            get { return this.MeasuredDepthOuomValue; }

            set { SetProperty(ref MeasuredDepthOuomValue, value); }

        }
        private decimal? TrueVerticalDepthValue;

        public decimal? TrueVerticalDepth

        {

            get { return this.TrueVerticalDepthValue; }

            set { SetProperty(ref TrueVerticalDepthValue, value); }

        }
        private string? TrueVerticalDepthOuomValue;

        public string? TrueVerticalDepthOuom

        {

            get { return this.TrueVerticalDepthOuomValue; }

            set { SetProperty(ref TrueVerticalDepthOuomValue, value); }

        }
        
        // Completion information
        private decimal? CompletionTopDepthValue;

        public decimal? CompletionTopDepth

        {

            get { return this.CompletionTopDepthValue; }

            set { SetProperty(ref CompletionTopDepthValue, value); }

        }
        private string? CompletionTopDepthOuomValue;

        public string? CompletionTopDepthOuom

        {

            get { return this.CompletionTopDepthOuomValue; }

            set { SetProperty(ref CompletionTopDepthOuomValue, value); }

        }
        private decimal? CompletionBaseDepthValue;

        public decimal? CompletionBaseDepth

        {

            get { return this.CompletionBaseDepthValue; }

            set { SetProperty(ref CompletionBaseDepthValue, value); }

        }
        private string? CompletionBaseDepthOuomValue;

        public string? CompletionBaseDepthOuom

        {

            get { return this.CompletionBaseDepthOuomValue; }

            set { SetProperty(ref CompletionBaseDepthOuomValue, value); }

        }
        private decimal? PerforationTopDepthValue;

        public decimal? PerforationTopDepth

        {

            get { return this.PerforationTopDepthValue; }

            set { SetProperty(ref PerforationTopDepthValue, value); }

        }
        private string? PerforationTopDepthOuomValue;

        public string? PerforationTopDepthOuom

        {

            get { return this.PerforationTopDepthOuomValue; }

            set { SetProperty(ref PerforationTopDepthOuomValue, value); }

        }
        private decimal? PerforationBaseDepthValue;

        public decimal? PerforationBaseDepth

        {

            get { return this.PerforationBaseDepthValue; }

            set { SetProperty(ref PerforationBaseDepthValue, value); }

        }
        private string? PerforationBaseDepthOuomValue;

        public string? PerforationBaseDepthOuom

        {

            get { return this.PerforationBaseDepthOuomValue; }

            set { SetProperty(ref PerforationBaseDepthOuomValue, value); }

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
    }

    /// <summary>
    /// Response containing development well data (includes audit fields from WELL table)
    /// </summary>
    public class DevelopmentWellResponse : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Well classification
        private string? WellTypeValue;

        public string? WellType

        {

            get { return this.WellTypeValue; }

            set { SetProperty(ref WellTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? WellClassificationValue;

        public string? WellClassification

        {

            get { return this.WellClassificationValue; }

            set { SetProperty(ref WellClassificationValue, value); }

        }
        private string? CompletionTypeValue;

        public string? CompletionType

        {

            get { return this.CompletionTypeValue; }

            set { SetProperty(ref CompletionTypeValue, value); }

        }
        
        // Dates
        private DateTime? SpudDateValue;

        public DateTime? SpudDate

        {

            get { return this.SpudDateValue; }

            set { SetProperty(ref SpudDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private DateTime? FirstProductionDateValue;

        public DateTime? FirstProductionDate

        {

            get { return this.FirstProductionDateValue; }

            set { SetProperty(ref FirstProductionDateValue, value); }

        }
        private DateTime? RigReleaseDateValue;

        public DateTime? RigReleaseDate

        {

            get { return this.RigReleaseDateValue; }

            set { SetProperty(ref RigReleaseDateValue, value); }

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
        private decimal? GroundElevationValue;

        public decimal? GroundElevation

        {

            get { return this.GroundElevationValue; }

            set { SetProperty(ref GroundElevationValue, value); }

        }
        private string? GroundElevationOuomValue;

        public string? GroundElevationOuom

        {

            get { return this.GroundElevationOuomValue; }

            set { SetProperty(ref GroundElevationOuomValue, value); }

        }
        private decimal? KBElevationValue;

        public decimal? KBElevation

        {

            get { return this.KBElevationValue; }

            set { SetProperty(ref KBElevationValue, value); }

        }
        private string? KBElevationOuomValue;

        public string? KBElevationOuom

        {

            get { return this.KBElevationOuomValue; }

            set { SetProperty(ref KBElevationOuomValue, value); }

        }
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Depth information
        private decimal? TotalDepthValue;

        public decimal? TotalDepth

        {

            get { return this.TotalDepthValue; }

            set { SetProperty(ref TotalDepthValue, value); }

        }
        private string? TotalDepthOuomValue;

        public string? TotalDepthOuom

        {

            get { return this.TotalDepthOuomValue; }

            set { SetProperty(ref TotalDepthOuomValue, value); }

        }
        private decimal? MeasuredDepthValue;

        public decimal? MeasuredDepth

        {

            get { return this.MeasuredDepthValue; }

            set { SetProperty(ref MeasuredDepthValue, value); }

        }
        private string? MeasuredDepthOuomValue;

        public string? MeasuredDepthOuom

        {

            get { return this.MeasuredDepthOuomValue; }

            set { SetProperty(ref MeasuredDepthOuomValue, value); }

        }
        private decimal? TrueVerticalDepthValue;

        public decimal? TrueVerticalDepth

        {

            get { return this.TrueVerticalDepthValue; }

            set { SetProperty(ref TrueVerticalDepthValue, value); }

        }
        private string? TrueVerticalDepthOuomValue;

        public string? TrueVerticalDepthOuom

        {

            get { return this.TrueVerticalDepthOuomValue; }

            set { SetProperty(ref TrueVerticalDepthOuomValue, value); }

        }
        
        // Completion information
        private decimal? CompletionTopDepthValue;

        public decimal? CompletionTopDepth

        {

            get { return this.CompletionTopDepthValue; }

            set { SetProperty(ref CompletionTopDepthValue, value); }

        }
        private string? CompletionTopDepthOuomValue;

        public string? CompletionTopDepthOuom

        {

            get { return this.CompletionTopDepthOuomValue; }

            set { SetProperty(ref CompletionTopDepthOuomValue, value); }

        }
        private decimal? CompletionBaseDepthValue;

        public decimal? CompletionBaseDepth

        {

            get { return this.CompletionBaseDepthValue; }

            set { SetProperty(ref CompletionBaseDepthValue, value); }

        }
        private string? CompletionBaseDepthOuomValue;

        public string? CompletionBaseDepthOuom

        {

            get { return this.CompletionBaseDepthOuomValue; }

            set { SetProperty(ref CompletionBaseDepthOuomValue, value); }

        }
        private decimal? PerforationTopDepthValue;

        public decimal? PerforationTopDepth

        {

            get { return this.PerforationTopDepthValue; }

            set { SetProperty(ref PerforationTopDepthValue, value); }

        }
        private string? PerforationTopDepthOuomValue;

        public string? PerforationTopDepthOuom

        {

            get { return this.PerforationTopDepthOuomValue; }

            set { SetProperty(ref PerforationTopDepthOuomValue, value); }

        }
        private decimal? PerforationBaseDepthValue;

        public decimal? PerforationBaseDepth

        {

            get { return this.PerforationBaseDepthValue; }

            set { SetProperty(ref PerforationBaseDepthValue, value); }

        }
        private string? PerforationBaseDepthOuomValue;

        public string? PerforationBaseDepthOuom

        {

            get { return this.PerforationBaseDepthOuomValue; }

            set { SetProperty(ref PerforationBaseDepthOuomValue, value); }

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

    #endregion

    #region Wellbore DTOs

    /// <summary>
    /// Request for creating or updating a wellbore (maps to WELLBORE table)
    /// </summary>
    public class WellboreRequest : ModelEntityBase
    {
        private string? WellboreIdValue;

        public string? WellboreId

        {

            get { return this.WellboreIdValue; }

            set { SetProperty(ref WellboreIdValue, value); }

        }
        private string WellboreNameValue = string.Empty;

        public string WellboreName

        {

            get { return this.WellboreNameValue; }

            set { SetProperty(ref WellboreNameValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Wellbore classification
        private string? WellboreTypeValue;

        public string? WellboreType

        {

            get { return this.WellboreTypeValue; }

            set { SetProperty(ref WellboreTypeValue, value); }

        } // e.g., "PRIMARY", "LATERAL", "SIDETRACK"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "ACTIVE", "PLUGGED", "ABANDONED"
        private string? TrajectoryTypeValue;

        public string? TrajectoryType

        {

            get { return this.TrajectoryTypeValue; }

            set { SetProperty(ref TrajectoryTypeValue, value); }

        } // e.g., "VERTICAL", "DIRECTIONAL", "HORIZONTAL", "DEVIATED"
        
        // Depth information
        private decimal? MeasuredDepthValue;

        public decimal? MeasuredDepth

        {

            get { return this.MeasuredDepthValue; }

            set { SetProperty(ref MeasuredDepthValue, value); }

        }
        private string? MeasuredDepthOuomValue;

        public string? MeasuredDepthOuom

        {

            get { return this.MeasuredDepthOuomValue; }

            set { SetProperty(ref MeasuredDepthOuomValue, value); }

        } // e.g., "FT", "M"
        private decimal? TrueVerticalDepthValue;

        public decimal? TrueVerticalDepth

        {

            get { return this.TrueVerticalDepthValue; }

            set { SetProperty(ref TrueVerticalDepthValue, value); }

        }
        private string? TrueVerticalDepthOuomValue;

        public string? TrueVerticalDepthOuom

        {

            get { return this.TrueVerticalDepthOuomValue; }

            set { SetProperty(ref TrueVerticalDepthOuomValue, value); }

        }
        private decimal? KickoffDepthValue;

        public decimal? KickoffDepth

        {

            get { return this.KickoffDepthValue; }

            set { SetProperty(ref KickoffDepthValue, value); }

        }
        private string? KickoffDepthOuomValue;

        public string? KickoffDepthOuom

        {

            get { return this.KickoffDepthOuomValue; }

            set { SetProperty(ref KickoffDepthOuomValue, value); }

        }
        
        // Geometry
        private decimal? HoleDiameterValue;

        public decimal? HoleDiameter

        {

            get { return this.HoleDiameterValue; }

            set { SetProperty(ref HoleDiameterValue, value); }

        }
        private string? HoleDiameterOuomValue;

        public string? HoleDiameterOuom

        {

            get { return this.HoleDiameterOuomValue; }

            set { SetProperty(ref HoleDiameterOuomValue, value); }

        } // e.g., "IN", "MM"
        private decimal? CasingDiameterValue;

        public decimal? CasingDiameter

        {

            get { return this.CasingDiameterValue; }

            set { SetProperty(ref CasingDiameterValue, value); }

        }
        private string? CasingDiameterOuomValue;

        public string? CasingDiameterOuom

        {

            get { return this.CasingDiameterOuomValue; }

            set { SetProperty(ref CasingDiameterOuomValue, value); }

        }
        private decimal? TubingDiameterValue;

        public decimal? TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        }
        private string? TubingDiameterOuomValue;

        public string? TubingDiameterOuom

        {

            get { return this.TubingDiameterOuomValue; }

            set { SetProperty(ref TubingDiameterOuomValue, value); }

        }
        
        // Completion information
        private decimal? CompletionTopDepthValue;

        public decimal? CompletionTopDepth

        {

            get { return this.CompletionTopDepthValue; }

            set { SetProperty(ref CompletionTopDepthValue, value); }

        }
        private string? CompletionTopDepthOuomValue;

        public string? CompletionTopDepthOuom

        {

            get { return this.CompletionTopDepthOuomValue; }

            set { SetProperty(ref CompletionTopDepthOuomValue, value); }

        }
        private decimal? CompletionBaseDepthValue;

        public decimal? CompletionBaseDepth

        {

            get { return this.CompletionBaseDepthValue; }

            set { SetProperty(ref CompletionBaseDepthValue, value); }

        }
        private string? CompletionBaseDepthOuomValue;

        public string? CompletionBaseDepthOuom

        {

            get { return this.CompletionBaseDepthOuomValue; }

            set { SetProperty(ref CompletionBaseDepthOuomValue, value); }

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
        
        // Dates
        private DateTime? DrillingStartDateValue;

        public DateTime? DrillingStartDate

        {

            get { return this.DrillingStartDateValue; }

            set { SetProperty(ref DrillingStartDateValue, value); }

        }
        private DateTime? DrillingEndDateValue;

        public DateTime? DrillingEndDate

        {

            get { return this.DrillingEndDateValue; }

            set { SetProperty(ref DrillingEndDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

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
    }

    /// <summary>
    /// Response containing wellbore data (includes audit fields from WELLBORE table)
    /// </summary>
    public class WellboreResponse : ModelEntityBase
    {
        private string WellboreIdValue = string.Empty;

        public string WellboreId

        {

            get { return this.WellboreIdValue; }

            set { SetProperty(ref WellboreIdValue, value); }

        }
        private string WellboreNameValue = string.Empty;

        public string WellboreName

        {

            get { return this.WellboreNameValue; }

            set { SetProperty(ref WellboreNameValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Wellbore classification
        private string? WellboreTypeValue;

        public string? WellboreType

        {

            get { return this.WellboreTypeValue; }

            set { SetProperty(ref WellboreTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? TrajectoryTypeValue;

        public string? TrajectoryType

        {

            get { return this.TrajectoryTypeValue; }

            set { SetProperty(ref TrajectoryTypeValue, value); }

        }
        
        // Depth information
        private decimal? MeasuredDepthValue;

        public decimal? MeasuredDepth

        {

            get { return this.MeasuredDepthValue; }

            set { SetProperty(ref MeasuredDepthValue, value); }

        }
        private string? MeasuredDepthOuomValue;

        public string? MeasuredDepthOuom

        {

            get { return this.MeasuredDepthOuomValue; }

            set { SetProperty(ref MeasuredDepthOuomValue, value); }

        }
        private decimal? TrueVerticalDepthValue;

        public decimal? TrueVerticalDepth

        {

            get { return this.TrueVerticalDepthValue; }

            set { SetProperty(ref TrueVerticalDepthValue, value); }

        }
        private string? TrueVerticalDepthOuomValue;

        public string? TrueVerticalDepthOuom

        {

            get { return this.TrueVerticalDepthOuomValue; }

            set { SetProperty(ref TrueVerticalDepthOuomValue, value); }

        }
        private decimal? KickoffDepthValue;

        public decimal? KickoffDepth

        {

            get { return this.KickoffDepthValue; }

            set { SetProperty(ref KickoffDepthValue, value); }

        }
        private string? KickoffDepthOuomValue;

        public string? KickoffDepthOuom

        {

            get { return this.KickoffDepthOuomValue; }

            set { SetProperty(ref KickoffDepthOuomValue, value); }

        }
        
        // Geometry
        private decimal? HoleDiameterValue;

        public decimal? HoleDiameter

        {

            get { return this.HoleDiameterValue; }

            set { SetProperty(ref HoleDiameterValue, value); }

        }
        private string? HoleDiameterOuomValue;

        public string? HoleDiameterOuom

        {

            get { return this.HoleDiameterOuomValue; }

            set { SetProperty(ref HoleDiameterOuomValue, value); }

        }
        private decimal? CasingDiameterValue;

        public decimal? CasingDiameter

        {

            get { return this.CasingDiameterValue; }

            set { SetProperty(ref CasingDiameterValue, value); }

        }
        private string? CasingDiameterOuomValue;

        public string? CasingDiameterOuom

        {

            get { return this.CasingDiameterOuomValue; }

            set { SetProperty(ref CasingDiameterOuomValue, value); }

        }
        private decimal? TubingDiameterValue;

        public decimal? TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        }
        private string? TubingDiameterOuomValue;

        public string? TubingDiameterOuom

        {

            get { return this.TubingDiameterOuomValue; }

            set { SetProperty(ref TubingDiameterOuomValue, value); }

        }
        
        // Completion information
        private decimal? CompletionTopDepthValue;

        public decimal? CompletionTopDepth

        {

            get { return this.CompletionTopDepthValue; }

            set { SetProperty(ref CompletionTopDepthValue, value); }

        }
        private string? CompletionTopDepthOuomValue;

        public string? CompletionTopDepthOuom

        {

            get { return this.CompletionTopDepthOuomValue; }

            set { SetProperty(ref CompletionTopDepthOuomValue, value); }

        }
        private decimal? CompletionBaseDepthValue;

        public decimal? CompletionBaseDepth

        {

            get { return this.CompletionBaseDepthValue; }

            set { SetProperty(ref CompletionBaseDepthValue, value); }

        }
        private string? CompletionBaseDepthOuomValue;

        public string? CompletionBaseDepthOuom

        {

            get { return this.CompletionBaseDepthOuomValue; }

            set { SetProperty(ref CompletionBaseDepthOuomValue, value); }

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
        
        // Dates
        private DateTime? DrillingStartDateValue;

        public DateTime? DrillingStartDate

        {

            get { return this.DrillingStartDateValue; }

            set { SetProperty(ref DrillingStartDateValue, value); }

        }
        private DateTime? DrillingEndDateValue;

        public DateTime? DrillingEndDate

        {

            get { return this.DrillingEndDateValue; }

            set { SetProperty(ref DrillingEndDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

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

    #endregion

    #region Feasibility Study DTOs

    /// <summary>
    /// Request for feasibility study calculation
    /// </summary>
    public class FeasibilityStudyRequest : ModelEntityBase
    {
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? ProjectIdValue;

        public string? ProjectId

        {

            get { return this.ProjectIdValue; }

            set { SetProperty(ref ProjectIdValue, value); }

        }
        private string? StudyTypeValue;

        public string? StudyType

        {

            get { return this.StudyTypeValue; }

            set { SetProperty(ref StudyTypeValue, value); }

        } // e.g., "ECONOMIC", "TECHNICAL", "REGULATORY", "COMPREHENSIVE"
        
        // Project scope
        private int? NumberOfWellsValue;

        public int? NumberOfWells

        {

            get { return this.NumberOfWellsValue; }

            set { SetProperty(ref NumberOfWellsValue, value); }

        }
        private int? NumberOfFacilitiesValue;

        public int? NumberOfFacilities

        {

            get { return this.NumberOfFacilitiesValue; }

            set { SetProperty(ref NumberOfFacilitiesValue, value); }

        }
        private decimal? DevelopmentAreaValue;

        public decimal? DevelopmentArea

        {

            get { return this.DevelopmentAreaValue; }

            set { SetProperty(ref DevelopmentAreaValue, value); }

        }
        private string? DevelopmentAreaOuomValue;

        public string? DevelopmentAreaOuom

        {

            get { return this.DevelopmentAreaOuomValue; }

            set { SetProperty(ref DevelopmentAreaOuomValue, value); }

        }
        
        // Capital costs
        private decimal? DrillingCostPerWellValue;

        public decimal? DrillingCostPerWell

        {

            get { return this.DrillingCostPerWellValue; }

            set { SetProperty(ref DrillingCostPerWellValue, value); }

        }
        private string? DrillingCostCurrencyValue;

        public string? DrillingCostCurrency

        {

            get { return this.DrillingCostCurrencyValue; }

            set { SetProperty(ref DrillingCostCurrencyValue, value); }

        }
        private decimal? CompletionCostPerWellValue;

        public decimal? CompletionCostPerWell

        {

            get { return this.CompletionCostPerWellValue; }

            set { SetProperty(ref CompletionCostPerWellValue, value); }

        }
        private string? CompletionCostCurrencyValue;

        public string? CompletionCostCurrency

        {

            get { return this.CompletionCostCurrencyValue; }

            set { SetProperty(ref CompletionCostCurrencyValue, value); }

        }
        private decimal? FacilityCostValue;

        public decimal? FacilityCost

        {

            get { return this.FacilityCostValue; }

            set { SetProperty(ref FacilityCostValue, value); }

        }
        private string? FacilityCostCurrencyValue;

        public string? FacilityCostCurrency

        {

            get { return this.FacilityCostCurrencyValue; }

            set { SetProperty(ref FacilityCostCurrencyValue, value); }

        }
        private decimal? InfrastructureCostValue;

        public decimal? InfrastructureCost

        {

            get { return this.InfrastructureCostValue; }

            set { SetProperty(ref InfrastructureCostValue, value); }

        } // Pipelines, roads, etc.
        private string? InfrastructureCostCurrencyValue;

        public string? InfrastructureCostCurrency

        {

            get { return this.InfrastructureCostCurrencyValue; }

            set { SetProperty(ref InfrastructureCostCurrencyValue, value); }

        }
        private decimal? TotalCapitalCostValue;

        public decimal? TotalCapitalCost

        {

            get { return this.TotalCapitalCostValue; }

            set { SetProperty(ref TotalCapitalCostValue, value); }

        }
        private string? TotalCapitalCostCurrencyValue;

        public string? TotalCapitalCostCurrency

        {

            get { return this.TotalCapitalCostCurrencyValue; }

            set { SetProperty(ref TotalCapitalCostCurrencyValue, value); }

        }
        
        // Operating costs
        private decimal? OperatingCostPerUnitValue;

        public decimal? OperatingCostPerUnit

        {

            get { return this.OperatingCostPerUnitValue; }

            set { SetProperty(ref OperatingCostPerUnitValue, value); }

        } // Per volume unit
        private string? OperatingCostCurrencyValue;

        public string? OperatingCostCurrency

        {

            get { return this.OperatingCostCurrencyValue; }

            set { SetProperty(ref OperatingCostCurrencyValue, value); }

        }
        private decimal? AnnualOperatingCostValue;

        public decimal? AnnualOperatingCost

        {

            get { return this.AnnualOperatingCostValue; }

            set { SetProperty(ref AnnualOperatingCostValue, value); }

        }
        private string? AnnualOperatingCostCurrencyValue;

        public string? AnnualOperatingCostCurrency

        {

            get { return this.AnnualOperatingCostCurrencyValue; }

            set { SetProperty(ref AnnualOperatingCostCurrencyValue, value); }

        }
        
        // Production forecast
        private List<FeasibilityProductionPoint>? ProductionForecastValue;

        public List<FeasibilityProductionPoint>? ProductionForecast

        {

            get { return this.ProductionForecastValue; }

            set { SetProperty(ref ProductionForecastValue, value); }

        }
        private int? ProductionLifetimeYearsValue;

        public int? ProductionLifetimeYears

        {

            get { return this.ProductionLifetimeYearsValue; }

            set { SetProperty(ref ProductionLifetimeYearsValue, value); }

        }
        
        // Economic parameters
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private string? OilPriceCurrencyValue;

        public string? OilPriceCurrency

        {

            get { return this.OilPriceCurrencyValue; }

            set { SetProperty(ref OilPriceCurrencyValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private string? GasPriceCurrencyValue;

        public string? GasPriceCurrency

        {

            get { return this.GasPriceCurrencyValue; }

            set { SetProperty(ref GasPriceCurrencyValue, value); }

        }
        private decimal? DiscountRateValue;

        public decimal? DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        } // Percentage
        private decimal? InflationRateValue;

        public decimal? InflationRate

        {

            get { return this.InflationRateValue; }

            set { SetProperty(ref InflationRateValue, value); }

        } // Percentage
        
        // Fiscal terms
        private decimal? RoyaltyRateValue;

        public decimal? RoyaltyRate

        {

            get { return this.RoyaltyRateValue; }

            set { SetProperty(ref RoyaltyRateValue, value); }

        } // Percentage
        private decimal? TaxRateValue;

        public decimal? TaxRate

        {

            get { return this.TaxRateValue; }

            set { SetProperty(ref TaxRateValue, value); }

        } // Percentage
        private decimal? WorkingInterestValue;

        public decimal? WorkingInterest

        {

            get { return this.WorkingInterestValue; }

            set { SetProperty(ref WorkingInterestValue, value); }

        } // Percentage
        
        // Technical constraints
        private decimal? MaximumProductionRateValue;

        public decimal? MaximumProductionRate

        {

            get { return this.MaximumProductionRateValue; }

            set { SetProperty(ref MaximumProductionRateValue, value); }

        }
        private string? MaximumProductionRateOuomValue;

        public string? MaximumProductionRateOuom

        {

            get { return this.MaximumProductionRateOuomValue; }

            set { SetProperty(ref MaximumProductionRateOuomValue, value); }

        }
        private List<string>? RegulatoryRequirementsValue;

        public List<string>? RegulatoryRequirements

        {

            get { return this.RegulatoryRequirementsValue; }

            set { SetProperty(ref RegulatoryRequirementsValue, value); }

        }
        private List<string>? TechnicalConstraintsValue;

        public List<string>? TechnicalConstraints

        {

            get { return this.TechnicalConstraintsValue; }

            set { SetProperty(ref TechnicalConstraintsValue, value); }

        }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Production point for feasibility study
    /// </summary>
    public class FeasibilityProductionPoint : ModelEntityBase
    {
        private int YearValue;

        public int Year

        {

            get { return this.YearValue; }

            set { SetProperty(ref YearValue, value); }

        }
        private decimal? OilVolumeValue;

        public decimal? OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }
        private decimal? GasVolumeValue;

        public decimal? GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }
        private decimal? WaterVolumeValue;

        public decimal? WaterVolume

        {

            get { return this.WaterVolumeValue; }

            set { SetProperty(ref WaterVolumeValue, value); }

        }
        private string? VolumeOuomValue;

        public string? VolumeOuom

        {

            get { return this.VolumeOuomValue; }

            set { SetProperty(ref VolumeOuomValue, value); }

        }
        private decimal? OperatingCostValue;

        public decimal? OperatingCost

        {

            get { return this.OperatingCostValue; }

            set { SetProperty(ref OperatingCostValue, value); }

        }
    }

    /// <summary>
    /// Result of feasibility study calculation
    /// </summary>
    public class FeasibilityStudyResponse : ModelEntityBase
    {
        private string StudyIdValue = string.Empty;

        public string StudyId

        {

            get { return this.StudyIdValue; }

            set { SetProperty(ref StudyIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? ProjectIdValue;

        public string? ProjectId

        {

            get { return this.ProjectIdValue; }

            set { SetProperty(ref ProjectIdValue, value); }

        }
        private string? StudyTypeValue;

        public string? StudyType

        {

            get { return this.StudyTypeValue; }

            set { SetProperty(ref StudyTypeValue, value); }

        }
        private DateTime StudyDateValue;

        public DateTime StudyDate

        {

            get { return this.StudyDateValue; }

            set { SetProperty(ref StudyDateValue, value); }

        }
        
        // Economic feasibility
        private decimal? NetPresentValueValue;

        public decimal? NetPresentValue

        {

            get { return this.NetPresentValueValue; }

            set { SetProperty(ref NetPresentValueValue, value); }

        } // NPV
        private decimal? InternalRateOfReturnValue;

        public decimal? InternalRateOfReturn

        {

            get { return this.InternalRateOfReturnValue; }

            set { SetProperty(ref InternalRateOfReturnValue, value); }

        } // IRR (percentage)
        private decimal? PaybackPeriodValue;

        public decimal? PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        } // Years
        private decimal? ReturnOnInvestmentValue;

        public decimal? ReturnOnInvestment

        {

            get { return this.ReturnOnInvestmentValue; }

            set { SetProperty(ref ReturnOnInvestmentValue, value); }

        } // ROI (percentage)
        private decimal? ProfitabilityIndexValue;

        public decimal? ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }
        private bool? IsEconomicallyFeasibleValue;

        public bool? IsEconomicallyFeasible

        {

            get { return this.IsEconomicallyFeasibleValue; }

            set { SetProperty(ref IsEconomicallyFeasibleValue, value); }

        }
        
        // Technical feasibility
        private bool? IsTechnicallyFeasibleValue;

        public bool? IsTechnicallyFeasible

        {

            get { return this.IsTechnicallyFeasibleValue; }

            set { SetProperty(ref IsTechnicallyFeasibleValue, value); }

        }
        private List<string> TechnicalChallengesValue = new List<string>();

        public List<string> TechnicalChallenges

        {

            get { return this.TechnicalChallengesValue; }

            set { SetProperty(ref TechnicalChallengesValue, value); }

        }
        private List<string> TechnicalRecommendationsValue = new List<string>();

        public List<string> TechnicalRecommendations

        {

            get { return this.TechnicalRecommendationsValue; }

            set { SetProperty(ref TechnicalRecommendationsValue, value); }

        }
        
        // Regulatory feasibility
        private bool? IsRegulatorilyFeasibleValue;

        public bool? IsRegulatorilyFeasible

        {

            get { return this.IsRegulatorilyFeasibleValue; }

            set { SetProperty(ref IsRegulatorilyFeasibleValue, value); }

        }
        private List<string> RegulatoryRequirementsValue = new List<string>();

        public List<string> RegulatoryRequirements

        {

            get { return this.RegulatoryRequirementsValue; }

            set { SetProperty(ref RegulatoryRequirementsValue, value); }

        }
        private List<string> RegulatoryChallengesValue = new List<string>();

        public List<string> RegulatoryChallenges

        {

            get { return this.RegulatoryChallengesValue; }

            set { SetProperty(ref RegulatoryChallengesValue, value); }

        }
        
        // Overall feasibility assessment
        private bool? IsFeasibleValue;

        public bool? IsFeasible

        {

            get { return this.IsFeasibleValue; }

            set { SetProperty(ref IsFeasibleValue, value); }

        }
        private string? FeasibilityStatusValue;

        public string? FeasibilityStatus

        {

            get { return this.FeasibilityStatusValue; }

            set { SetProperty(ref FeasibilityStatusValue, value); }

        } // e.g., "FEASIBLE", "MARGINAL", "NOT_FEASIBLE"
        private string? FeasibilityRecommendationValue;

        public string? FeasibilityRecommendation

        {

            get { return this.FeasibilityRecommendationValue; }

            set { SetProperty(ref FeasibilityRecommendationValue, value); }

        } // Overall recommendation
        
        // Cost analysis
        private decimal? TotalCapitalCostValue;

        public decimal? TotalCapitalCost

        {

            get { return this.TotalCapitalCostValue; }

            set { SetProperty(ref TotalCapitalCostValue, value); }

        }
        private string? TotalCapitalCostCurrencyValue;

        public string? TotalCapitalCostCurrency

        {

            get { return this.TotalCapitalCostCurrencyValue; }

            set { SetProperty(ref TotalCapitalCostCurrencyValue, value); }

        }
        private decimal? TotalOperatingCostValue;

        public decimal? TotalOperatingCost

        {

            get { return this.TotalOperatingCostValue; }

            set { SetProperty(ref TotalOperatingCostValue, value); }

        }
        private string? TotalOperatingCostCurrencyValue;

        public string? TotalOperatingCostCurrency

        {

            get { return this.TotalOperatingCostCurrencyValue; }

            set { SetProperty(ref TotalOperatingCostCurrencyValue, value); }

        }
        private decimal? TotalRevenueValue;

        public decimal? TotalRevenue

        {

            get { return this.TotalRevenueValue; }

            set { SetProperty(ref TotalRevenueValue, value); }

        }
        private string? TotalRevenueCurrencyValue;

        public string? TotalRevenueCurrency

        {

            get { return this.TotalRevenueCurrencyValue; }

            set { SetProperty(ref TotalRevenueCurrencyValue, value); }

        }
        private decimal? NetCashFlowValue;

        public decimal? NetCashFlow

        {

            get { return this.NetCashFlowValue; }

            set { SetProperty(ref NetCashFlowValue, value); }

        }
        
        // Cash flow analysis
        private List<FeasibilityCashFlowPoint> CashFlowPointsValue = new List<FeasibilityCashFlowPoint>();

        public List<FeasibilityCashFlowPoint> CashFlowPoints

        {

            get { return this.CashFlowPointsValue; }

            set { SetProperty(ref CashFlowPointsValue, value); }

        }
        
        // Production analysis
        private decimal? TotalOilProductionValue;

        public decimal? TotalOilProduction

        {

            get { return this.TotalOilProductionValue; }

            set { SetProperty(ref TotalOilProductionValue, value); }

        }
        private decimal? TotalGasProductionValue;

        public decimal? TotalGasProduction

        {

            get { return this.TotalGasProductionValue; }

            set { SetProperty(ref TotalGasProductionValue, value); }

        }
        private string? ProductionOuomValue;

        public string? ProductionOuom

        {

            get { return this.ProductionOuomValue; }

            set { SetProperty(ref ProductionOuomValue, value); }

        }
        private decimal? PeakProductionRateValue;

        public decimal? PeakProductionRate

        {

            get { return this.PeakProductionRateValue; }

            set { SetProperty(ref PeakProductionRateValue, value); }

        }
        private int? PeakProductionYearValue;

        public int? PeakProductionYear

        {

            get { return this.PeakProductionYearValue; }

            set { SetProperty(ref PeakProductionYearValue, value); }

        }
        
        // Risk assessment
        private string? RiskLevelValue;

        public string? RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        } // e.g., "LOW", "MEDIUM", "HIGH"
        private List<string> KeyRisksValue = new List<string>();

        public List<string> KeyRisks

        {

            get { return this.KeyRisksValue; }

            set { SetProperty(ref KeyRisksValue, value); }

        }
        private List<string> RiskMitigationStrategiesValue = new List<string>();

        public List<string> RiskMitigationStrategies

        {

            get { return this.RiskMitigationStrategiesValue; }

            set { SetProperty(ref RiskMitigationStrategiesValue, value); }

        }
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Cash flow point for feasibility study
    /// </summary>
    public class FeasibilityCashFlowPoint : ModelEntityBase
    {
        private int YearValue;

        public int Year

        {

            get { return this.YearValue; }

            set { SetProperty(ref YearValue, value); }

        }
        private decimal? RevenueValue;

        public decimal? Revenue

        {

            get { return this.RevenueValue; }

            set { SetProperty(ref RevenueValue, value); }

        }
        private decimal? CapitalCostsValue;

        public decimal? CapitalCosts

        {

            get { return this.CapitalCostsValue; }

            set { SetProperty(ref CapitalCostsValue, value); }

        }
        private decimal? OperatingCostsValue;

        public decimal? OperatingCosts

        {

            get { return this.OperatingCostsValue; }

            set { SetProperty(ref OperatingCostsValue, value); }

        }
        private decimal? TaxesValue;

        public decimal? Taxes

        {

            get { return this.TaxesValue; }

            set { SetProperty(ref TaxesValue, value); }

        }
        private decimal? RoyaltiesValue;

        public decimal? Royalties

        {

            get { return this.RoyaltiesValue; }

            set { SetProperty(ref RoyaltiesValue, value); }

        }
        private decimal? NetCashFlowValue;

        public decimal? NetCashFlow

        {

            get { return this.NetCashFlowValue; }

            set { SetProperty(ref NetCashFlowValue, value); }

        }
        private decimal? CumulativeCashFlowValue;

        public decimal? CumulativeCashFlow

        {

            get { return this.CumulativeCashFlowValue; }

            set { SetProperty(ref CumulativeCashFlowValue, value); }

        }
        private decimal? DiscountedCashFlowValue;

        public decimal? DiscountedCashFlow

        {

            get { return this.DiscountedCashFlowValue; }

            set { SetProperty(ref DiscountedCashFlowValue, value); }

        }
        private decimal? CumulativeDiscountedCashFlowValue;

        public decimal? CumulativeDiscountedCashFlow

        {

            get { return this.CumulativeDiscountedCashFlowValue; }

            set { SetProperty(ref CumulativeDiscountedCashFlowValue, value); }

        }
    }

    #endregion
}


