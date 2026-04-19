namespace Beep.OilandGas.Branchs.BusinessProcess;

/// <summary>
/// Static registry of all business-process categories exposed in the BusinessProcess tree branch.
/// Each category maps to one or more process types that live inside the LifeCycle service layer.
/// </summary>
public static class BusinessProcessCategories
{
    public static List<BusinessProcessCategory> GetAll() => new()
    {
        new(1,  "Exploration Workflows",         "Lead-to-Prospect, Prospect-to-Discovery, and Discovery-to-Development processes"),
        new(2,  "Development Workflows",          "Pool definition, facility development, well drilling, and pipeline construction processes"),
        new(3,  "Production Workflows",           "Well start-up, production operations, decline management, and workover processes"),
        new(4,  "Decommissioning Workflows",      "Well abandonment and facility decommissioning processes"),
        new(5,  "Work Order Workflows",           "Maintenance work orders, inspection rounds, and repair workflows"),
        new(6,  "Approval & Gate Reviews",        "Stage-gate reviews, management approvals, and regulatory sanction workflows"),
        new(7,  "HSE & Safety Workflows",         "Safety assessments, incident investigations, and SIMOPS coordination processes"),
        new(8,  "Compliance & Regulatory",        "Permit applications, regulatory reporting, and obligation tracking workflows"),
        new(9,  "Well Lifecycle Workflows",       "Full well-lifecycle workflows from spud to abandonment"),
        new(10, "Facility Lifecycle Workflows",   "Full facility-lifecycle workflows from design to decommissioning"),
        new(11, "Reservoir Management Workflows", "Reservoir performance reviews, enhanced-recovery decisions, and pool management processes"),
        new(12, "Pipeline & Infrastructure",      "Pipeline integrity, surveillance, and tie-in or abandonment workflows"),
    };
}

/// <summary>Represents a business-process category in the tree.</summary>
public record BusinessProcessCategory(int Id, string Name, string Description);
