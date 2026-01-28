using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Stratigraphy
{
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
