namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for Nodal analysis for well performance
    /// </summary>
    public class NodalAnalysisModuleData : FileSystemModuleData
    {
        public override string ModuleName => "NodalAnalysis";
        public override string Description => "Nodal analysis for well performance";
        public override string ScriptBasePath => "NodalAnalysis";
        public override int ExecutionOrder => 21;
        public override bool IsRequired => false;

        public NodalAnalysisModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
