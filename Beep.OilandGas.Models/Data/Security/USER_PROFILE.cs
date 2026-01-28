using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class USER_PROFILE : ModelEntityBase
    {
        private String USER_IDValue;
        public String USER_ID
        {
            get { return this.USER_IDValue; }
            set { SetProperty(ref USER_IDValue, value); }
        }

        private String PRIMARY_ROLEValue;
        public String PRIMARY_ROLE
        {
            get { return this.PRIMARY_ROLEValue; }
            set { SetProperty(ref PRIMARY_ROLEValue, value); }
        }

        private String PREFERRED_LAYOUTValue;
        public String PREFERRED_LAYOUT
        {
            get { return this.PREFERRED_LAYOUTValue; }
            set { SetProperty(ref PREFERRED_LAYOUTValue, value); }
        }

        private String USER_PREFERENCESValue;
        public String USER_PREFERENCES
        {
            get { return this.USER_PREFERENCESValue; }
            set { SetProperty(ref USER_PREFERENCESValue, value); }
        }

        private DateTime? LAST_LOGIN_DATEValue;
        public DateTime? LAST_LOGIN_DATE
        {
            get { return this.LAST_LOGIN_DATEValue; }
            set { SetProperty(ref LAST_LOGIN_DATEValue, value); }
        }

        // Standard PPDM columns

    }
}
