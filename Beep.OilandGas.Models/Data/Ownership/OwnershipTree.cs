using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.Models.Data.Ownership
{
    /// <summary>
    /// Represents an ownership tree node.
    /// </summary>
    public class OwnershipTreeNode
    {
        /// <summary>
        /// Gets or sets the owner identifier.
        /// </summary>
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the owner name.
        /// </summary>
        public string OwnerName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the interest percentage (0-100).
        /// </summary>
        public decimal InterestPercentage { get; set; }

        /// <summary>
        /// Gets or sets the child nodes (sub-owners).
        /// </summary>
        public List<OwnershipTreeNode> Children { get; set; } = new();

        /// <summary>
        /// Gets the total interest of all children.
        /// </summary>
        public decimal TotalChildrenInterest => Children.Sum(c => c.InterestPercentage);

        /// <summary>
        /// Gets whether this is a leaf node (no children).
        /// </summary>
        public bool IsLeaf => Children.Count == 0;
    }

    /// <summary>
    /// Represents an ownership tree for hierarchical ownership structures.
    /// </summary>
    public class OwnershipTree
    {
        /// <summary>
        /// Gets or sets the root node.
        /// </summary>
        public OwnershipTreeNode Root { get; set; } = new();

        /// <summary>
        /// Gets or sets the property or lease identifier.
        /// </summary>
        public string PropertyOrLeaseId { get; set; } = string.Empty;

        /// <summary>
        /// Gets all leaf nodes (final owners).
        /// </summary>
        public List<OwnershipTreeNode> GetLeafNodes()
        {
            var leafNodes = new List<OwnershipTreeNode>();
            CollectLeafNodes(Root, leafNodes);
            return leafNodes;
        }

        /// <summary>
        /// Collects leaf nodes recursively.
        /// </summary>
        private void CollectLeafNodes(OwnershipTreeNode node, List<OwnershipTreeNode> leafNodes)
        {
            if (node.IsLeaf)
            {
                leafNodes.Add(node);
            }
            else
            {
                foreach (var child in node.Children)
                {
                    CollectLeafNodes(child, leafNodes);
                }
            }
        }

        /// <summary>
        /// Calculates effective interest for a leaf owner.
        /// </summary>
        public decimal CalculateEffectiveInterest(string ownerId)
        {
            var leafNode = FindNode(ownerId);
            if (leafNode == null || !leafNode.IsLeaf)
                return 0;

            // Calculate path from root to leaf
            decimal effectiveInterest = 1.0m;
            var path = FindPathToNode(ownerId);
            
            foreach (var node in path)
            {
                effectiveInterest *= (node.InterestPercentage / 100m);
            }

            return effectiveInterest;
        }

        /// <summary>
        /// Finds a node by owner ID.
        /// </summary>
        private OwnershipTreeNode? FindNode(string ownerId)
        {
            return FindNodeRecursive(Root, ownerId);
        }

        /// <summary>
        /// Finds a node recursively.
        /// </summary>
        private OwnershipTreeNode? FindNodeRecursive(OwnershipTreeNode node, string ownerId)
        {
            if (node.OwnerId == ownerId)
                return node;

            foreach (var child in node.Children)
            {
                var found = FindNodeRecursive(child, ownerId);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Finds the path from root to a node.
        /// </summary>
        private List<OwnershipTreeNode> FindPathToNode(string ownerId)
        {
            var path = new List<OwnershipTreeNode>();
            FindPathRecursive(Root, ownerId, path);
            return path;
        }

        /// <summary>
        /// Finds path recursively.
        /// </summary>
        private bool FindPathRecursive(OwnershipTreeNode node, string ownerId, List<OwnershipTreeNode> path)
        {
            path.Add(node);

            if (node.OwnerId == ownerId)
                return true;

            foreach (var child in node.Children)
            {
                if (FindPathRecursive(child, ownerId, path))
                    return true;
            }

            path.RemoveAt(path.Count - 1);
            return false;
        }

        /// <summary>
        /// Validates that all interests sum to 100%.
        /// </summary>
        public bool ValidateInterests()
        {
            return ValidateNodeInterests(Root);
        }

        /// <summary>
        /// Validates node interests recursively.
        /// </summary>
        private bool ValidateNodeInterests(OwnershipTreeNode node)
        {
            if (node.IsLeaf)
                return true;

            decimal total = node.Children.Sum(c => c.InterestPercentage);
            if (Math.Abs(total - 100m) > 0.01m)
                return false;

            foreach (var child in node.Children)
            {
                if (!ValidateNodeInterests(child))
                    return false;
            }

            return true;
        }
    }
}

