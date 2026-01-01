namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Field lifecycle phases
    /// </summary>
    public class LifeCycleModuleData : FileSystemModuleData
    {
        public override string ModuleName => "LifeCycle";
        public override string Description => "Field lifecycle phases";
        public override string ScriptBasePath => "LifeCycle";
        public override int ExecutionOrder => 84;
        public override bool IsRequired => false;

        public LifeCycleModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
