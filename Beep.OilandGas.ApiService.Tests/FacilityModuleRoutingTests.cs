using System.Reflection;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class FacilityModuleRoutingTests
{
    [Theory]
    [InlineData(typeof(FACILITY), "PPDM_CORE")]
    [InlineData(typeof(PDEN_VOL_SUMMARY), "PPDM_CORE")]
    [InlineData(typeof(FACILITY_MEASUREMENT), "FACILITY")]
    [InlineData(typeof(FACILITY_EQUIPMENT_ACTIVITY), "FACILITY")]
    public async Task RepositoryFactoryResolvesTheDeclaredStorageOwner(Type entity, string expectedModule)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        string? selected = null;
        var service = Create(editor, module => { selected = module; throw new InvalidOperationException("Unbound"); });
        var factory = typeof(FacilityManagementService).GetMethod("RepoAsync", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var task = (Task)factory.MakeGenericMethod(entity).Invoke(service, [entity.Name])!;
        await Assert.ThrowsAsync<InvalidOperationException>(() => task);
        Assert.Equal(expectedModule, selected);
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task FacilityListCannotFallBackToGlobalConnection()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor, _ => Task.FromResult(""));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ListFacilitiesAsync(null));
        editor.VerifyNoOtherCalls();
    }

    private static FacilityManagementService Create(Mock<IDMEEditor> editor, Func<string, Task<string>> resolver) =>
        new(editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), "global-db", resolveModuleConnection: resolver);
}
