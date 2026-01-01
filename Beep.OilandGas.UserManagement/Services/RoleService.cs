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
    /// Implementation of IRoleService using PPDMGenericRepository
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly PPDMGenericRepository _roleRepository;
        private readonly PPDMGenericRepository _rolePermissionRepository;

        public RoleService(
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

            _roleRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ROLE), _connectionName, "ROLE");

            _rolePermissionRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ROLE_PERMISSION), _connectionName, "ROLE_PERMISSION");
        }

        public async Task<ROLE?> GetByIdAsync(string id)
        {
            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = id }
                };

                var results = await _roleRepository.GetAsync(filters);
                return results?.Cast<ROLE>().FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<ROLE?> GetByNameAsync(string name)
        {
            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ROLE_NAME", Operator = "=", FilterValue = name }
                };

                var results = await _roleRepository.GetAsync(filters);
                return results?.Cast<ROLE>().FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<ROLE>> GetAllAsync()
        {
            try
            {
                var results = await _roleRepository.GetAsync(new List<AppFilter>());
                return results?.Cast<ROLE>() ?? Enumerable.Empty<ROLE>();
            }
            catch
            {
                return Enumerable.Empty<ROLE>();
            }
        }

        public async Task<ROLE> CreateAsync(ROLE role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            // Set defaults if not provided
            if (string.IsNullOrWhiteSpace(role.ROLE_ID))
            {
                role.ROLE_ID = Guid.NewGuid().ToString();
            }

            var currentUserId = "SYSTEM";
            var result = await _roleRepository.InsertAsync(role, currentUserId);
            return (ROLE)result;
        }

        public async Task<bool> UpdateAsync(ROLE role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            try
            {
                var currentUserId = "SYSTEM";
                await _roleRepository.UpdateAsync(role, currentUserId);
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
                var role = await GetByIdAsync(id);
                if (role == null)
                    return false;

                await _roleRepository.DeleteAsync(role);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddPermissionToRoleAsync(string roleId, string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role ID cannot be null or empty", nameof(roleId));

            if (string.IsNullOrWhiteSpace(permissionCode))
                throw new ArgumentException("Permission code cannot be null or empty", nameof(permissionCode));

            try
            {
                // First, get permission by code to get its ID
                var permissionService = new PermissionService(_editor, _commonColumnHandler, _defaults, _metadata, _connectionName);
                var permission = await permissionService.GetByCodeAsync(permissionCode);
                if (permission == null)
                    return false;

                // Check if relationship already exists
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = roleId },
                    new AppFilter { FieldName = "PERMISSION_ID", Operator = "=", FilterValue = permission.PERMISSION_ID }
                };

                var existing = await _rolePermissionRepository.GetAsync(filters);
                if (existing != null && existing.Any())
                {
                    return true; // Already exists
                }

                // Create new ROLE_PERMISSION relationship
                var rolePermission = new ROLE_PERMISSION
                {
                    ROLE_PERMISSION_ID = Guid.NewGuid().ToString(),
                    ROLE_ID = roleId,
                    PERMISSION_ID = permission.PERMISSION_ID
                };

                var currentUserId = "SYSTEM";
                await _rolePermissionRepository.InsertAsync(rolePermission, currentUserId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role ID cannot be null or empty", nameof(roleId));

            if (string.IsNullOrWhiteSpace(permissionCode))
                throw new ArgumentException("Permission code cannot be null or empty", nameof(permissionCode));

            try
            {
                // First, get permission by code to get its ID
                var permissionService = new PermissionService(_editor, _commonColumnHandler, _defaults, _metadata, _connectionName);
                var permission = await permissionService.GetByCodeAsync(permissionCode);
                if (permission == null)
                    return false;

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = roleId },
                    new AppFilter { FieldName = "PERMISSION_ID", Operator = "=", FilterValue = permission.PERMISSION_ID }
                };

                var results = await _rolePermissionRepository.GetAsync(filters);
                var rolePermission = results?.Cast<ROLE_PERMISSION>().FirstOrDefault();

                if (rolePermission == null)
                    return false;

                await _rolePermissionRepository.DeleteAsync(rolePermission);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetRolePermissionsAsync(string roleId)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                return Enumerable.Empty<string>();

            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "ROLE_ID", Operator = "=", FilterValue = roleId }
                };

                var results = await _rolePermissionRepository.GetAsync(filters);
                var rolePermissions = results?.Cast<ROLE_PERMISSION>() ?? Enumerable.Empty<ROLE_PERMISSION>();

                // Get permission codes for each permission ID
                var permissionService = new PermissionService(_editor, _commonColumnHandler, _defaults, _metadata, _connectionName);
                var permissionCodes = new List<string>();

                foreach (var rp in rolePermissions)
                {
                    if (!string.IsNullOrWhiteSpace(rp.PERMISSION_ID))
                    {
                        var permission = await permissionService.GetByIdAsync(rp.PERMISSION_ID);
                        if (permission != null && !string.IsNullOrWhiteSpace(permission.PERMISSION_CODE))
                        {
                            permissionCodes.Add(permission.PERMISSION_CODE);
                        }
                    }
                }

                return permissionCodes.Distinct();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
