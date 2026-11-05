using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Repository.Models;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.Repository;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CompanyProductionBoundaryTests
{
    [Fact]
    public async Task OriginalInterventionRecoveryReadsOnlyItsWellActivityAndRefusesMismatchingRows()
    {
        using var fixture=await Ready();var profile=Profile(fixture);await Save(profile);
        const string well="well/Ø§Ù„ÙƒÙˆÙŠØª%2F",other="other-authorized-well";
        await fixture.Grant("WELL",well,"WRITE");await fixture.Grant("WELL",other,"WRITE");NativeDecisionSource(fixture);NativeDecisionProvider(fixture);
        fixture.Rows.Add(new WELL{UWI=other,ASSIGNED_FIELD="A",OPERATOR="company",ACTIVE_IND="Y"});
        var otherActivity=new WELL_ACTIVITY{UWI=other,SOURCE="other-source",ACTIVITY_OBS_NO=900,ACTIVE_IND="Y",ACTIVITY_TYPE_ID="WORKOVER_CANDIDATE",ROW_CREATED_BY="other-engineer",REMARK="Unrelated authorized preparation"};fixture.Rows.Add(otherActivity);
        var id=Guid.NewGuid();var result=await Phase(fixture,profile).RecoverNativeInterventionDecisionAsync(well,id,"actor");
        Assert.Null(result.Intent);Assert.Null(result.Outcome);
        var historyReads=fixture.Source.Invocations.Where(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WELL_ACTIVITY").ToArray();
        Assert.NotEmpty(historyReads);Assert.All(historyReads,call=>Assert.Contains((List<TheTechIdea.Beep.Report.AppFilter>)call.Arguments[1],filter=>filter.FieldName=="UWI" && filter.Operator=="=" && filter.FilterValue==well));
        fixture.Source.Setup(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[]{otherActivity});
        await Assert.ThrowsAnyAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>Phase(fixture,profile).RecoverNativeInterventionDecisionAsync(well,id,"actor"));
        var inactive=CompanyInventorySourceReview.Copy(fixture.Rows.OfType<WELL_ACTIVITY>().Single(row=>row.UWI==well));inactive.ACTIVE_IND="N";
        fixture.Source.Setup(value=>value.GetEntityAsync("WELL_ACTIVITY",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new object[]{inactive});
        await Assert.ThrowsAnyAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>Phase(fixture,profile).RecoverNativeInterventionDecisionAsync(well,id,"actor"));
        Assert.Empty(fixture.Writes);Assert.Empty(fixture.LeaseTransactions);Assert.Equal("Unrelated authorized preparation",otherActivity.REMARK);
    }
    [Theory][InlineData("allowed")][InlineData("receiver")][InlineData("wrong-well")][InlineData("no-order-right")][InlineData("component-drift")][InlineData("metadata")]
    public async Task NativeApprovedInterventionRetainsExactRecordedMaintenanceHeaderAndComponent(string fault)
    {
        using var fixture=await Ready();var profile=Profile(fixture);await Save(profile);
        const string well="well/Ø§Ù„ÙƒÙˆÙŠØª%2F",order="order/Ø§Ù„Ø¹Ø§Ù„Ù…%2F";
        await fixture.Grant("WELL",well,"WRITE");if(fault!="no-order-right")await fixture.Grant("WORK_ORDER",order);
        NativeDecisionSource(fixture);NativeDecisionProvider(fixture);
        fixture.Rows.Add(new WORK_ORDER{WORK_ORDER_ID=order,WORK_ORDER_NUMBER="Recorded corrective order",WORK_ORDER_TYPE=Beep.OilandGas.Models.Data.WorkOrder.WorkOrderSubType.Corrective,INSTRUCTIONS="Recorded original scope",ACTIVE_IND="Y"});
        fixture.Rows.Add(new WORK_ORDER_COMPONENT{WORK_ORDER_ID=order,COMPONENT_ID="component",FIELD_ID="A",UWI=fault=="wrong-well"?"another-well":well,EQUIPMENT_ID="equipment",ACTIVE_IND="Y"});
        if(fault=="metadata")
        {
            var original=await fixture.Metadata.Object.GetTableMetadataAsync("WORK_ORDER_COMPONENT");var changed=CompanyInventorySourceReview.Copy(original);changed.ForeignKeys.RemoveAll(key=>key.ForeignKeyColumn=="UWI");
            fixture.Metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_COMPONENT")).ReturnsAsync(changed);
        }
        var callback=false;
        if(fault=="component-drift")fixture.AfterLeaseWrite=row=>{if(row is WELL_INTERVENTION_INTENT){callback=true;fixture.Rows.OfType<WORK_ORDER_COMPONENT>().Single().UWI="changed-well";}};
        var request=await NativeDecisionRequest(fixture,profile);request.WorkOrderId=order;
        if(fault is not ("allowed" or "receiver"))
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>Phase(fixture,profile).RecordNativeInterventionDecisionAsync(well,request,"actor"));
            Assert.Empty(fixture.Rows.OfType<WELL_INTERVENTION_RESULT>());Assert.Single(fixture.Rows.OfType<WELL_ACTIVITY>());
            if(fault=="component-drift"){Assert.True(callback);Assert.Equal(new[]{"begin","rollback"},fixture.LeaseTransactions);}
            else Assert.Empty(fixture.Writes);
            return;
        }
        var result=await Phase(fixture,profile).RecordNativeInterventionDecisionAsync(well,request,"actor");Assert.Equal(order,result.WorkOrderId);
        var intent=Assert.Single(fixture.Rows.OfType<WELL_INTERVENTION_INTENT>());var retained=intent.Restore();Assert.NotNull(retained.Review.WorkOrder);
        Assert.Equal("Recorded original scope",retained.Review.WorkOrder.Header.INSTRUCTIONS);var component=Assert.Single(retained.Review.WorkOrder.Components);
        Assert.Equal(well,component.UWI);Assert.Equal("equipment",component.EQUIPMENT_ID);Assert.Contains("WORK_ORDER_ID:"+order,fixture.Rows.OfType<WELL_ACTIVITY>().Single(row=>row.ACTIVITY_OBS_NO==8).REMARK);
        var writes=fixture.Writes.Count;var recovery=await Phase(fixture,profile).RecoverNativeInterventionDecisionAsync(well,request.IntentId,"actor");Assert.NotNull(recovery.Outcome);Assert.Equal(order,recovery.Outcome.Restore(recovery.Intent!).WorkOrderId);Assert.Equal(writes,fixture.Writes.Count);
        Assert.Equal(3,writes);Assert.Single(fixture.Rows.OfType<WORK_ORDER>());Assert.Single(fixture.Rows.OfType<WORK_ORDER_COMPONENT>());Assert.Null(fixture.LeaseActiveTransaction);
        if(fault=="receiver")
        {
            fixture.Db.Users.Add(new Beep.OilandGas.Repository.Models.OilGasUser{Id="receiver",UserName="receiver",IsActive=true});
            fixture.Db.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>{UserId="receiver",RoleId="admin"});await fixture.Db.SaveChangesAsync();
            fixture.Request.User=new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new("party_id","receiver")],"fixture"));fixture.Request.Items.Clear();
            await fixture.Select("company","receiver");await fixture.Grant("FIELD","A");await fixture.Grant("WELL",well);await fixture.Grant("WORK_ORDER",order);
            var receiving=Profile(fixture);await Save(receiving);
            var source=await Phase(fixture,receiving).ReadInterventionMaintenanceLinkAsync(well,request.IntentId,order,"receiver",new("tenant","company"));
            Assert.Equal("receiver",source.Actor);Assert.Equal("actor",source.Reviewer);Assert.Equal(order,source.WorkOrderId);Assert.Equal(request.Note,source.Reason);Assert.Equal(writes,fixture.Writes.Count);
            await Assert.ThrowsAnyAsync<Exception>(()=>Phase(fixture,receiving).RecoverNativeInterventionDecisionAsync(well,request.IntentId,"receiver"));Assert.Equal(writes,fixture.Writes.Count);
        }
    }
    [Fact]
    public void NativeInterventionJournalUsesCanonicalBoundedBeepSchemasAndOriginalParentMetadata()
    {
        var metadata = ProductionInterventionNativeMetadata.AddTo(new());
        Assert.Equal(2, metadata.Count);
        Assert.All(ProductionInterventionNativeSchema.EntityTypes, type =>
        {
            Assert.Equal("Beep.OilandGas.Models", type.Assembly.GetName().Name);
            Assert.Equal("PPDM_CORE", metadata[type.Name].Module); Assert.Equal("INTENT_ID", metadata[type.Name].PrimaryKeyColumn);
            Assert.Equal(type.FullName, metadata[type.Name].EntityTypeName);
        });
        Assert.Contains(metadata["WELL_INTERVENTION_INTENT"].ForeignKeys, key => key.ForeignKeyColumn == "UWI" && key.ReferencedTable == "WELL" && key.ReferencedPrimaryKey == "UWI");
        Assert.Contains(metadata["WELL_INTERVENTION_INTENT"].ForeignKeys, key => key.ForeignKeyColumn == "FIELD_ID" && key.ReferencedTable == "FIELD" && key.ReferencedPrimaryKey == "FIELD_ID");
        Assert.Contains(metadata["WELL_INTERVENTION_RESULT"].ForeignKeys, key => key.ForeignKeyColumn == "INTENT_ID" && key.ReferencedTable == "WELL_INTERVENTION_INTENT");
        Assert.Throws<InvalidOperationException>(() => ProductionInterventionNativeMetadata.AddTo(metadata));
    }
    [Fact]
    public void ClosedOriginalPreparationCannotMintAnotherNativeDispositionWithoutNewSource()
    {
        var context=new FieldSelectionReview(new("tenant","company"),new("owned","fingerprint"),"actor","revision","A");
        var well=new WELL{UWI="well",ASSIGNED_FIELD="A",OPERATOR="company",ACTIVE_IND="Y"};
        WELL_ACTIVITY[] history=[new(){UWI="well",SOURCE="engineer-source",ACTIVITY_OBS_NO=1,ACTIVE_IND="Y",ACTIVITY_TYPE_ID="WORKOVER_CANDIDATE",ROW_CREATED_BY="engineer",REMARK="Original problem"},new(){UWI="well",SOURCE="engineer-source",ACTIVITY_OBS_NO=2,ACTIVE_IND="Y",ACTIVITY_TYPE_ID="WORKOVER_APPROVED",ROW_CREATED_BY="actor",REMARK="Recorded disposition"}];
        var id=Guid.NewGuid();var request=new ProductionInterventionDecisionRequest{IntentId=id,Context=context,FieldId="A",Decision="REJECTED",Note="Another disposition",Problem="Original problem",InterventionType="WORKOVER_CANDIDATE",ExpectedSourceRevision=ProductionInterventionActivityChecks.Revision(history,context.Company,"actor","A","well")};
        var command=new ProductionInterventionNativeCommand(id,ProductionInterventionNativeReview.Create(context,well,history),request,3);
        Assert.Contains("already has a recorded disposition",Assert.Throws<InvalidOperationException>(command.Require).Message);
    }
    internal static void NativeDecisionProvider(CompanyPPDMRowAccessTests.Fixture fixture)
    {
        fixture.EnableCorrectiveActionTransactions();
        var metadata = ProductionInterventionNativeMetadata.AddTo(new());
        foreach (var pair in metadata)
        {
            fixture.Metadata.Setup(value => value.GetTableMetadataAsync(pair.Key)).ReturnsAsync(pair.Value);
            fixture.Metadata.Setup(value => value.GetPrimaryKeyColumnsAsync(pair.Key)).ReturnsAsync(new[] { "INTENT_ID" });
        }
        var types = ProductionInterventionNativeSchema.EntityTypes.ToDictionary(type => type.Name);
        var physical = types.ToDictionary(pair => pair.Key, pair => new ClassCreator(fixture.Editor.Object).ConvertToEntityStructure(pair.Value));
        fixture.Source.Setup(value => value.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure requested, bool _) => physical[requested.EntityName]);
    }

    internal static async Task<ProductionInterventionDecisionRequest> NativeDecisionRequest(CompanyPPDMRowAccessTests.Fixture fixture,
        RepositoryUserProfileService profile, string decision = "APPROVED")
    {
        var context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var history = fixture.Rows.OfType<WELL_ACTIVITY>().ToArray();
        return new() { IntentId = Guid.NewGuid(), Context = context, FieldId = "A", Decision = decision,
            ExpectedSourceRevision = ProductionInterventionActivityChecks.Revision(history, context.Company, "actor", "A", "well/Ø§Ù„ÙƒÙˆÙŠØª%2F"),
            InterventionType = "WORKOVER_CANDIDATE", Problem = "Original recorded diagnosis", Note = "Independent original decision rationale" };
    }

    internal static void NativeDecisionSource(CompanyPPDMRowAccessTests.Fixture fixture)
    {
        fixture.Rows.Add(new WELL { UWI = "well/Ø§Ù„ÙƒÙˆÙŠØª%2F", ASSIGNED_FIELD = "A", OPERATOR = "company", ACTIVE_IND = "Y" });
        fixture.Rows.Add(new WELL_ACTIVITY { UWI = "well/Ø§Ù„ÙƒÙˆÙŠØª%2F", SOURCE = "engineer-source", ACTIVITY_OBS_NO = 7, ACTIVE_IND = "Y",
            ACTIVITY_TYPE_ID = "WORKOVER_CANDIDATE", ROW_CREATED_BY = "engineer", REMARK = "Original recorded diagnosis" });
    }

    [Theory]
    [InlineData("APPROVED")]
    [InlineData("DEFERRED")]
    [InlineData("REJECTED")]
    public async Task NativeInterventionDecisionRetainsOriginalCommandAndExactOutcomeWithoutChangingPreparation(string decision)
    {
        using var fixture = await Ready(); var profile = Profile(fixture); await Save(profile);
        await fixture.Grant("WELL", "well/Ø§Ù„ÙƒÙˆÙŠØª%2F", "WRITE"); NativeDecisionSource(fixture); NativeDecisionProvider(fixture);
        var original = JsonSerializer.Serialize(fixture.Rows.OfType<WELL_ACTIVITY>().Single());
        var request = await NativeDecisionRequest(fixture, profile, decision); var service = Phase(fixture, profile);
        var result = await service.RecordNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request, "actor");
        Assert.Equal(request.IntentId, result.IntentId); Assert.Equal(decision, result.Decision); Assert.True(result.Success);
        ProductionInterventionDecisionChecks.RequireAcknowledgement(request, result, "well/Ø§Ù„ÙƒÙˆÙŠØª%2F");
        Assert.Equal(8, result.DecisionObservationNumber); Assert.Equal("engineer-source", result.DecisionSource); Assert.Equal(64, result.NativeReceiptRevision.Length);
        var intent = Assert.Single(fixture.Rows.OfType<WELL_INTERVENTION_INTENT>()); var outcome = Assert.Single(fixture.Rows.OfType<WELL_INTERVENTION_RESULT>());
        Assert.Equal(JsonSerializer.Serialize(request), JsonSerializer.Serialize(intent.Restore().Decision));
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(outcome.Restore(intent)));
        Assert.Equal(original, JsonSerializer.Serialize(fixture.Rows.OfType<WELL_ACTIVITY>().Single(row => row.ACTIVITY_OBS_NO == 7)));
        Assert.Equal("actor", fixture.Rows.OfType<WELL_ACTIVITY>().Single(row => row.ACTIVITY_OBS_NO == 8).ROW_CREATED_BY);
        Assert.Equal(new[] { "begin", "commit", "begin", "commit" }, fixture.LeaseTransactions);
        var writes = fixture.Writes.Count; var transactions = fixture.LeaseTransactions.Count;
        var recovered = await Phase(fixture, profile).RecoverNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request.IntentId, "actor");
        Assert.NotNull(recovered.Outcome); Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(recovered.Outcome.Restore(recovered.Intent!)));
        var repeated = await Phase(fixture, profile).RecordNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request, "actor");
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(repeated)); Assert.Equal(writes, fixture.Writes.Count); Assert.Equal(transactions, fixture.LeaseTransactions.Count);
        var changed = CompanyInventorySourceReview.Copy(request); changed.Note = "A different decision cannot reuse this identity";
        await Assert.ThrowsAnyAsync<Exception>(() => Phase(fixture, profile).RecordNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", changed, "actor"));
        Assert.Equal(writes, fixture.Writes.Count); Assert.Null(fixture.LeaseActiveTransaction);
    }

    [Theory] [InlineData("retention")] [InlineData("disposition")]
    public async Task LostNativeCommitResponseIsRecoveredFromItsOriginalJournalWithoutInventingAnOutcome(string point)
    {
        using var fixture = await Ready(); var profile = Profile(fixture); await Save(profile);
        await fixture.Grant("WELL", "well/Ø§Ù„ÙƒÙˆÙŠØª%2F", "WRITE"); NativeDecisionSource(fixture); NativeDecisionProvider(fixture);
        var request = await NativeDecisionRequest(fixture, profile);
        fixture.AfterLeaseWrite = row => { if (point == "retention" && row is WELL_INTERVENTION_INTENT || point == "disposition" && row is WELL_INTERVENTION_RESULT) { fixture.LeaseCommitFailed = true; fixture.LeaseCommitAfterFailure = true; } };
        await Assert.ThrowsAsync<InvalidOperationException>(() => Phase(fixture, profile).RecordNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request, "actor"));
        fixture.LeaseCommitFailed = false; fixture.AfterLeaseWrite = null;
        var writes = fixture.Writes.Count;
        var recovery = await Phase(fixture, profile).RecoverNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request.IntentId, "actor");
        Assert.NotNull(recovery.Intent); Assert.Equal(point == "disposition", recovery.Outcome is not null); Assert.Equal(writes, fixture.Writes.Count);
        if (point == "disposition") { var result = await Phase(fixture, profile).RecordNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request, "actor"); Assert.True(result.Success); Assert.Equal(writes, fixture.Writes.Count); }
        else { var result = await Phase(fixture, profile).RecordNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request, "actor"); Assert.True(result.Success); Assert.Equal(writes + 2, fixture.Writes.Count); }
        Assert.Single(fixture.Rows.OfType<WELL_INTERVENTION_INTENT>()); Assert.Single(fixture.Rows.OfType<WELL_INTERVENTION_RESULT>()); Assert.Null(fixture.LeaseActiveTransaction);
    }

    [Fact]
    public async Task FailedNativeResultInsertionRollsBackOnlyDispositionAndLeavesItsOriginalReservationRecoverable()
    {
        using var fixture = await Ready(); var profile = Profile(fixture); await Save(profile);
        await fixture.Grant("WELL", "well/Ø§Ù„ÙƒÙˆÙŠØª%2F", "WRITE"); NativeDecisionSource(fixture); NativeDecisionProvider(fixture);
        var request = await NativeDecisionRequest(fixture, profile); fixture.LeaseFailWriteNumber = 3;
        await Assert.ThrowsAnyAsync<Exception>(() => Phase(fixture, profile).RecordNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request, "actor"));
        Assert.Single(fixture.Rows.OfType<WELL_INTERVENTION_INTENT>()); Assert.Empty(fixture.Rows.OfType<WELL_INTERVENTION_RESULT>());
        Assert.Equal(7, Assert.Single(fixture.Rows.OfType<WELL_ACTIVITY>()).ACTIVITY_OBS_NO); Assert.Equal(new[] { "begin", "commit", "begin", "rollback" }, fixture.LeaseTransactions);
        var writes = fixture.Writes.Count; var recovery = await Phase(fixture, profile).RecoverNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request.IntentId, "actor");
        Assert.NotNull(recovery.Intent); Assert.Null(recovery.Outcome); Assert.Equal(writes, fixture.Writes.Count);
    }

    [Theory] [InlineData("maker")] [InlineData("read-only")] [InlineData("isolation")]
    public async Task NativeDecisionRequiresIndependentMakerWriteRightsAndRealSerializableOwnership(string fault)
    {
        using var fixture = await Ready(); var profile = Profile(fixture); await Save(profile);
        await fixture.Grant("WELL", "well/Ø§Ù„ÙƒÙˆÙŠØª%2F", fault == "read-only" ? "READ" : "WRITE"); NativeDecisionSource(fixture); NativeDecisionProvider(fixture);
        if (fault == "maker") fixture.Rows.OfType<WELL_ACTIVITY>().Single().ROW_CREATED_BY = "actor";
        if (fault == "isolation") fixture.TransactionIsolation = System.Data.IsolationLevel.ReadCommitted;
        var request = await NativeDecisionRequest(fixture, profile);
        await Assert.ThrowsAnyAsync<Exception>(() => Phase(fixture, profile).RecordNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request, "actor"));
        Assert.Empty(fixture.Writes); Assert.Empty(fixture.Rows.OfType<WELL_INTERVENTION_INTENT>()); Assert.Empty(fixture.Rows.OfType<WELL_INTERVENTION_RESULT>()); Assert.Null(fixture.LeaseActiveTransaction);
        if (fault == "isolation") Assert.Equal(new[] { "begin", "rollback" }, fixture.LeaseTransactions);
        else Assert.Empty(fixture.LeaseTransactions);
    }

    [Theory] [InlineData("metadata")] [InlineData("schema")] [InlineData("parent")]
    public async Task NativeDecisionRefusesChangedMetadataSchemaOrParentBeforePublishingAReservation(string fault)
    {
        using var fixture = await Ready(); var profile = Profile(fixture); await Save(profile);
        await fixture.Grant("WELL", "well/Ø§Ù„ÙƒÙˆÙŠØª%2F", "WRITE"); NativeDecisionSource(fixture); NativeDecisionProvider(fixture);
        var request = await NativeDecisionRequest(fixture, profile); var callback = false;
        if (fault == "metadata") fixture.Metadata.Setup(value => value.GetTableMetadataAsync("WELL_INTERVENTION_INTENT")).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata { PrimaryKeyColumn = "UWI" });
        if (fault == "schema") fixture.Source.Setup(value => value.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure requested, bool _) =>
        {
            callback = true; var type = ProductionInterventionNativeSchema.EntityTypes.Single(type => type.Name == requested.EntityName);
            var value = new ClassCreator(fixture.Editor.Object).ConvertToEntityStructure(type); value.Fields.Single(field => field.FieldName == "INTENT_ID").IsKey = false; return value;
        });
        if (fault == "parent") fixture.AfterLeaseWrite = row => { if (row is WELL_INTERVENTION_INTENT) { callback = true; fixture.Rows.OfType<WELL>().Single().REMARK = "Changed original parent during retention"; } };
        await Assert.ThrowsAnyAsync<Exception>(() => Phase(fixture, profile).RecordNativeInterventionDecisionAsync("well/Ø§Ù„ÙƒÙˆÙŠØª%2F", request, "actor"));
        Assert.Empty(fixture.Rows.OfType<WELL_INTERVENTION_INTENT>()); Assert.Empty(fixture.Rows.OfType<WELL_INTERVENTION_RESULT>()); Assert.Single(fixture.Rows.OfType<WELL_ACTIVITY>());
        if (fault is "schema" or "parent") Assert.True(callback);
        if (fault == "parent") Assert.Equal(new[] { "begin", "rollback" }, fixture.LeaseTransactions);
        else { Assert.Empty(fixture.Writes); Assert.Empty(fixture.LeaseTransactions); }
    }
}
