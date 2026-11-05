using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED

namespace Beep.OilandGas.Web.Services;

// The focused component harness controls repository readiness; the live identity flow is tested separately.
public sealed class RepositoryAccountClient(Func<Task<RepositoryReadiness>> read)
{
    public Task<RepositoryReadiness> GetReadinessAsync() => read();
}
#endif
