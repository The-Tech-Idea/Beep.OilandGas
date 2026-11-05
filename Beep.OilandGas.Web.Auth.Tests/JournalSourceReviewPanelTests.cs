using Beep.OilandGas.Web.Components.BusinessProcess;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class JournalSourceReviewPanelTests
{
    // Static component-render fixture only; any browser call fails instead of being simulated as successful.
    private sealed class NoBrowser : IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string identifier,object?[]? args)=>throw new InvalidOperationException("No browser in this focused renderer.");
        public ValueTask<T> InvokeAsync<T>(string identifier,CancellationToken token,object?[]? args)=>InvokeAsync<T>(identifier,args);
    }
    private static ProcessTransitionReview Review(IReadOnlyList<JournalSourceLineReview>? lines)=>new(new("tenant","company"),"reviewer","field","process","draft","review",new string('A',64),[],
        new("journal/42","JE-42",null,"DRAFT","Review evidence","preparer",null,"invoice/42","MANUAL","KuwaitBook",null,0m,DateTimeOffset.UtcNow,lines));
    private static async Task<string> Render(ProcessTransitionReview? review)
    {
        var services=new ServiceCollection();services.AddLogging();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddMudServices();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<JournalSourceReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",review}}));
            return root.ToHtmlString();
        });
    }
    [Fact] public async Task RendersRecordedHeaderAndLineCurrencyWithoutInventingValues()
    {
        var html=await Render(Review([new("line/1","journal/42",1,"account/1",10m,null,"KWD",null,"cost/1","Recorded line")]));
        Assert.Contains("journal/42",html);Assert.Contains("JE-42",html);Assert.Contains("preparer",html);Assert.Contains("KuwaitBook",html);
        Assert.Contains("account/1",html);Assert.Contains("KWD",html);Assert.Contains("cost/1",html);Assert.Contains("Recorded line",html);
        Assert.Contains("Recorded debit: Unavailable",html);Assert.Contains("Recorded credit: 0",html);Assert.Contains("data-label=\"Currency\"",html);
        Assert.Contains("Accounting period: unavailable",html);Assert.DoesNotContain("$",html);
    }
    [Fact] public async Task EmptyRecordedLinesHaveExplicitReadinessWarning()
    {
        var html=await Render(Review([]));Assert.Contains("No recorded journal lines were returned",html);Assert.DoesNotContain("<table",html);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task UnavailableReviewDoesNotShowJournalOrAmounts(bool missingLines)
    {
        var html=await Render(missingLines?Review(null):null);Assert.Contains("Recorded journal details are unavailable",html);
        Assert.DoesNotContain("JE-42",html);Assert.DoesNotContain("Recorded credit",html);
    }
    [Fact] public async Task RecordedDescriptionsAreHtmlEncoded()
    {
        var review=Review([new("line","journal/42",1,"account",null,null,null,null,null,"<script>alert(1)</script>")]);
        var html=await Render(review);Assert.DoesNotContain("<script>",html);Assert.Contains("&lt;script&gt;",html);
    }
}
