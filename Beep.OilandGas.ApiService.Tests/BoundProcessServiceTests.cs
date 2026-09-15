using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class BoundProcessServiceTests
{
    public static IEnumerable<object[]> Operations => typeof(IProcessService).GetMethods()
        .Select(x => new object[] { x.Name });

    [Theory]
    [MemberData(nameof(Operations))]
    public async Task EveryOperationRequiresABindingBeforeCreatingPersistence(string methodName)
    {
        var created = false;
        var service = new BoundProcessService(() => Task.FromResult(" "), _ =>
        {
            created = true;
            throw new InvalidOperationException("Must not construct persistence");
        });
        var method = typeof(IProcessService).GetMethod(methodName)!;
        var args = method.GetParameters().Select(p => p.ParameterType == typeof(string) ? (object)"id" : null).ToArray();
        var task = (Task)method.Invoke(service, args)!;
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
        Assert.Contains("bound LIFECYCLE", error.Message);
        Assert.False(created);
    }

    [Fact]
    public async Task EachOperationCreatesAFreshServiceWithTheCurrentTarget()
    {
        var first = new Mock<IProcessService>(MockBehavior.Strict);
        var second = new Mock<IProcessService>(MockBehavior.Strict);
        var instance = new ProcessInstance { InstanceId = "instance" };
        first.Setup(x => x.StartProcessAsync("process", "entity", "WELL", "field", "actor")).ReturnsAsync(instance);
        second.Setup(x => x.CancelProcessAsync("instance", "reason", "actor")).ReturnsAsync(true);
        var resolutions = 0;
        var targets = new List<string>();
        var service = new BoundProcessService(() => Task.FromResult(++resolutions == 1 ? "first" : "second"), target =>
        {
            targets.Add(target);
            return target == "first" ? first.Object : second.Object;
        });
        Assert.Same(instance, await service.StartProcessAsync("process", "entity", "WELL", "field", "actor"));
        Assert.True(await service.CancelProcessAsync("instance", "reason", "actor"));
        Assert.Equal(new[] { "first", "second" }, targets);
        Assert.Equal(2, resolutions);
        first.VerifyAll();
        second.VerifyAll();
    }
}
