namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for HydraulicPumps calculation results and properties
    /// </summary>
    public class HydraulicPumpsModuleData : FileSystemModuleData
    {
        public override string ModuleName => "HydraulicPumps";
        public override string Description => "Hydraulic pumps calculation results and properties";
        public override string ScriptBasePath => "HydraulicPumps";
        public override int ExecutionOrder => 20;
        public override bool IsRequired => false;

        public HydraulicPumpsModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
