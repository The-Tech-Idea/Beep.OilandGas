using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.AccessControl
{
    /// <summary>
    /// Request to validate access to an asset path
    /// </summary>
    public class ValidateAccessRequest : ModelEntityBase
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "AssetPath is required")]
        public List<AssetHierarchyNode> AssetPath { get; set; } = new List<AssetHierarchyNode>();
    }
}




