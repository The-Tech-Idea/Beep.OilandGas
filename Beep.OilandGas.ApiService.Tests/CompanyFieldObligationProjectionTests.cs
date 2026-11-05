#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Controllers.Compliance;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Utilities;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.PPDM39.Models;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyFieldObligationProjectionTests
{
    private sealed class WriterFixture : IDisposable
    {
        public readonly DevelopmentDraftAuthorityTests.Fixture Company = new();
        public readonly CreateFieldObligation Request;
        public readonly CompanyFieldObligationWriter Writer;
        public readonly CompanyFieldObligationCommandScope Scope;
        public bool Revoke;
        public Action? AfterInsert;
        public Action<EntityStructure>? AlterSchema;
        public string? MissingProjectionTable;
        public string? TypedProjectionTable;
        public WriterFixture()
        {
            var native = Company.Drafts.Native; native.Rows.Clear(); native.Connection = "core";
            native.Source.SetupGet(s => s.DatasourceName).Returns("core");
            var core = native.Editor.Object.ConfigEditor.DataConnections.Single(c => c.ConnectionName == "core");
            Mock.Get(native.Source.Object.Dataconnection).SetupGet(c => c.ConnectionProp).Returns(core);
            native.Editor.Setup(e => e.OpenDataSource("core")).Returns(System.Data.ConnectionState.Open);
            native.Rows.Add(new FIELD { FIELD_ID = "field", ACTIVE_IND = "Y" });
            native.Rows.Add(new R_OBLIG_TYPE { LAND_OBLIG_TYPE = "REPORT", ACTIVE_IND = "Y" });
            native.Rows.Add(new R_OBLIG_COMPONENT_TYPE { OBLIGATION_COMPONENT_TYPE = "FIELD", ACTIVE_IND = "Y" });
            var keys = new Dictionary<string, string[]> { ["FIELD"] = ["FIELD_ID"], ["R_OBLIG_TYPE"] = ["LAND_OBLIG_TYPE"],
                ["R_OBLIG_COMPONENT_TYPE"] = ["OBLIGATION_COMPONENT_TYPE"], ["OBLIGATION"] = ["OBLIGATION_ID", "OBLIGATION_SEQ_NO"],
                ["OBLIGATION_COMPONENT"] = ["OBLIGATION_ID", "OBLIGATION_SEQ_NO", "COMPONENT_OBS_NO"] };
            var types = new[] { typeof(FIELD), typeof(R_OBLIG_TYPE), typeof(R_OBLIG_COMPONENT_TYPE), typeof(OBLIGATION), typeof(OBLIGATION_COMPONENT) };
            var metadata = new Mock<IPPDMMetadataRepository>();
            metadata.Setup(m => m.GetTableMetadataAsync(It.IsAny<string>())).ReturnsAsync((string name) => new PPDMTableMetadata { EntityTypeName = name, TableName = name, PrimaryKeyColumn = string.Join(",", keys[name]) });
            metadata.Setup(m => m.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string name) => keys[name]);
            native.Source.Setup(s => s.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure entity, bool _) => {
                var schema = new ClassCreator(native.Editor.Object).ConvertToEntityStructure(types.Single(t => t.Name == entity.EntityName));
                foreach (var field in schema.Fields) { field.IsKey = keys[entity.EntityName].Contains(field.FieldName); if (field.Fieldtype == typeof(string).FullName) { field.Size = 512; field.Size1 = 512; } }
                AlterSchema?.Invoke(schema); return schema; });
            native.Source.Setup(s => s.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>())).ReturnsAsync((string table, List<AppFilter> filters) => {
                var rows = native.Rows.Where(row => row.GetType().Name == table && filters.All(f => Convert.ToString(row.GetType().GetProperty(f.FieldName)!.GetValue(row), System.Globalization.CultureInfo.InvariantCulture) == f.FilterValue)).ToArray();
                if (table == TypedProjectionTable || table is not ("R_OBLIG_TYPE" or "R_OBLIG_COMPONENT_TYPE" or "OBLIGATION" or "OBLIGATION_COMPONENT")) return rows;
                return rows.Select(row => (object)row.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
                    .Where(property => property.CanRead && (table != MissingProjectionTable || property.Name != "EXPIRY_DATE")).ToDictionary(property => property.Name, property => property.GetValue(row))).ToArray();
            });
            native.Source.Setup(s => s.InsertEntity(It.IsAny<string>(), It.IsAny<object>())).Returns((string _, object row) => {
                native.Writes++; if (native.Writes == native.FailWrite) return new ErrorsInfo { Flag = Errors.Failed };
                native.Rows.Add(System.Text.Json.JsonSerializer.Deserialize(System.Text.Json.JsonSerializer.Serialize(row), row.GetType())!); AfterInsert?.Invoke(); return new ErrorsInfo { Flag = Errors.Ok }; });
            var policy = new Mock<IPPDMRowAccessPolicy>();
            policy.Setup(p => p.GetEffectiveAccessLevelAsync(typeof(FIELD), "FIELD", It.IsAny<object>(), true)).ReturnsAsync("WRITE");
            Writer = new(Company.Company, native.Editor.Object, new CommonColumnHandler(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object, policy.Object);
            Request = new(Company.Context, Guid.NewGuid(), "REPORT", "FIELD", "Recorded obligation", new DateTime(2030, 1, 2));
            Task Check() => Revoke ? Task.FromException(new UnauthorizedAccessException()) : Task.CompletedTask;
            Scope = new(Company.Context, Check, Check);
        }
        public void Dispose() => Company.Dispose();
    }

    [Fact]
    public async Task WriterCreatesBothRecordsAndExactRetryDoesNotDuplicate()
    {
        using var f = new WriterFixture(); var first = await f.Writer.CreateAsync(f.Request, f.Scope);
        var retry = await f.Writer.CreateAsync(f.Request, f.Scope);
        Assert.Equal(first, retry); Assert.Single(f.Company.Drafts.Native.Rows.OfType<OBLIGATION>());
        Assert.Single(f.Company.Drafts.Native.Rows.OfType<OBLIGATION_COMPONENT>()); Assert.Equal(2, f.Company.Drafts.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(() => f.Writer.CreateAsync(f.Request with { Description = "Different evidence" }, f.Scope));
    }

    [Fact]
    public async Task OptionsUseActiveRecordedTypesAndRejectAmbiguousCodes()
    {
        using var f = new WriterFixture();
        f.Company.Drafts.Native.Rows.Add(new R_OBLIG_TYPE { LAND_OBLIG_TYPE = "EXPIRED", ACTIVE_IND = "Y", EXPIRY_DATE = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        var options = await f.Writer.OptionsAsync(f.Scope);
        Assert.Equal(new[] { "REPORT" }, options.ObligationTypes); Assert.Equal(new[] { "FIELD" }, options.ComponentTypes);
        f.Company.Drafts.Native.Rows.Add(new R_OBLIG_TYPE { LAND_OBLIG_TYPE = "REPORT", ACTIVE_IND = "Y" });
        await Assert.ThrowsAnyAsync<Exception>(() => f.Writer.OptionsAsync(f.Scope));
    }

    [Fact]
    public async Task CreationControllerUsesCurrentCompanyAuthorityForOptionsAndWrite()
    {
        using var f = new WriterFixture(); var company = f.Company;
        company.Permissions = [PermissionConstants.Regulatory.ManageFilings];
        var resolver = new RepositoryCompanyContextResolver(company.Accounts.Object);
        company.Tenant.SelectCompany((await resolver.ResolveAsync("planner", new("tenant", "company")))!);
        company.Grants.Setup(g => g.ReadAsync("planner", company.Context.Database.Fingerprint, "company"))
            .ReturnsAsync(() => new List<AppUserAssetAccess> { new() { Id = "grant", UserId = "planner", DatabaseScope = company.Context.Database.Fingerprint,
                OrganizationId = "company", AssetType = "FIELD", AssetId = "field", AccessLevel = "WRITE", Inherit = true, IsActive = true, ConcurrencyStamp = "original" } });
        var authority = new CompanyFieldObligationAuthority(company.Account, company.Company, company.Fields.Object, company.Preference.Object, company.Assets.Object, company.Grants.Object);
        var controller = new FieldObligationCreationController(authority, f.Writer, company.Fields.Object) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = company.User } } };
        var choices = Assert.IsType<FieldObligationCreationOptions>(Assert.IsType<OkObjectResult>((await controller.OptionsAsync()).Result).Value);
        Assert.Equal(new[] { "REPORT" }, choices.ObligationTypes);
        var saved = Assert.IsType<CreatedFieldObligation>(Assert.IsType<OkObjectResult>((await controller.CreateAsync(f.Request)).Result).Value);
        Assert.Equal(f.Request.Context, saved.Context); Assert.Equal(2, company.Drafts.Native.Writes);
        company.Context = company.Context with { Revision = "refreshed" };
        var recovered = Assert.IsType<CreatedFieldObligation>(Assert.IsType<OkObjectResult>((await controller.CreateAsync(f.Request)).Result).Value);
        Assert.Equal(company.Context, recovered.Context); Assert.Equal(saved.ObligationId, recovered.ObligationId); Assert.Equal(2, company.Drafts.Native.Writes);
        company.Context = company.Context with { FieldId = "other" };
        await Assert.ThrowsAnyAsync<Exception>(() => controller.CreateAsync(f.Request));
        company.Context = company.Context with { FieldId = "field" };
        company.Permissions = [];
        await Assert.ThrowsAnyAsync<Exception>(() => controller.CreateAsync(f.Request with { IntentId = Guid.NewGuid() }));
        Assert.Equal(2, company.Drafts.Native.Writes);
    }

    [Theory]
    [InlineData("second-write")]
    [InlineData("reference")]
    [InlineData("isolation")]
    [InlineData("revoked")]
    [InlineData("schema-key")]
    [InlineData("schema-width")]
    [InlineData("schema-column")]
    [InlineData("type-projection")]
    [InlineData("component-projection")]
    [InlineData("type-schema")]
    [InlineData("typed-type")]
    [InlineData("typed-component")]
    public async Task WriterDoesNotLeaveAPartialObligation(string fault)
    {
        using var f = new WriterFixture(); var native = f.Company.Drafts.Native;
        if (fault == "second-write") native.FailWrite = 2;
        if (fault == "reference") native.Rows.RemoveAll(row => row is R_OBLIG_COMPONENT_TYPE);
        if (fault == "isolation") native.Isolation = System.Data.IsolationLevel.ReadCommitted;
        if (fault == "revoked") f.AfterInsert = () => f.Revoke = true;
        if (fault == "schema-key") f.AlterSchema = schema => { if (schema.EntityName == "OBLIGATION_COMPONENT") schema.Fields.Single(field => field.FieldName == "COMPONENT_OBS_NO").IsKey = false; };
        if (fault == "schema-width") f.AlterSchema = schema => { if (schema.EntityName == "OBLIGATION") { var field = schema.Fields.Single(column => column.FieldName == "DESCRIPTION"); field.Size = field.Size1 = 1; } };
        if (fault == "schema-column") f.AlterSchema = schema => { if (schema.EntityName == "OBLIGATION_COMPONENT") schema.Fields.RemoveAll(field => field.FieldName == "FIELD_ID"); };
        if (fault == "type-projection") f.MissingProjectionTable = "R_OBLIG_TYPE";
        if (fault == "component-projection") f.MissingProjectionTable = "R_OBLIG_COMPONENT_TYPE";
        if (fault == "type-schema") f.AlterSchema = schema => { if (schema.EntityName == "R_OBLIG_TYPE") schema.Fields.RemoveAll(field => field.FieldName == "EXPIRY_DATE"); };
        if (fault == "typed-type") f.TypedProjectionTable = "R_OBLIG_TYPE";
        if (fault == "typed-component") f.TypedProjectionTable = "R_OBLIG_COMPONENT_TYPE";
        await Assert.ThrowsAnyAsync<Exception>(() => f.Writer.CreateAsync(f.Request, f.Scope));
        Assert.Empty(native.Rows.OfType<OBLIGATION>()); Assert.Empty(native.Rows.OfType<OBLIGATION_COMPONENT>());
    }

    [Fact]
    public async Task UnconfirmedCommitCanBeRecoveredWithTheSameRequest()
    {
        using var f = new WriterFixture(); var native = f.Company.Drafts.Native; native.FailCommit = true;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Writer.CreateAsync(f.Request, f.Scope));
        Assert.Single(native.Rows.OfType<OBLIGATION>()); Assert.Single(native.Rows.OfType<OBLIGATION_COMPONENT>());
        native.FailCommit = false; var saved = await f.Writer.CreateAsync(f.Request, f.Scope);
        Assert.Equal(2, native.Writes); Assert.Equal("FO-" + f.Request.IntentId.ToString("N").ToUpperInvariant(), saved.ObligationId);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("permission")]
    [InlineData("grant")]
    [InlineData("field")]
    [InlineData("preference")]
    [InlineData("account")]
    public async Task FilingAuthorityRequiresOriginalCurrentCompanyAndField(string scenario)
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture();
        fixture.Permissions = [PermissionConstants.Regulatory.ManageFilings];
        var context = new RepositoryCompanyContextResolver(fixture.Accounts.Object);
        fixture.Tenant.SelectCompany((await context.ResolveAsync("planner", new("tenant", "company")))!);
        fixture.Grants.Setup(g => g.ReadAsync("planner", fixture.Context.Database.Fingerprint, "company"))
            .ReturnsAsync(() => new List<AppUserAssetAccess> { new() { Id = "grant", UserId = "planner", DatabaseScope = fixture.Context.Database.Fingerprint,
                OrganizationId = "company", AssetType = "FIELD", AssetId = "field", AccessLevel = fixture.GrantChanged ? "READ" : "WRITE", Inherit = true, IsActive = true, ConcurrencyStamp = "original" } });
        var authority = new CompanyFieldObligationAuthority(fixture.Account, fixture.Company, fixture.Fields.Object,
            fixture.Preference.Object, fixture.Assets.Object, fixture.Grants.Object);
        var captured = await authority.CaptureAsync(fixture.User, fixture.Context);
        switch (scenario)
        {
            case "permission": fixture.Permissions = []; break;
            case "grant": fixture.GrantChanged = true; break;
            case "field": fixture.Context = fixture.Context with { FieldId = "other" }; break;
            case "preference": fixture.Stored = false; break;
            case "account": fixture.Active = false; break;
        }
        if (scenario == "valid") { await captured.CheckCurrentAsync(); await captured.ConfirmAsync(); }
        else await Assert.ThrowsAnyAsync<Exception>(() => captured.ConfirmAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ObligationParentAndFieldLinkRejectUnownedGenericMutations(bool component)
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture();
        var native = fixture.Drafts.Native;
        var type = component ? typeof(OBLIGATION_COMPONENT) : typeof(OBLIGATION);
        var entity = Activator.CreateInstance(type)!;
        var repository = new PPDMGenericRepository(native.Editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), type, "core", type.Name);
        await Assert.ThrowsAnyAsync<Exception>(() => repository.InsertAsync(entity, "planner"));
        await Assert.ThrowsAnyAsync<Exception>(() => repository.UpdateAsync(entity, "planner"));
        await Assert.ThrowsAnyAsync<Exception>(() => repository.DeleteAsync(entity));
        await Assert.ThrowsAnyAsync<Exception>(() => repository.SoftDeleteAsync("id", "planner"));
        await Assert.ThrowsAnyAsync<Exception>(() => repository.InsertBatchAsync([entity], "planner"));
        Assert.ThrowsAny<Exception>(() => repository.UnitOfWork);
        native.Source.Verify(source => source.InsertEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        native.Source.Verify(source => source.UpdateEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        native.Source.Verify(source => source.DeleteEntity(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Theory]
    [InlineData("valid")]
    [InlineData("parent-column")]
    [InlineData("component-column")]
    [InlineData("schema-column")]
    public async Task RecordedObligationReadRequiresProjectedColumnsButAcceptsNullValues(string mode)
    {
        using var fixture = new DevelopmentDraftAuthorityTests.Fixture();
        var native = fixture.Drafts.Native; var core = native.Editor.Object.ConfigEditor.DataConnections.Single(x => x.ConnectionName == "core");
        native.Source.SetupGet(x => x.DatasourceName).Returns("core");
        Mock.Get(native.Source.Object.Dataconnection).SetupGet(x => x.ConnectionProp).Returns(core);
        native.Editor.Setup(x => x.OpenDataSource("core")).Returns(System.Data.ConnectionState.Open);
        native.Source.Setup(x => x.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure entity, bool _) =>
        {
            var type = entity.EntityName == "OBLIGATION" ? typeof(OBLIGATION) : typeof(OBLIGATION_COMPONENT);
            var schema = new ClassCreator(native.Editor.Object).ConvertToEntityStructure(type);
            if (mode == "schema-column" && type == typeof(OBLIGATION)) schema.Fields.RemoveAll(field => field.FieldName == "CRITICAL_DATE");
            return schema;
        });
        var link = new Dictionary<string, object?> { ["OBLIGATION_ID"] = "obligation", ["OBLIGATION_SEQ_NO"] = 1m, ["COMPONENT_OBS_NO"] = 1m,
            ["FIELD_ID"] = "field", ["ACTIVE_IND"] = "Y", ["OBLIGATION_COMPONENT_TYPE"] = null, ["CRITICAL_DATE"] = null,
            ["FULFILLED_IND"] = null, ["FULFILLED_DATE"] = null, ["DESCRIPTION"] = null };
        var parent = new Dictionary<string, object?> { ["OBLIGATION_ID"] = "obligation", ["OBLIGATION_SEQ_NO"] = 1m,
            ["ACTIVE_IND"] = "Y", ["OBLIGATION_TYPE"] = null, ["CRITICAL_DATE"] = null,
            ["FULFILLED_IND"] = null, ["FULFILLED_DATE"] = null, ["DESCRIPTION"] = null };
        if (mode == "parent-column") parent.Remove("CRITICAL_DATE");
        if (mode == "component-column") link.Remove("DESCRIPTION");
        native.Source.Setup(x => x.GetEntityAsync("OBLIGATION_COMPONENT", It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[] { link });
        native.Source.Setup(x => x.GetEntityAsync("OBLIGATION", It.IsAny<List<AppFilter>>())).ReturnsAsync(new object[] { parent });
        var metadata = new Mock<IPPDMMetadataRepository>();
        metadata.Setup(x => x.GetTableMetadataAsync("OBLIGATION")).ReturnsAsync(new PPDMTableMetadata { EntityTypeName = "OBLIGATION", TableName = "OBLIGATION", PrimaryKeyColumn = "OBLIGATION_ID,OBLIGATION_SEQ_NO" });
        metadata.Setup(x => x.GetTableMetadataAsync("OBLIGATION_COMPONENT")).ReturnsAsync(new PPDMTableMetadata { EntityTypeName = "OBLIGATION_COMPONENT", TableName = "OBLIGATION_COMPONENT", PrimaryKeyColumn = "OBLIGATION_ID,OBLIGATION_SEQ_NO,COMPONENT_OBS_NO" });
        var reader = new CompanyFieldObligationReader(fixture.Company, fixture.Fields.Object, fixture.Assets.Object,
            native.Editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(), metadata.Object);
        if (mode != "valid") { await Assert.ThrowsAnyAsync<Exception>(() => reader.ReadAsync("planner")); return; }
        var result = await reader.ReadAsync("planner"); var row = Assert.Single(result.Obligations);
        Assert.Null(row.CriticalDate); Assert.Null(row.Description); Assert.Null(Assert.Single(row.Components).Description);
    }
}
#endif
