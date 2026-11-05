using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Processes;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class ProcessDefinitionCatalogTests
{
    [Fact] public async Task ExistingFinancialCategoryIsIncludedWithoutInventedDefinitions()
    {
        var calls=new List<string>();var definition=new ProcessDefinition{ProcessId="actual-financial",ProcessType="FINANCIAL"};
        var rows=await ProcessDefinitionCatalog.ReadAsync(category=>{calls.Add(category);return Task.FromResult(category=="FINANCIAL"?new List<ProcessDefinition>{definition}:[]);});
        Assert.Same(definition,Assert.Single(rows));Assert.Contains("FINANCIAL",calls);Assert.Equal(9,calls.Distinct().Count());
    }
    [Theory][InlineData("null")][InlineData("identity")][InlineData("category")][InlineData("duplicate")]
    public async Task MissingOrAmbiguousCategoryCannotProduceSuccessfulPartialCatalog(string fault)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(()=>ProcessDefinitionCatalog.ReadAsync(category=>
        {
            if(category!="FINANCIAL")return Task.FromResult(new List<ProcessDefinition>());
            if(fault=="null")return Task.FromResult<List<ProcessDefinition>>(null!);
            var row=new ProcessDefinition{ProcessId=fault=="identity"?"":"definition",ProcessType=fault=="category"?"WORK_ORDER":"FINANCIAL"};
            return Task.FromResult(fault=="duplicate"?new List<ProcessDefinition>{row,row}:new List<ProcessDefinition>{row});
        }));
    }
}
