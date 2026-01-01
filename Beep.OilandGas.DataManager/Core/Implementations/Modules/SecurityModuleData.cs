namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Security and user management entities
    /// </summary>
    public class SecurityModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Security";
        public override string Description => "Security and user management entities";
        public override string ScriptBasePath => "Security";
        public override int ExecutionOrder => 2;
        public override bool IsRequired => true;

        public SecurityModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => new[] { "PPDM39", "Common" };
    }
}
