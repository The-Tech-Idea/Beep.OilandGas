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
    [AddinAttribute(Caption = "PPDM 3.9", misc = "Beep", BranchType = EnumPointType.Genre, FileType = "Beep", iconimage = "ppdm39.svg", menu = "PPDM39", ObjectType = "Beep", ClassType = "LJ")]
    [AddinVisSchema(BranchType = EnumPointType.Genre, BranchClass = "Genre")]
    public class PPDM39RootNode : IBranch
    {
        public string GuidID { get; set; } = Guid.NewGuid().ToString();
        public string ParentGuidID { get; set; }
        public string DataSourceConnectionGuidID { get; set; }
        public string EntityGuidID { get; set; }
        public string MiscStringID { get; set; }
        public bool Visible { get; set; } = true;
        public string MenuID { get; set; }

        public PPDM39RootNode(ITree pTreeEditor, IDMEEditor pDMEEditor, IBranch pParentNode, string pBranchText, int pID, EnumPointType pBranchType, string pimagename)
        {
            BranchText = "PPDM 3.9";
            BranchClass = "PPDM39.ROOT";
            IconImageName = "ppdm39.svg";
            BranchType = EnumPointType.Genre;
            ID = pID;
        }

        public PPDM39RootNode()
        {
            BranchText = "PPDM 3.9";
            BranchClass = "PPDM39.ROOT";
            IconImageName = "ppdm39.svg";
            BranchType = EnumPointType.Genre;
            ID = -1;
        }

        public bool IsDataSourceNode { get; set; } = true;
        public string ObjectType { get; set; } = "Beep";
        public int Order { get; set; } = 0;
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
        public string BranchText { get; set; } = "PPDM 3.9";
        public int Level { get; set; }
        public EnumPointType BranchType { get; set; } = EnumPointType.Genre;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "ppdm39.svg";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "PPDM39.GENRE";
        public IBranch ParentBranch { get; set; }
        public List<Delegate> Delegates { get; set; }

        public IBranch CreateCategoryNode(CategoryFolder p)
        {
            return null!;
        }

        public IErrorsInfo CreateChildNodes()
        {
            try
            {
                var categories = PPDM39Categories.GetAll();
                foreach (var category in categories)
                {
                    if (!ChildBranchs.Any(p => p.BranchText.Equals(category.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        CreateCategoryNode(category);
                    }
                }
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage(ex.Message, "Error Creating PPDM39 Category Nodes",
                    DateTime.Now, -1, "PPDM39RootNode.CreateChildNodes", Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }

        public IBranch CreateCategoryNode(PPDM39Category category)
        {
            PPDM39CategoryNode? categoryBranch = null;
            try
            {
                IBranch parent = this;
                string iconFile = GetCategoryIcon(category.Name);
                categoryBranch = new PPDM39CategoryNode(TreeEditor, DMEEditor, parent,
                    category.Name, TreeEditor.SeqID, EnumPointType.Category, iconFile);
                TreeEditor.Treebranchhandler.AddBranch(parent, categoryBranch);
            }
            catch (Exception ex)
            {
                DMEEditor.AddLogMessage(ex.Message, $"Error Creating Category Node {category.Name}",
                    DateTime.Now, -1, "PPDM39RootNode.CreateCategoryNode", Errors.Failed);
            }
            return categoryBranch!;
        }

        private string GetCategoryIcon(string categoryName)
        {
            return categoryName.ToLower().Replace(" / ", "-").Replace(" ", "-") switch
            {
                "additives" => "additives.svg",
                "applications" => "applications.svg",
                "areas" => "areas.svg",
                "business-associates" => "business-associates.svg",
                "classification-taxonomy" => "classification.svg",
                "consents" => "consents.svg",
                "consultations" => "consultations.svg",
                "contracts-agreements" => "contracts.svg",
                "coordinate-reference-systems" => "coordinate-reference.svg",
                "ecozones" => "ecozones.svg",
                "entitlements-security" => "entitlements.svg",
                "equipment" => "equipment.svg",
                "facilities" => "facilities.svg",
                "fields" => "fields.svg",
                "financial" => "financial.svg",
                "fossils" => "fossils.svg",
                "hse-incidents" => "hse-incidents.svg",
                "instruments" => "instruments.svg",
                "interest-sets-partnerships" => "interest-sets.svg",
                "lithology" => "lithology.svg",
                "notifications" => "notifications.svg",
                "obligations" => "obligations.svg",
                "other" => "other.svg",
                "ppdm-data-management" => "ppdm-data-management.svg",
                "paleontology" => "paleontology.svg",
                "pools" => "pools.svg",
                "production-entities" => "production-entities.svg",
                "production-lease-units" => "production-lease-units.svg",
                "production-strings" => "production-strings.svg",
                "projects" => "projects.svg",
                "rate-schedules" => "rate-schedules.svg",
                "records-management" => "records-management.svg",
                "reference-values" => "reference-values.svg",
                "reporting-hierarchies" => "reporting-hierarchies.svg",
                "reserves" => "reserves.svg",
                "restrictions-environment" => "restrictions-environment.svg",
                "sample-analysis" => "sample-analysis.svg",
                "sample-masters" => "sample-masters.svg",
                "seismic" => "seismic.svg",
                "sources" => "sources.svg",
                "spacing-units" => "spacing-units.svg",
                "spatial-descriptions" => "spatial-descriptions.svg",
                "spatial-parcels" => "spatial-parcels.svg",
                "stratigraphy" => "stratigraphy.svg",
                "substances-products" => "substances-products.svg",
                "support-facilities" => "support-facilities.svg",
                "units-of-measure" => "units-of-measure.svg",
                "volume-conversions" => "volume-conversions.svg",
                "well-logs" => "well-logs.svg",
                "wells" => "wells.svg",
                "work-orders" => "work-orders.svg",
                _ => "category.svg"
            };
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
            }
            catch (Exception ex)
            {
                string mes = "Could not Set Config";
                DMEEditor.AddLogMessage(ex.Message, mes, DateTime.Now, -1, mes, Errors.Failed);
            }
            return DMEEditor.ErrorObject;
        }
    }
}
