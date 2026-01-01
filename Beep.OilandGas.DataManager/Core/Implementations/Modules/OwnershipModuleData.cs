namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Ownership interests and division orders
    /// </summary>
    public class OwnershipModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Ownership";
        public override string Description => "Ownership interests and division orders";
        public override string ScriptBasePath => "Ownership";
        public override int ExecutionOrder => 50;
        public override bool IsRequired => false;

        public OwnershipModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
