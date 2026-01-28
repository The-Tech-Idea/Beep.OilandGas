using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DriverInfo : ModelEntityBase
    {
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
        private string? NuGetPackageValue;

        public string? NuGetPackage

        {

            get { return this.NuGetPackageValue; }

            set { SetProperty(ref NuGetPackageValue, value); }

        }
        private bool IsAvailableValue;

        public bool IsAvailable

        {

            get { return this.IsAvailableValue; }

            set { SetProperty(ref IsAvailableValue, value); }

        }
        private bool IsInstalledValue;

        public bool IsInstalled

        {

            get { return this.IsInstalledValue; }

            set { SetProperty(ref IsInstalledValue, value); }

        }
        private string StatusMessageValue = string.Empty;

        public string StatusMessage

        {

            get { return this.StatusMessageValue; }

            set { SetProperty(ref StatusMessageValue, value); }

        }
    }
}
