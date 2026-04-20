using TheTechIdea.Beep.Vis.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.Branchs.BusinessProcess
{
    /// <summary>
    /// Root genre node for the Business Process / Workflow tree branch.
    /// </summary>
    [AddinAttribute(
        Caption = "Business Processes",
        misc = "Beep",
        BranchType = EnumPointType.Genre,
        FileType = "Beep",
        iconimage = "business-process.svg",
        menu = "BusinessProcess",
        ObjectType = "Beep",
        ClassType = "LJ")]
    [AddinVisSchema(BranchType = EnumPointType.Genre, BranchClass = "Genre")]
    public class BusinessProcessRootNode : IBranch
    {
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        public string MiscStringID { get; set; }
        public bool Visible { get; set; } = true;
        public string MenuID { get; set; }

        public BusinessProcessRootNode(
            ITree pTreeEditor, IDMEEditor pDMEEditor,
            IBranch pParentNode, string pBranchText,
            int pID, EnumPointType pBranchType, string pimagename)
        {
            BranchText = "Business Processes";
            BranchClass = "BUSINESSPROCESS.ROOT";
            IconImageName = "business-process.svg";
            BranchType = EnumPointType.Genre;
            ID = pID;
        }

        public BusinessProcessRootNode()
        {
            BranchText = "Business Processes";
            BranchClass = "BUSINESSPROCESS.ROOT";
            IconImageName = "business-process.svg";
            BranchType = EnumPointType.Genre;
            ID = -1;
        }

        public bool IsDataSourceNode { get; set; } = false;
        public string ObjectType { get; set; } = "Beep";
        public int Order { get; set; } = 1;
        public object TreeStrucure { get; set; }
        public IAppManager Visutil { get; set; }
        public int ID { get; set; } = -1;
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public List<IBranch> ChildBranchs { get; set; } = new List<IBranch>();
        public ITree TreeEditor { get; set; }
        public List<string> BranchActions { get; set; } = new List<string>();
        public EntityStructure EntityStructure { get; set; }
        public int MiscID { get; set; }
        public string Name { get; set; }
        public string BranchText { get; set; } = "Business Processes";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Genre;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "business-process.svg";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; } = "Oil & Gas business process and workflow management";
        public string BranchClass { get; set; } = "BUSINESSPROCESS.GENRE";
        public IBranch ParentBranch { get; set; }
        public List<Delegate> Delegates { get; set; }

        public IBranch CreateCategoryNode(CategoryFolder p) => null!;

        public IErrorsInfo CreateChildNodes()
        {
            try
            {
                var categories = BusinessProcessCategories.GetAll();
                foreach (var category in categories)
                {
                    if (!ChildBranchs.Any(b => b.BranchText.Equals(category.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        CreateCategoryNode(category);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor?.AddLogMessage(ex.Message,
                    "Error creating Business Process category nodes",
                    DateTime.Now, -1, "BusinessProcessRootNode.CreateChildNodes", Errors.Failed);
            }
            return DMEEditor?.ErrorObject!;
        }

        public IBranch CreateCategoryNode(BusinessProcessCategory category)
        {
            BusinessProcessCategoryNode? node = null;
            try
            {
                string icon = GetCategoryIcon(category.Name);
                node = new BusinessProcessCategoryNode(
                    TreeEditor, DMEEditor, this,
                    category.Name, TreeEditor.SeqID,
                    EnumPointType.Category, icon);
                node.MiscID = category.Id;
                TreeEditor.Treebranchhandler.AddBranch(this, node);
            }
            catch (Exception ex)
            {
                DMEEditor?.AddLogMessage(ex.Message,
                    $"Error creating BusinessProcess category node: {category.Name}",
                    DateTime.Now, -1, "BusinessProcessRootNode.CreateCategoryNode", Errors.Failed);
            }
            return node!;
        }

        private static string GetCategoryIcon(string categoryName) =>
            categoryName.ToLowerInvariant().Replace(" & ", "-").Replace(" / ", "-").Replace(" ", "-") switch
            {
                "exploration-workflows"          => "exploration-workflow.svg",
                "development-workflows"          => "development-workflow.svg",
                "production-workflows"           => "production-workflow.svg",
                "decommissioning-workflows"      => "decommissioning-workflow.svg",
                "work-order-workflows"           => "work-order-workflow.svg",
                "approval-gate-reviews"          => "approval-gate.svg",
                "hse-safety-workflows"           => "hse-workflow.svg",
                "compliance-regulatory"          => "compliance-workflow.svg",
                "well-lifecycle-workflows"       => "well-lifecycle.svg",
                "facility-lifecycle-workflows"   => "facility-lifecycle.svg",
                "reservoir-management-workflows" => "reservoir-workflow.svg",
                "pipeline-infrastructure"        => "pipeline-workflow.svg",
                _                                => "business-process.svg"
            };

        public IErrorsInfo ExecuteBranchAction(string ActionName) => DMEEditor?.ErrorObject!;
        public IErrorsInfo MenuItemClicked(string ActionName) => DMEEditor?.ErrorObject!;
        public IErrorsInfo RemoveChildNodes() => DMEEditor?.ErrorObject!;

        public IErrorsInfo SetConfig(
            ITree pTreeEditor, IDMEEditor pDMEEditor,
            IBranch pParentNode, string pBranchText,
            int pID, EnumPointType pBranchType, string pimagename)
        {
            try
            {
                TreeEditor = pTreeEditor;
                DMEEditor = pDMEEditor;
                ParentBranch = pParentNode;
                ParentBranchID = pParentNode?.ID ?? -1;
                BranchType = pBranchType;
                IconImageName = pimagename;
                if (pID != 0) { ID = pID; BranchID = ID; }
            }
            catch (Exception ex)
            {
                DMEEditor?.AddLogMessage(ex.Message, "Could not Set Config",
                    DateTime.Now, -1, "BusinessProcessRootNode.SetConfig", Errors.Failed);
            }
            return DMEEditor?.ErrorObject!;
        }
    }
}
