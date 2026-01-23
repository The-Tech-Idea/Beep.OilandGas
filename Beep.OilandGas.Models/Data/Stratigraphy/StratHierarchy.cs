using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Stratigraphy
{
    /// <summary>
    /// Data Transfer Object for Stratigraphic Hierarchy
    /// Represents parent-child relationships between stratigraphic units
    /// </summary>
    public class StratHierarchy : ModelEntityBase
    {
        /// <summary>
        /// Parent Stratigraphic Name Set ID
        /// </summary>
        private string PARENT_STRAT_NAME_SET_IDValue;

        public string PARENT_STRAT_NAME_SET_ID

        {

            get { return this.PARENT_STRAT_NAME_SET_IDValue; }

            set { SetProperty(ref PARENT_STRAT_NAME_SET_IDValue, value); }

        }

        /// <summary>
        /// Parent Stratigraphic Unit ID
        /// </summary>
        private string PARENT_STRAT_UNIT_IDValue;

        public string PARENT_STRAT_UNIT_ID

        {

            get { return this.PARENT_STRAT_UNIT_IDValue; }

            set { SetProperty(ref PARENT_STRAT_UNIT_IDValue, value); }

        }

        /// <summary>
        /// Child Stratigraphic Name Set ID
        /// </summary>
        private string CHILD_STRAT_NAME_SET_IDValue;

        public string CHILD_STRAT_NAME_SET_ID

        {

            get { return this.CHILD_STRAT_NAME_SET_IDValue; }

            set { SetProperty(ref CHILD_STRAT_NAME_SET_IDValue, value); }

        }

        /// <summary>
        /// Child Stratigraphic Unit ID
        /// </summary>
        private string CHILD_STRAT_UNIT_IDValue;

        public string CHILD_STRAT_UNIT_ID

        {

            get { return this.CHILD_STRAT_UNIT_IDValue; }

            set { SetProperty(ref CHILD_STRAT_UNIT_IDValue, value); }

        }

        /// <summary>
        /// Hierarchy Type
        /// </summary>
        private string HIERARCHY_TYPEValue;

        public string HIERARCHY_TYPE

        {

            get { return this.HIERARCHY_TYPEValue; }

            set { SetProperty(ref HIERARCHY_TYPEValue, value); }

        }

        /// <summary>
        /// Effective Date
        /// </summary>

        /// <summary>
        /// Expiry Date
        /// </summary>

        /// <summary>
        /// Remark
        /// </summary>

        /// <summary>
        /// Active Indicator ('Y' or 'N')
        /// </summary>

        /// <summary>
        /// PPDM GUID
        /// </summary>

        /// <summary>
        /// Row Created By
        /// </summary>

        /// <summary>
        /// Row Created Date
        /// </summary>

        /// <summary>
        /// Row Changed By
        /// </summary>

        /// <summary>
        /// Row Changed Date
        /// </summary>

    }

    /// <summary>
    /// Data Transfer Object for Stratigraphic Hierarchy Tree
    /// Represents a complete hierarchy tree structure
    /// </summary>
    public class StratHierarchyTree : ModelEntityBase
    {
        /// <summary>
        /// Root stratigraphic unit
        /// </summary>
        private StratUnit RootUnitValue;

        public StratUnit RootUnit

        {

            get { return this.RootUnitValue; }

            set { SetProperty(ref RootUnitValue, value); }

        }

        /// <summary>
        /// Child units (direct children)
        /// </summary>
        private List<StratHierarchyTreeNode> ChildrenValue = new List<StratHierarchyTreeNode>();

        public List<StratHierarchyTreeNode> Children

        {

            get { return this.ChildrenValue; }

            set { SetProperty(ref ChildrenValue, value); }

        }
    }

    /// <summary>
    /// Data Transfer Object for a node in the hierarchy tree
    /// </summary>
    public class StratHierarchyTreeNode : ModelEntityBase
    {
        /// <summary>
        /// Stratigraphic unit
        /// </summary>
        private StratUnit UnitValue;

        public StratUnit Unit

        {

            get { return this.UnitValue; }

            set { SetProperty(ref UnitValue, value); }

        }

        /// <summary>
        /// Child units (nested children)
        /// </summary>
        private List<StratHierarchyTreeNode> ChildrenValue = new List<StratHierarchyTreeNode>();

        public List<StratHierarchyTreeNode> Children

        {

            get { return this.ChildrenValue; }

            set { SetProperty(ref ChildrenValue, value); }

        }

        /// <summary>
        /// Hierarchy level (depth in tree)
        /// </summary>
        private int LevelValue;

        public int Level

        {

            get { return this.LevelValue; }

            set { SetProperty(ref LevelValue, value); }

        }
    }
}


