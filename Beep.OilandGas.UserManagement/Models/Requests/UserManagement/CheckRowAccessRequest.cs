using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to check row-level access for a user on a specific table/entity.
/// </summary>
public record CheckRowAccessRequest(
    [Required(ErrorMessage = "USER_ID is required")] string USER_ID,
    [Required(ErrorMessage = "TABLE_NAME is required")] string TABLE_NAME,
    string? ENTITY_DATA_JSON = null);
