namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Export configuration
    /// </summary>
    public class ExportModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Export";
        public override string Description => "Export configuration";
        public override string ScriptBasePath => "Export";
        public override int ExecutionOrder => 83;
        public override bool IsRequired => false;

        public ExportModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
