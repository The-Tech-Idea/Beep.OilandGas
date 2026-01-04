using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public partial class ROD_SECTION : Entity, IPPDMEntity
    {
        private String ROD_SECTION_IDValue;
        public String ROD_SECTION_ID
        {
            get { return this.ROD_SECTION_IDValue; }
            set { SetProperty(ref ROD_SECTION_IDValue, value); }
        }

        private String SUCKER_ROD_STRING_IDValue;
        public String SUCKER_ROD_STRING_ID
        {
            get { return this.SUCKER_ROD_STRING_IDValue; }
            set { SetProperty(ref SUCKER_ROD_STRING_IDValue, value); }
        }

        private Decimal? DIAMETERValue;
        public Decimal? DIAMETER
        {
            get { return this.DIAMETERValue; }
            set { SetProperty(ref DIAMETERValue, value); }
        }

        private Decimal? LENGTHValue;
        public Decimal? LENGTH
        {
            get { return this.LENGTHValue; }
            set { SetProperty(ref LENGTHValue, value); }
        }

        private Decimal? DENSITYValue;
        public Decimal? DENSITY
        {
            get { return this.DENSITYValue; }
            set { SetProperty(ref DENSITYValue, value); }
        }

        private Decimal? WEIGHTValue;
        public Decimal? WEIGHT
        {
            get { return this.WEIGHTValue; }
            set { SetProperty(ref WEIGHTValue, value); }
        }

        private Int32? SECTION_ORDERValue;
        public Int32? SECTION_ORDER
        {
            get { return this.SECTION_ORDERValue; }
            set { SetProperty(ref SECTION_ORDERValue, value); }
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
    }
}
