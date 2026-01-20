using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.AccessControl
{
    /// <summary>
    /// Request to grant access to an asset for a user
    /// </summary>
    public class GrantAccessRequest : ModelEntityBase
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
    public class RevokeAccessRequest : ModelEntityBase
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AssetId is required")]
        public string AssetId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AssetType is required")]
        public string AssetType { get; set; } = string.Empty;
    }
}




