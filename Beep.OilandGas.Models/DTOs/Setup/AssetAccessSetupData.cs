using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.Setup
{
    /// <summary>
    /// Data class for initial asset access configuration for setup users
    /// Maps to USER_ASSET_ACCESS table (custom access control table)
    /// </summary>
    public class AssetAccessSetupData
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string AssetType { get; set; } = string.Empty; // FIELD, POOL, FACILITY, WELL, etc.

        [Required]
        public string AssetId { get; set; } = string.Empty;

        [Required]
        public string AccessLevel { get; set; } = "READ"; // READ, WRITE, DELETE

        public bool Inherit { get; set; } = true;

        public string? OrganizationId { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool Active { get; set; } = true;
    }

    /// <summary>
    /// Collection of asset access grants for setup
    /// </summary>
    public class AssetAccessSetupCollection
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        public string? OrganizationId { get; set; }

        public List<AssetAccessSetupData> AssetAccesses { get; set; } = new List<AssetAccessSetupData>();
    }
}
