using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.Web.Services;

public interface IWellLookupServiceClient
{
    Task<WELL?> GetWellAsync(string uwi, CancellationToken cancellationToken = default);
}