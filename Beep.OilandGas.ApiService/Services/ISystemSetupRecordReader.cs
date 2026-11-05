using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public interface ISystemSetupRecordReader
{
    Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority);
}
