namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Royalty calculations and payments
    /// </summary>
    public class RoyaltyModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Royalty";
        public override string Description => "Royalty calculations and payments";
        public override string ScriptBasePath => "Royalty";
        public override int ExecutionOrder => 31;
        public override bool IsRequired => false;

        public RoyaltyModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
