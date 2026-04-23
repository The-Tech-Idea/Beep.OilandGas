using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

public class CheckPermissionAllRequest : ModelEntityBase
{
    [Key]
    public string CHECK_PERMISSION_ALL_REQUEST_ID { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "USER_ID is required")]
    public string USER_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "PERMISSIONS_JSON is required")]
    public string PERMISSIONS_JSON { get; set; } = "[]";
}
