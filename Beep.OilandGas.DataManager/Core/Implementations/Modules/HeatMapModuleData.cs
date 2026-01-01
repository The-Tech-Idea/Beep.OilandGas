namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Heat map visualization data
    /// </summary>
    public class HeatMapModuleData : FileSystemModuleData
    {
        public override string ModuleName => "HeatMap";
        public override string Description => "Heat map visualization data";
        public override string ScriptBasePath => "HeatMap";
        public override int ExecutionOrder => 24;
        public override bool IsRequired => false;

        public HeatMapModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
