using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.Models.Core.Interfaces.Security
{
    public interface IPermissionService
    {
        Task<PERMISSION?> GetByIdAsync(string id);
        Task<PERMISSION?> GetByCodeAsync(string code);
        Task<IEnumerable<PERMISSION>> GetAllAsync();
        Task<PERMISSION> CreateAsync(PERMISSION permission);
        Task<bool> DeleteAsync(string id);
    }
}
