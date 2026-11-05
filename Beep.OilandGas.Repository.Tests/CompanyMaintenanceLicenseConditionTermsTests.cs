using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Xunit;
using ReviewFixture=Beep.OilandGas.Repository.Tests.CompanyMaintenanceQualifiedAssetTests.ReviewFixture;
namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CompanyMaintenanceQualifiedLicensingTests
{
    private static async Task<ReviewFixture> ReadyTerms()
    {
        var f=await Ready();var at=DateTime.UtcNow.AddYears(-1);
        f.Native.Rows.Add(new FACILITY_LIC_COND{FACILITY_ID="plant",FACILITY_TYPE="gas",LICENSE_ID="licence-Kuwait",CONDITION_ID="numeric-condition",CONDITION_TYPE="RecordedCondition",CONDITION_CODE="RecordedCode",DUE_CONDITION="RecordedDue",CONDITION_VALUE=0m,CONDITION_VALUE_UOM="value-unit",DUE_TERM=2.5m,DUE_TERM_UOM="time-unit",DUE_FREQUENCY="Recorded frequency text",PRODUCT_TYPE="recorded-product",RESTRICTION_ID="recorded-restriction",RESTRICTION_VERSION=2m,FULFILLED_IND="N",EXEMPT_IND="N",ACTIVE_IND="Y",EFFECTIVE_DATE=at,SOURCE="native-condition",CONTACT_BA_ID="Private contact"});
        f.Native.Rows.Add(new R_FAC_LIC_COND{CONDITION_TYPE="RecordedCondition",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FAC_LIC_COND_CODE{CONDITION_TYPE="RecordedCondition",CONDITION_CODE="RecordedCode",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_FAC_LIC_DUE_CONDITION{DUE_CONDITION="RecordedDue",ACTIVE_IND="Y"});
        f.Native.Rows.Add(new PPDM_UNIT_OF_MEASURE{UOM_ID="value-unit",UOM_NAME="Recorded value unit",UOM_QUANTITY_TYPE="RecordedQuantity",UOM_SYSTEM_ID="RecordedSystem",UOM_USAGE_TYPE="RecordedUsage",ACTIVE_IND="Y",EFFECTIVE_DATE=at,SOURCE="native-unit",SOURCE_DOCUMENT_ID="unit-document",REMARK="Private unit remark"});
        f.Native.Rows.Add(new PPDM_UNIT_OF_MEASURE{UOM_ID="time-unit",UOM_NAME="Recorded term unit",UOM_QUANTITY_TYPE="RecordedTime",UOM_SYSTEM_ID="RecordedSystem",UOM_USAGE_TYPE="RecordedUsage",ACTIVE_IND="Y",EFFECTIVE_DATE=at,SOURCE="native-unit"});
        f.Native.Rows.Add(new PPDM_QUANTITY{QUANTITY_TYPE="RecordedQuantity",ACTIVE_IND="Y"});f.Native.Rows.Add(new PPDM_QUANTITY{QUANTITY_TYPE="RecordedTime",ACTIVE_IND="Y"});f.Native.Rows.Add(new PPDM_MEASUREMENT_SYSTEM{UOM_SYSTEM_ID="RecordedSystem",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_PPDM_UOM_USAGE{UOM_USAGE_TYPE="RecordedUsage",ACTIVE_IND="Y"});f.Native.Rows.Add(new SUBSTANCE{SUBSTANCE_ID="recorded-product",ACTIVE_IND="Y"});
        f.Native.Rows.Add(new RESTRICTION{RESTRICTION_ID="recorded-restriction",RESTRICTION_VERSION=2.00m,RESTRICTION_NAME="Recorded restriction two",DESCRIPTION="Recorded <restriction> text",REGULATORY_ACT="Recorded act",RESTRICTION_CLASS="RecordedClass",RESTRICTION_TYPE="RecordedType",REST_DURATION_TYPE="RecordedDuration",ACTIVE_IND="Y",EFFECTIVE_DATE=at,START_DATE=at,SOURCE="native-restriction",SOURCE_DOCUMENT_ID="restriction-document",REMARK="Private restriction remark"});
        f.Native.Rows.Add(new RESTRICTION{RESTRICTION_ID="recorded-restriction",RESTRICTION_VERSION=3m,RESTRICTION_NAME="Wrong later restriction",ACTIVE_IND="Y",EFFECTIVE_DATE=at});
        f.Native.Rows.Add(new REST_CLASS{RESTRICTION_CLASS_ID="RecordedClass",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_REST_TYPE{RESTRICTION_TYPE="RecordedType",ACTIVE_IND="Y"});f.Native.Rows.Add(new R_REST_DURATION{REST_DURATION_TYPE="RecordedDuration",ACTIVE_IND="Y"});return f;
    }
    [Fact]
    public async Task NativeTermsPreserveRecordedZeroFractionalTermExactRestrictionAndReferenceCatalogs()
    {
        using var f=await ReadyTerms();var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);var terms=Assert.Single(result.Conditions).Terms!;
        Assert.Equal(0m,terms.Value);Assert.Equal(2.5m,terms.DueTerm);Assert.Equal("value-unit",terms.ValueUnit!.UnitId);Assert.Equal("RecordedTime",terms.DueTermUnit!.QuantityType);Assert.Equal(2m,terms.Restriction!.Version);Assert.Equal("Recorded restriction two",terms.Restriction.Name);Assert.True(terms.Restriction.Covers(result.ComparedAtUtc));Assert.Equal("restriction-document",terms.Restriction.SourceDocumentId);Assert.Equal("Recorded frequency text",terms.DueFrequency);Assert.Equal("recorded-product",terms.ProductId);
        Assert.DoesNotContain(result.Blockers,value=>value.Contains("provenance is unconfirmed",StringComparison.Ordinal));Assert.Contains(result.Blockers,value=>value.Contains("numeric terms/units",StringComparison.Ordinal));Assert.Contains(result.References,row=>row.Kind=="PPDM_QUANTITY" && row.Key=="RecordedTime");Assert.Contains(result.References,row=>row.Kind=="SUBSTANCE" && row.Key=="recorded-product");
        var json=JsonSerializer.Serialize(result);Assert.DoesNotContain("Private contact",json);Assert.DoesNotContain("Private unit remark",json);Assert.DoesNotContain("Private restriction remark",json);Assert.DoesNotContain("Wrong later restriction",json);Assert.Equal(result.SourceRevision(),JsonSerializer.Deserialize<Beep.OilandGas.Models.Data.WorkOrder.CompanyMaintenanceQualifiedLicensing>(json)!.SourceRevision());Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("null-value")][InlineData("zero-value")][InlineData("null-term")][InlineData("zero-term")][InlineData("null-version")][InlineData("zero-version")]
    public async Task OptionalNativeDecimalsDistinguishUnconfirmedNullFromRecordedZero(string mode)
    {
        Assert.Null(new FACILITY_LIC_COND().CONDITION_VALUE);Assert.Null(new FACILITY_LIC_COND().DUE_TERM);Assert.Null(new FACILITY_LIC_COND().RESTRICTION_VERSION);
        using var f=await ReadyTerms();var condition=f.Native.Rows.OfType<FACILITY_LIC_COND>().Single();
        if(mode=="null-value")condition.CONDITION_VALUE=null;if(mode=="zero-value")condition.CONDITION_VALUE=0;if(mode=="null-term")condition.DUE_TERM=null;if(mode=="zero-term")condition.DUE_TERM=0;if(mode=="null-version")condition.RESTRICTION_VERSION=null;
        if(mode=="zero-version"){condition.RESTRICTION_VERSION=0;f.Native.Rows.OfType<RESTRICTION>().Single(row=>row.RESTRICTION_VERSION==2m).RESTRICTION_VERSION=0;}
        var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);var terms=Assert.Single(result.Conditions).Terms!;
        Assert.Equal(condition.CONDITION_VALUE,terms.Value);Assert.Equal(condition.DUE_TERM,terms.DueTerm);Assert.Equal(condition.RESTRICTION_VERSION,terms.RestrictionVersion);
        if(mode=="null-value")Assert.Contains(result.Blockers,value=>value.Contains("numeric value/unit",StringComparison.Ordinal));if(mode=="null-term")Assert.Contains(result.Blockers,value=>value.Contains("due term/unit",StringComparison.Ordinal));if(mode=="null-version")Assert.Null(terms.Restriction);if(mode=="zero-version")Assert.Equal(0m,terms.Restriction!.Version);Assert.Empty(f.Native.Writes);
    }
    [Fact]
    public async Task ProviderDbNullOptionalNumericColumnsRemainNullThroughTheActualCanonicalRepository()
    {
        using var f=await ReadyTerms();var condition=f.Native.Rows.OfType<FACILITY_LIC_COND>().Single();var values=condition.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(property=>property.CanRead && property.CanWrite).ToDictionary(property=>property.Name,property=>property.GetValue(condition)??DBNull.Value);values["CONDITION_VALUE"]=DBNull.Value;values["DUE_TERM"]=DBNull.Value;values["RESTRICTION_VERSION"]=DBNull.Value;
        f.Native.Source.Setup(source=>source.GetEntityAsync("FACILITY_LIC_COND",It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{values});var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);var terms=Assert.Single(result.Conditions).Terms!;Assert.Null(terms.Value);Assert.Null(terms.DueTerm);Assert.Null(terms.RestrictionVersion);Assert.Null(terms.Restriction);Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("missing-unit")][InlineData("expired-unit")][InlineData("unit-source")][InlineData("quantity")][InlineData("system")][InlineData("usage")][InlineData("product")][InlineData("missing-restriction")][InlineData("expired-restriction")][InlineData("restriction-source")][InlineData("restriction-class")][InlineData("restriction-type")][InlineData("restriction-duration")][InlineData("restriction-event")]
    public async Task UnprovenNativeTermsAndRestrictionApplicabilityRemainExplicitFindings(string mode)
    {
        using var f=await ReadyTerms();var unit=f.Native.Rows.OfType<PPDM_UNIT_OF_MEASURE>().First();var restriction=f.Native.Rows.OfType<RESTRICTION>().First();
        if(mode=="missing-unit")f.Native.Rows.Remove(unit);if(mode=="expired-unit")unit.EXPIRY_DATE=DateTime.UtcNow.AddDays(-1);if(mode=="unit-source")unit.SOURCE=null!;if(mode=="quantity")f.Native.Rows.RemoveAll(row=>row is PPDM_QUANTITY);if(mode=="system")f.Native.Rows.RemoveAll(row=>row is PPDM_MEASUREMENT_SYSTEM);if(mode=="usage")f.Native.Rows.RemoveAll(row=>row is R_PPDM_UOM_USAGE);if(mode=="product")f.Native.Rows.RemoveAll(row=>row is SUBSTANCE);if(mode=="missing-restriction")f.Native.Rows.Remove(restriction);if(mode=="expired-restriction")restriction.EXPIRY_DATE=DateTime.UtcNow.AddDays(-1);if(mode=="restriction-source")restriction.SOURCE=null!;if(mode=="restriction-class")f.Native.Rows.RemoveAll(row=>row is REST_CLASS);if(mode=="restriction-type")f.Native.Rows.RemoveAll(row=>row is R_REST_TYPE);if(mode=="restriction-duration")f.Native.Rows.RemoveAll(row=>row is R_REST_DURATION);if(mode=="restriction-event")restriction.END_DATE_EVENT=DateTime.UtcNow.AddDays(10);
        var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);result.Require(identity);Assert.Contains(result.Blockers,value=>value.Contains("provenance is unconfirmed",StringComparison.Ordinal) || value.Contains("restriction version/date/source/type/duration",StringComparison.Ordinal) || value.Contains("product/substance",StringComparison.Ordinal));Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("duplicate-unit")][InlineData("foreign-unit")][InlineData("duplicate-restriction")][InlineData("wrong-version")][InlineData("null-version-key")][InlineData("unit-schema")][InlineData("restriction-schema")][InlineData("restriction-key")]
    public async Task TermsRequireExactNativeUnitsAndCompoundRestrictionVersionKeys(string mode)
    {
        using var f=await ReadyTerms();var identity=await f.Read();
        if(mode=="duplicate-unit")f.Native.Rows.Add(Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<PPDM_UNIT_OF_MEASURE>().First()));
        if(mode=="foreign-unit"){var wrong=Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<PPDM_UNIT_OF_MEASURE>().First());wrong.UOM_ID="foreign-unit";f.Native.Source.Setup(source=>source.GetEntityAsync("PPDM_UNIT_OF_MEASURE",It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{wrong});}
        if(mode=="duplicate-restriction")f.Native.Rows.Add(Beep.OilandGas.Models.Data.Accounting.CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<RESTRICTION>().First()));
        if(mode=="wrong-version")f.Native.Source.Setup(source=>source.GetEntityAsync("RESTRICTION",It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{f.Native.Rows.OfType<RESTRICTION>().Last()});
        if(mode=="null-version-key")f.Native.Source.Setup(source=>source.GetEntityAsync("RESTRICTION",It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[]{new Dictionary<string,object>{{"RESTRICTION_ID","recorded-restriction"},{"RESTRICTION_VERSION",DBNull.Value},{"ACTIVE_IND","Y"}}});
        if(mode is "unit-schema" or "restriction-schema")f.Native.Source.Setup(source=>source.CheckEntityExist(It.IsAny<string>())).Returns((string table)=>table!=(mode=="unit-schema"?"PPDM_UNIT_OF_MEASURE":"RESTRICTION"));
        if(mode=="restriction-key")f.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool _)=>{var type=typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+requested.EntityName)!;var shape=f.Native.Editor.Object.classCreator.ConvertToEntityStructure(type);var keys=f.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(requested.EntityName).GetAwaiter().GetResult().ToArray();foreach(var field in shape.Fields)field.IsKey=keys.Contains(field.FieldName,StringComparer.Ordinal) && !(requested.EntityName=="RESTRICTION" && field.FieldName=="RESTRICTION_VERSION");return shape;});
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("PPDM_UNIT_OF_MEASURE")][InlineData("RESTRICTION")]
    public async Task CompletePairedTermsReadRejectsChangedNativeUnitOrRestrictionSource(string table)
    {
        using var f=await ReadyTerms();var identity=await f.Read();var reads=0;f.Native.AfterRead=()=>{var call=f.Native.Source.Invocations.Last(value=>value.Method.Name=="GetEntityAsync");if(call.Arguments[0] as string==table && ++reads==(table=="RESTRICTION"?2:3)){if(table=="RESTRICTION")f.Native.Rows.OfType<RESTRICTION>().First().REGULATORY_ACT="changed source";else f.Native.Rows.OfType<PPDM_UNIT_OF_MEASURE>().First().UOM_NAME="changed unit";}return Task.CompletedTask;};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard));Assert.Empty(f.Native.Writes);
    }
    [Theory][InlineData("unit")][InlineData("restriction-version")][InlineData("local-period")]
    public async Task SharedEvidenceRejectsSubstitutedUnitRestrictionVersionOrNonUtcPeriods(string mode)
    {
        using var f=await ReadyTerms();var identity=await f.Read();var result=await f.Owner.ReadMaintenanceQualifiedLicensingAsync(f.Context,f.Order,identity,f.Guard);var condition=Assert.Single(result.Conditions);var terms=condition.Terms!;
        terms=mode switch{"unit"=>terms with{ValueUnit=terms.ValueUnit! with{UnitId="other-unit"}},"restriction-version"=>terms with{Restriction=terms.Restriction! with{Version=3}},_=>terms with{Restriction=terms.Restriction! with{EffectiveAtUtc=DateTime.Now}}};
        Assert.Throws<InvalidOperationException>(()=>(result with{Conditions=[condition with{Terms=terms}]}).Require(identity));Assert.Empty(f.Native.Writes);
    }
}
