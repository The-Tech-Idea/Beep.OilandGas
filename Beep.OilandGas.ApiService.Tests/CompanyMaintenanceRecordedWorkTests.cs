using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
#if WORKFLOW_INBOX_FOCUSED
using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMaintenanceRecordedWorkTests
{
    private sealed class Fixture
    {
        internal readonly PersonaTaskRoutingTests.Fixture Native=new();
        internal readonly Mock<IPPDMMetadataRepository> Metadata=new();
        internal readonly FieldSelectionReview Context=new(new("tenant","company"),new("core","core-binding"),"reader","read-authority","field");
        internal readonly WorkOrderExecutionPermitService Owner;
        internal readonly CompanyMaintenanceRecordedWorkWindow Window;
        internal bool SchemaMissing,IgnorePredicates,Revoked,ChangeBinding,ChangeRead;
        internal int Reads;
        internal Fixture(int count=1)
        {
            Native.Rows.Clear();Window=new("field",DateTime.UtcNow.AddMinutes(-1),DateTime.UtcNow.AddMinutes(5));
            for(var index=0;index<count;index++){var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(Context.Company,Guid.NewGuid()),"field","order");var original=WorkOrderExecutionStartFixture.Submission(Context.Database,taskOverride:task);Native.Rows.Add(original.Command.Prepare(DateTime.UtcNow));}
            var creator=new ClassCreator(Native.Editor.Object);Native.Editor.SetupGet(editor=>editor.classCreator).Returns(creator);
            Native.Source.Setup(source=>source.CheckEntityExist("WORK_ORDER_EXECUTION_SUBMISSION")).Returns(()=>!SchemaMissing);
            Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_SUBMISSION));foreach(var field in shape.Fields)field.IsKey=field.FieldName=="SUBMISSION_ID";return shape;});
            Metadata.Setup(metadata=>metadata.GetTableMetadataAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_SUBMISSION",EntityTypeName=typeof(WORK_ORDER_EXECUTION_SUBMISSION).FullName!,PrimaryKeyColumn="SUBMISSION_ID"});Metadata.Setup(metadata=>metadata.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(["SUBMISSION_ID"]);
            Native.Source.Setup(source=>source.GetEntityAsync("WORK_ORDER_EXECUTION_SUBMISSION",It.IsAny<List<AppFilter>>())).ReturnsAsync((string _,List<AppFilter> filters)=>
            {
                Reads++;if(ChangeRead && Reads==2)Native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single().RECORDED_REVISION=new string('F',64);
                return Native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Where(row=>IgnorePredicates || filters.All(filter=>
                {
                    var value=typeof(WORK_ORDER_EXECUTION_SUBMISSION).GetProperty(filter.FieldName)!.GetValue(row);
                    if(filter.FieldName=="ROW_CREATED_DATE"){Assert.Equal(typeof(DateTime),filter.FieldType);var utc=DateTime.Parse(filter.FilterValue,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind);return filter.Operator==">="?(DateTime)value!>=utc:(DateTime)value!<utc;}return value?.ToString()==filter.FilterValue;
                })).Select(CompanyInventorySourceReview.Copy).ToArray();
            });
            Owner=new(Native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Metadata.Object);
        }
        internal Task Guard(){if(Revoked)throw new UnauthorizedAccessException("Read authority revoked");if(ChangeBinding)Native.Editor.Setup(editor=>editor.GetDataSource("core")).Returns(Mock.Of<TheTechIdea.Beep.IDataSource>());return Task.CompletedTask;}
        internal Task<IReadOnlyList<CompanyWorkOrderSubmissionFacts>> Read(CompanyMaintenanceRecordedWorkWindow? window=null)=>Owner.ReadRecordedWorkAsync(Context,window??Window,["order"],Guard);
    }
    [Fact]
    public async Task ActualNativeDiscoveryReturnsCompleteOriginalReportsAndExactPagedLinksWithoutMutating()
    {
        var f=new Fixture(3);var reports=await f.Read();Assert.Equal(3,reports.Count);var page=CompanyMaintenanceRecordedWorkPage.Create(f.Context,f.Window,2,1,reports,DateTime.UtcNow);page.Require(f.Context.Company,f.Context.ActorUserId,f.Window,2,1);Assert.Equal(3,page.TotalRecords);Assert.True(page.HasPrevious);Assert.True(page.HasNext);var entry=Assert.Single(page.Rows);Assert.Equal(reports[1].Query,entry.Query);Assert.Contains(Uri.EscapeDataString(entry.Query.SubmissionId),entry.DetailRoute(f.Context.Company));Assert.Contains(Uri.EscapeDataString(entry.Query.Task.InstanceId),entry.DetailRoute(f.Context.Company));var body=JsonSerializer.Serialize(page);Assert.DoesNotContain("COMMAND_JSON",body);Assert.DoesNotContain("IntentId",body);Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
        var empty=await f.Read(f.Window with{FromUtc=DateTime.UtcNow.AddDays(1),ToUtc=DateTime.UtcNow.AddDays(2)});Assert.Empty(empty);Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("schema")][InlineData("revoked")][InlineData("binding")][InlineData("changed")][InlineData("foreign-company")][InlineData("foreign-field")][InlineData("foreign-order")][InlineData("outside-period")][InlineData("duplicate")][InlineData("corrupt")]
    public async Task NativeSchemaAuthorityPredicatesPopulationAndOriginalBodyMustAgree(string fault)
    {
        var f=new Fixture();var row=f.Native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single();f.SchemaMissing=fault=="schema";f.Revoked=fault=="revoked";f.ChangeBinding=fault=="binding";f.ChangeRead=fault=="changed";if(fault is "foreign-company" or "foreign-field" or "foreign-order" or "outside-period")f.IgnorePredicates=true;
        if(fault=="foreign-company")row.OWNER_COMPANY_BA_ID="other";if(fault=="foreign-field")row.FIELD_ID="other";if(fault=="foreign-order")row.WORK_ORDER_ID="other";if(fault=="outside-period")row.ROW_CREATED_DATE=f.Window.ToUtc.AddMinutes(1);if(fault=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(row));if(fault=="corrupt")row.COMMAND_JSON="{}";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());Assert.Equal(0,f.Native.Writes);if(fault is "schema" or "revoked" or "binding")Assert.Equal(0,f.Reads);
    }
    [Theory][InlineData("{")][InlineData("{}")]
    public async Task InvalidRetainedProofProducesAControlledSourceConflict(string body)
    {
        var f=new Fixture();f.Native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single().COMMAND_JSON=body;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Read());Assert.Equal(0,f.Native.Writes);
    }
}
#endif
