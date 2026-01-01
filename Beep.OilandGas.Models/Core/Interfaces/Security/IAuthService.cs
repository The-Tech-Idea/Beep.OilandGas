using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Security;

namespace Beep.OilandGas.Models.Core.Interfaces.Security
{
    public interface IAuthService
    {
        Task<USER?> ValidateCredentialsAsync(string username, string password);
        Task<string> GenerateJwtAsync(USER user);
        Task<bool> SignOutAsync(string userId);
    }
}
