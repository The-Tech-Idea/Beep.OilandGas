using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    public partial class RESERVOIR_FORECAST_PROPERTIES : Entity, Core.Interfaces.IPPDMEntity
    {
        private String RESERVOIR_FORECAST_PROPERTIES_IDValue;
        public String RESERVOIR_FORECAST_PROPERTIES_ID
        {
            get { return this.RESERVOIR_FORECAST_PROPERTIES_IDValue; }
            set { SetProperty(ref RESERVOIR_FORECAST_PROPERTIES_IDValue, value); }
        }

        private Decimal? INITIAL_PRESSUREValue;
        public Decimal? INITIAL_PRESSURE
        {
            get { return this.INITIAL_PRESSUREValue; }
            set { SetProperty(ref INITIAL_PRESSUREValue, value); }
        }

        private Decimal? PERMEABILITYValue;
        public Decimal? PERMEABILITY
        {
            get { return this.PERMEABILITYValue; }
            set { SetProperty(ref PERMEABILITYValue, value); }
        }

        private Decimal? THICKNESSValue;
        public Decimal? THICKNESS
        {
            get { return this.THICKNESSValue; }
            set { SetProperty(ref THICKNESSValue, value); }
        }

        private Decimal? DRAINAGE_RADIUSValue;
        public Decimal? DRAINAGE_RADIUS
        {
            get { return this.DRAINAGE_RADIUSValue; }
            set { SetProperty(ref DRAINAGE_RADIUSValue, value); }
        }

        private Decimal? WELLBORE_RADIUSValue;
        public Decimal? WELLBORE_RADIUS
        {
            get { return this.WELLBORE_RADIUSValue; }
            set { SetProperty(ref WELLBORE_RADIUSValue, value); }
        }

        private Decimal? FORMATION_VOLUME_FACTORValue;
        public Decimal? FORMATION_VOLUME_FACTOR
        {
            get { return this.FORMATION_VOLUME_FACTORValue; }
            set { SetProperty(ref FORMATION_VOLUME_FACTORValue, value); }
        }

        private Decimal? OIL_VISCOSITYValue;
        public Decimal? OIL_VISCOSITY
        {
            get { return this.OIL_VISCOSITYValue; }
            set { SetProperty(ref OIL_VISCOSITYValue, value); }
        }

        private Decimal? TOTAL_COMPRESSIBILITYValue;
        public Decimal? TOTAL_COMPRESSIBILITY
        {
            get { return this.TOTAL_COMPRESSIBILITYValue; }
            set { SetProperty(ref TOTAL_COMPRESSIBILITYValue, value); }
        }

        private Decimal? POROSITYValue;
        public Decimal? POROSITY
        {
            get { return this.POROSITYValue; }
            set { SetProperty(ref POROSITYValue, value); }
        }

        private Decimal? SKIN_FACTORValue;
        public Decimal? SKIN_FACTOR
        {
            get { return this.SKIN_FACTORValue; }
            set { SetProperty(ref SKIN_FACTORValue, value); }
        }

        private Decimal? GAS_SPECIFIC_GRAVITYValue;
        public Decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal? TEMPERATUREValue;
        public Decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        // Standard PPDM columns
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
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

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
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

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
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

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
    }
}
