namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for ChokeAnalysis calculation results and properties
    /// </summary>
    public class ChokeAnalysisModuleData : FileSystemModuleData
    {
        public override string ModuleName => "ChokeAnalysis";
        public override string Description => "Choke analysis calculation results and properties";
        public override string ScriptBasePath => "ChokeAnalysis";
        public override int ExecutionOrder => 15; // After PPDM39, Common, Security
        public override bool IsRequired => false; // Computational modules are optional

        public ChokeAnalysisModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
