namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// The desktop page catalog. This is the single place a page is declared; the shell builds navigation
/// from it and the route guard refuses anything it does not contain.
/// </summary>
/// <remarks>
/// Stage 0 declares the pages it can actually render. Later stages add their rows here — the guard and
/// the catalog-completeness test keep the declaration and the view factory in step.
/// </remarks>
public static class OgModuleCatalog
{
    private static readonly IReadOnlyList<string> AdministrationRoles =
        [OgRoleNames.SystemAdmin, OgRoleNames.TenantAdmin];

    private static readonly IReadOnlyList<string> Everyone = [];

    /// <summary>ENG-10: computing pay is an engineering read, not a viewer read.</summary>
    private static readonly IReadOnlyList<string> PayRoles =
    [
        OgRoleNames.SystemAdmin,
        OgRoleNames.TenantAdmin,
        OgRoleNames.ReservoirEngineer,
        OgRoleNames.DrillingEngineer,
        OgRoleNames.ProductionEngineer,
    ];

    public static IReadOnlyList<OgPageDefinition> Pages { get; } =
    [
        new OgPageDefinition("dashboard", "Dashboard", OgModuleKeys.Core, Everyone),
        new OgPageDefinition("tasks", "My tasks", OgModuleKeys.Core, Everyone),
        new OgPageDefinition("settings", "Settings", OgModuleKeys.Administration, AdministrationRoles),
        new OgPageDefinition("users", "Users and roles", OgModuleKeys.Administration, AdministrationRoles),
        new OgPageDefinition("licence", "Licence", OgModuleKeys.Administration, AdministrationRoles),
        new OgPageDefinition("diagnostics", "Diagnostics", OgModuleKeys.Administration, AdministrationRoles),
        new OgPageDefinition("performance", "Performance", OgModuleKeys.Administration, AdministrationRoles),
        new OgPageDefinition("modulesetup", "Module setup", OgModuleKeys.Data, AdministrationRoles),
        new OgPageDefinition("ppdmtree", "PPDM browser", OgModuleKeys.Data, Everyone),
        new OgPageDefinition("tableworkbench", "Table workbench", OgModuleKeys.Data, Everyone),
        new OgPageDefinition("entityform", "Entity form", OgModuleKeys.Data, Everyone),
        new OgPageDefinition("csvimport", "Import CSV", OgModuleKeys.Data, Everyone),
        new OgPageDefinition("dataquality", "Data quality", OgModuleKeys.Data, Everyone),
        new OgPageDefinition("governance", "Governance", OgModuleKeys.Data, AdministrationRoles),
        new OgPageDefinition("exploration", "Exploration workspace", OgModuleKeys.Exploration, Everyone),
        new OgPageDefinition("prospects", "Prospect board", OgModuleKeys.Exploration, Everyone),
        new OgPageDefinition("prospect", "Prospect editor", OgModuleKeys.Exploration, Everyone),
        new OgPageDefinition("seismic", "Seismic register", OgModuleKeys.Exploration, Everyone),
        new OgPageDefinition("wellprogram", "Well program approval", OgModuleKeys.Exploration, Everyone),
        new OgPageDefinition("leases", "Lease register", OgModuleKeys.Lease, Everyone),
        new OgPageDefinition("leasepipeline", "Acquisition pipeline", OgModuleKeys.Lease, Everyone),
        new OgPageDefinition("leasefinancials", "Lease financials", OgModuleKeys.Lease, Everyone),
        new OgPageDefinition("development", "Development workspace", OgModuleKeys.Development, Everyone),
        new OgPageDefinition("fdpwizard", "Field development plan", OgModuleKeys.Development, Everyone),
        new OgPageDefinition("welldesign", "Well design", OgModuleKeys.Development, Everyone),
        new OgPageDefinition("poolsfacilities", "Pools and facilities", OgModuleKeys.Development, Everyone),
        new OgPageDefinition("drillingprogram", "Drilling program", OgModuleKeys.Development, Everyone),
        new OgPageDefinition("drillingexecution", "Drilling execution", OgModuleKeys.Development, Everyone),
        new OgPageDefinition("completion", "Well completion", OgModuleKeys.Development, Everyone),
        new OgPageDefinition("construction", "Construction", OgModuleKeys.Development, Everyone),
        new OgPageDefinition("operations", "Operations centre", OgModuleKeys.Production, Everyone),
        new OgPageDefinition("fieldsandpools", "Fields and pools", OgModuleKeys.Production, Everyone),
        new OgPageDefinition("deferments", "Deferments", OgModuleKeys.Production, Everyone),
        new OgPageDefinition("allocation", "Allocation", OgModuleKeys.Production, Everyone),
        new OgPageDefinition("reconciliation", "Reconciliation", OgModuleKeys.Production, Everyone),
        new OgPageDefinition("interventions", "Interventions", OgModuleKeys.Production, Everyone),
        new OgPageDefinition("gasproperties", "Gas properties", OgModuleKeys.Reservoir, Everyone),
        new OgPageDefinition("oilproperties", "Oil properties", OgModuleKeys.Reservoir, Everyone),
        new OgPageDefinition("flash", "Flash calculations", OgModuleKeys.Reservoir, Everyone),
        new OgPageDefinition("eorscreening", "EOR screening", OgModuleKeys.Reservoir, Everyone),
        new OgPageDefinition("reserves", "Reserves", OgModuleKeys.Reservoir, Everyone),
        new OgPageDefinition("declinecurve", "Decline curve", OgModuleKeys.Reservoir, Everyone),
        new OgPageDefinition("forecasting", "Forecasting", OgModuleKeys.Reservoir, Everyone),
        new OgPageDefinition("nodal", "Nodal analysis", OgModuleKeys.Lift, Everyone),
        new OgPageDefinition("gaslift", "Gas lift", OgModuleKeys.Lift, Everyone),
        new OgPageDefinition("pumps", "Pump performance", OgModuleKeys.Lift, Everyone),
        new OgPageDefinition("suckerrod", "Sucker rod", OgModuleKeys.Lift, Everyone),
        new OgPageDefinition("plunger", "Plunger lift", OgModuleKeys.Lift, Everyone),
        new OgPageDefinition("hydraulicpump", "Hydraulic pumps", OgModuleKeys.Lift, Everyone),
        new OgPageDefinition("choke", "Choke analysis", OgModuleKeys.Lift, Everyone),
        new OgPageDefinition("compressor", "Compressor", OgModuleKeys.Lift, Everyone),
        new OgPageDefinition("welltest", "Well test", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("welltestregister", "Well test register", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("pipeline", "Pipeline", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("economics", "Economic evaluation", OgModuleKeys.Economics, Everyone),
        new OgPageDefinition("afe", "AFE", OgModuleKeys.Economics, Everyone),
        new OgPageDefinition("investmentreview", "Investment review", OgModuleKeys.Economics, Everyone),
        new OgPageDefinition("ledger", "Ledger", OgModuleKeys.Accounting, Everyone),
        new OgPageDefinition("revenue", "Revenue", OgModuleKeys.Accounting, Everyone),
        new OgPageDefinition("receivables", "Receivables", OgModuleKeys.Accounting, Everyone),
        new OgPageDefinition("payables", "Payables", OgModuleKeys.Accounting, Everyone),
        new OgPageDefinition("warehouse", "Warehouse", OgModuleKeys.Accounting, Everyone),
        new OgPageDefinition("royalties", "Royalties and JIB", OgModuleKeys.Accounting, Everyone),
        new OgPageDefinition("tax", "Tax", OgModuleKeys.Accounting, Everyone),
        new OgPageDefinition("accounting", "Accounting dashboard", OgModuleKeys.Accounting, Everyone),
        new OgPageDefinition("hse", "HSE", OgModuleKeys.Hse, Everyone),
        new OgPageDefinition("incidents", "Incidents", OgModuleKeys.Hse, Everyone),
        new OgPageDefinition("investigation", "Investigation", OgModuleKeys.Hse, Everyone),
        new OgPageDefinition("hazop", "HAZOP", OgModuleKeys.Hse, Everyone),
        new OgPageDefinition("compliance", "Compliance", OgModuleKeys.Hse, Everyone),
        new OgPageDefinition("ghg", "GHG", OgModuleKeys.Hse, Everyone),
        new OgPageDefinition("permits", "Permits", OgModuleKeys.Permits, Everyone),
        new OgPageDefinition("decommissioning", "Decommissioning", OgModuleKeys.Decommissioning, Everyone),
        new OgPageDefinition("wellpa", "Well P&A", OgModuleKeys.Decommissioning, Everyone),
        new OgPageDefinition("facilitycost", "Facilities and cost", OgModuleKeys.Decommissioning, Everyone),
        new OgPageDefinition("work", "Work orders", OgModuleKeys.Work, Everyone),
        new OgPageDefinition("workorder", "Work order", OgModuleKeys.Work, Everyone),
        new OgPageDefinition("maintenance", "Maintenance", OgModuleKeys.Work, Everyone),
        new OgPageDefinition("permittowork", "Permit to work", OgModuleKeys.Work, Everyone),
        new OgPageDefinition("processes", "Processes", OgModuleKeys.Workflow, Everyone),
        new OgPageDefinition("definitions", "Definitions", OgModuleKeys.Workflow, Everyone),
        new OgPageDefinition("instances", "Instances", OgModuleKeys.Workflow, Everyone),
        new OgPageDefinition("gates", "Gates", OgModuleKeys.Workflow, Everyone),

        new OgPageDefinition("anticollision", "Anti-collision", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("casingdesign", "Casing design", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("metalloss", "Metal loss", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("casingpressure", "Casing pressure", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("integrityboard", "Integrity board", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("wellplan", "Well plan", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("torquedrag", "Torque & drag", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("hydraulics", "Hydraulics", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("drillingperformance", "Drilling performance", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("wellboresetup", "Wellbore setup", OgModuleKeys.Interop, Everyone),
        new OgPageDefinition("fieldqc", "Field QC", OgModuleKeys.Interop, Everyone),
        new OgPageDefinition("map", "Map", OgModuleKeys.Interop, Everyone),
        new OgPageDefinition("scene3d", "3D view", OgModuleKeys.Interop, Everyone),
        new OgPageDefinition("training", "Scenarios", OgModuleKeys.Training, Everyone),
        new OgPageDefinition("courses", "Courses", OgModuleKeys.Training, Everyone),
        new OgPageDefinition("trainingserver", "Server and packs", OgModuleKeys.Training, Everyone),
        new OgPageDefinition("reports", "Reports", OgModuleKeys.Reporting, Everyone),
        new OgPageDefinition("printpacks", "Print packs", OgModuleKeys.Reporting, Everyone),
        new OgPageDefinition("installation", "Installation", OgModuleKeys.Administration, AdministrationRoles),
        new OgPageDefinition("releaseacceptance", "Release acceptance", OgModuleKeys.Administration, Everyone),
        new OgPageDefinition("mappingprofiles", "Mapping profiles", OgModuleKeys.Interop, Everyone),        new OgPageDefinition("workflowadmin", "Workflow administration", OgModuleKeys.Workflow,
            [OgRoleNames.SystemAdmin, OgRoleNames.TenantAdmin]),
        new OgPageDefinition("wells", "Well index", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("well", "Well header", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("wellbores", "Wellbores", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("wellstatus", "Well status", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("stratigraphy", "Stratigraphy", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("compare", "Compare wells", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("failures", "Failures", OgModuleKeys.Administration, AdministrationRoles),
        new OgPageDefinition("audit", "Audit log", OgModuleKeys.Administration, AdministrationRoles),
        new OgPageDefinition("connections", "Connections", OgModuleKeys.Data, AdministrationRoles),
        new OgPageDefinition("las", "LAS / DLIS", OgModuleKeys.Interop, Everyone),
        new OgPageDefinition("witsml", "WITSML", OgModuleKeys.Interop, Everyone),
        new OgPageDefinition("osdu", "OSDU", OgModuleKeys.Interop, Everyone),
        new OgPageDefinition("charts", "Charts", OgModuleKeys.Reporting, Everyone),
        new OgPageDefinition("wellbore", "Wellbore view", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("logs", "Well logs", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("schematic", "Well schematic", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("symbols", "Symbol gallery", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("export", "Export", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("correlation", "Correlation", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("wellpath", "Well path", OgModuleKeys.Wells, Everyone),
        new OgPageDefinition("paysummary", "Pay summary", OgModuleKeys.Wells, PayRoles),
    ];

    public static OgPageDefinition? Find(string route) =>
        Pages.FirstOrDefault(page =>
            string.Equals(page.Route, route, StringComparison.OrdinalIgnoreCase));

    public static bool IsKnownRoute(string route) => Find(route) is not null;
}
