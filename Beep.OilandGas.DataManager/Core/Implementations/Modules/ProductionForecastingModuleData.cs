namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for ProductionForecasting calculation results and properties
    /// </summary>
    public class ProductionForecastingModuleData : FileSystemModuleData
    {
        public override string ModuleName => "ProductionForecasting";
        public override string Description => "Production forecasting calculation results and properties";
        public override string ScriptBasePath => "ProductionForecasting";
        public override int ExecutionOrder => 20;
        public override bool IsRequired => false;

        public ProductionForecastingModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
