using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.WellTest
{
    public enum WellTestType
    {
        [Description("Flow-After-Flow")] FlowAfterFlow,
        [Description("Isochronal")] Isochronal,
        [Description("Initial Potential (IP)")] InitialPotential,
        [Description("Pressure Build-Up (PBU)")] PressureBuildUp,
        [Description("Drawdown")] Drawdown,
        [Description("Interference")] Interference,
        [Description("Fall-Off")] FallOff,
        [Description("Drill Stem Test (DST)")] DrillStemTest,
        [Description("Extended Well Test (EWT)")] ExtendedWellTest,
        [Description("Mechanical Integrity Test (MIT)")] MechanicalIntegrityTest,
        [Description("Inflow Test")] InflowTest,
        [Description("Formation Tester (RFT/MDT)")] FormationTester,
        [Description("Production Log (PLT)")] ProductionLog,
        [Description("PVT Sampling")] PVTSampling,
        [Description("Production Test")] ProductionTest, // Legacy support
        [Description("Injection Test")] InjectionTest, // Legacy support
        [Description("Modified Isochronal")] ModifiedIsochronal, // Legacy support
        [Description("Unknown")] Unknown
    }
}
