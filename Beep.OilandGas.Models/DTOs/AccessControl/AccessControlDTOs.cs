using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.AccessControl
{
    /// <summary>
    /// Request to grant access to an asset for a user
    /// </summary>
    public class GrantAccessRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AssetId is required")]
        public string AssetId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AssetType is required")]
        public string AssetType { get; set; } = string.Empty;

        public string AccessLevel { get; set; } = "READ";
        public bool Inherit { get; set; } = true;
        public string? OrganizationId { get; set; }
    }

    /// <summary>
    /// Request to revoke access to an asset for a user
    /// </summary>
    public class RevokeAccessRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AssetId is required")]
        public string AssetId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AssetType is required")]
        public string AssetType { get; set; } = string.Empty;
    }
}
