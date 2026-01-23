using System;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class OIL_PROPERTY_RESULT : ModelEntityBase
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

    }
}


