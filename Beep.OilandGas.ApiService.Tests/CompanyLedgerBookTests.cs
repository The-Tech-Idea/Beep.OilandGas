#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyLedgerBookTests
{
    internal static readonly CompanyScope Company=new("tenant","company");
    internal sealed class Fixture
    {
        internal readonly CompanyGLAccountReviewTests.Fixture Reader=new();
        internal readonly GLAccountService Service;
        internal CompanyLedgerBookConfigurationSubmission Submission=null!;
        internal Action? OnInsert;
        internal bool FailInsert,CorruptInsert;
        internal Fixture()
        {
            Reader.Native.Rows.Clear();
            Reader.Native.Source.Setup(source=>source.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure requested,bool refresh)=>{var schema=new ClassCreator(Reader.Native.Editor.Object).ConvertToEntityStructure(typeof(COMPANY_LEDGER_BOOK));Reader.ChangeSchema?.Invoke(schema);return schema;});
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("COMPANY_LEDGER_BOOK")).ReturnsAsync(new List<string>{"COMPANY_LEDGER_BOOK_ID"});
            Service=new(Reader.Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,NullLogger<GLAccountService>.Instance,()=>Task.FromResult(Reader.Native.Connection));
            Reader.Native.Source.Setup(source=>source.InsertEntity("COMPANY_LEDGER_BOOK",It.IsAny<object>())).Returns((string table,object value)=>{Reader.Native.Writes++;if(FailInsert)return new ErrorsInfo{Flag=Errors.Failed};var row=JsonSerializer.Deserialize<COMPANY_LEDGER_BOOK>(JsonSerializer.Serialize(value))!;if(CorruptInsert)row.BOOK_NAME="changed";Reader.Native.Rows.Add(row);OnInsert?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        internal Task<CompanyLedgerBookReview> Read(string actor="administrator")=>Service.GetCompanyBooksAsync(Company,actor,Reader.Authority);
        internal async Task Prepare(){var review=await Read();Submission=new(Command(review),review);}
        internal Task<COMPANY_LEDGER_BOOK> Record()=>Service.ConfigureCompanyBookAsync(Submission,Reader.Authority);
    }
    internal static CompanyLedgerBookConfigurationCommand Command(CompanyLedgerBookReview review)=>new(Guid.NewGuid().ToString("N"),review.Company,review.ActorUserId,Guid.NewGuid().ToString("N"),"statutory","Recorded company book","KWD",3,"TO_EVEN","calendar-2026","Asia/Kuwait","Reviewed company configuration",review.Revision,new string('B',64));
    [Fact] public async Task ExactCommitRetryAndPublicFactsKeepOriginalProducerReceiptPrivate()
    {
        var fixture=new Fixture();await fixture.Prepare();var row=await fixture.Record();Assert.Equal(fixture.Submission.Command,CompanyLedgerBookConfigurationProof.Require(row).Command);Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(await fixture.Record()));Assert.Equal(1,fixture.Reader.Native.Writes);
        var review=await fixture.Read("accountant");var book=Assert.Single(review.Books);Assert.Equal(3,book.CurrencyDecimals);Assert.Equal("Asia/Kuwait",book.TimeZoneId);Assert.DoesNotContain("BindingRevision",JsonSerializer.Serialize(review));Assert.DoesNotContain("Reason",JsonSerializer.Serialize(review));review.RequireBook("statutory","KWD","calendar-2026");Assert.Throws<InvalidOperationException>(()=>review.RequireBook("statutory","USD","calendar-2026"));
        var recovered=await fixture.Service.GetCompanyBookConfigurationAsync(Company,"administrator",row.COMPANY_LEDGER_BOOK_ID,fixture.Reader.Authority);Assert.Equal(JsonSerializer.Serialize(row),JsonSerializer.Serialize(recovered));await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>fixture.Service.GetCompanyBookConfigurationAsync(Company,"accountant",row.COMPANY_LEDGER_BOOK_ID,fixture.Reader.Authority));
    }
    [Theory][InlineData("currency")][InlineData("precision")][InlineData("rounding")][InlineData("timezone")][InlineData("calendar")][InlineData("reason")][InlineData("source")]
    public async Task InvalidExplicitConfigurationNeverWrites(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();var command=fixture.Submission.Command;command=fault switch{"currency"=>command with{FunctionalCurrency="kwd"},"precision"=>command with{CurrencyDecimals=9},"rounding"=>command with{RoundingPolicy="DEFAULT"},"timezone"=>command with{TimeZoneId="unavailable/zone"},"calendar"=>command with{CalendarId=""},"reason"=>command with{Reason=""},_=>command with{SourceRevision=new string('C',64)}};fixture.Submission=fixture.Submission with{Command=command};await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record());Assert.Equal(0,fixture.Reader.Native.Writes);
    }
    [Theory][InlineData("insert")][InlineData("readback")][InlineData("authority")][InlineData("binding")]
    public async Task FailureRollsBackTheOriginalBook(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.FailInsert=fault=="insert";fixture.CorruptInsert=fault=="readback";fixture.OnInsert=()=>{if(fault=="authority")fixture.Reader.RevokeAt=fixture.Reader.AuthorityCalls+1;if(fault=="binding")fixture.Reader.Native.Connection="changed";};await Assert.ThrowsAnyAsync<Exception>(()=>fixture.Record());Assert.Empty(fixture.Reader.Native.Rows);Assert.Equal("rollback",fixture.Reader.Native.Transactions.Last());
    }
    [Fact] public async Task LostCommitAcknowledgementRecoversWithoutNewWritesOrAReview()
    {
        var fixture=new Fixture();await fixture.Prepare();fixture.Reader.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());fixture.Reader.Native.FailCommit=false;fixture.Submission=fixture.Submission with{Review=null};Assert.Equal(fixture.Submission.Command,CompanyLedgerBookConfigurationProof.Require(await fixture.Record()).Command);Assert.Equal(1,fixture.Reader.Native.Writes);
    }
    [Fact] public async Task EmptyRegistryCannotSupplyAMissingBookAndDuplicateCodesCannotBeConfigured()
    {
        var fixture=new Fixture();var empty=await fixture.Read();Assert.Empty(empty.Books);Assert.Throws<InvalidOperationException>(()=>empty.RequireBook("statutory","KWD","calendar-2026"));await fixture.Prepare();await fixture.Record();await fixture.Prepare();await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(1,fixture.Reader.Native.Writes);
    }
    [Theory][InlineData("borrowed")][InlineData("isolation")][InlineData("schema")]
    public async Task UnconfirmedNativeOwnershipAndSchemaNeverWrite(string fault)
    {
        var fixture=new Fixture();await fixture.Prepare();if(fault=="borrowed")fixture.Reader.Native.Borrowed=true;if(fault=="isolation")fixture.Reader.Native.Isolation=IsolationLevel.ReadCommitted;if(fault=="schema")fixture.Reader.ChangeSchema=schema=>schema.Fields.Remove(schema.Fields.Single(field=>field.FieldName=="CONFIGURATION_JSON"));await Assert.ThrowsAsync<InvalidOperationException>(()=>fixture.Record());Assert.Equal(0,fixture.Reader.Native.Writes);
    }
}
#endif
