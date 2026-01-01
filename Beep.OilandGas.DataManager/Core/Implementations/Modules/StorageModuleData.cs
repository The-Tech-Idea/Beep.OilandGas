namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Storage facility management
    /// </summary>
    public class StorageModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Storage";
        public override string Description => "Storage facility management";
        public override string ScriptBasePath => "Storage";
        public override int ExecutionOrder => 42;
        public override bool IsRequired => false;

        public StorageModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
