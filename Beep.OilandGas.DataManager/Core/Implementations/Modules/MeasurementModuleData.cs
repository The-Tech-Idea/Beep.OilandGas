namespace Beep.OilandGas.DataManager.Core.Implementations
{
    /// <summary>
    /// Module data for Measurement records
    /// </summary>
    public class MeasurementModuleData : FileSystemModuleData
    {
        public override string ModuleName => "Measurement";
        public override string Description => "Measurement records";
        public override string ScriptBasePath => "Measurement";
        public override int ExecutionOrder => 71;
        public override bool IsRequired => false;

        public MeasurementModuleData(string? scriptsBasePath = null) : base(scriptsBasePath) { }
    }
}
