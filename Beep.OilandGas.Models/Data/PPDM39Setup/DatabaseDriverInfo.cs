using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DatabaseDriverInfo : ModelEntityBase
    {
        private string NuGetPackageValue = string.Empty;

        public string NuGetPackage

        {

            get { return this.NuGetPackageValue; }

            set { SetProperty(ref NuGetPackageValue, value); }

        }
        private string DataSourceTypeValue = string.Empty;

        public string DataSourceType

        {

            get { return this.DataSourceTypeValue; }

            set { SetProperty(ref DataSourceTypeValue, value); }

        }
        private int DefaultPortValue;

        public int DefaultPort

        {

            get { return this.DefaultPortValue; }

            set { SetProperty(ref DefaultPortValue, value); }

        }
        private string ScriptPathValue = string.Empty;

        public string ScriptPath

        {

            get { return this.ScriptPathValue; }

            set { SetProperty(ref ScriptPathValue, value); }

        }
        private string DisplayNameValue = string.Empty;

        public string DisplayName

        {

            get { return this.DisplayNameValue; }

            set { SetProperty(ref DisplayNameValue, value); }

        }
    }
}
