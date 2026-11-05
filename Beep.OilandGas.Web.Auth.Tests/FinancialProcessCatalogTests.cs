using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Web.Services;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class FinancialProcessCatalogTests
{
    private static CompanyClientContext Company(){var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"accountant",new(new("tenant","company"),[],[])));return company;}
    private static ProcessDefinition Definition()=>new(){ProcessId="financial/a?b",ProcessName="Configured local process",ProcessType="FINANCIAL",EntityType="JOURNAL_ENTRY",Version="",IsActive=true,Configuration=new(){["regulation"]="Recorded local reference"}};
    [Fact] public async Task OnlyConfiguredActiveFinancialDefinitionsPublishWithoutMutatingSource()
    {
        using var company=Company();using var state=new FinancialProcessCatalogState(company);var financial=Definition();var inactive=Definition();inactive.ProcessId="inactive";inactive.IsActive=false;var other=Definition();other.ProcessId="other";other.ProcessType="PRODUCTION";
        await state.LoadAsync(()=>Task.FromResult<string?>("field"),()=>Task.FromResult(new List<ProcessDefinition>{financial,inactive,other}));
        var row=Assert.Single(state.Rows);Assert.Equal("Recorded local reference",row.Regulation);Assert.Equal("",row.Version);Assert.Equal("/ppdm39/process/definitions/financial%2Fa%3Fb",row.Route);
        financial.ProcessName="Changed";financial.Configuration["regulation"]="Changed";Assert.Equal("Configured local process",row.Name);Assert.Equal("Recorded local reference",row.Regulation);
        company.Clear();Assert.False(state.Confirmed);Assert.Empty(state.Rows);Assert.Null(state.LoadedAtUtc);
    }
    [Theory][InlineData("null")][InlineData("duplicate")][InlineData("identity")][InlineData("type")][InlineData("name")][InlineData("entity")]
    public async Task MalformedCatalogDoesNotPublishSuccessfulEmptyOrPartialDefinitions(string fault)
    {
        using var company=Company();using var state=new FinancialProcessCatalogState(company);var row=Definition();List<ProcessDefinition>? rows=[row];
        switch(fault){case "null":rows=null;break;case "duplicate":rows.Add(row);break;case "identity":row.ProcessId="";break;case "type":row.ProcessType="";break;case "name":row.ProcessName="";break;default:row.EntityType="";break;}
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync(()=>Task.FromResult<string?>("field"),()=>Task.FromResult(rows!)));Assert.False(state.Confirmed);Assert.Empty(state.Rows);
    }
    [Fact] public async Task LateCatalogCannotPublishIntoNewCompany()
    {
        using var company=Company();using var state=new FinancialProcessCatalogState(company);var pending=new TaskCompletionSource<List<ProcessDefinition>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var load=state.LoadAsync(()=>Task.FromResult<string?>("field"),()=>pending.Task);company.Clear();pending.SetResult([Definition()]);await load;Assert.False(state.Confirmed);Assert.Empty(state.Rows);
    }
    [Fact] public async Task FailedReloadClearsEarlierCatalog()
    {
        using var company=Company();using var state=new FinancialProcessCatalogState(company);await state.LoadAsync(()=>Task.FromResult<string?>("field"),()=>Task.FromResult(new List<ProcessDefinition>{Definition()}));
        await Assert.ThrowsAsync<HttpRequestException>(()=>state.LoadAsync(()=>Task.FromResult<string?>("field"),()=>Task.FromException<List<ProcessDefinition>>(new HttpRequestException())));Assert.False(state.Confirmed);Assert.Empty(state.Rows);
    }
    [Fact] public async Task MissingOrChangedFieldCannotStartCatalogRead()
    {
        using var company=Company();using var state=new FinancialProcessCatalogState(company);await state.LoadAsync(()=>Task.FromResult<string?>(null),()=>throw new Exception("Must not read"));
        var field=new TaskCompletionSource<string?>();var load=state.LoadAsync(()=>field.Task,()=>throw new Exception("Must not read"));state.Clear();field.SetResult("field");await load;Assert.False(state.Confirmed);
    }
    [Fact] public async Task JsonMetadataAndUnknownRegulationRemainRecordedOrUnavailable()
    {
        using var company=Company();using var state=new FinancialProcessCatalogState(company);var row=Definition();row.Configuration["regulation"]=JsonSerializer.SerializeToElement("Recorded international reference");
        await state.LoadAsync(()=>Task.FromResult<string?>("field"),()=>Task.FromResult(new List<ProcessDefinition>{row}));Assert.Equal("Recorded international reference",Assert.Single(state.Rows).Regulation);
        row.Configuration["regulation"]=42;await state.LoadAsync(()=>Task.FromResult<string?>("field"),()=>Task.FromResult(new List<ProcessDefinition>{row}));Assert.Null(Assert.Single(state.Rows).Regulation);
    }
    [Fact] public async Task EmptyResponseIsConfirmedButDoesNotInventDefinitions()
    {
        using var company=Company();using var state=new FinancialProcessCatalogState(company);await state.LoadAsync(()=>Task.FromResult<string?>("field"),()=>Task.FromResult(new List<ProcessDefinition>()));Assert.True(state.Confirmed);Assert.Empty(state.Rows);Assert.NotNull(state.LoadedAtUtc);
    }
}
