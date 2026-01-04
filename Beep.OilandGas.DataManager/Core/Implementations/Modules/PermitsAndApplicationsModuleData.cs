namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for Permits and Applications management
    /// </summary>
    public class PermitsAndApplicationsModuleData : FileSystemModuleData
    {
        public override string ModuleName => "PermitsAndApplications";
        public override string Description => "Permits and applications management for regulatory compliance";
        public override string ScriptBasePath => "PermitsAndApplications";
        public override int ExecutionOrder => 30;
        public override bool IsRequired => false;

        public PermitsAndApplicationsModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security", "ProspectIdentification" };
    }
}
