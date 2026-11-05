using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.DevelopmentPlanning.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentPlanNativeDraftTests
{
    internal static CreateDevelopmentPlan Input()=>new(){SelectionContext=new(new("tenant","company"),new("core","core-binding"),"planner","preference","field"),
        PlanningDatabase=new("planning","planning-binding"),IntentId=Guid.NewGuid(),FieldId="field",PlanName="Recorded field concept",Description="Reviewed description",
        PlannedFirstProductionDate=new(2030,4,1),PlannedFieldLifeEndDate=new(2031,4,1),EstimatedCapexMillions=12.125m,CapexCurrency="KWD"};
    internal sealed class Fixture
    {
        internal PersonaTaskRoutingTests.Fixture Native=new();
        internal DevelopmentPlanService Service;
        internal CreateDevelopmentPlan Request=Input();
        internal bool NullRows,FailInsert,CorruptInsert,Revoke;
        internal Action? OnInsert;
        internal Action<EntityStructure>? ChangeSchema;
        internal int Reads;
        internal Fixture()
        {
            Native.Connection="planning";Native.Rows.Clear();
            Native.Source.SetupGet(source=>source.DatasourceName).Returns("planning");
            var configured=new ConnectionProperties{ConnectionName="planning",DatabaseType=DataSourceType.SqlServer,Host="fixture",Database="planning"};
            var config=new Mock<IConfigEditor>();config.SetupGet(value=>value.DataConnections).Returns(new List<ConnectionProperties>{configured});
            Native.Editor.SetupGet(value=>value.ConfigEditor).Returns(config.Object);
            Mock.Get(Native.Source.Object.Dataconnection).SetupGet(value=>value.ConnectionProp).Returns(configured);
            Native.Rows.Add(new R_DEVELOPMENT_PLANNING_REFERENCE_CODE{REFERENCE_SET="FDP_STATUS",REFERENCE_CODE="DRAFT",ACTIVE_IND="Y"});
            Native.Rows.Add(new R_DEVELOPMENT_PLANNING_REFERENCE_CODE{REFERENCE_SET="COST_CURRENCY",REFERENCE_CODE="KWD",ACTIVE_IND="Y"});
            Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool refresh)=>{
                var type=requested.EntityName=="FIELD_DEVELOPMENT_PLAN"?typeof(FIELD_DEVELOPMENT_PLAN):typeof(R_DEVELOPMENT_PLANNING_REFERENCE_CODE);
                var result=new ClassCreator(Native.Editor.Object).ConvertToEntityStructure(type);ChangeSchema?.Invoke(result);return result;});
            Native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                Reads++;return NullRows?null!:Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(Clone).ToArray();});
            Native.Source.Setup(source=>source.InsertEntity("FIELD_DEVELOPMENT_PLAN",It.IsAny<object>())).Returns((string table,object value)=>{
                Native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=(FIELD_DEVELOPMENT_PLAN)Clone(value);
                if(CorruptInsert)row.FDP_NAME="corrupted";Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            Service=new(Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),()=>Task.FromResult(Native.Connection));
        }
        internal Task Authority(){if(Revoke)throw new UnauthorizedAccessException();return Task.CompletedTask;}
        internal Task<FIELD_DEVELOPMENT_PLAN> Save()=>Service.CreateDevelopmentPlanAsync(Request,Authority);
        internal Task<DevelopmentPlanDraftRecovery> Recover()=>Service.RecoverDevelopmentPlanAsync(Request,Request.SelectionContext,Request.PlanningDatabase,Authority);
        private static object Clone(object value)=>JsonSerializer.Deserialize(JsonSerializer.Serialize(value),value.GetType())!;
    }
    [Fact] public async Task SavedDraftReadsBackOriginalInputAndExactRetryDoesNotInsertAgain()
    {
        var fixture=new Fixture();var saved=await fixture.Save();DevelopmentPlanDraftReview.RequireRecorded(fixture.Request,saved);
        Assert.Equal("planner",saved.ROW_CREATED_BY);Assert.Equal("KWD",saved.CAPEX_CURRENCY);Assert.Null(saved.SUBMISSION_DATE);Assert.Null(saved.APPROVAL_DATE);
        saved.FDP_NAME="local mutation";Assert.Equal(fixture.Request.PlanName,(await fixture.Save()).FDP_NAME);Assert.Equal(1,fixture.Native.Writes);
        Assert.NotNull((await fixture.Recover()).Plan);var list=await fixture.Service.GetDevelopmentPlansAsync(fixture.Request.SelectionContext,fixture.Request.PlanningDatabase,fixture.Authority);
        Assert.Single(list.Plans);Assert.Equal(new[]{"KWD"},(await fixture.Service.GetDraftOptionsAsync(fixture.Request.SelectionContext,fixture.Request.PlanningDatabase,fixture.Authority)).Currencies);
    }
    [Fact] public async Task SuccessfulEmptyRecoveryIsDistinctFromUnavailableRowset()
    {var fixture=new Fixture();Assert.Null((await fixture.Recover()).Plan);fixture.NullRows=true;await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Recover());Assert.Equal(0,fixture.Native.Writes);}
    [Fact] public async Task ChangedInputCannotReuseOriginalIntent()
    {var fixture=new Fixture();await fixture.Save();fixture.Request=fixture.Request with{PlanName="Different input"};await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Save());Assert.Equal(1,fixture.Native.Writes);}
    [Theory][InlineData("currency")][InlineData("status")][InlineData("duplicate-reference")][InlineData("null")][InlineData("nullable-owner")][InlineData("width")][InlineData("key")][InlineData("read-committed")][InlineData("borrowed")]
    public async Task UnqualifiedSourceCannotInsert(string fault)
    {
        var fixture=new Fixture();switch(fault){case "currency":fixture.Native.Rows.RemoveAll(row=>row is R_DEVELOPMENT_PLANNING_REFERENCE_CODE code&&code.REFERENCE_SET=="COST_CURRENCY");break;
            case "status":fixture.Native.Rows.RemoveAll(row=>row is R_DEVELOPMENT_PLANNING_REFERENCE_CODE code&&code.REFERENCE_SET=="FDP_STATUS");break;
            case "duplicate-reference":fixture.Native.Rows.Add(fixture.Native.Rows[0]);break;case "null":fixture.NullRows=true;break;
            case "nullable-owner":fixture.ChangeSchema=s=>{if(s.EntityName=="FIELD_DEVELOPMENT_PLAN")s.Fields.Single(f=>f.FieldName=="TENANT_ID").AllowDBNull=true;};break;
            case "width":fixture.ChangeSchema=s=>{if(s.EntityName=="FIELD_DEVELOPMENT_PLAN"){var f=s.Fields.Single(f=>f.FieldName=="DRAFT_REQUEST_REVISION");f.Size=1;f.Size1=1;}};break;
            case "key":fixture.ChangeSchema=s=>s.Fields.ForEach(f=>f.IsKey=false);break;
            case "read-committed":fixture.Native.Isolation=IsolationLevel.ReadCommitted;break;case "borrowed":fixture.Native.Borrowed=true;break;}
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Save());Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Rows.OfType<FIELD_DEVELOPMENT_PLAN>());
    }
    [Theory][InlineData("failed")][InlineData("corrupt")][InlineData("revoked")][InlineData("binding")]
    public async Task FailedWriteOrClosingAuthorityRollsBack(string fault)
    {
        var fixture=new Fixture();fixture.FailInsert=fault=="failed";fixture.CorruptInsert=fault=="corrupt";
        fixture.OnInsert=()=>{if(fault=="revoked")fixture.Revoke=true;if(fault=="binding")fixture.Native.Connection="other";};
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Save());Assert.Empty(fixture.Native.Rows.OfType<FIELD_DEVELOPMENT_PLAN>());Assert.Contains("rollback",fixture.Native.Transactions);
    }
    [Fact] public async Task UncertainCommitCanOnlyBeRecoveredUsingOriginalIntent()
    {
        var fixture=new Fixture();fixture.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Save());Assert.Single(fixture.Native.Rows.OfType<FIELD_DEVELOPMENT_PLAN>());
        fixture.Native.FailCommit=false;Assert.NotNull((await fixture.Recover()).Plan);Assert.Equal(1,fixture.Native.Writes);
        var changed=fixture.Request.SelectionContext with{Company=new("tenant","other")};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.RecoverDevelopmentPlanAsync(fixture.Request,changed,fixture.Request.PlanningDatabase,fixture.Authority));
    }
    [Fact] public async Task CompanyIdentitySeparatesIdenticalIntentAndFieldIds()
    {
        var fixture=new Fixture();var first=await fixture.Save();fixture.Request=fixture.Request with{SelectionContext=fixture.Request.SelectionContext with{Company=new("tenant","other")}};
        var second=await fixture.Save();Assert.NotEqual(first.FDP_ID,second.FDP_ID);Assert.Equal(2,fixture.Native.Writes);
        Assert.Single((await fixture.Service.GetDevelopmentPlansAsync(fixture.Request.SelectionContext,fixture.Request.PlanningDatabase,fixture.Authority)).Plans);
    }
    [Theory][InlineData("inactive")][InlineData("moved-field")][InlineData("foreign-company")][InlineData("changed-audit")][InlineData("duplicate")]
    public async Task ChangedRecordedOriginalCannotBeReportedAbsentOrSilentlyRewritten(string fault)
    {
        var fixture=new Fixture();await fixture.Save();var row=Assert.Single(fixture.Native.Rows.OfType<FIELD_DEVELOPMENT_PLAN>());
        switch(fault){case "inactive":row.ACTIVE_IND="N";break;case "moved-field":row.FIELD_ID="other";break;
            case "foreign-company":row.BUSINESS_ASSOCIATE_ID="other";break;case "changed-audit":row.ROW_CHANGED_BY="editor";row.ROW_CHANGED_DATE=DateTime.UtcNow;break;
            case "duplicate":fixture.Native.Rows.Add(row);break;}
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Recover());await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Save());Assert.Equal(1,fixture.Native.Writes);
    }
    [Theory][InlineData(false)][InlineData(true)] public async Task UnboundOrChangedCachedTargetCannotBeReadOrWritten(bool changed)
    {
        var fixture=new Fixture();var configured=(ConnectionProperties)fixture.Native.Source.Object.Dataconnection.ConnectionProp;
        var different=new ConnectionProperties{ConnectionName="planning",DatabaseType=configured.DatabaseType,Host=configured.Host,Database="other"};
        Mock.Get(fixture.Native.Source.Object.Dataconnection).SetupGet(value=>value.ConnectionProp).Returns(changed?different:null!);
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Save());Assert.Equal(0,fixture.Native.Writes);Assert.Empty(fixture.Native.Transactions);
    }
    [Fact] public async Task GenericRepositoryCannotBypassTheDevelopmentDraftCommandOwner()
    {
        var fixture=new Fixture();var repository=new Beep.OilandGas.PPDM39.Core.PPDMGenericRepository(fixture.Native.Editor.Object,new CommonColumnHandler(),
            Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),typeof(FIELD_DEVELOPMENT_PLAN),"planning","FIELD_DEVELOPMENT_PLAN");
        await Assert.ThrowsAnyAsync<Exception>(()=>repository.InsertAsync(new FIELD_DEVELOPMENT_PLAN{FDP_ID="raw"},"planner"));Assert.Equal(0,fixture.Native.Writes);
        Assert.DoesNotContain(fixture.Native.Source.Invocations,call=>call.Method.Name=="InsertEntity");
    }
    [Fact] public async Task ExactSourceReviewIsDetachedContentBoundAndSharedAcrossCurrentReaders()
    {
        var fixture=new Fixture();var saved=await fixture.Save();
        var review=await fixture.Service.GetDevelopmentPlanAsync(fixture.Request.SelectionContext,fixture.Request.PlanningDatabase,saved.FDP_ID,fixture.Authority);
        review.Require(fixture.Request.SelectionContext,saved.FDP_ID);Assert.Equal(fixture.Request.Description,review.Plan!.DEVELOPMENT_CONCEPT_DESC);
        var other=fixture.Request.SelectionContext with{ActorUserId="independent-reviewer",Revision="reviewer-preference"};
        var second=await fixture.Service.GetDevelopmentPlanAsync(other,fixture.Request.PlanningDatabase,saved.FDP_ID,fixture.Authority);
        Assert.Equal(review.Revision,second.Revision);Assert.Equal("planner",second.Plan!.ROW_CREATED_BY);Assert.Equal("independent-reviewer",second.Context.ActorUserId);
        review.Plan.DEVELOPMENT_CONCEPT_DESC="tampered";Assert.Throws<InvalidOperationException>(()=>review.Require(fixture.Request.SelectionContext,saved.FDP_ID));
        Assert.Equal(fixture.Request.Description,fixture.Native.Rows.OfType<FIELD_DEVELOPMENT_PLAN>().Single().DEVELOPMENT_CONCEPT_DESC);
        var absent=await fixture.Service.GetDevelopmentPlanAsync(other,fixture.Request.PlanningDatabase,"missing",fixture.Authority);absent.Require(other,"missing");Assert.Null(absent.Plan);
        fixture.NullRows=true;await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Service.GetDevelopmentPlanAsync(other,fixture.Request.PlanningDatabase,saved.FDP_ID,fixture.Authority));
    }
}
#endif
