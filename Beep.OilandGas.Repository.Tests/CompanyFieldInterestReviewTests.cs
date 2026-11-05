using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.ApiService.Controllers.AccessControl;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Client.Exceptions;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
using Fixture = Beep.OilandGas.Repository.Tests.CompanyPPDMRowAccessTests.Fixture;
using AccessApi = Beep.OilandGas.Client.App.Services.AccessControl.AccessControlService;

namespace Beep.OilandGas.Repository.Tests;

public sealed class CompanyFieldInterestReviewTests
{
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);
    private sealed class Clock : TimeProvider
    {
        public DateTime Instant { get; set; } = Now;
        public override DateTimeOffset GetUtcNow() => new(Instant);
    }
    private static PPDMCompanyFieldInterestReview Review(Fixture f, Clock? clock = null) => new(f.Gate, f.Editor.Object,
        f.Columns.Object, f.Defaults.Object, f.Metadata.Object, f.Policy, Options.Create(f.EligibilityConfiguration), clock ?? new Clock());
    private static INT_SET_COMPONENT Component(Fixture f) => f.Rows.OfType<INT_SET_COMPONENT>().Single();
    private static INTEREST_SET Set(Fixture f) => f.Rows.OfType<INTEREST_SET>().Single();
    private static INT_SET_PARTNER Partner(Fixture f) => f.Rows.OfType<INT_SET_PARTNER>().Single();
    internal static async Task<Fixture> Ready()
    {
        var f = await Fixture.Create(); await f.Grant("FIELD", "A"); f.Rows.Add(new FIELD { FIELD_ID = "A", ACTIVE_IND = "Y" });
        var rules = f.EligibilityConfiguration.Companies.Single(); rules.InterestReviewSetType = "test-jv-review"; rules.InterestReviewComponentType = "test-field-interest";
        f.Rows.AddRange([new R_INT_SET_TYPE { INTEREST_SET_TYPE = "test-jv-review", ACTIVE_IND = "Y" },
            new R_INT_SET_COMPONENT { COMPONENT_TYPE = "test-field-interest", ACTIVE_IND = "Y" },
            new R_INT_SET_ROLE { INTEREST_SET_ROLE = "test-recorded-owner", ACTIVE_IND = "Y" },
            new INTEREST_SET { INTEREST_SET_ID = "set", INTEREST_SET_SEQ_NO = 1m, INTEREST_SET_TYPE = "test-jv-review", ACTIVE_IND = "Y", EFFECTIVE_DATE = Now.AddYears(-1) },
            new INT_SET_COMPONENT { INTEREST_SET_ID = "set", INTEREST_SET_SEQ_NO = 1m, COMPONENT_OBS_NO = 7,
                FIELD_ID = "A", COMPONENT_TYPE = "test-field-interest", ACTIVE_IND = "Y", EFFECTIVE_DATE = Now.AddYears(-1) },
            new INT_SET_PARTNER { INTEREST_SET_ID = "set", INTEREST_SET_SEQ_NO = 1m, PARTNER_BA_ID = "company", PARTNER_OBS_NO = 2,
                INTEREST_SET_ROLE = "test-recorded-owner", GROSS_PERCENT_INTEREST = 37.5m, NET_PERCENT_INTEREST = 34.125m,
                ACTIVE_IND = "Y", EFFECTIVE_DATE = Now.AddYears(-1) }]);
        return f;
    }

    [Fact]
    public async Task Review_preserves_complete_canonical_keys_and_recorded_percentages_without_cross_company_partners_or_writes()
    {
        using var f = await Ready();
        f.Rows.Add(new INT_SET_PARTNER { INTEREST_SET_ID = "set", INTEREST_SET_SEQ_NO = 1, PARTNER_BA_ID = "other", PARTNER_OBS_NO = 2, ACTIVE_IND = "Y" });
        f.Rows.Add(new INTEREST_SET { INTEREST_SET_ID = "set", INTEREST_SET_SEQ_NO = 2, ACTIVE_IND = "Y", INTEREST_SET_TYPE = "test-jv-review" });
        var result = await Review(f).ReadAsync("A"); var set = Assert.Single(result.Sets);
        Assert.Equal(f.Gate.Company.Scope, result.Company); Assert.Equal((await f.Gate.ResolveAsync()).Fingerprint, result.DatabaseScope);
        Assert.Equal(Now, result.ReviewedUtc); Assert.Equal(1m, set.InterestSet.INTEREST_SET_SEQ_NO);
        Assert.Equal(7m, Assert.Single(set.Components).COMPONENT_OBS_NO); var partner = Assert.Single(set.CompanyPartners);
        Assert.Equal("company", partner.PARTNER_BA_ID); Assert.Equal(2m, partner.PARTNER_OBS_NO);
        Assert.Equal(37.5m, partner.GROSS_PERCENT_INTEREST); Assert.Equal(34.125m, partner.NET_PERCENT_INTEREST);
        var partnerQuery = Assert.Single(f.InterestQueries, query => query.Table == "INT_SET_PARTNER");
        Assert.Contains(partnerQuery.Filters, filter => filter.FieldName == "INTEREST_SET_SEQ_NO" && filter.FilterValue == "1");
        Assert.Contains(partnerQuery.Filters, filter => filter.FieldName == "PARTNER_BA_ID" && filter.FilterValue == "company");
        Assert.Empty(f.Writes); Assert.Equal("READ", (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).AccessLevel);
    }

    [Theory]
    [InlineData("missing-component")] [InlineData("inactive-component")] [InlineData("foreign-field")]
    [InlineData("wrong-component-type")] [InlineData("expired-component")] [InlineData("missing-component-date")]
    [InlineData("wrong-set-sequence")] [InlineData("inactive-set")] [InlineData("missing-set")] [InlineData("wrong-set-type")]
    [InlineData("expired-set")] [InlineData("missing-set-date")] [InlineData("foreign-partner")] [InlineData("wrong-partner-sequence")]
    [InlineData("inactive-partner")] [InlineData("expired-partner")] [InlineData("inactive-date")] [InlineData("missing-partner-date")]
    public async Task Noncurrent_unlinked_or_foreign_evidence_returns_an_explicit_empty_review(string fault)
    {
        using var f = await Ready(); var component = Component(f); var set = Set(f); var partner = Partner(f);
        switch (fault)
        {
            case "missing-component": f.Rows.Remove(component); break;
            case "inactive-component": component.ACTIVE_IND = "N"; break;
            case "foreign-field": component.FIELD_ID = "B"; break;
            case "wrong-component-type": component.COMPONENT_TYPE = "other"; break;
            case "expired-component": component.EXPIRY_DATE = Now; break;
            case "missing-component-date": component.EFFECTIVE_DATE = null; break;
            case "wrong-set-sequence": set.INTEREST_SET_SEQ_NO = 2; break;
            case "inactive-set": set.ACTIVE_IND = "N"; break;
            case "missing-set": f.Rows.Remove(set); break;
            case "wrong-set-type": set.INTEREST_SET_TYPE = "other"; break;
            case "expired-set": set.EXPIRY_DATE = Now; break;
            case "missing-set-date": set.EFFECTIVE_DATE = null; break;
            case "foreign-partner": partner.PARTNER_BA_ID = "other"; break;
            case "wrong-partner-sequence": partner.INTEREST_SET_SEQ_NO = 2; break;
            case "inactive-partner": partner.ACTIVE_IND = "N"; break;
            case "expired-partner": partner.EXPIRY_DATE = Now; break;
            case "inactive-date": partner.INACTIVE_DATE = Now; break;
            case "missing-partner-date": partner.EFFECTIVE_DATE = null; break;
        }
        Assert.Empty((await Review(f).ReadAsync("A")).Sets); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("administrator")] [InlineData("membership")] [InlineData("app-grant")] [InlineData("data-entitlement")] [InlineData("inactive-field")]
    public async Task Current_company_Administrator_and_real_FIELD_access_are_both_required(string fault)
    {
        using var f = await Ready();
        if (fault == "administrator") f.Db.UserRoles.Remove(await f.Db.UserRoles.SingleAsync());
        if (fault == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
        if (fault == "app-grant") (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false;
        if (fault == "data-entitlement") f.Rows.OfType<ENT_COMPONENT>().Single().ACTIVE_IND = "N";
        if (fault == "inactive-field") f.Rows.OfType<FIELD>().Single().ACTIVE_IND = "N";
        await f.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<RefusalException>(() => Review(f).ReadAsync("A")); Assert.Empty(f.InterestQueries); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("duplicate-component")] [InlineData("duplicate-set")] [InlineData("duplicate-partner")]
    [InlineData("fractional-sequence")] [InlineData("negative-observation")] [InlineData("missing-role")]
    [InlineData("missing-reference")] [InlineData("inactive-reference")] [InlineData("missing-config")] [InlineData("ambiguous-date")]
    public async Task Ambiguous_incomplete_or_unreviewed_evidence_is_a_conflict(string fault)
    {
        using var f = await Ready();
        switch (fault)
        {
            case "duplicate-component": f.Rows.Add(Component(f)); break;
            case "duplicate-set": f.Rows.Add(Set(f)); break;
            case "duplicate-partner": f.Rows.Add(Partner(f)); break;
            case "fractional-sequence": Component(f).INTEREST_SET_SEQ_NO = 1.5m; break;
            case "negative-observation": Partner(f).PARTNER_OBS_NO = -1; break;
            case "missing-role": Partner(f).INTEREST_SET_ROLE = ""; break;
            case "missing-reference": f.Rows.RemoveAll(row => row is R_INT_SET_ROLE); break;
            case "inactive-reference": f.Rows.OfType<R_INT_SET_TYPE>().Single().ACTIVE_IND = "N"; break;
            case "missing-config": f.EligibilityConfiguration.Companies.Single().InterestReviewSetType = ""; break;
            case "ambiguous-date": f.EligibilityConfiguration.Companies.Single().TimeZoneId = "America/New_York"; Partner(f).EFFECTIVE_DATE = new DateTime(2025, 11, 2, 1, 30, 0); break;
        }
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Review(f).ReadAsync("A"))).Kind); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("INT_SET_COMPONENT")] [InlineData("INTEREST_SET")] [InlineData("INT_SET_PARTNER")]
    public async Task Composite_key_metadata_drift_refuses_review_before_provider_access(string table)
    {
        using var f = await Ready(); (await f.Metadata.Object.GetTableMetadataAsync(table)).PrimaryKeyColumn = "INTEREST_SET_ID";
        await Assert.ThrowsAsync<RefusalException>(() => Review(f).ReadAsync("A")); Assert.Empty(f.InterestQueries); Assert.Empty(f.Connections);
    }

    [Theory]
    [InlineData("component-field")] [InlineData("set-sequence")] [InlineData("partner-company")]
    public async Task Provider_filter_bypass_cannot_cross_field_set_sequence_or_company(string fault)
    {
        using var f = await Ready(); f.IgnoreInterestFilters = true;
        if (fault == "component-field") Component(f).FIELD_ID = "B";
        if (fault == "set-sequence") Set(f).INTEREST_SET_SEQ_NO = 2;
        if (fault == "partner-company") Partner(f).PARTNER_BA_ID = "other";
        await Assert.ThrowsAsync<RefusalException>(() => Review(f).ReadAsync("A")); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("app-grant")] [InlineData("data-entitlement")] [InlineData("membership")] [InlineData("binding")] [InlineData("expiry")]
    public async Task Changes_during_evidence_reads_refuse_the_accumulated_review(string fault)
    {
        using var f = await Ready(); var clock = new Clock(); Partner(f).EXPIRY_DATE = Now.AddSeconds(1);
        f.AfterInterestRead = async () => {
            if (f.InterestQueries.Last().Table != "R_INT_SET_ROLE") return; f.AfterInterestRead = null;
            if (fault == "app-grant") (await f.Db.Set<AppUserAssetAccess>().SingleAsync()).IsActive = false;
            if (fault == "data-entitlement") f.Rows.OfType<ENT_COMPONENT>().Single().ACTIVE_IND = "N";
            if (fault == "membership") (await f.Db.Set<AppCompanyMembership>().SingleAsync()).EffectiveToUtc = DateTime.UtcNow.AddSeconds(-1);
            if (fault == "binding") (await f.Db.ModuleDatabases.SingleAsync()).ConcurrencyStamp = "changed";
            if (fault == "expiry") clock.Instant = Now.AddSeconds(1);
            await f.Db.SaveChangesAsync();
        };
        await Assert.ThrowsAsync<RefusalException>(() => Review(f, clock).ReadAsync("A")); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task Decimal_key_scale_is_not_a_different_interest_set_identity()
    {
        using var f = await Ready(); Set(f).INTEREST_SET_SEQ_NO = 1.000m; Partner(f).INTEREST_SET_SEQ_NO = 1.00m;
        Assert.Equal(1m, Assert.Single((await Review(f).ReadAsync("A")).Sets).InterestSet.INTEREST_SET_SEQ_NO);
    }

    [Fact]
    public async Task A_provider_error_after_a_scoped_interest_read_is_not_an_empty_or_verified_review()
    {
        using var f = await Ready(); f.AfterInterestRead = () => {
            f.Source.SetupGet(source => source.ErrorObject).Returns(new TheTechIdea.Beep.ConfigUtil.ErrorsInfo { Flag = TheTechIdea.Beep.ConfigUtil.Errors.Failed });
            return Task.CompletedTask;
        };
        await Assert.ThrowsAsync<InvalidOperationException>(() => Review(f).ReadAsync("A")); Assert.Empty(f.Writes);
    }

    [Fact]
    public async Task The_response_limit_refuses_large_partner_evidence_without_truncating_the_review()
    {
        using var f = await Ready();
        for (var index = 3; index <= 256; index++) f.Rows.Add(new INT_SET_PARTNER { INTEREST_SET_ID = "set", INTEREST_SET_SEQ_NO = 1,
            PARTNER_BA_ID = "company", PARTNER_OBS_NO = index, INTEREST_SET_ROLE = "test-recorded-owner", ACTIVE_IND = "Y", EFFECTIVE_DATE = Now.AddYears(-1) });
        Assert.Equal(RefusalKind.Conflict, (await Assert.ThrowsAsync<RefusalException>(() => Review(f).ReadAsync("A"))).Kind); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("observation")] [InlineData("gross-percent")] [InlineData("net-percent")]
    public async Task Raw_partner_records_require_numeric_key_and_percentage_presence_before_conversion(string missing)
    {
        using var f = await Ready(); var partner = Partner(f);
        f.Source.Setup(source => source.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()))
            .ReturnsAsync((string table, List<TheTechIdea.Beep.Report.AppFilter> filters) => {
                if (table != "INT_SET_PARTNER") return (IEnumerable<object>)f.Rows.Where(row => row.GetType().Name == table && filters.All(filter =>
                    Convert.ToString(row.GetType().GetProperty(filter.FieldName)!.GetValue(row)) == filter.FilterValue)).ToArray();
                var values = typeof(INT_SET_PARTNER).GetProperties().Where(property => property.CanRead && property.GetIndexParameters().Length == 0)
                    .ToDictionary(property => property.Name, property => property.GetValue(partner)!);
                values.Remove(missing switch { "observation" => "PARTNER_OBS_NO", "gross-percent" => "GROSS_PERCENT_INTEREST", _ => "NET_PERCENT_INTEREST" });
                return (IEnumerable<object>)[values];
            });
        await Assert.ThrowsAsync<InvalidOperationException>(() => Review(f).ReadAsync("A")); Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("partner")] [InlineData("reference")]
    public async Task Evidence_marked_inactive_while_final_FIELD_access_is_rechecked_is_refused(string target)
    {
        using var f = await Ready(); f.AfterEligibilityRead = () => {
            if (f.InterestQueries.Any(query => query.Table == "INT_SET_PARTNER")) {
                if (target == "partner") Partner(f).ACTIVE_IND = "N"; else f.Rows.OfType<R_INT_SET_ROLE>().Single().ACTIVE_IND = "N";
            }
            return Task.CompletedTask;
        };
        await Assert.ThrowsAsync<RefusalException>(() => Review(f).ReadAsync("A"));
    }

    [Theory]
    [InlineData("empty")] [InlineData("full")]
    public async Task Actual_controller_facade_SDK_and_shared_JSON_preserve_the_scoped_review(string contents)
    {
        using var f = await Ready(); if (contents == "empty") f.Rows.RemoveAll(row => row is INT_SET_PARTNER);
        ((ClaimsIdentity)f.Request.User.Identity!).AddClaim(new(ClaimTypes.Role, "Administrator"));
        var controller = new AccessControlController(CompanyAssetFacadeTests.Service(f), principal => principal.FindFirst("party_id")?.Value)
            { ControllerContext = new() { HttpContext = f.Request } };
        using var company = new CompanyClientContext(); var generation = company.Clear(); Assert.True(company.TrySelect(generation, "account", new(f.Gate.Company.Scope, ["Administrator"], [])));
        using var http = new HttpClient(new Handler(async request => {
            Assert.Equal("/api/AccessControl/fields/A/interest-review", request.RequestUri!.AbsolutePath);
            Assert.Equal("company", request.Headers.GetValues(CompanyRequestHeaders.Company).Single());
            var result = Assert.IsType<OkObjectResult>((await controller.GetFieldInterestReview("A")).Result);
            return Json(result.Value!);
        }));
        var sdk = new AccessApi(new ClientApiTransport(http, new() { ApiBaseUrl = "https://api.test" }, company));
        var result = await sdk.GetFieldInterestReviewAsync("A"); Assert.Equal(contents == "empty" ? 0 : 1, result.Sets.Count);
        if (contents == "full") Assert.Equal(37.5m, result.Sets[0].CompanyPartners[0].GROSS_PERCENT_INTEREST);
        Assert.Empty(f.Writes);
    }

    [Theory]
    [InlineData("company")] [InlineData("field")] [InlineData("database")] [InlineData("missing-sequence")]
    [InlineData("missing-percent")] [InlineData("foreign-partner")] [InlineData("duplicate-key")]
    [InlineData("missing-date-zone")] [InlineData("padded-date-zone")] [InlineData("default-timestamp")] [InlineData("missing-type")]
    public async Task SDK_refuses_malformed_scope_or_defaulted_numeric_evidence(string fault)
    {
        using var f = await Ready(); var review = await Review(f).ReadAsync("A");
        var wire = JsonNode.Parse(JsonSerializer.Serialize(review))!; var set = wire["Sets"]![0]!;
        switch (fault)
        {
            case "company": wire["Company"]!["BusinessAssociateId"] = "other"; break;
            case "field": wire["FieldId"] = "B"; break;
            case "database": wire["DatabaseScope"] = "connection-alias"; break;
            case "missing-sequence": set["InterestSet"]!.AsObject().Remove("INTEREST_SET_SEQ_NO"); break;
            case "missing-percent": set["CompanyPartners"]![0]!.AsObject().Remove("GROSS_PERCENT_INTEREST"); break;
            case "foreign-partner": set["CompanyPartners"]![0]!["PARTNER_BA_ID"] = "other"; break;
            case "duplicate-key": set["Components"]!.AsArray().Add(set["Components"]![0]!.DeepClone()); break;
            case "missing-date-zone": wire.AsObject().Remove("DateZoneId"); break;
            case "padded-date-zone": wire["DateZoneId"] = " UTC "; break;
            case "default-timestamp": wire["ReviewedUtc"] = DateTime.MinValue.ToString("O"); break;
            case "missing-type": set["InterestSet"]!.AsObject().Remove("INTEREST_SET_TYPE"); break;
        }
        using var company = new CompanyClientContext(); var generation = company.Clear(); Assert.True(company.TrySelect(generation, "account", new(f.Gate.Company.Scope, ["Administrator"], [])));
        using var http = new HttpClient(new Handler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(wire.ToJsonString()) })));
        var sdk = new AccessApi(new ClientApiTransport(http, new() { ApiBaseUrl = "https://api.test" }, company));
        await Assert.ThrowsAsync<ApiClientException>(() => sdk.GetFieldInterestReviewAsync("A"));
    }

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    { protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request); }
    private static HttpResponseMessage Json(object value) => new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web))) };
}
