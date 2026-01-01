namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Gas lift design and performance
    /// </summary>
    public class GasLiftModuleData : FileSystemModuleData
    {
        public override string ModuleName => "GasLift";
        public override string Description => "Gas lift design and performance";
        public override string ScriptBasePath => "GasLift";
        public override int ExecutionOrder => 23;
        public override bool IsRequired => false;

        public GasLiftModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
