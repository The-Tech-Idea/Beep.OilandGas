using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.UserManagement
{
    /// <summary>
    /// Request DTO for checking source access
    /// </summary>
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




