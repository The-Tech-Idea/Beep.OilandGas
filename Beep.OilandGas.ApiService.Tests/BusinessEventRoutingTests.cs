using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class BusinessEventRoutingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingBindingPreventsPersistenceAndDispatch(bool blankResolver)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var provider = new Mock<IServiceProvider>(MockBehavior.Strict);
        var service = Create(editor.Object, blankResolver ? () => Task.FromResult(" ") : null, provider.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterTriggerAsync(new(), "actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetTriggersForEntityAsync("WELL"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.OnBusinessEventAsync(new() { EntityType = "WELL" }, "actor"));
        editor.VerifyNoOtherCalls();
        provider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RegistrationAndEventLookupUseTheirCurrentBinding()
    {
        var first = new Mock<IDataSource>(MockBehavior.Strict);
        first.Setup(x => x.InsertEntity("BUSINESS_EVENT_TRIGGER", It.IsAny<object>()))
            .Returns(new ErrorsInfo { Flag = Errors.Ok });
        var second = new Mock<IDataSource>(MockBehavior.Strict);
        second.Setup(x => x.GetEntityAsync("BUSINESS_EVENT_TRIGGER", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<BUSINESS_EVENT_TRIGGER>());
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("first")).Returns(first.Object);
        editor.Setup(x => x.GetDataSource("second")).Returns(second.Object);
        var provider = new Mock<IServiceProvider>(MockBehavior.Strict);
        var resolutions = 0;
        var service = Create(editor.Object, () => Task.FromResult(++resolutions == 1 ? "first" : "second"), provider.Object);
        var trigger = new BUSINESS_EVENT_TRIGGER { ENTITY_TYPE = "WELL" };
        Assert.Same(trigger, await service.RegisterTriggerAsync(trigger, "actor"));
        Assert.Equal(1, resolutions);
        Assert.Empty(await service.OnBusinessEventAsync(new() { EntityType = "WELL" }, "actor"));
        Assert.Equal(2, resolutions);
        first.Verify(x => x.InsertEntity("BUSINESS_EVENT_TRIGGER", trigger), Times.Once);
        second.Verify(x => x.GetEntityAsync("BUSINESS_EVENT_TRIGGER", It.IsAny<List<AppFilter>>()), Times.Once);
        editor.Verify(x => x.GetDataSource("first"), Times.Once);
        editor.Verify(x => x.GetDataSource("second"), Times.Once);
        editor.VerifyNoOtherCalls();
        provider.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RebindingDuringLookupCannotRedirectAnyProcessStartedByTheEvent()
    {
        var binding = "first";
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.GetEntityAsync("BUSINESS_EVENT_TRIGGER", It.IsAny<List<AppFilter>>()))
            .Callback(() => binding = "second")
            .ReturnsAsync(new List<BUSINESS_EVENT_TRIGGER>
            {
                new() { IS_ACTIVE = "Y", EVENT_TYPE = "STATUS_CHANGED", TARGET_PROCESS_DEF_ID = "p1", PRIORITY = 1 },
                new() { IS_ACTIVE = "Y", EVENT_TYPE = "STATUS_CHANGED", TARGET_PROCESS_DEF_ID = "p2", PRIORITY = 2 }
            });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("first")).Returns(source.Object);
        editor.Setup(x => x.GetDataSource("second")).Returns(source.Object);
        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(x => x.StartProcessAsync("p1", "well", "WELL", "field", "actor"))
            .ReturnsAsync(new ProcessInstance { InstanceId = "i1" });
        process.Setup(x => x.StartProcessAsync("p2", "well", "WELL", "field", "actor"))
            .ReturnsAsync(new ProcessInstance { InstanceId = "i2" });
        var targets = new List<string>();
        var resolutions = 0;
        var service = new BusinessEventTriggerService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            () => { resolutions++; return Task.FromResult(binding); }, null,
            target => { targets.Add(target); return process.Object; });

        var started = await service.OnBusinessEventAsync(new() { EntityType = "WELL", EntityId = "well", FieldId = "field" }, "actor");
        Assert.Equal(new[] { "i1", "i2" }, started);
        Assert.Equal(new[] { "first", "first" }, targets);
        Assert.Equal(1, resolutions);
        await service.GetTriggersForEntityAsync("WELL");
        Assert.Equal(2, resolutions);
        editor.Verify(x => x.GetDataSource("first"), Times.Once);
        editor.Verify(x => x.GetDataSource("second"), Times.Once);
        source.Verify(x => x.GetEntityAsync("BUSINESS_EVENT_TRIGGER", It.IsAny<List<AppFilter>>()), Times.Exactly(2));
        process.VerifyAll();
        editor.VerifyNoOtherCalls();
    }

    private static BusinessEventTriggerService Create(IDMEEditor editor, Func<Task<string>>? connection, IServiceProvider provider) =>
        new(editor, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), connection, null,
            _ => (IProcessService)provider.GetService(typeof(IProcessService))!);
}
