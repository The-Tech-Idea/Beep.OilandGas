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
    /// Category node grouping individual workflow/process nodes under a Business Process category
    /// (e.g. "Exploration Workflows", "Production Workflows").
    /// </summary>
    [AddinAttribute(
        Caption = "BusinessProcessCategory",
        BranchType = EnumPointType.Category,
        Name = "BusinessProcessCategoryNode.Beep",
        misc = "Beep",
        iconimage = "business-process.svg",
        menu = "BusinessProcess",
        ObjectType = "Beep")]
    [AddinVisSchema(
        BranchType = EnumPointType.Category,
        BranchClass = "BUSINESSPROCESSCATEGORY",
        RootNodeName = "BusinessProcessRootNode")]
    public class BusinessProcessCategoryNode : IBranch
    {
        public BusinessProcessCategoryNode()
        {
            IsDataSourceNode = false;
        }

        public BusinessProcessCategoryNode(
            ITree pTreeEditor, IDMEEditor pDMEEditor,
            IBranch pParentNode, string pBranchText,
            int pID, EnumPointType pBranchType, string pimagename)
        {
            TreeEditor = pTreeEditor;
            DMEEditor = pDMEEditor;
            ParentBranch = pParentNode;
            ParentBranchID = pParentNode?.ID ?? -1;
            BranchText = pBranchText;
            BranchType = pBranchType;
            IconImageName = pimagename;
            if (pID != 0) { ID = pID; BranchID = ID; }
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
        public string BranchText { get; set; } = "BusinessProcessCategory";
        public IDMEEditor DMEEditor { get; set; }
        public IDataSource DataSource { get; set; }
        public string DataSourceName { get; set; }
        public int Level { get; set; } = 1;
        public EnumPointType BranchType { get; set; } = EnumPointType.Category;
        public int BranchID { get; set; }
        public string IconImageName { get; set; } = "business-process.svg";
        public string BranchStatus { get; set; }
        public int ParentBranchID { get; set; }
        public string BranchDescription { get; set; }
        public string BranchClass { get; set; } = "BUSINESSPROCESSCATEGORY";
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
                var processNames = GetProcessNamesForCategory(MiscID);
                if (processNames.Count == 0) return DMEEditor?.ErrorObject;

                var existingNames = new HashSet<string>(
                    ChildBranchs.Select(b => b.BranchText),
                    StringComparer.OrdinalIgnoreCase);

                int nextId = ID * 1000;
                foreach (var name in processNames)
                {
                    if (existingNames.Contains(name)) continue;
                    var node = new BusinessProcessNode(
                        TreeEditor, DMEEditor, this,
                        name, nextId++, "process-step.svg");
                    ChildBranchs.Add(node);
                }
            }
            catch (Exception ex)
            {
                DMEEditor?.AddLogMessage(ex.Message,
                    $"Error creating BusinessProcess child nodes for {BranchText}",
                    DateTime.Now, -1, "BusinessProcessCategoryNode.CreateChildNodes", Errors.Failed);
            }
            return DMEEditor?.ErrorObject;
        }

        /// <summary>
        /// Returns the canonical list of workflow/process names that belong to each category.
        /// These names match the <see cref="ProcessDefinition.ProcessName"/> values seeded
        /// by <c>ProcessDefinitionInitializer</c> in the LifeCycle layer.
        /// </summary>
        private static List<string> GetProcessNamesForCategory(int categoryId) => categoryId switch
        {
            1 => new List<string>   // Exploration Workflows
            {
                "Lead-to-Prospect Assessment",
                "Prospect-to-Discovery Evaluation",
                "Discovery-to-Development Sanction",
                "Seismic Acquisition Workflow",
                "Exploration Well Planning",
                "Exploration Well Spud-to-TD",
                "G&G Study Completion",
                "Exploration License Renewal",
            },
            2 => new List<string>   // Development Workflows
            {
                "Pool Definition & Appraisal",
                "Field Development Plan Approval",
                "Facility Design & FEED",
                "Development Well Drilling & Completion",
                "Pipeline Design & Construction",
                "Tie-in to Production System",
                "First-Oil / First-Gas Readiness",
                "Reservoir Simulation Study",
            },
            3 => new List<string>   // Production Workflows
            {
                "Well Start-up & Commissioning",
                "Production Operations Routine",
                "Decline Curve Management",
                "Workover & Intervention",
                "Well Optimization Review",
                "Production Allocation Cycle",
                "Artificial Lift Change-out",
                "Emergency Shutdown Response",
            },
            4 => new List<string>   // Decommissioning Workflows
            {
                "Well P&A (Plug & Abandon)",
                "Facility Decommissioning & Removal",
                "Pipeline Decommissioning",
                "Site Remediation & Environmental Closure",
                "Decommissioning Cost Estimate Approval",
                "Regulatory Decommissioning Notification",
            },
            5 => new List<string>   // Work Order Workflows
            {
                "Preventive Maintenance Work Order",
                "Corrective Maintenance Work Order",
                "Inspection & Surveillance Work Order",
                "Shutdown & Turnaround Work Order",
                "Emergency Repair Work Order",
                "Capital Project Work Order",
            },
            6 => new List<string>   // Approval & Gate Reviews
            {
                "Stage-Gate 0: Opportunity Identification",
                "Stage-Gate 1: Concept Selection",
                "Stage-Gate 2: Pre-FEED Approval",
                "Stage-Gate 3: FEED / FID Approval",
                "Stage-Gate 4: Execution Approval",
                "Stage-Gate 5: Operations Readiness",
                "Management of Change (MOC) Approval",
                "Expenditure Approval (AFE)",
            },
            7 => new List<string>   // HSE & Safety Workflows
            {
                "HAZOP / HAZID Study",
                "Risk Assessment & Bowtie",
                "Incident Investigation (Tier 1-3)",
                "SIMOPS Coordination",
                "Emergency Response Drill",
                "Safety Critical Element Review",
                "Environmental Impact Assessment",
                "Near-Miss Reporting & Close-out",
            },
            8 => new List<string>   // Compliance & Regulatory
            {
                "Well Permit Application",
                "Facility Permit Renewal",
                "Regulatory Production Report",
                "Environmental Compliance Audit",
                "License Obligation Tracking",
                "Government Royalty Reporting",
                "Export Terminal Certification",
                "Emissions Reporting (GHG)",
            },
            9 => new List<string>   // Well Lifecycle Workflows
            {
                "Well Design & Engineering",
                "Well Spud Authorization",
                "Drilling & Completion Operations",
                "Well Tie-in & Commissioning",
                "Production Phase Operations",
                "Well Workover / Recompletion",
                "Well Suspension (Long-term)",
                "Well P&A & Site Restoration",
            },
            10 => new List<string>  // Facility Lifecycle Workflows
            {
                "Facility Concept & Screening",
                "Facility FEED & Basic Engineering",
                "Procurement & Construction",
                "Pre-Start-up Safety Review (PSSR)",
                "Facility Commissioning & Start-up",
                "Facility Operations & Maintenance",
                "Facility Modification & Upgrade",
                "Facility Shutdown & Decommissioning",
            },
            11 => new List<string>  // Reservoir Management Workflows
            {
                "Annual Reservoir Performance Review",
                "Reserves Certification Cycle",
                "IOR / EOR Screening & Pilot",
                "Pressure Maintenance Decision",
                "Infill Drilling Review",
                "Pool Reclassification",
                "Reservoir Simulation History Match",
                "Secondary / Tertiary Recovery Sanction",
            },
            12 => new List<string>  // Pipeline & Infrastructure Workflows
            {
                "Pipeline Integrity Assessment",
                "Corrosion Inhibition Programme",
                "Pigging Campaign",
                "Pipeline Tie-in Authorization",
                "Pipeline Expansion / Looping",
                "Emergency Pipeline Repair",
                "Pipeline Pressure Test",
                "Pipeline Abandonment",
            },
            _ => new List<string>()
        };

        public IBranch CreateCategoryNode(CategoryFolder p) => null;
        public IErrorsInfo ExecuteBranchAction(string ActionName) => DMEEditor?.ErrorObject;
        public IErrorsInfo MenuItemClicked(string ActionName) => DMEEditor?.ErrorObject;
        public IErrorsInfo RemoveChildNodes() => DMEEditor?.ErrorObject;

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
                BranchType = pBranchType;
                IconImageName = pimagename;
                if (pID != 0) { ID = pID; BranchID = ID; }
            }
            catch (Exception ex)
            {
                DMEEditor?.AddLogMessage(ex.Message, "Could not Set Config",
                    DateTime.Now, -1, "BusinessProcessCategoryNode.SetConfig", Errors.Failed);
            }
            return DMEEditor?.ErrorObject;
        }
    }
}
