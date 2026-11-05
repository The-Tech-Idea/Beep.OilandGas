using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.AccessControl;

/// <summary>
/// Request to check if a user has access to a specific database.
/// </summary>
public record CheckDatabaseAccessRequest(
    [Required(ErrorMessage = "DatabaseFingerprint is required")] string DatabaseFingerprint);
