using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

public static class ModuleMigrationScope
{
    public static List<Type> Resolve(IReadOnlyList<string> requested,
        IReadOnlyList<(string ModuleId, string ModuleName, int Order, IReadOnlyList<Type> EntityTypes)> available,
        IReadOnlyList<Type>? coreEntityTypes = null)
    {
        if (requested.Count == 0 || requested.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Select at least one non-empty module identifier.");
        var selected = new HashSet<string>(requested, StringComparer.OrdinalIgnoreCase);
        if (selected.Contains("SECURITY"))
            throw new ArgumentException("Security is installed in the default repository through EF migrations, not a module database.");
        var known = available.Select(x => x.ModuleId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (selected.Any(id => !known.Contains(id)))
            throw new ArgumentException("One or more selected module identifiers are unknown.");
        return available.Where(x => selected.Contains(x.ModuleId)).OrderBy(x => x.Order)
            .ThenBy(x => x.ModuleId, StringComparer.Ordinal)
            .SelectMany(x => string.Equals(x.ModuleId, "PPDM_CORE", StringComparison.OrdinalIgnoreCase)
                ? (coreEntityTypes ?? x.EntityTypes).Concat(x.EntityTypes) : x.EntityTypes)
            .Distinct().ToList();
    }
}
