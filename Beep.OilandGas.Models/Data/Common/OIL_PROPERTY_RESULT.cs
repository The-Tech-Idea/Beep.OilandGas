using System;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class OIL_PROPERTY_RESULT : ModelEntityBase
    {
        private string CALCULATION_IDValue;
        [Key, Required, MaxLength(128)]
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

        // ---- RES-02 black-oil screening (additive; the existing columns hold pressure, temperature, Bo,
        //      saturated viscosity, density, the correlation and the date) ----

        private decimal? API_GRAVITYValue;
        /// <summary>The oil's API gravity, one of the inputs a saved result must carry (RES-02 AC-5).</summary>
        public decimal? API_GRAVITY
        {
            get { return this.API_GRAVITYValue; }
            set { SetProperty(ref this.API_GRAVITYValue, value); }
        }

        private decimal? GAS_SPECIFIC_GRAVITYValue;
        public decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref this.GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private string GAS_GRAVITY_ASSUMED_INDValue = "N";
        /// <summary>'Y' only when the operator explicitly accepted the default gas gravity (AC-3).</summary>
        public string GAS_GRAVITY_ASSUMED_IND
        {
            get { return this.GAS_GRAVITY_ASSUMED_INDValue; }
            set { SetProperty(ref this.GAS_GRAVITY_ASSUMED_INDValue, value); }
        }

        private decimal? SOLUTION_GORValue;
        /// <summary>Solution gas-oil ratio at the recorded pressure, scf/STB.</summary>
        public decimal? SOLUTION_GOR
        {
            get { return this.SOLUTION_GORValue; }
            set { SetProperty(ref this.SOLUTION_GORValue, value); }
        }

        private decimal? BUBBLE_POINT_PRESSUREValue;
        public decimal? BUBBLE_POINT_PRESSURE
        {
            get { return this.BUBBLE_POINT_PRESSUREValue; }
            set { SetProperty(ref this.BUBBLE_POINT_PRESSUREValue, value); }
        }

        private decimal? DEAD_OIL_VISCOSITYValue;
        public decimal? DEAD_OIL_VISCOSITY
        {
            get { return this.DEAD_OIL_VISCOSITYValue; }
            set { SetProperty(ref this.DEAD_OIL_VISCOSITYValue, value); }
        }

        private string OUTSIDE_RANGE_INDValue = "N";
        /// <summary>'Y' when a result fell outside the correlation's stated range (AC-2).</summary>
        public string OUTSIDE_RANGE_IND
        {
            get { return this.OUTSIDE_RANGE_INDValue; }
            set { SetProperty(ref this.OUTSIDE_RANGE_INDValue, value); }
        }

        private string RANGE_NOTESValue;
        /// <summary>Which result was out of range, one per line, so the mark is never unexplained.</summary>
        public string RANGE_NOTES
        {
            get { return this.RANGE_NOTESValue; }
            set { SetProperty(ref this.RANGE_NOTESValue, value); }
        }

        // Standard PPDM columns

    }
}
