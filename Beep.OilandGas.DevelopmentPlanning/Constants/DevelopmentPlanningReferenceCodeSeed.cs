using System.Collections.Generic;

namespace Beep.OilandGas.DevelopmentPlanning.Constants;

public static class DevelopmentPlanningReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        // Development plan lifecycle
        yield return new(DevelopmentPlanningReferenceSets.FdpStatus, DevelopmentPlanningDefaults.Draft, "Draft");
        yield return new(DevelopmentPlanningReferenceSets.FdpStatus, "INTERNAL_REVIEW", "Internal Review");
        yield return new(DevelopmentPlanningReferenceSets.FdpStatus, "REGULATORY_SUBMISSION", "Regulatory Submission");
        yield return new(DevelopmentPlanningReferenceSets.FdpStatus, DevelopmentPlanningDefaults.Approved, "Approved");
        yield return new(DevelopmentPlanningReferenceSets.FdpStatus, "SUPERSEDED", "Superseded");

        yield return new(DevelopmentPlanningReferenceSets.ScheduleStatus, DevelopmentPlanningDefaults.Planned, "Planned");
        yield return new(DevelopmentPlanningReferenceSets.ScheduleStatus, DevelopmentPlanningDefaults.Approved, "Approved");
        yield return new(DevelopmentPlanningReferenceSets.ScheduleStatus, "SPUDDED", "Spudded");
        yield return new(DevelopmentPlanningReferenceSets.ScheduleStatus, "COMPLETED", "Completed");
        yield return new(DevelopmentPlanningReferenceSets.ScheduleStatus, DevelopmentPlanningDefaults.Deferred, "Deferred");
        yield return new(DevelopmentPlanningReferenceSets.ScheduleStatus, DevelopmentPlanningDefaults.Cancelled, "Cancelled");

        // Well / facility planning master families
        yield return new(DevelopmentPlanningReferenceSets.WellType, "PRODUCER", "Producer");
        yield return new(DevelopmentPlanningReferenceSets.WellType, "INJECTOR", "Injector");
        yield return new(DevelopmentPlanningReferenceSets.WellType, "DISPOSAL", "Disposal");
        yield return new(DevelopmentPlanningReferenceSets.WellType, "OBSERVATION", "Observation");

        yield return new(DevelopmentPlanningReferenceSets.FacilityType, "WELLHEAD", "Wellhead");
        yield return new(DevelopmentPlanningReferenceSets.FacilityType, "FLOWLINE", "Flowline");
        yield return new(DevelopmentPlanningReferenceSets.FacilityType, "SEPARATOR", "Separator");
        yield return new(DevelopmentPlanningReferenceSets.FacilityType, "COMPRESSOR", "Compressor");
        yield return new(DevelopmentPlanningReferenceSets.FacilityType, "PIPELINE", "Pipeline");
        yield return new(DevelopmentPlanningReferenceSets.FacilityType, "CPF", "Central Processing Facility");
        yield return new(DevelopmentPlanningReferenceSets.FacilityType, "FPSO", "Floating Production Storage Offloading");

        yield return new(DevelopmentPlanningReferenceSets.InvestmentPhase, "FEED", "Front End Engineering Design");
        yield return new(DevelopmentPlanningReferenceSets.InvestmentPhase, "DETAILED_ENGINEERING", "Detailed Engineering");
        yield return new(DevelopmentPlanningReferenceSets.InvestmentPhase, "PROCUREMENT", "Procurement");
        yield return new(DevelopmentPlanningReferenceSets.InvestmentPhase, "CONSTRUCTION", "Construction");
        yield return new(DevelopmentPlanningReferenceSets.InvestmentPhase, "COMMISSIONING", "Commissioning");
        yield return new(DevelopmentPlanningReferenceSets.InvestmentPhase, "STARTUP", "Startup");

        yield return new(DevelopmentPlanningReferenceSets.EstimateClass, "1", "Class 1 - Definitive");
        yield return new(DevelopmentPlanningReferenceSets.EstimateClass, "2", "Class 2 - Control");
        yield return new(DevelopmentPlanningReferenceSets.EstimateClass, "3", "Class 3 - Budget");
        yield return new(DevelopmentPlanningReferenceSets.EstimateClass, "4", "Class 4 - Study");
        yield return new(DevelopmentPlanningReferenceSets.EstimateClass, "5", "Class 5 - Order of Magnitude");

        // Cost classification families
        yield return new(DevelopmentPlanningReferenceSets.CostCategory, "CAPEX", "Capital Expenditure");
        yield return new(DevelopmentPlanningReferenceSets.CostCategory, "OPEX", "Operating Expenditure");
        yield return new(DevelopmentPlanningReferenceSets.CostCategory, "ABEX", "Abandonment Expenditure");

        yield return new(DevelopmentPlanningReferenceSets.CostType, "DRILLING", "Drilling");
        yield return new(DevelopmentPlanningReferenceSets.CostType, "COMPLETIONS", "Completions");
        yield return new(DevelopmentPlanningReferenceSets.CostType, "FACILITIES", "Facilities");
        yield return new(DevelopmentPlanningReferenceSets.CostType, "SUBSEA", "Subsea");
        yield return new(DevelopmentPlanningReferenceSets.CostType, "PIPELINE", "Pipeline");
        yield return new(DevelopmentPlanningReferenceSets.CostType, "WORKOVER", "Workover");
        yield return new(DevelopmentPlanningReferenceSets.CostType, "LOE", "Lease Operating Expense");

        yield return new(DevelopmentPlanningReferenceSets.CostCurrency, "USD", "US Dollar");
        yield return new(DevelopmentPlanningReferenceSets.CostCurrency, "CAD", "Canadian Dollar");
        yield return new(DevelopmentPlanningReferenceSets.CostCurrency, "EUR", "Euro");
        yield return new(DevelopmentPlanningReferenceSets.CostCurrency, "GBP", "Pound Sterling");

        // Maintenance families
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceType, "PREVENTIVE", "Preventive");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceType, "CORRECTIVE", "Corrective");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceType, "PREDICTIVE", "Predictive");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceType, "INTEGRITY", "Integrity");

        yield return new(DevelopmentPlanningReferenceSets.MaintenanceStatus, DevelopmentPlanningDefaults.Planned, "Planned");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceStatus, "SCHEDULED", "Scheduled");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceStatus, "IN_PROGRESS", "In Progress");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceStatus, "COMPLETED", "Completed");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceStatus, DevelopmentPlanningDefaults.Cancelled, "Cancelled");

        yield return new(DevelopmentPlanningReferenceSets.MaintenancePriority, DevelopmentPlanningDefaults.High, "High");
        yield return new(DevelopmentPlanningReferenceSets.MaintenancePriority, DevelopmentPlanningDefaults.Medium, "Medium");
        yield return new(DevelopmentPlanningReferenceSets.MaintenancePriority, DevelopmentPlanningDefaults.Low, "Low");

        yield return new(DevelopmentPlanningReferenceSets.MaintenanceFrequency, "DAILY", "Daily");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceFrequency, "WEEKLY", "Weekly");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceFrequency, "MONTHLY", "Monthly");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceFrequency, "QUARTERLY", "Quarterly");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceFrequency, "ANNUAL", "Annual");

        yield return new(DevelopmentPlanningReferenceSets.MaintenanceTriggerBasis, DevelopmentPlanningDefaults.TimeBased, "Time Based");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceTriggerBasis, "RUNTIME_HOURS", "Runtime Hours");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceTriggerBasis, "PRODUCTION_VOLUME", "Production Volume");
        yield return new(DevelopmentPlanningReferenceSets.MaintenanceTriggerBasis, "CONDITION_EVENT", "Condition Event");

        // Service-company job families
        yield return new(DevelopmentPlanningReferenceSets.WellJobType, "WORKOVER", "Workover");
        yield return new(DevelopmentPlanningReferenceSets.WellJobType, "WELL_TEST", "Well Test");
        yield return new(DevelopmentPlanningReferenceSets.WellJobType, "STIMULATION", "Stimulation");
        yield return new(DevelopmentPlanningReferenceSets.WellJobType, "INTERVENTION", "Intervention");
        yield return new(DevelopmentPlanningReferenceSets.WellJobType, "LOGGING", "Logging");
        yield return new(DevelopmentPlanningReferenceSets.WellJobType, "CEMENTING", "Cementing");

        yield return new(DevelopmentPlanningReferenceSets.ServiceJobStatus, DevelopmentPlanningDefaults.Planned, "Planned");
        yield return new(DevelopmentPlanningReferenceSets.ServiceJobStatus, "ASSIGNED", "Assigned");
        yield return new(DevelopmentPlanningReferenceSets.ServiceJobStatus, "IN_PROGRESS", "In Progress");
        yield return new(DevelopmentPlanningReferenceSets.ServiceJobStatus, "COMPLETED", "Completed");
        yield return new(DevelopmentPlanningReferenceSets.ServiceJobStatus, DevelopmentPlanningDefaults.Cancelled, "Cancelled");

        yield return new(DevelopmentPlanningReferenceSets.ServiceJobPriority, DevelopmentPlanningDefaults.High, "High");
        yield return new(DevelopmentPlanningReferenceSets.ServiceJobPriority, DevelopmentPlanningDefaults.Medium, "Medium");
        yield return new(DevelopmentPlanningReferenceSets.ServiceJobPriority, DevelopmentPlanningDefaults.Low, "Low");

        yield return new(DevelopmentPlanningReferenceSets.ServiceJobTriggerBasis, DevelopmentPlanningDefaults.TimeBased, "Time Based");
        yield return new(DevelopmentPlanningReferenceSets.ServiceJobTriggerBasis, "OPERATIONS_WINDOW", "Operations Window");
        yield return new(DevelopmentPlanningReferenceSets.ServiceJobTriggerBasis, "MAINTENANCE_EVENT", "Maintenance Event");
        yield return new(DevelopmentPlanningReferenceSets.ServiceJobTriggerBasis, "HSE_EVENT", "HSE Event");
    }
}
