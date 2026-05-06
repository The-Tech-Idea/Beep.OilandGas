using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to check if a user has a specific role.
/// </summary>
public record CheckRoleRequest(
    [Required(ErrorMessage = "USER_ID is required")] string USER_ID,
    [Required(ErrorMessage = "ROLE_NAME is required")] string ROLE_NAME);
