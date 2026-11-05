using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to apply row-level filters based on user's access scope.
/// </summary>
public record ApplyRowFiltersRequest(
    [Required(ErrorMessage = "TABLE_NAME is required")] string TABLE_NAME,
    List<AppFilter>? Filters = null);
