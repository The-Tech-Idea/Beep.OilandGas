using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to check if a user has ALL of the specified permissions.
/// </summary>
public record CheckPermissionAllRequest(
    [Required(ErrorMessage = "USER_ID is required")] string USER_ID,
    [Required(ErrorMessage = "PERMISSIONS is required")] string[] PERMISSIONS);
