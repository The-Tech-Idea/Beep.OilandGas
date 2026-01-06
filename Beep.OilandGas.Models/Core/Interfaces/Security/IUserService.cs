using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.Models.Core.Interfaces.Security
{
    public interface IUserService
    {
        Task<USER?> GetByIdAsync(string id);
        Task<USER?> GetByUsernameAsync(string username);
        Task<IEnumerable<USER>> GetAllAsync();
        Task<USER> CreateAsync(USER user, string password);
        Task<bool> UpdateAsync(USER user);
        Task<bool> DeleteAsync(string id);
        Task<bool> CheckPasswordAsync(USER user, string password);
        Task<bool> AddToRoleAsync(string userId, string roleName);
        Task<bool> RemoveFromRoleAsync(string userId, string roleName);
        Task<IEnumerable<string>> GetRolesAsync(string userId);
    }
}



