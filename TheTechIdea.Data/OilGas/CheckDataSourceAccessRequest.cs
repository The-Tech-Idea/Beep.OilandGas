using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

/// <summary>
/// Request to check if a user has access to a specific data source connection.
/// </summary>
public record CheckDataSourceAccessRequest(
    [Required(ErrorMessage = "DATASOURCE_NAME is required")] string DATASOURCE_NAME);
