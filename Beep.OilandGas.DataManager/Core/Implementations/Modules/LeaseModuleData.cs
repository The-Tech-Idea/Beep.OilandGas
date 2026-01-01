namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Lease management
    /// </summary>
    public class LeaseModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Lease";
        public override string Description => "Lease management";
        public override string ScriptBasePath => "Lease";
        public override int ExecutionOrder => 52;
        public override bool IsRequired => false;

        public LeaseModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
