using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.AccessControl;

/// <summary>
/// Request to check row-level access for a user on a specific table/entity.
/// </summary>
public record CheckRowAccessRequest(
    [Required(ErrorMessage = "TABLE_NAME is required")] string TABLE_NAME,
    string? ENTITY_DATA_JSON = null);
