using System.Reflection;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// Opt-in diagnostic: prints the columns the entity reader produces for a type, with duplicates marked. It exists
/// because a duplicate column surfaces only as the migration engine's refusal, with no field list.
/// </summary>
public sealed class FieldProbeTests
{
    [Fact]
    public void Print_entity_fields()
    {
        if (Environment.GetEnvironmentVariable("OG_FIELD_PROBE") != "1")
        {
            return;
        }

        using var host = OgBeepHost.Create(new OgBeepHostOptions(
            Path.Combine(OgPaths.DataRoot, "fieldprobe"), "BeepOilGasFieldProbe"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var lines = new List<string>();
        var temperatureProperty = typeof(RUN_TICKET).GetProperty("Temperature");
        lines.Add($"-- reflection: Temperature declared by {temperatureProperty?.DeclaringType.FullName}, " +
                  $"notMapped={temperatureProperty?.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>() != null}, " +
                  $"type={temperatureProperty?.PropertyType}");

        foreach (var type in new[] { typeof(RUN_TICKET) })
        {
            var structure = host.Editor.classCreator.ConvertToEntityStructure(type);
            lines.Add($"== {type.Name}: {structure?.Fields?.Count ?? 0} field(s) ==");

            foreach (var field in structure?.Fields ?? [])
            {
                lines.Add($"  [{field.FieldName}] notMapped={field.IsNotMapped} key={field.IsKey} type={field.Fieldtype} ordinal={field.OrdinalPosition}");
            }
        }

        File.WriteAllLines(Path.Combine(Path.GetTempPath(), "og-field-probe.txt"), lines);
    }
}
