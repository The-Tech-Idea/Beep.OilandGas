using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyAccountingAssignedScopeTests
{
    private sealed class Context(IProcessService process) : ICompanyAccountingProcessReadContext
    {
        public Task<(RepositoryCompanyAccess Access, string Binding, string Actor, IProcessService Process)> CaptureProcessAsync(ClaimsPrincipal user)
            => Task.FromResult((new RepositoryCompanyAccess(new("tenant", "company"), ["Accountant"], ["Accounting.View"]), "binding", "actor", process));
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task OriginalInboxCompanyAndActorMustMatchBeforeProcessOrNativeRead(bool foreignCompany)
    {
        var process = new Mock<IProcessService>(MockBehavior.Strict);var resources = new Mock<IAccessControlService>(MockBehavior.Strict);
        // Null native services prove the rejected scope cannot reach source lookup.
        var reads = new CompanyAccountingProcessReads(new Context(process.Object), resources.Object, null!, null!, null!, null!);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => reads.ReadAsync(new ClaimsPrincipal(), "process",
            new("tenant", foreignCompany ? "other" : "company"), foreignCompany ? "actor" : "other"));
        process.VerifyNoOtherCalls(); resources.VerifyNoOtherCalls();
    }
}
