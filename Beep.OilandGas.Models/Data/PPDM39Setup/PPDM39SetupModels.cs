using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// Database driver information.
    /// </summary>
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

    /// <summary>
    /// Driver availability information.
    /// </summary>
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

    /// <summary>
    /// Schema privilege check result.
    /// </summary>
    public class SchemaPrivilegeCheckResult : ModelEntityBase
    {
        private bool HasCreatePrivilegeValue;

        public bool HasCreatePrivilege

        {

            get { return this.HasCreatePrivilegeValue; }

            set { SetProperty(ref HasCreatePrivilegeValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
        private List<string> ExistingSchemasValue = new List<string>();

        public List<string> ExistingSchemas

        {

            get { return this.ExistingSchemasValue; }

            set { SetProperty(ref ExistingSchemasValue, value); }

        }
    }

    /// <summary>
    /// Create schema result.
    /// </summary>
    public class CreateSchemaResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
        private string? SchemaNameValue;

        public string? SchemaName

        {

            get { return this.SchemaNameValue; }

            set { SetProperty(ref SchemaNameValue, value); }

        }
    }

    /// <summary>
    /// Database connection list item.
    /// </summary>
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

    /// <summary>
    /// Delete connection result.
    /// </summary>
    public class DeleteConnectionResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private string? ErrorDetailsValue;

        public string? ErrorDetails

        {

            get { return this.ErrorDetailsValue; }

            set { SetProperty(ref ErrorDetailsValue, value); }

        }
    }
}


