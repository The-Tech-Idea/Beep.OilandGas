using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
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
}
