using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.Models.DTOs.AccessControl
{
    /// <summary>
    /// Request to validate access to an asset path
    /// </summary>
    public class ValidateAccessRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AssetPath is required")]
        public List<AssetHierarchyNode> AssetPath { get; set; } = new List<AssetHierarchyNode>();
    }
}
