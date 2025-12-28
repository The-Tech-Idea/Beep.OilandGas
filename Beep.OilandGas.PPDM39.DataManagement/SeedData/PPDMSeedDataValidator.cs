using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Validates seed data before insertion
    /// </summary>
    public class PPDMSeedDataValidator
    {
        private readonly IPPDMMetadataRepository _metadata;

        public PPDMSeedDataValidator(IPPDMMetadataRepository metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Validates seed data for a table
        /// </summary>
        public async Task<SeedDataValidationResult> ValidateSeedDataAsync(string tableName, List<Dictionary<string, object>> seedData)
        {
            var result = new SeedDataValidationResult
            {
                TableName = tableName,
                IsValid = true,
                Errors = new List<string>()
            };

            try
            {
                var metadata = await _metadata.GetTableMetadataAsync(tableName);
                if (metadata == null)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Table metadata not found for: {tableName}");
                    return result;
                }

                // Validate each row
                for (int i = 0; i < seedData.Count; i++)
                {
                    var row = seedData[i];
                    var rowErrors = await ValidateRowAsync(metadata, row, i);
                    result.Errors.AddRange(rowErrors);
                }

                result.IsValid = result.Errors.Count == 0;
                result.ValidRows = seedData.Count - result.Errors.Count;
                result.InvalidRows = result.Errors.Count;

                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Error validating seed data: {ex.Message}");
                return result;
            }
        }

        private async Task<List<string>> ValidateRowAsync(PPDMTableMetadata metadata, Dictionary<string, object> row, int rowIndex)
        {
            var errors = new List<string>();

            // Check required columns (primary key is always required)
            if (!string.IsNullOrEmpty(metadata.PrimaryKeyColumn) && !row.ContainsKey(metadata.PrimaryKeyColumn))
            {
                errors.Add($"Row {rowIndex + 1}: Required primary key column '{metadata.PrimaryKeyColumn}' is missing");
            }

            // Validate data types for provided columns
            // Note: Full validation would require column metadata which may not be available
            // This is a simplified validation

            // Note: PPDMTableMetadata doesn't include column information
            // Column validation would require additional metadata that's not currently available
            // For now, we skip the unknown column check since we don't have column metadata

            return errors;
        }

        private bool IsValidType(object value, string? expectedDataType)
        {
            if (string.IsNullOrEmpty(expectedDataType))
                return true;

            var valueType = value.GetType();

            // Simple type checking - can be enhanced
            if (expectedDataType.Contains("VARCHAR") || expectedDataType.Contains("CHAR") || expectedDataType.Contains("TEXT"))
            {
                return valueType == typeof(string);
            }

            if (expectedDataType.Contains("INT") || expectedDataType.Contains("NUMBER"))
            {
                return valueType == typeof(int) || valueType == typeof(long) || valueType == typeof(short) || valueType == typeof(byte);
            }

            if (expectedDataType.Contains("DECIMAL") || expectedDataType.Contains("NUMERIC") || expectedDataType.Contains("FLOAT"))
            {
                return valueType == typeof(decimal) || valueType == typeof(double) || valueType == typeof(float);
            }

            if (expectedDataType.Contains("DATE") || expectedDataType.Contains("TIME"))
            {
                return valueType == typeof(DateTime) || valueType == typeof(DateTime?);
            }

            return true; // Default to valid if we can't determine
        }

        /// <summary>
        /// Validation result
        /// </summary>
        public class SeedDataValidationResult
        {
            public string TableName { get; set; } = string.Empty;
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public int ValidRows { get; set; }
            public int InvalidRows { get; set; }
        }
    }
}

