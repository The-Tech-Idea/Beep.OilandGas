using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Ownership
{
    public partial class OwnershipTreeNode : ModelEntityBase
    {
        private System.String OwnerIdValue = string.Empty;
        /// <summary>
        /// Gets or sets the owner identifier.
        /// </summary>
        public System.String OwnerId
        {
            get => this.OwnerIdValue;
            set => SetProperty(ref OwnerIdValue, value);
        }

        private System.String OwnerNameValue = string.Empty;
        /// <summary>
        /// Gets or sets the owner name.
        /// </summary>
        public System.String OwnerName
        {
            get => this.OwnerNameValue;
            set => SetProperty(ref OwnerNameValue, value);
        }

        private System.Decimal InterestPercentageValue;
        /// <summary>
        /// Gets or sets the interest percentage (0-100).
        /// </summary>
        public System.Decimal InterestPercentage
        {
            get => this.InterestPercentageValue;
            set => SetProperty(ref InterestPercentageValue, value);
        }

        private List<OwnershipTreeNode> ChildrenValue = new List<OwnershipTreeNode>();
        /// <summary>
        /// Gets or sets the child nodes (sub-owners).
        /// </summary>
        public List<OwnershipTreeNode> Children
        {
            get => this.ChildrenValue;
            set => SetProperty(ref ChildrenValue, value);
        }

        /// <summary>
        /// Gets the total interest of all children.
        /// </summary>
        public decimal TotalChildrenInterest => Children.Sum(c => c.InterestPercentage);

        /// <summary>
        /// Gets whether this is a leaf node (no children).
        /// </summary>
        public bool IsLeaf => Children.Count == 0;
    }
}
