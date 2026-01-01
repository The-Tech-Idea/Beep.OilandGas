namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Economic analysis and cash flow modeling
    /// </summary>
    public class EconomicAnalysisModuleData : FileSystemModuleData
    {
        public override string ModuleName => "EconomicAnalysis";
        public override string Description => "Economic analysis and cash flow modeling";
        public override string ScriptBasePath => "EconomicAnalysis";
        public override int ExecutionOrder => 20;
        public override bool IsRequired => false;

        public EconomicAnalysisModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
