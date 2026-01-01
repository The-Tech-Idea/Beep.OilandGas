namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Chart and visualization configuration
    /// </summary>
    public class RenderingModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Rendering";
        public override string Description => "Chart and visualization configuration";
        public override string ScriptBasePath => "Rendering";
        public override int ExecutionOrder => 82;
        public override bool IsRequired => false;

        public RenderingModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
