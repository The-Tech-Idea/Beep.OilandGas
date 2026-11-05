#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Models.Data;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalPreparationHttpTests
{
    internal static readonly string[] Grants=CompanySalesJournalSourceHttpTests.Grants.Concat(["Accounting.PrepareSalesJournal"]).ToArray();
    private static ClaimsPrincipal User(string actor)=>new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"test"));
    internal static void Writes(CompanySalesJournalSourceTests.Fixture fixture)
    {
        fixture.Native.Source.Setup(value=>value.InsertEntity("JOURNAL_ENTRY",It.IsAny<object>())).Returns((string table,object value)=>{fixture.Native.Writes++;fixture.Native.Rows.Add(CompanyInventorySourceReview.Copy((JOURNAL_ENTRY)value));return new ErrorsInfo{Flag=Errors.Ok};});
        fixture.Native.Source.Setup(value=>value.InsertEntity("JOURNAL_ENTRY_LINE",It.IsAny<object>())).Returns((string table,object value)=>{fixture.Native.Writes++;fixture.Native.Rows.Add(CompanyInventorySourceReview.Copy((JOURNAL_ENTRY_LINE)value));return new ErrorsInfo{Flag=Errors.Ok};});
        fixture.Native.Source.Setup(value=>value.UpdateEntity("JOURNAL_ENTRY",It.IsAny<object>())).Returns((string table,object value)=>{fixture.Native.Writes++;fixture.Native.Rows.RemoveAll(row=>row is JOURNAL_ENTRY header && header.JOURNAL_ENTRY_ID==((JOURNAL_ENTRY)value).JOURNAL_ENTRY_ID);fixture.Native.Rows.Add(CompanyInventorySourceReview.Copy((JOURNAL_ENTRY)value));return new ErrorsInfo{Flag=Errors.Ok};});
    }
    [Fact] public async Task ActualApiClientAndCircuitRetainUncertainRequestThenRecoverOneDraftWithLiveSameBindingSourceReads()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalSourceTests.Fixture();await fixture.Prepare(taxAndCharges:true);Writes(fixture);await using var host=await CompanySalesJournalSourceHttpTests.Start(fixture);host.Context.Permissions=Grants;
        using var company=CompanyGLAccountHttpTests.Selected(actor:fixture.Current.ActorUserId,grants:Grants);var client=CompanyGLAccountHttpTests.Client(host.Client,company);using var state=new CompanySalesReceivableWorkspaceState(company);var owner=new object();state.AttachOwner(owner);state.Admit(User(fixture.Current.ActorUserId));
        await state.LoadJournalSourceAsync(fixture.Current.ReceivableId,"statutory","KWD","company-calendar",fixture.Date,fixture.Selection,client);Assert.True(state.CanPrepareJournal);var transactionReads=new List<string>();fixture.OnRead=table=>{if(table is "AR_INVOICE" or "GL_ACCOUNT" or "GL_ACCOUNT_MAPPING" or "COMPANY_LEDGER_PERIOD"){Assert.NotNull(((ITaskTransactionEvidence)fixture.Native.Source.Object).ActiveTransaction);transactionReads.Add(table);}};
        fixture.Native.FailCommit=true;var unavailable=await Assert.ThrowsAsync<OilGasApiException>(()=>state.PrepareJournalAsync("company-journal-001","Reviewed original journal source",client));Assert.Equal(HttpStatusCode.Conflict,unavailable.StatusCode);Assert.True(state.HasPendingJournal);var pending=state.PendingJournal!;var saved=JsonSerializer.Serialize(pending);Assert.Equal("REQUESTED",Assert.Single(fixture.Native.Rows.OfType<JOURNAL_ENTRY>()).STATUS);Assert.Empty(fixture.Native.Rows.OfType<JOURNAL_ENTRY_LINE>());
        state.DetachOwner(owner);company.Clear();Assert.Null(state.PendingJournal);Assert.True(state.HasPendingJournal);company.TrySelect(company.Version,fixture.Current.ActorUserId,new(fixture.Current.Company,["ConfiguredCompanyRole"],Grants));state.AttachOwner(new object());state.Admit(User(fixture.Current.ActorUserId));Assert.Equal(saved,JsonSerializer.Serialize(state.PendingJournal));
        fixture.Native.FailCommit=false;await state.SubmitJournalPreparationAsync(client);Assert.False(state.HasPendingJournal);Assert.NotNull(state.JournalPreparation);Assert.Equal("DRAFT",state.JournalPreparation!.Header.STATUS);Assert.Equal(4,state.JournalPreparation.Lines.Count);Assert.Equal(6,fixture.Native.Writes);
        foreach(var table in new[]{"AR_INVOICE","GL_ACCOUNT","GL_ACCOUNT_MAPPING","COMPANY_LEDGER_PERIOD"})Assert.Contains(table,transactionReads);Assert.DoesNotContain(fixture.Native.Rows,row=>row.GetType().Name is "GL_ENTRY" or "AR_PAYMENT");
        var recovered=await client.GetCompanySalesJournalPreparationAsync("statutory","KWD",pending.JournalId);Assert.NotNull(recovered);recovered.Require(pending);Assert.Equal(6,fixture.Native.Writes);
    }
    [Theory][InlineData("prepare-grant")][InlineData("field-write")][InlineData("binding")]
    public async Task MissingCurrentPrepareOrFieldWriteAuthorityNeverRetainsJournal(string fault)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalSourceTests.Fixture();await fixture.Prepare();Writes(fixture);await using var host=await CompanySalesJournalSourceHttpTests.Start(fixture);host.Context.Permissions=Grants;
        using var company=CompanyGLAccountHttpTests.Selected(actor:fixture.Current.ActorUserId,grants:Grants);var client=CompanyGLAccountHttpTests.Client(host.Client,company);var source=await client.GetCompanySalesJournalSourceAsync(fixture.Current.ReceivableId,"statutory","KWD","company-calendar",fixture.Date,fixture.Selection);
        var command=new CompanySalesJournalPreparation(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),source,"explicit-company-journal","Reviewed source",source.Lines.Select(_=>Guid.NewGuid().ToString("N")).ToArray());
        if(fault=="prepare-grant")host.Context.Permissions=CompanySalesJournalSourceHttpTests.Grants;if(fault=="field-write")fixture.Receivable.Data.Invoice.Data.Assets.Setup(value=>value.CheckAssetAccessAsync(fixture.Current.ActorUserId,fixture.Current.FieldId,"FIELD","WRITE",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});if(fault=="binding")host.Context.PolicyBinding=new string('C',64);
        var rejected=await Assert.ThrowsAsync<OilGasApiException>(()=>client.PrepareCompanySalesJournalAsync(command));Assert.Equal(HttpStatusCode.Forbidden,rejected.StatusCode);Assert.Empty(fixture.Native.Rows.OfType<JOURNAL_ENTRY>());Assert.Equal(0,fixture.Native.Writes);
    }
}
#endif
