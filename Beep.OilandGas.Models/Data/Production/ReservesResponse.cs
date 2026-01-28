using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
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
}
