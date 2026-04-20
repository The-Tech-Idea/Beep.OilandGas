using TheTechIdea.Beep.Vis.Modules;
using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Addin;
using TheTechIdea;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Vis;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.Branchs.PPDM39
{
    [AddinAttribute(Caption = "PPDM39Table", BranchType = EnumPointType.Entity, Name = "PPDM39TableNode.Beep", misc = "Beep", iconimage = "table.svg", menu = "PPDM39", ObjectType = "Beep")]
    [AddinVisSchema(BranchType = EnumPointType.Entity, BranchClass = "PPDM39TABLE", RootNodeName = "PPDM39CategoryNode")]
    public class PPDM39TableNode : IBranch
    {
        public PPDM39TableNode()
        {
            IsDataSourceNode = false;
        }

        public PPDM39TableNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, string tableName, int pID, string pimagename)
        {
            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            ParentBranch = pParentNode;
            ParentBranchID = pParentNode != null ? pParentNode.ID : -1;
            BranchText = tableName;
            MiscStringID = tableName;
            IconImageName = pimagename;
            BranchDescription = $"PPDM39 Table: {tableName}";
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
        public string BranchText { get; set; }
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int Level { get; set; } = 1;
        public EnumPointType BranchType { get; set; } = EnumPointType.Entity;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "table.svg";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "PPDM39TABLE";
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
            return DMEEditor.ErrorObject;
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
                MiscStringID = pBranchText;
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

        [CommandAttribute(Caption = "Edit Data", PointType = EnumPointType.Entity, iconimage = "editentity.png")]
        public IErrorsInfo EditData()
        {
            Visutil?.ShowPage("showtableeditor", new PassedArgs
            {
                CurrentEntity = BranchText,
                Id = ID,
                ObjectType = "PPDM39TABLE",
                ObjectName = BranchText,
                EventType = "TABLEEDITOR"
            }, DisplayType.InControl, false);
            return DMEEditor.ErrorObject;
        }
    }
}
