using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ROYALTY_CALCULATION : ModelEntityBase {
        private System.String ROYALTY_CALCULATION_IDValue;
        public System.String ROYALTY_CALCULATION_ID
        {
            get { return this.ROYALTY_CALCULATION_IDValue; }
            set { SetProperty(ref ROYALTY_CALCULATION_IDValue, value); }
        }

        private System.DateTime? CALCULATION_DATEValue;
        public System.DateTime? CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
        }

        private System.String PROPERTY_OR_LEASE_IDValue;
        public System.String PROPERTY_OR_LEASE_ID
        {
            get { return this.PROPERTY_OR_LEASE_IDValue; }
            set { SetProperty(ref PROPERTY_OR_LEASE_IDValue, value); }
        }

        private System.String ALLOCATION_RESULT_IDValue;
        public System.String ALLOCATION_RESULT_ID
        {
            get { return this.ALLOCATION_RESULT_IDValue; }
            set { SetProperty(ref ALLOCATION_RESULT_IDValue, value); }
        }

        private System.String ROYALTY_INTEREST_IDValue;
        public System.String ROYALTY_INTEREST_ID
        {
            get { return this.ROYALTY_INTEREST_IDValue; }
            set { SetProperty(ref ROYALTY_INTEREST_IDValue, value); }
        }

        private System.String ROYALTY_OWNER_IDValue;
        public System.String ROYALTY_OWNER_ID
        {
            get { return this.ROYALTY_OWNER_IDValue; }
            set { SetProperty(ref ROYALTY_OWNER_IDValue, value); }
        }

        private System.Decimal? GROSS_REVENUEValue;
        public System.Decimal? GROSS_REVENUE
        {
            get { return this.GROSS_REVENUEValue; }
            set { SetProperty(ref GROSS_REVENUEValue, value); }
        }

        private System.String ROYALTY_DEDUCTIONS_IDValue;
        public System.String ROYALTY_DEDUCTIONS_ID
        {
            get { return this.ROYALTY_DEDUCTIONS_IDValue; }
            set { SetProperty(ref ROYALTY_DEDUCTIONS_IDValue, value); }
        }

        private System.Decimal? NET_REVENUEValue;
        public System.Decimal? NET_REVENUE
        {
            get { return this.NET_REVENUEValue; }
            set { SetProperty(ref NET_REVENUEValue, value); }
        }

        private System.Decimal? ROYALTY_INTERESTValue;
        public System.Decimal? ROYALTY_INTEREST
        {
            get { return this.ROYALTY_INTERESTValue; }
            set { SetProperty(ref ROYALTY_INTERESTValue, value); }
        }

         private System.Decimal? ROYALTY_AMOUNTValue;
         public System.Decimal? ROYALTY_AMOUNT
         {
             get { return this.ROYALTY_AMOUNTValue; }
             set { SetProperty(ref ROYALTY_AMOUNTValue, value); }
         }

         private System.String ALLOCATION_DETAIL_IDValue;
         public System.String ALLOCATION_DETAIL_ID
         {
             get { return this.ALLOCATION_DETAIL_IDValue; }
             set { SetProperty(ref ALLOCATION_DETAIL_IDValue, value); }
         }

         private System.Decimal? TRANSPORTATION_COSTValue;
         public System.Decimal? TRANSPORTATION_COST
         {
             get { return this.TRANSPORTATION_COSTValue; }
             set { SetProperty(ref TRANSPORTATION_COSTValue, value); }
         }

         private System.Decimal? AD_VALOREM_TAXValue;
         public System.Decimal? AD_VALOREM_TAX
         {
             get { return this.AD_VALOREM_TAXValue; }
             set { SetProperty(ref AD_VALOREM_TAXValue, value); }
         }

         private System.Decimal? SEVERANCE_TAXValue;
         public System.Decimal? SEVERANCE_TAX
         {
             get { return this.SEVERANCE_TAXValue; }
             set { SetProperty(ref SEVERANCE_TAXValue, value); }
         }

         private System.String ROYALTY_STATUSValue;
         public System.String ROYALTY_STATUS
         {
             get { return this.ROYALTY_STATUSValue; }
             set { SetProperty(ref ROYALTY_STATUSValue, value); }
         }

        // ── Best-practice additions (COPAS / state royalty rules) ────────────────────

        // ROYALTY_TYPE: MINERAL / OVERRIDING / WORKING_INTEREST / NPI / ORRI
        private System.String ROYALTY_TYPEValue;
        public System.String ROYALTY_TYPE
        {
            get { return this.ROYALTY_TYPEValue; }
            set { SetProperty(ref ROYALTY_TYPEValue, value); }
        }

        // ROYALTY_BASIS: GROSS / NET_BACK / WELLHEAD / PROCESSED (basis for value calculation)
        private System.String ROYALTY_BASISValue;
        public System.String ROYALTY_BASIS
        {
            get { return this.ROYALTY_BASISValue; }
            set { SetProperty(ref ROYALTY_BASISValue, value); }
        }

        // FLUID_TYPE: OIL / GAS / NGL / CONDENSATE / WATER
        private System.String FLUID_TYPEValue;
        public System.String FLUID_TYPE
        {
            get { return this.FLUID_TYPEValue; }
            set { SetProperty(ref FLUID_TYPEValue, value); }
        }

        private System.DateTime? PRODUCTION_PERIOD_STARTValue;
        public System.DateTime? PRODUCTION_PERIOD_START
        {
            get { return this.PRODUCTION_PERIOD_STARTValue; }
            set { SetProperty(ref PRODUCTION_PERIOD_STARTValue, value); }
        }

        private System.Decimal? GROSS_VOLUMEValue;
        public System.Decimal? GROSS_VOLUME
        {
            get { return this.GROSS_VOLUMEValue; }
            set { SetProperty(ref GROSS_VOLUMEValue, value); }
        }

        private System.String GROSS_VOLUME_OUOMValue;
        public System.String GROSS_VOLUME_OUOM
        {
            get { return this.GROSS_VOLUME_OUOMValue; }
            set { SetProperty(ref GROSS_VOLUME_OUOMValue, value); }
        }

        private System.Decimal? PRICE_PER_UNITValue;
        public System.Decimal? PRICE_PER_UNIT
        {
            get { return this.PRICE_PER_UNITValue; }
            set { SetProperty(ref PRICE_PER_UNITValue, value); }
        }

        // PAYMENT_STATUS: PENDING / PAID / DISPUTED / SUSPENDED
        private System.String PAYMENT_STATUSValue;
        public System.String PAYMENT_STATUS
        {
            get { return this.PAYMENT_STATUSValue; }
            set { SetProperty(ref PAYMENT_STATUSValue, value); }
        }

    }
}
