#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Report;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class NativeProcessStartTests
{
    private sealed class RecordedIdsService(PersonaTaskRoutingTests.Fixture native) : PPDMProcessService(native.Editor.Object,
        Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), "lifecycle")
    {
        private int step;
        protected override string GenerateInstanceId() => "recorded-process";
        protected override string GenerateHistoryId() => "recorded-history";
        protected override string GenerateStepInstanceId() => "recorded-step-" + ++step;
    }
    private sealed class Fixture
    {
        public PersonaTaskRoutingTests.Fixture Native = new();
        public PROCESS_DEFINITION Definition;
        public PPDMProcessService Service;
        public int FailWrite;
        public string? CorruptTable;
        public bool ChangeDefinition;
        public Fixture()
        {
            Native.Rows.Clear();
            Definition = new() { PROCESS_DEFINITION_ID = "definition", PROCESS_NAME = "Recorded source review", ENTITY_TYPE = "AP_INVOICE", IS_ACTIVE = "Y", VERSION = "1.1.0" };
            Steps([new() { StepId = "prepare", StepName = "Prepare", SequenceNumber = 1, RequiredRoles = ["Accountant"] },
                new() { StepId = "review", StepName = "Review", SequenceNumber = 2, RequiredRoles = ["Controller"], RequiresApproval = true }]);
            Native.Rows.Add(Definition);
            Native.Source.Setup(source => source.InsertEntity(It.IsAny<string>(), It.IsAny<object>())).Returns((string table, object row) =>
            {
                Native.Writes++;
                if (Native.Writes == FailWrite) return new ErrorsInfo { Flag = Errors.Failed };
                var recorded = JsonSerializer.Deserialize(JsonSerializer.Serialize(row), row.GetType())!;
                if (table == CorruptTable)
                {
                    if (recorded is PROCESS_INSTANCE process) process.ENTITY_ID = "foreign";
                    if (recorded is PROCESS_STEP_INSTANCE step) step.REQUIRED_ROLE = "ForeignRole";
                    if (recorded is PROCESS_HISTORY history) history.USER_ID = "foreign";
                }
                Native.Rows.Add(recorded);
                if (ChangeDefinition) Definition.VERSION = "changed";
                return new ErrorsInfo { Flag = Errors.Ok };
            });
            Service = new RecordedIdsService(Native);
        }
        public void Steps(List<ProcessStepDefinition> steps) => Definition.PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = steps });
        public Task<ProcessInstance> Start(string field = "field") => Service.StartProcessAsync("definition", "invoice", "AP_INVOICE", field, "actor");
    }

    [Fact] public async Task GenericStartMutationAndPublicTaskRoutingCannotCreateUnownedCompletionApproval()
    {
        var f=new Fixture();
        f.Definition.ENTITY_TYPE="DRILLING_DAILY_REPORT";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.StartProcessAsync("definition","reported-claim","DRILLING_DAILY_REPORT","field","actor"));
        Assert.Equal(0,f.Native.Writes);
        f.Definition.ENTITY_TYPE="AP_INVOICE";
        var process=await f.Start();var writes=f.Native.Writes;
        Assert.Single(f.Native.Rows.OfType<PROCESS_INSTANCE>()).ENTITY_TYPE="DRILLING_DAILY_REPORT";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.CancelProcessAsync(process.InstanceId,"Unqualified completion decision","actor"));
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SkipStepAsync(process.InstanceId,"prepare","Unqualified completion decision","actor"));
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.AddHistoryEntryAsync(process.InstanceId,new(){Action="UNQUALIFIED_COMPLETION"}));
        var router=new CrossPersonaTaskRouter(f.Native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),()=>Task.FromResult("lifecycle"),personas:f.Native.Personas.Object);
        await Assert.ThrowsAnyAsync<Exception>(()=>router.RouteTaskAsync(process.InstanceId,process.StepInstances[0].StepInstanceId,"Accountant","DRILLING_DAILY_REPORT","reported-claim","Unqualified completion review","actor"));
        Assert.Equal(writes,f.Native.Writes);
    }
    [Theory][InlineData("FACILITY_DEVELOPMENT","FACILITY")][InlineData("other","FACILITY")][InlineData("WELL_DEVELOPMENT","WELL")]
    public async Task GenericStartAndMutationCannotAdvanceUnqualifiedConstructionOrHandover(string definition,string entityType)
    {
        var f=new Fixture();
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.StartProcessAsync(definition,"source",entityType,"field","actor"));
        Assert.Equal(0,f.Native.Writes);
        var process=await f.Start();var writes=f.Native.Writes;var recorded=Assert.Single(f.Native.Rows.OfType<PROCESS_INSTANCE>());
        recorded.ENTITY_TYPE=entityType;recorded.PROCESS_DEFINITION_ID=definition;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.CompleteStepAsync(process.InstanceId,"prepare","ACCEPTED","actor"));
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.CancelProcessAsync(process.InstanceId,"unqualified","actor"));
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SkipStepAsync(process.InstanceId,"prepare","unqualified","actor"));
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.AddHistoryEntryAsync(process.InstanceId,new(){Action="UNQUALIFIED_HANDOVER"}));
        Assert.Equal(writes,f.Native.Writes);
    }
    [Fact] public async Task GenericTaskRoutingCannotCreateAnUnqualifiedFacilityAssignment()
    {
        var f=new Fixture();var process=await f.Start();var writes=f.Native.Writes;
        var router=new CrossPersonaTaskRouter(f.Native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),()=>Task.FromResult("lifecycle"),personas:f.Native.Personas.Object);
        await Assert.ThrowsAnyAsync<Exception>(()=>router.RouteTaskAsync(process.InstanceId,process.StepInstances[0].StepInstanceId,"Accountant","FACILITY","facility","Unqualified construction","actor"));
        Assert.Equal(writes,f.Native.Writes);
    }
    [Fact] public async Task StartCommitsTheCompleteInitialGraphAndReturnsRecordedAudit()
    {
        var fixture = new Fixture(); var result = await fixture.Start();
        Assert.Equal(new[] { "begin", "commit" }, fixture.Native.Transactions);
        Assert.Equal(4, fixture.Native.Writes); Assert.Single(fixture.Native.Rows.OfType<PROCESS_INSTANCE>());
        Assert.Equal(2, fixture.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().Count());
        Assert.Equal("PROCESS_STARTED", Assert.Single(result.History).Action);
        Assert.Equal(StepStatus.PENDING, result.StepInstances[0].Status); Assert.Equal(StepStatus.BLOCKED, result.StepInstances[1].Status);
        Assert.Equal("Controller", result.StepInstances[1].RequiredRole); Assert.True(result.StepInstances[1].ApprovalRequired);
        var recorded = await fixture.Service.GetProcessInstanceAsync(result.InstanceId);
        Assert.NotNull(recorded); Assert.Equal(result.History[0].HistoryId, Assert.Single(recorded.History).HistoryId);
        result.History.Clear(); Assert.Single(fixture.Native.Rows.OfType<PROCESS_HISTORY>());
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]
    public async Task AnyInitialGraphWriteFailureRollsBackEveryNewRow(int write)
    {
        var fixture = new Fixture { FailWrite = write };
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start());
        Assert.Single(fixture.Native.Rows); Assert.IsType<PROCESS_DEFINITION>(fixture.Native.Rows[0]);
        Assert.Equal(new[] { "begin", "rollback" }, fixture.Native.Transactions);
    }
    [Theory][InlineData("PROCESS_INSTANCE")][InlineData("PROCESS_STEP_INSTANCE")][InlineData("PROCESS_HISTORY")]
    public async Task CorruptReadbackRollsBackTheGraph(string table)
    {
        var fixture = new Fixture { CorruptTable = table };
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start()); Assert.Single(fixture.Native.Rows);
        Assert.Equal(new[] { "begin", "rollback" }, fixture.Native.Transactions);
    }
    [Fact] public async Task DefinitionChangesDuringInsertionRollBackTheGraph()
    {
        var fixture = new Fixture { ChangeDefinition = true };
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start()); Assert.Single(fixture.Native.Rows);
    }
    [Theory][InlineData("inactive")][InlineData("source")][InlineData("duplicate")][InlineData("steps")][InlineData("sequence")][InlineData("roles")][InlineData("sla")]
    public async Task InvalidDefinitionNeverWrites(string fault)
    {
        var fixture = new Fixture();
        switch (fault)
        {
            case "inactive": fixture.Definition.ACTIVE_IND = "N"; break;
            case "source": fixture.Definition.ENTITY_TYPE = "PURCHASE_ORDER"; break;
            case "duplicate": fixture.Native.Rows.Add(fixture.Definition); break;
            case "steps": fixture.Steps([new() { StepId = "same" }, new() { StepId = "same", SequenceNumber = 1 }]); break;
            case "sequence": fixture.Steps([new() { StepId = "first" }, new() { StepId = "second" }]); break;
            case "roles": fixture.Steps([new() { StepId = "first", RequiredRoles = ["Accountant", "Controller"] }]); break;
            case "sla": fixture.Steps([new() { StepId = "first", SlaHours = -1 }]); break;
        }
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start()); Assert.Equal(0, fixture.Native.Writes);
    }
    [Fact] public async Task GenericFieldStartCannotCreateAnUnownedCompanyProcess()
    {
        var fixture = new Fixture(); await Assert.ThrowsAsync<ArgumentException>(() => fixture.Start(""));
        Assert.Empty(fixture.Native.Transactions); Assert.Equal(0, fixture.Native.Writes);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task BorrowedOrWeakTransactionNeverWrites(bool borrowed)
    {
        var fixture = new Fixture(); fixture.Native.Borrowed = borrowed;
        if (!borrowed) fixture.Native.Isolation = IsolationLevel.ReadCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start()); Assert.Equal(0, fixture.Native.Writes);
    }
    [Fact] public async Task UncertainCommitDoesNotReturnSuccessfulProcessStart()
    {
        var fixture = new Fixture(); fixture.Native.FailCommit = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start());
        Assert.Single(fixture.Native.Rows.OfType<PROCESS_INSTANCE>()); Assert.Equal(new[] { "begin", "commit" }, fixture.Native.Transactions);
    }
    [Theory][InlineData("process")][InlineData("step")][InlineData("history")]
    public async Task CollidingIdentitiesAreNeverAdoptedOrUpdated(string kind)
    {
        var fixture = new Fixture();
        object collision = kind switch
        {
            "process" => new PROCESS_INSTANCE { PROCESS_INSTANCE_ID = "recorded-process", ENTITY_ID = "foreign" },
            "step" => new PROCESS_STEP_INSTANCE { PROCESS_STEP_INSTANCE_ID = "recorded-step-1", PROCESS_INSTANCE_ID = "foreign" },
            _ => new PROCESS_HISTORY { PROCESS_HISTORY_ID = "recorded-history", PROCESS_INSTANCE_ID = "foreign" }
        };
        fixture.Native.Rows.Add(collision);
        var before = JsonSerializer.Serialize(collision);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start()); Assert.Equal(0, fixture.Native.Writes);
        Assert.Equal(before, JsonSerializer.Serialize(fixture.Native.Rows.Single(row => row.GetType() == collision.GetType())));
        fixture.Native.Source.Verify(source => source.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }
    [Fact] public async Task SuccessfulCommitFlagWithoutRecordedGraphCannotReturnSuccess()
    {
        var fixture = new Fixture();
        fixture.Native.Source.Setup(source => source.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>()))
            .ReturnsAsync((string table, List<AppFilter> filters) =>
            {
                if (fixture.Native.Transactions.LastOrDefault() == "commit") fixture.Native.Rows.RemoveAll(row => row is PROCESS_INSTANCE);
                return fixture.Native.Rows.Where(row => row.GetType().Name == table && filters.All(filter =>
                    row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString() == filter.FilterValue))
                    .Select(row => JsonSerializer.Deserialize(JsonSerializer.Serialize(row), row.GetType())!).ToArray();
            });
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start());
        Assert.Equal(new[] { "begin", "commit" }, fixture.Native.Transactions);
    }
}
#endif
