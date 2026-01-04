namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for PipelineAnalysis calculation results and properties
    /// </summary>
    public class PipelineAnalysisModuleData : FileSystemModuleData
    {
        public override string ModuleName => "PipelineAnalysis";
        public override string Description => "Pipeline analysis calculation results and properties";
        public override string ScriptBasePath => "PipelineAnalysis";
        public override int ExecutionOrder => 17;
        public override bool IsRequired => false;

        public PipelineAnalysisModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
