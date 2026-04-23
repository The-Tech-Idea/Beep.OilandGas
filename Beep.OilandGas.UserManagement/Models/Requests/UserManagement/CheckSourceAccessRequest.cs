using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

public class CheckSourceAccessRequest : ModelEntityBase
{
    [Key]
    public string CHECK_SOURCE_ACCESS_REQUEST_ID { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "USER_ID is required")]
    public string USER_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "TARGET_SOURCE is required")]
    public string TARGET_SOURCE { get; set; } = string.Empty;
}
