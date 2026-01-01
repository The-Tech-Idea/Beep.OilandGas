using System.Reflection;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services
{
    /// <summary>
    /// Implementation of IUserService using PPDMGenericRepository
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly PPDMGenericRepository _userRepository;
        private readonly PPDMGenericRepository _userRoleRepository;

        public UserService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));

            _userRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(USER), _connectionName, "USER");

            _userRoleRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(USER_ROLE), _connectionName, "USER_ROLE");
        }

        public async Task<USER?> GetByIdAsync(string id)
        {
            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = id }
                };

                var results = await _userRepository.GetAsync(filters);
                return results?.Cast<USER>().FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<USER?> GetByUsernameAsync(string username)
        {
            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USERNAME", Operator = "=", FilterValue = username }
                };

                var results = await _userRepository.GetAsync(filters);
                return results?.Cast<USER>().FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<USER>> GetAllAsync()
        {
            try
            {
                var results = await _userRepository.GetAsync(new List<AppFilter>());
                return results?.Cast<USER>() ?? Enumerable.Empty<USER>();
            }
            catch
            {
                return Enumerable.Empty<USER>();
            }
        }

        public async Task<USER> CreateAsync(USER user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Hash password
            user.PASSWORD_HASH = BCrypt.Net.BCrypt.HashPassword(password);

            // Set defaults if not provided
            if (string.IsNullOrWhiteSpace(user.USER_ID))
            {
                user.USER_ID = Guid.NewGuid().ToString();
            }

            if (!user.IS_ACTIVE)
            {
                user.IS_ACTIVE = true; // Default to active
            }

            // Insert user (common columns handled by repository)
            var currentUserId = user.ROW_CREATED_BY ?? "SYSTEM";
            var result = await _userRepository.InsertAsync(user, currentUserId);
            return (USER)result;
        }

        public async Task<bool> UpdateAsync(USER user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                var currentUserId = user.ROW_CHANGED_BY ?? "SYSTEM";
                await _userRepository.UpdateAsync(user, currentUserId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var user = await GetByIdAsync(id);
                if (user == null)
                    return false;

                // Soft delete by setting IS_ACTIVE = false
                user.IS_ACTIVE = false;
                return await UpdateAsync(user);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckPasswordAsync(USER user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (string.IsNullOrWhiteSpace(user.PASSWORD_HASH))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, user.PASSWORD_HASH);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddToRoleAsync(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));

            try
            {
                // Check if relationship already exists
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                    new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = roleName }
                };

                var existing = await _userRoleRepository.GetAsync(filters);
                if (existing != null && existing.Any())
                {
                    return true; // Already exists
                }

                // Create new USER_ROLE relationship
                var userRole = new USER_ROLE
                {
                    USER_ROLE_ID = Guid.NewGuid().ToString(),
                    USER_ID = userId,
                    ROLE_ID = roleName
                };

                var currentUserId = userId; // Use the user being modified
                await _userRoleRepository.InsertAsync(userRole, currentUserId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFromRoleAsync(string userId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));

            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
                    new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = roleName }
                };

                var results = await _userRoleRepository.GetAsync(filters);
                var userRole = results?.Cast<USER_ROLE>().FirstOrDefault();

                if (userRole == null)
                    return false;

                await _userRoleRepository.DeleteAsync(userRole);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetRolesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Enumerable.Empty<string>();

            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId }
                };

                var results = await _userRoleRepository.GetAsync(filters);
                return results?.Cast<USER_ROLE>()
                    .Select(ur => ur.ROLE_ID)
                    .Where(roleId => !string.IsNullOrWhiteSpace(roleId))
                    .ToList() ?? Enumerable.Empty<string>();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Helper method to get property value from dynamic object
        /// </summary>
        private object? GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            var property = obj.GetType().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(obj);
        }
    }
}
