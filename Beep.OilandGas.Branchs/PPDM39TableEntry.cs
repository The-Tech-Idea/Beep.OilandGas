

namespace Beep.OilandGas.Branchs
{
    /// <summary>
    /// Enriched descriptor for a single PPDM39 table.
    /// Combines the flat category assignment with the FK-derived parent/child graph.
    /// </summary>
    public sealed class PPDM39TableEntry // class represent category tables
    {
        /// <summary>The PPDM39 table name (upper-case).</summary>
        public string TableName { get; init; } = string.Empty;

        /// <summary>Category numeric ID (matches <see cref="PPDM39Categories"/>).</summary>
        public int CategoryId { get; init; }

        /// <summary>The root/parent table of this table's category. Empty string when this IS the root.</summary>
        public string Parent { get; init; } = string.Empty;

        /// <summary>The primary-key column used to join to the parent table. Empty when root.</summary>
        public string PKColumn { get; init; } = string.Empty;
     

        /// <summary>Tables that directly foreign-key-reference this table (its children).</summary>
        public IReadOnlyList<PPDM38ChildRecord> Children { get; set; } = [];

        /// <summary>True when no FK parent exists — this is a root/master entity.</summary>
        public bool IsRoot {  get; set; }

        /// <summary>True when no FK children exist — this is a leaf/detail entity.</summary>
        public bool IsLeaf => Children.Count == 0;
    }
    
    public  sealed class PPDM38ChildRecord
    {
        public string TableName { get; init; } = string.Empty;
        public string ParentTableName { get; init; } = string.Empty;
        public string ParentColumnID { get; init; } = string.Empty;
        public string ColumnID { get; init; } = string.Empty;
        /// <summary>Tables that directly foreign-key-reference this table (its children).</summary>
        public IReadOnlyList<PPDM38ChildRecord> Children { get; set; } = []; // child tables 


    }
}
