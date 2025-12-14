using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.Core.Metadata
{
    /// <summary>
    /// Metadata for a PPDM table
    /// </summary>
    public class PPDMTableMetadata
    {
        /// <summary>
        /// Table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Entity type name
        /// </summary>
        public string EntityTypeName { get; set; }

        /// <summary>
        /// Primary key column name
        /// </summary>
        public string PrimaryKeyColumn { get; set; }

        /// <summary>
        /// Module/Domain this table belongs to (e.g., "Stratigraphy", "Well", "Production")
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// Original module name (if it was remapped for UI grouping)
        /// </summary>
        public string OriginalModule { get; set; }

        /// <summary>
        /// High level subject area (PPDM roadmap terminology)
        /// </summary>
        public string SubjectArea { get; set; }

        /// <summary>
        /// Optional sub-category within a module (e.g., "Products & Substances", "Sample Management")
        /// </summary>
        public string Submodule { get; set; }

        /// <summary>
        /// Foreign key relationships
        /// </summary>
        public List<PPDMForeignKey> ForeignKeys { get; set; } = new List<PPDMForeignKey>();

        /// <summary>
        /// Common columns this table has
        /// </summary>
        public List<string> CommonColumns { get; set; } = new List<string>();
    }

    /// <summary>
    /// Foreign key relationship metadata
    /// </summary>
    public class PPDMForeignKey
    {
        /// <summary>
        /// Foreign key column name in this table
        /// </summary>
        public string ForeignKeyColumn { get; set; }

        /// <summary>
        /// Referenced table name
        /// </summary>
        public string ReferencedTable { get; set; }

        /// <summary>
        /// Referenced primary key column
        /// </summary>
        public string ReferencedPrimaryKey { get; set; }

        /// <summary>
        /// Relationship type (OneToOne, OneToMany, ManyToMany)
        /// </summary>
        public string RelationshipType { get; set; } = "OneToMany";
    }
}
