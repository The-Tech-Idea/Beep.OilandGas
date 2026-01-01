namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Production operations and cost tracking
    /// </summary>
    public class ProductionOperationsModuleData : FileSystemModuleData
    {
        public override string ModuleName => "ProductionOperations";
        public override string Description => "Production operations and cost tracking";
        public override string ScriptBasePath => "ProductionOperations";
        public override int ExecutionOrder => 11;
        public override bool IsRequired => true;

        public ProductionOperationsModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
