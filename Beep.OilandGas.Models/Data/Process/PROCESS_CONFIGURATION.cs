using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Process
{
    /// <summary>
    /// Represents strongly typed configuration for a process definition.
    /// Replaces Dictionary<string, object> Configuration.
    /// </summary>
    public class PROCESS_CONFIGURATION : ModelEntityBase
    {
        private string DefinitionIdValue = string.Empty;
        public string DefinitionId
        {
            get => DefinitionIdValue;
            set => SetProperty(ref DefinitionIdValue, value);
        }

        private string ConfigJsonValue = string.Empty;
        public string ConfigJson
        {
            get => ConfigJsonValue;
            set => SetProperty(ref ConfigJsonValue, value);
        }

        private string VersionValue = "1.0";
        public string Version
        {
            get => VersionValue;
            set => SetProperty(ref VersionValue, value);
        }
    }
}
