using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.UserManagement
{
    public class CheckSourceAccessRequest : ModelEntityBase
    {
        /// <summary>
        /// User ID to check access for
        /// </summary>
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Source system identifier to check access to
        /// </summary>
        [Required(ErrorMessage = "Source is required")]
        public string Source { get; set; } = string.Empty;
    }
}
