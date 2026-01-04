namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for OilProperties calculation results and properties
    /// </summary>
    public class OilPropertiesModuleData : FileSystemModuleData
    {
        public override string ModuleName => "OilProperties";
        public override string Description => "Oil properties calculation results and properties";
        public override string ScriptBasePath => "OilProperties";
        public override int ExecutionOrder => 23;
        public override bool IsRequired => false;

        public OilPropertiesModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
