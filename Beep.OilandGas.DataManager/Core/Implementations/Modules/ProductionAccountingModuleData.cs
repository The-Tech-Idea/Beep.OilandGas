namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Production accounting and financial management
    /// </summary>
    public class ProductionAccountingModuleData : FileSystemModuleData
    {
        public override string ModuleName => "ProductionAccounting";
        public override string Description => "Production accounting and financial management";
        public override string ScriptBasePath => "ProductionAccounting";
        public override int ExecutionOrder => 10;
        public override bool IsRequired => true;

        public ProductionAccountingModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
