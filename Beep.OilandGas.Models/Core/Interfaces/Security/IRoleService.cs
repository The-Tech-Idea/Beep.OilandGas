using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.Models.Core.Interfaces.Security
{
    public interface IRoleService
    {
        Task<ROLE?> GetByIdAsync(string id);
        Task<ROLE?> GetByNameAsync(string name);
        Task<IEnumerable<ROLE>> GetAllAsync();
        Task<ROLE> CreateAsync(ROLE role);
        Task<bool> UpdateAsync(ROLE role);
        Task<bool> DeleteAsync(string id);
        Task<bool> AddPermissionToRoleAsync(string roleId, string permissionCode);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionCode);
    }
}



