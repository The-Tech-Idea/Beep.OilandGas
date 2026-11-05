#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyEnvironmentBindingTests
{
    [Fact]
    public async Task BindingReaderUsesTenantOwnedModuleAndDetectsChangedTarget()
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture();
        var reader = new CompanyEnvironmentBindingReader(fixture.Modules);
        var original = await reader.ResolveAsync("PPDM_CORE", default);
        Assert.Equal(64, original!.Length);
        Assert.Null(await reader.ResolveAsync("MISSING", default));
        var binding = await fixture.Db.ModuleDatabases.SingleAsync(x => x.TenantId == "tenant" && x.ModuleId == "PPDM_CORE");
        binding.ConcurrencyStamp = "changed"; await fixture.Db.SaveChangesAsync();
        Assert.NotEqual(original, await reader.ResolveAsync("PPDM_CORE", default));
        fixture.Db.ModuleDatabases.Remove(binding); await fixture.Db.SaveChangesAsync();
        Assert.Null(await reader.ResolveAsync("PPDM_CORE", default));
    }
}
#endif
