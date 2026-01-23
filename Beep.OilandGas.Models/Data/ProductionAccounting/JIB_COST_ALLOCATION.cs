using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JIB_COST_ALLOCATION : ModelEntityBase {
        private String JIB_COST_ALLOCATION_IDValue;
        public String JIB_COST_ALLOCATION_ID
        {
            get { return this.JIB_COST_ALLOCATION_IDValue; }
            set { SetProperty(ref JIB_COST_ALLOCATION_IDValue, value); }
        }

        private String JIB_IDValue;
        public String JIB_ID
        {
            get { return this.JIB_IDValue; }
            set { SetProperty(ref JIB_IDValue, value); }
        }
        private String FINANCE_IDValue;
        public String FINANCE_ID
        {
            get { return this.FINANCE_IDValue; }
            set { SetProperty(ref FINANCE_IDValue, value); }
        }

        private String COST_CATEGORYValue;
        public String COST_CATEGORY
        {
            get { return this.COST_CATEGORYValue; }
            set { SetProperty(ref COST_CATEGORYValue, value); }
        }

        private Decimal GROSS_COSTValue;
        public Decimal GROSS_COST
        {
            get { return this.GROSS_COSTValue; }
            set { SetProperty(ref GROSS_COSTValue, value); }
        }

        private Decimal? WORKING_INTEREST_SHAREValue;
        public Decimal? WORKING_INTEREST_SHARE
        {
            get { return this.WORKING_INTEREST_SHAREValue; }
            set { SetProperty(ref WORKING_INTEREST_SHAREValue, value); }
        }

        private Decimal NET_COSTValue;
        public Decimal NET_COST
        {
            get { return this.NET_COSTValue; }
            set { SetProperty(ref NET_COSTValue, value); }
        }

        private String DESCRIPTIONValue;
        public String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        private String ROW_IDValue;
        public String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


