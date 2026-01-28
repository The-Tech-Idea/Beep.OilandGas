using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
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
}
