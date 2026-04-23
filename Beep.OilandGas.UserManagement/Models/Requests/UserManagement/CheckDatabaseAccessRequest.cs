using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

public class CheckDatabaseAccessRequest : ModelEntityBase
{
    [Key]
    public string CHECK_DATABASE_ACCESS_REQUEST_ID { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "USER_ID is required")]
    public string USER_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "DATABASE_NAME is required")]
    public string DATABASE_NAME { get; set; } = string.Empty;
}
