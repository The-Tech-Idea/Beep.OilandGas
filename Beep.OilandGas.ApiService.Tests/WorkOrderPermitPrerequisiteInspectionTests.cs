#if WORKFLOW_INBOX_FOCUSED
extern alias actualData;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
using Row=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
using Rules=actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules;
using Request=actualData::Beep.OilandGas.Models.Data.WorkOrder.AddWorkOrderInspectionRequirementRequest;
using Result=actualData::Beep.OilandGas.Models.Data.WorkOrder.RecordInspectionResultRequest;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderPermitPrerequisiteInspectionTests
{
    [Theory][InlineData("current")][InlineData("before")][InlineData("after")][InlineData("borrowed")]
    public async Task OwnedRequirementObservationsKeepBothParentBoundariesAndRefuseBeforePrivateReads(string mode)
    {
        var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();var company=new CompanyScope("tenant","company");var database=new AssetDatabaseScope("lifecycle","fingerprint");var context=new FieldSelectionReview(company,database,"reviewer","current-authority","field");
        f.Rows.Add(Rules.PrepareRequirement(new(){FieldId="field",CondSeq=1,CondType="PRE_START",RequiredText="Explicit native requirement",Jurisdiction="Reported authority",ApplicableStage="PRE_START"},"order","requirement-author","requirement",DateTime.UtcNow));
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_INSPECTION",PrimaryKeyColumn="INSPECTION_ID",EntityTypeName=typeof(Row).FullName!});metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(["INSPECTION_ID"]);
        var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_INSPECTION")).Returns(true);f.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(Row)));
        var parents=0;
        Task Parent(string actor,string field,string order,string level){Assert.Equal("reviewer",actor);Assert.Equal("field",field);Assert.Equal("order",order);Assert.Equal("WRITE",level);if(++parents==(mode=="before"?1:mode=="after"?2:0))throw new UnauthorizedAccessException("Native parent changed at observation boundary");return Task.CompletedTask;}
        var owner=new InspectionService(f.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((company,"reviewer",database)),NullLogger<InspectionService>.Instance,Parent);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.ReadRequirementProofCoreAsync(context,"order",()=>Task.CompletedTask));Assert.Equal(0,parents);
        if(mode=="borrowed")f.Borrowed=true;
        var error=await Record.ExceptionAsync(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Source.Object,async()=>{var rows=await owner.ReadRequirementProofCoreAsync(context,"order",()=>Task.CompletedTask);Assert.Single(rows);}));
        if(mode=="current"){Assert.Null(error);Assert.Equal(4,parents);}else Assert.True(error is UnauthorizedAccessException or InvalidOperationException);
        if(mode=="before")f.Source.Verify(value=>value.GetEntityAsync("WORK_ORDER_INSPECTION",It.IsAny<List<AppFilter>>()),Times.Never);
        Assert.Equal(0,f.Writes);if(mode=="after")Assert.Equal(2,parents);
    }
    [Theory][InlineData("allowed")][InlineData("missing-authority")][InlineData("revoked")][InlineData("column")][InlineData("result-revoked")]
    public async Task NativePrerequisiteRequirementAndResultUseExplicitKindCurrentAuthorityAndOwnedRollback(string fault)
    {
        var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_INSPECTION",PrimaryKeyColumn="INSPECTION_ID",EntityTypeName=typeof(Row).FullName!});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(["INSPECTION_ID"]);var creator=new ClassCreator(f.Editor.Object);
        f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_INSPECTION")).Returns(true);f.Source.Setup(s=>s.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure _,bool __)=>{var shape=creator.ConvertToEntityStructure(typeof(Row));if(fault=="column")shape.Fields.RemoveAll(field=>field.FieldName=="PERMIT_PREREQUISITE_KIND");return shape;});
        f.Source.Setup(s=>s.InsertEntity("WORK_ORDER_INSPECTION",It.IsAny<object>())).Returns((string _,object row)=>{f.Writes++;f.Rows.Add(JsonSerializer.Deserialize<Row>(JsonSerializer.Serialize(row))!);return new ErrorsInfo{Flag=Errors.Ok};});
        f.Source.Setup(s=>s.UpdateEntity("WORK_ORDER_INSPECTION",It.IsAny<object>())).Returns((string _,object row)=>{f.Writes++;var value=JsonSerializer.Deserialize<Row>(JsonSerializer.Serialize(row))!;f.Rows[f.Rows.FindIndex(old=>old is Row r && r.INSPECTION_ID==value.INSPECTION_ID)]=value;return new ErrorsInfo{Flag=Errors.Ok};});
        var guards=0;var revokeAt=fault=="revoked"?5:0;Task Guard(string actor){Assert.Equal("reviewer",actor);if(++guards==revokeAt)throw new UnauthorizedAccessException("Current HSE authority changed");return Task.CompletedTask;}
        var owner=new InspectionService(f.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((new CompanyScope("tenant","company"),"reviewer",new AssetDatabaseScope("lifecycle","fingerprint"))),NullLogger<InspectionService>.Instance,(_,_,_,_)=>Task.CompletedTask,fault=="missing-authority"?null:Guard);
        var request=new Request{FieldId="field",CondSeq=1,CondType="PRE_START",RequiredText="Explicit isolation prerequisite",Jurisdiction="Reported authority",RegulatoryRef="isolation-document",PermitPrerequisiteKind="ISOLATION"};
        if(fault is "missing-authority" or "revoked" or "column")
        {
            var error=await Record.ExceptionAsync(()=>owner.AddRequirementAsync("order",request,"reviewer"));Assert.True(error is RefusalException or UnauthorizedAccessException,error?.ToString()??"Unexpected prerequisite write");Assert.Empty(f.Rows);return;
        }
        var added=await owner.AddRequirementAsync("order",request,"reviewer");Assert.Equal("ISOLATION",added.PermitPrerequisiteKind);Assert.Equal("UNRECORDED",added.Status);if(fault=="result-revoked")revokeAt=guards+3;
        var result=new Result{FieldId="field",InspectionId=added.InspectionId,ExpectedSourceRevision=added.SourceRevision,Result="PASS",Notes="Authenticated independent inspection evidence"};
        if(fault=="result-revoked"){await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.RecordResultAsync("order",1,result,"reviewer"));Assert.Equal("",Assert.Single(f.Rows.OfType<Row>()).RESULT_STATUS);return;}
        var saved=await owner.RecordResultAsync("order",1,result,"reviewer");Assert.Equal("PASS",saved.Status);Assert.Equal("reviewer",saved.Inspector);Assert.Equal("ISOLATION",saved.PermitPrerequisiteKind);Assert.NotEqual(added.SourceRevision,saved.SourceRevision);Assert.Equal(2,f.Writes);Rules.Require("field","order",f.Rows.OfType<Row>().ToArray());
    }
}
#endif
