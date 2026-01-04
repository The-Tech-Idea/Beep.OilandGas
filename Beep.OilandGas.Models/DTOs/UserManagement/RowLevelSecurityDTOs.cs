using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.DTOs.UserManagement
{
    /// <summary>
    /// Request to check if a user has access to a specific row
    /// </summary>
    public class CheckRowAccessRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "TableName is required")]
        public string TableName { get; set; } = string.Empty;

        public Dictionary<string, object> EntityData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Request to apply row-level security filters for a user
    /// </summary>
    public class ApplyRowFiltersRequest
    {
        [Required(ErrorMessage = "UserId is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "TableName is required")]
        public string TableName { get; set; } = string.Empty;

        public List<AppFilter>? ExistingFilters { get; set; }
    }
}
