namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Analytics results
    /// </summary>
    public class AnalyticsModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Analytics";
        public override string Description => "Analytics results";
        public override string ScriptBasePath => "Analytics";
        public override int ExecutionOrder => 80;
        public override bool IsRequired => false;

        public AnalyticsModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
