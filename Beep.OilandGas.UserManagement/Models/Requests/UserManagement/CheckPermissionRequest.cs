using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to check if a user has a specific permission.
/// </summary>
public record CheckPermissionRequest(
    [Required(ErrorMessage = "USER_ID is required")] string USER_ID,
    [Required(ErrorMessage = "PERMISSION_CODE is required")] string PERMISSION_CODE);
