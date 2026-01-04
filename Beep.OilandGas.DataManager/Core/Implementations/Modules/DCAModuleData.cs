namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for DCA (Decline Curve Analysis) calculation results
    /// </summary>
    public class DCAModuleData : FileSystemModuleData
    {
        public override string ModuleName => "DCA";
        public override string Description => "Decline curve analysis calculation results";
        public override string ScriptBasePath => "DCA";
        public override int ExecutionOrder => 24;
        public override bool IsRequired => false;

        public DCAModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
