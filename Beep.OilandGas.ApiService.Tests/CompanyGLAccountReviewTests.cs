#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class CompanyGLAccountReviewTests
{
    private static readonly CompanyScope Company=new("tenant","company");
    internal sealed class Fixture
    {
        internal readonly PersonaTaskRoutingTests.Fixture Native=new();
        internal readonly GLAccountService Service;
        internal GL_ACCOUNT Row=new(){GL_ACCOUNT_ID="actual-account-id",ACCOUNT_NUMBER="1110",ACCOUNT_NAME="Recorded receivable",ACCOUNT_TYPE="ASSET",NORMAL_BALANCE="DEBIT",
            OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",BOOK_ID="statutory",CURRENCY_CODE="KWD",ACTIVE_IND="Y",ROW_CREATED_BY="administrator",
            ROW_CREATED_DATE=new DateTime(2026,10,12,0,0,0,DateTimeKind.Utc)};
        internal int Reads,AuthorityCalls,RevokeAt;
        internal Action? OnRead,OnAuthority;
        internal Action<EntityStructure>? ChangeSchema;
        internal object[]? OverrideRows;
        internal bool NullRows;
        internal Fixture(bool bound=true)
        {
            Native.Connection="production";Native.Rows.Clear();Native.Rows.Add(Row);
            Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool refresh)=>{
                var schema=new ClassCreator(Native.Editor.Object).ConvertToEntityStructure(typeof(GL_ACCOUNT));ChangeSchema?.Invoke(schema);return schema;});
            Native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                Reads++;OnRead?.Invoke();return NullRows?null!:OverrideRows ?? Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(Clone).ToArray();});
            Service=new(Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),NullLogger<GLAccountService>.Instance,
                bound?()=>Task.FromResult(Native.Connection):null);
        }
        internal Task Authority(){AuthorityCalls++;OnAuthority?.Invoke();if(AuthorityCalls==RevokeAt)throw new UnauthorizedAccessException();return Task.CompletedTask;}
        internal Task<CompanyGLAccountReview> Read()=>Service.GetCompanyAccountsAsync(Company,"accountant","statutory","KWD",Authority);
        internal static object Clone(object value)=>JsonSerializer.Deserialize(JsonSerializer.Serialize(value),value.GetType())!;
        internal void NoWrites(){Assert.Equal(0,Native.Writes);Assert.Empty(Native.Transactions);Assert.DoesNotContain(Native.Source.Invocations,call=>call.Method.Name is "InsertEntity" or "UpdateEntity" or "DeleteEntity" or "RunQuery" or "ExecuteSql");}
    }
    [Fact] public async Task CompleteRecordedAccountsAreScopedDetachedAndNeverInventBalances()
    {
        var fixture=new Fixture();fixture.Native.Rows.Add(new GL_ACCOUNT{GL_ACCOUNT_ID="foreign",ACCOUNT_NUMBER="1110",OWNER_TENANT_ID="other"});
        var review=await fixture.Read();review.Require(Company,"accountant","statutory","KWD");var row=Assert.Single(review.Accounts);
        Assert.Equal("actual-account-id",row.GL_ACCOUNT_ID);Assert.Equal("1110",row.ACCOUNT_NUMBER);Assert.Null(row.OPENING_BALANCE);Assert.Null(row.CURRENT_BALANCE);
        Assert.Equal(2,fixture.Reads);Assert.NotSame(fixture.Row,row);row.ACCOUNT_NAME="tampered";Assert.Equal("Recorded receivable",fixture.Row.ACCOUNT_NAME);
        Assert.Throws<InvalidOperationException>(()=>review.Require(Company,"accountant","statutory","KWD"));fixture.NoWrites();
        foreach(var call in fixture.Native.Source.Invocations.Where(call=>call.Method.Name=="GetEntityAsync"))
        {Assert.Equal("GL_ACCOUNT",call.Arguments[0]);var filters=Assert.IsType<List<AppFilter>>(call.Arguments[1]);Assert.Equal(5,filters.Count);Assert.All(filters,filter=>Assert.Equal("=",filter.Operator));
            Assert.Contains(filters,filter=>filter.FieldName=="OWNER_TENANT_ID" && filter.FilterValue=="tenant");Assert.Contains(filters,filter=>filter.FieldName=="OWNER_COMPANY_BA_ID" && filter.FilterValue=="company");
            Assert.Contains(filters,filter=>filter.FieldName=="BOOK_ID" && filter.FilterValue=="statutory");Assert.Contains(filters,filter=>filter.FieldName=="CURRENCY_CODE" && filter.FilterValue=="KWD");}
    }
    [Fact] public async Task EmptyIsARecordedEmptySetButUnavailableIsRefused()
    {
        var fixture=new Fixture();fixture.Native.Rows.Clear();Assert.Empty((await fixture.Read()).Accounts);fixture.NullRows=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Read());fixture.NoWrites();
    }
    [Theory][InlineData("tenant")][InlineData("company")][InlineData("book")][InlineData("currency")][InlineData("inactive")][InlineData("type")][InlineData("normal")]
    [InlineData("creator")][InlineData("created")][InlineData("local")][InlineData("changedBy")][InlineData("changedDate")][InlineData("duplicateId")][InlineData("duplicateNumber")][InlineData("wrongType")][InlineData("capacity")]
    public async Task ProviderCannotPublishForeignAmbiguousOrIncompleteFacts(string fault)
    {
        var fixture=new Fixture();switch(fault){case "tenant":fixture.Row.OWNER_TENANT_ID="foreign";break;case "company":fixture.Row.OWNER_COMPANY_BA_ID="foreign";break;
            case "book":fixture.Row.BOOK_ID="foreign";break;case "currency":fixture.Row.CURRENCY_CODE="USD";break;case "inactive":fixture.Row.ACTIVE_IND="N";break;
            case "type":fixture.Row.ACCOUNT_TYPE="UNKNOWN";break;case "normal":fixture.Row.NORMAL_BALANCE="UNKNOWN";break;case "creator":fixture.Row.ROW_CREATED_BY=null!;break;
            case "created":fixture.Row.ROW_CREATED_DATE=null;break;case "local":typeof(GL_ACCOUNT).BaseType!.GetField("ROW_CREATED_DATEValue",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.SetValue(fixture.Row,DateTime.SpecifyKind(fixture.Row.ROW_CREATED_DATE!.Value,DateTimeKind.Local));break;
            case "changedBy":fixture.Row.ROW_CHANGED_BY="administrator";break;case "changedDate":fixture.Row.ROW_CHANGED_DATE=fixture.Row.ROW_CREATED_DATE;break;
            case "wrongType":fixture.OverrideRows=[new object()];break;
            case "duplicateId":case "duplicateNumber":var extra=(GL_ACCOUNT)Fixture.Clone(fixture.Row);if(fault=="duplicateId")extra.ACCOUNT_NUMBER="2220";else extra.GL_ACCOUNT_ID="other";fixture.OverrideRows=[fixture.Row,extra];break;
            case "capacity":fixture.OverrideRows=Enumerable.Range(0,CompanyGLAccountReview.MaximumAccounts+1).Select(_=>(object)fixture.Row).ToArray();break;}
        fixture.OverrideRows ??=[fixture.Row];await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Read());fixture.NoWrites();
    }
    [Theory][InlineData("key")][InlineData("width")][InlineData("nullable")][InlineData("missing")][InlineData("type")][InlineData("duplicate")]
    public async Task UnconfirmedBeepSchemaStopsBeforeBusinessReads(string fault)
    {
        var fixture=new Fixture();fixture.ChangeSchema=schema=>{var field=schema.Fields.Single(value=>value.FieldName=="BOOK_ID");switch(fault){
            case "key":schema.Fields.Single(value=>value.FieldName=="GL_ACCOUNT_ID").IsKey=false;break;case "width":field.Size=field.Size1=1;break;
            case "nullable":field.AllowDBNull=false;break;case "missing":schema.Fields.Remove(field);break;case "type":field.Fieldtype=typeof(int).FullName;break;case "duplicate":schema.Fields.Add(field);break;}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());Assert.Equal(0,fixture.Reads);fixture.NoWrites();
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(5)][InlineData(7)][InlineData(8)]
    public async Task RevocationAtEveryBoundaryPublishesNoPrivateReview(int boundary)
    {var fixture=new Fixture{RevokeAt=boundary};await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Read());if(boundary<=2)Assert.Equal(0,fixture.Reads);fixture.NoWrites();}
    [Theory][InlineData("binding")][InlineData("source")][InlineData("account")][InlineData("balance")]
    public async Task ClosingBindingAndSourceChangesCannotPublish(string fault)
    {
        var fixture=new Fixture();fixture.OnRead=()=>{if(fixture.Reads!=2)return;switch(fault){case "binding":fixture.Native.Connection="changed";break;
            case "source":fixture.Native.Editor.Setup(editor=>editor.GetDataSource(It.IsAny<string>())).Returns(Mock.Of<IDataSource>());break;
            case "account":fixture.Row.ACCOUNT_NAME="changed";break;case "balance":fixture.Row.CURRENT_BALANCE=99m;break;}};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());fixture.NoWrites();
    }
    [Fact] public async Task CompanyReadRequiresOriginalProductionResolver()
    {var fixture=new Fixture(bound:false);await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Read());Assert.Equal(0,fixture.Reads);fixture.NoWrites();}
    [Theory][InlineData("number")][InlineData("all")][InlineData("type")][InlineData("balance")][InlineData("create")][InlineData("defaults")]
    public async Task SelectedCompanyCannotBorrowLegacyAccountsBalancesOrDefaults(string action)
    {
        var fixture=new Fixture();await Assert.ThrowsAsync<InvalidOperationException>(()=>Legacy(fixture.Service,action));Assert.Equal(0,fixture.Reads);fixture.NoWrites();
    }
    [Theory][InlineData("tenant")][InlineData("company")][InlineData("book")][InlineData("currency")][InlineData("emptyMarker")]
    public async Task CompanylessOwnerCannotReturnEvenPartiallyMarkedAccounts(string marker)
    {
        var fixture=new Fixture(bound:false);fixture.Row.OWNER_TENANT_ID=fixture.Row.OWNER_COMPANY_BA_ID=fixture.Row.BOOK_ID=fixture.Row.CURRENCY_CODE=null;
        switch(marker){case "tenant":fixture.Row.OWNER_TENANT_ID="tenant";break;case "company":fixture.Row.OWNER_COMPANY_BA_ID="company";break;
            case "book":fixture.Row.BOOK_ID="statutory";break;case "currency":fixture.Row.CURRENCY_CODE="KWD";break;case "emptyMarker":fixture.Row.OWNER_TENANT_ID="";break;}
        foreach(var action in new[]{"number","all","type","balance","create","defaults"})await Assert.ThrowsAsync<InvalidOperationException>(()=>Legacy(fixture.Service,action));fixture.NoWrites();
    }
    [Fact] public async Task StandaloneLegacyBalanceUsesActualIdAndDuplicateNumberIsRefused()
    {
        var fixture=new Fixture(bound:false);fixture.Row.OWNER_TENANT_ID=fixture.Row.OWNER_COMPANY_BA_ID=fixture.Row.BOOK_ID=fixture.Row.CURRENCY_CODE=null;
        fixture.Native.Rows.Add(new GL_ENTRY{GL_ENTRY_ID="entry",GL_ACCOUNT_ID="actual-account-id",DEBIT_AMOUNT=12m,CREDIT_AMOUNT=2m,ACTIVE_IND="Y"});
        Assert.Equal(10m,await fixture.Service.GetAccountBalanceAsync("1110"));var extra=(GL_ACCOUNT)Fixture.Clone(fixture.Row);extra.GL_ACCOUNT_ID="other";fixture.Native.Rows.Add(extra);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Service.GetAccountByNumberAsync("1110"));fixture.NoWrites();
    }
    private static async Task Legacy(GLAccountService service,string action)
    {switch(action){case "number":await service.GetAccountByNumberAsync("1110");break;case "all":await service.GetAllAccountsAsync();break;case "type":await service.GetAccountsByTypeAsync("ASSET");break;
        case "balance":await service.GetAccountBalanceAsync("1110");break;case "create":await service.CreateAccountAsync("1110","name","ASSET","DEBIT","reason","actor");break;case "defaults":await service.GenerateDefaultAccountsAsync("actor");break;}}
}
#endif
