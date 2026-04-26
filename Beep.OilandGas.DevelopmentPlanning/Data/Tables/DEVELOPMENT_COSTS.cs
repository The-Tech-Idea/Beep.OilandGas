using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    public partial class DEVELOPMENT_COSTS : ModelEntityBase
    {
        private System.String? DEVELOPMENT_COST_IDValue;
        public System.String DEVELOPMENT_COST_ID
        {
            get { return this.DEVELOPMENT_COST_IDValue ?? string.Empty; }
            set { SetProperty(ref DEVELOPMENT_COST_IDValue, value); }
        }

        private System.String? FIELD_IDValue;
        /// <summary>FK to PPDM FIELD (project/field context).</summary>
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue ?? string.Empty; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String? FDP_IDValue;
        /// <summary>FK to FIELD_DEVELOPMENT_PLAN — ties cost to an approved FDP.</summary>
        public System.String FDP_ID
        {
            get { return this.FDP_IDValue ?? string.Empty; }
            set { SetProperty(ref FDP_IDValue, value); }
        }

        private System.String? PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue ?? string.Empty; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.Int32 COST_YEARValue;
        /// <summary>Fiscal year this cost row applies to.</summary>
        public System.Int32 COST_YEAR
        {
            get { return this.COST_YEARValue; }
            set { SetProperty(ref COST_YEARValue, value); }
        }

        private System.String? COST_CATEGORYValue;
        /// <summary>CAPEX / OPEX / ABEX — high-level cost category.</summary>
        public System.String COST_CATEGORY
        {
            get { return this.COST_CATEGORYValue ?? string.Empty; }
            set { SetProperty(ref COST_CATEGORYValue, value); }
        }

        private System.String? COST_TYPEValue;
        /// <summary>DRILLING / FACILITIES / G_AND_A / WORKOVER / SUBSEA / PIPELINE / OPEX_LOE.</summary>
        public System.String COST_TYPE
        {
            get { return this.COST_TYPEValue ?? string.Empty; }
            set { SetProperty(ref COST_TYPEValue, value); }
        }

        private System.String? COST_CURRENCYValue;
        public System.String COST_CURRENCY
        {
            get { return this.COST_CURRENCYValue ?? string.Empty; }
            set { SetProperty(ref COST_CURRENCYValue, value); }
        }

        private System.Decimal? DEVELOPMENT_WELL_DRILLING_COSTSValue;
        public System.Decimal? DEVELOPMENT_WELL_DRILLING_COSTS
        {
            get { return this.DEVELOPMENT_WELL_DRILLING_COSTSValue; }
            set { SetProperty(ref DEVELOPMENT_WELL_DRILLING_COSTSValue, value); }
        }

        private System.Decimal? DEVELOPMENT_WELL_EQUIPMENTValue;
        public System.Decimal? DEVELOPMENT_WELL_EQUIPMENT
        {
            get { return this.DEVELOPMENT_WELL_EQUIPMENTValue; }
            set { SetProperty(ref DEVELOPMENT_WELL_EQUIPMENTValue, value); }
        }

        private System.Decimal? SUPPORT_EQUIPMENT_AND_FACILITIESValue;
        public System.Decimal? SUPPORT_EQUIPMENT_AND_FACILITIES
        {
            get { return this.SUPPORT_EQUIPMENT_AND_FACILITIESValue; }
            set { SetProperty(ref SUPPORT_EQUIPMENT_AND_FACILITIESValue, value); }
        }

        private System.Decimal? SERVICE_WELL_COSTSValue;
        public System.Decimal? SERVICE_WELL_COSTS
        {
            get { return this.SERVICE_WELL_COSTSValue; }
            set { SetProperty(ref SERVICE_WELL_COSTSValue, value); }
        }

        private System.Decimal? ABANDONMENT_PROVISION_MMValue;
        /// <summary>Estimated decommissioning provision (ABEX) per SPE PRMS Section 6.</summary>
        public System.Decimal? ABANDONMENT_PROVISION_MM
        {
            get { return this.ABANDONMENT_PROVISION_MMValue; }
            set { SetProperty(ref ABANDONMENT_PROVISION_MMValue, value); }
        }

        private System.Decimal? TOTAL_DEVELOPMENT_COSTSValue;
        public System.Decimal? TOTAL_DEVELOPMENT_COSTS
        {
            get { return this.TOTAL_DEVELOPMENT_COSTSValue; }
            set { SetProperty(ref TOTAL_DEVELOPMENT_COSTSValue, value); }
        }

        private System.DateTime? COST_DATEValue;
        public System.DateTime? COST_DATE
        {
            get { return this.COST_DATEValue; }
            set { SetProperty(ref COST_DATEValue, value); }
        }

        private System.String? COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue ?? string.Empty; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
        }

        // Standard PPDM columns

        private System.String? REMARKValue;

        private System.String? SOURCEValue;

        private System.String? ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue ?? string.Empty; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
