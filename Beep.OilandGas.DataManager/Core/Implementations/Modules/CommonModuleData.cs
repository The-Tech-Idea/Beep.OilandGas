namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Common/shared entities and reference data
    /// </summary>
    public class CommonModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Common";
        public override string Description => "Common/shared entities and reference data";
        public override string ScriptBasePath => "Common";
        public override int ExecutionOrder => 1;
        public override bool IsRequired => true;

        public CommonModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => new[] { "PPDM39" };
    }
}
