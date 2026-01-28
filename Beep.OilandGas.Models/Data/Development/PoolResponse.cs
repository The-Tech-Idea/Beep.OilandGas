using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
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
}
