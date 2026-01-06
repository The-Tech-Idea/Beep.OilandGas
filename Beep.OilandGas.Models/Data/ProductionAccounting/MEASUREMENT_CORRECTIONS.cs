using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class MEASUREMENT_CORRECTIONS : Entity, IPPDMEntity
    {
        private System.String MEASUREMENT_CORRECTIONS_IDValue;
        public System.String MEASUREMENT_CORRECTIONS_ID
        {
            get { return this.MEASUREMENT_CORRECTIONS_IDValue; }
            set { SetProperty(ref MEASUREMENT_CORRECTIONS_IDValue, value); }
        }

        private System.String MEASUREMENT_RECORD_IDValue;
        public System.String MEASUREMENT_RECORD_ID
        {
            get { return this.MEASUREMENT_RECORD_IDValue; }
            set { SetProperty(ref MEASUREMENT_RECORD_IDValue, value); }
        }

        private System.Decimal? TEMPERATURE_CORRECTION_FACTORValue;
        public System.Decimal? TEMPERATURE_CORRECTION_FACTOR
        {
            get { return this.TEMPERATURE_CORRECTION_FACTORValue; }
            set { SetProperty(ref TEMPERATURE_CORRECTION_FACTORValue, value); }
        }

        private System.Decimal? PRESSURE_CORRECTION_FACTORValue;
        public System.Decimal? PRESSURE_CORRECTION_FACTOR
        {
            get { return this.PRESSURE_CORRECTION_FACTORValue; }
            set { SetProperty(ref PRESSURE_CORRECTION_FACTORValue, value); }
        }

        private System.Decimal? METER_FACTORValue;
        public System.Decimal? METER_FACTOR
        {
            get { return this.METER_FACTORValue; }
            set { SetProperty(ref METER_FACTORValue, value); }
        }

        private System.Decimal? SHRINKAGE_FACTORValue;
        public System.Decimal? SHRINKAGE_FACTOR
        {
            get { return this.SHRINKAGE_FACTORValue; }
            set { SetProperty(ref SHRINKAGE_FACTORValue, value); }
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




