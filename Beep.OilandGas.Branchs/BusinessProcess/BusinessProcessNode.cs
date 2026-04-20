using TheTechIdea.Beep.Vis.Modules;
using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.Branchs.BusinessProcess
{
    /// <summary>
    /// Leaf node representing a single business process or workflow definition.
    /// <see cref="MiscStringID"/> holds the process name / process ID used to look up
    /// the corresponding <c>ProcessDefinition</c> from the LifeCycle service layer.
    /// </summary>
    [AddinAttribute(
        Caption = "BusinessProcess",
        BranchType = EnumPointType.Entity,
        Name = "BusinessProcessNode.Beep",
        misc = "Beep",
        iconimage = "process-step.svg",
        menu = "BusinessProcess",
        ObjectType = "Beep")]
    [AddinVisSchema(
        BranchType = EnumPointType.Entity,
        BranchClass = "BUSINESSPROCESS",
        RootNodeName = "BusinessProcessCategoryNode")]
    public class BusinessProcessNode : IBranch
    {
        public BusinessProcessNode()
        {
            IsDataSourceNode = false;
        }

        public BusinessProcessNode(
            ITree pTreeEditor, IDMEEditor pDMEEditor,
            IBranch pParentNode, string processName,
            int pID, string pimagename)
        {
            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            ParentBranch = pParentNode;
            ParentBranchID = pParentNode?.ID ?? -1;
            BranchText = processName;
            MiscStringID = processName;        // used to look up ProcessDefinition by name
            IconImageName = pimagename;
            BranchDescription = $"Business Process: {processName}";
            if (pID != 0) { ID = pID; BranchID = ID; }
        }

        public string MenuID { get; set; }
        public bool Visible { get; set; } = true;
        public bool IsDataSourceNode { get; set; } = false;
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        /// <summary>Process name or ID — key used to look up <c>ProcessDefinition</c>.</summary>
        public string MiscStringID { get; set; }
        public IBranch ParentBranch { get; set; }
        public string Name { get; set; }
        public EntityStructure EntityStructure { get; set; }
        public string BranchText { get; set; }
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int Level { get; set; } = 2;
        public EnumPointType BranchType { get; set; } = EnumPointType.Entity;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "process-step.svg";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "BUSINESSPROCESS";
        public List<IBranch> ChildBranchs { get; set; } = new List<IBranch>();
        public ITree TreeEditor { get; set; }
        public List<string> BranchActions { get; set; } = new List<string>
        {
            "View Process Definition",
            "Start New Process Instance",
            "View Active Instances",
            "View Process History",
            "Edit Process Definition",
        };
        public List<Delegate> Delegates { get; set; }
        public int ID { get; set; }
        public int Order { get; set; } = 0;
        public int MiscID { get; set; }
        public IAppManager Visutil { get; set; }
        public object TreeStrucure { get; set; }
        public string ObjectType { get; set; } = "Beep";

        // Leaf node — no children to create
        public IErrorsInfo CreateChildNodes() => DMEEditor?.ErrorObject!;

        public IBranch CreateCategoryNode(CategoryFolder p) => null!;

        public IErrorsInfo ExecuteBranchAction(string ActionName)
        {
            try
            {
                // Actions are dispatched by the host UI; log intent here for diagnostics
                DMEEditor?.AddLogMessage(
                    $"BusinessProcessNode action: {ActionName} on process '{MiscStringID}'",
                    "BusinessProcessNode.ExecuteBranchAction",
                    DateTime.Now, ID, MiscStringID, Errors.Ok);
            }
            catch (Exception ex)
            {
                DMEEditor?.AddLogMessage(ex.Message,
                    $"Error executing action {ActionName} on process '{MiscStringID}'",
                    DateTime.Now, -1, "BusinessProcessNode.ExecuteBranchAction", Errors.Failed);
            }
            return DMEEditor?.ErrorObject!;
        }

        public IErrorsInfo MenuItemClicked(string ActionName) => ExecuteBranchAction(ActionName);

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
                BranchText = pBranchText;
                MiscStringID = pBranchText;
                BranchType = pBranchType;
                IconImageName = pimagename;
                if (pID != 0) { ID = pID; BranchID = ID; }
            }
            catch (Exception ex)
            {
                DMEEditor?.AddLogMessage(ex.Message, "Could not Set Config",
                    DateTime.Now, -1, "BusinessProcessNode.SetConfig", Errors.Failed);
            }
            return DMEEditor?.ErrorObject!;
        }
    }
}
