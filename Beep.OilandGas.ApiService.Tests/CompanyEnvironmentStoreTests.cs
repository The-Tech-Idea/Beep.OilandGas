using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyEnvironmentStoreTests
{
    private static readonly CompanyScope Company = new("tenant", "company");
    private sealed class Fixture
    {
        public readonly CompanyGLAccountReviewTests.Fixture Source = new();
        public readonly CompanyEnvironmentStore Store;
        public Action? OnWrite;
        public Action? OnReview;
        public Fixture()
        {
            Source.Native.Rows.Clear();
            Source.Native.Source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure _, bool _) =>
            { var shape = new ClassCreator(Source.Native.Editor.Object).ConvertToEntityStructure(typeof(COMPANY_OILGAS_ENVIRONMENT)); Source.ChangeSchema?.Invoke(shape); return shape; });
            var metadata = new Mock<IPPDMMetadataRepository>();
            metadata.Setup(x => x.GetPrimaryKeyColumnsAsync("COMPANY_OILGAS_ENVIRONMENT")).ReturnsAsync(["COMPANY_OILGAS_ENVIRONMENT_ID"]);
            Store = new(Source.Native.Editor.Object, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object, () => Task.FromResult(Source.Native.Connection),
                async (company, draft, review, source, guard) =>
                {
                    await guard(); PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); OnReview?.Invoke();
                    if (review.DraftRevision != CompanyEnvironmentDraftIdentity.ForReview(company, draft, review.SystemRevision, review.CountryRevision, review.ToolsVersion))
                        throw new InvalidOperationException("Review changed.");
                });
            Source.Native.Source.Setup(x => x.InsertEntity("COMPANY_OILGAS_ENVIRONMENT", It.IsAny<object>())).Returns((string _, object value) =>
            { Source.Native.Writes++; Source.Native.Rows.Add(Clone(value)); OnWrite?.Invoke(); return new ErrorsInfo { Flag = Errors.Ok }; });
            Source.Native.Source.Setup(x => x.UpdateEntity("COMPANY_OILGAS_ENVIRONMENT", It.IsAny<object>())).Returns((string _, object value) =>
            { Source.Native.Writes++; Source.Native.Rows.Clear(); Source.Native.Rows.Add(Clone(value)); OnWrite?.Invoke(); return new ErrorsInfo { Flag = Errors.Ok }; });
        }
        private static COMPANY_OILGAS_ENVIRONMENT Clone(object value) => JsonSerializer.Deserialize<COMPANY_OILGAS_ENVIRONMENT>(JsonSerializer.Serialize(value))!;
        public Task<CompanyEnvironmentConfiguration?> Read() => Store.ReadAsync(Company, Source.Authority);
        public Task<CompanyEnvironmentConfiguration> Save(CompanyEnvironmentSaveRequest request) => Store.SaveAsync(Company, "administrator", Reviewed(Company, request), Source.Authority);
    }

    private static CompanyEnvironmentSaveRequest Request(string expected = "") => new("KW", "{\"culture\":\"ar-KW\"}", expected, Guid.NewGuid().ToString("N"));
    private static CompanyEnvironmentReviewedSaveRequest Reviewed(CompanyScope company, CompanyEnvironmentSaveRequest request)
    {
        var draft = new CompanyEnvironmentPreviewRequest(request.CountryCode, request.OverridesJson);
        var settings = CompanyEnvironmentPreviewer.ValidateDraft(company, draft);
        var revision = new string('A', 64); var version = CompanyEnvironmentPreviewer.ToolsVersion;
        return new(request, new(settings, revision, null, version, CompanyEnvironmentDraftIdentity.ForReview(company, draft, revision, null, version)));
    }

    [Fact]
    public async Task SaveReadRetryAndUpdateUseCompanyRevision()
    {
        var fixture = new Fixture(); Assert.Null(await fixture.Read());
        var request = Request(); var first = await fixture.Save(request);
        Assert.Equal(first, await fixture.Read()); Assert.Equal(first, await fixture.Save(request)); Assert.Equal(1, fixture.Source.Native.Writes);
        var changed = await fixture.Save(Request(first.Revision) with { CountryCode = "NO", OverridesJson = "{\"culture\":\"nb-NO\"}" });
        Assert.Equal(first.Revision, changed.PreviousRevision); Assert.Equal("NO", changed.CountryCode); Assert.Equal(2, fixture.Source.Native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Save(Request(first.Revision)));
        Assert.Equal(changed, await fixture.Read()); Assert.Equal(2, fixture.Source.Native.Writes);
    }

    [Theory]
    [InlineData("schema")][InlineData("isolation")][InlineData("authority")][InlineData("binding")]
    public async Task UnconfirmedSchemaTransactionOrAuthorityNeverCommits(string fault)
    {
        var fixture = new Fixture();
        if (fault == "schema") fixture.Source.ChangeSchema = schema => schema.Fields.Remove(schema.Fields.Single(x => x.FieldName == "OVERRIDES_JSON"));
        if (fault == "isolation") fixture.Source.Native.Isolation = IsolationLevel.ReadCommitted;
        fixture.OnWrite = () => { if (fault == "authority") fixture.Source.RevokeAt = fixture.Source.AuthorityCalls + 1; if (fault == "binding") fixture.Source.Native.Connection = "other"; };
        await Assert.ThrowsAnyAsync<Exception>(() => fixture.Save(Request()));
        Assert.Empty(fixture.Source.Native.Rows);
    }

    [Fact]
    public async Task ForeignRowsAndCorruptRevisionsAreNeverReturned()
    {
        var fixture = new Fixture(); await fixture.Save(Request());
        var row = Assert.IsType<COMPANY_OILGAS_ENVIRONMENT>(Assert.Single(fixture.Source.Native.Rows));
        row.OWNER_COMPANY_BA_ID = "other";
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Read());
        row.OWNER_COMPANY_BA_ID = "company"; row.OVERRIDES_JSON = "{}";
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Read());
        row.OVERRIDES_JSON = "{\"culture\":\"ar-KW\"}"; row.REVIEWED_DRAFT_REVISION = new string('B', 64);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Read());
    }

    [Fact]
    public async Task LostCommitAcknowledgementCanBeRecoveredAndRetriedWithoutAnotherWrite()
    {
        var fixture = new Fixture(); var request = Request(); fixture.Source.Native.FailCommit = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Save(request));
        fixture.Source.Native.FailCommit = false;
        var saved = Assert.IsType<CompanyEnvironmentConfiguration>(await fixture.Read());
        Assert.Equal(request.IntentId, saved.IntentId);
        Assert.Equal(saved, await fixture.Save(request));
        Assert.Equal(1, fixture.Source.Native.Writes);
    }

    [Fact]
    public async Task ExactRetrySurvivesChangedSharedSetupButAnotherReviewCannotReuseTheIntent()
    {
        var fixture = new Fixture(); var request = Request(); var command = Reviewed(Company, request);
        var saved = await fixture.Store.SaveAsync(Company, "administrator", command, fixture.Source.Authority);
        Assert.Equal(command.Review.DraftRevision, saved.ReviewedDraftRevision);
        fixture.OnReview = () => throw new InvalidOperationException("Shared setup changed.");
        Assert.Equal(saved, await fixture.Store.SaveAsync(Company, "administrator", command, fixture.Source.Authority));
        var newerSystem = new string('B', 64);
        var changedReview = command.Review with { SystemRevision = newerSystem,
            DraftRevision = CompanyEnvironmentDraftIdentity.ForReview(Company, new(request.CountryCode, request.OverridesJson), newerSystem, null, command.Review.ToolsVersion) };
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Store.SaveAsync(Company, "administrator", command with { Review = changedReview }, fixture.Source.Authority));
        Assert.Equal(1, fixture.Source.Native.Writes);
    }

    [Fact]
    public async Task TwoCompaniesRetainDistinctConfigurationsOnTheSameDatasource()
    {
        var fixture = new Fixture(); var first = await fixture.Save(Request());
        var other = new CompanyScope("tenant", "other-company");
        var second = await fixture.Store.SaveAsync(other, "administrator", Reviewed(other, Request() with { CountryCode = "NO", OverridesJson = "{}" }), fixture.Source.Authority);
        Assert.Equal(first, await fixture.Read());
        Assert.Equal(second, await fixture.Store.ReadAsync(other, fixture.Source.Authority));
        Assert.NotEqual(first.Revision, second.Revision);
        Assert.Equal(2, fixture.Source.Native.Rows.Count);
    }

    [Fact]
    public async Task FailedReviewBeforeOrAfterWriteRollsBackTheCompanyChange()
    {
        foreach (var failureOnReview in new[] { 1, 2 })
        {
            var fixture = new Fixture(); var calls = 0;
            fixture.OnReview = () => { if (++calls == failureOnReview) throw new InvalidOperationException("Shared setup changed."); };
            await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Save(Request()));
            Assert.Empty(fixture.Source.Native.Rows);
        }
    }
}
#endif
