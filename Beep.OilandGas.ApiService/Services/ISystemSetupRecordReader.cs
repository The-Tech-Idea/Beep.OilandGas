using Beep.OilandGas.PPDM39.DataManagement.Models;

namespace Beep.OilandGas.ApiService.Services;

public interface ISystemSetupRecordReader
{
    Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority);
}
