namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Unit agreements and participating areas
    /// </summary>
    public class UnitizationModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Unitization";
        public override string Description => "Unit agreements and participating areas";
        public override string ScriptBasePath => "Unitization";
        public override int ExecutionOrder => 51;
        public override bool IsRequired => false;

        public UnitizationModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
