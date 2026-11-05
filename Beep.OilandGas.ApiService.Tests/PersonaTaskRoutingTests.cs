#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Addin;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public interface ITaskTransactionEvidence { IDbTransaction? ActiveTransaction {get;} }
public interface ITaskConnectionEvidence { IDbConnection DbConn {get;} }
public sealed class PersonaTaskRoutingTests
{
    internal sealed class Fixture
    {
        public Mock<IDataSource> Source=new();
        public Mock<IDMEEditor> Editor=new();
        public Mock<IRolePersonaReader> Personas=new(MockBehavior.Strict);
        public List<object> Rows=[];
        public List<string> Transactions=[];
        public int Writes,FailWrite,PersonaReads;
        public string Connection="lifecycle";
        public string[] Destinations=["ACCOUNTANT","CONTROLLER"];
        public Action? AfterCommit;
        public bool ChangedDestinations,ChangedStep,ChangedBinding,FailCommit,Borrowed,CorruptReadback,DropCommittedTasks;
        public IsolationLevel Isolation=IsolationLevel.Serializable;
        public PROCESS_INSTANCE Process;
        public PROCESS_STEP_INSTANCE Step;
        public CrossPersonaTaskRouter Router;
        private IDbTransaction? active;
        private List<object> before=[];
        private static object Clone(object value)=>JsonSerializer.Deserialize(JsonSerializer.Serialize(value),value.GetType())!;
        public Fixture(bool company=true)
        {
            Process=new(){PROCESS_INSTANCE_ID="process/حساب%2F",PROCESS_DEFINITION_ID="definition",ENTITY_TYPE=company?"AP_INVOICE":"WELL",ENTITY_ID="source",FIELD_ID=company?null:"field",
                STATUS="IN_PROGRESS",STARTED_DATE=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc),CURRENT_STEP_ID="review",INSTANCE_DATA_JSON=JsonSerializer.Serialize(new{InstanceId="process/حساب%2F",Data=new Dictionary<string,object>{
                    [CompanyAccountingProcessBinding.PayloadKey]=new CompanyAccountingProcessBinding(new("tenant","company"),"process/حساب%2F","AP_INVOICE","source")}})};
            Step=new(){PROCESS_INSTANCE_ID=Process.PROCESS_INSTANCE_ID,PROCESS_STEP_INSTANCE_ID="step",STEP_ID="review",REQUIRED_ROLE="Accountant",STATUS="PENDING",APPROVAL_REQUIRED=true};
            Rows.Add(Process);Rows.Add(Step);
            var physical=new Mock<IDbConnection>();physical.SetupGet(value=>value.State).Returns(ConnectionState.Open);
            var connection=new Mock<IDataConnection>();connection.As<ITaskConnectionEvidence>().SetupGet(value=>value.DbConn).Returns(physical.Object);
            Source.SetupGet(value=>value.Dataconnection).Returns(connection.Object);Source.SetupGet(value=>value.DatasourceName).Returns("lifecycle");
            Source.As<ITaskTransactionEvidence>().SetupGet(value=>value.ActiveTransaction).Returns(()=>Borrowed?Mock.Of<IDbTransaction>():active);
            Source.Setup(value=>value.BeginTransaction(It.IsAny<PassedArgs>())).Returns(()=>{
                Transactions.Add("begin");before=Rows.Select(Clone).ToList();var transaction=new Mock<IDbTransaction>();transaction.SetupGet(value=>value.Connection).Returns(physical.Object);
                transaction.SetupGet(value=>value.IsolationLevel).Returns(Isolation);active=transaction.Object;return new ErrorsInfo{Flag=Errors.Ok};});
            Source.Setup(value=>value.Commit(It.IsAny<PassedArgs>())).Returns(()=>{Transactions.Add("commit");active=null;
                if(DropCommittedTasks)Rows.RemoveAll(row=>row is CROSS_PERSONA_TASK);AfterCommit?.Invoke();return new ErrorsInfo{Flag=FailCommit?Errors.Failed:Errors.Ok};});
            Source.Setup(value=>value.EndTransaction(It.IsAny<PassedArgs>())).Returns(()=>{Transactions.Add("rollback");Rows.Clear();Rows.AddRange(before);active=null;return new ErrorsInfo{Flag=Errors.Ok};});
            Source.Setup(value=>value.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>
                Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(Clone).ToArray());
            Source.Setup(value=>value.InsertEntity("CROSS_PERSONA_TASK",It.IsAny<object>())).Returns((string table,object row)=>{
                Writes++;if(Writes==FailWrite)return new ErrorsInfo{Flag=Errors.Failed};var recorded=(CROSS_PERSONA_TASK)Clone(row);
                if(CorruptReadback)recorded.PRIORITY=1;Rows.Add(recorded);if(ChangedStep)Step.STATUS="COMPLETED";
                if(ChangedBinding)Connection="changed";return new ErrorsInfo{Flag=Errors.Ok};});
            Editor.Setup(value=>value.GetDataSource(It.IsAny<string>())).Returns(Source.Object);
            Personas.Setup(value=>value.GetActivePersonasAsync("Accountant")).ReturnsAsync(()=>{PersonaReads++;return ChangedDestinations && PersonaReads>1?new List<string>{"OTHER"}:Destinations.ToList();});
            Router=new(Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),()=>Task.FromResult(Connection),NullLogger<CrossPersonaTaskRouter>.Instance,Personas.Object);
        }
        public Task<List<CROSS_PERSONA_TASK>> Route()=>Router.RouteTaskAsync(Process.PROCESS_INSTANCE_ID,"step","Accountant",Process.ENTITY_TYPE,"source","Recorded source","actor");
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task CompletePersonaSetCommitsOnceAndExactRetryAddsNoTask(bool company)
    {
        var fixture=new Fixture(company);var first=await fixture.Route();Assert.Equal(2,first.Count);Assert.Equal(2,fixture.Writes);
        Assert.All(first,row=>Assert.Equal(WorkflowTaskRoute.For(company?"COMPANY_ACCOUNTING":"FIELD",fixture.Process.PROCESS_INSTANCE_ID,fixture.Process.ENTITY_TYPE),row.ROUTE));
        var retry=await fixture.Route();Assert.Equal(first.Select(row=>row.CROSS_TASK_ID),retry.Select(row=>row.CROSS_TASK_ID));Assert.Equal(2,fixture.Writes);
        Assert.Equal(new[]{"begin","commit","begin","commit"},fixture.Transactions);Assert.Equal(2,fixture.Rows.OfType<CROSS_PERSONA_TASK>().Count());
    }
    [Fact] public async Task SecondPersonaFailureRollsBackTheFirstTask()
    {var fixture=new Fixture{FailWrite=2};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Route());Assert.Empty(fixture.Rows.OfType<CROSS_PERSONA_TASK>());Assert.Equal(new[]{"begin","rollback"},fixture.Transactions);}
    [Theory][InlineData("missingStep")][InlineData("parent")][InlineData("role")][InlineData("completed")][InlineData("source")][InlineData("owner")][InlineData("inactive")]
    [InlineData("duplicateProcess")][InlineData("duplicateStep")][InlineData("duplicatePersona")][InlineData("emptyPersona")][InlineData("sla")]
    public async Task InvalidParentAssignmentOrCompanyBindingNeverInserts(string fault)
    {
        var fixture=new Fixture();switch(fault){case "missingStep":fixture.Rows.Remove(fixture.Step);break;case "parent":fixture.Step.PROCESS_INSTANCE_ID="foreign";break;
            case "role":fixture.Step.REQUIRED_ROLE="Buyer";break;case "completed":fixture.Step.STATUS="COMPLETED";break;case "source":fixture.Process.ENTITY_ID="foreign";break;
            case "owner":fixture.Process.INSTANCE_DATA_JSON="{}";break;case "inactive":fixture.Process.ACTIVE_IND="N";break;
            case "duplicateProcess":fixture.Rows.Add(fixture.Process);break;case "duplicateStep":fixture.Rows.Add(fixture.Step);break;
            case "duplicatePersona":fixture.Destinations=["ACCOUNTANT","ACCOUNTANT"];break;case "emptyPersona":fixture.Destinations=[];break;case "sla":fixture.Step.SLA_HOURS=-1;break;}
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Route());Assert.Equal(0,fixture.Writes);Assert.Empty(fixture.Rows.OfType<CROSS_PERSONA_TASK>());
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task ClosingDestinationsOrStepChangeRollsBackAllTasks(bool step)
    {
        var fixture=new Fixture{ChangedStep=step,ChangedDestinations=!step};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Route());
        Assert.Empty(fixture.Rows.OfType<CROSS_PERSONA_TASK>());Assert.Equal(new[]{"begin","rollback"},fixture.Transactions);
    }
    [Fact] public async Task CompletedRecordedTaskCannotBeReopenedByRetry()
    {
        var fixture=new Fixture();await fixture.Route();fixture.Rows.OfType<CROSS_PERSONA_TASK>().First().TASK_STATUS="COMPLETED";
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Route());Assert.Equal(2,fixture.Writes);
    }
    [Fact] public async Task ReadCommittedCannotAuthorizeTaskWrites()
    {var fixture=new Fixture{Isolation=IsolationLevel.ReadCommitted};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Route());Assert.Equal(0,fixture.Writes);}
    [Fact] public async Task BorrowedTransactionIsNeverJoinedOrCommitted()
    {var fixture=new Fixture{Borrowed=true};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Route());Assert.Empty(fixture.Transactions);Assert.Equal(0,fixture.Writes);}
    [Theory][InlineData(false)][InlineData(true)] public async Task ChangedBindingOrCorruptReadbackRollsBack(bool binding)
    {var fixture=new Fixture{ChangedBinding=binding,CorruptReadback=!binding};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Route());Assert.Empty(fixture.Rows.OfType<CROSS_PERSONA_TASK>());Assert.Equal(new[]{"begin","rollback"},fixture.Transactions);}
    [Fact] public async Task UncertainCommitReturnsNoSuccessAndExactRetryReadsRecordedTasks()
    {
        var fixture=new Fixture{FailCommit=true};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Route());Assert.Equal(2,fixture.Rows.OfType<CROSS_PERSONA_TASK>().Count());
        fixture.FailCommit=false;Assert.Equal(2,(await fixture.Route()).Count);Assert.Equal(2,fixture.Writes);Assert.DoesNotContain("rollback",fixture.Transactions);
    }
    [Fact] public async Task PartialRecordedDestinationSetIsNotSilentlyRepaired()
    {
        var fixture=new Fixture();await fixture.Route();fixture.Rows.Remove(fixture.Rows.OfType<CROSS_PERSONA_TASK>().First());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Route());Assert.Equal(2,fixture.Writes);Assert.Single(fixture.Rows.OfType<CROSS_PERSONA_TASK>());
    }
    [Fact] public async Task SuccessfulCommitFlagWithoutRecordedTasksCannotReturnSuccess()
    {
        var fixture=new Fixture{DropCommittedTasks=true};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Route());
        Assert.Equal(new[]{"begin","commit"},fixture.Transactions);Assert.Empty(fixture.Rows.OfType<CROSS_PERSONA_TASK>());
    }
}
#endif
