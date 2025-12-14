using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.Core.Tree
{
    /// <summary>
    /// Tree node for displaying PPDM 3.9 data model hierarchy
    /// UI-framework agnostic - can be used in WinForms, WPF, Blazor, etc.
    /// </summary>
    public class PPDMTreeNode
    {
        /// <summary>
        /// Unique identifier for this node
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display text for the node
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Tooltip or description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Node type: Module, Table, Relationship, etc.
        /// </summary>
        public PPDMTreeNodeType NodeType { get; set; }

        /// <summary>
        /// Icon/image key for UI frameworks
        /// </summary>
        public string IconKey { get; set; }

        /// <summary>
        /// Additional data associated with the node
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Child nodes
        /// </summary>
        public List<PPDMTreeNode> Children { get; set; } = new List<PPDMTreeNode>();

        /// <summary>
        /// Parent node (null for root)
        /// </summary>
        public PPDMTreeNode Parent { get; set; }

        /// <summary>
        /// Whether this node is expanded (for UI state)
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Whether this node is selected (for UI state)
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Whether this node is visible (for filtering)
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Tag for custom UI framework data
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Adds a child node
        /// </summary>
        public PPDMTreeNode AddChild(PPDMTreeNode child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            child.Parent = this;
            Children.Add(child);
            return child;
        }

        /// <summary>
        /// Removes a child node
        /// </summary>
        public bool RemoveChild(PPDMTreeNode child)
        {
            if (child == null)
                return false;

            child.Parent = null;
            return Children.Remove(child);
        }

        /// <summary>
        /// Finds a child node by ID
        /// </summary>
        public PPDMTreeNode FindChildById(string id)
        {
            return FindNodeById(this, id);
        }

        /// <summary>
        /// Recursively finds a node by ID
        /// </summary>
        private static PPDMTreeNode FindNodeById(PPDMTreeNode node, string id)
        {
            if (node.Id == id)
                return node;

            foreach (var child in node.Children)
            {
                var found = FindNodeById(child, id);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Gets the full path from root to this node
        /// </summary>
        public string GetPath(string separator = " > ")
        {
            var path = new List<string>();
            var current = this;

            while (current != null)
            {
                path.Insert(0, current.Text);
                current = current.Parent;
            }

            return string.Join(separator, path);
        }

        /// <summary>
        /// Gets the depth level (0 for root)
        /// </summary>
        public int GetDepth()
        {
            int depth = 0;
            var current = Parent;
            while (current != null)
            {
                depth++;
                current = current.Parent;
            }
            return depth;
        }

        /// <summary>
        /// Flattens the tree to a list
        /// </summary>
        public List<PPDMTreeNode> Flatten()
        {
            var result = new List<PPDMTreeNode> { this };
            foreach (var child in Children)
            {
                result.AddRange(child.Flatten());
            }
            return result;
        }
    }

    /// <summary>
    /// Types of nodes in the PPDM tree
    /// </summary>
    public enum PPDMTreeNodeType
    {
        /// <summary>
        /// Root node
        /// </summary>
        Root,

        /// <summary>
        /// Subject Area/Category (e.g., "Support Modules", "Stratigraphy, Lithology & Sample Analysis")
        /// </summary>
        SubjectArea,

        /// <summary>
        /// Module/Sub-module (e.g., "Stratigraphy", "Wells", "Production")
        /// </summary>
        Module,

        /// <summary>
        /// Subcategory within a module (e.g., "Well Tests", "Well Operations", "Name Sets")
        /// </summary>
        Subcategory,

        /// <summary>
        /// Table/Entity
        /// </summary>
        Table,

        /// <summary>
        /// Foreign key relationship
        /// </summary>
        Relationship,

        /// <summary>
        /// Primary key
        /// </summary>
        PrimaryKey,

        /// <summary>
        /// Column
        /// </summary>
        Column,

        /// <summary>
        /// Common column group
        /// </summary>
        CommonColumns
    }
}

