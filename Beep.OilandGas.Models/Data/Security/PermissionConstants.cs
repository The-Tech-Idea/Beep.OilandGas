namespace Beep.OilandGas.Models.Data.Security
{
    /// <summary>
    /// Permission constants for all Oil & Gas domains.
    /// Format: Domain.Action (e.g., Production.View, WellManagement.Edit).
    /// Aligned with RBAC roles seeded in DefaultSecuritySeedService.
    /// </summary>
    public static class PermissionConstants
    {
        // ─────────────────────────────────────────────────────────────────
        // Cross-Cutting / Platform Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Admin
        {
            public const string ManageUsers = "Admin.ManageUsers";
            public const string AssignRoles = "Admin.AssignRoles";
            public const string ViewAuditLogs = "Admin.ViewAuditLogs";
            public const string ConfigureSystem = "Admin.ConfigureSystem";
            public const string ManageTenants = "Admin.ManageTenants";
            public const string ViewSystemHealth = "Admin.ViewSystemHealth";
        }

        public static class Security
        {
            public const string ManagePermissions = "Security.ManagePermissions";
            public const string ManageRoles = "Security.ManageRoles";
            public const string ViewAccessLogs = "Security.ViewAccessLogs";
            public const string ManagePolicies = "Security.ManagePolicies";
            public const string EmergencyAccess = "Security.EmergencyAccess";
        }

        public static class DataManagement
        {
            public const string ImportData = "DataManagement.ImportData";
            public const string ExportData = "DataManagement.ExportData";
            public const string ValidateData = "DataManagement.ValidateData";
            public const string ApproveData = "DataManagement.ApproveData";
            public const string DeleteData = "DataManagement.DeleteData";
        }

        // ─────────────────────────────────────────────────────────────────
        // Accounting & Financial Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Accounting
        {
            public const string View = "Accounting.View";
            public const string PostJournal = "Accounting.PostJournal";
            public const string ApproveJournal = "Accounting.ApproveJournal";
            public const string EditSettings = "Accounting.EditSettings";
            public const string ViewReports = "Accounting.ViewReports";
            public const string ManagePeriods = "Accounting.ManagePeriods";
        }

        public static class Tax
        {
            public const string ViewProvision = "Tax.ViewProvision";
            public const string Calculate = "Tax.Calculate";
            public const string Adjust = "Tax.Adjust";
            public const string SubmitReturns = "Tax.SubmitReturns";
        }

        public static class EconomicAnalysis
        {
            public const string View = "EconomicAnalysis.View";
            public const string Create = "EconomicAnalysis.Create";
            public const string Edit = "EconomicAnalysis.Edit";
            public const string Approve = "EconomicAnalysis.Approve";
            public const string RunNPV = "EconomicAnalysis.RunNPV";
            public const string RunIRR = "EconomicAnalysis.RunIRR";
        }

        // ─────────────────────────────────────────────────────────────────
        // Well Management Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class WellManagement
        {
            public const string View = "WellManagement.View";
            public const string Create = "WellManagement.Create";
            public const string Edit = "WellManagement.Edit";
            public const string Delete = "WellManagement.Delete";
            public const string Approve = "WellManagement.Approve";
            public const string ViewWellStatus = "WellManagement.ViewWellStatus";
            public const string UpdateWellStatus = "WellManagement.UpdateWellStatus";
            public const string ViewWellStructures = "WellManagement.ViewWellStructures";
            public const string ManageWellbore = "WellManagement.ManageWellbore";
            public const string ManageCompletion = "WellManagement.ManageCompletion";
        }

        // ─────────────────────────────────────────────────────────────────
        // Production Operations Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Production
        {
            public const string View = "Production.View";
            public const string Create = "Production.Create";
            public const string Edit = "Production.Edit";
            public const string Approve = "Production.Approve";
            public const string Delete = "Production.Delete";
            public const string ViewDaily = "Production.ViewDaily";
            public const string ViewMonthly = "Production.ViewMonthly";
            public const string SubmitProduction = "Production.SubmitProduction";
            public const string AllocateProduction = "Production.AllocateProduction";
            public const string ViewDeclineCurves = "Production.ViewDeclineCurves";
        }

        public static class ProductionForecasting
        {
            public const string View = "ProductionForecasting.View";
            public const string Create = "ProductionForecasting.Create";
            public const string Edit = "ProductionForecasting.Edit";
            public const string RunDCA = "ProductionForecasting.RunDCA";
            public const string Approve = "ProductionForecasting.Approve";
        }

        public static class ProductionAccounting
        {
            public const string View = "ProductionAccounting.View";
            public const string Reconcile = "ProductionAccounting.Reconcile";
            public const string Allocate = "ProductionAccounting.Allocate";
            public const string Adjust = "ProductionAccounting.Adjust";
            public const string Approve = "ProductionAccounting.Approve";
        }

        // ─────────────────────────────────────────────────────────────────
        // Reservoir Engineering Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Reservoir
        {
            public const string View = "Reservoir.View";
            public const string Create = "Reservoir.Create";
            public const string Edit = "Reservoir.Edit";
            public const string Approve = "Reservoir.Approve";
            public const string ViewMaterialBalance = "Reservoir.ViewMaterialBalance";
            public const string RunSimulation = "Reservoir.RunSimulation";
            public const string ViewReserves = "Reservoir.ViewReserves";
            public const string UpdateReserves = "Reservoir.UpdateReserves";
        }

        // ─────────────────────────────────────────────────────────────────
        // Drilling & Completions Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Drilling
        {
            public const string View = "Drilling.View";
            public const string Create = "Drilling.Create";
            public const string Edit = "Drilling.Edit";
            public const string Approve = "Drilling.Approve";
            public const string ViewDailyReports = "Drilling.ViewDailyReports";
            public const string ManageWellProgram = "Drilling.ManageWellProgram";
            public const string ViewCostTracking = "Drilling.ViewCostTracking";
        }

        public static class Completions
        {
            public const string View = "Completions.View";
            public const string Create = "Completions.Create";
            public const string Edit = "Completions.Edit";
            public const string Approve = "Completions.Approve";
            public const string DesignCompletion = "Completions.DesignCompletion";
        }

        // ─────────────────────────────────────────────────────────────────
        // Facilities & Pipeline Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Facilities
        {
            public const string View = "Facilities.View";
            public const string Create = "Facilities.Create";
            public const string Edit = "Facilities.Edit";
            public const string Approve = "Facilities.Approve";
            public const string ViewEquipment = "Facilities.ViewEquipment";
            public const string ManageEquipment = "Facilities.ManageEquipment";
            public const string ViewMaintenance = "Facilities.ViewMaintenance";
        }

        public static class Pipeline
        {
            public const string View = "Pipeline.View";
            public const string Create = "Pipeline.Create";
            public const string Edit = "Pipeline.Edit";
            public const string Approve = "Pipeline.Approve";
            public const string RunHydraulics = "Pipeline.RunHydraulics";
            public const string ViewCapacity = "Pipeline.ViewCapacity";
        }

        // ─────────────────────────────────────────────────────────────────
        // Engineering Analysis Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class NodalAnalysis
        {
            public const string View = "NodalAnalysis.View";
            public const string Create = "NodalAnalysis.Create";
            public const string Edit = "NodalAnalysis.Edit";
            public const string RunIPR = "NodalAnalysis.RunIPR";
            public const string RunVLP = "NodalAnalysis.RunVLP";
            public const string Optimize = "NodalAnalysis.Optimize";
        }

        public static class WellTestAnalysis
        {
            public const string View = "WellTestAnalysis.View";
            public const string Create = "WellTestAnalysis.Create";
            public const string Edit = "WellTestAnalysis.Edit";
            public const string RunPTA = "WellTestAnalysis.RunPTA";
            public const string Approve = "WellTestAnalysis.Approve";
        }

        public static class ChokeAnalysis
        {
            public const string View = "ChokeAnalysis.View";
            public const string Create = "ChokeAnalysis.Create";
            public const string Edit = "ChokeAnalysis.Edit";
            public const string RunCorrelations = "ChokeAnalysis.RunCorrelations";
        }

        public static class CompressorAnalysis
        {
            public const string View = "CompressorAnalysis.View";
            public const string Create = "CompressorAnalysis.Create";
            public const string Edit = "CompressorAnalysis.Edit";
            public const string RunCalculations = "CompressorAnalysis.RunCalculations";
        }

        public static class GasLift
        {
            public const string View = "GasLift.View";
            public const string Create = "GasLift.Create";
            public const string Edit = "GasLift.Edit";
            public const string DesignValves = "GasLift.DesignValves";
            public const string Optimize = "GasLift.Optimize";
        }

        public static class PumpPerformance
        {
            public const string View = "PumpPerformance.View";
            public const string Create = "PumpPerformance.Create";
            public const string Edit = "PumpPerformance.Edit";
            public const string RunAnalysis = "PumpPerformance.RunAnalysis";
        }

        public static class SuckerRodPumping
        {
            public const string View = "SuckerRodPumping.View";
            public const string Create = "SuckerRodPumping.Create";
            public const string Edit = "SuckerRodPumping.Edit";
            public const string DesignRodString = "SuckerRodPumping.DesignRodString";
            public const string RunDynamometer = "SuckerRodPumping.RunDynamometer";
        }

        public static class PlungerLift
        {
            public const string View = "PlungerLift.View";
            public const string Create = "PlungerLift.Create";
            public const string Edit = "PlungerLift.Edit";
            public const string Analyze = "PlungerLift.Analyze";
        }

        public static class HydraulicPumps
        {
            public const string View = "HydraulicPumps.View";
            public const string Create = "HydraulicPumps.Create";
            public const string Edit = "HydraulicPumps.Edit";
            public const string RunAnalysis = "HydraulicPumps.RunAnalysis";
        }

        public static class EnhancedRecovery
        {
            public const string View = "EnhancedRecovery.View";
            public const string Create = "EnhancedRecovery.Create";
            public const string Edit = "EnhancedRecovery.Edit";
            public const string Approve = "EnhancedRecovery.Approve";
            public const string ManageInjection = "EnhancedRecovery.ManageInjection";
        }

        // ─────────────────────────────────────────────────────────────────
        // HSE & Compliance Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class HSE
        {
            public const string View = "HSE.View";
            public const string ReportIncident = "HSE.ReportIncident";
            public const string ManageIncidents = "HSE.ManageIncidents";
            public const string ApproveIncident = "HSE.ApproveIncident";
            public const string ViewObservations = "HSE.ViewObservations";
            public const string CreateObservation = "HSE.CreateObservation";
            public const string ViewAudits = "HSE.ViewAudits";
            public const string ConductAudit = "HSE.ConductAudit";
            public const string ViewPermits = "HSE.ViewPermits";
            public const string IssuePermit = "HSE.IssuePermit";
            public const string ApprovePermit = "HSE.ApprovePermit";
            public const string ViewRiskAssessments = "HSE.ViewRiskAssessments";
            public const string CreateRiskAssessment = "HSE.CreateRiskAssessment";
            public const string ViewCertifications = "HSE.ViewCertifications";
            public const string ManageCertifications = "HSE.ManageCertifications";
        }

        public static class Environmental
        {
            public const string View = "Environmental.View";
            public const string Report = "Environmental.Report";
            public const string Approve = "Environmental.Approve";
            public const string ViewEmissions = "Environmental.ViewEmissions";
            public const string ReportEmissions = "Environmental.ReportEmissions";
            public const string ViewCompliance = "Environmental.ViewCompliance";
        }

        public static class Regulatory
        {
            public const string View = "Regulatory.View";
            public const string Submit = "Regulatory.Submit";
            public const string Approve = "Regulatory.Approve";
            public const string ViewCompliance = "Regulatory.ViewCompliance";
            public const string ManageFilings = "Regulatory.ManageFilings";
        }

        // ─────────────────────────────────────────────────────────────────
        // Exploration & Prospect Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Exploration
        {
            public const string View = "Exploration.View";
            public const string Create = "Exploration.Create";
            public const string Edit = "Exploration.Edit";
            public const string Approve = "Exploration.Approve";
            public const string ViewSeismic = "Exploration.ViewSeismic";
            public const string ManageSeismic = "Exploration.ManageSeismic";
        }

        public static class ProspectIdentification
        {
            public const string View = "ProspectIdentification.View";
            public const string Create = "ProspectIdentification.Create";
            public const string Edit = "ProspectIdentification.Edit";
            public const string Approve = "ProspectIdentification.Approve";
            public const string RunScreening = "ProspectIdentification.RunScreening";
        }

        public static class LeaseAcquisition
        {
            public const string View = "LeaseAcquisition.View";
            public const string Create = "LeaseAcquisition.Create";
            public const string Edit = "LeaseAcquisition.Edit";
            public const string Approve = "LeaseAcquisition.Approve";
        }

        public static class DevelopmentPlanning
        {
            public const string View = "DevelopmentPlanning.View";
            public const string Create = "DevelopmentPlanning.Create";
            public const string Edit = "DevelopmentPlanning.Edit";
            public const string Approve = "DevelopmentPlanning.Approve";
            public const string RunFDP = "DevelopmentPlanning.RunFDP";
        }

        public static class Decommissioning
        {
            public const string View = "Decommissioning.View";
            public const string Create = "Decommissioning.Create";
            public const string Edit = "Decommissioning.Edit";
            public const string Approve = "Decommissioning.Approve";
            public const string PlanAbandonment = "Decommissioning.PlanAbandonment";
        }

        // ─────────────────────────────────────────────────────────────────
        // PVT & Fluid Properties Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class OilProperties
        {
            public const string View = "OilProperties.View";
            public const string Create = "OilProperties.Create";
            public const string Edit = "OilProperties.Edit";
            public const string RunCorrelations = "OilProperties.RunCorrelations";
        }

        public static class GasProperties
        {
            public const string View = "GasProperties.View";
            public const string Create = "GasProperties.Create";
            public const string Edit = "GasProperties.Edit";
            public const string RunCalculations = "GasProperties.RunCalculations";
        }

        public static class FlashCalculations
        {
            public const string View = "FlashCalculations.View";
            public const string Create = "FlashCalculations.Create";
            public const string Edit = "FlashCalculations.Edit";
            public const string RunFlash = "FlashCalculations.RunFlash";
            public const string RunEOS = "FlashCalculations.RunEOS";
        }

        // ─────────────────────────────────────────────────────────────────
        // Drawing & Visualization Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Drawing
        {
            public const string View = "Drawing.View";
            public const string Create = "Drawing.Create";
            public const string Edit = "Drawing.Edit";
            public const string Approve = "Drawing.Approve";
            public const string Publish = "Drawing.Publish";
        }

        // ─────────────────────────────────────────────────────────────────
        // Reporting & Dashboard Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Reporting
        {
            public const string View = "Reporting.View";
            public const string Create = "Reporting.Create";
            public const string Edit = "Reporting.Edit";
            public const string Export = "Reporting.Export";
            public const string Schedule = "Reporting.Schedule";
        }

        public static class Dashboard
        {
            public const string View = "Dashboard.View";
            public const string Customize = "Dashboard.Customize";
            public const string Share = "Dashboard.Share";
        }

        // ─────────────────────────────────────────────────────────────────
        // Lifecycle Workflow & Process Permissions
        // ─────────────────────────────────────────────────────────────────

        public static class Workflow
        {
            public const string View = "Workflow.View";
            public const string Define = "Workflow.Define";
            public const string Start = "Workflow.Start";
            public const string Execute = "Workflow.Execute";
            public const string Approve = "Workflow.Approve";
            public const string ManageTemplates = "Workflow.ManageTemplates";
            public const string ConfigureSLA = "Workflow.ConfigureSLA";
        }

        public static class Process
        {
            public const string View = "Process.View";
            public const string ManageDefinitions = "Process.ManageDefinitions";
            public const string ManageTemplates = "Process.ManageTemplates";
            public const string ConfigureSLA = "Process.ConfigureSLA";
            public const string ViewHistory = "Process.ViewHistory";
            public const string ViewApprovals = "Process.ViewApprovals";
        }
    }
}
