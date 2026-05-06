using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class ORGANIZATION_HIERARCHY_CONFIG : ModelEntityBase
    {
        private String CONFIG_IDValue;
        public String CONFIG_ID
        {
            get { return this.CONFIG_IDValue; }
            set { SetProperty(ref CONFIG_IDValue, value); }
        }

        private String ORGANIZATION_IDValue;
        public String ORGANIZATION_ID
        {
            get { return this.ORGANIZATION_IDValue; }
            set { SetProperty(ref ORGANIZATION_IDValue, value); }
        }

        private String? PARENT_ORG_IDValue;
        public String? PARENT_ORG_ID
        {
            get { return this.PARENT_ORG_IDValue; }
            set { SetProperty(ref PARENT_ORG_IDValue, value); }
        }

        private Int32 HIERARCHY_LEVELValue;
        public Int32 HIERARCHY_LEVEL
        {
            get { return this.HIERARCHY_LEVELValue; }
            set { SetProperty(ref HIERARCHY_LEVELValue, value); }
        }

        private String? HIERARCHY_PATHValue;
        public String? HIERARCHY_PATH
        {
            get { return this.HIERARCHY_PATHValue; }
            set { SetProperty(ref HIERARCHY_PATHValue, value); }
        }

        private String LEVEL_NAMEValue;
        public String LEVEL_NAME
        {
            get { return this.LEVEL_NAMEValue; }
            set { SetProperty(ref LEVEL_NAMEValue, value); }
        }

        private String ASSET_TYPEValue;
        public String ASSET_TYPE
        {
            get { return this.ASSET_TYPEValue; }
            set { SetProperty(ref ASSET_TYPEValue, value); }
        }

        private Int32? PARENT_LEVELValue;
        public Int32? PARENT_LEVEL
        {
            get { return this.PARENT_LEVELValue; }
            set { SetProperty(ref PARENT_LEVELValue, value); }
        }
    }
}
