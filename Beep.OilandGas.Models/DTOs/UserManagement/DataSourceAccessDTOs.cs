using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.DTOs.UserManagement
{
    /// <summary>
    /// Request to check if a user has access to a data source
    /// </summary>
    public class CheckDataSourceAccessRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "DataSourceName is required")]
        public string DataSourceName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to check if a user has access to a database
    /// </summary>
    public class CheckDatabaseAccessRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "DatabaseName is required")]
        public string DatabaseName { get; set; } = string.Empty;
    }
}
