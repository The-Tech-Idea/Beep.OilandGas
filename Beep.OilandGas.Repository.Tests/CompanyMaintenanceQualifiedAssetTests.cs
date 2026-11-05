using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
using Fixture=Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;
namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyMaintenanceQualifiedAssetTests
{
    [Theory][InlineData("W")][InlineData("E")][InlineData("F")][InlineData("WE")][InlineData("WF")][InlineData("EF")][InlineData("WEF")]
    public void OriginalAssetReturnCapabilitiesUseExistingRegistryAndOnlyExplicitTargets(string targets)
    {
        var order=new WorkOrderDetailModel{InstanceId="order",FieldId="field",Source="WORK_ORDER",Components=[new("order","component","field",targets.Contains('W')?"well":"",targets.Contains('E')?"equipment":""){FacilityId=targets.Contains('F')?"facility":null,FacilityType=targets.Contains('F')?"gas":null}]};
        var required=CompanyMaintenanceAssetReturnCapabilities.ForOrder(order);var expected=new List<string>{Beep.OilandGas.Models.Data.Security.PermissionConstants.Maintenance.ReturnToService,Beep.OilandGas.Models.Data.Security.PermissionConstants.Workflow.Execute};
        if(targets.Contains('W'))expected.Add(Beep.OilandGas.Models.Data.Security.PermissionConstants.WellManagement.UpdateWellStatus);
        if(targets.Contains('F'))expected.Add(Beep.OilandGas.Models.Data.Security.PermissionConstants.Facilities.Edit);
        if(targets.Contains('E'))expected.Add(Beep.OilandGas.Models.Data.Security.PermissionConstants.Facilities.ManageEquipment);
        Assert.Equal(expected,required);
        order.Components.Add(order.Components[0] with{ComponentId="second-component"});Assert.Equal(required,CompanyMaintenanceAssetReturnCapabilities.ForOrder(order));
        order.Components[1]=order.Components[1] with{FieldId="foreign"};Assert.Throws<InvalidOperationException>(()=>CompanyMaintenanceAssetReturnCapabilities.ForOrder(order));
    }
    internal sealed class ReviewFixture:IDisposable
    {
        internal readonly Fixture Native;internal readonly FieldSelectionReview Context;internal readonly WorkOrderDetailModel Order;internal readonly FacilityManagementService Owner;
        internal bool Revoked,WrongPhysicalKey,SchemaMissing;internal int Guards;
        private ReviewFixture(Fixture native,FieldSelectionReview context)
        {
            Native=native;Context=context;var utc=DateTime.UtcNow;Order=new(){InstanceId="order",FieldId="A",Source="WORK_ORDER",Components=[new("order","component","A","","pump"){FacilityId="plant",FacilityType="gas",NativeRevision=new string('A',64)}]};
            native.Rows.Add(new EQUIPMENT{EQUIPMENT_ID="pump",CURRENT_OWNER_BA_ID=context.Company.BusinessAssociateId,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1),COMMISSION_DATE=utc.AddDays(-10)});
            native.Rows.Add(new FACILITY{FACILITY_ID="plant",FACILITY_TYPE="gas",PRIMARY_FIELD_ID="A",CURRENT_OPERATOR=context.Company.BusinessAssociateId,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
            native.Rows.Add(new FACILITY{FACILITY_ID="plant",FACILITY_TYPE="oil",PRIMARY_FIELD_ID="other-field",CURRENT_OPERATOR="other-company",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
            native.Rows.Add(new FACILITY_EQUIPMENT{FACILITY_ID="plant",FACILITY_TYPE="gas",EQUIPMENT_ID="pump",INSTALL_OBS_NO=7,ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-5),INSTALL_DATE=utc.AddDays(-5),SOURCE="recorded-install"});
            var creator=new ClassCreator(native.Editor.Object);native.Editor.SetupGet(editor=>editor.classCreator).Returns(creator);native.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns(()=>!SchemaMissing);
            native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>
            {
                var type=typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;var shape=creator.ConvertToEntityStructure(type);var keys=native.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult().ToArray();foreach(var field in shape.Fields)field.IsKey=keys.Contains(field.FieldName,StringComparer.Ordinal) && !(WrongPhysicalKey && field.FieldName=="FACILITY_TYPE");return shape;
            });
            Owner=new(native.Editor.Object,native.Columns.Object,native.Defaults.Object,native.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),context.Database.ConnectionName,maintenanceAssetAccess:native.Policy,maintenanceEvidenceZone:"Asia/Kuwait");
        }
        internal static async Task<ReviewFixture> Create()
        {
            var native=await Fixture.Create();
            _=CompanyAssetFacadeTests.Service(native);await native.Grant("FIELD","A");await native.Grant("EQUIPMENT","pump");await native.Grant("FACILITY","plant",facilityType:"gas");
            return new(native,new(native.Gate.Company.Scope,await native.Gate.ResolveAsync(),"actor","current-qualified-review","A"));
        }
        internal Task Guard(){Guards++;if(Revoked)throw new UnauthorizedAccessException("Original return-read authority revoked.");return Task.CompletedTask;}
        internal Task<CompanyMaintenanceQualifiedAssets> Read()=>Owner.ReadMaintenanceQualifiedAssetsAsync(Context,Order,Guard);
        public void Dispose()=>Native.Dispose();
    }
    [Theory][InlineData("allowed")][InlineData("partner")][InlineData("equipment-read")][InlineData("facility-read")][InlineData("partner-no-inheritance")][InlineData("foreign-field")][InlineData("foreign-owner")][InlineData("duplicate")][InlineData("changed")][InlineData("revoked")]
    public async Task InstallationReviewRequiresWriteAuthorityAndStableRecordedPopulation(string mode)
    {
        using var f=await ReviewFixture.Create();var grants=await f.Native.Db.Set<AppUserAssetAccess>().ToListAsync();foreach(var grant in grants)grant.AccessLevel="WRITE";
        if(mode.StartsWith("partner",StringComparison.Ordinal)){f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_TYPE=="gas").CURRENT_OPERATOR="partner";grants.Single(row=>row.AssetType=="FIELD").Inherit=mode!="partner-no-inheritance";}
        if(mode=="equipment-read")grants.Single(row=>row.AssetType=="EQUIPMENT").AccessLevel="READ";if(mode=="facility-read")grants.Single(row=>row.AssetType=="FACILITY").AccessLevel="READ";await f.Native.Db.SaveChangesAsync();
        if(mode=="foreign-field")f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_TYPE=="gas").PRIMARY_FIELD_ID="B";if(mode=="foreign-owner")f.Native.Rows.OfType<EQUIPMENT>().Single().CURRENT_OWNER_BA_ID="other-company";
        if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single()));if(mode=="revoked")f.Revoked=true;
        if(mode=="changed"){var reads=0;f.Native.AfterRead=()=>{var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");if(call.Arguments[0] as string=="FACILITY_EQUIPMENT" && ++reads==2)f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=8;return Task.CompletedTask;};}
        var proposal=new DevelopmentEquipmentInstallationProposal(f.Context,"plant","gas","pump","Engineer","Recorded maintenance requirement");
        Task<DevelopmentEquipmentInstallationSource> Read()=>f.Owner.ReviewDevelopmentEquipmentInstallationAsync(proposal,f.Guard);
        if(mode is "allowed" or "partner"){var source=await Read();Assert.Equal("company",source.EquipmentOwner);Assert.Equal(1,source.RecordedRows);Assert.Equal(1,source.ActiveRows);Assert.True(ProcessDefinitionRevision.IsValid(source.Revision));}
        else await Assert.ThrowsAnyAsync<Exception>(Read);Assert.Empty(f.Native.Writes);
    }

    [Theory][InlineData("allowed")][InlineData("partner")][InlineData("partner-no-direct")][InlineData("partner-no-inheritance")][InlineData("foreign-field")][InlineData("foreign-owner")][InlineData("facility-grant")][InlineData("equipment-grant")][InlineData("revoked")][InlineData("duplicate")][InlineData("changed")]
    public async Task FacilityEquipmentReadRequiresCurrentParentsAndStableMembership(string mode)
    {
        using var f=await ReviewFixture.Create();
        if(mode.StartsWith("partner",StringComparison.Ordinal))
        {
            f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_TYPE=="gas").CURRENT_OPERATOR="partner";
            await f.Native.Grant("FIELD","A","READ",inherit:mode!="partner-no-inheritance");
            if(mode=="partner-no-direct"){var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}
        }
        if(mode=="foreign-field")f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_TYPE=="gas").PRIMARY_FIELD_ID="B";
        if(mode=="foreign-owner")f.Native.Rows.OfType<EQUIPMENT>().Single().CURRENT_OWNER_BA_ID="other-company";
        if(mode is "facility-grant" or "equipment-grant")
        {
            var type=mode=="facility-grant"?"FACILITY":"EQUIPMENT";var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType==type);grant.IsActive=false;await f.Native.Db.SaveChangesAsync();
        }
        if(mode=="revoked")f.Revoked=true;
        if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single()));
        if(mode=="changed")
        {
            var reads=0;f.Native.AfterRead=()=>{var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");if(call.Arguments[0] as string=="FACILITY_EQUIPMENT" && ++reads==2)f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=8;return Task.CompletedTask;};
        }
        Task<IReadOnlyList<FACILITY_EQUIPMENT>> Read()=>f.Owner.ReadDevelopmentFacilityEquipmentAsync(f.Context,"plant","gas",f.Guard);
        if(mode is "allowed" or "partner"){var row=Assert.Single(await Read());Assert.Equal("pump",row.EQUIPMENT_ID);Assert.Equal("gas",row.FACILITY_TYPE);Assert.Equal(7,row.INSTALL_OBS_NO);}
        else await Assert.ThrowsAnyAsync<Exception>(Read);
        Assert.Empty(f.Native.Writes);
    }

    [Fact]
    public async Task ActualOwnerReadsExactQualifiedParentsAndCurrentInstallationWithoutStatusReadsOrWrites()
    {
        using var f=await ReviewFixture.Create();var result=await f.Read();result.Require(f.Order);var facility=Assert.Single(result.Facilities);Assert.Equal("gas",facility.FacilityType);Assert.Equal("A",facility.FieldId);Assert.Equal("READ",facility.AccessLevel);var equipment=Assert.Single(result.Equipment);Assert.Equal("pump",equipment.EquipmentId);Assert.Equal(7,equipment.Installation.InstallationObservation);Assert.Equal(1,equipment.Installation.ReviewedPopulation);Assert.Equal("recorded-install",equipment.Installation.Source);Assert.Equal("Asia/Kuwait",result.DateTimeZoneId);Assert.True(ProcessDefinitionRevision.IsValid(result.SourceRevision()));Assert.Empty(f.Native.Writes);Assert.DoesNotContain(f.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] is string table && (table.EndsWith("_STATUS",StringComparison.Ordinal) || table.Contains("LICENSE",StringComparison.Ordinal)));
    }
    [Theory][InlineData("allowed")][InlineData("no-direct")][InlineData("no-inheritance")][InlineData("wrong-field")][InlineData("revoked")]
    public async Task PartnerFacilityMaintenanceEvidenceRequiresExactFieldAndCurrentDirectAuthority(string mode)
    {
        using var f=await ReviewFixture.Create();var facility=f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_TYPE=="gas");facility.CURRENT_OPERATOR="partner";
        await f.Native.Grant("FIELD","A","READ",inherit:mode!="no-inheritance");
        if(mode=="no-direct"){var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}
        if(mode=="wrong-field")facility.PRIMARY_FIELD_ID="B";if(mode=="revoked")f.Revoked=true;
        if(mode=="allowed")
        {
            var result=await f.Read();result.Require(f.Order);var source=Assert.Single(result.Facilities);Assert.Equal("partner",source.OperatorBaId);Assert.Equal("READ",source.AccessLevel);Assert.Empty(f.Native.Writes);
            var direct=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");direct.IsActive=false;await f.Native.Db.SaveChangesAsync();await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());
        Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("owner")][InlineData("field")][InlineData("operator")][InlineData("equipment-grant")][InlineData("facility-grant")][InlineData("schema")][InlineData("physical-key")][InlineData("revoked")][InlineData("missing-install")][InlineData("duplicate-install")][InlineData("overlap")][InlineData("future")][InlineData("expired")][InlineData("local-date")][InlineData("missing-date")][InlineData("component-type")][InlineData("predicate")]
    public async Task UnconfirmedOwnershipAuthoritySchemaOrInstallationCannotProducePartialEvidence(string fault)
    {
        using var f=await ReviewFixture.Create();var equipment=f.Native.Rows.OfType<EQUIPMENT>().Single();var facility=f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_TYPE=="gas");var installation=f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single();
        if(fault=="owner")equipment.CURRENT_OWNER_BA_ID="other-company";if(fault=="field")facility.PRIMARY_FIELD_ID="B";if(fault=="operator")facility.CURRENT_OPERATOR="other-company";
        if(fault is "equipment-grant" or "facility-grant"){var type=fault=="equipment-grant"?"EQUIPMENT":"FACILITY";var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType==type);grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}
        f.SchemaMissing=fault=="schema";f.WrongPhysicalKey=fault=="physical-key";f.Revoked=fault=="revoked";
        if(fault=="missing-install")f.Native.Rows.Remove(installation);if(fault is "duplicate-install" or "overlap"){var copy=CompanyInventorySourceReview.Copy(installation);if(fault=="overlap")copy.INSTALL_OBS_NO=8;f.Native.Rows.Add(copy);}if(fault=="future")installation.INSTALL_DATE=DateTime.UtcNow.AddDays(1);if(fault=="expired")installation.REMOVE_DATE=DateTime.UtcNow.AddDays(-1);if(fault=="local-date")installation.EFFECTIVE_DATE=DateTime.Now;if(fault=="missing-date")installation.INSTALL_DATE=null;if(fault=="component-type")f.Order.Components[0]=f.Order.Components[0] with{FacilityType="oil"};if(fault=="predicate")f.Native.IgnoreFilters=true;
        if(fault is "physical-key" or "schema")await Assert.ThrowsAsync<Beep.OilandGas.PPDM39.Core.NativeSchemaCompatibilityException>(()=>f.Read());else await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());Assert.Empty(f.Native.Writes);Assert.DoesNotContain(f.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] is string table && table.EndsWith("_STATUS",StringComparison.Ordinal));
    }
    [Theory][InlineData("installation")][InlineData("grant")][InlineData("component")][InlineData("schema")]
    public async Task PairedSourceReadRejectsChangedMembershipParentAuthorityAndOriginalScope(string fault)
    {
        using var f=await ReviewFixture.Create();var reads=0;f.Native.AfterRead=async()=>{var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");if(call.Arguments[0] as string!=(fault=="schema"?"FACILITY":"FACILITY_EQUIPMENT") || ++reads!=2)return;if(fault=="schema")f.WrongPhysicalKey=true;if(fault=="installation")f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=9;if(fault=="component")f.Order.Components[0]=f.Order.Components[0] with{NativeRevision=new string('F',64)};if(fault=="grant"){var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Read());Assert.Equal(2,reads);Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task HistoricalInstallationMembershipRemainsInTheReviewedSourceDigestWithoutBecomingCurrent()
    {
        using var f=await ReviewFixture.Create();var historical=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single());historical.INSTALL_OBS_NO=6;historical.ACTIVE_IND="N";historical.EFFECTIVE_DATE=DateTime.UtcNow.AddDays(-30);historical.INSTALL_DATE=DateTime.UtcNow.AddDays(-30);historical.REMOVE_DATE=DateTime.UtcNow.AddDays(-10);historical.EXPIRY_DATE=DateTime.UtcNow.AddDays(-10);f.Native.Rows.Add(historical);
        var first=await f.Read();Assert.Equal(7,Assert.Single(first.Equipment).Installation.InstallationObservation);Assert.Equal(2,first.Equipment[0].Installation.ReviewedPopulation);historical.SOURCE="reviewed-history-correction";var closing=await f.Read();Assert.NotEqual(first.SourceRevision(),closing.SourceRevision());Assert.Equal(7,closing.Equipment[0].Installation.InstallationObservation);Assert.Empty(f.Native.Writes);
    }
    private static FacilityManagementService InZone(ReviewFixture f,string zone)=>new(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),f.Context.Database.ConnectionName,maintenanceAssetAccess:f.Native.Policy,maintenanceEvidenceZone:zone);
    [Theory][InlineData("Asia/Kuwait")][InlineData("America/New_York")]
    public async Task UnspecifiedNativeInstallationDatesUseTheExplicitCompanyZone(string zone)
    {
        using var f=await ReviewFixture.Create();var expected=DateTime.UtcNow.AddDays(-5);var recorded=TimeZoneInfo.ConvertTimeFromUtc(expected,TimeZoneInfo.FindSystemTimeZoneById(zone));var row=f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single();row.INSTALL_DATE=DateTime.SpecifyKind(recorded,DateTimeKind.Unspecified);row.EFFECTIVE_DATE=row.INSTALL_DATE;
        var result=await InZone(f,zone).ReadMaintenanceQualifiedAssetsAsync(f.Context,f.Order,f.Guard);Assert.Equal(zone,result.DateTimeZoneId);Assert.Equal(expected,Assert.Single(result.Equipment).Installation.InstalledAtUtc);Assert.Equal(DateTimeKind.Utc,result.Equipment[0].Installation.InstalledAtUtc.Kind);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData(3,8,2,30)][InlineData(11,1,1,30)]
    public async Task AmbiguousOrInvalidNativeLocalDatesNeverUseTheMachineZone(int month,int day,int hour,int minute)
    {
        using var f=await ReviewFixture.Create();f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_DATE=new DateTime(2026,month,day,hour,minute,0,DateTimeKind.Unspecified);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>InZone(f,"America/New_York").ReadMaintenanceQualifiedAssetsAsync(f.Context,f.Order,f.Guard));Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task UnknownCompanyDateZoneRefusesBeforeNativeIdentityReads()
    {
        using var f=await ReviewFixture.Create();var before=f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync");await Assert.ThrowsAsync<InvalidOperationException>(()=>InZone(f,"not/a/zone").ReadMaintenanceQualifiedAssetsAsync(f.Context,f.Order,f.Guard));Assert.Equal(before,f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync"));Assert.Empty(f.Native.Writes);
    }
}
