namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for WellTestAnalysis calculation results and properties
    /// </summary>
    public class WellTestAnalysisModuleData : FileSystemModuleData
    {
        public override string ModuleName => "WellTestAnalysis";
        public override string Description => "Well test analysis calculation results and properties";
        public override string ScriptBasePath => "WellTestAnalysis";
        public override int ExecutionOrder => 21;
        public override bool IsRequired => false;

        public WellTestAnalysisModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
