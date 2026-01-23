using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TAX_ADJUSTMENT : ModelEntityBase {
        private string TAX_ADJUSTMENT_IDValue;
        public string TAX_ADJUSTMENT_ID
        {
            get { return this.TAX_ADJUSTMENT_IDValue; }
            set { SetProperty(ref TAX_ADJUSTMENT_IDValue, value); }
        }

        private string PROPERTY_IDValue;
        public string PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private string ADJUSTMENT_TYPEValue;
        public string ADJUSTMENT_TYPE
        {
            get { return this.ADJUSTMENT_TYPEValue; }
            set { SetProperty(ref ADJUSTMENT_TYPEValue, value); }
        }

        private DateTime? PERIOD_END_DATEValue;
        public DateTime? PERIOD_END_DATE
        {
            get { return this.PERIOD_END_DATEValue; }
            set { SetProperty(ref PERIOD_END_DATEValue, value); }
        }

        private decimal? AMOUNTValue;
        public decimal? AMOUNT
        {
            get { return this.AMOUNTValue; }
            set { SetProperty(ref AMOUNTValue, value); }
        }

        private string NOTESValue;
        public string NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


