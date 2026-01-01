namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Pricing and price index management
    /// </summary>
    public class PricingModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Pricing";
        public override string Description => "Pricing and price index management";
        public override string ScriptBasePath => "Pricing";
        public override int ExecutionOrder => 32;
        public override bool IsRequired => false;

        public PricingModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
