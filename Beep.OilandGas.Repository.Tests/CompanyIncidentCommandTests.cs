using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyIncidentCommandTests
{
    [Fact]
    public void HseWorkflowOriginalHasOneIncidentKeyAndRejectsForeignPayloads()
    {
        var context = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
        var original = new CompanyHseWorkflowStart(context, new("lifecycle", "lifecycle-binding"), Guid.NewGuid(), "incident", new string('A', 64), new string('B', 64));
        var id = original.InstanceId();
        Assert.Equal(id, (original with { IntentId = Guid.NewGuid(), Context = context with { ActorUserId = "other", Revision = "new-review" } }).InstanceId());
        Assert.NotEqual(id, (original with { Context = context with { Company = new("tenant", "other-company") } }).InstanceId());
        Assert.NotEqual(id, (original with { Context = context with { FieldId = "other-field" } }).InstanceId());
        Assert.NotEqual(id, (original with { LifecycleDatabase = new("lifecycle", "other-binding") }).InstanceId());
        Assert.NotEqual(id, (original with { IncidentId = "other-incident" }).InstanceId());
        var payload = System.Text.Json.JsonSerializer.Serialize(new { Data = new Dictionary<string, object> { [CompanyHseWorkflowStart.PayloadKey] = original } });
        Assert.Equal(original, CompanyHseWorkflowStart.FromPayload(payload, id, "field", "incident"));
        Assert.Throws<InvalidOperationException>(() => CompanyHseWorkflowStart.FromPayload(payload, id, "other-field", "incident"));
        Assert.Throws<InvalidOperationException>(() => CompanyHseWorkflowStart.FromPayload(payload, id, "field", "other-incident"));
        Assert.Throws<InvalidOperationException>(() => CompanyHseWorkflowStart.FromPayload("{}", id, "field", "incident"));
        Assert.Throws<InvalidOperationException>(() => CompanyHseWorkflowStart.FromPayload(new string('x', 262145), id, "field", "incident"));
        Assert.Throws<InvalidOperationException>(() => CompanyHseWorkflowStart.FromPayload(new string('x', 16385), id, "field", "incident"));
        var owner = JsonSerializer.Serialize(original);
        Assert.Throws<InvalidOperationException>(() => CompanyHseWorkflowStart.FromPayload("{\"Data\":{\"" + CompanyHseWorkflowStart.PayloadKey + "\":" + owner + "},\"Data\":{\"" + CompanyHseWorkflowStart.PayloadKey + "\":" + owner + "}}", id, "field", "incident"));
        Assert.Throws<InvalidOperationException>(() => CompanyHseWorkflowStart.FromPayload("{\"Data\":{\"" + CompanyHseWorkflowStart.PayloadKey + "\":" + owner + ",\"" + CompanyHseWorkflowStart.PayloadKey + "\":" + owner + "}}", id, "field", "incident"));
        var largeEnvelope = JsonSerializer.Serialize(new { Data = new Dictionary<string, object> { [CompanyHseWorkflowStart.PayloadKey] = original, ["OtherEvidence"] = new string('x', 20000) } });
        Assert.Equal(original, CompanyHseWorkflowStart.FromPayload(largeEnvelope, id, "field", "incident"));
        var oversizedOwner = System.Text.Json.Nodes.JsonNode.Parse(owner)!; oversizedOwner["Padding"] = new string('x', 16385);
        var largeOwnership = JsonSerializer.Serialize(new { Data = new Dictionary<string, object> { [CompanyHseWorkflowStart.PayloadKey] = oversizedOwner } });
        Assert.Throws<InvalidOperationException>(() => CompanyHseWorkflowStart.FromPayload(largeOwnership, id, "field", "incident"));
        Assert.Throws<InvalidOperationException>(() => (original with { DefinitionRevision = "invalid" }).Require());
    }
#if BEEP_SECURE_CONNECTIONS
    [Theory]
    [InlineData("CLOSED", false)]
    [InlineData("CANCELLED", false)]
    [InlineData("CLOSED", true)]
    public async Task RecordedRcaRemainsReadableAfterTerminalStateWithCurrentAuthority(string state, bool revoked)
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        f.Rows.Add(new R_INCIDENT_CAUSE_TYPE { CAUSE_TYPE = "ROOT", ACTIVE_IND = "Y" });
        f.Rows.Add(new R_INCIDENT_CAUSE_CODE { CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", LONG_NAME = "Process cause", ACTIVE_IND = "Y" });
        var definition = Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.HseIncidentNativeMetadata.AddTo(new())[nameof(HSE_INCIDENT_RCA_INTENT)];
        f.Metadata.Setup(x => x.GetTableMetadataAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync(definition);
        f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync([definition.PrimaryKeyColumn]);
        f.Source.Setup(x => x.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure requested, bool _) =>
        {
            var type = requested.EntityName == nameof(HSE_INCIDENT_RCA_INTENT) ? typeof(HSE_INCIDENT_RCA_INTENT) : typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + requested.EntityName)!;
            var schema = new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(type);
            var keys = f.Metadata.Object.GetPrimaryKeyColumnsAsync(type.Name).GetAwaiter().GetResult();
            foreach (var field in schema.Fields) { field.IsKey = keys.Contains(field.FieldName); if (field.Fieldtype == typeof(string).FullName) field.Size = field.Size1 = 16384; }
            return schema;
        });
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = Commands(f); var review = await commands.ReviewRcaSourceAsync(context, "local");
        var original = new IncidentRcaIntent(Guid.NewGuid(), review, Assert.Single(review.Options), "Recorded process cause requiring corrective work.");
        (await commands.RecordRcaAsync(original)).Require(original);
        f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(r => r.INCIDENT_ID == "local" && r.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = state;
        var writes = f.Writes.Count;
        if (revoked)
        {
            f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(c => c.ClaimValue == "HSE.ManageIncidents")); await f.Db.SaveChangesAsync();
            await Assert.ThrowsAnyAsync<Exception>(() => commands.ReadSharedRcaAsync(context, "local"));
            await Assert.ThrowsAnyAsync<Exception>(() => commands.ReadRecordedRcaAsync(context, "local"));
        }
        else
        {
            Assert.Equal(original.IntentId, Assert.Single(await commands.ReadSharedRcaAsync(context, "local")).IntentId);
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(original), System.Text.Json.JsonSerializer.Serialize(Assert.Single(await commands.ReadRecordedRcaAsync(context, "local"))));
            await Assert.ThrowsAnyAsync<Exception>(() => commands.ReviewRcaSourceAsync(context, "local"));
            await Assert.ThrowsAnyAsync<Exception>(() => commands.RecordRcaAsync(original with { IntentId = Guid.NewGuid() }));
        }
        Assert.Equal(writes, f.Writes.Count);
    }
    [Theory]
    [InlineData("write")]
    [InlineData("failed-1")]
    [InlineData("failed-2")]
    [InlineData("failed-3")]
    [InlineData("failed-4")]
    [InlineData("failed-5")]
    [InlineData("revoked")]
    [InlineData("reference-changed")]
    [InlineData("lost-commit")]
    public async Task ReportCommandOwnsAllRecordsAndReconcilesOriginalIntent(string mode)
    {
        using var f = await Ready(); f.IncidentProjectionEnabled = true;
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" });
        await f.Db.SaveChangesAsync();
        f.Rows.AddRange([new HSE_INCIDENT_SET { INCIDENT_SET_ID = "SET", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_TYPE { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_2", ACTIVE_IND = "Y" },
            new R_HSE_INCIDENT_DETAIL { DETAIL_TYPE = "PRIMARY", ACTIVE_IND = "Y" },
            new R_INCIDENT_RESP_RESULT { RESPONSE_RESULT = IncidentState.Reported, ACTIVE_IND = "Y" }]);
        if (!f.Rows.OfType<R_HSE_COMP_ROLE>().Any(row => row.COMPONENT_ROLE == HseReferenceCodes.FieldContext.PrimaryRole))
            f.Rows.Add(new R_HSE_COMP_ROLE { COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, ACTIVE_IND = "Y" });
        if (!f.Rows.OfType<R_HSE_INCIDENT_COMP_TYPE>().Any(row => row.COMPONENT_TYPE == HseReferenceCodes.FieldContext.ComponentType))
            f.Rows.Add(new R_HSE_INCIDENT_COMP_TYPE { COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType, ACTIVE_IND = "Y" });
        f.Metadata.Setup(x => x.GetTableMetadataAsync(nameof(HSE_INCIDENT_REPORT_INTENT))).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata { EntityTypeName = typeof(HSE_INCIDENT_REPORT_INTENT).FullName!, TableName = nameof(HSE_INCIDENT_REPORT_INTENT), PrimaryKeyColumn = "HSE_INCIDENT_REPORT_INTENT_ID" });
        f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(nameof(HSE_INCIDENT_REPORT_INTENT))).ReturnsAsync(["HSE_INCIDENT_REPORT_INTENT_ID"]);
        f.Source.Setup(source => source.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure requested, bool _) => {
            var type = requested.EntityName == nameof(HSE_INCIDENT_REPORT_INTENT) ? typeof(HSE_INCIDENT_REPORT_INTENT) : typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + requested.EntityName)!;
            var schema = new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(type);
            var keys = f.Metadata.Object.GetPrimaryKeyColumnsAsync(type.Name).GetAwaiter().GetResult().ToArray();
            foreach (var field in schema.Fields) { field.IsKey = keys.Contains(field.FieldName); if (field.Fieldtype == typeof(string).FullName) { field.Size = 16384; field.Size1 = 16384; } }
            return schema;
        });
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f); var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = Commands(f); var options = await commands.ReviewReportOptionsAsync(review);
        var intent = new IncidentReportIntent(Guid.NewGuid(), review, Assert.Single(options.Options), options.Revision,
            new DateTime(2026, 1, 2, 3, 0, 0, DateTimeKind.Utc), "Site", "Recorded incident", "الكويت");
        if (mode.StartsWith("failed-", StringComparison.Ordinal)) f.LeaseFailWriteNumber = int.Parse(mode[7..], System.Globalization.CultureInfo.InvariantCulture);
        if (mode == "revoked") f.AfterLeaseWrite = _ => f.Rows.OfType<ENT_COMPONENT>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
        if (mode == "reference-changed") f.AfterLeaseWrite = _ => f.Rows.OfType<R_HSE_INCIDENT_DETAIL>().Single(row => row.DETAIL_TYPE == "PRIMARY").LONG_NAME = "Changed meaning";
        if (mode == "lost-commit") { f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true; }
        if (mode == "write") Assert.Equal(intent.IncidentId(), await commands.CreateReportAsync(intent));
        else await Assert.ThrowsAnyAsync<Exception>(() => commands.CreateReportAsync(intent));
        if (mode.StartsWith("failed-", StringComparison.Ordinal) || mode is "revoked" or "reference-changed")
        {
            Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT>(), row => row.INCIDENT_ID == intent.IncidentId());
            Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT_DETAIL>(), row => row.INCIDENT_ID == intent.IncidentId());
            Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT_COMPONENT>(), row => row.INCIDENT_ID == intent.IncidentId());
            Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT_RESPONSE>(), row => row.INCIDENT_ID == intent.IncidentId());
            Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT_REPORT_INTENT>(), row => row.INCIDENT_ID == intent.IncidentId());
            Assert.Contains("rollback", f.LeaseTransactions); return;
        }
        f.LeaseCommitFailed = false; f.LeaseCommitAfterFailure = false;
        Assert.Equal(intent.IncidentId(), await commands.CreateReportAsync(intent));
        Assert.Single(f.Rows.OfType<HSE_INCIDENT_REPORT_INTENT>(), row => row.INCIDENT_ID == intent.IncidentId());
        Assert.Single(f.Rows.OfType<HSE_INCIDENT>(), row => row.INCIDENT_ID == intent.IncidentId());
        Assert.Single(f.Rows.OfType<HSE_INCIDENT_DETAIL>(), row => row.INCIDENT_ID == intent.IncidentId());
        Assert.Single(f.Rows.OfType<HSE_INCIDENT_COMPONENT>(), row => row.INCIDENT_ID == intent.IncidentId());
        Assert.Single(f.Rows.OfType<HSE_INCIDENT_RESPONSE>(), row => row.INCIDENT_ID == intent.IncidentId());
        Assert.Equal(5, f.Writes.Count);
        await Assert.ThrowsAnyAsync<Exception>(() => commands.CreateReportAsync(intent with { Description = "Changed original" }));
        Assert.Equal(5, f.Writes.Count);
        if (mode == "write")
        {
            var record = await Service(f).GetByIdAsync(intent.IncidentId());
            Assert.Equal("الكويت", record!.Jurisdiction);
            var retained = Assert.Single(f.Rows.OfType<HSE_INCIDENT_REPORT_INTENT>(), row => row.INCIDENT_ID == intent.IncidentId());
            retained.COMMAND_HASH = new string('F', 64);
            await Assert.ThrowsAnyAsync<Exception>(() => Service(f).GetByIdAsync(intent.IncidentId()));
        }
    }
#endif
    [Fact]
    public async Task IncidentIntentRetainsExactOriginalAndRejectsGenericMutation()
    {
        using var f = await Ready();
        var review = (await CompanyCorrectiveActionAuthorityTests.Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var original = new IncidentReportIntent(Guid.Parse("f44a5b27-3854-40a2-b8f6-66e058948d94"), review,
            new("SET", "SPILL", "TIER_2"), new string('A', 64), new DateTime(2026, 1, 2, 3, 4, 0, DateTimeKind.Utc), "Site", "Incident\nSecond line", "الكويت");
        var row = HSE_INCIDENT_REPORT_INTENT.Create(original);
        new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler().PrepareForInsert(row, "actor");
        Assert.Equal("HSE-F44A5B27385440A2B8F666E058948D94", row.INCIDENT_ID);
        Assert.Equal(original, row.Restore(review.Company, "actor"));
        Assert.Throws<InvalidOperationException>(() => row.Restore(review.Company, "other"));
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(typeof(HSE_INCIDENT_REPORT_INTENT), nameof(HSE_INCIDENT_REPORT_INTENT), row, null, PPDMRowMutation.Insert, "actor"));
        row.COMMAND_JSON = row.COMMAND_JSON.Replace("Site", "Different site", StringComparison.Ordinal);
        Assert.Throws<InvalidOperationException>(() => row.Restore(review.Company, "actor"));
        Assert.Empty(f.Writes);
    }
    [Fact]
    public void IncidentIntentHasBoundedBeepMigrationMetadata()
    {
        var schema = new TheTechIdea.Beep.Tools.ClassCreator(Mock.Of<TheTechIdea.Beep.Editor.IDMEEditor>()).ConvertToEntityStructure(typeof(HSE_INCIDENT_REPORT_INTENT));
        var key = Assert.Single(schema.Fields, field => field.IsKey);
        Assert.Equal("HSE_INCIDENT_REPORT_INTENT_ID", key.FieldName); Assert.Equal(32, key.Size1); Assert.False(key.AllowDBNull);
        foreach (var (name, size) in new[] { ("INCIDENT_ID", 36), ("FIELD_ID", 128), ("COMMAND_JSON", 16384), ("COMMAND_HASH", 64) })
        {
            var field = Assert.Single(schema.Fields, value => value.FieldName == name);
            Assert.Equal(typeof(string).FullName, field.Fieldtype); Assert.Equal(size, field.Size1); Assert.False(field.AllowDBNull);
        }
    }
    [Fact]
    public void IncidentIntentMetadataUsesCoreDatabaseAndRejectsDuplicateOwner()
    {
        var metadata = new Dictionary<string, Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata>();
        Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.HseIncidentNativeMetadata.AddTo(metadata);
        Assert.Equal(4, metadata.Count);
        var table = metadata[nameof(HSE_INCIDENT_REPORT_INTENT)];
        Assert.Equal(typeof(HSE_INCIDENT_REPORT_INTENT).FullName, table.EntityTypeName);
        Assert.Equal("HSE_INCIDENT_REPORT_INTENT_ID", table.PrimaryKeyColumn);
        Assert.Equal("PPDM_CORE", table.Module);
        Assert.Contains(table.ForeignKeys, key => key.ForeignKeyColumn == "FIELD_ID" && key.ReferencedTable == "FIELD" && key.ReferencedPrimaryKey == "FIELD_ID");
        Assert.Contains(table.ForeignKeys, key => key.ForeignKeyColumn == "INCIDENT_ID" && key.ReferencedTable == "HSE_INCIDENT" && key.ReferencedPrimaryKey == "INCIDENT_ID");
        var verification = metadata[nameof(HSE_CA_VERIFICATION)];
        Assert.Equal(typeof(HSE_CA_VERIFICATION).FullName, verification.EntityTypeName);
        Assert.Equal(nameof(HSE_CA_VERIFICATION.HSE_CA_VERIFICATION_ID), verification.PrimaryKeyColumn);
        Assert.Equal("PPDM_CORE", verification.Module);
        Assert.Contains(verification.ForeignKeys, key => key.ForeignKeyColumn == "PROJECT_ID" && key.ReferencedTable == "PROJECT" && key.ReferencedPrimaryKey == "PROJECT_ID");
        Assert.Throws<InvalidOperationException>(() => Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.HseIncidentNativeMetadata.AddTo(metadata));
    }
    internal static async Task<Fixture> Ready(Fixture? fixture = null)
    {
        var f = await CompanyCorrectiveActionCommandTests.Ready(fixture: fixture);
        f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = IncidentState.Reported;
        f.Rows.AddRange([new R_HSE_RESPONSE_TYPE { ACTION_TYPE = "STATUS", ACTIVE_IND = "Y" },
            new R_INCIDENT_BA_ROLE { BA_ROLE = "WITNESS", ACTIVE_IND = "Y" }, new R_INCIDENT_BA_ROLE { BA_ROLE = HseReferenceCodes.Investigator.Role, ACTIVE_IND = "Y" },
            new R_EMPLOYEE_STATUS { EMPLOYEE_STATUS = HseReferenceCodes.Investigator.Assigned, ACTIVE_IND = "Y" },
            new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 17, PARTY_ROLE_OBS_NO = 4, INVOLVED_PARTY_BA_ID = "local-person", INVOLVED_BA_ROLE = "WITNESS", ACTIVE_IND = "Y", REMARK = "recorded participant" }]);
        return f;
    }
    internal static PPDMCompanyIncidentCommands Commands(Fixture f) => new(f.Gate, CompanyCorrectiveActionAuthorityTests.Profile(f), f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy);
    private static HSEIncidentService Service(Fixture f) => new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, CompanyCorrectiveActionAuthorityTests.Profile(f), f.Policy, NullLogger<HSEIncidentService>.Instance, Commands(f));
    internal static async Task<AssignIncidentInvestigatorRequest> Request(Fixture f, string ba = "local-person")
    {
        var review = await Service(f).ReviewInvestigatorAssignmentAsync("local");
        var target = review.Participants.FirstOrDefault(row => row.BusinessAssociateId == ba);
        return new(review.Revision, target?.PartyObservation ?? 17, target?.RoleObservation ?? 4, ba);
    }
    private static async Task Assign(Fixture f, string ba = "local-person")
    {
        var request = await Request(f, ba);
        await Service(f).AssignInvestigatorAsync("local", request, "actor");
    }
#if BEEP_SECURE_CONNECTIONS
    internal static void EnableReportOptionProjections(Fixture f)
    {
        f.IncidentProjectionEnabled = true;
        f.Source.Setup(source => source.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure requested, bool _) =>
            new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + requested.EntityName)!));
    }
    [Theory]
    [InlineData(typeof(short))]
    [InlineData(typeof(double))]
    public async Task RcaWriteRejectsNarrowOrFractionalObservationStorage(Type storage)
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        f.Rows.Add(new R_INCIDENT_CAUSE_TYPE { CAUSE_TYPE = "ROOT", ACTIVE_IND = "Y" });
        f.Rows.Add(new R_INCIDENT_CAUSE_CODE { CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", LONG_NAME = "Process cause", ACTIVE_IND = "Y" });
        f.Metadata.Setup(x => x.GetTableMetadataAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata
            { EntityTypeName = typeof(HSE_INCIDENT_RCA_INTENT).FullName!, TableName = nameof(HSE_INCIDENT_RCA_INTENT), PrimaryKeyColumn = "HSE_INCIDENT_RCA_INTENT_ID" });
        f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync(["HSE_INCIDENT_RCA_INTENT_ID"]);
        f.Source.Setup(source => source.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure requested, bool _) =>
        {
            var type = requested.EntityName == nameof(HSE_INCIDENT_RCA_INTENT) ? typeof(HSE_INCIDENT_RCA_INTENT) : typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + requested.EntityName)!;
            var shape = new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(type);
            var keys = f.Metadata.Object.GetPrimaryKeyColumnsAsync(type.Name).GetAwaiter().GetResult().ToArray();
            foreach (var field in shape.Fields)
            {
                field.IsKey = keys.Contains(field.FieldName);
                if (field.Fieldtype == typeof(string).FullName) field.Size = field.Size1 = 16384;
                if (type == typeof(HSE_INCIDENT_RCA_INTENT) && field.FieldName == "CAUSE_OBS_NO") field.Fieldtype = storage.FullName;
            }
            return shape;
        });
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = Commands(f);
        var review = await commands.ReviewRcaSourceAsync(context, "local");
        var original = new IncidentRcaIntent(Guid.NewGuid(), review, Assert.Single(review.Options), "The recorded process cause requires corrective action.");
        var error = await Assert.ThrowsAsync<Beep.OilandGas.PPDM39.Core.NativeSchemaCompatibilityException>(() => commands.RecordRcaAsync(original));
        Assert.Contains("HSE_INCIDENT_RCA_INTENT.CAUSE_OBS_NO", error.Message);
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
        Assert.DoesNotContain(f.Rows, row => row is HSE_INCIDENT_RCA_INTENT or HSE_INCIDENT_CAUSE);
    }
    [Theory]
    [InlineData("success")]
    [InlineData("fail-original")]
    [InlineData("fail-cause")]
    [InlineData("revoked")]
    [InlineData("changed-reference")]
    [InlineData("lost-commit")]
    public async Task RcaWriteOwnsTheOriginalAndCauseAndReconcilesRetry(string mode)
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        f.Rows.Add(new R_INCIDENT_CAUSE_TYPE { CAUSE_TYPE = "ROOT", ACTIVE_IND = "Y" });
        var reference = new R_INCIDENT_CAUSE_CODE { CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", LONG_NAME = "Process cause", ACTIVE_IND = "Y" };
        f.Rows.Add(reference);
        f.Metadata.Setup(x => x.GetTableMetadataAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync(new Beep.OilandGas.PPDM39.Core.Metadata.PPDMTableMetadata
        { EntityTypeName = typeof(HSE_INCIDENT_RCA_INTENT).FullName!, TableName = nameof(HSE_INCIDENT_RCA_INTENT), PrimaryKeyColumn = "HSE_INCIDENT_RCA_INTENT_ID" });
        f.Metadata.Setup(x => x.GetPrimaryKeyColumnsAsync(nameof(HSE_INCIDENT_RCA_INTENT))).ReturnsAsync(["HSE_INCIDENT_RCA_INTENT_ID"]);
        f.Source.Setup(source => source.GetEntityStructure(It.IsAny<TheTechIdea.Beep.DataBase.EntityStructure>(), true)).Returns((TheTechIdea.Beep.DataBase.EntityStructure requested, bool _) =>
        {
            var type = requested.EntityName == nameof(HSE_INCIDENT_RCA_INTENT) ? typeof(HSE_INCIDENT_RCA_INTENT) : typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + requested.EntityName)!;
            var schema = new TheTechIdea.Beep.Tools.ClassCreator(f.Editor.Object).ConvertToEntityStructure(type);
            var keys = f.Metadata.Object.GetPrimaryKeyColumnsAsync(type.Name).GetAwaiter().GetResult().ToArray();
            foreach (var field in schema.Fields) { field.IsKey = keys.Contains(field.FieldName); if (field.Fieldtype == typeof(string).FullName) { field.Size = 16384; field.Size1 = 16384; } }
            return schema;
        });
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var commands = Commands(f); var review = await commands.ReviewRcaSourceAsync(context, "local");
        var original = new IncidentRcaIntent(Guid.NewGuid(), review, Assert.Single(review.Options), "The recorded process cause requires corrective action.");
        if (mode == "fail-original") f.LeaseFailWriteNumber = 1;
        if (mode == "fail-cause") f.LeaseFailWriteNumber = 2;
        if (mode == "revoked") f.AfterLeaseWrite = _ => f.Rows.OfType<ENT_COMPONENT>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
        if (mode == "changed-reference") f.AfterLeaseWrite = _ => reference.LONG_NAME = "Changed reference";
        if (mode == "lost-commit") { f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true; }
        if (mode == "success")
        {
            var receipt = await commands.RecordRcaAsync(original); receipt.Require(original);
            var retry = await commands.RecordRcaAsync(original); Assert.Equal(receipt, retry); Assert.Equal(2, f.Writes.Count);
            var recovered = Assert.Single(await commands.ReadRecordedRcaAsync(context, "local"));
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(original), System.Text.Json.JsonSerializer.Serialize(recovered));
            var shared = Assert.Single(await commands.ReadSharedRcaAsync(context, "local"));
            shared.Require(); Assert.Equal(original.IntentId, shared.IntentId); Assert.Equal(original.Summary, shared.Summary);
            Assert.DoesNotContain("COMMAND_JSON", System.Text.Json.JsonSerializer.Serialize(shared));
            var otherContext = context with { ActorUserId = "another-author" };
            var otherOriginal = original with { IntentId = Guid.NewGuid(), Source = review with { Context = otherContext, NextObservation = 2 } };
            var otherIntent = HSE_INCIDENT_RCA_INTENT.Create(otherOriginal);
            var otherCause = new HSE_INCIDENT_CAUSE { INCIDENT_ID = "local", CAUSE_OBS_NO = 2, ACTIVE_IND = "Y", CAUSE_TYPE = "ROOT",
                CAUSE_CODE = otherOriginal.Option.CauseCode, REMARK = otherOriginal.Summary, SOURCE = "Beep.OilandGas.HSE", EFFECTIVE_DATE = DateTime.UtcNow };
            var columns = new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler();
            columns.PrepareForInsert(otherIntent, "another-author"); columns.PrepareForInsert(otherCause, "another-author");
            f.Rows.Add(otherIntent); f.Rows.Add(otherCause);
            Assert.Single(await commands.ReadRecordedRcaAsync(context, "local"));
            var sharedByOthers = await commands.ReadSharedRcaAsync(context, "local");
            Assert.Equal(2, sharedByOthers.Count); Assert.Contains(sharedByOthers, value => value.IntentId == otherOriginal.IntentId && value.RecordedBy == "another-author");
            f.Rows.Remove(otherIntent); f.Rows.Remove(otherCause);
            var recordedCause = Assert.Single(f.Rows.OfType<HSE_INCIDENT_CAUSE>());
            recordedCause.REMARK = "tampered";
            await Assert.ThrowsAnyAsync<Exception>(() => commands.ReadRecordedRcaAsync(context, "local"));
            await Assert.ThrowsAnyAsync<Exception>(() => commands.ReadSharedRcaAsync(context, "local"));
            recordedCause.REMARK = original.Summary;
            await Assert.ThrowsAnyAsync<Exception>(() => commands.RecordRcaAsync(original with { Summary = "A different summary cannot replace the original request." }));
            Assert.Single(f.Rows.OfType<HSE_INCIDENT_RCA_INTENT>()); Assert.Single(f.Rows.OfType<HSE_INCIDENT_CAUSE>());
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(() => commands.RecordRcaAsync(original));
            if (mode == "lost-commit")
            {
                f.LeaseCommitFailed = false;
                Assert.Equal(System.Text.Json.JsonSerializer.Serialize(original), System.Text.Json.JsonSerializer.Serialize(Assert.Single(await commands.ReadRecordedRcaAsync(context, "local"))));
                var receipt = await commands.RecordRcaAsync(original); receipt.Require(original);
                Assert.Single(f.Rows.OfType<HSE_INCIDENT_RCA_INTENT>()); Assert.Single(f.Rows.OfType<HSE_INCIDENT_CAUSE>()); Assert.Equal(2, f.Writes.Count);
            }
            else { Assert.Empty(f.Rows.OfType<HSE_INCIDENT_RCA_INTENT>()); Assert.Empty(f.Rows.OfType<HSE_INCIDENT_CAUSE>()); }
        }
    }
    [Theory]
    [InlineData("recorded")]
    [InlineData("duplicate")]
    [InlineData("changed")]
    [InlineData("missing-projection")]
    [InlineData("permission")]
    [InlineData("retired-parent")]
    [InlineData("duplicate-code")]
    public async Task RcaSourceReviewUsesRecordedCauseKeysAndCurrentAuthority(string scenario)
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        var cause = new HSE_INCIDENT_CAUSE { INCIDENT_ID = "local", CAUSE_OBS_NO = 1, ACTIVE_IND = "Y", CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", REMARK = "Recorded cause", EFFECTIVE_DATE = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        f.Rows.Add(cause);
        f.Rows.Add(new HSE_INCIDENT_CAUSE { INCIDENT_ID = "local", CAUSE_OBS_NO = scenario == "duplicate" ? 1 : 2, ACTIVE_IND = "N", CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", REMARK = "Retired cause" });
        f.Rows.Add(new R_INCIDENT_CAUSE_TYPE { CAUSE_TYPE = "ROOT", ACTIVE_IND = scenario == "retired-parent" ? "N" : "Y" });
        f.Rows.Add(new R_INCIDENT_CAUSE_CODE { CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", LONG_NAME = "Process cause", ACTIVE_IND = "Y" });
        if (scenario == "duplicate-code") f.Rows.Add(new R_INCIDENT_CAUSE_CODE { CAUSE_TYPE = "ROOT", CAUSE_CODE = "PROCESS", ACTIVE_IND = "Y" });
        if (scenario == "changed") f.AfterRead = () =>
        {
            if (f.Source.Invocations.Any(call => call.Method.Name == "GetEntityAsync" && call.Arguments[0] as string == "HSE_INCIDENT_CAUSE")) cause.REMARK = "Changed cause";
            return Task.CompletedTask;
        };
        if (scenario == "missing-projection") f.IncidentProjectionEnabled = false;
        if (scenario == "permission") { f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(row => row.ClaimValue == "HSE.ManageIncidents")); await f.Db.SaveChangesAsync(); }
        var context = (await CompanyCorrectiveActionAuthorityTests.Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        if (scenario is "recorded" or "retired-parent")
        {
            var result = await Commands(f).ReviewRcaSourceAsync(context, "local"); result.Require();
            Assert.Equal(3, result.NextObservation); Assert.Equal(2, result.Findings.Count); Assert.True(result.Findings[0].Active); Assert.False(result.Findings[1].Active);
            if (scenario == "recorded") Assert.Equal(new IncidentRcaOption("ROOT", "PROCESS", "Process cause"), Assert.Single(result.Options));
            else Assert.Empty(result.Options);
            Assert.Equal(result.Revision, (await Commands(f).ReviewRcaSourceAsync(context, "local")).Revision);
        }
        else await Assert.ThrowsAnyAsync<Exception>(() => Commands(f).ReviewRcaSourceAsync(context, "local"));
        Assert.Empty(f.Writes);
    }
    [Theory]
    [InlineData("reported")]
    [InlineData("changed-state")]
    [InlineData("missing-projection")]
    [InlineData("permission")]
    public async Task WorkflowSourceReviewRequiresReportedCaseAndAcknowledgedStatus(string mode)
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        if (mode == "changed-state") f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = IncidentState.UnderInvestigation;
        if (mode == "missing-projection") f.IncidentProjectionEnabled = false;
        if (mode == "permission")
        {
            f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(row => row.ClaimValue == "HSE.ManageIncidents"));
            await f.Db.SaveChangesAsync();
        }
        var review = (await CompanyCorrectiveActionAuthorityTests.Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        if (mode == "reported")
        {
            var first = await Commands(f).ReviewWorkflowSourceRevisionAsync(review, "local");
            Assert.True(ProcessDefinitionRevision.IsValid(first));
            Assert.Equal(first, await Commands(f).ReviewWorkflowSourceRevisionAsync(review, "local"));
        }
        else await Assert.ThrowsAnyAsync<Exception>(() => Commands(f).ReviewWorkflowSourceRevisionAsync(review, "local"));
        Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task ReportOptionsRefuseAProviderWithoutAcknowledgedProjection()
    {
        using var f = await Ready(); EnableReportOptionProjections(f); f.IncidentProjectionEnabled = false;
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" });
        await f.Db.SaveChangesAsync();
        var review = (await CompanyCorrectiveActionAuthorityTests.Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        await Assert.ThrowsAsync<NotSupportedException>(() => Commands(f).ReviewReportOptionsAsync(review));
        Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task ReportOptionsUseRecordedSetTypeAndSeverityKeys()
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" });
        await f.Db.SaveChangesAsync();
        f.Rows.AddRange([
            new HSE_INCIDENT_SET { INCIDENT_SET_ID = "KUWAIT", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SET { INCIDENT_SET_ID = "NORWAY", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_TYPE { INCIDENT_SET_ID = "ORPHAN", INCIDENT_TYPE_ID = "SPILL", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "ORPHAN", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_1", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_TYPE { INCIDENT_SET_ID = "KUWAIT", INCIDENT_TYPE_ID = "SPILL", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_TYPE { INCIDENT_SET_ID = "NORWAY", INCIDENT_TYPE_ID = "SPILL", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "KUWAIT", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_1", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "NORWAY", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_2", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "OTHER", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_3", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "KUWAIT", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_4", ACTIVE_IND = "N" }]);
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var options = await Commands(f).ReviewReportOptionsAsync(review);
        Assert.Equal(new[] { new IncidentReportOption("KUWAIT", "SPILL", "TIER_1"), new IncidentReportOption("NORWAY", "SPILL", "TIER_2") }, options.Options);
        Assert.Equal(64, options.Revision.Length); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task ReportOptionsRejectDuplicateSeverityKeys()
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" });
        await f.Db.SaveChangesAsync();
        f.Rows.Add(new HSE_INCIDENT_TYPE { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", ACTIVE_IND = "Y" });
        f.Rows.AddRange([new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_1", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_1", ACTIVE_IND = "Y" }]);
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        await Assert.ThrowsAsync<RefusalException>(() => Commands(f).ReviewReportOptionsAsync(review));
        Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task ReportOptionsChangeWhenTheRecordedParentSetIsRetired()
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" });
        await f.Db.SaveChangesAsync();
        var set = new HSE_INCIDENT_SET { INCIDENT_SET_ID = "SET", ACTIVE_IND = "Y" };
        f.Rows.AddRange([set, new HSE_INCIDENT_TYPE { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_1", ACTIVE_IND = "Y" }]);
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var before = await Commands(f).ReviewReportOptionsAsync(review);
        Assert.Single(before.Options);
        set.ACTIVE_IND = "N";
        var after = await Commands(f).ReviewReportOptionsAsync(review);
        Assert.Empty(after.Options); Assert.NotEqual(before.Revision, after.Revision); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task IncidentReportRejectsRetiredOptionsBeforeAnyWrite()
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" });
        await f.Db.SaveChangesAsync();
        var set = new HSE_INCIDENT_SET { INCIDENT_SET_ID = "SET", ACTIVE_IND = "Y" };
        f.Rows.AddRange([set, new HSE_INCIDENT_TYPE { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_SEVERITY { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", INCIDENT_SEVERITY_ID = "TIER_1", ACTIVE_IND = "Y" }]);
        var review = (await CompanyCorrectiveActionAuthorityTests.Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var options = await Commands(f).ReviewReportOptionsAsync(review);
        set.ACTIVE_IND = "N";
        var report = new ReportIncidentRequest("A", "SPILL", 1, DateTime.UtcNow, "site", "incident", "KW", "SET", options.Revision);
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).ReportIncidentAsync(report, "actor"));
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Fact]
    public async Task ReportOptionsRejectUnspecifiedReferenceDates()
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" });
        await f.Db.SaveChangesAsync();
        f.Rows.Add(new HSE_INCIDENT_TYPE { INCIDENT_SET_ID = "SET", INCIDENT_TYPE_ID = "SPILL", ACTIVE_IND = "Y", EFFECTIVE_DATE = new DateTime(2026, 1, 1) });
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        await Assert.ThrowsAsync<RefusalException>(() => Commands(f).ReviewReportOptionsAsync(review));
        Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task ReportOptionsEndpointReturnsTheCurrentReviewedField()
    {
        using var f = await Ready(); EnableReportOptionProjections(f);
        f.Db.RoleClaims.Add(new Microsoft.AspNetCore.Identity.IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" });
        await f.Db.SaveChangesAsync();
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var controller = new Beep.OilandGas.ApiService.Controllers.HSE.IncidentReportOptionsController(Commands(f), profile, f.Account)
        { ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = f.Request } };
        var result = await controller.GetAsync(default);
        var options = Assert.IsType<IncidentReportOptions>(Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result).Value);
        Assert.Equal("A", options.Context.FieldId); Assert.Empty(options.Options); Assert.Empty(f.Writes);
        var attribute = Assert.Single(typeof(Beep.OilandGas.ApiService.Controllers.HSE.IncidentReportOptionsController)
            .GetCustomAttributes(typeof(Beep.OilandGas.ApiService.Attributes.RequireCurrentFieldAccessAttribute), false));
        Assert.NotNull(attribute);
    }
    [Fact]
    public async Task ReportEndpointConfirmsTheOriginalAndRefusesChangedSelection()
    {
        using var f = await Ready();
        var profile = CompanyCorrectiveActionAuthorityTests.Profile(f);
        var selected = await profile.GetCurrentFieldSelectionAsync(); var review = selected.SelectionContext!;
        var original = new IncidentReportIntent(Guid.NewGuid(), review, new("SET", "SPILL", "TIER_2"), new string('A', 64),
            new DateTime(2026, 1, 2, 3, 0, 0, DateTimeKind.Utc), "Site", "Incident", "Kuwait");
        var commands = new Mock<ICompanyIncidentCommands>(MockBehavior.Strict);
        commands.Setup(x => x.CreateReportAsync(original)).ReturnsAsync(original.IncidentId());
        var selections = new Mock<ICompanyFieldSelectionService>(MockBehavior.Strict);
        selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(selected);
        var controller = new Beep.OilandGas.ApiService.Controllers.HSE.IncidentReportController(commands.Object, selections.Object, f.Account)
        { ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext { HttpContext = f.Request } };
        var auth = Assert.Single(typeof(Beep.OilandGas.ApiService.Controllers.HSE.IncidentReportController)
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false).Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
        Assert.Equal("HSE.ReportIncident", auth.Policy);
        Assert.Single(typeof(Beep.OilandGas.ApiService.Controllers.HSE.IncidentReportController)
            .GetCustomAttributes(typeof(Beep.OilandGas.ApiService.Attributes.RequireCurrentFieldAccessAttribute), false));
        var result = await controller.RecordAsync(original, default);
        var receipt = Assert.IsType<IncidentReportReceipt>(Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result).Value);
        receipt.Require(original);
        commands.Verify(x => x.CreateReportAsync(original), Times.Once);

        selections.Reset();
        selections.SetupSequence(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(selected)
            .ReturnsAsync(new Beep.OilandGas.Models.Data.FieldResponse { SelectionAvailable = false });
        await Assert.ThrowsAnyAsync<Exception>(() => controller.RecordAsync(original, default));
        commands.Verify(x => x.CreateReportAsync(original), Times.Exactly(2));
        var otherActor = original with { Context = original.Context with { ActorUserId = "other" } };
        var refusedActor = await Assert.ThrowsAsync<RefusalException>(() => controller.RecordAsync(otherActor, default));
        Assert.Equal(Beep.OilandGas.Models.Core.Refusals.RefusalKind.Forbidden, refusedActor.Kind);
        commands.Verify(x => x.CreateReportAsync(otherActor), Times.Never);

        selections.Reset(); selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(selected);
        commands.Setup(x => x.CreateReportAsync(original)).ReturnsAsync("wrong-id");
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.RecordAsync(original, default));

        selections.Reset();
        selections.Setup(x => x.GetCurrentFieldSelectionAsync()).ReturnsAsync(new Beep.OilandGas.Models.Data.FieldResponse { SelectionAvailable = false });
        await Assert.ThrowsAnyAsync<Exception>(() => controller.RecordAsync(original, default));
        commands.Verify(x => x.CreateReportAsync(original), Times.Exactly(3));
    }
#endif
    [Fact]
    public async Task Assignment_uses_the_recorded_party_full_role_history_and_confirms_audit_and_idempotent_retry()
    {
        using var f = await Ready();
        f.Rows.Add(new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 17, PARTY_ROLE_OBS_NO = 9, ACTIVE_IND = "N", INVOLVED_PARTY_BA_ID = "local-person", INVOLVED_BA_ROLE = HseReferenceCodes.Investigator.Role });
        await Assign(f);
        var row = Assert.Single(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.ACTIVE_IND == "Y" && row.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role);
        Assert.Equal(17, row.PARTY_OBS_NO); Assert.Equal(10, row.PARTY_ROLE_OBS_NO); Assert.Equal("actor", row.ROW_CREATED_BY); Assert.Equal("actor", row.ROW_CHANGED_BY);
        Assert.Equal(HseReferenceCodes.Investigator.Assigned, row.INVOLVED_BA_STATUS); Assert.Null(row.REMARK); Assert.Equal("recorded participant", f.Rows.OfType<HSE_INCIDENT_BA>().Single(value => value.PARTY_OBS_NO == 17 && value.PARTY_ROLE_OBS_NO == 4).REMARK); Assert.Equal("local-person", (await Service(f).GetByIdAsync("local"))!.InvestigatorId);
        Assert.Single(f.Writes); Assert.Equal(new[] { "begin", "commit" }, f.LeaseTransactions);
        await Assign(f); Assert.Single(f.Writes); Assert.Null(f.LeaseActiveTransaction);
    }
    [Fact]
    public async Task Reassignment_retires_the_original_full_key_and_preserves_both_parties_and_history()
    {
        using var f = await Ready();
        f.Rows.AddRange([new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "original-person", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 3, PARTY_ROLE_OBS_NO = 7, INVOLVED_PARTY_BA_ID = "original-person", INVOLVED_BA_ROLE = HseReferenceCodes.Investigator.Role, INVOLVED_BA_STATUS = HseReferenceCodes.Investigator.Assigned, EFFECTIVE_DATE = DateTime.UtcNow.AddDays(-5), ACTIVE_IND = "Y" }]);
        await Assign(f);
        var retired = Assert.Single(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.PARTY_OBS_NO == 3); Assert.Equal(7, retired.PARTY_ROLE_OBS_NO); Assert.Equal("N", retired.ACTIVE_IND); Assert.Equal("original-person", retired.INVOLVED_PARTY_BA_ID); Assert.NotNull(retired.EXPIRY_DATE);
        var assigned = Assert.Single(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role && row.ACTIVE_IND == "Y");
        Assert.Equal(17, assigned.PARTY_OBS_NO); Assert.Equal(5, assigned.PARTY_ROLE_OBS_NO); Assert.Equal(assigned.EFFECTIVE_DATE, retired.EXPIRY_DATE); Assert.Equal(2, f.Writes.Count);
    }
    [Theory] [InlineData("unknown-party")] [InlineData("missing-person")] [InlineData("missing-role")] [InlineData("missing-status")] [InlineData("missing-state")] [InlineData("closed")] [InlineData("ambiguous-state")] [InlineData("duplicate-key")] [InlineData("exhausted-history")]
    public async Task Assignment_refuses_unrecorded_or_ambiguous_references_and_state_before_write(string fault)
    {
        using var f = await Ready();
        switch (fault)
        {
            case "missing-person": f.Rows.RemoveAll(row => row is BUSINESS_ASSOCIATE ba && ba.BUSINESS_ASSOCIATE_ID == "local-person"); break;
            case "missing-role": f.Rows.RemoveAll(row => row is R_INCIDENT_BA_ROLE role && role.BA_ROLE == HseReferenceCodes.Investigator.Role); break;
            case "missing-status": f.Rows.RemoveAll(row => row is R_EMPLOYEE_STATUS); break;
            case "missing-state": f.Rows.RemoveAll(row => row is HSE_INCIDENT_RESPONSE status && status.INCIDENT_ID == "local" && status.RESPONSE_TYPE == "STATUS"); break;
            case "closed": f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = IncidentState.Closed; break;
            case "ambiguous-state": f.Rows.Add(new HSE_INCIDENT_RESPONSE { INCIDENT_ID = "local", RESPONSE_OBS_NO = 9, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = IncidentState.Reported, ACTIVE_IND = "Y", EFFECTIVE_DATE = new(2026,1,2,0,0,0,DateTimeKind.Utc) }); break;
            case "duplicate-key": f.Rows.Add(new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 17, PARTY_ROLE_OBS_NO = 4, INVOLVED_PARTY_BA_ID = "local-person", INVOLVED_BA_ROLE = "WITNESS", ACTIVE_IND = "Y" }); break;
            case "exhausted-history": f.Rows.Add(new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 17, PARTY_ROLE_OBS_NO = int.MaxValue, INVOLVED_PARTY_BA_ID = "local-person", INVOLVED_BA_ROLE = "WITNESS", ACTIVE_IND = "N" }); break;
        }
        await Assert.ThrowsAsync<RefusalException>(() => Assign(f, fault == "unknown-party" ? "foreign-person" : "local-person")); Assert.Empty(f.Writes); Assert.Null(f.LeaseActiveTransaction);
    }
    [Theory] [InlineData(1)] [InlineData(2)]
    public async Task Reassignment_rolls_back_the_retirement_and_new_assignment_when_a_write_fails(int failedWrite)
    {
        using var f = await Ready(); f.Rows.AddRange([new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "original-person", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 3, PARTY_ROLE_OBS_NO = 7, INVOLVED_PARTY_BA_ID = "original-person", INVOLVED_BA_ROLE = HseReferenceCodes.Investigator.Role, INVOLVED_BA_STATUS = HseReferenceCodes.Investigator.Assigned, EFFECTIVE_DATE = DateTime.UtcNow.AddDays(-5), ACTIVE_IND = "Y" }]);
        f.LeaseFailWriteNumber = failedWrite;
        await Assert.ThrowsAnyAsync<Exception>(() => Assign(f));
        var current = Assert.Single(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.ACTIVE_IND == "Y" && row.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role); Assert.Equal("original-person", current.INVOLVED_PARTY_BA_ID);
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Null(f.LeaseActiveTransaction);
    }
    [Theory] [InlineData("manage")] [InlineData("read-permission")] [InlineData("read-grant")] [InlineData("inheritance")] [InlineData("foreign-incident")] [InlineData("actor")] [InlineData("review-field")] [InlineData("review-database")]
    public async Task Direct_incident_command_independently_enforces_current_app_case_FIELD_and_actor_authority(string fault)
    {
        using var f = await Ready();
        if (fault is "manage" or "read-permission") {
            var value = fault == "manage" ? "HSE.ManageIncidents" : "HSE.View"; f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == value).ToArrayAsync()); await f.Db.SaveChangesAsync(); await f.Select("company");
        }
        if (fault is "read-grant" or "inheritance") { var grant = await f.Db.Set<AppUserAssetAccess>().SingleAsync(); if (fault == "read-grant") grant.AccessLevel = "READ"; else grant.Inherit = false; await f.Db.SaveChangesAsync(); }
        var review = (await CompanyCorrectiveActionAuthorityTests.Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        if (fault == "review-field") review = review with { FieldId = "B" };
        if (fault == "review-database") review = review with { Database = review.Database with { Fingerprint = "unreviewed" } };
        await Assert.ThrowsAsync<RefusalException>(() => Commands(f).AssignInvestigatorAsync(review, fault == "foreign-incident" ? "foreign" : "local", new(new string('A', 64), 17, 4, "local-person"), fault == "actor" ? "other" : "actor"));
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("no-op-begin")] [InlineData("failed-begin")] [InlineData("wrong-isolation")]
    public async Task Investigator_command_requires_its_own_serializable_transaction_before_any_write(string fault)
    {
        using var f = await Ready(); f.LeaseBeginNoOp = fault == "no-op-begin"; f.LeaseBeginFailed = fault == "failed-begin"; if (fault == "wrong-isolation") f.TransactionIsolation = System.Data.IsolationLevel.ReadCommitted;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Assign(f)); Assert.Empty(f.Writes); Assert.Null(f.LeaseActiveTransaction);
    }
    [Theory] [InlineData("no-effect")] [InlineData("payload")]
    public async Task Investigator_provider_OK_requires_the_exact_committed_roster_effect(string fault)
    {
        using var f = await Ready();
        if (fault == "no-effect") f.Source.Setup(value => value.InsertEntity("HSE_INCIDENT_BA", It.IsAny<object>())).Returns(new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Ok });
        else f.AfterLeaseWrite = row => { if (row is HSE_INCIDENT_BA ba && ba.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role) ba.REMARK = "provider substituted payload"; };
        await Assert.ThrowsAsync<RefusalException>(() => Assign(f));
        Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role && row.ACTIVE_IND == "Y"); Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Null(f.LeaseActiveTransaction);
    }
    [Theory] [InlineData("state")] [InlineData("entitlement")] [InlineData("grant")] [InlineData("grant-revision")]
    public async Task Investigator_case_or_write_authority_change_during_provider_write_rolls_back(string fault)
    {
        using var f = await Ready(); f.AfterLeaseWrite = row => {
            if (row is not HSE_INCIDENT_BA) return;
            if (fault == "state") f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(value => value.INCIDENT_ID == "local" && value.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = IncidentState.Closed;
            else if (fault == "entitlement") f.Rows.OfType<ENTITLEMENT>().Single().ACCESS_COND_CODE = "test-read";
            else { var grant = f.Db.Set<AppUserAssetAccess>().Single(); if (fault == "grant-revision") grant.ConcurrencyStamp = Guid.NewGuid().ToString(); else grant.AssetId = "B"; f.Db.SaveChanges(); }
        };
        await Assert.ThrowsAsync<RefusalException>(() => Assign(f)); Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role && row.ACTIVE_IND == "Y");
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Null(f.LeaseActiveTransaction);
    }
    [Fact]
    public async Task Investigator_fresh_review_confirms_a_durable_assignment_after_lost_commit_acknowledgment()
    {
        using var f = await Ready(); f.LeaseCommitFailed = true; f.LeaseCommitAfterFailure = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => Assign(f));
        Assert.Single(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role && row.ACTIVE_IND == "Y");
        f.LeaseCommitFailed = false; await Assign(f); Assert.Single(f.Writes); Assert.Null(f.LeaseActiveTransaction);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Investigator_restricted_case_requires_its_reviewed_binding_before_assignment(bool bind)
    {
        using var f = await Ready(); CompanyHseRowAccessTests.Restrict(f); await CompanyHseRowAccessTests.ReviewPermissions(f);
        if (bind) { await CompanyHseRowAccessTests.Bind(f); await Assign(f); Assert.Single(f.Writes); }
        else { await Assert.ThrowsAsync<RefusalException>(() => Assign(f)); Assert.Empty(f.Writes); }
    }

    [Fact]
    public async Task Investigator_one_shot_state_change_during_reference_read_is_refused_before_transaction()
    {
        using var f = await Ready(); var reads = 0;
        f.Source.Setup(value => value.GetEntityAsync("R_HSE_RESPONSE_TYPE", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).Returns(() => {
            if (++reads == 1) f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_TYPE == "STATUS").REMARK = "changed during state reference";
            return Task.FromResult<IEnumerable<object>>(f.Rows.OfType<R_HSE_RESPONSE_TYPE>().Cast<object>().ToArray());
        });
        await Assert.ThrowsAsync<RefusalException>(() => Assign(f)); Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Fact]
    public async Task Investigator_case_interpretation_change_rolls_back_even_if_both_permissions_still_allow_read()
    {
        using var f = await Ready(); CompanyHseRowAccessTests.Restrict(f); await CompanyHseRowAccessTests.ReviewPermissions(f); await CompanyHseRowAccessTests.Bind(f, "HSE.View");
        f.AfterLeaseWrite = row => { if (row is HSE_INCIDENT_BA) f.EligibilityConfiguration.Companies.Single().HseCaseReadRestrictions.Single().RequiredPermission = "HSE.ViewRestricted"; };
        await Assert.ThrowsAsync<RefusalException>(() => Assign(f)); Assert.DoesNotContain(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role && row.ACTIVE_IND == "Y");
        Assert.Equal(new[] { "begin", "rollback" }, f.LeaseTransactions); Assert.Null(f.LeaseActiveTransaction);
    }

    [Theory] [InlineData(IncidentState.PendingCorrectiveAction)] [InlineData(IncidentState.PendingCloseOut)]
    public async Task Investigator_assignment_preserves_all_existing_nonterminal_workflow_stages(string state)
    {
        using var f = await Ready(); f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = state;
        await Assign(f); Assert.Single(f.Writes);
        Assert.Equal(state, (await Service(f).GetByIdAsync("local"))!.CurrentState);
    }

    [Fact]
    public async Task Investigator_review_is_read_only_exposes_complete_keys_and_excludes_inactive_history()
    {
        using var f = await Ready();
        f.Rows.Add(new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 17, PARTY_ROLE_OBS_NO = 9, ACTIVE_IND = "N", INVOLVED_PARTY_BA_ID = "local-person" });
        var review = await Service(f).ReviewInvestigatorAssignmentAsync("local");
        Assert.Equal("local", review.IncidentId); Assert.Equal("actor", review.Context.ActorUserId); Assert.Equal("A", review.Context.FieldId);
        Assert.Equal(64, review.Revision.Length); Assert.Null(review.CurrentInvestigator);
        Assert.Contains(review.Participants, row => row.PartyObservation == 17 && row.RoleObservation == 4 && row.BusinessAssociateId == "local-person" && row.Role == "WITNESS");
        Assert.DoesNotContain(review.Participants, row => row.RoleObservation == 9);
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("active-roster")] [InlineData("inactive-history")] [InlineData("grant-revision")] [InlineData("state")] [InlineData("case")] [InlineData("participant-reference")]
    public async Task Investigator_submission_requires_the_original_full_review_revision(string change)
    {
        using var f = await Ready(); var request = await Request(f);
        if (change == "active-roster") f.Rows.OfType<HSE_INCIDENT_BA>().Single(row => row.PARTY_OBS_NO == 17).REMARK = "changed witness statement";
        if (change == "inactive-history") f.Rows.Add(new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 17, PARTY_ROLE_OBS_NO = 9, ACTIVE_IND = "N" });
        if (change == "grant-revision") { f.Db.Set<AppUserAssetAccess>().Single().ConcurrencyStamp = Guid.NewGuid().ToString(); await f.Db.SaveChangesAsync(); }
        if (change == "state") f.Rows.OfType<HSE_INCIDENT_RESPONSE>().Single(row => row.INCIDENT_ID == "local" && row.RESPONSE_TYPE == "STATUS").RESPONSE_RESULT = IncidentState.UnderInvestigation;
        if (change == "case") f.Rows.OfType<HSE_INCIDENT>().Single(row => row.INCIDENT_ID == "local").REMARK = "changed case";
        if (change == "participant-reference") f.Rows.OfType<R_INCIDENT_BA_ROLE>().Single(row => row.BA_ROLE == "WITNESS").REMARK = "changed role interpretation";
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).AssignInvestigatorAsync("local", request, "actor"));
        Assert.Empty(f.Writes); Assert.Empty(f.LeaseTransactions);
    }
    [Theory] [InlineData("party")] [InlineData("role")] [InlineData("ba")] [InlineData("fractional-party")] [InlineData("zero-role")] [InlineData("missing-revision")]
    public async Task Investigator_submission_cannot_substitute_a_target_or_omit_complete_review(string change)
    {
        using var f = await Ready(); var request = await Request(f);
        request = change switch {
            "party" => request with { PartyObservation = 3 }, "role" => request with { RoleObservation = 99 },
            "ba" => request with { BusinessAssociateId = "foreign-person" }, "fractional-party" => request with { PartyObservation = 17.5m },
            "zero-role" => request with { RoleObservation = 0 }, _ => request with { ExpectedRevision = "" }
        };
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).AssignInvestigatorAsync("local", request, "actor")); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Investigator_old_request_cannot_reverse_an_intervening_reassignment()
    {
        using var f = await Ready();
        f.Rows.AddRange([new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "next-person", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 21, PARTY_ROLE_OBS_NO = 1, INVOLVED_PARTY_BA_ID = "next-person", INVOLVED_BA_ROLE = "WITNESS", ACTIVE_IND = "Y" }]);
        var original = await Request(f); await Service(f).AssignInvestigatorAsync("local", original, "actor");
        await Assign(f, "next-person"); var writes = f.Writes.Count;
        await Assert.ThrowsAsync<RefusalException>(() => Service(f).AssignInvestigatorAsync("local", original, "actor"));
        Assert.Equal(writes, f.Writes.Count); Assert.Equal("next-person", (await Service(f).GetByIdAsync("local"))!.InvestigatorId);
    }
    [Fact]
    public async Task Investigator_exact_role_keys_allow_selection_when_a_BA_has_multiple_recorded_roles()
    {
        using var f = await Ready(); f.Rows.Add(new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 21, PARTY_ROLE_OBS_NO = 1, INVOLVED_PARTY_BA_ID = "local-person", INVOLVED_BA_ROLE = "WITNESS", ACTIVE_IND = "Y" });
        var review = await Service(f).ReviewInvestigatorAssignmentAsync("local");
        await Service(f).AssignInvestigatorAsync("local", new(review.Revision, 21, 1, "local-person"), "actor");
        var assigned = Assert.Single(f.Rows.OfType<HSE_INCIDENT_BA>(), row => row.INVOLVED_BA_ROLE == HseReferenceCodes.Investigator.Role && row.ACTIVE_IND == "Y");
        Assert.Equal(21, assigned.PARTY_OBS_NO); Assert.Equal(2, assigned.PARTY_ROLE_OBS_NO); Assert.Single(f.Writes);
    }

}
