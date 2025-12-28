using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Generates seed data templates based on workflow analysis
    /// </summary>
    public class PPDMWorkflowSeedDataGenerator
    {
        /// <summary>
        /// Gets required seed data for ProductionAccounting workflows
        /// </summary>
        public List<WorkflowSeedDataRequirement> GetAccountingWorkflowRequirements()
        {
            return new List<WorkflowSeedDataRequirement>
            {
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "AFE Management",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "AFE_STATUS", "COST_TYPE", "COST_CATEGORY", "COST_CENTER" },
                    Description = "Authorization for Expenditure management requires status, cost types, categories, and centers"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Cost Transactions",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "COST_TYPE", "COST_CATEGORY", "COST_CENTER", "COST_BASIS" },
                    Description = "Cost transaction processing requires cost classifications"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Revenue Allocation",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "REVENUE_TYPE", "ALLOCATION_METHOD", "PRODUCT_TYPE" },
                    Description = "Revenue allocation requires revenue types, allocation methods, and product types"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Journal Entries",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "GL_ACCOUNT", "ENTRY_TYPE", "ENTRY_STATUS" },
                    Description = "Journal entry processing requires GL accounts, entry types, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "General Ledger",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "GL_ACCOUNT", "ACCOUNT_TYPE", "ACCOUNT_CATEGORY" },
                    Description = "General ledger requires chart of accounts with account types and categories"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Invoice Management",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "INVOICE_TYPE", "PAYMENT_TERMS", "TAX_RATE", "INVOICE_STATUS" },
                    Description = "Invoice management requires invoice types, payment terms, tax rates, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Purchase Orders",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "PO_STATUS", "VENDOR_STATUS", "PO_TYPE" },
                    Description = "Purchase order management requires PO statuses, vendor statuses, and PO types"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Accounts Payable",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "PAYMENT_METHOD", "PAYMENT_STATUS", "VENDOR_TYPE" },
                    Description = "Accounts payable requires payment methods, payment statuses, and vendor types"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Accounts Receivable",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "PAYMENT_METHOD", "INVOICE_STATUS", "CUSTOMER_TYPE" },
                    Description = "Accounts receivable requires payment methods, invoice statuses, and customer types"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Royalty Management",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "ROYALTY_TYPE", "ROYALTY_CALCULATION_METHOD", "ROYALTY_STATUS" },
                    Description = "Royalty management requires royalty types, calculation methods, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Inventory",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "INVENTORY_TYPE", "UNIT_OF_MEASURE", "INVENTORY_STATUS" },
                    Description = "Inventory management requires inventory types, units of measure, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Tax Management",
                    WorkflowCategory = "Accounting",
                    RequiredTables = new List<string> { "TAX_TYPE", "TAX_RATE", "TAX_JURISDICTION" },
                    Description = "Tax management requires tax types, rates, and jurisdictions"
                }
            };
        }

        /// <summary>
        /// Gets required seed data for LifeCycle workflows
        /// </summary>
        public List<WorkflowSeedDataRequirement> GetLifeCycleWorkflowRequirements()
        {
            return new List<WorkflowSeedDataRequirement>
            {
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Work Orders",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "WORK_ORDER_TYPE", "WORK_ORDER_STATUS", "PRIORITY", "WORK_ORDER_CATEGORY" },
                    Description = "Work order management requires types, statuses, priorities, and categories"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Maintenance",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "MAINTENANCE_TYPE", "MAINTENANCE_STATUS", "EQUIPMENT_TYPE", "MAINTENANCE_PRIORITY" },
                    Description = "Maintenance management requires types, statuses, equipment types, and priorities"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Inspections",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "INSPECTION_TYPE", "INSPECTION_STATUS", "INSPECTION_RESULT", "INSPECTION_FREQUENCY" },
                    Description = "Inspection management requires types, statuses, results, and frequencies"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Operations",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "OPERATION_TYPE", "SHIFT_TYPE", "INCIDENT_TYPE", "OPERATION_STATUS" },
                    Description = "Operations management requires types, shift types, incident types, and statuses"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Field Management",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "FIELD_STATUS", "FIELD_TYPE", "FIELD_CLASSIFICATION" },
                    Description = "Field management requires statuses, types, and classifications"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Well Management",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "WELL_STATUS", "WELL_TYPE", "COMPLETION_TYPE", "WELL_CLASSIFICATION" },
                    Description = "Well management requires statuses, types, completion types, and classifications"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Pipeline Management",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "PIPELINE_STATUS", "PIPELINE_TYPE", "MATERIAL_TYPE", "PIPELINE_CLASSIFICATION" },
                    Description = "Pipeline management requires statuses, types, material types, and classifications"
                },
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Facility Management",
                    WorkflowCategory = "LifeCycle",
                    RequiredTables = new List<string> { "FACILITY_STATUS", "FACILITY_TYPE", "FACILITY_CLASSIFICATION" },
                    Description = "Facility management requires statuses, types, and classifications"
                }
            };
        }

        /// <summary>
        /// Gets required seed data for analysis workflows
        /// </summary>
        public List<WorkflowSeedDataRequirement> GetAnalysisWorkflowRequirements()
        {
            return new List<WorkflowSeedDataRequirement>
            {
                new WorkflowSeedDataRequirement
                {
                    WorkflowName = "Analysis Results",
                    WorkflowCategory = "Analysis",
                    RequiredTables = new List<string> { "ANALYSIS_STATUS", "ANALYSIS_TYPE", "EQUIPMENT_TYPE" },
                    Description = "Analysis result management requires statuses, types, and equipment types"
                }
            };
        }

        /// <summary>
        /// Gets required seed data for a specific workflow
        /// </summary>
        public WorkflowSeedDataRequirement? GetRequiredSeedDataForWorkflow(string workflowName)
        {
            var allRequirements = new List<WorkflowSeedDataRequirement>();
            allRequirements.AddRange(GetAccountingWorkflowRequirements());
            allRequirements.AddRange(GetLifeCycleWorkflowRequirements());
            allRequirements.AddRange(GetAnalysisWorkflowRequirements());

            return allRequirements.FirstOrDefault(r => 
                r.WorkflowName.Equals(workflowName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all unique table names required across all workflows
        /// </summary>
        public List<string> GetAllRequiredTables()
        {
            var allRequirements = new List<WorkflowSeedDataRequirement>();
            allRequirements.AddRange(GetAccountingWorkflowRequirements());
            allRequirements.AddRange(GetLifeCycleWorkflowRequirements());
            allRequirements.AddRange(GetAnalysisWorkflowRequirements());

            return allRequirements
                .SelectMany(r => r.RequiredTables)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }
    }
}

