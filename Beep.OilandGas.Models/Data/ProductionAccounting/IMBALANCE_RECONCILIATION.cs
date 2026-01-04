using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class IMBALANCE_RECONCILIATION : Entity, IPPDMEntity
    {
        private System.String IMBALANCE_RECONCILIATION_IDValue;
        public System.String IMBALANCE_RECONCILIATION_ID
        {
            get { return this.IMBALANCE_RECONCILIATION_IDValue; }
            set { SetProperty(ref IMBALANCE_RECONCILIATION_IDValue, value); }
        }

        private System.DateTime? RECONCILIATION_DATEValue;
        public System.DateTime? RECONCILIATION_DATE
        {
            get { return this.RECONCILIATION_DATEValue; }
            set { SetProperty(ref RECONCILIATION_DATEValue, value); }
        }

        private System.Decimal? TOTAL_IMBALANCE_BEFOREValue;
        public System.Decimal? TOTAL_IMBALANCE_BEFORE
        {
            get { return this.TOTAL_IMBALANCE_BEFOREValue; }
            set { SetProperty(ref TOTAL_IMBALANCE_BEFOREValue, value); }
        }

        private System.Decimal? TOTAL_ADJUSTMENTSValue;
        public System.Decimal? TOTAL_ADJUSTMENTS
        {
            get { return this.TOTAL_ADJUSTMENTSValue; }
            set { SetProperty(ref TOTAL_ADJUSTMENTSValue, value); }
        }

        private System.Decimal? TOTAL_IMBALANCE_AFTERValue;
        public System.Decimal? TOTAL_IMBALANCE_AFTER
        {
            get { return this.TOTAL_IMBALANCE_AFTERValue; }
            set { SetProperty(ref TOTAL_IMBALANCE_AFTERValue, value); }
        }

        private System.String RECONCILED_BYValue;
        public System.String RECONCILED_BY
        {
            get { return this.RECONCILED_BYValue; }
            set { SetProperty(ref RECONCILED_BYValue, value); }
        }

        private System.String NOTESValue;
        public System.String NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

        // Standard PPDM columns
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
