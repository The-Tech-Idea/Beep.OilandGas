using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

public static class ModuleSchemaBoundary
{
    private static readonly string[] RepositoryNamespaces =
    {
        "Beep.OilandGas.Models.Data.Security",
        "Beep.OilandGas.UserManagement.Models.Identity",
        "TheTechIdea.Data.OilGas",
        "Microsoft.AspNetCore.Identity"
    };

    public static void Validate(IEnumerable<Type> entityTypes)
    {
        if (entityTypes.Any(type => RepositoryNamespaces.Any(ns =>
            string.Equals(type.Namespace, ns, StringComparison.Ordinal) ||
            (type.Namespace?.StartsWith(ns + ".", StringComparison.Ordinal) ?? false))))
            throw new ArgumentException("Security and Identity extensions belong to the default EF repository and cannot be migrated into a module database.");
    }
}
