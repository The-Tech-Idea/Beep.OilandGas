namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Calculation results
    /// </summary>
    public class CalculationsModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Calculations";
        public override string Description => "Calculation results";
        public override string ScriptBasePath => "Calculations";
        public override int ExecutionOrder => 81;
        public override bool IsRequired => false;

        public CalculationsModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
