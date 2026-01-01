namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Decommissioning and abandonment
    /// </summary>
    public class DecommissioningModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Decommissioning";
        public override string Description => "Decommissioning and abandonment";
        public override string ScriptBasePath => "Decommissioning";
        public override int ExecutionOrder => 62;
        public override bool IsRequired => false;

        public DecommissioningModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
