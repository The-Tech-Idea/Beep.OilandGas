namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Development planning and costs
    /// </summary>
    public class DevelopmentPlanningModuleData : FileSystemModuleData
    {
        public override string ModuleName => "DevelopmentPlanning";
        public override string Description => "Development planning and costs";
        public override string ScriptBasePath => "DevelopmentPlanning";
        public override int ExecutionOrder => 61;
        public override bool IsRequired => false;

        public DevelopmentPlanningModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
