using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.LifeCycle.Services.AccessControl
{
    /// <summary>
    /// Service for managing user profiles
    /// </summary>
    public class UserProfileService : IUserProfileService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly PPDMMappingService _mappingService;
        private readonly IAccessControlService _accessControlService;
        private readonly string _connectionName;

        public UserProfileService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            PPDMMappingService mappingService,
            IAccessControlService accessControlService,
            string connectionName = "PPDM39")
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            _accessControlService = accessControlService ?? throw new ArgumentNullException(nameof(accessControlService));
            _connectionName = connectionName;
        }

        public async Task<UserProfile?> GetUserProfileAsync(string userId)
        {
            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "USER_PROFILE");

                var results = await repo.GetAsync(filters);
                var profile = results?.FirstOrDefault();

                if (profile == null)
                {
                    // Create default profile if it doesn't exist
                    return await CreateDefaultProfileAsync(userId);
                }

                var roles = await _accessControlService.GetUserRolesAsync(userId);

                return new UserProfile
                {
                    UserId = userId,
                    PrimaryRole = GetPropertyValue(profile, "PRIMARY_ROLE")?.ToString(),
                    PreferredLayout = GetPropertyValue(profile, "PREFERRED_LAYOUT")?.ToString(),
                    UserPreferences = GetPropertyValue(profile, "USER_PREFERENCES")?.ToString(),
                    LastLoginDate = GetPropertyValue(profile, "LAST_LOGIN_DATE") is { } lld ? DateTime.Parse(lld.ToString()!) : null,
                    Active = GetPropertyValue(profile, "ACTIVE_IND")?.ToString() == "Y",
                    Roles = roles
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string userId, string? organizationId = null)
        {
            return await _accessControlService.GetUserRolesAsync(userId, organizationId);
        }

        public async Task<string?> GetUserDefaultLayoutAsync(string userId)
        {
            var profile = await GetUserProfileAsync(userId);
            if (profile == null) return null;

            // If user has a preferred layout, use it
            if (!string.IsNullOrEmpty(profile.PreferredLayout))
            {
                return profile.PreferredLayout;
            }

            // Otherwise, use default layout based on primary role
            if (!string.IsNullOrEmpty(profile.PrimaryRole))
            {
                // Return default layout based on role
                return profile.PrimaryRole switch
                {
                    "ADMIN" => "AdminLayout",
                    "ENGINEER" => "EngineerLayout",
                    "OPERATOR" => "OperatorLayout",
                    "MANAGER" => "ManagerLayout",
                    _ => "DefaultLayout"
                };
            }

            return "DefaultLayout";
        }

        public async Task<bool> UpdateUserPreferencesAsync(string userId, string preferencesJson)
        {
            return await UpsertUserProfileFieldAsync(userId, "USER_PREFERENCES", preferencesJson);
        }

        public async Task<bool> UpdateUserPrimaryRoleAsync(string userId, string primaryRole)
        {
            return await UpsertUserProfileFieldAsync(userId, "PRIMARY_ROLE", primaryRole);
        }

        public async Task<bool> UpdateUserPreferredLayoutAsync(string userId, string preferredLayout)
        {
            return await UpsertUserProfileFieldAsync(userId, "PREFERRED_LAYOUT", preferredLayout);
        }

        public async Task RecordUserLoginAsync(string userId)
        {
            await UpsertUserProfileFieldAsync(userId, "LAST_LOGIN_DATE", DateTime.UtcNow);
        }

        private async Task<UserProfile> CreateDefaultProfileAsync(string userId)
        {
            // Create a default profile with no primary role
            // The profile will use DefaultLayout until a role is assigned
            return new UserProfile
            {
                UserId = userId,
                PrimaryRole = null,
                PreferredLayout = "DefaultLayout",
                Active = true,
                Roles = new List<string>()
            };
        }

        private async Task<bool> UpsertUserProfileFieldAsync(string userId, string fieldName, object? fieldValue)
        {
            try
            {
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    typeof(object), _connectionName, "USER_PROFILE");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                    new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
                };

                var results = await repo.GetAsync(filters);
                var existing = results?.FirstOrDefault();

                if (existing != null)
                {
                    SetPropertyValue(existing, fieldName, fieldValue);
                    SetPropertyValue(existing, "ROW_CHANGED_BY", userId);
                    SetPropertyValue(existing, "ROW_CHANGED_DATE", DateTime.UtcNow);
                    await repo.UpdateAsync(existing, userId);
                }
                else
                {
                    var insertObj = new Dictionary<string, object?>
                    {
                        ["USER_ID"] = userId,
                        [fieldName] = fieldValue,
                        ["ACTIVE_IND"] = "Y",
                        ["ROW_CREATED_BY"] = userId,
                        ["ROW_CREATED_DATE"] = DateTime.UtcNow,
                        ["ROW_CHANGED_BY"] = userId,
                        ["ROW_CHANGED_DATE"] = DateTime.UtcNow
                    };
                    await repo.InsertAsync(insertObj, userId);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void SetPropertyValue(object obj, string key, object? value)
        {
            if (obj is IDictionary<string, object> dict)
            {
                dict[key] = value!;
                return;
            }
            var prop = obj.GetType().GetProperty(key,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            prop?.SetValue(obj, value);
        }

        private object? GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null) return null;

            if (obj is Dictionary<string, object> dict)
            {
                return dict.TryGetValue(propertyName, out var value) ? value : null;
            }

            var prop = obj.GetType().GetProperty(propertyName);
            return prop?.GetValue(obj);
        }
    }
}
