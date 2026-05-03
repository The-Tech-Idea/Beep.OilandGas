using System.Collections.Generic;

namespace Beep.OilandGas.ProductionOperations.Constants;

public static class ProductionOperationsReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ShortName);

    public static IEnumerable<SeedRow> GetMonitoringReferenceRows()
    {
        yield return new(ProductionOperationsReferenceSets.EquipmentActivityType, "INSTALL", "Equipment Installed", "INSTALL");
        yield return new(ProductionOperationsReferenceSets.EquipmentActivityType, "UNINSTALL", "Equipment Uninstalled", "UNINSTALL");
        yield return new(ProductionOperationsReferenceSets.EquipmentActivityType, "MOVE", "Equipment Moved", "MOVE");
        yield return new(ProductionOperationsReferenceSets.EquipmentActivityType, "REPLACE", "Equipment Replaced", "REPLACE");

        yield return new(ProductionOperationsReferenceSets.MeasurementType, "TANK_LEVEL", "Tank Level", "TANK_LEVEL");
        yield return new(ProductionOperationsReferenceSets.MeasurementType, "FLOW_RATE", "Flow Rate", "FLOW_RATE");
        yield return new(ProductionOperationsReferenceSets.MeasurementType, "PRESSURE", "Pressure", "PRESSURE");
        yield return new(ProductionOperationsReferenceSets.MeasurementType, "TEMPERATURE", "Temperature", "TEMPERATURE");
        yield return new(ProductionOperationsReferenceSets.MeasurementType, "VIBRATION", "Vibration", "VIBRATION");
        yield return new(ProductionOperationsReferenceSets.MeasurementType, "POWER_DRAW", "Power Draw", "POWER_DRAW");

        yield return new(ProductionOperationsReferenceSets.MeasurementQuality, "MEASURED", "Directly Measured", "MEASURED");
        yield return new(ProductionOperationsReferenceSets.MeasurementQuality, "ESTIMATED", "Estimated", "ESTIMATED");
        yield return new(ProductionOperationsReferenceSets.MeasurementQuality, "IMPUTED", "Imputed", "IMPUTED");

        yield return new(ProductionOperationsReferenceSets.MeasurementUom, "PERCENT", "Percent", "%");
        yield return new(ProductionOperationsReferenceSets.MeasurementUom, "M", "Meter", "M");
        yield return new(ProductionOperationsReferenceSets.MeasurementUom, "FT", "Foot", "FT");
        yield return new(ProductionOperationsReferenceSets.MeasurementUom, "PSI", "Pressure (PSI)", "PSI");
        yield return new(ProductionOperationsReferenceSets.MeasurementUom, "DEG_C", "Degrees Celsius", "C");
        yield return new(ProductionOperationsReferenceSets.MeasurementUom, "DEG_F", "Degrees Fahrenheit", "F");
    }
}
