namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Prospect identification and exploration
    /// </summary>
    public class ProspectIdentificationModuleData : FileSystemModuleData
    {
        public override string ModuleName => "ProspectIdentification";
        public override string Description => "Prospect identification and exploration";
        public override string ScriptBasePath => "ProspectIdentification";
        public override int ExecutionOrder => 60;
        public override bool IsRequired => false;

        public ProspectIdentificationModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
