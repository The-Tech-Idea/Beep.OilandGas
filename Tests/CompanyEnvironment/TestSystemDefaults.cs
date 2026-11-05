using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;

namespace CompanyEnvironment.Tests;

internal sealed class TestSystemDefaults : ISystemSetupRecordReader
{
    internal static readonly SystemSetupConfiguration Baseline = SystemSetupIdentity.Prepare(
        new("TOOLS", "DEFAULT", "1", "{}", "", "00000000000000000000000000000001"), null);
    public async Task<SystemSetupConfiguration?> ReadAsync(string category, string key, Func<Task> authority)
    { await authority(); return category == "TOOLS" && key == "DEFAULT" ? Baseline : null; }
}
