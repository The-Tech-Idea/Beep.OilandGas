#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderPersonnelRosterTests
{
    [Theory][InlineData("current")][InlineData("empty")][InlineData("foreign")][InlineData("duplicate")][InlineData("drift")][InlineData("damaged")][InlineData("authority")][InlineData("schema")][InlineData("borrowed")]
    public async Task NativePublicRosterClosesAllHeadersBeforePrivateProofAndNeverWritesOrBorrows(string mode)
    {
        var command=WorkOrderPersonnelReviewFixture.Original();var stored=command.Prepare(DateTime.UtcNow);var context=command.Review.Personnel.Context with{ActorUserId="another-reviewer",Revision="current-roster-authority"};var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();if(mode!="empty")f.Rows.Add(stored);if(mode=="duplicate")f.Rows.Add(CompanyInventorySourceReview.Copy(stored));if(mode=="damaged")stored.COMMAND_JSON="malformed proof";if(mode=="foreign"){stored.OWNER_COMPANY_BA_ID="other";stored.COMMAND_JSON="malformed foreign private proof";}
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_PERSONNEL_ASSIGNMENT")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_PERSONNEL_ASSIGNMENT",EntityTypeName=typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT).FullName!,PrimaryKeyColumn="ASSIGNMENT_ID"});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_PERSONNEL_ASSIGNMENT")).ReturnsAsync(["ASSIGNMENT_ID"]);
        var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_PERSONNEL_ASSIGNMENT")).Returns(true);f.Source.Setup(s=>s.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT));if(mode=="schema")shape.Fields.RemoveAll(field=>field.FieldName=="COMMAND_JSON");return shape;});
        f.Source.Setup(source=>source.CheckEntityExist("WORK_ORDER_PERSONNEL_RELEASE")).Returns(true);
        f.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName=="WORK_ORDER_PERSONNEL_RELEASE"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_PERSONNEL_RELEASE)));
        metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_PERSONNEL_RELEASE")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_PERSONNEL_RELEASE",EntityTypeName=typeof(WORK_ORDER_PERSONNEL_RELEASE).FullName!,PrimaryKeyColumn="RELEASE_ID"});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_PERSONNEL_RELEASE")).ReturnsAsync(["RELEASE_ID"]);
        var reads=0;f.Source.Setup(s=>s.GetEntityAsync("WORK_ORDER_PERSONNEL_ASSIGNMENT",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>
        {
            reads++;if(mode=="drift" && reads==2)stored.DUTY_CODE="changed";return f.Rows.OfType<WORK_ORDER_PERSONNEL_ASSIGNMENT>().Select(CompanyInventorySourceReview.Copy).ToArray();
        });
        if(mode=="borrowed")f.Borrowed=true;var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((context.Company,context.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);
        var error=await Record.ExceptionAsync(async()=>
        {
            var result=await owner.ReadPersonnelAssignmentsAsync(context,command.Review.Personnel.Query.Permit,()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask);
            result.Require(context.Company,context.ActorUserId,command.Review.Personnel.Query.Permit);
            if(mode=="empty")Assert.Empty(result.Assignments);else{var row=Assert.Single(result.Assignments);Assert.Equal("independent",row.AssignedBy);Assert.Equal(stored.RECORDED_REVISION,row.RecordedRevision);}
            var body=JsonSerializer.Serialize(result);Assert.DoesNotContain(command.Reason,body);Assert.DoesNotContain(command.IntentId.ToString("D"),body);Assert.DoesNotContain("COMMAND_JSON",body);
        });
        if(mode is "current" or "empty")Assert.Null(error);else Assert.NotNull(error);if(mode=="foreign")Assert.IsType<UnauthorizedAccessException>(error);
        Assert.Equal(0,f.Writes);Assert.Empty(f.Transactions);
    }
}
#endif
