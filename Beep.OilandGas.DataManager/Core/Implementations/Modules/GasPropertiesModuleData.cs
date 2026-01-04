namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for GasProperties calculation results and properties
    /// </summary>
    public class GasPropertiesModuleData : FileSystemModuleData
    {
        public override string ModuleName => "GasProperties";
        public override string Description => "Gas properties calculation results and properties";
        public override string ScriptBasePath => "GasProperties";
        public override int ExecutionOrder => 22;
        public override bool IsRequired => false;

        public GasPropertiesModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
