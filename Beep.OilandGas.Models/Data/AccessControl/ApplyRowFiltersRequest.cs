using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data.AccessControl;

/// <summary>
/// Request to apply row-level filters based on user's access scope.
/// </summary>
public record ApplyRowFiltersRequest(
    [Required(ErrorMessage = "TABLE_NAME is required")] string TABLE_NAME,
    List<AppFilter>? Filters = null);
