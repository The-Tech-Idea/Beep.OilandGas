namespace Beep.OilandGas.Models.Data.AccessControl
{
    public class GrantAccessRequest : ModelEntityBase
    {
        public string UserId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = "READ";
        public bool Inherit { get; set; } = true;
        public string? OrganizationId { get; set; }
    }

    public class RevokeAccessRequest : ModelEntityBase
    {
        public string UserId { get; set; } = string.Empty;
        public string AssetId { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
    }

    public static class RoleDefinitions
    {
        public const string Viewer = "Viewer";
        public const string Manager = "Manager";
        public const string PetroleumEngineer = "PetroleumEngineer";
        public const string ReservoirEngineer = "ReservoirEngineer";
        public const string Administrator = "Administrator";
        public const string Approver = "Approver";
        public const string Auditor = "Auditor";

        public static string[] GetAllRoles() => new[]
        {
            Viewer, Manager, PetroleumEngineer, ReservoirEngineer,
            Administrator, Approver, Auditor
        };
    }
}
