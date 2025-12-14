using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Core.Metadata
{
    /// <summary>
    /// Repository for PPDM table and relationship metadata
    /// Provides information about tables, primary keys, foreign keys, and relationships
    /// </summary>
    public interface IPPDMMetadataRepository
    {
        /// <summary>
        /// Gets metadata for a specific table
        /// </summary>
        Task<PPDMTableMetadata> GetTableMetadataAsync(string tableName);

        /// <summary>
        /// Gets all tables in a module
        /// </summary>
        Task<IEnumerable<PPDMTableMetadata>> GetTablesByModuleAsync(string module);

        /// <summary>
        /// Gets all foreign key relationships for a table
        /// </summary>
        Task<IEnumerable<PPDMForeignKey>> GetForeignKeysAsync(string tableName);

        /// <summary>
        /// Gets all tables that reference this table (reverse foreign keys)
        /// </summary>
        Task<IEnumerable<PPDMTableMetadata>> GetReferencingTablesAsync(string tableName);

        /// <summary>
        /// Gets the primary key column name for a table
        /// </summary>
        Task<string> GetPrimaryKeyColumnAsync(string tableName);

        /// <summary>
        /// Gets entity type name for a table
        /// </summary>
        Task<string> GetEntityTypeNameAsync(string tableName);

        /// <summary>
        /// Checks if a table has common columns
        /// </summary>
        Task<bool> HasCommonColumnsAsync(string tableName);

        /// <summary>
        /// Gets all modules
        /// </summary>
        Task<IEnumerable<string>> GetModulesAsync();
    }
}

