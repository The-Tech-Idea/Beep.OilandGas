using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyGLMappingReviewTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal static readonly DateTime Date=new(2026,10,12);
    internal sealed class Fixture
    {
        internal readonly CompanyGLAccountReviewTests.Fixture Accounts=new();
        internal AccountMappingService Service;
        internal readonly Mock<IPPDMMetadataRepository> Metadata=new();
        internal GLAccountMapping Mapping=new(){GL_ACCOUNT_MAPPING_ID="mapping-id",MAPPING_KEY="AccountsReceivable",GL_ACCOUNT_ID="actual-account-id",GL_ACCOUNT_NUMBER="1110",
            OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",BOOK_ID="statutory",CURRENCY_CODE="KWD",ACTIVE_IND="Y",EFFECTIVE_DATE=new DateTime(2026,1,1),
            ROW_CREATED_BY="administrator",ROW_CREATED_DATE=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc)};
        internal int MappingReads;
        internal Action? OnMappingRead;
        internal Action<EntityStructure>? ChangeSchema;
        internal object[]? OverrideRows;
        internal bool NullRows;
        internal Fixture(bool bound=true,string? accountConnection=null)
        {
            Accounts.Native.Rows.Add(Mapping);Metadata.Setup(value=>value.GetTableMetadataAsync("GL_ACCOUNT_MAPPING")).ReturnsAsync(new PPDMTableMetadata{TableName="GL_ACCOUNT_MAPPING"});
            var defaults=new Mock<IPPDM39DefaultsRepository>();defaults.Setup(value=>value.GetActiveIndicatorYes()).Returns("Y");
            Accounts.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>{
                var schema=new ClassCreator(Accounts.Native.Editor.Object).ConvertToEntityStructure(request.EntityName=="GL_ACCOUNT"?typeof(GL_ACCOUNT):typeof(GLAccountMapping));
                if(request.EntityName=="GL_ACCOUNT_MAPPING")ChangeSchema?.Invoke(schema);return schema;});
            Accounts.Native.Source.Setup(source=>source.GetEntityAsync("GL_ACCOUNT_MAPPING",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                MappingReads++;OnMappingRead?.Invoke();return NullRows?null!:OverrideRows ?? Accounts.Native.Rows.OfType<GLAccountMapping>().Where(row=>filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(row=>JsonSerializer.Deserialize<GLAccountMapping>(JsonSerializer.Serialize(row))!).Cast<object>().ToArray();});
            var accounts=accountConnection is null?Accounts.Service:new GLAccountService(Accounts.Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Metadata.Object,NullLogger<GLAccountService>.Instance,()=>Task.FromResult(accountConnection));
            Service=new(Accounts.Native.Editor.Object,new CommonColumnHandler(),defaults.Object,Metadata.Object,NullLogger<AccountMappingService>.Instance,
                bound?()=>Task.FromResult(Accounts.Native.Connection):null,accounts);
        }
        internal Task<CompanyGLMappingReview> Read()=>Service.GetCompanyMappingsAsync(Company,"accountant","statutory","KWD",Date,Accounts.Authority);
    }
    [Fact] public void CanonicalBeepMappingUsesTheExistingPhysicalTableAndProductionFinancialManifest()
    {
        var schema=new ClassCreator(Mock.Of<TheTechIdea.Beep.Editor.IDMEEditor>()).ConvertToEntityStructure(typeof(GLAccountMapping));
        Assert.Equal("GL_ACCOUNT_MAPPING",schema.EntityName);Assert.Equal("GL_ACCOUNT_MAPPING",schema.DatasourceEntityName);var key=Assert.Single(schema.Fields,field=>field.IsKey);
        Assert.Equal("GL_ACCOUNT_MAPPING_ID",key.FieldName);Assert.False(key.AllowDBNull);Assert.Equal(128,key.Size1);ModuleSchemaBoundary.Validate([typeof(GLAccountMapping)]);
        var financialTypes=Beep.OilandGas.ProductionAccounting.Modules.ProductionAccountingFinancialManifest.EntityTypes;
        Assert.Single(financialTypes,type=>type==typeof(COMPANY_LEDGER_BOOK));
        Assert.Single(financialTypes,type=>type==typeof(FX_RATE));
        Assert.Single(financialTypes,type=>type==typeof(GLAccountMapping));Assert.Equal(financialTypes.Count,financialTypes.Distinct().Count());
        ModuleSchemaBoundary.Validate(financialTypes);Assert.All(financialTypes,type=>Assert.Equal(typeof(CompanyScope).Assembly,type.Assembly));
    }
    [Fact] public async Task CompleteEffectiveMappingResolvesActualAccountIdentityWithoutDefaultCacheOrWrites()
    {
        var fixture=new Fixture();fixture.Accounts.Native.Rows.Add(new GLAccountMapping{GL_ACCOUNT_MAPPING_ID="foreign",OWNER_TENANT_ID="foreign"});var review=await fixture.Read();
        review.Require(Company,"accountant","statutory","KWD",Date);var row=review.Resolve("AccountsReceivable","ASSET","DEBIT");Assert.Equal("actual-account-id",row.GL_ACCOUNT_ID);Assert.Equal("1110",row.ACCOUNT_NUMBER);
        Assert.Equal(2,fixture.MappingReads);Assert.Equal(8,fixture.Accounts.Reads);Assert.Single(review.Records);row.ACCOUNT_NAME="caller changed";Assert.Equal("Recorded receivable",fixture.Accounts.Row.ACCOUNT_NAME);
        foreach(var call in fixture.Accounts.Native.Source.Invocations.Where(call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="GL_ACCOUNT_MAPPING"))Assert.Equal(5,Assert.IsType<List<AppFilter>>(call.Arguments[1]).Count);
        Assert.Throws<InvalidOperationException>(()=>review.Resolve("Revenue","REVENUE","CREDIT"));Assert.Throws<InvalidOperationException>(()=>review.Resolve("AccountsReceivable","REVENUE","CREDIT"));fixture.Accounts.NoWrites();
    }
    [Fact] public async Task ExpiredAndFutureVersionsDoNotSupplyTheCurrentPostingAccount()
    {
        var fixture=new Fixture();fixture.Mapping.EXPIRY_DATE=new DateTime(2026,10,11);var future=JsonSerializer.Deserialize<GLAccountMapping>(JsonSerializer.Serialize(fixture.Mapping))!;
        future.GL_ACCOUNT_MAPPING_ID="future";future.EFFECTIVE_DATE=new DateTime(2026,10,13);future.EXPIRY_DATE=null;future.GL_ACCOUNT_ID="not-configured-yet";future.GL_ACCOUNT_NUMBER="future-number";
        fixture.Accounts.Native.Rows.Add(future);var review=await fixture.Read();Assert.Equal(2,review.Records.Count);Assert.Throws<InvalidOperationException>(()=>review.Resolve("AccountsReceivable","ASSET","DEBIT"));fixture.Accounts.NoWrites();
    }
    [Theory][InlineData("tenant")][InlineData("company")][InlineData("book")][InlineData("currency")][InlineData("inactive")][InlineData("accountId")][InlineData("accountNumber")]
    [InlineData("start")][InlineData("expiry")][InlineData("utc-date")][InlineData("time")][InlineData("creator")][InlineData("duplicateId")][InlineData("overlap")][InlineData("capacity")][InlineData("missingAccount")]
    public async Task ForeignAmbiguousDatedOrUnresolvedMappingFactsCannotPublish(string fault)
    {
        var fixture=new Fixture();switch(fault){case "tenant":fixture.Mapping.OWNER_TENANT_ID="foreign";break;case "company":fixture.Mapping.OWNER_COMPANY_BA_ID="foreign";break;
            case "book":fixture.Mapping.BOOK_ID="other";break;case "currency":fixture.Mapping.CURRENCY_CODE="USD";break;case "inactive":fixture.Mapping.ACTIVE_IND="N";break;
            case "accountId":fixture.Mapping.GL_ACCOUNT_ID="1110";break;case "accountNumber":fixture.Mapping.GL_ACCOUNT_NUMBER="actual-account-id";break;case "start":fixture.Mapping.EFFECTIVE_DATE=null;break;
            case "expiry":fixture.Mapping.EXPIRY_DATE=new DateTime(2025,12,31);break;case "utc-date":fixture.Mapping.EFFECTIVE_DATE=DateTime.SpecifyKind(Date,DateTimeKind.Utc);break;
            case "time":fixture.Mapping.EFFECTIVE_DATE=Date.AddHours(1);break;case "creator":fixture.Mapping.ROW_CREATED_BY=null!;break;case "missingAccount":fixture.Accounts.Native.Rows.Remove(fixture.Accounts.Row);break;
            case "duplicateId":fixture.OverrideRows=[fixture.Mapping,fixture.Mapping];break;case "overlap":var other=JsonSerializer.Deserialize<GLAccountMapping>(JsonSerializer.Serialize(fixture.Mapping))!;other.GL_ACCOUNT_MAPPING_ID="other";fixture.OverrideRows=[fixture.Mapping,other];break;
            case "capacity":fixture.OverrideRows=Enumerable.Repeat((object)fixture.Mapping,CompanyGLMappingReview.MaximumMappings+1).ToArray();break;}
        fixture.OverrideRows ??=[fixture.Mapping];await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Read());fixture.Accounts.NoWrites();
    }
    [Theory][InlineData("table")][InlineData("key")][InlineData("width")][InlineData("nullable")][InlineData("date")]
    public async Task UnconfirmedNativeMappingMetadataStopsBeforePrivateAccountOrMappingReads(string fault)
    {
        var fixture=new Fixture();fixture.ChangeSchema=schema=>{switch(fault){case "table":schema.EntityName="GLAccountMapping";break;case "key":schema.Fields.Single(field=>field.IsKey).IsKey=false;break;
            case "width":var field=schema.Fields.Single(field=>field.FieldName=="GL_ACCOUNT_ID");field.Size=field.Size1=1;break;case "nullable":schema.Fields.Single(field=>field.FieldName=="BOOK_ID").AllowDBNull=false;break;
            case "date":schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="EFFECTIVE_DATE"));break;}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());Assert.Equal(0,fixture.MappingReads);Assert.Equal(0,fixture.Accounts.Reads);fixture.Accounts.NoWrites();
    }
    [Theory][InlineData("mapping")][InlineData("account")][InlineData("binding")]
    public async Task ClosingMappingAccountOrBindingDriftCannotPublish(string fault)
    {
        var fixture=new Fixture();fixture.OnMappingRead=()=>{if(fixture.MappingReads!=2)return;if(fault=="mapping")fixture.Mapping.DESCRIPTION="changed";if(fault=="account")fixture.Accounts.Row.ACCOUNT_NAME="changed";if(fault=="binding")fixture.Accounts.Native.Connection="changed";};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());fixture.Accounts.NoWrites();
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(8)][InlineData(18)][InlineData(25)]
    public async Task CurrentAuthorityIsRequiredAcrossBothOwners(int boundary)
    {var fixture=new Fixture();fixture.Accounts.RevokeAt=boundary;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Read());fixture.Accounts.NoWrites();}
    [Theory][InlineData("missing")][InlineData("account-binding")]
    public async Task NeitherGlobalBindingNorAnotherAccountBindingCanSupplyPrivateMappings(string fault)
    {var fixture=new Fixture(bound:fault!="missing",accountConnection:fault=="account-binding"?"foreign-production":null);await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());Assert.Equal(0,fixture.MappingReads);Assert.Equal(0,fixture.Accounts.Reads);fixture.Accounts.NoWrites();}
    [Fact] public async Task EmptyAndUnavailableMappingHistoryRemainDistinctAndCannotBorrowDefaults()
    {
        var fixture=new Fixture();fixture.Accounts.Native.Rows.Remove(fixture.Mapping);var review=await fixture.Read();Assert.Empty(review.Records);Assert.Throws<InvalidOperationException>(()=>review.Resolve("AccountsReceivable","ASSET","DEBIT"));
        fixture.NullRows=true;await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Read());fixture.Accounts.NoWrites();
    }
    [Theory][InlineData("tenant")][InlineData("company")][InlineData("book")][InlineData("currency")][InlineData("id")]
    public async Task GlobalInitializationCannotSeedOrCachePartiallyOwnedMappings(string marker)
    {
        var fixture=new Fixture(bound:false);fixture.Mapping.OWNER_TENANT_ID=fixture.Mapping.OWNER_COMPANY_BA_ID=fixture.Mapping.BOOK_ID=fixture.Mapping.CURRENCY_CODE=fixture.Mapping.GL_ACCOUNT_ID=null;
        switch(marker){case "tenant":fixture.Mapping.OWNER_TENANT_ID="tenant";break;case "company":fixture.Mapping.OWNER_COMPANY_BA_ID="company";break;case "book":fixture.Mapping.BOOK_ID="statutory";break;case "currency":fixture.Mapping.CURRENCY_CODE="KWD";break;case "id":fixture.Mapping.GL_ACCOUNT_ID="actual-account-id";break;}
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.InitializeAsync("administrator"));fixture.Accounts.NoWrites();
    }
    [Fact] public async Task SelectedCompanyGlobalCacheDefaultAndInitializationPathsRefuseWithoutPrivateReads()
    {var fixture=new Fixture();Assert.Throws<InvalidOperationException>(()=>fixture.Service.GetAccountId("AccountsReceivable"));await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.InitializeAsync("administrator"));Assert.Equal(0,fixture.MappingReads);fixture.Accounts.NoWrites();}
    [Theory][InlineData("mapping")][InlineData("account")][InlineData("date")]
    public async Task DetachedReviewCannotResolveChangedMappingAccountOrDate(string fault)
    {
        var fixture=new Fixture();var review=await fixture.Read();if(fault=="mapping")review.Records[0].MAPPING_KEY="Revenue";if(fault=="account")review.Accounts.Accounts[0].ACCOUNT_NUMBER="other";if(fault=="date")review=review with{AsOfDate=Date.AddDays(1)};
        Assert.Throws<InvalidOperationException>(()=>review.Resolve("AccountsReceivable","ASSET","DEBIT"));Assert.Equal("AccountsReceivable",fixture.Mapping.MAPPING_KEY);Assert.Equal("1110",fixture.Accounts.Row.ACCOUNT_NUMBER);fixture.Accounts.NoWrites();
    }
    [Fact] public async Task AdjacentInclusiveVersionsResolveTheCurrentOneWithoutTreatingANumberAsAnId()
    {
        var fixture=new Fixture();fixture.Mapping.EXPIRY_DATE=Date.AddDays(-1);var next=JsonSerializer.Deserialize<GLAccountMapping>(JsonSerializer.Serialize(fixture.Mapping))!;next.GL_ACCOUNT_MAPPING_ID="new-mapping";next.EFFECTIVE_DATE=Date;next.EXPIRY_DATE=null;
        fixture.Accounts.Native.Rows.Add(next);var review=await fixture.Read();Assert.Equal("actual-account-id",review.Resolve("AccountsReceivable","ASSET","DEBIT").GL_ACCOUNT_ID);Assert.Equal(2,review.Records.Count);fixture.Accounts.NoWrites();
    }
    [Fact] public void MissingAccountPayloadCannotResolveAnIdentity()
    {var review=new CompanyGLMappingReview(null!,Date,[],new string('A',64));Assert.Throws<InvalidOperationException>(()=>review.Resolve("AccountsReceivable","ASSET","DEBIT"));}
}
#endif
