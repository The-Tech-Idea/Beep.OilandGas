using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public partial class LEAD : ModelEntityBase
    {
        private System.String LEAD_IDValue;
        public System.String LEAD_ID
        {
            get
            {
                return this.LEAD_IDValue;
            }
            set { SetProperty(ref LEAD_IDValue, value); }
        }
        private System.String LEAD_NAMEValue;
        public System.String LEAD_NAME
        {
            get
            {
                return this.LEAD_NAMEValue;
            }
            set { SetProperty(ref LEAD_NAMEValue, value); }
        }
        private System.String LEAD_SHORT_NAMEValue;
        public System.String LEAD_SHORT_NAME
        {
            get
            {
                return this.LEAD_SHORT_NAMEValue;
            }
            set { SetProperty(ref LEAD_SHORT_NAMEValue, value); }
        }
        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get
            {
                return this.FIELD_IDValue;
            }
            set { SetProperty(ref FIELD_IDValue, value); }
        }
        private System.String PLAY_IDValue;
        public System.String PLAY_ID
        {
            get
            {
                return this.PLAY_IDValue;
            }
            set { SetProperty(ref PLAY_IDValue, value); }
        }
        private System.String LEAD_STATUSValue;
        public System.String LEAD_STATUS
        {
            get
            {
                return this.LEAD_STATUSValue;
            }
            set { SetProperty(ref LEAD_STATUSValue, value); }
        }
        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get
            {
                return this.DESCRIPTIONValue;
            }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }
        private System.String LEAD_SOURCEValue;
        public System.String LEAD_SOURCE
        {
            get
            {
                return this.LEAD_SOURCEValue;
            }
            set { SetProperty(ref LEAD_SOURCEValue, value); }
        }
        private System.DateTime? INITIAL_ASSESSMENT_DATEValue;
        public System.DateTime? INITIAL_ASSESSMENT_DATE
        {
            get
            {
                return this.INITIAL_ASSESSMENT_DATEValue;
            }
            set { SetProperty(ref INITIAL_ASSESSMENT_DATEValue, value); }
        }
        private System.DateTime? PROMOTED_TO_PROSPECT_DATEValue;
        public System.DateTime? PROMOTED_TO_PROSPECT_DATE
        {
            get
            {
                return this.PROMOTED_TO_PROSPECT_DATEValue;
            }
            set { SetProperty(ref PROMOTED_TO_PROSPECT_DATEValue, value); }
        }
        private System.DateTime? REJECTED_DATEValue;
        public System.DateTime? REJECTED_DATE
        {
            get
            {
                return this.REJECTED_DATEValue;
            }
            set { SetProperty(ref REJECTED_DATEValue, value); }
        }
        private System.String REJECTION_REASONValue;
        public System.String REJECTION_REASON
        {
            get
            {
                return this.REJECTION_REASONValue;
            }
            set { SetProperty(ref REJECTION_REASONValue, value); }
        }
        private System.Decimal  LATITUDEValue;
        public System.Decimal  LATITUDE
        {
            get
            {
                return this.LATITUDEValue;
            }
            set { SetProperty(ref LATITUDEValue, value); }
        }
        private System.Decimal  LONGITUDEValue;
        public System.Decimal  LONGITUDE
        {
            get
            {
                return this.LONGITUDEValue;
            }
            set { SetProperty(ref LONGITUDEValue, value); }
        }
        private System.Decimal  ELEVATIONValue;
        public System.Decimal  ELEVATION
        {
            get
            {
                return this.ELEVATIONValue;
            }
            set { SetProperty(ref ELEVATIONValue, value); }
        }
        private System.String ELEVATION_OUOMValue;
        public System.String ELEVATION_OUOM
        {
            get
            {
                return this.ELEVATION_OUOMValue;
            }
            set { SetProperty(ref ELEVATION_OUOMValue, value); }
        }
        private System.String LOCATION_DESCRIPTIONValue;
        public System.String LOCATION_DESCRIPTION
        {
            get
            {
                return this.LOCATION_DESCRIPTIONValue;
            }
            set { SetProperty(ref LOCATION_DESCRIPTIONValue, value); }
        }
        private System.String INITIAL_RISK_ASSESSMENTValue;
        public System.String INITIAL_RISK_ASSESSMENT
        {
            get
            {
                return this.INITIAL_RISK_ASSESSMENTValue;
            }
            set { SetProperty(ref INITIAL_RISK_ASSESSMENTValue, value); }
        }
        private System.Decimal  INITIAL_VOLUME_ESTIMATE_OILValue;
        public System.Decimal  INITIAL_VOLUME_ESTIMATE_OIL
        {
            get
            {
                return this.INITIAL_VOLUME_ESTIMATE_OILValue;
            }
            set { SetProperty(ref INITIAL_VOLUME_ESTIMATE_OILValue, value); }
        }
        private System.Decimal  INITIAL_VOLUME_ESTIMATE_GASValue;
        public System.Decimal  INITIAL_VOLUME_ESTIMATE_GAS
        {
            get
            {
                return this.INITIAL_VOLUME_ESTIMATE_GASValue;
            }
            set { SetProperty(ref INITIAL_VOLUME_ESTIMATE_GASValue, value); }
        }
        private System.String INITIAL_VOLUME_ESTIMATE_OUOMValue;
        public System.String INITIAL_VOLUME_ESTIMATE_OUOM
        {
            get
            {
                return this.INITIAL_VOLUME_ESTIMATE_OUOMValue;
            }
            set { SetProperty(ref INITIAL_VOLUME_ESTIMATE_OUOMValue, value); }
        }
        private System.String PROMOTED_TO_PROSPECT_IDValue;
        public System.String PROMOTED_TO_PROSPECT_ID
        {
            get
            {
                return this.PROMOTED_TO_PROSPECT_IDValue;
            }
            set { SetProperty(ref PROMOTED_TO_PROSPECT_IDValue, value); }
        }

        // --- O&G best-practice additions ---

        private System.String LEAD_TYPEValue;
        /// <summary>Origin type: SEISMIC / GRAVITY / GEOCHEMICAL / SURFACE / REGIONAL.</summary>
        public System.String LEAD_TYPE
        {
            get { return this.LEAD_TYPEValue; }
            set { SetProperty(ref LEAD_TYPEValue, value); }
        }

        private System.Decimal? ESTIMATED_AREAValue;
        /// <summary>Estimated areal extent of the lead.</summary>
        public System.Decimal? ESTIMATED_AREA
        {
            get { return this.ESTIMATED_AREAValue; }
            set { SetProperty(ref ESTIMATED_AREAValue, value); }
        }

        private System.String ESTIMATED_AREA_OUOMValue;
        public System.String ESTIMATED_AREA_OUOM
        {
            get { return this.ESTIMATED_AREA_OUOMValue; }
            set { SetProperty(ref ESTIMATED_AREA_OUOMValue, value); }
        }

        private System.String GEOLOGIST_IDValue;
        /// <summary>User ID of the geologist who identified the lead.</summary>
        public System.String GEOLOGIST_ID
        {
            get { return this.GEOLOGIST_IDValue; }
            set { SetProperty(ref GEOLOGIST_IDValue, value); }
        }

        private System.String CONFIDENCE_LEVELValue;
        /// <summary>Confidence in lead: HIGH / MEDIUM / LOW.</summary>
        public System.String CONFIDENCE_LEVEL
        {
            get { return this.CONFIDENCE_LEVELValue; }
            set { SetProperty(ref CONFIDENCE_LEVELValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

        private System.String REMARKValue;

        private System.String SOURCEValue;

        public LEAD() { }
    }
}
