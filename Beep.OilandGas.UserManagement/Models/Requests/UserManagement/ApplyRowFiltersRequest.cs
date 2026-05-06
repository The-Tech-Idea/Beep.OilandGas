using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to apply row-level filters based on user's access scope.
/// </summary>
public record ApplyRowFiltersRequest(
    [Required(ErrorMessage = "USER_ID is required")] string USER_ID,
    [Required(ErrorMessage = "TABLE_NAME is required")] string TABLE_NAME,
    string? EXISTING_FILTERS_JSON = null);
