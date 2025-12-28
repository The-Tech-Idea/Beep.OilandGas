using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Generates seed data templates from entity definitions
    /// </summary>
    public class PPDMSeedDataGenerator
    {
        private readonly IPPDMMetadataRepository _metadata;

        public PPDMSeedDataGenerator(IPPDMMetadataRepository metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Generates seed data template for a table
        /// </summary>
        public async Task<List<Dictionary<string, object>>> GenerateSeedDataTemplateAsync(string tableName, int numberOfRows = 1)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            if (metadata == null)
            {
                throw new InvalidOperationException($"Table metadata not found for: {tableName}");
            }

            var template = new List<Dictionary<string, object>>();

            for (int i = 0; i < numberOfRows; i++)
            {
                var row = new Dictionary<string, object>();

                // Generate default values based on column metadata
                // This is a simplified version - in full implementation, would use metadata to determine defaults
                
                // For now, generate basic defaults for common columns
                // In full implementation, would iterate through metadata columns if available
                row["ACTIVE_IND"] = "Y";
                row["PPDM_GUID"] = Guid.NewGuid().ToString().ToUpper();

                template.Add(row);
            }

            return template;
        }

        /// <summary>
        /// Generates seed data template from entity type
        /// </summary>
        public async Task<List<Dictionary<string, object>>> GenerateSeedDataTemplateFromEntityAsync(Type entityType, int numberOfRows = 1)
        {
            var tableName = entityType.Name;
            return await GenerateSeedDataTemplateAsync(tableName, numberOfRows);
        }

        private object? GetDefaultValue(string columnName, string? dataType, bool isNullable)
        {
            // Skip if nullable and no specific default needed
            if (isNullable && !IsRequiredColumn(columnName))
                return null;

            // Handle common column patterns
            if (columnName.EndsWith("_ID", StringComparison.OrdinalIgnoreCase))
            {
                return Guid.NewGuid().ToString().Substring(0, 40);
            }

            if (columnName.Contains("GUID", StringComparison.OrdinalIgnoreCase))
            {
                return Guid.NewGuid().ToString().ToUpper();
            }

            if (columnName.Equals("ACTIVE_IND", StringComparison.OrdinalIgnoreCase))
            {
                return "Y";
            }

            if (columnName.Contains("DATE", StringComparison.OrdinalIgnoreCase))
            {
                return DateTime.UtcNow;
            }

            if (dataType != null)
            {
                if (dataType.Contains("VARCHAR") || dataType.Contains("CHAR") || dataType.Contains("TEXT"))
                {
                    return string.Empty;
                }

                if (dataType.Contains("INT") || dataType.Contains("NUMBER"))
                {
                    return 0;
                }

                if (dataType.Contains("DECIMAL") || dataType.Contains("NUMERIC") || dataType.Contains("FLOAT"))
                {
                    return 0.0m;
                }
            }

            return null;
        }

        private bool IsRequiredColumn(string columnName)
        {
            // Standard PPDM columns that should have defaults
            return columnName.Equals("ACTIVE_IND", StringComparison.OrdinalIgnoreCase) ||
                   columnName.Contains("GUID", StringComparison.OrdinalIgnoreCase) ||
                   columnName.Contains("DATE", StringComparison.OrdinalIgnoreCase);
        }
    }
}

