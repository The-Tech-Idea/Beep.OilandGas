using TheTechIdea.Beep.Vis.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Addin;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.Branchs;

namespace Beep.OilandGas.Branchs.PPDM39
{
    [AddinAttribute(Caption = "PPDM39Category", BranchType = EnumPointType.Category, Name = "PPDM39CategoryNode.Beep", misc = "Beep", iconimage = "category.svg", menu = "PPDM39", ObjectType = "Beep")]
    [AddinVisSchema(BranchType = EnumPointType.Category, BranchClass = "PPDM39CATEGORY", RootNodeName = "PPDM39RootNode")]
    public class PPDM39CategoryNode : IBranch
    {
        public PPDM39CategoryNode()
        {
            IsDataSourceNode = false;
        }

        public PPDM39CategoryNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
        {
            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            ParentBranchID = pParentNode != null ? pParentNode.ID : -1;
            BranchText = pBranchText;
            BranchType = pBranchType;
            IconImageName = pimagename;
            if (pID != 0)
            {
                ID = pID;
                BranchID = ID;
            }
        }

        public string MenuID { get; set; }
        public bool Visible { get; set; } = true;
        public bool IsDataSourceNode { get; set; } = false;
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        public string MiscStringID { get; set; }
        public IBranch ParentBranch { get; set; }
        public string Name { get; set; }
        public EntityStructure EntityStructure { get; set; }
        public string BranchText { get; set; } = "PPDM39Category";
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int Level { get; set; } = 1;
        public EnumPointType BranchType { get; set; } = EnumPointType.Category;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "category.png";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "PPDM39CATEGORY";
        public List<IBranch> ChildBranchs { get; set; } = new List<IBranch>();
        public ITree TreeEditor { get; set; }
        public List<string> BranchActions { get; set; } = new List<string>();
        public List<Delegate> Delegates { get; set; }
        public int ID { get; set; }
        public int Order { get; set; } = 0;
        public int MiscID { get; set; }
        public IAppManager Visutil { get; set; }
        public object TreeStrucure { get; set; }
        public string ObjectType { get; set; } = "Beep";

        public IErrorsInfo CreateChildNodes()
        {
            try
            {
                // Resolve category ID — prefer MiscID, fall back to BranchText lookup
                int catId = MiscID;
                if (catId <= 0)
                {
                    var cat = PPDM39Categories.GetAll()
                        .FirstOrDefault(c => c.Name.Equals(BranchText, StringComparison.OrdinalIgnoreCase));
                    catId = cat?.Id ?? 0;
                }
                if (catId <= 0) return DMEEditor.ErrorObject;

              

                var tables = PPDM39TableMapping.GetTablesForCategory(catId);
                if (tables.Count == 0) return DMEEditor.ErrorObject;

                var tableSet = new HashSet<string>(tables, StringComparer.OrdinalIgnoreCase);

                // Root tables = those with no FK parents that also belong to this category
                var rootTables = tables
                    .Where(t => !PPDM39TableMapping.GetParents(t).Any(p => tableSet.Contains(p)))
                    .OrderBy(t => t)
                    .ToList();

                // Pre-populate visited with tables already present so we only add what is missing
                var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                CollectExistingTableNames(this, visited);

                int nextId = ID * 10000 + visited.Count;

                foreach (var table in rootTables)
                    AddTableNode(this, table, tableSet, ref nextId, visited, depth: 0);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage(ex.Message, "Could not create PPDM39 child nodes",
                    DateTime.Now, -1, ex.Message, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        private static void CollectExistingTableNames(IBranch branch, HashSet<string> names)
        {
            foreach (var child in branch.ChildBranchs)
            {
                if (!string.IsNullOrEmpty(child.MiscStringID))
                    names.Add(child.MiscStringID);
                CollectExistingTableNames(child, names);
            }
        }

        private void AddTableNode(IBranch parent, string tableName,
            HashSet<string> categoryTables, ref int nextId, HashSet<string> visited, int depth)
        {
            if (!visited.Add(tableName) || depth > 20) return;

            var node = new PPDM39TableNode(
                TreeEditor, DMEEditor, parent,
                tableName, nextId++, "table.svg");

            parent.ChildBranchs.Add(node);

            foreach (var child in PPDM39TableMapping.GetChildren(tableName)
                         .Where(c => categoryTables.Contains(c))
                         .OrderBy(c => c))
                AddTableNode(node, child, categoryTables, ref nextId, visited, depth + 1);
        }

        public IBranch CreateCategoryNode(CategoryFolder p)
        {
            return null!;
        }

        public IErrorsInfo ExecuteBranchAction(string ActionName)
        {
            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo MenuItemClicked(string ActionNam)
        {
            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo RemoveChildNodes()
        {
            return DMEEditor.ErrorObject;
        }

        public IErrorsInfo SetConfig(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
        {
            try
            {
                TreeEditor = pTreeEditor;
                DMEEditor = pDMEEditor;
                ParentBranch = pParentNode;
                ParentBranchID = pParentNode != null ? pParentNode.ID : -1;
                BranchText = pBranchText;
                BranchType = pBranchType;
                IconImageName = pimagename;
                if (pID != 0)
                {
                    ID = pID;
                    BranchID = ID;
                }
            }
            catch (Exception ex)
            {
                string mes = "Could not Set Config";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }
        //[CommandAttribute(Caption = "Data Edit", PointType = EnumPointType.Entity, iconimage = "editentity.png")]
        //public IErrorsInfo EditData()
        //{
        //    int resolvedCatId = MiscID > 0 ? MiscID
        //        : PPDM39Categories.GetAll()
        //            .FirstOrDefault(c => c.Name.Equals(BranchText, StringComparison.OrdinalIgnoreCase))?.Id ?? 0;

        //    // Reference Values (id 33) has too many tables — open the dedicated editor page instead
        //    if (resolvedCatId == 33)
        //    {
        //        Visutil?.ShowPage("showreferenceeditor", new PassedArgs
        //        {
        //            CurrentEntity = BranchText,
        //            Id = ID,
        //            ObjectType = "PPDM39CATEGORY",
        //            ObjectName = BranchText,
        //            EventType = "REFERENCEEDITOR"
        //        }, DisplayType.InControl, true);
              
        //    }
        //    return DMEEditor.ErrorObject;
        //}
    }
}
