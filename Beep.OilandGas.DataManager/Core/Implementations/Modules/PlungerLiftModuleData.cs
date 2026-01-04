namespace Beep.OilandGas.DataManager.Core.Implementations.Modules
{
    /// <summary>
    /// Module data for PlungerLift calculation results and properties
    /// </summary>
    public class PlungerLiftModuleData : FileSystemModuleData
    {
        public override string ModuleName => "PlungerLift";
        public override string Description => "Plunger lift calculation results and properties";
        public override string ScriptBasePath => "PlungerLift";
        public override int ExecutionOrder => 18;
        public override bool IsRequired => false;

        public PlungerLiftModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }

        public override IEnumerable<string> GetDependencies() => 
            new[] { "PPDM39", "Common", "Security" };
    }
}
