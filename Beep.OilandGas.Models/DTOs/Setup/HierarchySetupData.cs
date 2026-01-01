using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.Setup
{
    /// <summary>
    /// Data class for AREA_HIERARCHY configuration during setup
    /// Maps to AREA_HIERARCHY and AREA tables in PPDM39 schema
    /// </summary>
    public class HierarchySetupData
    {
        [Required]
        public string OrganizationId { get; set; } = string.Empty;

        public List<HierarchyLevelConfig> HierarchyLevels { get; set; } = new List<HierarchyLevelConfig>();

        public List<AreaConfiguration> AreaConfigurations { get; set; } = new List<AreaConfiguration>();
    }

    /// <summary>
    /// Configuration for a single hierarchy level
    /// </summary>
    public class HierarchyLevelConfig
    {
        [Required]
        public int HierarchyLevel { get; set; }

        [Required]
        public string LevelName { get; set; } = string.Empty;

        [Required]
        public string AssetType { get; set; } = string.Empty;

        public int? ParentLevel { get; set; }

        public bool Active { get; set; } = true;
    }

    /// <summary>
    /// Area configuration for hierarchy
    /// </summary>
    public class AreaConfiguration
    {
        [Required]
        public string AreaId { get; set; } = string.Empty;

        [Required]
        public string AreaName { get; set; } = string.Empty;

        public string? AreaType { get; set; }

        public string? ParentAreaId { get; set; }

        public int HierarchyLevel { get; set; }

        public bool Active { get; set; } = true;
    }
}
