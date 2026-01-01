namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Exchange trading and transactions
    /// </summary>
    public class TradingModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Trading";
        public override string Description => "Exchange trading and transactions";
        public override string ScriptBasePath => "Trading";
        public override int ExecutionOrder => 40;
        public override bool IsRequired => false;

        public TradingModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
