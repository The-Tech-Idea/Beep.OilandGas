using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Stratigraphy
{
    /// <summary>
    /// Data Transfer Object for Stratigraphic Hierarchy
    /// Represents parent-child relationships between stratigraphic units
    /// </summary>
    public class StratHierarchyDto
    {
        /// <summary>
        /// Parent Stratigraphic Name Set ID
        /// </summary>
        public string PARENT_STRAT_NAME_SET_ID { get; set; }

        /// <summary>
        /// Parent Stratigraphic Unit ID
        /// </summary>
        public string PARENT_STRAT_UNIT_ID { get; set; }

        /// <summary>
        /// Child Stratigraphic Name Set ID
        /// </summary>
        public string CHILD_STRAT_NAME_SET_ID { get; set; }

        /// <summary>
        /// Child Stratigraphic Unit ID
        /// </summary>
        public string CHILD_STRAT_UNIT_ID { get; set; }

        /// <summary>
        /// Hierarchy Type
        /// </summary>
        public string HIERARCHY_TYPE { get; set; }

        /// <summary>
        /// Effective Date
        /// </summary>
        public DateTime? EFFECTIVE_DATE { get; set; }

        /// <summary>
        /// Expiry Date
        /// </summary>
        public DateTime? EXPIRY_DATE { get; set; }

        /// <summary>
        /// Remark
        /// </summary>
        public string REMARK { get; set; }

        /// <summary>
        /// Active Indicator ('Y' or 'N')
        /// </summary>
        public string ACTIVE_IND { get; set; }

        /// <summary>
        /// PPDM GUID
        /// </summary>
        public string PPDM_GUID { get; set; }

        /// <summary>
        /// Row Created By
        /// </summary>
        public string ROW_CREATED_BY { get; set; }

        /// <summary>
        /// Row Created Date
        /// </summary>
        public DateTime? ROW_CREATED_DATE { get; set; }

        /// <summary>
        /// Row Changed By
        /// </summary>
        public string ROW_CHANGED_BY { get; set; }

        /// <summary>
        /// Row Changed Date
        /// </summary>
        public DateTime? ROW_CHANGED_DATE { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for Stratigraphic Hierarchy Tree
    /// Represents a complete hierarchy tree structure
    /// </summary>
    public class StratHierarchyTreeDto
    {
        /// <summary>
        /// Root stratigraphic unit
        /// </summary>
        public StratUnitDto RootUnit { get; set; }

        /// <summary>
        /// Child units (direct children)
        /// </summary>
        public List<StratHierarchyTreeNodeDto> Children { get; set; } = new List<StratHierarchyTreeNodeDto>();
    }

    /// <summary>
    /// Data Transfer Object for a node in the hierarchy tree
    /// </summary>
    public class StratHierarchyTreeNodeDto
    {
        /// <summary>
        /// Stratigraphic unit
        /// </summary>
        public StratUnitDto Unit { get; set; }

        /// <summary>
        /// Child units (nested children)
        /// </summary>
        public List<StratHierarchyTreeNodeDto> Children { get; set; } = new List<StratHierarchyTreeNodeDto>();

        /// <summary>
        /// Hierarchy level (depth in tree)
        /// </summary>
        public int Level { get; set; }
    }
}

