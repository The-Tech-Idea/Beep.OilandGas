using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

public class CheckRoleRequest : ModelEntityBase
{
    [Key]
    public string CHECK_ROLE_REQUEST_ID { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "USER_ID is required")]
    public string USER_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "ROLE_NAME is required")]
    public string ROLE_NAME { get; set; } = string.Empty;
}
