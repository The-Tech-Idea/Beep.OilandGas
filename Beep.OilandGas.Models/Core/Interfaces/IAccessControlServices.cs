using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    #region Access Control Service Interfaces

    /// <summary>
    /// User profile service interface
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// Gets a user's profile
        /// </summary>
        Task<UserProfile?> GetUserProfileAsync(string userId);

        /// <summary>
        /// Gets all roles for a user
        /// </summary>
        Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null);

        /// <summary>
        /// Gets the default layout for a user based on their primary role
        /// </summary>
        Task<string?> GetUserDefaultLayoutAsync(string userId);

        /// <summary>
        /// Updates user preferences
        /// </summary>
        Task<bool> UpdateUserPreferencesAsync(string userId, string preferencesJson);

        /// <summary>
        /// Updates user's primary role
        /// </summary>
        Task<bool> UpdateUserPrimaryRoleAsync(string userId, string primaryRole);

        /// <summary>
        /// Updates user's preferred layout
        /// </summary>
        Task<bool> UpdateUserPreferredLayoutAsync(string userId, string preferredLayout);

        /// <summary>
        /// Records user login
        /// </summary>
        Task RecordUserLoginAsync(string userId);
    }

    #endregion
}




