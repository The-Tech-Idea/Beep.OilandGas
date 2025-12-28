using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Catalog of all seed data organized by category
    /// </summary>
    public class PPDMSeedDataCatalog
    {
        private readonly PPDMWorkflowSeedDataGenerator _workflowGenerator;

        public PPDMSeedDataCatalog()
        {
            _workflowGenerator = new PPDMWorkflowSeedDataGenerator();
        }

        /// <summary>
        /// Gets all available seed data categories
        /// </summary>
        public List<SeedDataCategory> GetSeedDataCategories()
        {
            var categories = new List<SeedDataCategory>();

            // PPDM Reference Tables
            categories.Add(new SeedDataCategory
            {
                CategoryName = "PPDM",
                Description = "PPDM standard reference tables (R_* tables)",
                TableNames = GetPPDMReferenceTables(),
                EstimatedRecords = 500
            });

            // Accounting
            var accountingTables = _workflowGenerator.GetAccountingWorkflowRequirements()
                .SelectMany(r => r.RequiredTables)
                .Distinct()
                .ToList();
            categories.Add(new SeedDataCategory
            {
                CategoryName = "Accounting",
                Description = "Seed data for ProductionAccounting workflows",
                TableNames = accountingTables,
                EstimatedRecords = 200
            });

            // LifeCycle
            var lifecycleTables = _workflowGenerator.GetLifeCycleWorkflowRequirements()
                .SelectMany(r => r.RequiredTables)
                .Distinct()
                .ToList();
            categories.Add(new SeedDataCategory
            {
                CategoryName = "LifeCycle",
                Description = "Seed data for LifeCycle workflows",
                TableNames = lifecycleTables,
                EstimatedRecords = 150
            });

            // Analysis
            var analysisTables = _workflowGenerator.GetAnalysisWorkflowRequirements()
                .SelectMany(r => r.RequiredTables)
                .Distinct()
                .ToList();
            categories.Add(new SeedDataCategory
            {
                CategoryName = "Analysis",
                Description = "Seed data for analysis modules",
                TableNames = analysisTables,
                EstimatedRecords = 50
            });

            return categories;
        }

        /// <summary>
        /// Gets seed data requirements for a specific category
        /// </summary>
        public SeedDataCategory? GetSeedDataForCategory(string category)
        {
            return GetSeedDataCategories()
                .FirstOrDefault(c => c.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets seed data requirements for a specific workflow
        /// </summary>
        public WorkflowSeedDataRequirement? GetSeedDataForWorkflow(string workflowName)
        {
            return _workflowGenerator.GetRequiredSeedDataForWorkflow(workflowName);
        }

        /// <summary>
        /// Gets all workflows that require a specific table
        /// </summary>
        public List<WorkflowSeedDataRequirement> GetWorkflowsRequiringTable(string tableName)
        {
            var allRequirements = new List<WorkflowSeedDataRequirement>();
            allRequirements.AddRange(_workflowGenerator.GetAccountingWorkflowRequirements());
            allRequirements.AddRange(_workflowGenerator.GetLifeCycleWorkflowRequirements());
            allRequirements.AddRange(_workflowGenerator.GetAnalysisWorkflowRequirements());

            return allRequirements
                .Where(r => r.RequiredTables.Any(t => t.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        /// <summary>
        /// Gets standard PPDM reference tables (R_* tables)
        /// </summary>
        private List<string> GetPPDMReferenceTables()
        {
            return new List<string>
            {
                "R_WELL_STATUS",
                "R_FIELD_STATUS",
                "R_FACILITY_STATUS",
                "R_PIPELINE_STATUS",
                "R_PROPERTY_STATUS",
                "R_COST_TYPE",
                "R_ACCOUNTING_METHOD",
                "R_UNIT_OF_MEASURE",
                "R_ROW_QUALITY",
                "R_SOURCE",
                "R_WORK_ORDER_TYPE",
                "R_WORK_ORDER_STATUS",
                "R_MAINTENANCE_TYPE",
                "R_INSPECTION_TYPE",
                "R_INVOICE_TYPE",
                "R_PAYMENT_METHOD",
                "R_GL_ACCOUNT_TYPE",
                "R_PRODUCT_TYPE",
                "R_LEASE_STATUS",
                "R_LEASE_TYPE",
                "R_OWNERSHIP_TYPE",
                "R_REVENUE_TYPE",
                "R_TAX_TYPE",
                "R_ROYALTY_TYPE"
            };
        }
    }
}

