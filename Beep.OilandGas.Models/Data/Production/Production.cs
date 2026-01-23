using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Field DTOs

    /// <summary>
    /// Request for creating or updating a field (maps to FIELD table)
    /// </summary>
    public class FieldRequest : ModelEntityBase
    {
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

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
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Dates
        private DateTime? DiscoveryDateValue;

        public DateTime? DiscoveryDate

        {

            get { return this.DiscoveryDateValue; }

            set { SetProperty(ref DiscoveryDateValue, value); }

        }
        private DateTime? FirstProductionDateValue;

        public DateTime? FirstProductionDate

        {

            get { return this.FirstProductionDateValue; }

            set { SetProperty(ref FirstProductionDateValue, value); }

        }
        
        // Classification
        private string? FieldTypeValue;

        public string? FieldType

        {

            get { return this.FieldTypeValue; }

            set { SetProperty(ref FieldTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Area information
        private decimal? AreaValue;

        public decimal? Area

        {

            get { return this.AreaValue; }

            set { SetProperty(ref AreaValue, value); }

        }
        private string? AreaOuomValue;

        public string? AreaOuom

        {

            get { return this.AreaOuomValue; }

            set { SetProperty(ref AreaOuomValue, value); }

        } // Unit of measure (e.g., "ACRE", "KM2")
        
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
    /// Response containing field data (includes audit fields from FIELD table)
    /// </summary>
    public class FieldResponse : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

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
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Dates
        private DateTime? DiscoveryDateValue;

        public DateTime? DiscoveryDate

        {

            get { return this.DiscoveryDateValue; }

            set { SetProperty(ref DiscoveryDateValue, value); }

        }
        private DateTime? FirstProductionDateValue;

        public DateTime? FirstProductionDate

        {

            get { return this.FirstProductionDateValue; }

            set { SetProperty(ref FirstProductionDateValue, value); }

        }
        
        // Classification
        private string? FieldTypeValue;

        public string? FieldType

        {

            get { return this.FieldTypeValue; }

            set { SetProperty(ref FieldTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Area information
        private decimal? AreaValue;

        public decimal? Area

        {

            get { return this.AreaValue; }

            set { SetProperty(ref AreaValue, value); }

        }
        private string? AreaOuomValue;

        public string? AreaOuom

        {

            get { return this.AreaOuomValue; }

            set { SetProperty(ref AreaOuomValue, value); }

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
        private object FieldValue;

        public object Field

        {

            get { return this.FieldValue; }

            set { SetProperty(ref FieldValue, value); }

        }
    }

    #endregion

    #region Production DTOs

    /// <summary>
    /// Request for creating or updating production data (maps to PDEN_VOL_SUMMARY table)
    /// </summary>
    public class ProductionRequest : ModelEntityBase
    {
        private string? ProductionIdValue;

        public string? ProductionId

        {

            get { return this.ProductionIdValue; }

            set { SetProperty(ref ProductionIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        
        // Production date and period
        private DateTime? ProductionDateValue;

        public DateTime? ProductionDate

        {

            get { return this.ProductionDateValue; }

            set { SetProperty(ref ProductionDateValue, value); }

        }
        private DateTime? ProductionPeriodStartValue;

        public DateTime? ProductionPeriodStart

        {

            get { return this.ProductionPeriodStartValue; }

            set { SetProperty(ref ProductionPeriodStartValue, value); }

        }
        private DateTime? ProductionPeriodEndValue;

        public DateTime? ProductionPeriodEnd

        {

            get { return this.ProductionPeriodEndValue; }

            set { SetProperty(ref ProductionPeriodEndValue, value); }

        }
        private int? ProductionDaysValue;

        public int? ProductionDays

        {

            get { return this.ProductionDaysValue; }

            set { SetProperty(ref ProductionDaysValue, value); }

        }
        
        // Volumes
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
        private decimal? CondensateVolumeValue;

        public decimal? CondensateVolume

        {

            get { return this.CondensateVolumeValue; }

            set { SetProperty(ref CondensateVolumeValue, value); }

        }
        private string? VolumeOuomValue;

        public string? VolumeOuom

        {

            get { return this.VolumeOuomValue; }

            set { SetProperty(ref VolumeOuomValue, value); }

        } // e.g., "BBL", "MSCF", "BBL/D"
        
        // Rates
        private decimal? OilRateValue;

        public decimal? OilRate

        {

            get { return this.OilRateValue; }

            set { SetProperty(ref OilRateValue, value); }

        }
        private decimal? GasRateValue;

        public decimal? GasRate

        {

            get { return this.GasRateValue; }

            set { SetProperty(ref GasRateValue, value); }

        }
        private decimal? WaterRateValue;

        public decimal? WaterRate

        {

            get { return this.WaterRateValue; }

            set { SetProperty(ref WaterRateValue, value); }

        }
        private string? RateOuomValue;

        public string? RateOuom

        {

            get { return this.RateOuomValue; }

            set { SetProperty(ref RateOuomValue, value); }

        } // e.g., "BBL/D", "MSCF/D"
        
        // Pressures
        private decimal? FlowingPressureValue;

        public decimal? FlowingPressure

        {

            get { return this.FlowingPressureValue; }

            set { SetProperty(ref FlowingPressureValue, value); }

        }
        private decimal? StaticPressureValue;

        public decimal? StaticPressure

        {

            get { return this.StaticPressureValue; }

            set { SetProperty(ref StaticPressureValue, value); }

        }
        private decimal? BottomHolePressureValue;

        public decimal? BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }
        private string? PressureOuomValue;

        public string? PressureOuom

        {

            get { return this.PressureOuomValue; }

            set { SetProperty(ref PressureOuomValue, value); }

        } // e.g., "PSI", "KPA"
        
        // Production classification
        private string? ProductionTypeValue;

        public string? ProductionType

        {

            get { return this.ProductionTypeValue; }

            set { SetProperty(ref ProductionTypeValue, value); }

        } // e.g., "NORMAL", "TEST", "ESTIMATED"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

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
    /// Response containing production data (includes audit fields from PDEN_VOL_SUMMARY table)
    /// </summary>
    public class ProductionResponse : ModelEntityBase
    {
        private string ProductionIdValue = string.Empty;

        public string ProductionId

        {

            get { return this.ProductionIdValue; }

            set { SetProperty(ref ProductionIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        
        // Production date and period
        private DateTime? ProductionDateValue;

        public DateTime? ProductionDate

        {

            get { return this.ProductionDateValue; }

            set { SetProperty(ref ProductionDateValue, value); }

        }
        private DateTime? ProductionPeriodStartValue;

        public DateTime? ProductionPeriodStart

        {

            get { return this.ProductionPeriodStartValue; }

            set { SetProperty(ref ProductionPeriodStartValue, value); }

        }
        private DateTime? ProductionPeriodEndValue;

        public DateTime? ProductionPeriodEnd

        {

            get { return this.ProductionPeriodEndValue; }

            set { SetProperty(ref ProductionPeriodEndValue, value); }

        }
        private int? ProductionDaysValue;

        public int? ProductionDays

        {

            get { return this.ProductionDaysValue; }

            set { SetProperty(ref ProductionDaysValue, value); }

        }
        
        // Volumes
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
        private decimal? CondensateVolumeValue;

        public decimal? CondensateVolume

        {

            get { return this.CondensateVolumeValue; }

            set { SetProperty(ref CondensateVolumeValue, value); }

        }
        private string? VolumeOuomValue;

        public string? VolumeOuom

        {

            get { return this.VolumeOuomValue; }

            set { SetProperty(ref VolumeOuomValue, value); }

        }
        
        // Rates
        private decimal? OilRateValue;

        public decimal? OilRate

        {

            get { return this.OilRateValue; }

            set { SetProperty(ref OilRateValue, value); }

        }
        private decimal? GasRateValue;

        public decimal? GasRate

        {

            get { return this.GasRateValue; }

            set { SetProperty(ref GasRateValue, value); }

        }
        private decimal? WaterRateValue;

        public decimal? WaterRate

        {

            get { return this.WaterRateValue; }

            set { SetProperty(ref WaterRateValue, value); }

        }
        private string? RateOuomValue;

        public string? RateOuom

        {

            get { return this.RateOuomValue; }

            set { SetProperty(ref RateOuomValue, value); }

        }
        
        // Pressures
        private decimal? FlowingPressureValue;

        public decimal? FlowingPressure

        {

            get { return this.FlowingPressureValue; }

            set { SetProperty(ref FlowingPressureValue, value); }

        }
        private decimal? StaticPressureValue;

        public decimal? StaticPressure

        {

            get { return this.StaticPressureValue; }

            set { SetProperty(ref StaticPressureValue, value); }

        }
        private decimal? BottomHolePressureValue;

        public decimal? BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }
        private string? PressureOuomValue;

        public string? PressureOuom

        {

            get { return this.PressureOuomValue; }

            set { SetProperty(ref PressureOuomValue, value); }

        }
        
        // Production classification
        private string? ProductionTypeValue;

        public string? ProductionType

        {

            get { return this.ProductionTypeValue; }

            set { SetProperty(ref ProductionTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

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

    #region Reserves DTOs

    /// <summary>
    /// Request for creating or updating reserves data (maps to RESENT table)
    /// </summary>
    public class ReservesRequest : ModelEntityBase
    {
        private string? ReservesIdValue;

        public string? ReservesId

        {

            get { return this.ReservesIdValue; }

            set { SetProperty(ref ReservesIdValue, value); }

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
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        
        // Reserve classification
        private string? ReserveCategoryValue;

        public string? ReserveCategory

        {

            get { return this.ReserveCategoryValue; }

            set { SetProperty(ref ReserveCategoryValue, value); }

        } // e.g., "PROVED", "PROBABLE", "POSSIBLE"
        private string? ReserveTypeValue;

        public string? ReserveType

        {

            get { return this.ReserveTypeValue; }

            set { SetProperty(ref ReserveTypeValue, value); }

        } // e.g., "DEVELOPED", "UNDEVELOPED"
        private string? ReserveClassificationValue;

        public string? ReserveClassification

        {

            get { return this.ReserveClassificationValue; }

            set { SetProperty(ref ReserveClassificationValue, value); }

        } // e.g., "PROVED_PRODUCING", "PROVED_NON_PRODUCING"
        
        // Effective date
        private DateTime? EffectiveDateValue;

        public DateTime? EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        
        // Volumes
        private decimal? OilReservesValue;

        public decimal? OilReserves

        {

            get { return this.OilReservesValue; }

            set { SetProperty(ref OilReservesValue, value); }

        }
        private decimal? GasReservesValue;

        public decimal? GasReserves

        {

            get { return this.GasReservesValue; }

            set { SetProperty(ref GasReservesValue, value); }

        }
        private decimal? CondensateReservesValue;

        public decimal? CondensateReserves

        {

            get { return this.CondensateReservesValue; }

            set { SetProperty(ref CondensateReservesValue, value); }

        }
        private string? ReservesOuomValue;

        public string? ReservesOuom

        {

            get { return this.ReservesOuomValue; }

            set { SetProperty(ref ReservesOuomValue, value); }

        } // e.g., "BBL", "MSCF"
        
        // Recovery factors
        private decimal? OilRecoveryFactorValue;

        public decimal? OilRecoveryFactor

        {

            get { return this.OilRecoveryFactorValue; }

            set { SetProperty(ref OilRecoveryFactorValue, value); }

        } // Percentage
        private decimal? GasRecoveryFactorValue;

        public decimal? GasRecoveryFactor

        {

            get { return this.GasRecoveryFactorValue; }

            set { SetProperty(ref GasRecoveryFactorValue, value); }

        } // Percentage
        
        // Economic parameters
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private string? PriceCurrencyValue;

        public string? PriceCurrency

        {

            get { return this.PriceCurrencyValue; }

            set { SetProperty(ref PriceCurrencyValue, value); }

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
    /// Response containing reserves data (includes audit fields from RESERVE_ENTITY table)
    /// </summary>
    public class ReservesResponse : ModelEntityBase
    {
        private string ReservesIdValue = string.Empty;

        public string ReservesId

        {

            get { return this.ReservesIdValue; }

            set { SetProperty(ref ReservesIdValue, value); }

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
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        
        // Reserve classification
        private string? ReserveCategoryValue;

        public string? ReserveCategory

        {

            get { return this.ReserveCategoryValue; }

            set { SetProperty(ref ReserveCategoryValue, value); }

        }
        private string? ReserveTypeValue;

        public string? ReserveType

        {

            get { return this.ReserveTypeValue; }

            set { SetProperty(ref ReserveTypeValue, value); }

        }
        private string? ReserveClassificationValue;

        public string? ReserveClassification

        {

            get { return this.ReserveClassificationValue; }

            set { SetProperty(ref ReserveClassificationValue, value); }

        }
        
        // Effective date
        private DateTime? EffectiveDateValue;

        public DateTime? EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        
        // Volumes
        private decimal? OilReservesValue;

        public decimal? OilReserves

        {

            get { return this.OilReservesValue; }

            set { SetProperty(ref OilReservesValue, value); }

        }
        private decimal? GasReservesValue;

        public decimal? GasReserves

        {

            get { return this.GasReservesValue; }

            set { SetProperty(ref GasReservesValue, value); }

        }
        private decimal? CondensateReservesValue;

        public decimal? CondensateReserves

        {

            get { return this.CondensateReservesValue; }

            set { SetProperty(ref CondensateReservesValue, value); }

        }
        private string? ReservesOuomValue;

        public string? ReservesOuom

        {

            get { return this.ReservesOuomValue; }

            set { SetProperty(ref ReservesOuomValue, value); }

        }
        
        // Recovery factors
        private decimal? OilRecoveryFactorValue;

        public decimal? OilRecoveryFactor

        {

            get { return this.OilRecoveryFactorValue; }

            set { SetProperty(ref OilRecoveryFactorValue, value); }

        }
        private decimal? GasRecoveryFactorValue;

        public decimal? GasRecoveryFactor

        {

            get { return this.GasRecoveryFactorValue; }

            set { SetProperty(ref GasRecoveryFactorValue, value); }

        }
        
        // Economic parameters
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private string? PriceCurrencyValue;

        public string? PriceCurrency

        {

            get { return this.PriceCurrencyValue; }

            set { SetProperty(ref PriceCurrencyValue, value); }

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

    #region Production Forecast DTOs

    /// <summary>
    /// Request for creating or updating production forecast (maps to PRODUCTION_FORECAST table)
    /// </summary>
    public class ProductionForecastRequest : ModelEntityBase
    {
        private string? ForecastIdValue;

        public string? ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

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
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        
        // Forecast classification
        private string? ForecastTypeValue;

        public string? ForecastType

        {

            get { return this.ForecastTypeValue; }

            set { SetProperty(ref ForecastTypeValue, value); }

        } // e.g., "DCA", "RESERVOIR_SIMULATION", "ANALOG"
        private string? ForecastMethodValue;

        public string? ForecastMethod

        {

            get { return this.ForecastMethodValue; }

            set { SetProperty(ref ForecastMethodValue, value); }

        } // e.g., "EXPONENTIAL", "HYPERBOLIC", "HARMONIC"
        private string? ForecastNameValue;

        public string? ForecastName

        {

            get { return this.ForecastNameValue; }

            set { SetProperty(ref ForecastNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Forecast period
        private DateTime? ForecastStartDateValue;

        public DateTime? ForecastStartDate

        {

            get { return this.ForecastStartDateValue; }

            set { SetProperty(ref ForecastStartDateValue, value); }

        }
        private DateTime? ForecastEndDateValue;

        public DateTime? ForecastEndDate

        {

            get { return this.ForecastEndDateValue; }

            set { SetProperty(ref ForecastEndDateValue, value); }

        }
        private int? ForecastPeriodMonthsValue;

        public int? ForecastPeriodMonths

        {

            get { return this.ForecastPeriodMonthsValue; }

            set { SetProperty(ref ForecastPeriodMonthsValue, value); }

        }
        
        // Forecast parameters
        private decimal? InitialRateValue;

        public decimal? InitialRate

        {

            get { return this.InitialRateValue; }

            set { SetProperty(ref InitialRateValue, value); }

        }
        private decimal? DeclineRateValue;

        public decimal? DeclineRate

        {

            get { return this.DeclineRateValue; }

            set { SetProperty(ref DeclineRateValue, value); }

        }
        private decimal? DeclineConstantValue;

        public decimal? DeclineConstant

        {

            get { return this.DeclineConstantValue; }

            set { SetProperty(ref DeclineConstantValue, value); }

        }
        private decimal? HyperbolicExponentValue;

        public decimal? HyperbolicExponent

        {

            get { return this.HyperbolicExponentValue; }

            set { SetProperty(ref HyperbolicExponentValue, value); }

        }
        
        // Forecasted volumes
        private decimal? ForecastOilVolumeValue;

        public decimal? ForecastOilVolume

        {

            get { return this.ForecastOilVolumeValue; }

            set { SetProperty(ref ForecastOilVolumeValue, value); }

        }
        private decimal? ForecastGasVolumeValue;

        public decimal? ForecastGasVolume

        {

            get { return this.ForecastGasVolumeValue; }

            set { SetProperty(ref ForecastGasVolumeValue, value); }

        }
        private string? ForecastVolumeOuomValue;

        public string? ForecastVolumeOuom

        {

            get { return this.ForecastVolumeOuomValue; }

            set { SetProperty(ref ForecastVolumeOuomValue, value); }

        }
        
        // Confidence levels
        private decimal? P10ForecastValue;

        public decimal? P10Forecast

        {

            get { return this.P10ForecastValue; }

            set { SetProperty(ref P10ForecastValue, value); }

        } // 10th percentile (optimistic)
        private decimal? P50ForecastValue;

        public decimal? P50Forecast

        {

            get { return this.P50ForecastValue; }

            set { SetProperty(ref P50ForecastValue, value); }

        } // 50th percentile (most likely)
        private decimal? P90ForecastValue;

        public decimal? P90Forecast

        {

            get { return this.P90ForecastValue; }

            set { SetProperty(ref P90ForecastValue, value); }

        } // 90th percentile (conservative)
        
        // Status
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "DRAFT", "APPROVED", "SUPERSEDED"
        
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
    /// Response containing production forecast data (includes audit fields from PRODUCTION_FORECAST table)
    /// </summary>
    public class ProductionForecastResponse : ModelEntityBase
    {
        private string ForecastIdValue = string.Empty;

        public string ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

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
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        
        // Forecast classification
        private string? ForecastTypeValue;

        public string? ForecastType

        {

            get { return this.ForecastTypeValue; }

            set { SetProperty(ref ForecastTypeValue, value); }

        }
        private string? ForecastMethodValue;

        public string? ForecastMethod

        {

            get { return this.ForecastMethodValue; }

            set { SetProperty(ref ForecastMethodValue, value); }

        }
        private string? ForecastNameValue;

        public string? ForecastName

        {

            get { return this.ForecastNameValue; }

            set { SetProperty(ref ForecastNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Forecast period
        private DateTime? ForecastStartDateValue;

        public DateTime? ForecastStartDate

        {

            get { return this.ForecastStartDateValue; }

            set { SetProperty(ref ForecastStartDateValue, value); }

        }
        private DateTime? ForecastEndDateValue;

        public DateTime? ForecastEndDate

        {

            get { return this.ForecastEndDateValue; }

            set { SetProperty(ref ForecastEndDateValue, value); }

        }
        private int? ForecastPeriodMonthsValue;

        public int? ForecastPeriodMonths

        {

            get { return this.ForecastPeriodMonthsValue; }

            set { SetProperty(ref ForecastPeriodMonthsValue, value); }

        }
        
        // Forecast parameters
        private decimal? InitialRateValue;

        public decimal? InitialRate

        {

            get { return this.InitialRateValue; }

            set { SetProperty(ref InitialRateValue, value); }

        }
        private decimal? DeclineRateValue;

        public decimal? DeclineRate

        {

            get { return this.DeclineRateValue; }

            set { SetProperty(ref DeclineRateValue, value); }

        }
        private decimal? DeclineConstantValue;

        public decimal? DeclineConstant

        {

            get { return this.DeclineConstantValue; }

            set { SetProperty(ref DeclineConstantValue, value); }

        }
        private decimal? HyperbolicExponentValue;

        public decimal? HyperbolicExponent

        {

            get { return this.HyperbolicExponentValue; }

            set { SetProperty(ref HyperbolicExponentValue, value); }

        }
        
        // Forecasted volumes
        private decimal? ForecastOilVolumeValue;

        public decimal? ForecastOilVolume

        {

            get { return this.ForecastOilVolumeValue; }

            set { SetProperty(ref ForecastOilVolumeValue, value); }

        }
        private decimal? ForecastGasVolumeValue;

        public decimal? ForecastGasVolume

        {

            get { return this.ForecastGasVolumeValue; }

            set { SetProperty(ref ForecastGasVolumeValue, value); }

        }
        private string? ForecastVolumeOuomValue;

        public string? ForecastVolumeOuom

        {

            get { return this.ForecastVolumeOuomValue; }

            set { SetProperty(ref ForecastVolumeOuomValue, value); }

        }
        
        // Confidence levels
        private decimal? P10ForecastValue;

        public decimal? P10Forecast

        {

            get { return this.P10ForecastValue; }

            set { SetProperty(ref P10ForecastValue, value); }

        }
        private decimal? P50ForecastValue;

        public decimal? P50Forecast

        {

            get { return this.P50ForecastValue; }

            set { SetProperty(ref P50ForecastValue, value); }

        }
        private decimal? P90ForecastValue;

        public decimal? P90Forecast

        {

            get { return this.P90ForecastValue; }

            set { SetProperty(ref P90ForecastValue, value); }

        }
        
        // Status
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

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

    #region Well Test DTOs

    /// <summary>
    /// Request for creating or updating well test data (maps to WELL_TEST table)
    /// </summary>
    public class WellTestRequest : ModelEntityBase
    {
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? WellboreIdValue;

        public string? WellboreId

        {

            get { return this.WellboreIdValue; }

            set { SetProperty(ref WellboreIdValue, value); }

        }
        
        // Test classification
        private string? TestTypeValue;

        public string? TestType

        {

            get { return this.TestTypeValue; }

            set { SetProperty(ref TestTypeValue, value); }

        } // e.g., "FLOW_TEST", "BUILDUP_TEST", "INTERFERENCE_TEST"
        private string? TestPurposeValue;

        public string? TestPurpose

        {

            get { return this.TestPurposeValue; }

            set { SetProperty(ref TestPurposeValue, value); }

        } // e.g., "PRODUCTION_TEST", "DRILLSTEM_TEST"
        private string? TestNameValue;

        public string? TestName

        {

            get { return this.TestNameValue; }

            set { SetProperty(ref TestNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Test dates
        private DateTime? TestStartDateValue;

        public DateTime? TestStartDate

        {

            get { return this.TestStartDateValue; }

            set { SetProperty(ref TestStartDateValue, value); }

        }
        private DateTime? TestEndDateValue;

        public DateTime? TestEndDate

        {

            get { return this.TestEndDateValue; }

            set { SetProperty(ref TestEndDateValue, value); }

        }
        private DateTime? TestDateValue;

        public DateTime? TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
        
        // Test conditions
        private decimal? TestDurationValue;

        public decimal? TestDuration

        {

            get { return this.TestDurationValue; }

            set { SetProperty(ref TestDurationValue, value); }

        } // Hours
        private string? TestStatusValue;

        public string? TestStatus

        {

            get { return this.TestStatusValue; }

            set { SetProperty(ref TestStatusValue, value); }

        } // e.g., "COMPLETED", "ABANDONED", "IN_PROGRESS"
        
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
    /// Response containing well test data (includes audit fields from WELL_TEST table)
    /// </summary>
    public class WellTestResponse : ModelEntityBase
    {
        private string TestIdValue = string.Empty;

        public string TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? WellboreIdValue;

        public string? WellboreId

        {

            get { return this.WellboreIdValue; }

            set { SetProperty(ref WellboreIdValue, value); }

        }
        
        // Test classification
        private string? TestTypeValue;

        public string? TestType

        {

            get { return this.TestTypeValue; }

            set { SetProperty(ref TestTypeValue, value); }

        }
        private string? TestPurposeValue;

        public string? TestPurpose

        {

            get { return this.TestPurposeValue; }

            set { SetProperty(ref TestPurposeValue, value); }

        }
        private string? TestNameValue;

        public string? TestName

        {

            get { return this.TestNameValue; }

            set { SetProperty(ref TestNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Test dates
        private DateTime? TestStartDateValue;

        public DateTime? TestStartDate

        {

            get { return this.TestStartDateValue; }

            set { SetProperty(ref TestStartDateValue, value); }

        }
        private DateTime? TestEndDateValue;

        public DateTime? TestEndDate

        {

            get { return this.TestEndDateValue; }

            set { SetProperty(ref TestEndDateValue, value); }

        }
        private DateTime? TestDateValue;

        public DateTime? TestDate

        {

            get { return this.TestDateValue; }

            set { SetProperty(ref TestDateValue, value); }

        }
        
        // Test conditions
        private decimal? TestDurationValue;

        public decimal? TestDuration

        {

            get { return this.TestDurationValue; }

            set { SetProperty(ref TestDurationValue, value); }

        }
        private string? TestStatusValue;

        public string? TestStatus

        {

            get { return this.TestStatusValue; }

            set { SetProperty(ref TestStatusValue, value); }

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

    #region Well Test Analysis DTOs

    /// <summary>
    /// Request for creating or updating well test analysis data (maps to WELL_TEST_ANALYSIS table)
    /// </summary>
    public class WellTestAnalysisRequest : ModelEntityBase
    {
        private string? AnalysisIdValue;

        public string? AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        
        // Analysis parameters
        private decimal? PermeabilityValue;

        public decimal? Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private string? PermeabilityOuomValue;

        public string? PermeabilityOuom

        {

            get { return this.PermeabilityOuomValue; }

            set { SetProperty(ref PermeabilityOuomValue, value); }

        } // e.g., "MD", "DARCY"
        private decimal? SkinValue;

        public decimal? Skin

        {

            get { return this.SkinValue; }

            set { SetProperty(ref SkinValue, value); }

        }
        private decimal? ProductivityIndexValue;

        public decimal? ProductivityIndex

        {

            get { return this.ProductivityIndexValue; }

            set { SetProperty(ref ProductivityIndexValue, value); }

        }
        private string? ProductivityIndexOuomValue;

        public string? ProductivityIndexOuom

        {

            get { return this.ProductivityIndexOuomValue; }

            set { SetProperty(ref ProductivityIndexOuomValue, value); }

        }
        
        // Flow potential
        private decimal? AofPotentialValue;

        public decimal? AofPotential

        {

            get { return this.AofPotentialValue; }

            set { SetProperty(ref AofPotentialValue, value); }

        } // Absolute Open Flow
        private string? AofPotentialOuomValue;

        public string? AofPotentialOuom

        {

            get { return this.AofPotentialOuomValue; }

            set { SetProperty(ref AofPotentialOuomValue, value); }

        }
        
        // Wellbore storage
        private decimal? WellboreStorageCoeffValue;

        public decimal? WellboreStorageCoeff

        {

            get { return this.WellboreStorageCoeffValue; }

            set { SetProperty(ref WellboreStorageCoeffValue, value); }

        }
        private string? WellboreStorageOuomValue;

        public string? WellboreStorageOuom

        {

            get { return this.WellboreStorageOuomValue; }

            set { SetProperty(ref WellboreStorageOuomValue, value); }

        }
        
        // Flow efficiency
        private decimal? FlowEfficiencyValue;

        public decimal? FlowEfficiency

        {

            get { return this.FlowEfficiencyValue; }

            set { SetProperty(ref FlowEfficiencyValue, value); }

        } // Percentage
        
        // Analysis method
        private string? AnalysisMethodValue;

        public string? AnalysisMethod

        {

            get { return this.AnalysisMethodValue; }

            set { SetProperty(ref AnalysisMethodValue, value); }

        } // e.g., "HORNER", "MDH", "AGARWAL"
        
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
    /// Response containing well test analysis data (includes audit fields from WELL_TEST_ANALYSIS table)
    /// </summary>
    public class WellTestAnalysisResponse : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        
        // Analysis parameters
        private decimal? PermeabilityValue;

        public decimal? Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private string? PermeabilityOuomValue;

        public string? PermeabilityOuom

        {

            get { return this.PermeabilityOuomValue; }

            set { SetProperty(ref PermeabilityOuomValue, value); }

        }
        private decimal? SkinValue;

        public decimal? Skin

        {

            get { return this.SkinValue; }

            set { SetProperty(ref SkinValue, value); }

        }
        private decimal? ProductivityIndexValue;

        public decimal? ProductivityIndex

        {

            get { return this.ProductivityIndexValue; }

            set { SetProperty(ref ProductivityIndexValue, value); }

        }
        private string? ProductivityIndexOuomValue;

        public string? ProductivityIndexOuom

        {

            get { return this.ProductivityIndexOuomValue; }

            set { SetProperty(ref ProductivityIndexOuomValue, value); }

        }
        
        // Flow potential
        private decimal? AofPotentialValue;

        public decimal? AofPotential

        {

            get { return this.AofPotentialValue; }

            set { SetProperty(ref AofPotentialValue, value); }

        }
        private string? AofPotentialOuomValue;

        public string? AofPotentialOuom

        {

            get { return this.AofPotentialOuomValue; }

            set { SetProperty(ref AofPotentialOuomValue, value); }

        }
        
        // Wellbore storage
        private decimal? WellboreStorageCoeffValue;

        public decimal? WellboreStorageCoeff

        {

            get { return this.WellboreStorageCoeffValue; }

            set { SetProperty(ref WellboreStorageCoeffValue, value); }

        }
        private string? WellboreStorageOuomValue;

        public string? WellboreStorageOuom

        {

            get { return this.WellboreStorageOuomValue; }

            set { SetProperty(ref WellboreStorageOuomValue, value); }

        }
        
        // Flow efficiency
        private decimal? FlowEfficiencyValue;

        public decimal? FlowEfficiency

        {

            get { return this.FlowEfficiencyValue; }

            set { SetProperty(ref FlowEfficiencyValue, value); }

        }
        
        // Analysis method
        private string? AnalysisMethodValue;

        public string? AnalysisMethod

        {

            get { return this.AnalysisMethodValue; }

            set { SetProperty(ref AnalysisMethodValue, value); }

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

    #region Well Test Flow DTOs

    /// <summary>
    /// Request for creating or updating well test flow data (maps to WELL_TEST_FLOW table)
    /// </summary>
    public class WellTestFlowRequest : ModelEntityBase
    {
        private string? FlowIdValue;

        public string? FlowId

        {

            get { return this.FlowIdValue; }

            set { SetProperty(ref FlowIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        
        // Flow period
        private DateTime? FlowDateValue;

        public DateTime? FlowDate

        {

            get { return this.FlowDateValue; }

            set { SetProperty(ref FlowDateValue, value); }

        }
        private decimal? FlowDurationValue;

        public decimal? FlowDuration

        {

            get { return this.FlowDurationValue; }

            set { SetProperty(ref FlowDurationValue, value); }

        } // Hours
        
        // Flow rates
        private decimal? FlowRateOilValue;

        public decimal? FlowRateOil

        {

            get { return this.FlowRateOilValue; }

            set { SetProperty(ref FlowRateOilValue, value); }

        }
        private decimal? FlowRateGasValue;

        public decimal? FlowRateGas

        {

            get { return this.FlowRateGasValue; }

            set { SetProperty(ref FlowRateGasValue, value); }

        }
        private decimal? FlowRateWaterValue;

        public decimal? FlowRateWater

        {

            get { return this.FlowRateWaterValue; }

            set { SetProperty(ref FlowRateWaterValue, value); }

        }
        private string? FlowRateOuomValue;

        public string? FlowRateOuom

        {

            get { return this.FlowRateOuomValue; }

            set { SetProperty(ref FlowRateOuomValue, value); }

        } // e.g., "BBL/D", "MSCF/D"
        
        // Choke information
        private decimal? ChokeSizeValue;

        public decimal? ChokeSize

        {

            get { return this.ChokeSizeValue; }

            set { SetProperty(ref ChokeSizeValue, value); }

        }
        private string? ChokeSizeOuomValue;

        public string? ChokeSizeOuom

        {

            get { return this.ChokeSizeOuomValue; }

            set { SetProperty(ref ChokeSizeOuomValue, value); }

        } // e.g., "IN", "MM"
        private string? ChokeTypeValue;

        public string? ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

        } // e.g., "BEAN", "ADJUSTABLE"
        
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
    /// Response containing well test flow data (includes audit fields from WELL_TEST_FLOW table)
    /// </summary>
    public class WellTestFlowResponse : ModelEntityBase
    {
        private string FlowIdValue = string.Empty;

        public string FlowId

        {

            get { return this.FlowIdValue; }

            set { SetProperty(ref FlowIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        
        // Flow period
        private DateTime? FlowDateValue;

        public DateTime? FlowDate

        {

            get { return this.FlowDateValue; }

            set { SetProperty(ref FlowDateValue, value); }

        }
        private decimal? FlowDurationValue;

        public decimal? FlowDuration

        {

            get { return this.FlowDurationValue; }

            set { SetProperty(ref FlowDurationValue, value); }

        }
        
        // Flow rates
        private decimal? FlowRateOilValue;

        public decimal? FlowRateOil

        {

            get { return this.FlowRateOilValue; }

            set { SetProperty(ref FlowRateOilValue, value); }

        }
        private decimal? FlowRateGasValue;

        public decimal? FlowRateGas

        {

            get { return this.FlowRateGasValue; }

            set { SetProperty(ref FlowRateGasValue, value); }

        }
        private decimal? FlowRateWaterValue;

        public decimal? FlowRateWater

        {

            get { return this.FlowRateWaterValue; }

            set { SetProperty(ref FlowRateWaterValue, value); }

        }
        private string? FlowRateOuomValue;

        public string? FlowRateOuom

        {

            get { return this.FlowRateOuomValue; }

            set { SetProperty(ref FlowRateOuomValue, value); }

        }
        
        // Choke information
        private decimal? ChokeSizeValue;

        public decimal? ChokeSize

        {

            get { return this.ChokeSizeValue; }

            set { SetProperty(ref ChokeSizeValue, value); }

        }
        private string? ChokeSizeOuomValue;

        public string? ChokeSizeOuom

        {

            get { return this.ChokeSizeOuomValue; }

            set { SetProperty(ref ChokeSizeOuomValue, value); }

        }
        private string? ChokeTypeValue;

        public string? ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

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

    #region Well Test Pressure DTOs

    /// <summary>
    /// Request for creating or updating well test pressure data (maps to WELL_TEST_PRESSURE table)
    /// </summary>
    public class WellTestPressureRequest : ModelEntityBase
    {
        private string? PressureIdValue;

        public string? PressureId

        {

            get { return this.PressureIdValue; }

            set { SetProperty(ref PressureIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        
        // Pressure measurement
        private DateTime? PressureDateValue;

        public DateTime? PressureDate

        {

            get { return this.PressureDateValue; }

            set { SetProperty(ref PressureDateValue, value); }

        }
        private decimal? StaticPressureValue;

        public decimal? StaticPressure

        {

            get { return this.StaticPressureValue; }

            set { SetProperty(ref StaticPressureValue, value); }

        }
        private decimal? FlowingPressureValue;

        public decimal? FlowingPressure

        {

            get { return this.FlowingPressureValue; }

            set { SetProperty(ref FlowingPressureValue, value); }

        }
        private decimal? BottomHolePressureValue;

        public decimal? BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }
        private decimal? WellheadPressureValue;

        public decimal? WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        }
        private string? PressureOuomValue;

        public string? PressureOuom

        {

            get { return this.PressureOuomValue; }

            set { SetProperty(ref PressureOuomValue, value); }

        } // e.g., "PSI", "KPA", "BAR"
        
        // Pressure type
        private string? PressureTypeValue;

        public string? PressureType

        {

            get { return this.PressureTypeValue; }

            set { SetProperty(ref PressureTypeValue, value); }

        } // e.g., "INITIAL", "FINAL", "AVERAGE"
        
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
    /// Response containing well test pressure data (includes audit fields from WELL_TEST_PRESSURE table)
    /// </summary>
    public class WellTestPressureResponse : ModelEntityBase
    {
        private string PressureIdValue = string.Empty;

        public string PressureId

        {

            get { return this.PressureIdValue; }

            set { SetProperty(ref PressureIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        
        // Pressure measurement
        private DateTime? PressureDateValue;

        public DateTime? PressureDate

        {

            get { return this.PressureDateValue; }

            set { SetProperty(ref PressureDateValue, value); }

        }
        private decimal? StaticPressureValue;

        public decimal? StaticPressure

        {

            get { return this.StaticPressureValue; }

            set { SetProperty(ref StaticPressureValue, value); }

        }
        private decimal? FlowingPressureValue;

        public decimal? FlowingPressure

        {

            get { return this.FlowingPressureValue; }

            set { SetProperty(ref FlowingPressureValue, value); }

        }
        private decimal? BottomHolePressureValue;

        public decimal? BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        }
        private decimal? WellheadPressureValue;

        public decimal? WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        }
        private string? PressureOuomValue;

        public string? PressureOuom

        {

            get { return this.PressureOuomValue; }

            set { SetProperty(ref PressureOuomValue, value); }

        }
        
        // Pressure type
        private string? PressureTypeValue;

        public string? PressureType

        {

            get { return this.PressureTypeValue; }

            set { SetProperty(ref PressureTypeValue, value); }

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
}


