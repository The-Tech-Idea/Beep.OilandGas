using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.DataManagement.Services.HSE;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Report;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;

namespace Beep.OilandGas.Repository.Tests;

// Real profile/access registry/eligibility/policy/repositories/incident and RCA services; explicit recording Beep provider.
public sealed class CompanyHseRowAccessTests
{
    public static IEnumerable<object[]> Tables => new[] { typeof(HSE_INCIDENT), typeof(HSE_INCIDENT_COMPONENT), typeof(HSE_INCIDENT_DETAIL), typeof(HSE_INCIDENT_RESPONSE), typeof(HSE_INCIDENT_CAUSE), typeof(HSE_INCIDENT_BA) }.Select(type => new object[] { type.Name });
    private static Type Entity(string name) => typeof(HSE_INCIDENT).Assembly.GetType("Beep.OilandGas.PPDM39.Models." + name)!;
    private static RepositoryUserProfileService Profile(Fixture f)
    { f.Http.HttpContext = f.Request; return new(f.Db, new RepositoryApplicationAuthorizationReader(f.Gate), new UpperInvariantLookupNormalizer(), f.Account, f.Gate, f.Service); }
    private static HSEIncidentService Incident(Fixture f, RepositoryUserProfileService profile) => new(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, NullLogger<HSEIncidentService>.Instance);
    private static async Task<Fixture> Ready(bool inherit = true, bool boundedPages = false)
    {
        var f = await Fixture.Create(boundedPages: boundedPages);
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.View });
        await f.Db.SaveChangesAsync(); await f.Grant("FIELD", "A", inherit: inherit);
        f.Rows.AddRange([new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" }, new FIELD { FIELD_ID = "B", ACTIVE_IND = "Y" },
            new R_HSE_COMP_ROLE { COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, ACTIVE_IND = "Y" },
            new R_HSE_INCIDENT_COMP_TYPE { COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType, ACTIVE_IND = "Y" }]);
        Seed(f, "local", "A"); Seed(f, "foreign", "B");
        var profile = Profile(f); await profile.SetCurrentFieldSelectionAsync(new() { FieldId = "A", Context = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext });
        return f;
    }
    private static void Seed(Fixture f, string id, string field)
    {
        if (!f.Rows.OfType<BUSINESS_ASSOCIATE>().Any(row => row.BUSINESS_ASSOCIATE_ID == "recorded-investigator"))
            f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "recorded-investigator", ACTIVE_IND = "Y" });
        if (!f.Rows.OfType<R_INCIDENT_BA_ROLE>().Any(row => row.BA_ROLE == "INVESTIGATOR"))
            f.Rows.Add(new R_INCIDENT_BA_ROLE { BA_ROLE = "INVESTIGATOR", ACTIVE_IND = "Y" });
        f.Rows.AddRange([new HSE_INCIDENT { INCIDENT_ID = id, ACTIVE_IND = "Y", INCIDENT_DATE = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) },
            new HSE_INCIDENT_COMPONENT { INCIDENT_ID = id, COMPONENT_OBS_NO = 1, FIELD_ID = field, COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType, ACTIVE_IND = "Y" },
            new HSE_INCIDENT_DETAIL { INCIDENT_ID = id, DETAIL_OBS_NO = 1, DETAIL_TYPE = "PRIMARY", INCIDENT_SEVERITY_ID = "TIER_2", INCIDENT_TYPE_ID = "test-recorded-type", REMARK = "recorded description", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_RESPONSE { INCIDENT_ID = id, RESPONSE_OBS_NO = 1, RESPONSE_TYPE = "STATUS", RESPONSE_RESULT = "Reported", EFFECTIVE_DATE = new(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), ACTIVE_IND = "Y" },
            new HSE_INCIDENT_CAUSE { INCIDENT_ID = id, CAUSE_OBS_NO = 1, CAUSE_TYPE = "test-recorded-cause", REMARK = "recorded cause", ACTIVE_IND = "Y" },
            new HSE_INCIDENT_BA { INCIDENT_ID = id, PARTY_OBS_NO = 1, PARTY_ROLE_OBS_NO = 1, INVOLVED_BA_ROLE = "INVESTIGATOR", INVOLVED_PARTY_BA_ID = "recorded-investigator", ACTIVE_IND = "Y" }]);
    }
    private static HSE_INCIDENT_BA Participant(Fixture f) => f.Rows.OfType<HSE_INCIDENT_BA>().Single(row => row.INCIDENT_ID == "local");
    [Theory]
    [InlineData("valid")]
    [InlineData("empty")]
    [InlineData("unsupported")]
    [InlineData("foreign")]
    [InlineData("correlation")]
    [InlineData("count")]
    [InlineData("bytes")]
    [InlineData("visibility-drift")]
    [InlineData("revoked")]
    [InlineData("out-of-range")]
    [InlineData("cancelled")]
    [InlineData("withheld")]
    public async Task VisibleIncidentPageBoundsProviderWorkAfterAuthorityAndRejectsUnconfirmedWindows(string scenario)
    {
        using var f = await Ready(boundedPages: scenario != "unsupported"); var profile = Profile(f);
        for (var i = 0; i < 11; i++) Seed(f, "case" + i.ToString("D2"), "A");
        if (scenario == "empty") f.Rows.RemoveAll(row => row is HSE_INCIDENT_COMPONENT component && component.FIELD_ID == "A");
        if (scenario == "withheld")
        {
            Restrict(f); await ReviewPermissions(f); await Bind(f);
            f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(claim => claim.ClaimValue == "HSE.ViewRestricted").ToArrayAsync());
            await f.Db.SaveChangesAsync(); await f.Select("company"); profile = Profile(f);
        }
        var calls = 0;
        var window = scenario == "withheld" ? new[] { "case10" } : new[] { "case10", "local" };
        if (scenario != "unsupported")
            f.Source.As<TheTechIdea.Beep.DataBase.IBoundedPagedDataSource>()
                .Setup(source => source.ReadPageAsync(nameof(HSE_INCIDENT), It.IsAny<TheTechIdea.Beep.DataBase.BoundedPageRequest>(), It.IsAny<CancellationToken>()))
                .Returns(async (string entity, TheTechIdea.Beep.DataBase.BoundedPageRequest request, CancellationToken token) =>
                {
                    ++calls; Assert.Equal(1, request.PageNumber); Assert.Equal(window.Length, request.PageSize); Assert.Equal(256 * 1024, request.MaxPayloadBytes);
                    Assert.Equal("INCIDENT_ID", Assert.Single(request.Order).FieldName);
                    Assert.Equal(window, request.CopyAnyFilters().Select(filter => filter.FilterValue));
                    Assert.All(request.CopyAnyFilters(), filter => { Assert.Equal("INCIDENT_ID", filter.FieldName); Assert.Equal("=", filter.Operator); });
                    var active = Assert.Single(request.CopyFilters()); Assert.Equal("ACTIVE_IND", active.FieldName); Assert.Equal("Y", active.FilterValue);
                    if (scenario == "visibility-drift") Seed(f, "appeared", "A");
                    if (scenario == "revoked")
                    {
                        f.Db.RoleClaims.Remove(f.Db.RoleClaims.Single(claim => claim.ClaimValue == "HSE.View")); await f.Db.SaveChangesAsync();
                    }
                    var rows = f.Rows.OfType<HSE_INCIDENT>().Where(row => window.Contains(row.INCIDENT_ID)).OrderBy(row => row.INCIDENT_ID, StringComparer.Ordinal).ToArray();
                    if (scenario == "foreign") rows[0] = f.Rows.OfType<HSE_INCIDENT>().Single(row => row.INCIDENT_ID == "foreign");
                    var payload = scenario == "bytes" ? new byte[request.MaxPayloadBytes + 1] : System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(rows);
                    return new TheTechIdea.Beep.DataBase.BoundedPageResponse(scenario == "correlation" ? Guid.NewGuid() : request.RequestId,
                        1, window.Length, scenario == "count" ? window.Length + 1 : window.Length, payload);
                });
        f.Source.Invocations.Clear();
        Task<HseIncidentPage> Read() => Incident(f, profile).GetVisibleFieldIncidentPageAsync("A",
            scenario == "empty" ? 1 : scenario == "out-of-range" ? 3 : 2, new CancellationToken(scenario == "cancelled"));
        if (scenario is "valid" or "empty" or "withheld")
        {
            var page = await Read(); page.Require(); Assert.Equal("A", page.Context.FieldId);
            Assert.Equal(scenario == "empty" ? 0 : scenario == "withheld" ? 11 : 12, page.VisibleIncidentCount); Assert.False(page.HasNextPage);
            Assert.Equal(scenario == "empty" ? Array.Empty<string>() : window, page.IncidentIds);
        }
        else if (scenario == "cancelled") await Assert.ThrowsAsync<OperationCanceledException>(Read);
        else await Assert.ThrowsAsync<RefusalException>(Read);
        Assert.Equal(scenario is "empty" or "unsupported" or "out-of-range" or "cancelled" ? 0 : 1, calls);
        var details = f.Source.Invocations.Where(call => call.Method.Name == "GetEntityAsync" && call.Arguments[0] as string == nameof(HSE_INCIDENT_DETAIL)).ToArray();
        Assert.Empty(details);
        Assert.Empty(f.Writes);
    }
    [Theory]
    [InlineData("large-field")]
    [InlineData("unrelated-malformed")]
    [InlineData("foreign")]
    [InlineData("duplicate-primary")]
    [InlineData("restricted")]
    [InlineData("late-retirement")]
    [InlineData("foreign-provider-row")]
    [InlineData("contradictory-filter")]
    public async Task ActualIncidentDetailReadCarriesExactFilterThroughEveryPolicyCheck(string scenario)
    {
        using var f = await Ready(); var profile = Profile(f);
        _ = await profile.GetCurrentFieldSelectionAsync();
        for (var i = 0; i < 300; i++) f.Rows.Add(new HSE_INCIDENT_COMPONENT
        {
            INCIDENT_ID = "unrelated-" + i, COMPONENT_OBS_NO = 1, FIELD_ID = "A", ACTIVE_IND = "Y",
            COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType
        });
        if (scenario == "unrelated-malformed") f.Rows.OfType<HSE_INCIDENT_COMPONENT>().First(row => row.INCIDENT_ID.StartsWith("unrelated-", StringComparison.Ordinal)).INCIDENT_ID = "";
        if (scenario == "duplicate-primary") f.Rows.Add(new HSE_INCIDENT_COMPONENT
        {
            INCIDENT_ID = "local", COMPONENT_OBS_NO = 2, FIELD_ID = "B", ACTIVE_IND = "Y",
            COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType
        });
        if (scenario == "restricted")
        {
            var primary = f.Rows.OfType<HSE_INCIDENT_COMPONENT>().Single(row => row.INCIDENT_ID == "local");
            primary.RESTRICTION_ID = "unreviewed"; primary.RESTRICTION_VERSION = 1;
        }
        if (scenario == "late-retirement") f.AfterRead = () =>
        {
            var last = f.Source.Invocations.LastOrDefault(call => call.Method.Name == "GetEntityAsync");
            if (last?.Arguments[0] as string == nameof(HSE_INCIDENT_DETAIL)) f.Rows.OfType<FIELD>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
            return Task.CompletedTask;
        };
        if (scenario == "foreign-provider-row")
            f.Source.Setup(source => source.GetEntityAsync(nameof(HSE_INCIDENT_DETAIL), It.IsAny<List<AppFilter>>()))
                .ReturnsAsync(new object[] { new HSE_INCIDENT_DETAIL { INCIDENT_ID = "foreign", DETAIL_OBS_NO = 1, ACTIVE_IND = "Y", DETAIL_TYPE = "PRIMARY" } });
        f.Source.Invocations.Clear(); f.Queries.Clear();
        if (scenario == "contradictory-filter")
            await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync([
                new() { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = "local" },
                new() { FieldName = "INCIDENT_ID", Operator = "=", FilterValue = "foreign" }]));
        else if (scenario == "foreign") Assert.Null(await Incident(f, profile).GetByIdAsync("foreign"));
        else if (scenario is "large-field" or "unrelated-malformed")
        {
            var record = await Incident(f, profile).GetByIdAsync("local");
            Assert.NotNull(record); Assert.Equal("local", record.IncidentId); Assert.Equal("A", record.FieldId); Assert.Equal("recorded description", record.Description);
        }
        else await Assert.ThrowsAsync<RefusalException>(() => Incident(f, profile).GetByIdAsync("local"));
        var incident = scenario == "foreign" ? "foreign" : "local";
        var primaryReads = f.Source.Invocations.Where(call => call.Method.Name == "GetEntityAsync" && call.Arguments[0] as string == nameof(HSE_INCIDENT_COMPONENT)).ToArray();
        Assert.All(primaryReads, call => Assert.Contains((List<AppFilter>)call.Arguments[1], filter => filter.FieldName == "INCIDENT_ID" && filter.Operator == "=" && filter.FilterValue == incident));
        if (scenario is "large-field" or "unrelated-malformed") Assert.NotEmpty(primaryReads);
        Assert.Empty(f.Writes);
    }
    [Theory]
    [InlineData("large-field")]
    [InlineData("unrelated-malformed")]
    [InlineData("foreign-primary")]
    [InlineData("duplicate-primary")]
    [InlineData("retired-field")]
    [InlineData("late-retirement")]
    [InlineData("unreviewed-restriction")]
    public async Task ExactIncidentAuthorityReadsOnlyItsOwnCaseAndRechecksCurrentField(string scenario)
    {
        using var f = await Ready();
        var profile = Profile(f); var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        // Establish the stored selection before introducing a FIELD larger than a complete-population lookup.
        for (var i = 0; i < 300; i++) f.Rows.Add(new HSE_INCIDENT_COMPONENT
        {
            INCIDENT_ID = "unrelated-" + i, COMPONENT_OBS_NO = 1, FIELD_ID = "A", ACTIVE_IND = "Y",
            COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType
        });
        var primary = f.Rows.OfType<HSE_INCIDENT_COMPONENT>().Single(row => row.INCIDENT_ID == "local");
        if (scenario == "unrelated-malformed") f.Rows.OfType<HSE_INCIDENT_COMPONENT>().First(row => row.INCIDENT_ID.StartsWith("unrelated-", StringComparison.Ordinal)).INCIDENT_ID = "";
        if (scenario == "foreign-primary") primary.FIELD_ID = "B";
        if (scenario == "duplicate-primary") f.Rows.Add(new HSE_INCIDENT_COMPONENT
        {
            INCIDENT_ID = "local", COMPONENT_OBS_NO = 2, FIELD_ID = "B", ACTIVE_IND = "Y",
            COMPONENT_ROLE = HseReferenceCodes.FieldContext.PrimaryRole, COMPONENT_TYPE = HseReferenceCodes.FieldContext.ComponentType
        });
        if (scenario == "retired-field") f.Rows.OfType<FIELD>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
        if (scenario == "unreviewed-restriction") { primary.RESTRICTION_ID = "unreviewed"; primary.RESTRICTION_VERSION = 1; }
        if (scenario == "late-retirement") f.AfterRead = () =>
        {
            var last = f.Source.Invocations.LastOrDefault(call => call.Method.Name == "GetEntityAsync");
            if (last?.Arguments[0] as string == nameof(HSE_INCIDENT)) f.Rows.OfType<FIELD>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
            return Task.CompletedTask;
        };
        f.Source.Invocations.Clear(); f.Queries.Clear();
        if (scenario is "large-field" or "unrelated-malformed")
            Assert.True(ProcessDefinitionRevision.IsValid(await f.Policy.CaptureHseIncidentAuthorityAsync(review, "local")));
        else await Assert.ThrowsAsync<RefusalException>(() => f.Policy.CaptureHseIncidentAuthorityAsync(review, "local"));
        var caseReads = f.Source.Invocations.Where(call => call.Method.Name == "GetEntityAsync" && call.Arguments[0] as string == nameof(HSE_INCIDENT_COMPONENT)).ToArray();
        Assert.All(caseReads, call => Assert.Contains((List<AppFilter>)call.Arguments[1], filter => filter.FieldName == "INCIDENT_ID" && filter.Operator == "=" && filter.FilterValue == "local"));
        if (scenario is "large-field" or "unrelated-malformed") Assert.NotEmpty(caseReads);
        Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task ReportAuthorityUsesReportPermissionAndInheritedFieldWrite()
    {
        using var f = await Ready();
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" });
        var grant = await f.Db.Set<Beep.OilandGas.Models.Data.AccessControl.AppUserAssetAccess>().SingleAsync(row => row.AssetType == "FIELD" && row.AssetId == "A");
        grant.AccessLevel = "WRITE"; await f.Db.SaveChangesAsync();
        var profile = Profile(f); var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        var evidence = await f.Policy.CaptureHseReportAuthorityAsync(profile, review, "actor");
        Assert.False(string.IsNullOrWhiteSpace(evidence));
        Assert.Equal(evidence, await f.Policy.CaptureHseReportAuthorityAsync(profile, review, "actor"));
        grant.Inherit = false; await f.Db.SaveChangesAsync();
        await Assert.ThrowsAnyAsync<Exception>(() => f.Policy.CaptureHseReportAuthorityAsync(profile, review, "actor"));
        Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task ReportAuthorityRequiresReportPermissionEvenWithFieldWrite()
    {
        using var f = await Ready();
        var grant = await f.Db.Set<Beep.OilandGas.Models.Data.AccessControl.AppUserAssetAccess>().SingleAsync(row => row.AssetType == "FIELD" && row.AssetId == "A");
        grant.AccessLevel = "WRITE"; await f.Db.SaveChangesAsync();
        var profile = Profile(f); var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Policy.CaptureHseReportAuthorityAsync(profile, review, "actor"));
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("permission")]
    [InlineData("entitlement")]
    [InlineData("actor")]
    [InlineData("company")]
    public async Task ReportAuthorityRejectsChangedCompanyEvidence(string change)
    {
        using var f = await Ready();
        var claim = new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = "HSE.ReportIncident" };
        f.Db.RoleClaims.Add(claim);
        var grant = await f.Db.Set<Beep.OilandGas.Models.Data.AccessControl.AppUserAssetAccess>().SingleAsync(row => row.AssetType == "FIELD" && row.AssetId == "A");
        grant.AccessLevel = "WRITE"; await f.Db.SaveChangesAsync();
        var profile = Profile(f); var review = (await profile.GetCurrentFieldSelectionAsync()).SelectionContext!;
        Assert.False(string.IsNullOrEmpty(await f.Policy.CaptureHseReportAuthorityAsync(profile, review, "actor")));
        if (change == "permission") { f.Db.RoleClaims.Remove(claim); await f.Db.SaveChangesAsync(); }
        if (change == "entitlement") f.Rows.OfType<ENT_COMPONENT>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
        if (change == "company") await f.Select("other-company");
        await Assert.ThrowsAnyAsync<Exception>(() => f.Policy.CaptureHseReportAuthorityAsync(profile, review, change == "actor" ? "other" : "actor"));
        Assert.Empty(f.Writes);
    }
    private static void Crew(Fixture f, bool whole = false)
    {
        var row = Participant(f); row.COMPANY_BA_ID = "contractor"; row.UWI = "recorded-well"; row.PERIOD_OBS_NO = 17; row.DETAIL_OBS_NO = 23;
        if (whole) row.INVOLVED_PARTY_BA_ID = "contractor";
        f.Rows.AddRange([new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "contractor", ACTIVE_IND = "Y" },
            new WELL { UWI = "recorded-well", ASSIGNED_FIELD = "A", OPERATOR = "company", ACTIVE_IND = "Y" },
            new WELL_DRILL_PERIOD { UWI = "recorded-well", PERIOD_OBS_NO = 17, ACTIVE_IND = "Y" },
            new WELL_DRILL_PERIOD_CREW { UWI = "recorded-well", PERIOD_OBS_NO = 17, CREW_COMPANY_BA_ID = "contractor", DETAIL_OBS_NO = 23,
                CREW_MEMBER_RECORD_IND = whole ? "N" : "Y", CREW_RECORD_IND = whole ? "Y" : "N", CREW_MEMBER_BA_ID = whole ? "" : "recorded-investigator", ACTIVE_IND = "Y" }]);
    }
    [Theory] [InlineData("external")] [InlineData("member")] [InlineData("whole")]
    public async Task Participant_evidence_supports_recorded_external_companies_and_exact_member_or_whole_crews(string kind)
    {
        using var f = await Ready();
        if (kind == "external") { Participant(f).COMPANY_BA_ID = "contractor"; f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = "contractor", ACTIVE_IND = "Y" }); }
        else Crew(f, kind == "whole");
        Participant(f).INVOLVED_BA_STATUS = "ASSIGNED"; f.Rows.Add(new R_EMPLOYEE_STATUS { EMPLOYEE_STATUS = "ASSIGNED", ACTIVE_IND = "Y" });
        Assert.Same(Participant(f), Assert.Single(await f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new())));
        Assert.Equal(kind == "whole" ? "contractor" : "recorded-investigator", (await Incident(f, Profile(f)).GetByIdAsync("local"))!.InvestigatorId);
        Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("party-key")] [InlineData("role-key")] [InlineData("duplicate-key")] [InlineData("missing-person")] [InlineData("inactive-person")] [InlineData("expired-person")] [InlineData("duplicate-person")] [InlineData("missing-role")] [InlineData("unknown-role")] [InlineData("missing-status")] [InlineData("missing-company")] [InlineData("orphan-period")] [InlineData("orphan-detail")] [InlineData("non-UTC")]
    public async Task Participant_evidence_refuses_incomplete_ambiguous_or_unrecorded_identity(string fault)
    {
        using var f = await Ready(); var row = Participant(f); var person = f.Rows.OfType<BUSINESS_ASSOCIATE>().Single();
        switch (fault)
        {
            case "party-key": row.PARTY_OBS_NO = 1.5m; break;
            case "role-key": row.PARTY_ROLE_OBS_NO = -1; break;
            case "duplicate-key": f.Rows.Add(new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 1, PARTY_ROLE_OBS_NO = 1, ACTIVE_IND = "Y" }); break;
            case "missing-person": f.Rows.Remove(person); break;
            case "inactive-person": person.ACTIVE_IND = "N"; break;
            case "expired-person": person.EXPIRY_DATE = DateTime.UtcNow.AddDays(-1); break;
            case "duplicate-person": f.Rows.Add(new BUSINESS_ASSOCIATE { BUSINESS_ASSOCIATE_ID = person.BUSINESS_ASSOCIATE_ID, ACTIVE_IND = "Y" }); break;
            case "missing-role": f.Rows.RemoveAll(value => value is R_INCIDENT_BA_ROLE); break;
            case "unknown-role": row.INVOLVED_BA_ROLE = "UNRECORDED"; break;
            case "missing-status": row.INVOLVED_BA_STATUS = "ASSIGNED"; break;
            case "missing-company": row.COMPANY_BA_ID = "missing"; break;
            case "orphan-period": row.PERIOD_OBS_NO = 17; break;
            case "orphan-detail": row.DETAIL_OBS_NO = 23; break;
            case "non-UTC": row.EFFECTIVE_DATE = new(2020, 1, 1); break;
        }
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("period")] [InlineData("detail")] [InlineData("company")] [InlineData("member")] [InlineData("flags")] [InlineData("duplicate-crew")] [InlineData("inactive-crew")] [InlineData("missing-period")] [InlineData("duplicate-period")] [InlineData("expired-period")] [InlineData("missing-well")] [InlineData("foreign-field")] [InlineData("foreign-operator")]
    public async Task Participant_crew_evidence_cannot_use_partial_keys_or_promote_foreign_WELL_authority(string fault)
    {
        using var f = await Ready(); Crew(f); var crew = f.Rows.OfType<WELL_DRILL_PERIOD_CREW>().Single(); var period = f.Rows.OfType<WELL_DRILL_PERIOD>().Single(); var well = f.Rows.OfType<WELL>().Single();
        switch (fault)
        {
            case "period": crew.PERIOD_OBS_NO = 18; break;
            case "detail": crew.DETAIL_OBS_NO = 24; break;
            case "company": crew.CREW_COMPANY_BA_ID = "other"; break;
            case "member": crew.CREW_MEMBER_BA_ID = "other"; break;
            case "flags": crew.CREW_RECORD_IND = "Y"; break;
            case "duplicate-crew": f.Rows.Add(new WELL_DRILL_PERIOD_CREW { UWI = crew.UWI, PERIOD_OBS_NO = 17, CREW_COMPANY_BA_ID = crew.CREW_COMPANY_BA_ID, DETAIL_OBS_NO = 23, ACTIVE_IND = "Y" }); break;
            case "inactive-crew": crew.ACTIVE_IND = "N"; break;
            case "missing-period": f.Rows.Remove(period); break;
            case "duplicate-period": f.Rows.Add(new WELL_DRILL_PERIOD { UWI = period.UWI, PERIOD_OBS_NO = 17, ACTIVE_IND = "Y" }); break;
            case "expired-period": period.EXPIRY_DATE = DateTime.UtcNow.AddDays(-1); break;
            case "missing-well": f.Rows.Remove(well); break;
            case "foreign-field": well.ASSIGNED_FIELD = "B"; break;
            case "foreign-operator": well.OPERATOR = "other"; break;
        }
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("expired")] [InlineData("future")]
    public async Task Participant_noncurrent_roster_is_excluded_without_inventing_an_investigator(string window)
    {
        using var f = await Ready(); var row = Participant(f);
        if (window == "expired") row.EXPIRY_DATE = DateTime.UtcNow.AddDays(-1); else row.EFFECTIVE_DATE = DateTime.UtcNow.AddDays(1);
        Assert.Empty(await f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new()));
        Assert.Null((await Incident(f, Profile(f)).GetByIdAsync("local"))!.InvestigatorId); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Participant_actual_incident_owner_refuses_multiple_current_investigator_observations()
    {
        using var f = await Ready(); f.Rows.Add(new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 2, PARTY_ROLE_OBS_NO = 1, INVOLVED_BA_ROLE = "INVESTIGATOR", INVOLVED_PARTY_BA_ID = "recorded-investigator", ACTIVE_IND = "Y" });
        Assert.Equal(2, (await f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new())).Count());
        var error = await Assert.ThrowsAsync<RefusalException>(() => Incident(f, Profile(f)).GetByIdAsync("local")); Assert.Equal(RefusalKind.Conflict, error.Kind); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Participant_provider_cannot_substitute_payload_under_the_same_complete_key()
    {
        using var f = await Ready();
        f.Source.Setup(value => value.GetEntityAsync("HSE_INCIDENT_BA", It.Is<List<AppFilter>>(filters => filters.Any(filter => filter.FieldName == "PARTY_OBS_NO"))))
            .ReturnsAsync(new object[] { new HSE_INCIDENT_BA { INCIDENT_ID = "local", PARTY_OBS_NO = 1, PARTY_ROLE_OBS_NO = 1, INVOLVED_BA_ROLE = "INVESTIGATOR", INVOLVED_PARTY_BA_ID = "recorded-investigator", REMARK = "unrecorded payload", ACTIVE_IND = "Y" } });
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Participant_reference_await_cannot_adopt_an_aliased_roster_change()
    {
        using var f = await Ready(); var reads = 0;
        f.Source.Setup(value => value.GetEntityAsync("R_INCIDENT_BA_ROLE", It.IsAny<List<AppFilter>>())).Returns(() => {
            if (++reads == 2) Participant(f).REMARK = "changed during evidence read";
            return Task.FromResult<IEnumerable<object>>(f.Rows.OfType<R_INCIDENT_BA_ROLE>().Cast<object>().ToArray());
        });
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Participant_withheld_case_never_materializes_personal_roster()
    {
        using var f = await Ready(); Restrict(f); await ReviewPermissions(f); await Bind(f);
        var claim = await f.Db.RoleClaims.SingleAsync(value => value.ClaimValue == "HSE.ViewRestricted"); f.Db.RoleClaims.Remove(claim); await f.Db.SaveChangesAsync(); await f.Select("company");
        Assert.Empty(await f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new()));
        f.Source.Verify(value => value.GetEntityAsync("HSE_INCIDENT_BA", It.IsAny<List<AppFilter>>()), Times.Never); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("HSE_INCIDENT_BA")] [InlineData("WELL_DRILL_PERIOD_CREW")] [InlineData("WELL_DRILL_PERIOD")]
    public async Task Participant_metadata_must_preserve_the_complete_canonical_key(string table)
    {
        using var f = await Ready(); Crew(f);
        var definition = await f.Metadata.Object.GetTableMetadataAsync(table); definition.PrimaryKeyColumn = table == "HSE_INCIDENT_BA" ? "INCIDENT_ID" : "UWI";
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Participant_role_identity_is_exact_and_never_guessed_from_case_insensitive_text()
    {
        using var f = await Ready(); Participant(f).INVOLVED_BA_ROLE = "investigator";
        f.Rows.Add(new R_INCIDENT_BA_ROLE { BA_ROLE = "investigator", ACTIVE_IND = "Y" });
        Assert.Single(await f.Repository(typeof(HSE_INCIDENT_BA)).GetAsync(new()));
        Assert.Null((await Incident(f, Profile(f)).GetByIdAsync("local"))!.InvestigatorId); Assert.Empty(f.Writes);
    }
    [Theory] [MemberData(nameof(Tables))]
    public async Task Canonical_family_reads_only_active_incident_rows_with_inherited_eligible_FIELD_authority(string table)
    {
        using var f = await Ready(); var rows = (await f.Repository(Entity(table)).GetAsync(new())).ToArray();
        var row = Assert.Single(rows); Assert.Equal("local", row.GetType().GetProperty("INCIDENT_ID")!.GetValue(row));
        Assert.All(f.Connections, value => Assert.Equal("selected-tenant", value)); Assert.Empty(f.Writes);
        Assert.Equal("READ", await f.Policy.GetEffectiveAccessLevelAsync(Entity(table), table, row));
        Assert.Null(await f.Policy.GetEffectiveAccessLevelAsync(Entity(table), table, row, includeInherited: false));
    }
    [Theory] [MemberData(nameof(Tables))]
    public async Task FIELD_grants_without_explicit_inheritance_do_not_authorize_incident_children(string table)
    {
        using var f = await Ready(false); Assert.Empty(await f.Repository(Entity(table)).GetAsync(new()));
        f.Source.Verify(source => source.GetEntityAsync(table, It.IsAny<List<AppFilter>>()), Times.Never); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Actual_incident_and_RCA_owners_return_recorded_data_through_current_profile_and_canonical_policy()
    {
        using var f = await Ready(); var profile = Profile(f); var service = Incident(f, profile);
        var record = Assert.Single(await service.GetFieldIncidentsAsync("A", null));
        Assert.Equal("local", record.IncidentId); Assert.Equal("A", record.FieldId); Assert.Equal(2, record.Tier);
        Assert.Equal("recorded description", record.Description); Assert.Equal("Reported", record.CurrentState); Assert.Equal("recorded-investigator", record.InvestigatorId);
        Assert.Equal("local", (await service.GetByIdAsync("local"))!.IncidentId); Assert.Null(await service.GetByIdAsync("foreign"));
        var rca = new RCAService(f.Editor.Object, f.Columns.Object, f.Defaults.Object, f.Metadata.Object, profile, f.Policy, service, NullLogger<RCAService>.Instance);
        var cause = Assert.Single(await rca.GetCauseChainAsync("local")); Assert.Equal("recorded cause", cause.Description);
        await Assert.ThrowsAsync<RefusalException>(() => rca.GetCauseChainAsync("foreign")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("duplicate")] [InlineData("foreign-primary")] [InlineData("wrong-type")] [InlineData("expired")] [InlineData("future")] [InlineData("ambiguous-date")] [InlineData("inactive")] [InlineData("missing-role-reference")] [InlineData("duplicate-type-reference")] [InlineData("missing-header")]
    public async Task Missing_inactive_or_ambiguous_primary_evidence_cannot_authorize_an_incident(string reason)
    {
        using var f = await Ready(); var primary = f.Rows.OfType<HSE_INCIDENT_COMPONENT>().Single(row => row.INCIDENT_ID == "local");
        switch (reason)
        {
            case "duplicate": case "foreign-primary": f.Rows.Add(new HSE_INCIDENT_COMPONENT { INCIDENT_ID = "local", COMPONENT_OBS_NO = 2, FIELD_ID = reason == "duplicate" ? "A" : "B", COMPONENT_ROLE = primary.COMPONENT_ROLE, COMPONENT_TYPE = primary.COMPONENT_TYPE, ACTIVE_IND = "Y" }); break;
            case "wrong-type": primary.COMPONENT_TYPE = "unreviewed-type"; break;
            case "expired": primary.EXPIRY_DATE = DateTime.UtcNow.AddDays(-1); break;
            case "future": primary.EFFECTIVE_DATE = DateTime.UtcNow.AddDays(1); break;
            case "ambiguous-date": primary.EFFECTIVE_DATE = new(2020, 1, 1); break;
            case "inactive": primary.ACTIVE_IND = "N"; break;
            case "missing-role-reference": f.Rows.RemoveAll(row => row is R_HSE_COMP_ROLE); break;
            case "duplicate-type-reference": f.Rows.Add(new R_HSE_INCIDENT_COMP_TYPE { COMPONENT_TYPE = primary.COMPONENT_TYPE, ACTIVE_IND = "Y" }); break;
            case "missing-header": f.Rows.RemoveAll(row => row is HSE_INCIDENT header && header.INCIDENT_ID == "local"); break;
        }
        var repo = f.Repository(typeof(HSE_INCIDENT_DETAIL));
        if (reason is "expired" or "future" or "inactive" or "missing-header") Assert.Empty(await repo.GetAsync(new()));
        else await Assert.ThrowsAsync<RefusalException>(() => repo.GetAsync(new()));
        f.Source.Verify(source => source.GetEntityAsync("HSE_INCIDENT_DETAIL", It.IsAny<List<AppFilter>>()), Times.Never); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("reparent")] [InlineData("revoke")]
    public async Task Association_or_app_grant_changes_during_provider_read_refuse_returned_rows(string change)
    {
        using var f = await Ready();
        f.Source.Setup(source => source.GetEntityAsync("HSE_INCIDENT_DETAIL", It.IsAny<List<AppFilter>>())).Returns(async () =>
        {
            if (change == "reparent") f.Rows.OfType<HSE_INCIDENT_COMPONENT>().Single(row => row.INCIDENT_ID == "local").FIELD_ID = "B";
            else { (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false; await f.Db.SaveChangesAsync(); }
            return new object[] { f.Rows.OfType<HSE_INCIDENT_DETAIL>().Single(row => row.INCIDENT_ID == "local") };
        });
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Provider_ignoring_incident_predicates_cannot_return_foreign_children()
    {
        using var f = await Ready(); f.Source.Setup(source => source.GetEntityAsync("HSE_INCIDENT_DETAIL", It.IsAny<List<AppFilter>>())).ReturnsAsync(f.Rows.OfType<HSE_INCIDENT_DETAIL>().Cast<object>().ToArray());
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData(PPDMRowMutation.Insert)] [InlineData(PPDMRowMutation.Update)] [InlineData(PPDMRowMutation.Delete)]
    public async Task Incident_generic_writes_remain_unavailable_until_reviewed_commands_exist(PPDMRowMutation operation)
    {
        using var f = await Ready(); var row = f.Rows.OfType<HSE_INCIDENT_DETAIL>().Single(row => row.INCIDENT_ID == "local");
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ValidateMutationAsync(typeof(HSE_INCIDENT_DETAIL), "HSE_INCIDENT_DETAIL", row, operation == PPDMRowMutation.Insert ? null : row, operation, "actor")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("inactive-field")] [InlineData("missing-field")] [InlineData("foreign-entitlement")]
    public async Task FIELD_existence_and_current_company_entitlement_remain_prerequisites(string failure)
    {
        using var f = await Ready();
        if (failure == "inactive-field") f.Rows.OfType<FIELD>().Single(row => row.FIELD_ID == "A").ACTIVE_IND = "N";
        else if (failure == "missing-field") f.Rows.RemoveAll(row => row is FIELD field && field.FIELD_ID == "A");
        else f.Rows.OfType<ENT_COMPONENT>().Single().BUSINESS_ASSOCIATE_ID = "other-company";
        Assert.Empty(await f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new())); Assert.Empty(f.Writes);
        f.Source.Verify(source => source.GetEntityAsync("HSE_INCIDENT_DETAIL", It.IsAny<List<AppFilter>>()), Times.Never);
    }
    [Theory] [InlineData("key")] [InlineData("relationship")]
    public async Task Canonical_schema_drift_refuses_the_incident_relationship_before_payload_reads(string failure)
    {
        using var f = await Ready(); var definition = await f.Metadata.Object.GetTableMetadataAsync("HSE_INCIDENT_DETAIL");
        if (failure == "key") definition.PrimaryKeyColumn = "INCIDENT_ID";
        else definition.ForeignKeys.Single(key => key.ForeignKeyColumn == "INCIDENT_ID" && key.ReferencedTable == "HSE_INCIDENT").ReferencedPrimaryKey = "OTHER";
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new())); Assert.Empty(f.Writes);
        f.Source.Verify(source => source.GetEntityAsync("HSE_INCIDENT_DETAIL", It.IsAny<List<AppFilter>>()), Times.Never);
    }
    [Fact]
    public async Task Reported_Beep_provider_failure_does_not_become_incident_evidence()
    {
        using var f = await Ready(); var error = new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Ok };
        f.Source.SetupGet(source => source.ErrorObject).Returns(error);
        f.Source.Setup(source => source.GetEntityAsync("HSE_INCIDENT_DETAIL", It.IsAny<List<AppFilter>>())).ReturnsAsync(() =>
        { error.Flag = TheTechIdea.Beep.ConfigUtil.Errors.Failed; return f.Rows.OfType<HSE_INCIDENT_DETAIL>().Cast<object>().ToArray(); });
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("missing")] [InlineData("duplicate")]
    public async Task Recorded_primary_detail_must_be_unique_before_the_service_interprets_severity(string state)
    {
        using var f = await Ready(); var row = f.Rows.OfType<HSE_INCIDENT_DETAIL>().Single(value => value.INCIDENT_ID == "local");
        if (state == "missing") row.DETAIL_TYPE = "OTHER";
        else f.Rows.Add(new HSE_INCIDENT_DETAIL { INCIDENT_ID = "local", DETAIL_OBS_NO = 2, DETAIL_TYPE = "PRIMARY", INCIDENT_SEVERITY_ID = "TIER_4", ACTIVE_IND = "Y" });
        await Assert.ThrowsAsync<RefusalException>(() => Incident(f, Profile(f)).GetByIdAsync("local")); Assert.Empty(f.Writes);
    }
    internal static void Restrict(Fixture f, bool secondary = false)
    {
        var component = f.Rows.OfType<HSE_INCIDENT_COMPONENT>().Single(row => row.INCIDENT_ID == "local");
        if (secondary) { component = new HSE_INCIDENT_COMPONENT { INCIDENT_ID = "local", COMPONENT_OBS_NO = 2, ACTIVE_IND = "Y" }; f.Rows.Add(component); }
        component.RESTRICTION_ID = "recorded-rest"; component.RESTRICTION_VERSION = 7;
        f.Rows.AddRange([new RESTRICTION { RESTRICTION_ID = "recorded-rest", RESTRICTION_VERSION = 7, RESTRICTION_CLASS = "recorded-class", RESTRICTION_TYPE = "recorded-type", REST_DURATION_TYPE = "recorded-duration", EFFECTIVE_DATE = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), ACTIVE_IND = "Y" },
            new REST_CLASS { RESTRICTION_CLASS_ID = "recorded-class", ACTIVE_IND = "Y" }, new R_REST_TYPE { RESTRICTION_TYPE = "recorded-type", ACTIVE_IND = "Y" },
            new R_REST_DURATION { REST_DURATION_TYPE = "recorded-duration", ACTIVE_IND = "Y" }, new R_REST_ACTIVITY { RESTRICTED_ACTIVITY = "recorded-activity", ACTIVE_IND = "Y" },
            new REST_ACTIVITY { RESTRICTION_ID = "recorded-rest", RESTRICTION_VERSION = 7, RESTRICTED_ACTIVITY = "recorded-activity", ACTIVE_IND = "Y" },
            new REST_TEXT { RESTRICTION_ID = "recorded-rest", RESTRICTION_VERSION = 7, TEXT_SEQ_NO = 1, BODY_OF_TEXT = "Human-reviewed restriction; contact is not a grant", ACTIVE_IND = "Y" }]);
    }
    internal static async Task ReviewPermissions(Fixture f)
    {
        f.Db.RoleClaims.AddRange(new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ViewRestricted },
            new IdentityRoleClaim<string> { RoleId = "admin", ClaimType = "permission", ClaimValue = Beep.OilandGas.Models.Data.Security.PermissionConstants.HSE.ManageIncidents });
        await f.Db.SaveChangesAsync();
    }
    internal static async Task<HseRestrictionReview> Bind(Fixture f, string permission = "HSE.ViewRestricted")
    {
        var review = (await Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        var result = await f.Policy.ReviewHseRestrictionsAsync(review, "local");
        var definition = Assert.Single(result.Restrictions);
        f.EligibilityConfiguration.Companies.Single().HseCaseReadRestrictions.Add(new() { DatabaseFingerprint = result.Database.Fingerprint,
            RestrictionId = definition.RestrictionId, RestrictionVersion = definition.RestrictionVersion, DefinitionFingerprint = definition.DefinitionFingerprint, RequiredPermission = permission });
        return result;
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Restricted_case_review_and_explicit_binding_authorize_actual_incident_and_complete_population(bool secondary)
    {
        using var f = await Ready(); Restrict(f, secondary); await ReviewPermissions(f);
        var review = await Bind(f); Assert.Equal("A", review.FieldId); Assert.Equal(64, review.Restrictions[0].DefinitionFingerprint.Length);
        Assert.Equal("local", Assert.Single(await Incident(f, Profile(f)).GetFieldIncidentsAsync("A", null)).IncidentId);
        await f.Policy.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", "A"); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Explicit_normal_read_binding_remains_company_reviewed_and_needs_no_restricted_permission()
    {
        using var f = await Ready(); Restrict(f); await ReviewPermissions(f); await Bind(f, "HSE.View");
        f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == "HSE.ViewRestricted").ToArrayAsync()); await f.Db.SaveChangesAsync(); await f.Select("company");
        Assert.Equal(f.EligibilityConfiguration.Companies.Single().HseCaseReadRestrictions[0].DatabaseFingerprint, (await f.Gate.ResolveAsync()).Fingerprint);
        Assert.Equal("DELETE", await f.Eligibility.GetMaximumLevelAsync("A"));
        Assert.True((await f.Db.Set<AppUserAssetAccess>().SingleAsync()).Inherit);
        Assert.Single(await f.Policy.GetReadConjunctionsAsync(typeof(HSE_INCIDENT_DETAIL), "HSE_INCIDENT_DETAIL"));
        Assert.Single(await f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("unmapped")] [InlineData("database")] [InlineData("version")] [InlineData("fingerprint")] [InlineData("permission")] [InlineData("duplicate")] [InlineData("null-rule")]
    public async Task Restricted_case_missing_stale_or_ambiguous_review_refuses_before_child_payload(string fault)
    {
        using var f = await Ready(); Restrict(f); await ReviewPermissions(f);
        if (fault != "unmapped") await Bind(f);
        var rules = f.EligibilityConfiguration.Companies.Single().HseCaseReadRestrictions;
        if (fault != "unmapped") switch (fault) {
            case "database": rules[0].DatabaseFingerprint = "another-database"; break;
            case "version": rules[0].RestrictionVersion = 6; break;
            case "fingerprint": rules[0].DefinitionFingerprint = "stale"; break;
            case "permission": rules[0].RequiredPermission = "Administrator"; break;
            case "duplicate": rules.Add(rules[0]); break;
            case "null-rule": rules.Add(null!); break;
        }
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new()));
        f.Source.Verify(source => source.GetEntityAsync("HSE_INCIDENT_DETAIL", It.IsAny<List<AppFilter>>()), Times.Never); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("text")] [InlineData("activity")] [InlineData("class")] [InlineData("header")] [InlineData("expiry")] [InlineData("event-date")] [InlineData("orphan-version")] [InlineData("duplicate-component")]
    public async Task Recorded_definition_or_association_changes_invalidate_restricted_case_binding(string change)
    {
        using var f = await Ready(); Restrict(f, true); await ReviewPermissions(f); await Bind(f);
        switch (change) {
            case "text": f.Rows.OfType<REST_TEXT>().Single().BODY_OF_TEXT = "changed evidence"; break;
            case "activity": f.Rows.OfType<R_REST_ACTIVITY>().Single().REMARK = "changed reference"; break;
            case "class": f.Rows.OfType<REST_CLASS>().Single().REMARK = "changed classification"; break;
            case "header": f.Rows.OfType<RESTRICTION>().Single().SOURCE_DOCUMENT_ID = "changed-source"; break;
            case "expiry": f.Rows.OfType<RESTRICTION>().Single().EXPIRY_DATE = DateTime.UtcNow.AddDays(-1); break;
            case "event-date": f.Rows.OfType<RESTRICTION>().Single().START_DATE_EVENT = DateTime.UtcNow.AddDays(-1); break;
            case "orphan-version": f.Rows.OfType<HSE_INCIDENT_COMPONENT>().Single(row => row.INCIDENT_ID == "local" && row.COMPONENT_OBS_NO == 2).RESTRICTION_ID = null!; break;
            case "duplicate-component": f.Rows.Add(new HSE_INCIDENT_COMPONENT { INCIDENT_ID = "local", COMPONENT_OBS_NO = 2, ACTIVE_IND = "Y" }); break;
        }
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new())); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Current_app_permission_withdrawal_withholds_case_and_refuses_incomplete_FIELD_totals()
    {
        using var f = await Ready(); Restrict(f); await ReviewPermissions(f); await Bind(f);
        f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == "HSE.ViewRestricted").ToArrayAsync()); await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new()));
        await f.Select("company");
        Assert.Empty(await f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new()));
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", "A"));
        f.Source.Verify(source => source.GetEntityAsync("HSE_INCIDENT_DETAIL", It.IsAny<List<AppFilter>>()), Times.Never); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("foreign-field")] [InlineData("missing-permissions")] [InlineData("foreign-actor")] [InlineData("foreign-database")]
    public async Task Metadata_review_requires_current_app_reviewer_and_exact_reviewed_FIELD_scope(string fault)
    {
        using var f = await Ready(); Restrict(f); if (fault != "missing-permissions") await ReviewPermissions(f);
        var review = (await Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        if (fault == "foreign-field") review = review with { FieldId = "B" };
        if (fault == "foreign-actor") review = review with { ActorUserId = "other" };
        if (fault == "foreign-database") review = review with { Database = review.Database with { Fingerprint = "other" } };
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ReviewHseRestrictionsAsync(review, "local")); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Complete_population_proof_requires_the_exact_requested_FIELD_grant()
    {
        using var f = await Ready(); await f.Policy.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", "A");
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", "B")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("definition")] [InlineData("configuration")] [InlineData("permission")]
    public async Task Restricted_case_drift_during_payload_read_refuses_delivery(string change)
    {
        using var f = await Ready(); Restrict(f); await ReviewPermissions(f); await Bind(f);
        f.Source.Setup(source => source.GetEntityAsync("HSE_INCIDENT_DETAIL", It.IsAny<List<AppFilter>>())).Returns(async () => {
            if (change == "definition") f.Rows.OfType<REST_TEXT>().Single().BODY_OF_TEXT = "changed";
            if (change == "configuration") f.EligibilityConfiguration.Companies.Single().HseCaseReadRestrictions.Clear();
            if (change == "permission") { f.Db.RoleClaims.RemoveRange(await f.Db.RoleClaims.Where(row => row.ClaimValue == "HSE.ViewRestricted").ToArrayAsync()); await f.Db.SaveChangesAsync(); }
            return new object[] { f.Rows.OfType<HSE_INCIDENT_DETAIL>().Single(row => row.INCIDENT_ID == "local") };
        });
        await Assert.ThrowsAsync<RefusalException>(() => f.Repository(typeof(HSE_INCIDENT_DETAIL)).GetAsync(new())); Assert.Empty(f.Writes);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task Restriction_access_uses_the_inspected_subjects_current_app_permission(bool restrictedPermission)
    {
        using var f = await Ready(); Restrict(f); await ReviewPermissions(f); await Bind(f);
        f.Db.Users.Add(new OilGasUser { Id = "reader", UserName = "reader", IsActive = true });
        f.Db.Roles.Add(new IdentityRole { Id = "reader-role", Name = "Reader", NormalizedName = "READER" });
        f.Db.UserRoles.Add(new IdentityUserRole<string> { UserId = "reader", RoleId = "reader-role" });
        f.Db.Add(new AppCompanyMembership { UserId = "reader", TenantId = "tenant", BusinessAssociateId = "company", GrantedByUserId = "actor" });
        f.Db.Add(new AppUserRoleExtension { UserId = "reader", RoleId = "reader-role", TenantId = "tenant", BusinessAssociateId = "company" });
        f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "reader-role", ClaimType = "permission", ClaimValue = "HSE.View" });
        if (restrictedPermission) f.Db.RoleClaims.Add(new IdentityRoleClaim<string> { RoleId = "reader-role", ClaimType = "permission", ClaimValue = "HSE.ViewRestricted" });
        var database = await f.Gate.ResolveAsync(); f.Db.Add(new AppUserAssetAccess { Id = "reader-field", UserId = "reader", OrganizationId = "company", AssetType = "FIELD", AssetId = "A", DatabaseScope = database.Fingerprint, AccessLevel = "READ", Inherit = true, IsActive = true }); await f.Db.SaveChangesAsync();
        var policy = new CompanyPPDMRowAccessPolicy(f.Gate, new RepositoryAssetAccessStore(f.Db, f.Gate, Mock.Of<IAssetGrantTargetVerifier>()), f.Metadata.Object, f.Eligibility, f.Editor.Object, f.Columns.Object, f.Defaults.Object, Microsoft.Extensions.Options.Options.Create(f.EligibilityConfiguration), "reader");
        var predicates = await policy.GetReadConjunctionsAsync(typeof(HSE_INCIDENT_DETAIL), "HSE_INCIDENT_DETAIL");
        if (restrictedPermission) Assert.Single(predicates); else Assert.Empty(predicates);
        var review = (await Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        await Assert.ThrowsAsync<RefusalException>(() => policy.ReviewHseRestrictionsAsync(review, "local")); Assert.Empty(f.Writes);
    }
    [Theory] [InlineData("field")] [InlineData("header")] [InlineData("restriction-fk")]
    public async Task Complete_population_cannot_turn_missing_targets_or_canonical_relationships_into_zero_cases(string missing)
    {
        using var f = await Ready();
        if (missing == "field") f.Rows.RemoveAll(row => row is FIELD field && field.FIELD_ID == "A");
        if (missing == "header") f.Rows.RemoveAll(row => row is HSE_INCIDENT incident && incident.INCIDENT_ID == "local");
        if (missing == "restriction-fk") (await f.Metadata.Object.GetTableMetadataAsync("HSE_INCIDENT_COMPONENT")).ForeignKeys.Single(key => key.ForeignKeyColumn == "RESTRICTION_ID").ReferencedPrimaryKey = "OTHER";
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", "A")); Assert.Empty(f.Writes);
    }

    [Theory] [InlineData("header-key")] [InlineData("wrong-version")] [InlineData("UTC")] [InlineData("missing-class")] [InlineData("duplicate-activity")]
    public async Task Restriction_review_requires_canonical_full_identity_current_UTC_and_unique_reference_evidence(string failure)
    {
        using var f = await Ready(); Restrict(f); await ReviewPermissions(f);
        switch (failure) {
            case "header-key": (await f.Metadata.Object.GetTableMetadataAsync("RESTRICTION")).PrimaryKeyColumn = "RESTRICTION_ID"; break;
            case "wrong-version": f.Source.Setup(source => source.GetEntityAsync("RESTRICTION", It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[] { new RESTRICTION { RESTRICTION_ID = "recorded-rest", RESTRICTION_VERSION = 6, ACTIVE_IND = "Y" } }); break;
            case "UTC": f.Rows.OfType<RESTRICTION>().Single().EFFECTIVE_DATE = new(2025, 1, 1); break;
            case "missing-class": f.Rows.RemoveAll(row => row is REST_CLASS); break;
            case "duplicate-activity": f.Rows.Add(new REST_ACTIVITY { RESTRICTION_ID = "recorded-rest", RESTRICTION_VERSION = 7, RESTRICTED_ACTIVITY = "recorded-activity", ACTIVE_IND = "Y" }); break;
        }
        var review = (await Profile(f).GetCurrentFieldSelectionAsync()).SelectionContext!;
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.ReviewHseRestrictionsAsync(review, "local")); Assert.Empty(f.Writes);
    }
    [Fact]
    public async Task Complete_population_refuses_a_new_case_added_after_its_initial_FIELD_inventory()
    {
        using var f = await Ready(); var changed = false;
        f.Source.Setup(source => source.GetEntityAsync("HSE_INCIDENT", It.IsAny<List<AppFilter>>())).Returns((string _, List<AppFilter> filters) => {
            var result = f.Rows.OfType<HSE_INCIDENT>().Where(row => filters.All(filter => Convert.ToString(row.GetType().GetProperty(filter.FieldName)!.GetValue(row)) == filter.FilterValue)).Cast<object>().ToArray();
            if (!changed) { changed = true; Seed(f, "new-case", "A"); }
            return Task.FromResult<IEnumerable<object>>(result);
        });
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", "A")); Assert.True(changed); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Complete_population_checks_live_grants_after_its_final_provider_inventory()
    {
        using var f = await Ready(); var inventoryReads = 0;
        f.AfterRead = async () => {
            if (f.Queries.Last().Any(filter => filter.FieldName == "FIELD_ID") && f.Queries.Last().Any(filter => filter.FieldName == "COMPONENT_ROLE") && ++inventoryReads == 2)
            { (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false; await f.Db.SaveChangesAsync(); }
        };
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", "A")); Assert.Equal(2, inventoryReads); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Complete_population_refuses_a_restriction_added_to_an_existing_case_during_final_inventory()
    {
        using var f = await Ready(); var inventoryReads = 0;
        f.AfterRead = () => {
            if (f.Queries.Last().Any(filter => filter.FieldName == "FIELD_ID") && f.Queries.Last().Any(filter => filter.FieldName == "COMPONENT_ROLE") && ++inventoryReads == 2)
            { var component = f.Rows.OfType<HSE_INCIDENT_COMPONENT>().Single(row => row.INCIDENT_ID == "local"); component.RESTRICTION_ID = "new-unreviewed-restriction"; component.RESTRICTION_VERSION = 4; }
            return Task.CompletedTask;
        };
        await Assert.ThrowsAsync<RefusalException>(() => f.Policy.RequireCompleteReadScopeAsync(typeof(HSE_INCIDENT_COMPONENT), "HSE_INCIDENT_COMPONENT", "A")); Assert.Equal(2, inventoryReads); Assert.Empty(f.Writes);
    }

}
