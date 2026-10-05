using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

public static class ModuleMigrationScope
{
    /// <summary>
    /// Why a module selection cannot be migrated, in words for the person who chose it; null when it can. The one statement
    /// of the rule: <see cref="Resolve"/> refuses with it, and the planner asks it first so that a selection it refuses is
    /// answered in the plan's result rather than as a failed plan (OILGAS-CATCH-01).
    /// </summary>
    public static string? Refusal(IReadOnlyList<string> requested,
        IReadOnlyList<(string ModuleId, string ModuleName, int Order, IReadOnlyList<Type> EntityTypes)> available)
    {
        if (requested.Count == 0 || requested.Any(string.IsNullOrWhiteSpace))
            return "Select at least one non-empty module identifier.";
        var selected = new HashSet<string>(requested, StringComparer.OrdinalIgnoreCase);
        if (selected.Contains("SECURITY"))
            return "Security is installed in the default repository through EF migrations, not a module database.";
        var known = available.Select(x => x.ModuleId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (selected.Any(id => !known.Contains(id)))
            return "One or more selected module identifiers are unknown.";
        return null;
    }

    public static List<Type> Resolve(IReadOnlyList<string> requested,
        IReadOnlyList<(string ModuleId, string ModuleName, int Order, IReadOnlyList<Type> EntityTypes)> available,
        IReadOnlyList<Type>? coreEntityTypes = null)
    {
        var refusal = Refusal(requested, available);
        if (refusal != null)
            throw new ArgumentException(refusal);
        var selected = new HashSet<string>(requested, StringComparer.OrdinalIgnoreCase);
        return available.Where(x => selected.Contains(x.ModuleId)).OrderBy(x => x.Order)
            .ThenBy(x => x.ModuleId, StringComparer.Ordinal)
            .SelectMany(x => string.Equals(x.ModuleId, "PPDM_CORE", StringComparison.OrdinalIgnoreCase)
                ? (coreEntityTypes ?? x.EntityTypes).Concat(x.EntityTypes) : x.EntityTypes)
            .Distinct().ToList();
    }
}
