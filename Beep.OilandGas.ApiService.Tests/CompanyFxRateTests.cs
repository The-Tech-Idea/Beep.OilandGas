using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Moq;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
using TheTechIdea.Beep;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyFxRateTests
{
    internal sealed class Fixture
    {
        internal readonly CompanyLedgerBookTests.Fixture Books=new();
        internal CompanyFxRateConfigurationSubmission Submission=null!;
        internal GLAccountService Service=null!;
        internal Action? OnInsert;
        internal bool Corrupt,Fail,ProviderScale;
        internal readonly DateTime Date=new(2026,10,12);
        internal async Task Prepare(int precision=3,string rounding="TO_EVEN")
        {
            await Books.Prepare();Books.Submission=Books.Submission with{Command=Books.Submission.Command with{CurrencyDecimals=precision,RoundingPolicy=rounding}};await Books.Record();Books.Reader.Native.Writes=0;Books.Reader.Native.Transactions.Clear();
            Books.Reader.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>{var schema=new ClassCreator(Books.Reader.Native.Editor.Object).ConvertToEntityStructure(request.EntityName=="FX_RATE"?typeof(FX_RATE):typeof(COMPANY_LEDGER_BOOK));Books.Reader.ChangeSchema?.Invoke(schema);return schema;});
            Books.Reader.Native.Source.Setup(source=>source.GetEntityAsync(It.IsAny<string>(),It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{Books.Reader.Reads++;Books.Reader.OnRead?.Invoke();return Books.Reader.NullRows?null!:Books.Reader.OverrideRows ?? Books.Reader.Native.Rows.Where(row=>row.GetType().Name==table && filters.All(filter=>Matches(row,filter))).Select(CompanyGLAccountReviewTests.Fixture.Clone).ToArray();});
            Books.Reader.Native.Source.Setup(source=>source.InsertEntity("FX_RATE",It.IsAny<object>())).Returns((string table,object value)=>{Books.Reader.Native.Writes++;if(Fail)return new ErrorsInfo{Flag=Errors.Failed};var row=JsonSerializer.Deserialize<FX_RATE>(JsonSerializer.Serialize(value))!;if(ProviderScale)row.RATE=decimal.Parse(row.RATE!.Value.ToString("F12",System.Globalization.CultureInfo.InvariantCulture),System.Globalization.CultureInfo.InvariantCulture);if(Corrupt)row.RATE=99;Books.Reader.Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table)=>new List<string>{table+"_ID"});Service=new(Books.Reader.Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,NullLogger<GLAccountService>.Instance,()=>Task.FromResult(Books.Reader.Native.Connection));var review=await Read();Submission=new(Command(review),review);
        }
        internal Task<CompanyFxRateReview> Read(string actor="administrator")=>Service.GetCompanyFxRatesAsync(CompanyLedgerBookTests.Company,actor,"statutory","USD",Date,Books.Reader.Authority);
        internal Task<FX_RATE> Record()=>Service.ConfigureCompanyFxRateAsync(Submission,Books.Reader.Authority);
    }
    internal static CompanyFxRateConfigurationCommand Command(CompanyFxRateReview review)=>new(Guid.NewGuid().ToString("N"),review.Books.Company,review.Books.ActorUserId,Guid.NewGuid().ToString("N"),review.Books.Books.Single(),review.SourceCurrency,"rate-2026-10-12",0.30675m,5,"TARGET_PER_SOURCE",review.ReviewDate,review.ReviewDate,review.ReviewDate,new DateTime(2026,10,12,0,0,0,DateTimeKind.Utc),"Recorded provider","Original quote reference","Controller reviewed quote authority","LARGEST_REMAINDER","Explicit company rate configuration",review.Revision,new string('B',64));
    internal static bool Matches(object row,AppFilter filter)
    {
        var value=row.GetType().GetProperty(filter.FieldName)?.GetValue(row);if(filter.Operator=="=")return value?.ToString()==filter.FilterValue;
        if(value is not DateTime date || !DateTime.TryParseExact(filter.FilterValue,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out var boundary))return false;
        return filter.Operator switch{"<="=>date<=boundary,">="=>date>=boundary,_=>false};
    }
    [Fact] public async Task NativeRateCommitsOncePreservesDirectedEvidenceAndPrivateProducerRecovery()
    {
        var f=new Fixture();await f.Prepare();var row=await f.Record();Assert.Equal(f.Submission.Command,CompanyFxRateProof.Require(row).Command);Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(await f.Record()));Assert.Equal(1,f.Books.Reader.Native.Writes);var publicReview=await f.Read("accountant");Assert.Equal(0.30675m,publicReview.RequireApplicable().Rate);Assert.DoesNotContain("BindingRevision",JsonSerializer.Serialize(publicReview));Assert.DoesNotContain("CONFIGURATION_JSON",JsonSerializer.Serialize(publicReview));var recovered=await f.Service.GetCompanyFxRateConfigurationAsync(CompanyLedgerBookTests.Company,"administrator",row.FX_RATE_ID,f.Books.Reader.Authority);Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(recovered));await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Service.GetCompanyFxRateConfigurationAsync(CompanyLedgerBookTests.Company,"accountant",row.FX_RATE_ID,f.Books.Reader.Authority));
    }
    [Theory][InlineData("rate")][InlineData("precision")][InlineData("direction")][InlineData("provider")][InlineData("authority")][InlineData("allocation")][InlineData("dates")][InlineData("source")]
    public async Task InvalidExplicitQuoteNeverWrites(string fault)
    {
        var f=new Fixture();await f.Prepare();var c=f.Submission.Command;c=fault switch{"rate"=>c with{Rate=0},"precision"=>c with{RateDecimals=2},"direction"=>c with{Direction="SOURCE_PER_TARGET"},"provider"=>c with{Provider=""},"authority"=>c with{AuthorityReference=""},"allocation"=>c with{AllocationPolicy="DEFAULT"},"dates"=>c with{EffectiveThrough=c.EffectiveFrom.AddDays(-1)},_=>c with{SourceRevision=new string('C',64)}};f.Submission=f.Submission with{Command=c};await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Equal(0,f.Books.Reader.Native.Writes);
    }
    [Theory][InlineData("insert")][InlineData("readback")][InlineData("authority")][InlineData("binding")][InlineData("book")]
    public async Task FailedOrChangedClosingEvidenceRollsBackOnlyTheOriginalRate(string fault)
    {
        var f=new Fixture();await f.Prepare();f.Fail=fault=="insert";f.Corrupt=fault=="readback";f.OnInsert=()=>{if(fault=="authority")f.Books.Reader.RevokeAt=f.Books.Reader.AuthorityCalls+1;if(fault=="binding")f.Books.Reader.Native.Connection="changed";if(fault=="book")f.Books.Reader.Native.Rows.OfType<COMPANY_LEDGER_BOOK>().Single().BOOK_NAME="changed";};await Assert.ThrowsAnyAsync<Exception>(()=>f.Record());Assert.Empty(f.Books.Reader.Native.Rows.OfType<FX_RATE>());Assert.Single(f.Books.Reader.Native.Rows.OfType<COMPANY_LEDGER_BOOK>());Assert.Equal("rollback",f.Books.Reader.Native.Transactions.Last());
    }
    [Fact] public async Task UnknownCommitAndLaterRegistryChangeReconcileOnlyExactOriginalBody()
    {
        var f=new Fixture();await f.Prepare();f.Books.Reader.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());f.Books.Reader.Native.FailCommit=false;f.Submission=f.Submission with{Review=null};Assert.Equal(f.Submission.Command,CompanyFxRateProof.Require(await f.Record()).Command);Assert.Equal(1,f.Books.Reader.Native.Writes);f.Submission=f.Submission with{Command=f.Submission.Command with{Rate=.3m}};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(1,f.Books.Reader.Native.Writes);
    }
    [Fact] public async Task EmptyExpiredDuplicateAndOverlappingQuotesCannotSupplyOrCreateAnotherRate()
    {
        var f=new Fixture();await f.Prepare();Assert.Throws<InvalidOperationException>(()=>f.Submission.Review!.RequireApplicable());await f.Record();var current=await f.Read();var next=Command(current);f.Submission=new(next,current);await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());var expired=await f.Service.GetCompanyFxRatesAsync(CompanyLedgerBookTests.Company,"administrator","statutory","USD",f.Date.AddDays(1),f.Books.Reader.Authority);Assert.Throws<InvalidOperationException>(()=>expired.RequireApplicable());Assert.Equal(1,f.Books.Reader.Native.Writes);
    }
    [Theory][InlineData("borrowed")][InlineData("isolation")][InlineData("schema")]
    public async Task MissingOwnedTransactionOrReceiptSchemaCannotInsert(string fault)
    {
        var f=new Fixture();await f.Prepare();if(fault=="borrowed")f.Books.Reader.Native.Borrowed=true;if(fault=="isolation")f.Books.Reader.Native.Isolation=IsolationLevel.ReadCommitted;if(fault=="schema")f.Books.Reader.ChangeSchema=schema=>{if(schema.EntityName=="FX_RATE")schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="CONFIGURATION_JSON"));};await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Record());Assert.Equal(0,f.Books.Reader.Native.Writes);
    }
    [Theory][InlineData("TO_EVEN","0.002")][InlineData("AWAY_FROM_ZERO","0.003")]
    public async Task ExactRationalConversionRoundsTotalOnceAndAllocatesOnlyDeclaredMinorUnits(string rounding,string expectedText)
    {
        var f=new Fixture();await f.Prepare(rounding:rounding);f.Submission=f.Submission with{Command=f.Submission.Command with{Rate=.5m,RateDecimals=1}};await f.Record();var rates=await f.Read();var conversion=CompanyFxConversion.Create(rates,3,.005m,.003m,.001m,.001m);conversion.Require(rates,3,.005m,.003m,.001m,.001m);Assert.Equal(decimal.Parse(expectedText,System.Globalization.CultureInfo.InvariantCulture),conversion.TargetTotal);Assert.Equal(conversion.TargetTotal,conversion.Components.Sum(value=>value.TargetAmount));Assert.All(conversion.Components,value=>Assert.InRange(value.AllocatedMinorUnits,0,1));Assert.Equal("SALES_REVENUE",conversion.Components[0].Purpose);Assert.Equal(.002m,conversion.Components[0].TargetAmount);Assert.Throws<InvalidOperationException>(()=>(conversion with{TargetTotal=.1m}).Require(rates,3,.005m,.003m,.001m,.001m));
    }
    [Fact] public async Task ZeroFunctionalTotalsUnsupportedSourceMinorUnitsAndDecimalOverflowAreRefused()
    {
        var f=new Fixture();await f.Prepare();await f.Record();var rates=await f.Read();Assert.Throws<InvalidOperationException>(()=>CompanyFxConversion.Create(rates,3,.001m,.001m,0,0));Assert.Throws<InvalidOperationException>(()=>CompanyFxConversion.Create(rates,2,.005m,.005m,0,0));
        var huge=new Fixture();await huge.Prepare();huge.Submission=huge.Submission with{Command=huge.Submission.Command with{Rate=decimal.MaxValue,RateDecimals=0}};await huge.Record();var overflow=await huge.Read();Assert.Throws<InvalidOperationException>(()=>CompanyFxConversion.Create(overflow,0,2,1,1,0));Assert.Equal(1,huge.Books.Reader.Native.Writes);
    }
    [Fact] public async Task ProviderTrailingZerosPreserveExactValueAndOriginalPrecisionWithoutRoundingOrAnotherWrite()
    {
        var f=new Fixture();await f.Prepare();f.ProviderScale=true;var row=await f.Record();Assert.Equal(f.Submission.Command,CompanyFxRateProof.Require(row).Command);Assert.Equal(.30675m,(await f.Read()).RequireApplicable().Rate);Assert.Equal(5,(await f.Read()).RequireApplicable().RateDecimals);await f.Record();Assert.Equal(1,f.Books.Reader.Native.Writes);f.Books.Reader.Native.Rows.OfType<FX_RATE>().Single().RATE=.30676m;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Read());
        var tiny=new Fixture();await tiny.Prepare();tiny.Submission=tiny.Submission with{Command=tiny.Submission.Command with{Rate=.000000000001m,RateDecimals=12}};await tiny.Record();Assert.Equal(.000000000001m,(await tiny.Read()).RequireApplicable().Rate);
    }
}
#endif
