using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class WellComparisonStateTests
{
    private static WellComparisonData Result(params string[] ids) => new()
    {
        Wells = ids.Select(id => new WellComparisonItem { WellIdentifier = id }).ToList()
    };

    [Theory]
    [InlineData("a", " a ")]
    [InlineData("a", " ")]
    public async Task InvalidIdentifiersDoNotCallApi(string first, string second)
    {
        using var state = new WellComparisonState(_ => throw new Exception("Unexpected request"));
        await state.CompareAsync(first, second);
        Assert.NotNull(state.Error); Assert.Null(state.Result); Assert.False(state.Loading);
    }

    [Fact]
    public async Task ExportIdentifiersAreTheTrimmedCompletedRequest()
    {
        using var state = new WellComparisonState(request =>
        {
            Assert.Equal(new[] { "a", "b" }, request.WellIdentifiers);
            return Task.FromResult<WellComparisonData?>(Result("b", "a"));
        });
        await state.CompareAsync(" a ", "b", " ");
        Assert.Equal(new[] { "a", "b" }, state.RequestedWells); Assert.NotNull(state.Result); Assert.Null(state.Error);
        state.Reset(); Assert.Empty(state.RequestedWells); Assert.Null(state.Result);
    }

    [Fact]
    public async Task FailureClearsPreviousComparisonAndExportSnapshot()
    {
        var fail = false;
        using var state = new WellComparisonState(_ => fail
            ? Task.FromException<WellComparisonData?>(new HttpRequestException("secret", null, System.Net.HttpStatusCode.Forbidden))
            : Task.FromResult<WellComparisonData?>(Result("a", "b")));
        await state.CompareAsync("a", "b"); fail = true;
        await state.CompareAsync("a", "c");
        Assert.Null(state.Result); Assert.Empty(state.RequestedWells); Assert.Contains("do not have access", state.Error); Assert.DoesNotContain("secret", state.Error);
    }

    [Fact]
    public async Task PartialResponseCannotBeExportedAsComplete()
    {
        using var state = new WellComparisonState(_ => Task.FromResult<WellComparisonData?>(Result("a")));
        await state.CompareAsync("a", "b");
        Assert.Null(state.Result); Assert.Empty(state.RequestedWells); Assert.NotNull(state.Error);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContextChangeOrDisposalRejectsLateResponse(bool dispose)
    {
        var pending = new TaskCompletionSource<WellComparisonData?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = new WellComparisonState(_ => pending.Task);
        var load = state.CompareAsync("a", "b");
        if (dispose) state.Dispose(); else state.Reset();
        pending.SetResult(Result("a", "b")); await load;
        Assert.Null(state.Result); Assert.Empty(state.RequestedWells); Assert.False(state.Loading);
    }

    [Fact]
    public async Task NewerComparisonWinsOverLateOlderRequest()
    {
        var pending = new TaskCompletionSource<WellComparisonData?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var state = new WellComparisonState(request => request.WellIdentifiers.Contains("a")
            ? pending.Task : Task.FromResult<WellComparisonData?>(Result("c", "d")));
        var oldLoad = state.CompareAsync("a", "b");
        await state.CompareAsync("c", "d");
        pending.SetResult(Result("a", "b")); await oldLoad;
        Assert.Equal(new[] { "c", "d" }, state.RequestedWells);
    }
}
