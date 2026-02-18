using System.ComponentModel;

namespace Beep.OilandGas.Models.Enums.WellTest
{
    public enum SurfaceTestEquipment
    {
        [Description("Surface Test Tree (Flowhead)")] SurfaceTestTree,
        [Description("Emergency Shutdown (ESD) System")] ESDSystem,
        [Description("Data Header")] DataHeader,
        [Description("Surface Choke Manifold")] SurfaceChokeManifold,
        [Description("Indirect Fired Heater")] IndirectFiredHeater,
        [Description("Three-Phase Separator")] ThreePhaseSeparator,
        [Description("Surge Tank / Gauge Tank")] SurgeTank,
        [Description("Transfer Pumps")] TransferPumps,
        [Description("Evergreen Burner (Flare)")] EvergreenBurner,
        [Description("Data Acquisition System (DAQ)")] DataAcquisitionSystem,
        [Description("Unknown")] Unknown
    }
}
