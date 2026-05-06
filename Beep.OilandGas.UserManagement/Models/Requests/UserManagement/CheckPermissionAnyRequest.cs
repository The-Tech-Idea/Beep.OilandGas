using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to check if a user has ANY of the specified permissions.
/// </summary>
public record CheckPermissionAnyRequest(
    [Required(ErrorMessage = "USER_ID is required")] string USER_ID,
    [Required(ErrorMessage = "PERMISSIONS is required")] string[] PERMISSIONS);
