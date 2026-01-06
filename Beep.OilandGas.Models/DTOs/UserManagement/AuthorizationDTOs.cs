using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.UserManagement
{
    /// <summary>
    /// Request to check if a user has a specific permission
    /// </summary>
    public class CheckPermissionRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Permission is required")]
        public string Permission { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to check if a user has any of the specified permissions
    /// </summary>
    public class CheckPermissionAnyRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one permission is required")]
        public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
    }

    /// <summary>
    /// Request to check if a user has all of the specified permissions
    /// </summary>
    public class CheckPermissionAllRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one permission is required")]
        public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
    }

    /// <summary>
    /// Request to check if a user has a specific role
    /// </summary>
    public class CheckRoleRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = string.Empty;
    }
}



