using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

public class CheckPermissionRequest : ModelEntityBase
{
    [Key]
    public string CHECK_PERMISSION_REQUEST_ID { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "USER_ID is required")]
    public string USER_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "PERMISSION_CODE is required")]
    public string PERMISSION_CODE { get; set; } = string.Empty;
}
