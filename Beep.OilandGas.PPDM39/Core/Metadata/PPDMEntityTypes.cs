#nullable enable
using System;
using System.Linq;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.PPDM39.Core.Metadata
{
    /// <summary>
    /// The entity class a PPDM table's rows are read and written as.
    /// </summary>
    /// <remarks>
    /// One answer for the generic data-management API and the web's generic table page (2026-10-10): the page read the API's
    /// rows as untyped JSON, so it could not tell one record from another — its Delete did nothing and its Edit posted a
    /// duplicate. It now reads them as this class, which is the class the API built them from.
    /// </remarks>
    public static class PPDMEntityTypes
    {
        private const string ModelsNamespace = "Beep.OilandGas.PPDM39.Models";

        /// <summary>
        /// The table's entity class: the class its metadata names, else the class named like the table — exactly, then
        /// ignoring case, then without underscores (<c>STRAT_UNIT</c> → <c>STRATUNIT</c>). Null when there is none.
        /// </summary>
        public static Type? Find(string? entityTypeName, string? tableName)
        {
            var assembly = typeof(Beep.OilandGas.PPDM39.Models.WELL).Assembly;

            if (!string.IsNullOrWhiteSpace(entityTypeName)
                && assembly.GetType($"{ModelsNamespace}.{entityTypeName}") is { } named)
                return named;

            if (string.IsNullOrWhiteSpace(tableName))
                return null;

            if (assembly.GetType($"{ModelsNamespace}.{tableName}") is { } exact)
                return exact;

            var withoutUnderscores = tableName.Replace("_", string.Empty);
            var entities = assembly.GetTypes().Where(type => typeof(IPPDMEntity).IsAssignableFrom(type)).ToList();
            return entities.FirstOrDefault(type => type.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                ?? entities.FirstOrDefault(type => type.Name.Equals(withoutUnderscores, StringComparison.OrdinalIgnoreCase));
        }
    }
}
