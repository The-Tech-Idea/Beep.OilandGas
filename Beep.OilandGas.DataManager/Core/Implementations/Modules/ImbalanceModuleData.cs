namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Production imbalance tracking
    /// </summary>
    public class ImbalanceModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Imbalance";
        public override string Description => "Production imbalance tracking";
        public override string ScriptBasePath => "Imbalance";
        public override int ExecutionOrder => 70;
        public override bool IsRequired => false;

        public ImbalanceModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
