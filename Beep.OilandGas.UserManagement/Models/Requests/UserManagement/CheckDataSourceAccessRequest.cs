using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to check if a user has access to a specific data source connection.
/// </summary>
public record CheckDataSourceAccessRequest(
    [Required(ErrorMessage = "USER_ID is required")] string USER_ID,
    [Required(ErrorMessage = "DATASOURCE_NAME is required")] string DATASOURCE_NAME);
