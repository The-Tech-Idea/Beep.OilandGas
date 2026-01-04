namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for SuckerRodPumping calculation results and properties
    /// </summary>
    public class SuckerRodPumpingModuleData : FileSystemModuleData
    {
        public override string ModuleName => "SuckerRodPumping";
        public override string Description => "Sucker rod pumping calculation results and properties";
        public override string ScriptBasePath => "SuckerRodPumping";
        public override int ExecutionOrder => 19;
        public override bool IsRequired => false;

        public SuckerRodPumpingModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
