#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyProcessStartTests
{
    internal sealed class Fixture
    {
        public PersonaTaskRoutingTests.Fixture Native = new();
        public PROCESS_DEFINITION Definition;
        public PPDMProcessService Service;
        public int FailWrite, Guards, RevokeAt;
        public bool ChangeDefinition;
        public Fixture(string role = "Accountant")
        {
            Native.Rows.Clear();
            Definition = new() { PROCESS_DEFINITION_ID = "definition", PROCESS_NAME = "Recorded invoice review", PROCESS_TYPE = "FINANCIAL", ENTITY_TYPE = "AP_INVOICE", IS_ACTIVE = "Y", VERSION = "1.1.0",
                PROCESS_CONFIG_JSON = JsonSerializer.Serialize(new { Steps = new[] {
                    new ProcessStepDefinition { StepId = "review", StepName = "Independent review", SequenceNumber = 1, RequiredRoles = [role], RequiresApproval = true },
                    new ProcessStepDefinition { StepId = "post", StepName = "Post", SequenceNumber = 2, RequiredRoles = ["Controller"] } } }) };
            Native.Rows.Add(Definition);
            Native.Personas.Setup(value => value.GetActivePersonasAsync(role)).ReturnsAsync(() => Native.ChangedDestinations && ++Native.PersonaReads > 1 ? new List<string> { "OTHER" } : Native.Destinations.ToList());
            Native.Source.Setup(source => source.InsertEntity(It.IsAny<string>(), It.IsAny<object>())).Returns((string table, object value) =>
            {
                Native.Writes++; if (Native.Writes == FailWrite) return new ErrorsInfo { Flag = Errors.Failed };
                Native.Rows.Add(JsonSerializer.Deserialize(JsonSerializer.Serialize(value), value.GetType())!);
                if (ChangeDefinition) Definition.VERSION = "changed";
                return new ErrorsInfo { Flag = Errors.Ok };
            });
            Service = new(Native.Editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), "lifecycle", personaReader: Native.Personas.Object);
        }
        public async Task<CompanyAccountingProcessStart> Intent()
        {
            var definition = await Service.GetProcessDefinitionAsync("definition");
            return new(new("tenant", "company"), "actor", "8d8c05f5070247aa831a6f550f7b15f7", "definition", "AP_INVOICE", "invoice",
                new string('A', 64), ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)), new string('B', 64), "invoice-preparer");
        }
        public Task Guard()
        {
            Guards++; if (Guards == RevokeAt) throw new UnauthorizedAccessException("Current company write review changed.");
            return Task.CompletedTask;
        }
        public Task<CompanyAccountingProcessStartReceipt> Start(CompanyAccountingProcessStart intent) => Service.StartCompanyAccountingAsync(intent, Guard);
    }
    [Fact] public async Task CompleteCompanyHandoffCommitsOnceAndUnchangedRetryWritesNothing()
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); var first = await fixture.Start(intent);
        Assert.Equal(6, fixture.Native.Writes); Assert.Equal(new[] { "begin", "commit" }, fixture.Native.Transactions);
        var process = Assert.Single(fixture.Native.Rows.OfType<PROCESS_INSTANCE>()); Assert.Equal("", process.FIELD_ID);
        var read = await fixture.Service.GetProcessInstanceAsync(first.InstanceId); Assert.NotNull(read);
        var binding = CompanyAccountingProcessBinding.FromJson(JsonSerializer.Serialize(read.ProcessData.Data[CompanyAccountingProcessBinding.PayloadKey]), intent.Company, first.InstanceId, intent.EntityType, intent.EntityId);
        Assert.Equal(intent.Company, binding.Company);
        var recorded = JsonSerializer.Deserialize<CompanyAccountingProcessStart>(JsonSerializer.Serialize(read.ProcessData.Data[CompanyAccountingProcessStart.PayloadKey]))!;
        Assert.Equal(intent, recorded); Assert.Equal("invoice-preparer", recorded.SourcePreparerUserId);
        Assert.Equal(2, read.StepInstances.Count); Assert.Single(read.History);
        Assert.All(fixture.Native.Rows.OfType<CROSS_PERSONA_TASK>(), task => { Assert.Equal(first.InstanceId, task.PROCESS_INSTANCE_ID); Assert.Equal("APPROVAL", task.TASK_TYPE); Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING", first.InstanceId, "AP_INVOICE"), task.ROUTE); });
        var retry = await fixture.Start(intent); Assert.Equal(first.InstanceId, retry.InstanceId); Assert.Equal(first.TaskIds, retry.TaskIds); Assert.Equal(first.RecordedRevision, retry.RecordedRevision);
        Assert.Equal(6, fixture.Native.Writes); Assert.Equal(new[] { "begin", "commit", "begin", "commit" }, fixture.Native.Transactions);
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)]
    public async Task EveryGraphOrPersonaInsertFailureRollsBackTheWholeHandoff(int write)
    {
        var fixture = new Fixture { FailWrite = write }; var intent = await fixture.Intent();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent)); Assert.Single(fixture.Native.Rows);
        Assert.Equal(new[] { "begin", "rollback" }, fixture.Native.Transactions);
    }
    [Theory][InlineData("actor")][InlineData("preparer")][InlineData("source")][InlineData("revision")][InlineData("binding")]
    public async Task ChangedIntentCannotAdoptAnExistingHandoff(string changed)
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); await fixture.Start(intent);
        var conflict = changed switch { "actor" => intent with { ActorUserId = "other" }, "preparer" => intent with { SourcePreparerUserId = "other" },
            "source" => intent with { EntityId = "foreign" }, "revision" => intent with { SourceRevision = new string('C', 64) }, _ => intent with { BindingRevision = new string('D', 64) } };
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(conflict)); Assert.Equal(6, fixture.Native.Writes);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task MissingOrPartialRecordedTaskSetIsNotSilentlyRepaired(bool missing)
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); await fixture.Start(intent);
        if (missing) fixture.Native.Rows.RemoveAll(row => row is CROSS_PERSONA_TASK); else fixture.Native.Rows.Remove(fixture.Native.Rows.OfType<CROSS_PERSONA_TASK>().First());
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent)); Assert.Equal(6, fixture.Native.Writes);
    }
    [Fact] public async Task UncertainCommitCanBeConfirmedByTheSameFrozenIntent()
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); fixture.Native.FailCommit = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent)); Assert.Equal(6, fixture.Native.Writes);
        fixture.Native.FailCommit = false; var receipt = await fixture.Start(intent); Assert.Equal(intent.InstanceId(), receipt.InstanceId); Assert.Equal(6, fixture.Native.Writes);
    }
    [Fact] public async Task ChangedDefinitionOrPersonaDestinationsRollBackAllRows()
    {
        foreach (var definition in new[] { true, false })
        {
            var fixture = new Fixture { ChangeDefinition = definition }; fixture.Native.ChangedDestinations = !definition; var intent = await fixture.Intent();
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent)); Assert.Single(fixture.Native.Rows);
        }
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]
    public async Task RevokedCurrentReviewNeverReturnsSuccessfulHandoff(int boundary)
    {
        var fixture = new Fixture { RevokeAt = boundary }; var intent = await fixture.Intent();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Start(intent)); Assert.Single(fixture.Native.Rows);
    }
    [Fact] public async Task DifferentIntentForSameOwnedActiveSourceRefusesAnotherProcess()
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); await fixture.Start(intent);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent with { IntentId = Guid.NewGuid().ToString("N") })); Assert.Equal(6, fixture.Native.Writes);
    }
    [Fact] public async Task ProgressedProcessCannotBeReopenedByStartRetry()
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); await fixture.Start(intent);
        fixture.Native.Rows.OfType<PROCESS_INSTANCE>().Single().STATUS = "COMPLETED";
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent)); Assert.Equal(6, fixture.Native.Writes);
    }
    [Theory][InlineData("step")][InlineData("audit")][InlineData("owner")]
    public async Task ChangedRecordedGraphCannotBeAdopted(string changed)
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); await fixture.Start(intent);
        if (changed == "step") fixture.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().First().REQUIRED_ROLE = "Controller";
        if (changed == "audit") fixture.Native.Rows.OfType<PROCESS_HISTORY>().Single().USER_ID = "other";
        if (changed == "owner") fixture.Native.Rows.OfType<PROCESS_INSTANCE>().Single().INSTANCE_DATA_JSON = "{}";
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent)); Assert.Equal(6, fixture.Native.Writes);
    }
    [Fact] public async Task DroppedCommittedTasksRequireReconciliationRatherThanRetryRepair()
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); fixture.Native.DropCommittedTasks = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent)); Assert.Single(fixture.Native.Rows.OfType<PROCESS_INSTANCE>());
        fixture.Native.DropCommittedTasks = false; await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent)); Assert.Equal(6, fixture.Native.Writes);
    }
    [Theory][InlineData("actor",false)][InlineData("invoice-preparer",false)][InlineData("reviewer",true)]
    public async Task RecordedCompanyStarterAndSourcePreparerCannotCheckEvenAsAdministrator(string actor,bool allowed)
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); var receipt = await fixture.Start(intent);
        var instance = (await fixture.Service.GetProcessInstanceAsync(receipt.InstanceId))!;
        var definition = (await fixture.Service.GetProcessDefinitionAsync(intent.ProcessId))!;
        var user = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"Administrator")],"test"));
        Assert.Equal(allowed,ProcessStepRoleChecks.CanAct(user,actor,instance,instance.StepInstances[0],definition,definition.Steps[0]));
        instance.CurrentState = "review";
        definition.Transitions.Add("approve",new(){FromStateId="review",ToStateId="approved",Trigger="approve",RequiresApproval=true});
        Assert.Equal(allowed,ProcessStepRoleChecks.CanTransition(user,actor,instance,definition,"review","approved","approve"));
    }
    [Fact] public async Task MissingCompanyPreparerProofCannotUnlockIndependentAction()
    {
        var fixture = new Fixture(); var intent = await fixture.Intent(); var receipt = await fixture.Start(intent);
        var instance = (await fixture.Service.GetProcessInstanceAsync(receipt.InstanceId))!;
        var definition = (await fixture.Service.GetProcessDefinitionAsync(intent.ProcessId))!;
        instance.ProcessData.Data.Remove(CompanyAccountingProcessStart.PayloadKey);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.Role,"Administrator")],"test"));
        Assert.False(ProcessStepRoleChecks.CanAct(user,"reviewer",instance,instance.StepInstances[0],definition,definition.Steps[0]));
    }
    [Theory][InlineData("intent")][InlineData("preparer")][InlineData("revision")]
    public async Task IncompleteReviewedIntentNeverBeginsATransaction(string missing)
    {
        var fixture = new Fixture(); var intent = await fixture.Intent();
        intent = missing switch { "intent" => intent with { IntentId = "" }, "preparer" => intent with { SourcePreparerUserId = "" }, _ => intent with { SourceRevision = "" } };
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Start(intent)); Assert.Empty(fixture.Native.Transactions); Assert.Equal(0,fixture.Native.Writes);
    }
    [Fact] public async Task PostCommitAuthorityLossReturnsNoReceiptAndSameIntentCanLaterConfirm()
    {
        var fixture = new Fixture { RevokeAt = 5 }; var intent = await fixture.Intent();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Start(intent));
        Assert.Equal(new[]{"begin","commit"},fixture.Native.Transactions);Assert.Equal(6,fixture.Native.Writes);
        var receipt = await fixture.Start(intent); Assert.Equal(intent.InstanceId(),receipt.InstanceId);Assert.Equal(6,fixture.Native.Writes);
    }
}
#endif
