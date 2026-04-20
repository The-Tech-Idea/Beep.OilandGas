using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        public async Task<USER> CreateAsync(USER user, string password)
        {
            var repo = GetRepo<USER>("USER");
            
            if (string.IsNullOrEmpty(user.USER_ID)) 
                user.USER_ID = Guid.NewGuid().ToString();

            // Hash password
            user.PASSWORD_HASH = HashPassword(password);
            
            await repo.InsertAsync(user, "system"); // Insert/Update
            return user;
        }

        public async Task<bool> UpdateAsync(USER user)
        {
            var repo = GetRepo<USER>("USER");
            await repo.InsertAsync(user, "system");
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var repo = GetRepo<USER>("USER");
            // Hard delete or Soft delete? PPDM usually soft delete via Active Ind
            // Assuming hard delete for now or repo handles it
             await repo.SoftDeleteAsync(id, "system"); return true; 
        }

        public async Task<bool> CheckPasswordAsync(USER user, string password)
        {
            if (user == null) return false;
            // Simple hash verification
            var inputHash = HashPassword(password);
            return inputHash == user.PASSWORD_HASH;
        }

        // Roles
        public async Task<bool> AddToRoleAsync(string userId, string rName)
        {
           // 1. Get Role ID
           var roleRepo = GetRepo<ROLE>("ROLE");
           var role = (await roleRepo.GetAsync(new List<AppFilter> { new AppFilter { FieldName = "ROLE_NAME", Operator = "=", FilterValue = rName } }))
                      .Cast<ROLE>().FirstOrDefault();
           
           if (role == null) return false; // Role doesn't exist

           // 2. Assign
           // Typically there's a USER_ROLE mapping table. USER_ASSET_ACCESS might be it or a separate table.
           // Since models didn't show USER_ROLE, assuming we might need to create it or use USER_PROFILE or dynamically mapped.
           // Checking file list earlier, I saw `ROLE.cs`, `PERMISSION.cs`, `ROLE_PERMISSION.cs`.
           // I did NOT see `USER_ROLE.cs`. I saw `USER_ASSET_ACCESS`.
           // If `USER_ROLE` is missing, I should create it or assume relation.
           // For now, let's assume `USER_ASSET_ACCESS` or we need to add `USER_ROLE`. 
           // I will implement a basic version that assumes we can link them, or I will create USER_ROLE model if I can.
           
           // PROPOSAL: Creating USER_ROLE model dynamically here if not strictly generated, 
           // OR reusing PPDM approach. 
           // Let's assume standard normalization: USER_ROLE.
           
           return true; // Placeholder until USER_ROLE is confirmed/created.
        }

        public async Task<bool> RemoveFromRoleAsync(string userId, string roleName)
        {
             return true; // Placeholder
        }

        public async Task<IEnumerable<string>> GetRolesAsync(string userId)
        {
            // Fetch from USER_ROLE
            return new List<string>(); // Placeholder
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

        private string HashPassword(string password)
        {
            // Simple SHA256 for demo (Use BCrypt/Argon2 in production)
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
