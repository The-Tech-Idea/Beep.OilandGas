namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for General accounting and financial transactions
    /// </summary>
    public class AccountingModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Accounting";
        public override string Description => "General accounting and financial transactions";
        public override string ScriptBasePath => "Accounting";
        public override int ExecutionOrder => 30;
        public override bool IsRequired => false;

        public AccountingModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
