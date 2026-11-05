using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.DevelopmentPlanning.Modules;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentPlanDraftContractTests
{
    private static CreateDevelopmentPlan Input() => new() { PlanName="Field development / تطوير",FieldId="field/one",IntentId=Guid.Parse("ed83fae8-0b38-49bb-bc07-a0a7bd976ed8"),PlanningDatabase=new("planning","planning-binding"),
        SelectionContext=new(new("tenant","company"),new("core","binding"),"engineer","revision","field/one"),
        Description="Recorded development concept",PlannedFirstProductionDate=new(2030,1,2),PlannedFieldLifeEndDate=new(2031,1,2),EstimatedCapexMillions=12.125m,CapexCurrency="KWD" };
    private static FIELD_DEVELOPMENT_PLAN Recorded(CreateDevelopmentPlan input) => new() {
        FDP_ID=DevelopmentPlanDraftReview.PlanIdFor(input),TENANT_ID=input.SelectionContext!.Company.TenantId,BUSINESS_ASSOCIATE_ID=input.SelectionContext.Company.BusinessAssociateId,
        FIELD_ID=input.FieldId,DRAFT_INTENT_ID=input.IntentId.ToString("D"),DRAFT_REQUEST_REVISION=DevelopmentPlanDraftReview.RequestRevision(input),
        ROW_CREATED_BY=input.SelectionContext.ActorUserId,ROW_CREATED_DATE=new(2026,10,25,0,0,0,DateTimeKind.Utc),ACTIVE_IND="Y",FDP_STATUS="DRAFT",FDP_VERSION=1,
        FDP_NAME=input.PlanName,DEVELOPMENT_CONCEPT_DESC=input.Description ?? "",FIRST_PRODUCTION_DATE=input.PlannedFirstProductionDate?.ToDateTime(TimeOnly.MinValue),FIELD_LIFE_END=input.PlannedFieldLifeEndDate?.ToDateTime(TimeOnly.MinValue),
        TOTAL_CAPEX_MM=input.EstimatedCapexMillions,CAPEX_CURRENCY=input.CapexCurrency ?? "" };
    [Fact]
    public void ExistingRequestAndNativeHeaderHaveOneDataOwnerAndFrozenOriginalIntent()
    {
        Assert.Equal(typeof(CompanyScope).Assembly,typeof(CreateDevelopmentPlan).Assembly);
        Assert.Equal(typeof(CompanyScope).Assembly,typeof(FIELD_DEVELOPMENT_PLAN).Assembly);
        var input=Input();var frozen=DevelopmentPlanDraftReview.Capture(input,input.SelectionContext!);
        var revision=DevelopmentPlanDraftReview.RequestRevision(frozen);var plan=DevelopmentPlanDraftReview.PlanIdFor(frozen);
        Assert.Equal(plan,DevelopmentPlanDraftReview.PlanIdFor(JsonSerializer.Deserialize<CreateDevelopmentPlan>(JsonSerializer.Serialize(frozen))!));
        input=input with{PlanName="Changed input"};Assert.NotEqual(input.PlanName,frozen.PlanName);Assert.NotEqual(revision,DevelopmentPlanDraftReview.RequestRevision(input));
        Assert.Equal(plan,DevelopmentPlanDraftReview.PlanIdFor(input));
        input=input with{SelectionContext=input.SelectionContext with { Company=new("tenant","second") }};
        Assert.NotEqual(plan,DevelopmentPlanDraftReview.PlanIdFor(input));
        DevelopmentPlanDraftReview.RequireRecorded(frozen,Recorded(frozen));
    }
    [Theory][InlineData("intent")][InlineData("field")][InlineData("context")][InlineData("name")][InlineData("control")]
    [InlineData("amount")][InlineData("currency")][InlineData("currency-only")][InlineData("early-date")][InlineData("dates")]
    public void InvalidDraftInputsCannotBecomeAReviewedSave(string fault)
    {
        var input=Input();var current=input.SelectionContext!;
        switch(fault)
        {
            case "intent": input=input with{IntentId=Guid.Empty};break;
            case "field": input=input with{FieldId="other"};break;
            case "context": input=input with{SelectionContext=current with { Revision="other" }};break;
            case "name":input=input with{PlanName=" "};break;
            case "control":input=input with{PlanName="Plan\0"};break;
            case "amount":input=input with{EstimatedCapexMillions=-1};break;
            case "currency":input=input with{CapexCurrency="kwd"};break;
            case "currency-only":input=input with{EstimatedCapexMillions=null};break;
            case "early-date":input=input with{PlannedFirstProductionDate=new(1899,1,1)};break;
            default:input=input with{PlannedFieldLifeEndDate=new(2029,1,1)};break;
        }
        Assert.Throws<InvalidOperationException>(()=>DevelopmentPlanDraftReview.Capture(input,current));
    }
    [Theory][InlineData("company")][InlineData("tenant")][InlineData("actor")][InlineData("intent")][InlineData("hash")]
    [InlineData("name")][InlineData("amount")][InlineData("currency")][InlineData("approved")][InlineData("submitted")][InlineData("missing-ownership")]
    public void DraftReadbackRequiresExactOwnedOriginalInputsAndNoApproval(string fault)
    {
        var input=Input();var row=Recorded(input);
        switch(fault)
        {
            case "company":row.BUSINESS_ASSOCIATE_ID="other";break;
            case "tenant":row.TENANT_ID="other";break;
            case "actor":row.ROW_CREATED_BY="other";break;
            case "intent":row.DRAFT_INTENT_ID=Guid.NewGuid().ToString("D");break;
            case "hash":row.DRAFT_REQUEST_REVISION=new('0',64);break;
            case "name":row.FDP_NAME="other";break;
            case "amount":row.TOTAL_CAPEX_MM=12125;break;
            case "currency":row.CAPEX_CURRENCY="USD";break;
            case "approved":row.APPROVAL_DATE=DateTime.UtcNow;break;
            case "submitted":row.SUBMISSION_DATE=DateTime.UtcNow;break;
            default:row.TENANT_ID="";row.BUSINESS_ASSOCIATE_ID="";row.DRAFT_INTENT_ID="";row.DRAFT_REQUEST_REVISION="";break;
        }
        Assert.Throws<InvalidOperationException>(()=>DevelopmentPlanDraftReview.RequireRecorded(input,row));
    }
    [Fact]
    public void MissingEstimateAndDatesStayUnrecordedWithoutCurrencyOrSubmissionDefaults()
    {
        var input=Input() with{EstimatedCapexMillions=null,CapexCurrency=null,Description=null,PlannedFirstProductionDate=null,PlannedFieldLifeEndDate=null};
        var frozen=DevelopmentPlanDraftReview.Capture(input,input.SelectionContext!);var row=Recorded(frozen);
        DevelopmentPlanDraftReview.RequireRecorded(frozen,row);Assert.Null(row.TOTAL_CAPEX_MM);Assert.Equal("",row.CAPEX_CURRENCY);Assert.Null(row.SUBMISSION_DATE);
    }
    [Theory][InlineData("SelectionContext")][InlineData("PlanningDatabase")][InlineData("IntentId")][InlineData("PlanName")][InlineData("FieldId")]
    public void MissingRequiredWireFieldsRefuseInsteadOfUsingOldRequestDefaults(string field)
    {
        var payload=JsonSerializer.SerializeToNode(Input())!.AsObject();payload.Remove(field);
        Assert.Throws<JsonException>(()=>payload.Deserialize<CreateDevelopmentPlan>());
    }
    [Theory][InlineData("2030-01-02T00:00:00Z")][InlineData("2030-01-02T10:00:00+03:00")]
    public void CalendarDatesDoNotAcceptTimestampOrTimeZoneFallbacks(string value)
    {
        var payload=JsonSerializer.SerializeToNode(Input())!.AsObject();payload["PlannedFirstProductionDate"]=value;
        Assert.Throws<JsonException>(()=>payload.Deserialize<CreateDevelopmentPlan>());
    }
    [Theory][InlineData("complete")][InlineData("TENANT_ID")][InlineData("BUSINESS_ASSOCIATE_ID")][InlineData("DRAFT_INTENT_ID")][InlineData("DRAFT_REQUEST_REVISION")]
    [InlineData("nullable-TENANT_ID")][InlineData("nullable-BUSINESS_ASSOCIATE_ID")][InlineData("nullable-DRAFT_INTENT_ID")][InlineData("nullable-DRAFT_REQUEST_REVISION")]
    public void ActualBeepManifestAndReflectionRequireOwnedDraftSchema(string fault)
    {
        var module=new DevelopmentPlanningModule(new ModuleSetupContext());
        Assert.Equal("DEVELOPMENT_PLANNING",module.ModuleId);Assert.Equal(7,module.EntityTypes.Count);
        Assert.Single(module.EntityTypes,type=>type==typeof(FIELD_DEVELOPMENT_PLAN));
        var editor=new Mock<IDMEEditor>();var creator=new ClassCreator(editor.Object);editor.SetupGet(value=>value.classCreator).Returns(creator);
        var expected=creator.ConvertToEntityStructure(typeof(FIELD_DEVELOPMENT_PLAN));
        var key=Assert.Single(expected.Fields,field=>field.IsKey);Assert.Equal("FDP_ID",key.FieldName);Assert.False(key.AllowDBNull);
        var description=Assert.Single(expected.Fields,field=>field.FieldName=="DEVELOPMENT_CONCEPT_DESC");Assert.True(Math.Max(description.Size,description.Size1)>=4000);
        foreach(var name in new[]{"TENANT_ID","BUSINESS_ASSOCIATE_ID","DRAFT_INTENT_ID","DRAFT_REQUEST_REVISION"})
        {
            var ownership=Assert.Single(expected.Fields,field=>field.FieldName==name);
            Assert.False(ownership.IsKey);Assert.False(ownership.AllowDBNull);
        }
        var source=new Mock<IDataSource>();
        source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>
        {
            var actual=creator.ConvertToEntityStructure(typeof(FIELD_DEVELOPMENT_PLAN));
            if(fault.StartsWith("nullable-",StringComparison.Ordinal))actual.Fields.Single(field=>field.FieldName==fault["nullable-".Length..]).AllowDBNull=true;
            else if(fault!="complete")actual.Fields.Remove(actual.Fields.Single(field=>field.FieldName==fault));
            return actual;
        });
        if(fault=="complete")ModuleSchemaVerification.Verify(editor.Object,source.Object,[typeof(FIELD_DEVELOPMENT_PLAN)]);
        else Assert.ThrowsAny<InvalidOperationException>(()=>ModuleSchemaVerification.Verify(editor.Object,source.Object,[typeof(FIELD_DEVELOPMENT_PLAN)]));
    }
}
