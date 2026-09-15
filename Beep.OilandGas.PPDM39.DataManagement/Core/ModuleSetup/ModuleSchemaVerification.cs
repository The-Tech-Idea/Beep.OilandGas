using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

public static class ModuleSchemaVerification
{
    public static void Verify(IDMEEditor editor, IDataSource source, IReadOnlyList<Type> entityTypes)
    {
        if (entityTypes.Count == 0)
            throw new InvalidOperationException("Module schema verification requires an explicit entity manifest.");
        ModuleSchemaBoundary.Validate(entityTypes);
        foreach (var type in entityTypes)
        {
            var expected = editor.classCreator.ConvertToEntityStructure(type)
                ?? throw new InvalidOperationException($"Cannot resolve schema metadata for '{type.Name}'.");
            // A new structure prevents cached, pre-execution fields from satisfying verification.
            var actual = source.GetEntityStructure(new EntityStructure
            {
                EntityName = expected.EntityName,
                DatasourceEntityName = expected.EntityName
            }, true);
            if (actual?.Fields is null || actual.Fields.Count == 0)
                throw new InvalidOperationException($"Module table '{expected.EntityName}' could not be verified after migration.");
            foreach (var field in expected.Fields)
            {
                var matches = actual.Fields.Where(candidate => string.Equals(candidate.FieldName,
                    field.FieldName, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (matches.Length != 1 || field.IsKey && (!matches[0].IsKey || matches[0].AllowDBNull))
                    throw new InvalidOperationException($"Module column '{expected.EntityName}.{field.FieldName}' is missing or its primary key is invalid after migration.");
                if (string.Equals(field.Fieldtype, typeof(DateTime).FullName, StringComparison.Ordinal)
                    && string.Equals(matches[0].ColumnTypeName?.Trim(), "date", StringComparison.OrdinalIgnoreCase)
                    && source.DatasourceType is TheTechIdea.Beep.Utilities.DataSourceType.SqlServer
                        or TheTechIdea.Beep.Utilities.DataSourceType.Postgre)
                    throw new InvalidOperationException($"Module column '{expected.EntityName}.{field.FieldName}' is date-only but requires a timestamp. Review a schema upgrade before using this module.");
            }
        }
    }
}
