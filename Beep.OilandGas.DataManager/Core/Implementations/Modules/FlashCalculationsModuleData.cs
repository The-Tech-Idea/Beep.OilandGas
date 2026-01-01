namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Flash calculations for phase equilibrium
    /// </summary>
    public class FlashCalculationsModuleData : FileSystemModuleData
    {
        public override string ModuleName => "FlashCalculations";
        public override string Description => "Flash calculations for phase equilibrium";
        public override string ScriptBasePath => "FlashCalculations";
        public override int ExecutionOrder => 22;
        public override bool IsRequired => false;

        public FlashCalculationsModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
