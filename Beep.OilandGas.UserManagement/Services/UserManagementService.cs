using Beep.OilandGas.PPDM39.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Security;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.UserManagement.Services
{
    public class UserManagementService : IUserService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly ILogger<UserManagementService>? _logger;

        public UserManagementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName,
            ILogger<UserManagementService>? logger = null)
        {
            _editor = editor;
            _commonColumnHandler = commonColumnHandler;
            _defaults = defaults;
            _metadata = metadata;
            _connectionName = connectionName;
            _logger = logger;
        }

        private PPDMGenericRepository GetRepo<T>(string tableName)
        {
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), _connectionName, tableName, null);
        }

        public async Task<USER?> GetByIdAsync(string id)
        {
            var repo = GetRepo<USER>("USER");
            return await repo.GetByIdAsync(id) as USER;
        }

        public async Task<USER?> GetByUsernameAsync(string username)
        {
            var repo = GetRepo<USER>("USER");
            var result = await repo.GetAsync(new List<AppFilter> 
            { 
                new AppFilter { FieldName = "USERNAME", Operator = "=", FilterValue = username } 
            });
            return result.FirstOrDefault() as USER;
        }

        public async Task<IEnumerable<USER>> GetAllAsync()
        {
            var repo = GetRepo<USER>("USER");
            return (await repo.GetAsync(new List<AppFilter>())).Cast<USER>();
        }

        // No password: people sign in at Beep.IdentityServer. This hashed and verified passwords of its own — a second
        // sign-in nothing called (S3-06 §6).
        public async Task<USER> CreateAsync(USER user)
        {
            var repo = GetRepo<USER>("USER");

            if (string.IsNullOrEmpty(user.USER_ID))
                user.USER_ID = Guid.NewGuid().ToString();

            await repo.InsertAsync(user, "system"); // Insert/Update
            return user;
        }

        // Backward-compatible with IUserService.UpdateAsync(USER user)
        public async Task<bool> UpdateAsync(USER user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var repo = GetRepo<USER>("USER");
            await repo.UpdateAsync(user, "system");
            return true;
        }

        public async Task<bool> UpdateAsync(USER user, string userId)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("User ID is required for audit.", nameof(userId));

            var repo = GetRepo<USER>("USER");
            await repo.UpdateAsync(user, userId);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var repo = GetRepo<USER>("USER");
            // Hard delete or Soft delete? PPDM usually soft delete via Active Ind
            // Assuming hard delete for now or repo handles it
             await repo.SoftDeleteAsync(id, "system"); return true; 
        }

        // Roles
        public async Task<bool> AddToRoleAsync(string userId, string roleName)
        {
            var roleRepo = GetRepo<ROLE>("ROLE");
            var role = (await roleRepo.GetAsync(new List<AppFilter> 
            { 
                new AppFilter { FieldName = "ROLE_NAME", Operator = "=", FilterValue = roleName } 
            })).Cast<ROLE>().FirstOrDefault();
            
            if (role == null)
            {
                _logger?.LogWarning("Role {RoleName} not found for user {UserId}", roleName, userId);
                return false;
            }

            var userRoleRepo = GetRepo<USER_ROLE>("USER_ROLE");
            var existing = (await userRoleRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = role.ROLE_ID }
            })).Cast<USER_ROLE>().FirstOrDefault();

            if (existing != null)
            {
                _logger?.LogInformation("User {UserId} already has role {RoleName}", userId, roleName);
                return true;
            }

            var userRole = new USER_ROLE
            {
                USER_ROLE_ID = Guid.NewGuid().ToString(),
                USER_ID = userId,
                ROLE_ID = role.ROLE_ID,
                EFFECTIVE_DATE = DateTime.UtcNow,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = "system",
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_BY = "system",
                ROW_CHANGED_DATE = DateTime.UtcNow,
                ROW_EFFECTIVE_DATE = DateTime.UtcNow
            };

            await userRoleRepo.InsertAsync(userRole, "system");
            _logger?.LogInformation("Assigned role {RoleName} to user {UserId}", roleName, userId);
            return true;
        }

        public async Task<bool> RemoveFromRoleAsync(string userId, string roleName)
        {
            var roleRepo = GetRepo<ROLE>("ROLE");
            var role = (await roleRepo.GetAsync(new List<AppFilter> 
            { 
                new AppFilter { FieldName = "ROLE_NAME", Operator = "=", FilterValue = roleName } 
            })).Cast<ROLE>().FirstOrDefault();
            
            if (role == null) return false;

            var userRoleRepo = GetRepo<USER_ROLE>("USER_ROLE");
            var userRole = (await userRoleRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = role.ROLE_ID }
            })).Cast<USER_ROLE>().FirstOrDefault();

            if (userRole == null) return true;

            await userRoleRepo.SoftDeleteAsync(userRole.USER_ROLE_ID, "system");
            _logger?.LogInformation("Removed role {RoleName} from user {UserId}", roleName, userId);
            return true;
        }

        public async Task<IEnumerable<string>> GetRolesAsync(string userId)
        {
            var userRoleRepo = GetRepo<USER_ROLE>("USER_ROLE");
            var roleRepo = GetRepo<ROLE>("ROLE");

            var userRoles = (await userRoleRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            })).Cast<USER_ROLE>().ToList();

            if (!userRoles.Any()) return Enumerable.Empty<string>();

            var roleIds = userRoles.Select(ur => ur.ROLE_ID).Distinct().ToList();
            var roles = (await roleRepo.GetAsync(new List<AppFilter>
            {
                new AppFilter { FieldName = "ROLE_ID", Operator = "IN", FilterValue = string.Join(",", roleIds) }
            })).Cast<ROLE>().ToList();

            return roles.Select(r => r.ROLE_NAME).ToList();
        }

        // --- Permissions Logic ---
        
        public async Task<bool> CheckPermissionAsync(string userId, string permissionCode)
        {
            // 1. Get User Roles
            var roles = await GetRolesAsync(userId);
            if (!roles.Any()) return false;

            // 2. Get Permissions for Roles
            // Join ROLE -> ROLE_PERMISSION -> PERMISSION
            var permRepo = GetRepo<PERMISSION>("PERMISSION");
            var rpRepo = GetRepo<ROLE_PERMISSION>("ROLE_PERMISSION");
            var roleRepo = GetRepo<ROLE>("ROLE");

            // Optimizing: This needs a joined query or cached lookup.
            // For now, explicit fetch.
            
            // Get Role IDs
            var roleObjs = (await roleRepo.GetAsync(new List<AppFilter>()))
                           .Cast<ROLE>()
                           .Where(r => roles.Contains(r.ROLE_NAME))
                           .ToList();
            
            foreach (var role in roleObjs)
            {
                var rolePerms = (await rpRepo.GetAsync(new List<AppFilter> { new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = role.ROLE_ID } }))
                                .Cast<ROLE_PERMISSION>();
                
                foreach (var rp in rolePerms)
                {
                    var perm = await permRepo.GetByIdAsync(rp.PERMISSION_ID) as PERMISSION;
                    if (perm != null && perm.PERMISSION_CODE == permissionCode)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task SeedDefaultPermissionsAsync()
        {
             var permRepo = GetRepo<PERMISSION>("PERMISSION");
             var existing = (await permRepo.GetAsync(new List<AppFilter>())).Cast<PERMISSION>().ToList();
             
             var defaults = new List<string> 
             { 
                 PermissionConstants.Accounting.View,
                 PermissionConstants.Accounting.PostJournal,
                 PermissionConstants.Admin.ManageUsers,
                 // ... others
             };

             foreach (var code in defaults)
             {
                 if (!existing.Any(p => p.PERMISSION_CODE == code))
                 {
                     await permRepo.InsertAsync(new PERMISSION
                     {
                         PERMISSION_ID = Guid.NewGuid().ToString(),
                         PERMISSION_CODE = code,
                         DESCRIPTION = $"Auto-seeded permission: {code}"
                     }, "system");
                 }
             }
        }

    }
}
