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
    /// Implementation of IPermissionService using PPDMGenericRepository
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly string _connectionName;
        private readonly PPDMGenericRepository _permissionRepository;

        public PermissionService(
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

            _permissionRepository = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(PERMISSION), _connectionName, "PERMISSION");
        }

        public async Task<PERMISSION?> GetByIdAsync(string id)
        {
            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMISSION_ID", Operator = "=", FilterValue = id }
                };

                var results = await _permissionRepository.GetAsync(filters);
                return results?.Cast<PERMISSION>().FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<PERMISSION?> GetByCodeAsync(string code)
        {
            try
            {
                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PERMISSION_CODE", Operator = "=", FilterValue = code }
                };

                var results = await _permissionRepository.GetAsync(filters);
                return results?.Cast<PERMISSION>().FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<PERMISSION>> GetAllAsync()
        {
            try
            {
                var results = await _permissionRepository.GetAsync(new List<AppFilter>());
                return results?.Cast<PERMISSION>() ?? Enumerable.Empty<PERMISSION>();
            }
            catch
            {
                return Enumerable.Empty<PERMISSION>();
            }
        }

        public async Task<PERMISSION> CreateAsync(PERMISSION permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));

            if (string.IsNullOrWhiteSpace(permission.PERMISSION_CODE))
                throw new ArgumentException("Permission code cannot be null or empty", nameof(permission));

            // Set defaults if not provided
            if (string.IsNullOrWhiteSpace(permission.PERMISSION_ID))
            {
                permission.PERMISSION_ID = Guid.NewGuid().ToString();
            }

            var currentUserId = "SYSTEM";
            var result = await _permissionRepository.InsertAsync(permission, currentUserId);
            return (PERMISSION)result;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var permission = await GetByIdAsync(id);
                if (permission == null)
                    return false;

                await _permissionRepository.DeleteAsync(permission);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
