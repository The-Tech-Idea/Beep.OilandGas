#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.DataManagement;
using Moq;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyEnvironmentWellReaderTests
{
    [Fact]
    public async Task ExactWellQueryUsesTheSelectedCompanyCoreBindingAndEqualityFilters()
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture();
        var data = new Mock<IPPDM39DataService>();
        data.Setup(x => x.GetEntitiesAsync("WELL", It.IsAny<List<AppFilter>>(), "core"))
            .ReturnsAsync(new GetEntitiesResponse { Success = true, Count = 1, Entities = [new() { ["UWI"] = "well-1", ["WELL_NAME"] = "Well" }] });
        var reader = new CompanyEnvironmentWellReader(data.Object, fixture.Company);
        var filters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["UWI"] = "well-1", ["ACTIVE_IND"] = "Y" };
        var result = await reader.ReadAsync(new("tenant", "company"), "well-1", filters, () => Task.CompletedTask, default);
        Assert.Equal("well-1", Assert.Single(result)["UWI"]);
        data.Verify(x => x.GetEntitiesAsync("WELL", It.Is<List<AppFilter>>(f => f.Count == 2 && f.Any(p => p.FieldName == "UWI" && p.FilterValue == "well-1")), "core"), Times.Once);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => reader.ReadAsync(new("tenant", "foreign"), "well-1", filters, () => Task.CompletedTask, default));
        data.Verify(x => x.GetEntitiesAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UnconfirmedProviderResultsAreRefused()
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture();
        var data = new Mock<IPPDM39DataService>();
        data.Setup(x => x.GetEntitiesAsync("WELL", It.IsAny<List<AppFilter>>(), "core"))
            .ReturnsAsync(new GetEntitiesResponse { Success = true, Count = 1, Entities = [] });
        var reader = new CompanyEnvironmentWellReader(data.Object, fixture.Company);
        var filters = new Dictionary<string, string> { ["UWI"] = "well-1" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadAsync(new("tenant", "company"), "well-1", filters, () => Task.CompletedTask, default));
    }

    [Fact]
    public async Task BindingChangeDuringProviderReadCannotPublishAWell()
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture();
        var data = new Mock<IPPDM39DataService>();
        data.Setup(x => x.GetEntitiesAsync("WELL", It.IsAny<List<AppFilter>>(), "core"))
            .ReturnsAsync(() =>
            {
                var binding = fixture.Db.ModuleDatabases.Single(x => x.ModuleId == "PPDM_CORE" && x.TenantId == "tenant");
                binding.ConcurrencyStamp = "changed"; fixture.Db.SaveChanges();
                return new GetEntitiesResponse { Success = true, Count = 1, Entities = [new() { ["UWI"] = "well-1" }] };
            });
        var reader = new CompanyEnvironmentWellReader(data.Object, fixture.Company);
        await Assert.ThrowsAnyAsync<Exception>(() => reader.ReadAsync(new("tenant", "company"), "well-1",
            new Dictionary<string, string> { ["UWI"] = "well-1" }, () => Task.CompletedTask, default));
    }
}
#endif
