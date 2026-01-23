using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class COPAS_OVERHEAD_SCHEDULE : ModelEntityBase {
        private string COPAS_OVERHEAD_SCHEDULE_IDValue;
        public string COPAS_OVERHEAD_SCHEDULE_ID
        {
            get { return this.COPAS_OVERHEAD_SCHEDULE_IDValue; }
            set { SetProperty(ref COPAS_OVERHEAD_SCHEDULE_IDValue, value); }
        }

        private string LEASE_IDValue;
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private string COST_CATEGORYValue;
        public string COST_CATEGORY
        {
            get { return this.COST_CATEGORYValue; }
            set { SetProperty(ref COST_CATEGORYValue, value); }
        }

        private decimal? OVERHEAD_RATEValue;
        public decimal? OVERHEAD_RATE
        {
            get { return this.OVERHEAD_RATEValue; }
            set { SetProperty(ref OVERHEAD_RATEValue, value); }
        }

        private string APPROVED_BYValue;
        public string APPROVED_BY
        {
            get { return this.APPROVED_BYValue; }
            set { SetProperty(ref APPROVED_BYValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


