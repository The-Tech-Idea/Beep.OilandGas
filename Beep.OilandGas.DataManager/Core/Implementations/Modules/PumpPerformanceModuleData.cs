namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for PumpPerformance (ESP) calculation results and properties
    /// </summary>
    public class PumpPerformanceModuleData : FileSystemModuleData
    {
        public override string ModuleName => "PumpPerformance";
        public override string Description => "Pump performance (ESP) calculation results and properties";
        public override string ScriptBasePath => "PumpPerformance";
        public override int ExecutionOrder => 25;
        public override bool IsRequired => false;

        public PumpPerformanceModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
