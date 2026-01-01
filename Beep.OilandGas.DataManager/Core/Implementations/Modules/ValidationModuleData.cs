namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Data validation results
    /// </summary>
    public class ValidationModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Validation";
        public override string Description => "Data validation results";
        public override string ScriptBasePath => "Validation";
        public override int ExecutionOrder => 90;
        public override bool IsRequired => false;

        public ValidationModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
