using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class OIL_PROPERTY_RESULT : Entity
    {
        private string CALCULATION_IDValue;
        public string CALCULATION_ID
        {
            get { return this.CALCULATION_IDValue; }
            set { SetProperty(ref CALCULATION_IDValue, value); }
        }

        private string OIL_COMPOSITION_IDValue;
        public string OIL_COMPOSITION_ID
        {
            get { return this.OIL_COMPOSITION_IDValue; }
            set { SetProperty(ref OIL_COMPOSITION_IDValue, value); }
        }

        private decimal? PRESSUREValue;
        public decimal? PRESSURE
        {
            get { return this.PRESSUREValue; }
            set { SetProperty(ref PRESSUREValue, value); }
        }

        private decimal? TEMPERATUREValue;
        public decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        private decimal? FORMATION_VOLUME_FACTORValue;
        public decimal? FORMATION_VOLUME_FACTOR
        {
            get { return this.FORMATION_VOLUME_FACTORValue; }
            set { SetProperty(ref FORMATION_VOLUME_FACTORValue, value); }
        }

        private decimal? DENSITYValue;
        public decimal? DENSITY
        {
            get { return this.DENSITYValue; }
            set { SetProperty(ref DENSITYValue, value); }
        }

        private decimal? VISCOSITYValue;
        public decimal? VISCOSITY
        {
            get { return this.VISCOSITYValue; }
            set { SetProperty(ref VISCOSITYValue, value); }
        }

        private decimal? COMPRESSIBILITYValue;
        public decimal? COMPRESSIBILITY
        {
            get { return this.COMPRESSIBILITYValue; }
            set { SetProperty(ref COMPRESSIBILITYValue, value); }
        }

        private DateTime? CALCULATION_DATEValue;
        public DateTime? CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
        }

        private string CORRELATION_METHODValue;
        public string CORRELATION_METHOD
        {
            get { return this.CORRELATION_METHODValue; }
            set { SetProperty(ref CORRELATION_METHODValue, value); }
        }

        // Standard PPDM columns
        private string ACTIVE_INDValue;
        public string ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private string PPDM_GUIDValue;
        public string PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private string REMARKValue;
        public string REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private string SOURCEValue;
        public string SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }
    }
}

