using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Requests.UserManagement;

public class CheckRowAccessRequest : ModelEntityBase
{
    [Key]
    public string CHECK_ROW_ACCESS_REQUEST_ID { get; set; } = Guid.NewGuid().ToString();

    [Required(ErrorMessage = "USER_ID is required")]
    public string USER_ID { get; set; } = string.Empty;

    [Required(ErrorMessage = "TABLE_NAME is required")]
    public string TABLE_NAME { get; set; } = string.Empty;

    public string ENTITY_DATA_JSON { get; set; } = "{}";
}
