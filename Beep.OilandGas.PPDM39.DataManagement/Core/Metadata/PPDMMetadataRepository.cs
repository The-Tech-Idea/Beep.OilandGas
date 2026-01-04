using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata
{
    /// <summary>
    /// Implementation of PPDM metadata repository
    /// Loads metadata from SQL scripts or seeded data
    /// </summary>
    /// <remarks>
    /// [Obsolete] This class is deprecated. Use PPDMMetadataService instead for better performance, caching, and extended functionality.
    /// </remarks>
    [Obsolete("Use PPDMMetadataService instead. This class will be removed in a future version.")]
    public class PPDMMetadataRepository : IPPDMMetadataRepository
    {
        private readonly Dictionary<string, PPDMTableMetadata> _metadata;

        /// <summary>
        /// Creates metadata repository from SQL script
        /// </summary>
        public PPDMMetadataRepository(string sqlScriptPath = null)
        {
            if (!string.IsNullOrWhiteSpace(sqlScriptPath))
            {
                var loader = new PPDMSqlMetadataLoader();
                _metadata = loader.LoadFromSqlFile(sqlScriptPath);
            }
            else
            {
                _metadata = SeedMetadata();
            }
        }

        /// <summary>
        /// Creates metadata repository from pre-loaded metadata dictionary
        /// </summary>
        public PPDMMetadataRepository(Dictionary<string, PPDMTableMetadata> metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        /// <summary>
        /// Creates metadata repository from generated C# class
        /// </summary>
        public static PPDMMetadataRepository FromGeneratedClass()
        {
            // This will use the generated PPDM39Metadata class
            var metadata = PPDM39Metadata.GetMetadata();
            return new PPDMMetadataRepository(metadata);
        }

        /// <summary>
        /// Creates metadata repository from JSON file
        /// </summary>
        public static PPDMMetadataRepository FromJsonFile(string jsonPath)
        {
            if (!System.IO.File.Exists(jsonPath))
                throw new System.IO.FileNotFoundException($"JSON metadata file not found: {jsonPath}");

            var json = System.IO.File.ReadAllText(jsonPath);
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, PPDMTableMetadata>>(json, options);
            return new PPDMMetadataRepository(metadata);
        }

        public Task<PPDMTableMetadata> GetTableMetadataAsync(string tableName)
        {
            _metadata.TryGetValue(tableName.ToUpper(), out var tableMeta);
            return Task.FromResult(tableMeta);
        }

        public Task<IEnumerable<PPDMTableMetadata>> GetTablesByModuleAsync(string module)
        {
            var tables = _metadata.Values
                .Where(m => m.Module.Equals(module, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult<IEnumerable<PPDMTableMetadata>>(tables);
        }

        public Task<IEnumerable<PPDMForeignKey>> GetForeignKeysAsync(string tableName)
        {
            var metadata = _metadata.GetValueOrDefault(tableName.ToUpper());
            return Task.FromResult(metadata?.ForeignKeys ?? Enumerable.Empty<PPDMForeignKey>());
        }

        public Task<IEnumerable<PPDMTableMetadata>> GetReferencingTablesAsync(string tableName)
        {
            var referencing = _metadata.Values
                .Where(m => m.ForeignKeys.Any(fk => 
                    fk.ReferencedTable.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            return Task.FromResult<IEnumerable<PPDMTableMetadata>>(referencing);
        }

        public Task<string> GetPrimaryKeyColumnAsync(string tableName)
        {
            var metadata = _metadata.GetValueOrDefault(tableName.ToUpper());
            return Task.FromResult(metadata?.PrimaryKeyColumn);
        }

        public Task<string> GetEntityTypeNameAsync(string tableName)
        {
            var metadata = _metadata.GetValueOrDefault(tableName.ToUpper());
            return Task.FromResult(metadata?.EntityTypeName);
        }

        public Task<bool> HasCommonColumnsAsync(string tableName)
        {
            var metadata = _metadata.GetValueOrDefault(tableName.ToUpper());
            return Task.FromResult(metadata?.CommonColumns?.Any() ?? false);
        }

        public Task<IEnumerable<string>> GetModulesAsync()
        {
            var modules = _metadata.Values
                .Select(m => m.Module)
                .Distinct()
                .ToList();
            return Task.FromResult<IEnumerable<string>>(modules);
        }

        /// <summary>
        /// Seeds metadata about PPDM tables and relationships
        /// This should be loaded from a configuration file or database in production
        /// </summary>
        private Dictionary<string, PPDMTableMetadata> SeedMetadata()
        {
            var metadata = new Dictionary<string, PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase);

            // Stratigraphy Module
            AddTable(metadata, new PPDMTableMetadata
            {
                TableName = "STRAT_UNIT",
                EntityTypeName = "STRAT_UNIT",
                PrimaryKeyColumn = "STRAT_UNIT_ID",
                Module = "Stratigraphy",
                CommonColumns = new List<string> { "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID" },
                ForeignKeys = new List<PPDMForeignKey>
                {
                    new PPDMForeignKey { ForeignKeyColumn = "STRAT_NAME_SET_ID", ReferencedTable = "STRAT_NAME_SET", ReferencedPrimaryKey = "STRAT_NAME_SET_ID" },
                    new PPDMForeignKey { ForeignKeyColumn = "AREA_ID", ReferencedTable = "AREA", ReferencedPrimaryKey = "AREA_ID" }
                }
            });

            AddTable(metadata, new PPDMTableMetadata
            {
                TableName = "STRAT_COLUMN",
                EntityTypeName = "STRAT_COLUMN",
                PrimaryKeyColumn = "STRAT_COLUMN_ID",
                Module = "Stratigraphy",
                CommonColumns = new List<string> { "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID" },
                ForeignKeys = new List<PPDMForeignKey>
                {
                    new PPDMForeignKey { ForeignKeyColumn = "AREA_ID", ReferencedTable = "AREA", ReferencedPrimaryKey = "AREA_ID" }
                }
            });

            AddTable(metadata, new PPDMTableMetadata
            {
                TableName = "STRAT_HIERARCHY",
                EntityTypeName = "STRAT_HIERARCHY",
                PrimaryKeyColumn = "PARENT_STRAT_UNIT_ID", // Composite key
                Module = "Stratigraphy",
                CommonColumns = new List<string> { "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID" },
                ForeignKeys = new List<PPDMForeignKey>
                {
                    new PPDMForeignKey { ForeignKeyColumn = "PARENT_STRAT_UNIT_ID", ReferencedTable = "STRAT_UNIT", ReferencedPrimaryKey = "STRAT_UNIT_ID" },
                    new PPDMForeignKey { ForeignKeyColumn = "CHILD_STRAT_UNIT_ID", ReferencedTable = "STRAT_UNIT", ReferencedPrimaryKey = "STRAT_UNIT_ID" }
                }
            });

            AddTable(metadata, new PPDMTableMetadata
            {
                TableName = "STRAT_WELL_SECTION",
                EntityTypeName = "STRAT_WELL_SECTION",
                PrimaryKeyColumn = "INTERP_ID", // Composite with UWI
                Module = "Stratigraphy",
                CommonColumns = new List<string> { "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID" },
                ForeignKeys = new List<PPDMForeignKey>
                {
                    new PPDMForeignKey { ForeignKeyColumn = "UWI", ReferencedTable = "WELL", ReferencedPrimaryKey = "UWI" },
                    new PPDMForeignKey { ForeignKeyColumn = "STRAT_UNIT_ID", ReferencedTable = "STRAT_UNIT", ReferencedPrimaryKey = "STRAT_UNIT_ID" }
                }
            });

            AddTable(metadata, new PPDMTableMetadata
            {
                TableName = "STRAT_COLUMN_UNIT",
                EntityTypeName = "STRAT_COLUMN_UNIT",
                PrimaryKeyColumn = "STRAT_COLUMN_ID", // Composite key
                Module = "Stratigraphy",
                CommonColumns = new List<string> { "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID" },
                ForeignKeys = new List<PPDMForeignKey>
                {
                    new PPDMForeignKey { ForeignKeyColumn = "STRAT_COLUMN_ID", ReferencedTable = "STRAT_COLUMN", ReferencedPrimaryKey = "STRAT_COLUMN_ID" },
                    new PPDMForeignKey { ForeignKeyColumn = "STRAT_UNIT_ID", ReferencedTable = "STRAT_UNIT", ReferencedPrimaryKey = "STRAT_UNIT_ID" }
                }
            });

            // Well Module
            AddTable(metadata, new PPDMTableMetadata
            {
                TableName = "WELL",
                EntityTypeName = "WELL",
                PrimaryKeyColumn = "UWI",
                Module = "Well",
                CommonColumns = new List<string> { "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID" }
            });

            // Production Module
            AddTable(metadata, new PPDMTableMetadata
            {
                TableName = "PDEN",
                EntityTypeName = "PDEN",
                PrimaryKeyColumn = "PDEN_ID",
                Module = "Production",
                CommonColumns = new List<string> { "ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID" },
                ForeignKeys = new List<PPDMForeignKey>
                {
                    new PPDMForeignKey { ForeignKeyColumn = "UWI", ReferencedTable = "WELL", ReferencedPrimaryKey = "UWI" }
                }
            });

            // Add more tables as needed...
            // In production, this should be loaded from a configuration file or database

            return metadata;
        }

        private void AddTable(Dictionary<string, PPDMTableMetadata> metadata, PPDMTableMetadata table)
        {
            metadata[table.TableName.ToUpper()] = table;
        }
    }
}

