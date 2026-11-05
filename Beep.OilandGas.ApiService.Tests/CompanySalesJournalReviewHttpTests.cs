using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Models.Data;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalReviewHttpTests
{
    [Fact] public async Task ActualIndependentReviewerApiUsesPeerAuthorityAndFrozenMakerViewThenRetainsAndRecoversExactDisposition()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalSourceTests.Fixture();await fixture.Prepare();CompanySalesJournalPreparationHttpTests.Writes(fixture);
        var source=await fixture.Read();var preparation=new CompanySalesJournalPreparation(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),source,"explicit-reviewed-journal","Original approved source",source.Lines.Select(_=>Guid.NewGuid().ToString("N")).ToArray());var draft=await fixture.Journal.PrepareCompanySalesJournalAsync(preparation,fixture.Guard,fixture.Read);fixture.Native.Writes=0;fixture.Native.Transactions.Clear();
        var types=fixture.Native.Rows.Select(row=>row.GetType()).Concat([typeof(SALES_JOURNAL_REVIEW_INTENT)]).Distinct().ToDictionary(type=>type==typeof(GLAccountMapping)?"GL_ACCOUNT_MAPPING":type.Name,type=>type);
        fixture.Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool refresh)=>new ClassCreator(fixture.Native.Editor.Object).ConvertToEntityStructure(types[request.EntityName]));
        fixture.Native.Source.Setup(value=>value.InsertEntity("SALES_JOURNAL_REVIEW_INTENT",It.IsAny<object>())).Returns((string table,object value)=>{fixture.Native.Writes++;fixture.Native.Rows.Add(CompanyInventorySourceReview.Copy((SALES_JOURNAL_REVIEW_INTENT)value));return new ErrorsInfo{Flag=Errors.Ok};});
        await using var host=await CompanySalesJournalSourceHttpTests.Start(fixture);host.Context.Permissions=CompanySalesJournalSourceHttpTests.Grants.Concat(["Accounting.ReviewSalesJournal","Accounting.RecordSalesJournalDecision"]).ToArray();host.Context.Actor="independent-reviewer";host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor","independent-reviewer");
        var route="api/accounting/traditional/company-receivables/journal-reviews/"+preparation.JournalId;var response=await host.Client.GetAsync(route+"/source?book=statutory&currency=KWD");Assert.Equal(HttpStatusCode.OK,response.StatusCode);var review=JsonSerializer.Deserialize<CompanySalesJournalReviewSource>(await response.Content.ReadAsStringAsync(),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;review.Require();Assert.Equal("independent-reviewer",review.ActorUserId);Assert.Equal(source.ActorUserId,review.Current.ActorUserId);
        using var company=CompanyGLAccountHttpTests.Selected(actor:"independent-reviewer",grants:host.Context.Permissions);var client=CompanyGLAccountHttpTests.Client(host.Client,company);
        var command=new CompanySalesJournalReviewCommand(Guid.NewGuid().ToString("N"),review,"APPROVE","CONFIRMED","Exact balanced native lines","CONFIRMED","Original approved source/configuration/period","Explicit independent controller review");
        var grants=host.Context.Permissions;
        foreach(var missing in new[]{"Accounting.ReviewSalesJournal","Accounting.RecordSalesJournalDecision","Accounting.ReviewSalesJournalSource"})
        {
            host.Context.Permissions=grants.Where(value=>value!=missing).ToArray();using var deniedBody=new StringContent(JsonSerializer.Serialize(command),System.Text.Encoding.UTF8,"application/json");Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsync("api/accounting/traditional/company-receivables/journal-reviews",deniedBody)).StatusCode);Assert.Equal(0,fixture.Native.Writes);
        }
        host.Context.Permissions=grants;fixture.Receivable.Data.Invoice.Data.Assets.Setup(value=>value.CheckAssetAccessAsync("independent-reviewer",fixture.Current.FieldId,"FIELD","WRITE",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
        using(var deniedBody=new StringContent(JsonSerializer.Serialize(command),System.Text.Encoding.UTF8,"application/json")){Assert.Equal(HttpStatusCode.Forbidden,(await host.Client.PostAsync("api/accounting/traditional/company-receivables/journal-reviews",deniedBody)).StatusCode);Assert.Equal(0,fixture.Native.Writes);}
        fixture.Receivable.Data.Invoice.Data.Assets.Setup(value=>value.CheckAssetAccessAsync("independent-reviewer",fixture.Current.FieldId,"FIELD","WRITE",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        using var body=new StringContent(JsonSerializer.Serialize(command),System.Text.Encoding.UTF8,"application/json");fixture.Native.FailCommit=true;var unknown=await host.Client.PostAsync("api/accounting/traditional/company-receivables/journal-reviews",body);Assert.Equal(HttpStatusCode.Conflict,unknown.StatusCode);Assert.Equal("DRAFT",fixture.Native.Rows.OfType<JOURNAL_ENTRY>().Single().STATUS);Assert.Single(fixture.Native.Rows.OfType<SALES_JOURNAL_REVIEW_INTENT>());
        fixture.Native.FailCommit=false;var recoveryResponse=await host.Client.GetAsync(route+"/recovery?book=statutory&currency=KWD");Assert.Equal(HttpStatusCode.OK,recoveryResponse.StatusCode);var recovery=JsonSerializer.Deserialize<CompanySalesJournalReviewRecovery>(await recoveryResponse.Content.ReadAsStringAsync(),new JsonSerializerOptions{PropertyNameCaseInsensitive=true})!;recovery.Require();Assert.Null(recovery.Outcome);
        var outcome=await client.RecordCompanySalesJournalDecisionAsync(command);outcome.Require(preparation);Assert.Equal("APPROVED",outcome.Header.STATUS);Assert.Equal("independent-reviewer",outcome.Header.ROW_CHANGED_BY);Assert.Equal(2,fixture.Native.Writes);Assert.Equal(JsonSerializer.Serialize(draft.Lines),JsonSerializer.Serialize(outcome.Lines));
        var confirmed=await client.GetCompanySalesJournalReviewRecoveryAsync("statutory","KWD",preparation.JournalId);confirmed.Require();Assert.Equal(JsonSerializer.Serialize(outcome),JsonSerializer.Serialize(confirmed.Outcome));
    }
}
#endif
