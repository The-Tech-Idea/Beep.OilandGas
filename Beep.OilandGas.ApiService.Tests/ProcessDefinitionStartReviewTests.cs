using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class ProcessDefinitionStartReviewTests
{
    private static ProcessDefinition Definition() => new()
    {
        ProcessId = "definition", ProcessName = "Recorded preparation", ProcessType = "FINANCIAL",
        EntityType = "JOURNAL_ENTRY", IsActive = true,
        Steps = [new() { StepId = "prepare", StepName = "Prepare", SequenceNumber = 1, RequiredRoles = ["Accountant"] }]
    };
    private static ProcessInstanceRequest Request(ProcessDefinition definition) => new()
    {
        ProcessId = definition.ProcessId, EntityId = "journal", EntityType = definition.EntityType,
        ExpectedFieldId = "field", ExpectedDefinitionRevision = ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition))
    };
    [Fact] public void ExactConfiguredDefinitionCanBeReviewedWithoutGrantingAuthority()
    {
        var definition = Definition();
        ProcessDefinitionStartReview.Require(Request(definition), "field", definition);
    }
    [Theory]
    [InlineData("field")][InlineData("missing")][InlineData("identity")][InlineData("inactive")]
    [InlineData("type")][InlineData("changed")][InlineData("empty")][InlineData("duplicate")]
    [InlineData("sequence")][InlineData("roles")][InlineData("name")][InlineData("entity")][InlineData("revision")]
    public void ChangedOrMalformedOriginalReviewRefusesBeforeStart(string fault)
    {
        var definition = Definition(); var request = Request(definition);
        if (fault == "identity") definition.ProcessId = "another";
        if (fault == "inactive") definition.IsActive = false;
        if (fault == "type") request.EntityType = "WELL";
        if (fault == "changed") definition.Steps[0].RequiredRoles.Add("Reviewer");
        if (fault == "empty") definition.Steps.Clear();
        if (fault == "duplicate") definition.Steps.Add(definition.Steps[0]);
        if (fault == "sequence") definition.Steps[0].SequenceNumber = 0;
        if (fault == "roles") definition.Steps[0].RequiredRoles = null!;
        if (fault == "name") definition.ProcessName = "";
        if (fault == "entity") request.EntityId = "";
        if (fault == "revision") request.ExpectedDefinitionRevision = new string('z',64);
        // Even a caller hashing malformed data cannot bypass structural checks.
        if (fault is "empty" or "duplicate" or "sequence" or "roles" or "name")
            request.ExpectedDefinitionRevision = ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition));
        Assert.Throws<InvalidOperationException>(() => ProcessDefinitionStartReview.Require(request,
            fault == "field" ? "another" : "field", fault == "missing" ? null : definition));
    }
}
