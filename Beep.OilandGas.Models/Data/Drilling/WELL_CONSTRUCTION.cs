using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class WELL_CONSTRUCTION : ModelEntityBase
    {
        private String WELL_CONSTRUCTION_IDValue;
        public String WELL_CONSTRUCTION_ID
        {
            get { return this.WELL_CONSTRUCTION_IDValue; }
            set { SetProperty(ref WELL_CONSTRUCTION_IDValue, value); }
        }

        private String WELL_UWIValue;
        public String WELL_UWI
        {
            get { return this.WELL_UWIValue; }
            set { SetProperty(ref WELL_UWIValue, value); }
        }

        private String STATUSValue;
        public String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private DateTime? START_DATEValue;
        public DateTime? START_DATE
        {
            get { return this.START_DATEValue; }
            set { SetProperty(ref START_DATEValue, value); }
        }

        private DateTime? COMPLETION_DATEValue;
        public DateTime? COMPLETION_DATE
        {
            get { return this.COMPLETION_DATEValue; }
            set { SetProperty(ref COMPLETION_DATEValue, value); }
        }

        // Standard PPDM columns

        public WELL_CONSTRUCTION() { }

        private string CONSTRUCTION_IDValue;
        public string CONSTRUCTION_ID
        {
            get { return this.CONSTRUCTION_IDValue; }
            set { SetProperty(ref CONSTRUCTION_IDValue, value); }
        }

        private string? REMARKSValue;
        public string? REMARKS
        {
            get { return this.REMARKSValue; }
            set { SetProperty(ref REMARKSValue, value); }
        }
    }
}
