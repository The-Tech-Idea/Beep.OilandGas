using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;
public sealed class FacilityEditPreflightTests
{
    [Theory][InlineData("facility")][InlineData("edit-intent")][InlineData("create-intent")][InlineData("facility-link")][InlineData("equipment-link")][InlineData("equipment-intent")]
    public async Task FacilityAndPrivateIntentCannotBeWrittenThroughAnUnownedGenericRepository(string kind)
    {
        using var f=await CompanyPPDMRowAccessTests.Fixture.Create();
        object row=kind switch{"facility"=>new FACILITY{FACILITY_ID="original",FACILITY_TYPE="gas"},"facility-link"=>new FACILITY_FIELD{FACILITY_ID="original",FACILITY_TYPE="gas",FIELD_ID="A"},"create-intent"=>new FACILITY_CREATE_INTENT{FACILITY_CREATE_INTENT_ID="original"},"equipment-link"=>new FACILITY_EQUIPMENT{FACILITY_ID="original",FACILITY_TYPE="gas",EQUIPMENT_ID="pump"},"equipment-intent"=>new FACILITY_EQUIPMENT_INSTALL_INTENT{FACILITY_EQUIPMENT_INSTALL_INTENT_ID="original"},_=>new FACILITY_EDIT_INTENT{FACILITY_EDIT_INTENT_ID="original"}};var type=row.GetType();var repository=new PPDMGenericRepository(f.Editor.Object,f.Columns.Object,f.Defaults.Object,f.Metadata.Object,type,"selected-tenant",type.Name);
        await Assert.ThrowsAsync<RefusalException>(()=>repository.InsertAsync(row,"actor"));
        await Assert.ThrowsAsync<RefusalException>(()=>repository.UpdateAsync(row,"actor"));
        await Assert.ThrowsAsync<RefusalException>(()=>repository.DeleteAsync(row));
        await Assert.ThrowsAsync<RefusalException>(()=>repository.SoftDeleteAsync("original","actor"));
        await Assert.ThrowsAsync<RefusalException>(()=>repository.InsertBatchAsync([row],"actor"));
        await Assert.ThrowsAsync<RefusalException>(()=>repository.UpdateBatchAsync([row],"actor"));
        Assert.Throws<RefusalException>(()=>repository.UnitOfWork);
        Assert.Empty(f.Writes);
    }
    [Theory][InlineData("valid")][InlineData("split-binding")][InlineData("missing-schema")][InlineData("wrong-key")][InlineData("wrong-field-link")][InlineData("small-body")][InlineData("small-owner")][InlineData("read-only")][InlineData("changed-source")][InlineData("revoked")][InlineData("mid-read-change")][InlineData("mid-read-revoke")]
    public async Task PreflightRequiresCurrentCompoundSourceDirectWriteAndColocatedPhysicalSchema(string mode)
    {
        using var f=await CompanyMaintenanceQualifiedAssetTests.ReviewFixture.Create();
        var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");if(mode!="read-only")grant.AccessLevel="WRITE";await f.Native.Db.SaveChangesAsync();
        var row=f.Native.Rows.OfType<FACILITY>().Single(value=>value.FACILITY_TYPE=="gas");
        var review=new DevelopmentFacilityEditReview(f.Context,CompanyInventorySourceReview.Copy(row),"Editor",DevelopmentFacilityEditReview.SourceDigest(row),new string('A',64),"",DateTimeOffset.UtcNow);
        review=review with{Revision=review.ContentRevision()};var command=new DevelopmentFacilityEdit(review,Guid.NewGuid(),"Updated facility","Updated description","Correct the recorded description",true);
        f.Native.Metadata.Setup(m=>m.GetTableMetadataAsync("FACILITY_EDIT_INTENT")).ReturnsAsync(new PPDMTableMetadata{TableName="FACILITY_EDIT_INTENT",EntityTypeName=nameof(FACILITY_EDIT_INTENT),PrimaryKeyColumn="FACILITY_EDIT_INTENT_ID"});
        var creator=new ClassCreator(f.Native.Editor.Object);f.Native.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns((string name)=>mode!="missing-schema" || name!="FACILITY_EDIT_INTENT");
        f.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>
        {
            var type=requested.EntityName=="FACILITY_EDIT_INTENT"?typeof(FACILITY_EDIT_INTENT):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;
            var shape=creator.ConvertToEntityStructure(type);var keys=requested.EntityName=="FACILITY_EDIT_INTENT"?new[]{"FACILITY_EDIT_INTENT_ID"}:f.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult().ToArray();
            foreach(var column in shape.Fields){column.IsKey=keys.Contains(column.FieldName,StringComparer.Ordinal) && !(mode=="wrong-key" && column.FieldName=="FACILITY_TYPE");if((mode=="small-body" && column.FieldName=="COMMAND_JSON") || (mode=="small-owner" && column.FieldName=="OWNER_COMPANY_BA_ID")){column.Size=32;column.Size1=32;}}
            return shape;
        });
        if(mode=="changed-source")row.REMARK="changed after review";if(mode=="revoked")f.Revoked=true;
        if(mode=="wrong-field-link")(await f.Native.Metadata.Object.GetTableMetadataAsync("FACILITY")).ForeignKeys.Single(key=>key.ForeignKeyColumn=="PRIMARY_FIELD_ID").ReferencedTable="OTHER";
        if(mode is "mid-read-change" or "mid-read-revoke")f.Native.AfterRead=()=>
        {
            if(f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync").Arguments[0] as string=="FACILITY")
            {if(mode=="mid-read-change")row.REMARK="changed during read";else f.Revoked=true;}
            return Task.CompletedTask;
        };
        var owner=new FacilityManagementService(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),f.Context.Database.ConnectionName,
            resolveModuleConnection:module=>Task.FromResult(mode=="split-binding" && module=="FACILITY"?"other":f.Context.Database.ConnectionName),maintenanceAssetAccess:f.Native.Policy,maintenanceEvidenceZone:"Asia/Kuwait");
        if(mode=="valid"){var source=await owner.ReadFacilityEditPreflightAsync(command,f.Guard);Assert.Equal("plant",source.FACILITY_ID);Assert.Equal("gas",source.FACILITY_TYPE);}
        else await Assert.ThrowsAnyAsync<Exception>(()=>owner.ReadFacilityEditPreflightAsync(command,f.Guard));
        Assert.Empty(f.Native.Writes);
    }
}
