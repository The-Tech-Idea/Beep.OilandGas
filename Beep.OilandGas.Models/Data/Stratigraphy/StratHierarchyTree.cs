using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Stratigraphy
{
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
}
