using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.AccessControl
{
    /// <summary>
    /// Request to update user preferences
    /// </summary>
    public class UpdatePreferencesRequest
    {
        [Required(ErrorMessage = "PreferencesJson is required")]
        public string PreferencesJson { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to update user's primary role
    /// </summary>
    public class UpdatePrimaryRoleRequest
    {
        [Required(ErrorMessage = "PrimaryRole is required")]
        public string PrimaryRole { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to update user's preferred layout
    /// </summary>
    public class UpdatePreferredLayoutRequest
    {
        [Required(ErrorMessage = "PreferredLayout is required")]
        public string PreferredLayout { get; set; } = string.Empty;
    }
}
