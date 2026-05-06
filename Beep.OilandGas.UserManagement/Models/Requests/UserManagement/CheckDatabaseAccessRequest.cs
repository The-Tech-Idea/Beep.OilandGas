using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to check if a user has access to a specific database.
/// </summary>
public record CheckDatabaseAccessRequest(
    [Required(ErrorMessage = "USER_ID is required")] string USER_ID,
    [Required(ErrorMessage = "DATABASE_NAME is required")] string DATABASE_NAME);
