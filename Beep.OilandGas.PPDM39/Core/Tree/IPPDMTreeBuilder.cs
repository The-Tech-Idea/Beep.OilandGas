using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.PPDM39.Core.Tree
{
    /// <summary>
    /// Interface for building PPDM 3.9 data model tree structures
    /// </summary>
    public interface IPPDMTreeBuilder
    {
        /// <summary>
        /// Builds a complete tree structure from metadata
        /// </summary>
        /// <param name="includeRelationships">Whether to include relationship nodes</param>
        /// <param name="includeColumns">Whether to include column nodes</param>
        /// <returns>Root node of the tree</returns>
        Task<PPDMTreeNode> BuildTreeAsync(bool includeRelationships = false, bool includeColumns = false);

        /// <summary>
        /// Builds a tree for a specific module
        /// </summary>
        /// <param name="moduleName">Module name (e.g., "Stratigraphy", "Wells")</param>
        /// <param name="includeRelationships">Whether to include relationship nodes</param>
        /// <param name="includeColumns">Whether to include column nodes</param>
        /// <returns>Module node with its tables</returns>
        Task<PPDMTreeNode> BuildModuleTreeAsync(string moduleName, bool includeRelationships = false, bool includeColumns = false);

        /// <summary>
        /// Builds a tree showing relationships for a specific table
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns>Table node with its relationships</returns>
        Task<PPDMTreeNode> BuildTableRelationshipsTreeAsync(string tableName);

        /// <summary>
        /// Gets all modules as a flat list
        /// </summary>
        Task<List<PPDMTreeNode>> GetModulesAsync();

        /// <summary>
        /// Gets all tables in a module as a flat list
        /// </summary>
        Task<List<PPDMTreeNode>> GetModuleTablesAsync(string moduleName);

        /// <summary>
        /// Filters the tree based on search text
        /// </summary>
        Task<PPDMTreeNode> FilterTreeAsync(PPDMTreeNode root, string searchText);
    }
}

