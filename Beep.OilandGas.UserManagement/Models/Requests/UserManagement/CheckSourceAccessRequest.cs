using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to check if a user has access to a specific data source.
/// </summary>
public record CheckSourceAccessRequest(
    [Required(ErrorMessage = "USER_ID is required")] string USER_ID,
    [Required(ErrorMessage = "TARGET_SOURCE is required")] string TARGET_SOURCE);
