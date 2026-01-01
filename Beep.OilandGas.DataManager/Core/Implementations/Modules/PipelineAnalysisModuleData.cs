namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Pipeline analysis and management
    /// </summary>
    public class PipelineAnalysisModuleData : FileSystemModuleData
    {
        public override string ModuleName => "PipelineAnalysis";
        public override string Description => "Pipeline analysis and management";
        public override string ScriptBasePath => "PipelineAnalysis";
        public override int ExecutionOrder => 25;
        public override bool IsRequired => false;

        public PipelineAnalysisModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
