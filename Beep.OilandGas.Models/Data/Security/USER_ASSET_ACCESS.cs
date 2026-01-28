using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class USER_ASSET_ACCESS : ModelEntityBase
    {
        private String USER_IDValue;
        public String USER_ID
        {
            get { return this.USER_IDValue; }
            set { SetProperty(ref USER_IDValue, value); }
        }

        private String ASSET_TYPEValue;
        public String ASSET_TYPE
        {
            get { return this.ASSET_TYPEValue; }
            set { SetProperty(ref ASSET_TYPEValue, value); }
        }

        private String ASSET_IDValue;
        public String ASSET_ID
        {
            get { return this.ASSET_IDValue; }
            set { SetProperty(ref ASSET_IDValue, value); }
        }

        private String ACCESS_LEVELValue;
        public String ACCESS_LEVEL
        {
            get { return this.ACCESS_LEVELValue; }
            set { SetProperty(ref ACCESS_LEVELValue, value); }
        }

        private String INHERIT_INDValue;
        public String INHERIT_IND
        {
            get { return this.INHERIT_INDValue; }
            set { SetProperty(ref INHERIT_INDValue, value); }
        }

        private String ORGANIZATION_IDValue;
        public String ORGANIZATION_ID
        {
            get { return this.ORGANIZATION_IDValue; }
            set { SetProperty(ref ORGANIZATION_IDValue, value); }
        }

        // Standard PPDM columns

    }
}
