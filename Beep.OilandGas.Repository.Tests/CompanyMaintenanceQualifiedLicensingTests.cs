using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Beep.DataBase;
using Xunit;
using ReviewFixture=Beep.OilandGas.Repository.Tests.CompanyMaintenanceQualifiedAssetTests.ReviewFixture;
namespace Beep.OilandGas.Repository.Tests;
public sealed partial class CompanyMaintenanceQualifiedLicensingTests
{
    [Fact]
    public async Task PartnerFacilityLicensingUsesDirectAccessEvenWhenFieldGrantIsStronger()
    {
        using var f=await Ready();f.Native.Rows.OfType<FACILITY>().Single(row=>row.FACILITY_TYPE=="gas").CURRENT_OPERATOR="partner";await f.Native.Grant("FIELD","A","WRITE",inherit:true);
        var identity=await f.Read();Assert.Equal("READ",Assert.Single(identity.Facilities).AccessLevel);var licensing=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);licensing.Require(identity);Assert.Single(licensing.Licenses);Assert.Empty(f.Native.Writes);
    }
    private static async Task<ReviewFixture> Ready()
    {
        var f=await ReviewFixture.Create();var utc=DateTime.UtcNow;
        f.Native.Rows.Add(new FACILITY_LICENSE{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="licence-Kuwait",LICENSE_NUM="LIC-001",FACILITY_LICENSE_TYPE="RecordedType",GRANTED_BY_BA_ID="issuer",GRANTED_TO_BA_ID="company",ASSIGNED_FIELD_ID="A",ACTIVE_IND="Y",GRANTED_DATE=utc.AddDays(-20),EFFECTIVE_DATE=utc.AddDays(-10),EXPIRY_DATE=utc.AddDays(100),VIOLATION_IND="N",FEES_PAID_IND="Y",SOURCE="recorded-regulator",REMARK="Private licence remark"});
        f.Native.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="issuer",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1),FIRST_NAME="Private issuer contact"});f.Native.Rows.Add(new BUSINESS_ASSOCIATE{BUSINESS_ASSOCIATE_ID="company",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
        f.Native.Rows.Add(new FACILITY_LIC_TYPE{GRANTED_BY_BA_ID="issuer",FACILITY_LICENSE_TYPE="RecordedType",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
        f.Native.Rows.Add(new FACILITY_LIC_STATUS{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="licence-Kuwait",STATUS_ID="native-licence-status",LICENSE_STATUS_TYPE="RecordedDecision",LICENSE_STATUS="RECORDED_APPROVAL",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddDays(-10),SOURCE="recorded-decision"});
        f.Native.Rows.Add(new R_LIC_STATUS_TYPE{LICENSE_STATUS_TYPE="RecordedDecision",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});f.Native.Rows.Add(new R_LICENSE_STATUS{LICENSE_STATUS_TYPE="RecordedDecision",LICENSE_STATUS="RECORDED_APPROVAL",ACTIVE_IND="Y",EFFECTIVE_DATE=utc.AddYears(-1)});
        AddArea(f);return f;
    }
    private static void AddArea(ReviewFixture f,string id="recorded-area",string license="licence-Kuwait",string active="Y")
    {
        var at=DateTime.UtcNow.AddYears(-1);f.Native.Rows.Add(new FACILITY_LIC_AREA{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID=license,AREA_ID=id,AREA_TYPE="RecordedRegion",DESCRIPTION="Recorded <region> assignment",ACTIVE_IND=active,EFFECTIVE_DATE=at,SOURCE="native-area-assignment",REMARK="Private licence area remark"});
        if(!f.Native.Rows.OfType<AREA>().Any(row=>row.AREA_ID==id && row.AREA_TYPE=="RecordedRegion"))f.Native.Rows.Add(new AREA{AREA_ID=id,AREA_TYPE="RecordedRegion",PREFERRED_NAME="Recorded region name",ACTIVE_IND="Y",EFFECTIVE_DATE=at,SOURCE="native-area-catalog",SOURCE_DOCUMENT_ID="recorded-document-reference",REMARK="Private area remark"});
        if(!f.Native.Rows.OfType<R_AREA_TYPE>().Any())f.Native.Rows.Add(new R_AREA_TYPE{AREA_TYPE="RecordedRegion",ACTIVE_IND="Y",EFFECTIVE_DATE=at});
    }
    private static int Reads(ReviewFixture f)=>f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="FACILITY_LICENSE");
    [Theory][InlineData("opening","authority")][InlineData("opening","source")][InlineData("opening","order")][InlineData("opening","schema")]
    [InlineData("closing","authority")][InlineData("closing","source")][InlineData("closing","order")][InlineData("closing","schema")]
    public async Task ConsecutiveNativeSchemaChecksCloseCurrentAuthorityBeforeTheNextDefinition(string phase,string fault)
    {
        using var f=await Ready();var identity=await f.Read();var writes=f.Native.Writes.Count;
        var shape=f.Native.Source.Object.GetEntityStructure(new EntityStructure{EntityName="FACILITY_LICENSE",DatasourceEntityName="FACILITY_LICENSE"},true);
        var closing=false;var injected=false;var nextAtFault=-1;var readsAtFault=-1;
        int NextDefinitions()=>f.Native.Source.Invocations.Count(call=>call.Method.Name=="GetEntityStructure" && call.Arguments[0] is EntityStructure value && value.EntityName=="FACILITY_LIC_STATUS");
        f.Native.AfterRead=()=>
        {
            var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");
            if(call.Arguments[0] as string=="FACILITY_LIC_VIOLATION")closing=true;
            return Task.CompletedTask;
        };
        f.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(value=>value.EntityName=="FACILITY_LICENSE"),true)).Returns(()=>
        {
            if(injected || phase=="closing" && !closing)return shape;
            if(fault=="authority")f.Revoked=true;
            if(fault=="source")f.Native.Editor.Setup(value=>value.GetDataSource(f.Context.Database.ConnectionName)).Returns(Mock.Of<TheTechIdea.Beep.IDataSource>());
            if(fault=="order")f.Order.Components[0]=f.Order.Components[0] with{NativeRevision=new string('F',64)};
            if(fault=="schema")shape.Fields.Single(value=>value.FieldName=="LICENSE_ID").IsKey=false;
            nextAtFault=NextDefinitions();readsAtFault=Reads(f);injected=true;return shape;
        });
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));
        Assert.True(injected);Assert.Equal(nextAtFault,NextDefinitions());Assert.Equal(readsAtFault,Reads(f));Assert.Equal(writes,f.Native.Writes.Count);
        if(phase=="opening")Assert.Equal(0,readsAtFault);else Assert.True(readsAtFault>0);
    }
    [Fact]
    public async Task ActualNativeReaderPreservesQualifiedLicenceIssuerHolderDatesAndReferencesWithoutApprovalInference()
    {
        using var f=await Ready();var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);var license=Assert.Single(result.Licenses);Assert.Equal("gas",license.FacilityType);Assert.Equal("issuer",license.IssuerBaId);Assert.Equal("company",license.HolderBaId);Assert.Equal("LIC-001",license.LicenseNumber);Assert.True(license.Covers(result.ComparedAtUtc));Assert.Equal("RECORDED_APPROVAL",Assert.Single(result.Statuses).Value);Assert.Equal(6,result.References.Count);Assert.Contains("do not establish legal or operating approval",Assert.Single(result.Blockers));
        var combined=identity with{Licensing=result};combined.Require(f.Order);Assert.NotEqual(identity.SourceRevision(),combined.SourceRevision());var body=JsonSerializer.Serialize(result);Assert.DoesNotContain("Private licence remark",body);Assert.DoesNotContain("Private issuer contact",body);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("missing")][InlineData("expired")][InlineData("future")][InlineData("holder")][InlineData("field")][InlineData("issuer")][InlineData("type")][InlineData("status-reference")][InlineData("status-type")][InlineData("status-overlap")][InlineData("missing-grant-date")][InlineData("local-date")][InlineData("fees")][InlineData("violation-indicator")]
    public async Task MissingOrUnresolvedLicenceApplicabilityRemainsAnExplicitFinding(string fault)
    {
        using var f=await Ready();var license=f.Native.Rows.OfType<FACILITY_LICENSE>().Single();
        if(fault=="missing")f.Native.Rows.Remove(license);if(fault=="expired")license.EXPIRY_DATE=DateTime.UtcNow.AddDays(-1);if(fault=="future")license.EFFECTIVE_DATE=DateTime.UtcNow.AddDays(1);if(fault=="holder")license.GRANTED_TO_BA_ID="other-company";if(fault=="field")license.ASSIGNED_FIELD_ID="B";if(fault=="issuer")f.Native.Rows.RemoveAll(row=>row is BUSINESS_ASSOCIATE party && party.BUSINESS_ASSOCIATE_ID=="issuer");if(fault=="type")f.Native.Rows.RemoveAll(row=>row is FACILITY_LIC_TYPE);if(fault=="status-reference")f.Native.Rows.RemoveAll(row=>row is R_LICENSE_STATUS);if(fault=="status-type")f.Native.Rows.RemoveAll(row=>row is R_LIC_STATUS_TYPE);if(fault=="missing-grant-date")license.GRANTED_DATE=null;if(fault=="local-date")license.GRANTED_DATE=DateTime.Now;if(fault=="fees")license.FEES_PAID_IND=null!;if(fault=="violation-indicator")license.VIOLATION_IND="Y";
        if(fault=="status-overlap"){var row=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_LIC_STATUS>().Single());row.STATUS_ID="other-current-status";f.Native.Rows.Add(row);}
        var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);Assert.True(result.Blockers.Count>1);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("owner")][InlineData("grant")][InlineData("installation")]
    public async Task LicensingCannotReadThroughAChangedParentOrRevokedAuthority(string fault)
    {
        using var f=await Ready();var identity=await f.Read();if(fault=="owner")f.Native.Rows.OfType<EQUIPMENT>().Single().CURRENT_OWNER_BA_ID="other-company";if(fault=="installation")f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=8;if(fault=="grant"){var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}var before=Reads(f);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));Assert.Equal(before,Reads(f));Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("owner")][InlineData("grant")][InlineData("installation")]
    public async Task ClosingLicensingParentCheckRejectsChangesAfterTheSecondLicencePopulation(string change)
    {
        using var f=await Ready();var identity=await f.Read();var reads=0;
        f.Native.AfterRead=async()=>
        {
            var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");
            if(call.Arguments[0] as string!="FACILITY_LICENSE" || ++reads!=2)return;
            if(change=="owner")f.Native.Rows.OfType<EQUIPMENT>().Single().CURRENT_OWNER_BA_ID="other-company";
            if(change=="installation")f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single().INSTALL_OBS_NO=8;
            if(change=="grant"){var grant=await f.Native.Db.Set<AppUserAssetAccess>().SingleAsync(row=>row.AssetType=="FACILITY");grant.IsActive=false;await f.Native.Db.SaveChangesAsync();}
        };
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));
        Assert.Equal(2,reads);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("foreign")][InlineData("duplicate")]
    public async Task LicenceCompoundKeysAndPredicatesCannotChooseAnotherQualifiedFacility(string fault)
    {
        using var f=await Ready();var identity=await f.Read();var row=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_LICENSE>().Single());if(fault=="foreign"){row.FACILITY_TYPE="oil";f.Native.Source.Setup(source=>source.GetEntityAsync("FACILITY_LICENSE",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[]{row});}else f.Native.Rows.Add(row);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task CompleteConditionsAndViolationsRemainRecordedEvidenceWithExplicitPolicyFindings()
    {
        using var f=await Ready();var at=DateTime.UtcNow.AddDays(-5);f.Native.Rows.OfType<FACILITY_LICENSE>().Single().VIOLATION_IND="Y";
        f.Native.Rows.Add(new FACILITY_LIC_COND{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="licence-Kuwait",CONDITION_ID="condition",CONDITION_TYPE="Inspection",CONDITION_CODE="RecordedCheck",DUE_CONDITION="RecordedDue",DESCRIPTION="Recorded <inspection> condition",ACTIVE_IND="Y",EFFECTIVE_DATE=at,DUE_DATE=at.AddDays(1),FULFILLED_IND="N",EXEMPT_IND="N",SOURCE="recorded-condition"});f.Native.Rows.Add(new R_FAC_LIC_COND{CONDITION_TYPE="Inspection",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FAC_LIC_COND_CODE{CONDITION_TYPE="Inspection",CONDITION_CODE="RecordedCheck",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FAC_LIC_DUE_CONDITION{DUE_CONDITION="RecordedDue",ACTIVE_IND="Y"});
        f.Native.Rows.Add(new FACILITY_LIC_VIOLATION{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="licence-Kuwait",VIOLATION_ID="violation",CONDITION_ID="condition",VIOLATION_TYPE="RecordedViolation",RESOLVE_TYPE="RecordedResolution",ACTIVE_IND="Y",EFFECTIVE_DATE=at,VIOLATION_DATE=at,RESOLVE_DATE=at.AddDays(1),SOURCE="recorded-violation"});f.Native.Rows.Add(new R_FAC_LIC_VIOLATION_TYPE{VIOLATION_TYPE="RecordedViolation",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FAC_LIC_VIOL_RESOL{RESOLVE_TYPE="RecordedResolution",ACTIVE_IND="Y"});
        var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);Assert.Equal("Recorded <inspection> condition",Assert.Single(result.Conditions).Description);Assert.NotNull(Assert.Single(result.Violations).ResolvedAtUtc);Assert.Contains(result.Blockers,value=>value.Contains("numeric terms/units",StringComparison.Ordinal));Assert.Contains(result.Blockers,value=>value.Contains("a date alone is not clearance",StringComparison.Ordinal));Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task PairedLicensingReadRejectsAChangedOriginalHolderOrStatusWithoutWrites()
    {
        using var f=await Ready();var identity=await f.Read();var reads=0;f.Native.AfterRead=()=>{var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");if(call.Arguments[0] as string=="FACILITY_LICENSE" && ++reads==2)f.Native.Rows.OfType<FACILITY_LICENSE>().Single().GRANTED_TO_BA_ID="other-company";return Task.CompletedTask;};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));Assert.Equal(2,reads);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("missing-licence-schema")][InlineData("status-key")]
    public async Task LicensingRequiresActualPhysicalSchemaAndEveryCompoundStatusKey(string fault)
    {
        using var f=await Ready();var identity=await f.Read();
        if(fault=="missing-licence-schema")f.Native.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns((string table)=>table!="FACILITY_LICENSE");
        else f.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>{var type=typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;var shape=f.Native.Editor.Object.classCreator.ConvertToEntityStructure(type);var keys=f.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult().ToArray();foreach(var field in shape.Fields)field.IsKey=keys.Contains(field.FieldName,StringComparer.Ordinal) && !(requested.EntityName=="FACILITY_LIC_STATUS" && field.FieldName=="STATUS_ID");return shape;});
        var before=Reads(f);await Assert.ThrowsAsync<Beep.OilandGas.PPDM39.Core.NativeSchemaCompatibilityException>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));Assert.Equal(before,Reads(f));Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task MultipleRecordedLicenceCandidatesArePreservedWithoutAnArbitraryChosenLicence()
    {
        using var f=await Ready();var second=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_LICENSE>().Single());second.LICENSE_ID="second-recorded-licence";second.LICENSE_NUM="LIC-002";f.Native.Rows.Add(second);var status=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_LIC_STATUS>().Single());status.LICENSE_ID=second.LICENSE_ID;f.Native.Rows.Add(status);AddArea(f,license:second.LICENSE_ID);
        var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);Assert.Equal(2,result.Licenses.Count);Assert.Equal(2,result.Statuses.Count);Assert.Contains("explicit",Assert.Single(result.Blockers),StringComparison.OrdinalIgnoreCase);Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task CompleteAreaAssignmentsRetainBothKeysHistorySourcesAndPeriodsWithoutJurisdictionInference()
    {
        using var f=await Ready();AddArea(f,"second-area");AddArea(f,"historical-area",active:"N");
        f.Native.Rows.Add(new AREA{AREA_ID="recorded-area",AREA_TYPE="Country",ACTIVE_IND="Y",PREFERRED_NAME="Wrong qualified country",EFFECTIVE_DATE=DateTime.UtcNow.AddYears(-1)});
        var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);Assert.Equal(3,result.Areas!.Count);Assert.Equal(2,result.Areas.Count(row=>row.Covers(result.ComparedAtUtc)));var area=result.Areas.Single(row=>row.AreaId=="recorded-area");Assert.Equal("RecordedRegion",area.AreaType);Assert.Equal("Recorded region name",area.Area!.PreferredName);Assert.Equal("recorded-document-reference",area.Area.SourceDocumentId);Assert.True(area.Area.Covers(result.ComparedAtUtc));Assert.Contains(result.References,row=>row.Kind=="R_AREA_TYPE" && row.Key=="RecordedRegion");Assert.Single(result.Blockers);
        var body=JsonSerializer.Serialize(result);Assert.DoesNotContain("Wrong qualified country",body);Assert.DoesNotContain("Private area remark",body);Assert.DoesNotContain("Private licence area remark",body);Assert.DoesNotContain("AREA_MAX_LATITUDE",body);Assert.Empty(f.Native.Writes);
        var restored=JsonSerializer.Deserialize<CompanyMaintenanceQualifiedLicensing>(body)!;restored.Require(identity);Assert.Equal(result.SourceRevision(),restored.SourceRevision());
        var changed=result with{Areas=result.Areas.Reverse().ToArray()};Assert.Equal(result.SourceRevision(),changed.SourceRevision());var changedArea=area with{Area=area.Area with{SourceDocumentId="other-reference"}};changed=result with{Areas=result.Areas.Select(row=>row==area?changedArea:row).ToArray()};Assert.NotEqual(result.SourceRevision(),changed.SourceRevision());
    }
    [Theory][InlineData("missing-assignment")][InlineData("missing-area")][InlineData("type-reference")][InlineData("expired-assignment")][InlineData("future-assignment")][InlineData("missing-date")][InlineData("local-date")][InlineData("assignment-source")][InlineData("area-source")][InlineData("area-date")][InlineData("expired-area")][InlineData("inactive-area")]
    public async Task UnresolvedAreaDatesSourcesAndReferencesRemainExplicitApplicabilityFindings(string fault)
    {
        using var f=await Ready();var assignment=f.Native.Rows.OfType<FACILITY_LIC_AREA>().Single();var area=f.Native.Rows.OfType<AREA>().Single();
        if(fault=="missing-assignment")f.Native.Rows.Remove(assignment);if(fault=="missing-area")f.Native.Rows.Remove(area);if(fault=="type-reference")f.Native.Rows.RemoveAll(row=>row is R_AREA_TYPE);if(fault=="expired-assignment")assignment.EXPIRY_DATE=DateTime.UtcNow.AddDays(-1);if(fault=="future-assignment")assignment.EFFECTIVE_DATE=DateTime.UtcNow.AddDays(1);if(fault=="missing-date")assignment.EFFECTIVE_DATE=null;if(fault=="local-date")assignment.EFFECTIVE_DATE=DateTime.Now;if(fault=="assignment-source")assignment.SOURCE=null!;if(fault=="area-source")area.SOURCE=null!;if(fault=="area-date")area.EFFECTIVE_DATE=null;if(fault=="expired-area")area.EXPIRY_DATE=DateTime.UtcNow.AddDays(-1);if(fault=="inactive-area")area.ACTIVE_IND="N";
        var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);Assert.Contains(result.Blockers,value=>value.Contains("area",StringComparison.OrdinalIgnoreCase));Assert.True(result.Blockers.Count>1);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("duplicate-assignment")][InlineData("foreign-assignment")][InlineData("duplicate-area")][InlineData("foreign-area")][InlineData("missing-area-schema")][InlineData("area-key")][InlineData("assignment-key")][InlineData("over-limit")]
    public async Task AreaEvidenceRefusesAmbiguousForeignUnboundedOrUnprovenPhysicalSources(string fault)
    {
        using var f=await Ready();var identity=await f.Read();
        if(fault=="duplicate-assignment")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_LIC_AREA>().Single()));
        if(fault=="foreign-assignment"){var wrong=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_LIC_AREA>().Single());wrong.FACILITY_TYPE="oil";f.Native.Source.Setup(source=>source.GetEntityAsync("FACILITY_LIC_AREA",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[]{wrong});}
        if(fault=="duplicate-area")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<AREA>().Single()));
        if(fault=="foreign-area"){var wrong=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<AREA>().Single());wrong.AREA_TYPE="Country";f.Native.Source.Setup(source=>source.GetEntityAsync("AREA",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[]{wrong});}
        if(fault=="missing-area-schema")f.Native.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns((string table)=>table!="AREA");
        if(fault is "area-key" or "assignment-key")f.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>{var type=typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;var shape=f.Native.Editor.Object.classCreator.ConvertToEntityStructure(type);var keys=f.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult().ToArray();foreach(var field in shape.Fields)field.IsKey=keys.Contains(field.FieldName,StringComparer.Ordinal) && !(requested.EntityName==(fault=="area-key"?"AREA":"FACILITY_LIC_AREA") && field.FieldName=="AREA_TYPE");return shape;});
        if(fault=="over-limit")for(var index=0;index<256;index++){var extra=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<FACILITY_LIC_AREA>().First());extra.AREA_ID="extra-"+index;f.Native.Rows.Add(extra);}
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task PairedAreaReadRejectsChangedReferencedAreaBeforeDeliveringEvidence()
    {
        using var f=await Ready();var identity=await f.Read();var reads=0;f.Native.AfterRead=()=>{var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");if(call.Arguments[0] as string=="AREA" && ++reads==2)f.Native.Rows.OfType<AREA>().Single().SOURCE_DOCUMENT_ID="changed-native-source";return Task.CompletedTask;};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));Assert.Equal(2,reads);Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task ClientCannotSubstituteAnotherQualifiedAreaOrRemoveReviewedMembership()
    {
        using var f=await Ready();var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);var row=Assert.Single(result.Areas!);
        Assert.Throws<InvalidOperationException>(()=>(result with{Areas=[row with{Area=row.Area! with{AreaType="Country"}}]}).Require(identity));
        Assert.Throws<InvalidOperationException>(()=>(result with{Areas=[]}).Require(identity));
        var legacy=result with{Areas=null,Blockers=CompanyMaintenanceQualifiedLicensing.GetBlockers(identity,result.Licenses,result.Statuses,result.Conditions,result.Violations,result.References,result.ComparedAtUtc)};legacy.Require(identity);Assert.DoesNotContain("\"Areas\"",JsonSerializer.Serialize(legacy));Assert.NotEqual(result.SourceRevision(),legacy.SourceRevision());
    }
}
