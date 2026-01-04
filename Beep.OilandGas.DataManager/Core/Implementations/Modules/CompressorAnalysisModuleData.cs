namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for CompressorAnalysis calculation results and properties
    /// </summary>
    public class CompressorAnalysisModuleData : FileSystemModuleData
    {
        public override string ModuleName => "CompressorAnalysis";
        public override string Description => "Compressor analysis calculation results and properties";
        public override string ScriptBasePath => "CompressorAnalysis";
        public override int ExecutionOrder => 16;
        public override bool IsRequired => false;

        public CompressorAnalysisModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
