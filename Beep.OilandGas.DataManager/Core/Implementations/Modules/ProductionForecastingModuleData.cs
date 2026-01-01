namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Production forecasting and projections
    /// </summary>
    public class ProductionForecastingModuleData : FileSystemModuleData
    {
        public override string ModuleName => "ProductionForecasting";
        public override string Description => "Production forecasting and projections";
        public override string ScriptBasePath => "ProductionForecasting";
        public override int ExecutionOrder => 12;
        public override bool IsRequired => false;

        public ProductionForecastingModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
