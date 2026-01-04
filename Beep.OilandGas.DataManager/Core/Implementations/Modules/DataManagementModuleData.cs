namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for DataManagement entities (audit, quality, validation, versioning)
    /// </summary>
    public class DataManagementModuleData : FileSystemModuleData
    {
        public override string ModuleName => "DataManagement";
        public override string Description => "Data management entities (audit, quality, validation, versioning)";
        public override string ScriptBasePath => "DataManagement";
        public override int ExecutionOrder => 5; // After Common and Security, before business modules
        public override bool IsRequired => true;

        public DataManagementModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => new[] { "PPDM39", "Common", "Security" };
    }
}
