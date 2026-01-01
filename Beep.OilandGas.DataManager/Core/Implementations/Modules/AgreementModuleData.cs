namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Sales and transportation agreements
    /// </summary>
    public class AgreementModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Agreement";
        public override string Description => "Sales and transportation agreements";
        public override string ScriptBasePath => "Agreement";
        public override int ExecutionOrder => 41;
        public override bool IsRequired => false;

        public AgreementModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
