using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Core.Interfaces;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class QUALITY_ADJUSTMENTS : Entity, IPPDMEntity
    {
        private System.String QUALITY_ADJUSTMENTS_IDValue;
        public System.String QUALITY_ADJUSTMENTS_ID
        {
            get { return this.QUALITY_ADJUSTMENTS_IDValue; }
            set { SetProperty(ref QUALITY_ADJUSTMENTS_IDValue, value); }
        }

        private System.Decimal? API_GRAVITY_ADJUSTMENTValue;
        public System.Decimal? API_GRAVITY_ADJUSTMENT
        {
            get { return this.API_GRAVITY_ADJUSTMENTValue; }
            set { SetProperty(ref API_GRAVITY_ADJUSTMENTValue, value); }
        }

        private System.Decimal? SULFUR_ADJUSTMENTValue;
        public System.Decimal? SULFUR_ADJUSTMENT
        {
            get { return this.SULFUR_ADJUSTMENTValue; }
            set { SetProperty(ref SULFUR_ADJUSTMENTValue, value); }
        }

        private System.Decimal? BSW_ADJUSTMENTValue;
        public System.Decimal? BSW_ADJUSTMENT
        {
            get { return this.BSW_ADJUSTMENTValue; }
            set { SetProperty(ref BSW_ADJUSTMENTValue, value); }
        }

        private System.Decimal? OTHER_ADJUSTMENTSValue;
        public System.Decimal? OTHER_ADJUSTMENTS
        {
            get { return this.OTHER_ADJUSTMENTSValue; }
            set { SetProperty(ref OTHER_ADJUSTMENTSValue, value); }
        }

        private System.Decimal? TOTAL_ADJUSTMENTValue;
        public System.Decimal? TOTAL_ADJUSTMENT
        {
            get { return this.TOTAL_ADJUSTMENTValue; }
            set { SetProperty(ref TOTAL_ADJUSTMENTValue, value); }
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
