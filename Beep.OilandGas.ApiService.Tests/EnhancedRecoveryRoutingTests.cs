using System.Reflection;
using Beep.OilandGas.EnhancedRecovery.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class EnhancedRecoveryRoutingTests
{
    [Theory]
    [InlineData("GetPDENUnitOfWorkAsync")]
    [InlineData("GetFieldUnitOfWorkAsync")]
    [InlineData("GetWellUnitOfWorkAsync")]
    [InlineData("GetPDENFlowMeasurementUnitOfWorkAsync")]
    public async Task UnitOfWorkRequiresCoreBindingBeforeDatasourceAccess(string factory)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; return Task.FromResult(""); }
        var service = new EnhancedRecoveryService(editor.Object, NullLogger<EnhancedRecoveryService>.Instance,
            "global-db", Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)typeof(EnhancedRecoveryService)
            .GetMethod(factory, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(service, null)!);
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
    }
}
