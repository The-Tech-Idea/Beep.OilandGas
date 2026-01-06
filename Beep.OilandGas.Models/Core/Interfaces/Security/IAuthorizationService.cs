using System.Threading.Tasks;

namespace Beep.OilandGas.Models.Core.Interfaces.Security
{
    public interface IAuthorizationService
    {
        Task<bool> UserHasPermissionAsync(string userId, string permissionCode);
        Task<bool> UserIsInRoleAsync(string userId, string roleName);
    }
}



