using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// The PPDM tables declare their primary keys in the table metadata, and the generated entity is what the schema
/// migration and the entity validator read. Those two must agree: a table with a metadata key but an unannotated model
/// reaches validation as "Entity has no primary key defined" and the module cannot be applied.
/// </summary>
/// <remarks>
/// This is the regression test for the reference-data module that could not be applied: `R_WELL_STATUS_TYPE` (and the
/// rest of the family, including the `RA_*` audit twins) now carries <c>[Key]</c> on each column the metadata names.
/// </remarks>
public sealed class PpdmCodeTableKeyTests
{
    private static readonly Assembly Models = typeof(R_WELL_STATUS_TYPE).Assembly;

    [Fact]
    public void Every_table_in_the_metadata_has_a_keyed_model_when_the_model_exists()
    {
        var missing = new List<string>();
        var checkedModels = 0;

        foreach (var (table, metadata) in PPDM39Metadata.GetMetadata())
        {
            if (string.IsNullOrWhiteSpace(metadata.PrimaryKeyColumn))
            {
                continue;
            }

            var type = FindModel(metadata.EntityTypeName, table);

            if (type is null)
            {
                continue;
            }

            checkedModels++;

            var keys = KeyProperties(type);

            foreach (var column in metadata.PrimaryKeyColumn.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!keys.Contains(column, StringComparer.OrdinalIgnoreCase))
                {
                    missing.Add($"{type.Name}.{column}");
                }
            }
        }

        Assert.True(checkedModels > 2_000, $"only {checkedModels} model(s) were checked");
        Assert.Empty(missing);
    }

    [Fact]
    public void The_reference_tables_and_their_audit_twins_are_keyed()
    {
        var referenceTables = PPDM39Metadata
            .GetMetadata()
            .Where(entry => entry.Key.StartsWith("R_", StringComparison.OrdinalIgnoreCase)
                            || entry.Key.StartsWith("RA_", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(referenceTables.Count > 800, $"{referenceTables.Count} reference table(s) found");

        foreach (var (table, metadata) in referenceTables)
        {
            var type = FindModel(metadata.EntityTypeName, table);

            if (type is null)
            {
                continue;
            }

            var keys = KeyProperties(type);

            Assert.True(
                keys.Count > 0,
                $"the reference table '{table}' has no [Key] on its model '{type.Name}'");
        }
    }

    [Fact]
    public void The_well_status_type_code_table_is_keyed_on_the_column_the_metadata_names()
    {
        var keys = KeyProperties(typeof(R_WELL_STATUS_TYPE));

        Assert.Contains("STATUS_TYPE", keys, StringComparer.OrdinalIgnoreCase);

        // A composite key is allowed, but this table's key is one column.
        Assert.Single(keys);
    }

    private static Type? FindModel(string? entityTypeName, string table)
    {
        if (!string.IsNullOrWhiteSpace(entityTypeName))
        {
            var byFullName = Models.GetType(entityTypeName, throwOnError: false);

            if (byFullName is not null)
            {
                return byFullName;
            }

            var shortName = entityTypeName.Split('.').Last();

            var byName = Models.GetTypes()
                .FirstOrDefault(candidate => string.Equals(candidate.Name, shortName, StringComparison.OrdinalIgnoreCase));

            if (byName is not null)
            {
                return byName;
            }
        }

        return Models.GetTypes()
            .FirstOrDefault(candidate => string.Equals(candidate.Name, table, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> KeyProperties(Type type) =>
        type.GetProperties()
            .Where(property => property.GetCustomAttribute<KeyAttribute>() is not null)
            .Select(property => property.Name)
            .ToList();
}
