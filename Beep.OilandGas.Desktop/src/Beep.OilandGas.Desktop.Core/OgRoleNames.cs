namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// The role names the desktop gates pages with. They mirror the client application's own RBAC store
/// (<c>Beep.OilandGas.Repository</c> / <c>UserManagement</c>); IdentityServer remains authentication-only.
/// </summary>
public static class OgRoleNames
{
    public const string SystemAdmin = "SystemAdmin";
    public const string TenantAdmin = "TenantAdmin";
    public const string Accountant = "Accountant";
    public const string ExplorationGeologist = "ExplorationGeologist";
    public const string ReservoirEngineer = "ReservoirEngineer";
    public const string DrillingEngineer = "DrillingEngineer";
    public const string ProductionEngineer = "ProductionEngineer";
    public const string HseOfficer = "HseOfficer";
    public const string DecommissioningCoordinator = "DecommissioningCoordinator";
    public const string Viewer = "Viewer";
}
