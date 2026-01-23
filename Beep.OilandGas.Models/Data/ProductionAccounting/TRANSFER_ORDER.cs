using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TRANSFER_ORDER : ModelEntityBase {
        private System.String TRANSFER_ORDER_IDValue;
        public System.String TRANSFER_ORDER_ID
        {
            get { return this.TRANSFER_ORDER_IDValue; }
            set { SetProperty(ref TRANSFER_ORDER_IDValue, value); }
        }

        private System.String PROPERTY_OR_LEASE_IDValue;
        public System.String PROPERTY_OR_LEASE_ID
        {
            get { return this.PROPERTY_OR_LEASE_IDValue; }
            set { SetProperty(ref PROPERTY_OR_LEASE_IDValue, value); }
        }

        private System.String FROM_OWNER_IDValue;
        public System.String FROM_OWNER_ID
        {
            get { return this.FROM_OWNER_IDValue; }
            set { SetProperty(ref FROM_OWNER_IDValue, value); }
        }

        private System.String TO_OWNER_IDValue;
        public System.String TO_OWNER_ID
        {
            get { return this.TO_OWNER_IDValue; }
            set { SetProperty(ref TO_OWNER_IDValue, value); }
        }

        private System.Decimal? INTEREST_TRANSFERREDValue;
        public System.Decimal? INTEREST_TRANSFERRED
        {
            get { return this.INTEREST_TRANSFERREDValue; }
            set { SetProperty(ref INTEREST_TRANSFERREDValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.String IS_APPROVEDValue;
        public System.String IS_APPROVED
        {
            get { return this.IS_APPROVEDValue; }
            set { SetProperty(ref IS_APPROVEDValue, value); }
        }

        private System.DateTime? APPROVAL_DATEValue;
        public System.DateTime? APPROVAL_DATE
        {
            get { return this.APPROVAL_DATEValue; }
            set { SetProperty(ref APPROVAL_DATEValue, value); }
        }

        private System.String APPROVED_BYValue;
        public System.String APPROVED_BY
        {
            get { return this.APPROVED_BYValue; }
            set { SetProperty(ref APPROVED_BYValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


