using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DatabaseConnectionListItem : ModelEntityBase
    {
        private string ConnectionNameValue = string.Empty;

        public string ConnectionName

        {

            get { return this.ConnectionNameValue; }

            set { SetProperty(ref ConnectionNameValue, value); }

        }
        private string DatabaseTypeValue = string.Empty;

        public string DatabaseType

        {

            get { return this.DatabaseTypeValue; }

            set { SetProperty(ref DatabaseTypeValue, value); }

        }
        private string HostValue = string.Empty;

        public string Host

        {

            get { return this.HostValue; }

            set { SetProperty(ref HostValue, value); }

        }
        private int PortValue;

        public int Port

        {

            get { return this.PortValue; }

            set { SetProperty(ref PortValue, value); }

        }
        private string DatabaseValue = string.Empty;

        public string Database

        {

            get { return this.DatabaseValue; }

            set { SetProperty(ref DatabaseValue, value); }

        }
        private string? UsernameValue;

        public string? Username

        {

            get { return this.UsernameValue; }

            set { SetProperty(ref UsernameValue, value); }

        }
        private bool IsCurrentValue;

        public bool IsCurrent

        {

            get { return this.IsCurrentValue; }

            set { SetProperty(ref IsCurrentValue, value); }

        }
        private string GuidIdValue = string.Empty;

        public string GuidId

        {

            get { return this.GuidIdValue; }

            set { SetProperty(ref GuidIdValue, value); }

        }
    }
}
