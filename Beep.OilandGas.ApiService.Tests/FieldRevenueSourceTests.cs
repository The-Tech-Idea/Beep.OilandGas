using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using System.Security.Claims;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Pages.PPDM39.Accounting;
using Beep.OilandGas.Web.Components.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests
{
public sealed class FieldRevenueSourceTests
{
    private sealed class DeliveryCorrectionFixture
    {
        public readonly Fixture Data=new();
        public readonly RevenueDeliveryAcceptancePreparation Original;
        public readonly RevenueDeliveryAcceptanceRecord Previous;
        public bool Revoked,Corrupt,FailWrite;
        public string Binding="binding";
        public Action? AfterWrite;
        public DeliveryCorrectionFixture(bool legacy=false)
        {
            Data.Native.Connection="production";var original=DeliveryPreparation();Original=legacy?original with{Facts=original.Facts with{AcceptedQuantity=null,AcceptedQuantityUnit=null}}:original;var source=Original.Source;
            Data.Native.Rows.RemoveAll(row=>row is REVENUE_TRANSACTION or SALES_CONTRACT);Data.Native.Rows.AddRange(source.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));Data.Native.Rows.Add(CompanyInventorySourceReview.Copy(source.Source.Contract));Data.Native.Rows.Add(CompanyInventorySourceReview.Copy(source.Allocation));
            var record=RevenueDeliveryAcceptanceRecord.Create(Original,new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc));Previous=record.Decide(new(Guid.NewGuid().ToString("N"),Original.Company,"delivery-reviewer",record.Revision,RevenueDeliveryAcceptanceDecision.Reject,"Original receiving evidence rejected"),source,new DateTime(2026,2,3,0,0,0,DateTimeKind.Utc));
            Data.Native.Rows.Add(DeliveryRecordedReview(source,Previous).Source.Ticket);
            Data.Native.Source.Setup(value=>value.UpdateEntity("RUN_TICKET",It.IsAny<object>())).Returns((string table,object value)=>{++Data.Native.Writes;if(FailWrite)return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((RUN_TICKET)value);if(Corrupt)row.NET_VOLUME=8;Data.Native.Rows.RemoveAll(row=>row is RUN_TICKET);Data.Native.Rows.Add(row);AfterWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};});
        }
        public Task Guard(){if(Revoked)throw new UnauthorizedAccessException();return Task.CompletedTask;}
        public async Task<RevenueDeliverySourceReview> Read()
        {
            var field=Original.Source.Source.Source;var rows=await Data.Service.GetRecordedFieldRevenueAsync(field.FieldId,field.From,field.Through,Data.Native.Connection);var contract=await Data.Service.GetRecordedSalesContractAsync("contract",Original.Company,Data.Native.Connection);
            var sales=RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(Original.Company,Original.ActorUserId,field.FieldId,field.From,field.Through,Binding,rows),Original.Source.Source.TransactionId,contract);
            return await Data.Service.GetRecordedRevenueDeliveryAsync(sales,Data.Native.Connection);
        }
        public async Task<RevenueDeliveryAcceptanceCorrection> Command(RevenueDeliveryAcceptanceRecord? previous=null,int version=1)
        {
            var current=await Read();var proof=RevenueDeliveryAcceptanceProof.RequireEnvelope(Original.Company,current.Ticket);previous ??=proof.Record;
            var normalized=RevenueDeliveryAcceptanceCorrection.NormalizeSource(current,previous);var facts=Original.Facts with{AcceptedQuantity=4m,AcceptedQuantityUnit="BBL",EvidenceReference="corrected-evidence-"+version,Notes="Receiving facts corrected after independent rejection "+version};
            var preparation=new RevenueDeliveryAcceptancePreparation(Guid.NewGuid().ToString("N"),Original.Company,Original.ActorUserId,normalized,facts);
            return new(previous,proof.Revision,preparation,"Correct the rejected receiving evidence with reviewed facts "+version);
        }
    }
    [Theory][InlineData("historical")][InlineData("current")][InlineData("unknown")][InlineData("company")][InlineData("tenant")][InlineData("role")][InlineData("grant")][InlineData("asset")][InlineData("closing-binding")][InlineData("closing-role")][InlineData("closing-grant")][InlineData("closing-native")][InlineData("source")][InlineData("native")][InlineData("history")][InlineData("schema")]
    public async Task DeliveryCorrectionHistoryApiReopensExactPreservedVersionWithCurrentAuthorityAndNoWrites(string mode)
    {
        var f=new DeliveryCorrectionFixture();var command=await f.Command();var prepared=await f.Data.Service.CorrectDeliveryAcceptanceAsync(command,"production",f.Read,f.Guard);var writes=f.Data.Native.Writes;
        var original=f.Original.Source.Source.Source;const string actor="history-reader";var captures=0;var ticketReads=0;
        var context=new Mock<ICompanyAccountingReadContext>();context.Setup(value=>value.CaptureAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(()=>{
            var closing=++captures>=3;return(new RepositoryCompanyAccess(mode=="company"?new("tenant","other"):mode=="tenant"?new("other","company"):f.Original.Company,
                mode=="role" || closing && mode=="closing-role"?["OtherRole"]:["ConfiguredCompanyRole"],mode=="grant" || closing && mode=="closing-grant"?[]:["Accounting.View"]),closing && mode=="closing-binding"?"changed":original.BindingRevision,actor);
        });
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(actor,"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=mode!="asset"});
        f.Data.Native.Source.Setup(value=>value.GetEntityAsync("RUN_TICKET",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
            ++ticketReads;var rows=f.Data.Native.Rows.OfType<RUN_TICKET>().Where(row=>filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(CompanyInventorySourceReview.Copy).Cast<object>().ToArray();
            if(mode=="closing-native" && ticketReads==3)f.Data.Native.Rows.OfType<RUN_TICKET>().Single().NET_VOLUME=99;return rows;
        });
        if(mode=="source")f.Data.Native.Rows.OfType<REVENUE_TRANSACTION>().Single().GROSS_REVENUE=99;if(mode=="native")f.Data.Native.Rows.OfType<RUN_TICKET>().Single().NET_VOLUME=99;f.Data.MissingDeliverySchema=mode=="schema";
        if(mode=="history"){var row=f.Data.Native.Rows.OfType<RUN_TICKET>().Single();var proof=RevenueDeliveryAcceptanceProof.RequireEnvelope(f.Original.Company,row);row.DELIVERY_WORKFLOW_JSON=JsonSerializer.Serialize(proof with{PriorVersions=[]});}
        var reads=new CompanyFieldRevenueReads(f.Data.Service,context.Object,assets.Object,()=>Task.FromResult(f.Data.Native.Connection));var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"app"));
        var revision=mode=="current"?CompanyAccountingSourceTypes.DeliveryPreparationRevision(prepared):mode=="unknown"?new string('a',64):CompanyAccountingSourceTypes.DeliveryPreparationRevision(f.Previous);
        if(mode is "historical" or "current")
        {
            var review=await reads.ReadProcessDeliveryAsync(user,"ticket",revision);review.Require();Assert.Equal(mode=="historical",review.IsHistorical);Assert.Equal(actor,review.Source.Source.Source.ActorUserId);Assert.Equal(JsonSerializer.Serialize(mode=="historical"?f.Previous:prepared),JsonSerializer.Serialize(review.Recorded));Assert.Null(review.ApprovedAcceptedQuantity);
            if(review.CurrentSource is {} current){Assert.Equal(prepared.Revision,RevenueDeliveryAcceptanceProof.Require(f.Original.Company,current.Ticket).Revision);current.Ticket.NET_VOLUME=999;Assert.Equal(7m,f.Data.Native.Rows.OfType<RUN_TICKET>().Single().NET_VOLUME);}
        }
        else{var failure=await Assert.ThrowsAnyAsync<Exception>(()=>reads.ReadProcessDeliveryAsync(user,"ticket",revision));Assert.False(failure is Xunit.Sdk.XunitException);}
        Assert.Equal(writes,f.Data.Native.Writes);if(mode is "role" or "grant")Assert.Equal(0,ticketReads);
    }
    [Theory][InlineData("valid")][InlineData("current-version")][InlineData("old-record")][InlineData("current-native")][InlineData("current-source")][InlineData("actor")][InlineData("absent-proof")][InlineData("replay")][InlineData("constructors")]
    public async Task DeliveryCorrectionHistoricalCanonicalReviewRequiresCompleteCurrentEvidenceAndCannotConfirmCommands(string mode)
    {
        var f=new DeliveryCorrectionFixture();var command=await f.Command();await f.Data.Service.CorrectDeliveryAcceptanceAsync(command,"production",f.Read,f.Guard);var current=await f.Read();
        var historical=RevenueDeliveryAcceptanceProof.HistoricalReview(f.Original.Company,f.Original.ActorUserId,current,CompanyAccountingSourceTypes.DeliveryPreparationRevision(f.Previous)) with{CurrentSource=CompanyInventorySourceReview.Copy(current)};
        if(mode=="constructors")
        {
            Assert.NotNull(typeof(RevenueDeliveryAcceptanceReadReview).GetConstructor([typeof(RevenueDeliverySourceReview),typeof(RevenueDeliveryAcceptanceRecord)]));Assert.NotNull(typeof(CompanyDeliveryProcessSourceReview).GetConstructor([typeof(CompanyScope),typeof(string),typeof(string),typeof(string),typeof(string),typeof(string),typeof(RevenueDeliveryAcceptanceReadReview)]));
            var live=new RevenueDeliveryAcceptanceReadReview(current,RevenueDeliveryAcceptanceProof.Require(f.Original.Company,current.Ticket));Assert.DoesNotContain("CurrentSource",JsonSerializer.Serialize(live));Assert.DoesNotContain("IsHistorical",JsonSerializer.Serialize(live));live.Require();return;
        }
        if(mode=="current-version")historical=new(current,RevenueDeliveryAcceptanceProof.Require(f.Original.Company,current.Ticket)){CurrentSource=current};
        if(mode=="old-record")historical=historical with{Recorded=historical.Recorded! with{Revision=new string('a',64)}};
        if(mode=="current-native")historical.CurrentSource!.Ticket.NET_VOLUME=999;
        if(mode=="current-source"){var field=current.Source.Source;var rows=CompanyInventorySourceReview.Copy(field.Rows);rows[0].GROSS_REVENUE=999;var changed=FieldRevenueSourceReview.Create(field.Company,field.ActorUserId,field.FieldId,field.From,field.Through,field.BindingRevision,rows);historical=historical with{CurrentSource=RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(changed,current.Source.TransactionId,current.Source.Contract),current.Allocation,current.Ticket)};}
        if(mode=="actor"){var field=current.Source.Source;var changed=FieldRevenueSourceReview.Create(field.Company,"other-reader",field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows);historical=historical with{CurrentSource=RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(changed,current.Source.TransactionId,current.Source.Contract),current.Allocation,current.Ticket)};}
        if(mode=="absent-proof"){var row=CompanyInventorySourceReview.Copy(historical.Source.Ticket);row.DELIVERY_WORKFLOW_JSON=null;historical=historical with{Source=RevenueDeliverySourceReview.Create(historical.Source.Source,historical.Source.Allocation,row),Recorded=null};}
        if(mode=="replay"){historical.Require();Assert.Throws<InvalidOperationException>(()=>new RevenueDeliveryAcceptanceIntent(f.Original,null).RequireCurrent(historical));return;}
        if(mode=="valid"){historical.Require();var process=new CompanyDeliveryProcessSourceReview(f.Original.Company,f.Original.ActorUserId,"process","ticket",CompanyAccountingSourceTypes.DeliveryPreparationRevision(f.Previous),new string('a',64),historical);process.Require(f.Original.Company,f.Original.ActorUserId,"process");Assert.True(process.IsHistorical);Assert.Null(historical.ApprovedAcceptedQuantity);}
        else Assert.ThrowsAny<Exception>(historical.Require);
    }
    [Theory][InlineData("valid")][InlineData("retry")][InlineData("uncertain")][InlineData("legacy")][InlineData("source")][InlineData("contract")][InlineData("binding")][InlineData("quantity")][InlineData("unit")][InlineData("reason")][InlineData("predecessor")][InlineData("record")][InlineData("owner")][InlineData("approved")][InlineData("undecided")][InlineData("same-facts")][InlineData("intent")][InlineData("schema")][InlineData("write")][InlineData("readback")][InlineData("revoke")][InlineData("authority")][InlineData("borrowed")][InlineData("isolation")][InlineData("datasource")]
    public async Task DeliveryCorrectionNativeOwnerPreservesRejectedPredecessorAndRequiresOriginalMakerSourceAndOwnedWrite(string mode)
    {
        var f=new DeliveryCorrectionFixture(mode=="legacy");var correction=await f.Command();var before=JsonSerializer.Serialize(f.Data.Native.Rows.OfType<RUN_TICKET>().Single());
        if(mode=="source")f.Data.Native.Rows.OfType<REVENUE_TRANSACTION>().Single().GROSS_REVENUE=999;
        if(mode=="contract")f.Data.Native.Rows.OfType<SALES_CONTRACT>().Single().BASE_PRICE=99;
        if(mode=="binding")f.Binding="changed";
        if(mode=="quantity")correction=correction with{Preparation=correction.Preparation with{Facts=correction.Preparation.Facts with{AcceptedQuantity=8}}};
        if(mode=="unit")correction=correction with{Preparation=correction.Preparation with{Facts=correction.Preparation.Facts with{AcceptedQuantityUnit="M3"}}};
        if(mode=="reason")correction=correction with{Reason=" "};if(mode=="predecessor")correction=correction with{PreviousNativeRevision=new string('a',64)};
        if(mode=="record")correction=correction with{Previous=correction.Previous with{Revision=new string('a',64)}};
        if(mode=="owner")correction=correction with{Preparation=correction.Preparation with{ActorUserId="other-preparer"}};
        if(mode=="same-facts")correction=correction with{Preparation=correction.Preparation with{Facts=correction.Previous.Preparation.Facts}};
        if(mode=="intent")correction=correction with{Preparation=correction.Preparation with{IntentId=correction.Previous.Preparation.IntentId}};
        if(mode is "approved" or "undecided")
        {
            var previous=RevenueDeliveryAcceptanceRecord.Create(f.Original,f.Previous.PreparedAtUtc);if(mode=="approved")previous=previous.Decide(new(Guid.NewGuid().ToString("N"),f.Original.Company,"delivery-reviewer",previous.Revision,RevenueDeliveryAcceptanceDecision.Approve,"Already approved receiving evidence"),f.Original.Source,f.Previous.DecidedAtUtc!.Value);correction=correction with{Previous=previous};
        }
        f.Data.MissingDeliverySchema=mode=="schema";f.FailWrite=mode=="write";f.Corrupt=mode=="readback";f.Revoked=mode=="authority";f.Data.Native.Borrowed=mode=="borrowed";if(mode=="isolation")f.Data.Native.Isolation=System.Data.IsolationLevel.ReadCommitted;
        if(mode=="revoke")f.AfterWrite=()=>f.Revoked=true;
        if(mode=="datasource")f.AfterWrite=()=>f.Data.Native.Editor.Setup(value=>value.GetDataSource("production")).Returns(Mock.Of<IDataSource>());
        Task<RevenueDeliveryAcceptanceRecord> Send()=>f.Data.Service.CorrectDeliveryAcceptanceAsync(correction,"production",f.Read,f.Guard);
        if(mode is not ("valid" or "retry" or "uncertain" or "legacy"))
        {await Assert.ThrowsAnyAsync<Exception>(Send);Assert.Equal(before,JsonSerializer.Serialize(f.Data.Native.Rows.OfType<RUN_TICKET>().Single()));Assert.DoesNotContain("commit",f.Data.Native.Transactions);return;}
        if(mode=="uncertain"){f.Data.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(Send);Assert.Equal(1,f.Data.Native.Writes);Assert.DoesNotContain("rollback",f.Data.Native.Transactions);f.Data.Native.FailCommit=false;}
        var result=await Send();result.Require();Assert.Null(result.Decision);Assert.Equal(JsonSerializer.Serialize(correction.Preparation),JsonSerializer.Serialize(result.Preparation));Assert.Equal(1,f.Data.Native.Writes);
        var row=f.Data.Native.Rows.OfType<RUN_TICKET>().Single();var proof=RevenueDeliveryAcceptanceProof.RequireEnvelope(f.Original.Company,row);var previousEntry=Assert.Single(proof.PriorVersions!);Assert.Equal(JsonSerializer.Serialize(f.Previous),JsonSerializer.Serialize(previousEntry.Record));Assert.Equal(correction.PreviousNativeRevision,previousEntry.Revision);Assert.Equal(correction.Reason,proof.Correction!.Reason);Assert.Equal(7m,row.NET_VOLUME);Assert.Equal("BBL",row.VOLUME_OUOM);Assert.Equal(f.Original.Source.Ticket.ROW_CREATED_BY,row.ROW_CREATED_BY);
        Assert.Equal(result.Revision,(await Send()).Revision);Assert.Equal(1,f.Data.Native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Service.CorrectDeliveryAcceptanceAsync(correction with{Reason="Different reason for the same intent"},"production",f.Read,f.Guard));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Service.CorrectDeliveryAcceptanceAsync(correction with{Preparation=correction.Preparation with{IntentId=Guid.NewGuid().ToString("N")}},"production",f.Read,f.Guard));Assert.Equal(1,f.Data.Native.Writes);
        var historical=RevenueDeliveryAcceptanceProof.HistoricalReview(f.Original.Company,"history-reader",await f.Read(),CompanyAccountingSourceTypes.DeliveryPreparationRevision(f.Previous));historical.Require();Assert.Equal(JsonSerializer.Serialize(f.Previous),JsonSerializer.Serialize(historical.Recorded));Assert.Equal("history-reader",historical.Source.Source.Source.ActorUserId);Assert.Null(historical.ApprovedAcceptedQuantity);
        var decision=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),f.Original.Company,"independent-next-reviewer",result.Revision,RevenueDeliveryAcceptanceDecision.Approve,"Review the corrected partial receiving quantity");var decided=await f.Data.Service.DecideDeliveryAcceptanceAsync(result,decision,"production",f.Read,f.Guard);Assert.Equal(decision,decided.Decision);Assert.Equal(2,f.Data.Native.Writes);
        var closing=RevenueDeliveryAcceptanceProof.RequireEnvelope(f.Original.Company,(await f.Read()).Ticket);Assert.Equal(JsonSerializer.Serialize(previousEntry),JsonSerializer.Serialize(Assert.Single(closing.PriorVersions!)));Assert.Equal(proof.Correction,closing.Correction);
        var read=new RevenueDeliveryAcceptanceReadReview(await f.Read(),decided);Assert.Equal(4m,read.ApprovedAcceptedQuantity);Assert.Equal(3m,read.MeasuredQuantityOutsideApprovedAcceptance);Assert.Equal(2,f.Data.Native.Writes);
    }
    [Theory][InlineData("previous-decision")][InlineData("older-preparation")][InlineData("older-decision")][InlineData("decision-preparation")][InlineData("decision-history")]
    public async Task DeliveryCorrectionRejectsIdentityReuseAcrossPreparationsAndIndependentDecisions(string mode)
    {
        var f=new DeliveryCorrectionFixture();var first=await f.Command();
        if(mode=="previous-decision")
        {
            first=first with{Preparation=first.Preparation with{IntentId=f.Previous.Decision!.IntentId}};
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Service.CorrectDeliveryAcceptanceAsync(first,"production",f.Read,f.Guard));Assert.Equal(0,f.Data.Native.Writes);return;
        }
        var prepared=await f.Data.Service.CorrectDeliveryAcceptanceAsync(first,"production",f.Read,f.Guard);
        var command=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),f.Original.Company,"next-reviewer",prepared.Revision,RevenueDeliveryAcceptanceDecision.Reject,"Review corrected receiving evidence");
        if(mode is "decision-preparation" or "decision-history")
        {
            command=command with{IntentId=mode=="decision-preparation"?prepared.Preparation.IntentId:f.Previous.Decision!.IntentId};var before=JsonSerializer.Serialize(f.Data.Native.Rows.OfType<RUN_TICKET>().Single());
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Service.DecideDeliveryAcceptanceAsync(prepared,command,"production",f.Read,f.Guard));Assert.Equal(1,f.Data.Native.Writes);Assert.Equal(before,JsonSerializer.Serialize(f.Data.Native.Rows.OfType<RUN_TICKET>().Single()));return;
        }
        await f.Data.Service.DecideDeliveryAcceptanceAsync(prepared,command,"production",f.Read,f.Guard);var next=await f.Command(version:2);next=next with{Preparation=next.Preparation with{IntentId=mode=="older-preparation"?f.Previous.Preparation.IntentId:f.Previous.Decision!.IntentId}};var original=JsonSerializer.Serialize(f.Data.Native.Rows.OfType<RUN_TICKET>().Single());
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Service.CorrectDeliveryAcceptanceAsync(next,"production",f.Read,f.Guard));Assert.Equal(2,f.Data.Native.Writes);Assert.Equal(original,JsonSerializer.Serialize(f.Data.Native.Rows.OfType<RUN_TICKET>().Single()));
    }
    [Theory][InlineData("history")][InlineData("limit")]
    public async Task DeliveryCorrectionRepeatedVersionsKeepFlatNativeHistoryAndExactIndependentDecisions(string mode)
    {
        var f=new DeliveryCorrectionFixture();var expected=new List<RevenueDeliveryAcceptanceRecord>{f.Previous};var versions=mode=="limit"?RevenueDeliveryAcceptanceProof.MaximumPriorVersions:3;
        for(var version=1;version<=versions;++version)
        {
            var correction=await f.Command(version:version);var prepared=await f.Data.Service.CorrectDeliveryAcceptanceAsync(correction,"production",f.Read,f.Guard);Assert.Null(prepared.Decision);
            var command=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),f.Original.Company,"history-reviewer-"+version,prepared.Revision,RevenueDeliveryAcceptanceDecision.Reject,"Review version "+version+" and preserve rejection evidence");
            var decided=await f.Data.Service.DecideDeliveryAcceptanceAsync(prepared,command,"production",f.Read,f.Guard);var current=await f.Read();var proof=RevenueDeliveryAcceptanceProof.RequireEnvelope(f.Original.Company,current.Ticket);
            Assert.Equal(version,proof.PriorVersions!.Count);Assert.Equal(expected.Select(entry=>JsonSerializer.Serialize(entry)),proof.PriorVersions.Select(entry=>JsonSerializer.Serialize(entry.Record)));Assert.All(proof.PriorVersions,entry=>Assert.Null(JsonSerializer.Deserialize<RUN_TICKET>(entry.NativeRowJson)!.DELIVERY_WORKFLOW_JSON));
            foreach(var preserved in expected)
            {var historical=RevenueDeliveryAcceptanceProof.HistoricalReview(f.Original.Company,"history-reader",current,CompanyAccountingSourceTypes.DeliveryPreparationRevision(preserved));historical.Require();Assert.Equal(JsonSerializer.Serialize(preserved),JsonSerializer.Serialize(historical.Recorded));}
            Assert.Equal(decided.Revision,(await f.Data.Service.DecideDeliveryAcceptanceAsync(prepared,command,"production",f.Read,f.Guard)).Revision);Assert.Equal(version*2,f.Data.Native.Writes);expected.Add(decided);
        }
        if(mode=="limit")
        {
            var before=JsonSerializer.Serialize(f.Data.Native.Rows.OfType<RUN_TICKET>().Single());var next=await f.Command(version:versions+1);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Data.Service.CorrectDeliveryAcceptanceAsync(next,"production",f.Read,f.Guard));Assert.Equal(versions*2,f.Data.Native.Writes);Assert.Equal(before,JsonSerializer.Serialize(f.Data.Native.Rows.OfType<RUN_TICKET>().Single()));Assert.Equal("rollback",f.Data.Native.Transactions.Last());
        }
        Assert.Equal(7m,f.Data.Native.Rows.OfType<RUN_TICKET>().Single().NET_VOLUME);
    }
    [Theory][InlineData("history-null")][InlineData("history-empty")][InlineData("origin-null")][InlineData("reason")][InlineData("predecessor")][InlineData("duplicate")][InlineData("record")][InlineData("native")][InlineData("audit")][InlineData("recursive")][InlineData("foreign")][InlineData("current-native")][InlineData("historic-id")][InlineData("historic-source")]
    public async Task DeliveryCorrectionNativeHistoryRejectsTamperedPredecessorAuditLinksAndHistoricalCoordinates(string mode)
    {
        var f=new DeliveryCorrectionFixture();var correction=await f.Command();await f.Data.Service.CorrectDeliveryAcceptanceAsync(correction,"production",f.Read,f.Guard);var current=await f.Read();var row=CompanyInventorySourceReview.Copy(current.Ticket);var proof=RevenueDeliveryAcceptanceProof.RequireEnvelope(f.Original.Company,row);
        if(mode is "historic-id" or "historic-source")
        {
            if(mode=="historic-source"){var rows=CompanyInventorySourceReview.Copy(current.Source.Source.Rows);rows[0].GROSS_REVENUE=999;var captured=current.Source.Source;var source=FieldRevenueSourceReview.Create(captured.Company,captured.ActorUserId,captured.FieldId,captured.From,captured.Through,captured.BindingRevision,rows);current=RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(source,current.Source.TransactionId,current.Source.Contract),current.Allocation,current.Ticket);}
            Assert.Throws<InvalidOperationException>(()=>RevenueDeliveryAcceptanceProof.HistoricalReview(f.Original.Company,"history-reader",current,mode=="historic-id"?new string('a',64):CompanyAccountingSourceTypes.DeliveryPreparationRevision(f.Previous)));return;
        }
        var entries=CompanyInventorySourceReview.Copy(proof.PriorVersions!.ToArray());
        if(mode=="history-null")proof=proof with{PriorVersions=null};if(mode=="history-empty")proof=proof with{PriorVersions=[]};if(mode=="origin-null")proof=proof with{Correction=null};if(mode=="reason")proof=proof with{Correction=proof.Correction! with{Reason="Different correction reason"}};if(mode=="predecessor")proof=proof with{Correction=proof.Correction! with{PreviousNativeRevision=new string('a',64)}};
        if(mode=="duplicate")proof=proof with{PriorVersions=[entries[0],entries[0]]};if(mode=="record")entries[0]=entries[0] with{Record=entries[0].Record with{Revision=new string('a',64)}};if(mode=="foreign")entries[0]=entries[0] with{Record=entries[0].Record with{Preparation=entries[0].Record.Preparation with{Company=new("tenant","foreign")}}};
        if(mode is "native" or "audit" or "recursive")
        {
            var native=JsonSerializer.Deserialize<RUN_TICKET>(entries[0].NativeRowJson)!;if(mode=="native")native.NET_VOLUME=8;if(mode=="audit")native.ROW_CHANGED_BY="other-reviewer";if(mode=="recursive")native.DELIVERY_WORKFLOW_JSON=row.DELIVERY_WORKFLOW_JSON;entries[0]=entries[0] with{NativeRowJson=JsonSerializer.Serialize(native)};
        }
        if(mode is "record" or "foreign" or "native" or "audit" or "recursive")proof=proof with{PriorVersions=entries};if(mode=="current-native")row.NET_VOLUME=8;
        row.DELIVERY_WORKFLOW_JSON=JsonSerializer.Serialize(proof);Assert.ThrowsAny<Exception>(()=>RevenueDeliveryAcceptanceProof.RequireEnvelope(f.Original.Company,row));Assert.Equal(1,f.Data.Native.Writes);
    }
    [Fact]
    public void DeliveryCorrectionNativeProofPreservesOriginalConstructorCreateSignatureAndLegacyDigest()
    {
        Assert.NotNull(typeof(RevenueDeliveryAcceptanceProof).GetConstructor([typeof(RevenueDeliveryAcceptanceRecord),typeof(string),typeof(string)]));var preparation=DeliveryPreparation();var record=RevenueDeliveryAcceptanceRecord.Create(preparation,new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc));var read=DeliveryRecordedReview(preparation.Source,record);var row=CompanyInventorySourceReview.Copy(read.Source.Ticket);row.DELIVERY_WORKFLOW_JSON=null;var native=JsonSerializer.Serialize(row);
        var expected=new RevenueDeliveryAcceptanceProof(record,native,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{record,native})));Assert.Equal(JsonSerializer.Serialize(expected),read.Source.Ticket.DELIVERY_WORKFLOW_JSON);Assert.DoesNotContain("PriorVersions",read.Source.Ticket.DELIVERY_WORKFLOW_JSON);Assert.DoesNotContain("Correction",read.Source.Ticket.DELIVERY_WORKFLOW_JSON);read.Require();
    }
    [Fact]
    public void DeliveryQuantityAdditiveFactsRetainOriginalPublicConstructorAndDeconstruction()
    {
        var type=typeof(RevenueDeliveryAcceptanceFacts);Assert.NotNull(type.GetConstructor([typeof(DateTime),typeof(string),typeof(string),typeof(string)]));
        var original=new RevenueDeliveryAcceptanceFacts(new DateTime(2026,1,31,0,0,0,DateTimeKind.Utc),"buyer","original-reference","Original receiving evidence");
        var (acceptedAt,buyer,evidence,notes)=original;Assert.Equal(original.AcceptedAtUtc,acceptedAt);Assert.Equal("buyer",buyer);Assert.Equal("original-reference",evidence);Assert.Equal(original.Notes,notes);Assert.Null(original.AcceptedQuantity);Assert.Null(original.AcceptedQuantityUnit);
        var quantity=original with{AcceptedQuantity=4m,AcceptedQuantityUnit="BBL"};var restored=JsonSerializer.Deserialize<RevenueDeliveryAcceptanceFacts>(JsonSerializer.Serialize(quantity));Assert.Equal(quantity,restored);Assert.NotEqual(original,restored);
    }
    [Theory][InlineData("partial")][InlineData("full")][InlineData("precision")][InlineData("zero")][InlineData("negative")][InlineData("excess")][InlineData("missing")][InlineData("quantity-only")][InlineData("unit-only")][InlineData("different-unit")][InlineData("unit-case")]
    public void DeliveryQuantityRequiresExplicitCertifiedAmountAndNeverInfersHistoricalQuantity(string mode)
    {
        var preparation=DeliveryPreparation();var facts=preparation.Facts with{AcceptedQuantity=mode=="partial"?4m:mode=="precision"?0.123456789m:7m};
        facts=mode switch{"zero"=>facts with{AcceptedQuantity=0},"negative"=>facts with{AcceptedQuantity=-1},"excess"=>facts with{AcceptedQuantity=8},"missing"=>facts with{AcceptedQuantity=null,AcceptedQuantityUnit=null},"quantity-only"=>facts with{AcceptedQuantityUnit=null},"unit-only"=>facts with{AcceptedQuantity=null},"different-unit"=>facts with{AcceptedQuantityUnit="M3"},"unit-case"=>facts with{AcceptedQuantityUnit="bbl"},_=>facts};
        preparation=preparation with{Facts=facts};
        if(mode is not ("partial" or "full" or "precision"))
        {
            Assert.Throws<InvalidOperationException>(()=>new RevenueDeliveryAcceptanceIntent(preparation,null).RequireExecution());
            if(mode=="missing")
            {
                preparation.Require();Assert.Equal(JsonSerializer.Serialize(new{facts.AcceptedAtUtc,facts.ReceivingBusinessAssociateId,facts.EvidenceReference,facts.Notes}),JsonSerializer.Serialize(facts));
                var legacy=DeliveryRecordedReview(preparation.Source,RevenueDeliveryAcceptanceRecord.Create(preparation,new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc)));legacy.Require();Assert.Null(legacy.ApprovedAcceptedQuantity);Assert.Null(legacy.MeasuredQuantityOutsideApprovedAcceptance);
            }
            return;
        }
        new RevenueDeliveryAcceptanceIntent(preparation,null).RequireExecution();var record=RevenueDeliveryAcceptanceRecord.Create(preparation,new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc));
        var field=preparation.Source.Source.Source;var source=RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(field.Company,"delivery-reviewer",field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows),preparation.Source.Source.TransactionId,preparation.Source.Source.Contract),preparation.Source.Allocation,preparation.Source.Ticket);
        var command=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,"delivery-reviewer",record.Revision,RevenueDeliveryAcceptanceDecision.Approve,"Reviewed explicit accepted quantity");command.RequireExecution(record);
        var approved=DeliveryRecordedReview(source,record.Decide(command,source,new DateTime(2026,2,3,0,0,0,DateTimeKind.Utc)));approved.Require();Assert.Equal(facts.AcceptedQuantity,approved.ApprovedAcceptedQuantity);Assert.Equal("BBL",approved.ApprovedQuantityUnit);Assert.Equal(7m-facts.AcceptedQuantity,approved.MeasuredQuantityOutsideApprovedAcceptance);Assert.Equal(mode!="full",approved.IsApprovedPartialAcceptance);Assert.Equal(7m,approved.Source.Ticket.NET_VOLUME);
        approved.Source.Ticket.NET_VOLUME=8;Assert.Throws<InvalidOperationException>(()=>approved.ApprovedAcceptedQuantity);
    }
    [Theory][InlineData("native-prepare")][InlineData("native-save")][InlineData("native-approve")][InlineData("api-prepare")][InlineData("api-save")][InlineData("api-approve")][InlineData("sdk-prepare")][InlineData("sdk-save")][InlineData("sdk-approve")][InlineData("state-prepare")][InlineData("state-approve")]
    public async Task DeliveryQuantityUnknownExecutionRefusesBeforeJournalOrBusinessWrites(string mode)
    {
        var f=new Fixture();var preparation=DeliveryPreparation();preparation=preparation with{Facts=preparation.Facts with{AcceptedQuantity=null,AcceptedQuantityUnit=null}};var record=RevenueDeliveryAcceptanceRecord.Create(preparation,new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc));var actor=mode.EndsWith("approve",StringComparison.Ordinal)?"delivery-reviewer":preparation.ActorUserId;
        var decision=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,actor,record.Revision,RevenueDeliveryAcceptanceDecision.Approve,"Unknown quantity must not be approved");var submission=new RevenueDeliveryAcceptanceSubmission(record,decision);var envelope=new RevenueDeliveryAcceptanceIntent(preparation,null);
        var field=preparation.Source.Source.Source;var source=RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(field.Company,actor,field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows),preparation.Source.Source.TransactionId,preparation.Source.Source.Contract),preparation.Source.Allocation,preparation.Source.Ticket);var current=mode.EndsWith("approve",StringComparison.Ordinal)?DeliveryRecordedReview(source,record):new RevenueDeliveryAcceptanceReadReview(source,null);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve"]));
        if(mode.StartsWith("native",StringComparison.Ordinal))
        {
            Task Send()=>mode=="native-prepare"?f.Service.PrepareDeliveryAcceptanceAsync(preparation,f.Native.Connection,()=>Task.FromResult(source),()=>Task.CompletedTask):mode=="native-save"?f.Service.SaveDeliveryAcceptanceIntentAsync(envelope,f.Native.Connection,()=>Task.CompletedTask,()=>Task.FromResult(current)):f.Service.DecideDeliveryAcceptanceAsync(record,decision,f.Native.Connection,()=>Task.FromResult(source),()=>Task.CompletedTask);
            await Assert.ThrowsAsync<InvalidOperationException>(Send);Assert.Empty(f.Native.Source.Invocations);
        }
        else if(mode.StartsWith("api",StringComparison.Ordinal))
        {
            await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve"],configure:(services,context)=>{context.Actor=actor;services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,Mock.Of<IAccessControlService>(),()=>Task.FromResult(f.Native.Connection)));});host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);
            var path="api/fields/field/accounting/revenue-source/delivery/acceptance/"+(mode=="api-prepare"?"prepare":mode=="api-save"?"intent":"decide");var response=await host.Client.PostAsJsonAsync(path,mode=="api-prepare"?(object)preparation:mode=="api-save"?envelope:submission);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Empty(f.Native.Source.Invocations);
        }
        else if(mode.StartsWith("sdk",StringComparison.Ordinal))
        {
            var sends=0;using var http=new HttpClient(new Reply(()=>{++sends;throw new InvalidOperationException("No transport may occur before explicit quantity validation");})){BaseAddress=new Uri("https://fixture/")};var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
            Task Send()=>mode=="sdk-prepare"?client.PrepareDeliveryAcceptanceAsync(preparation):mode=="sdk-save"?client.SaveDeliveryIntentAsync(envelope):client.DecideDeliveryAcceptanceAsync(submission);await Assert.ThrowsAsync<InvalidOperationException>(Send);Assert.Equal(0,sends);
        }
        else
        {
            var client=new Mock<IAccountingServiceClient>();client.Setup(value=>value.GetDeliverySavedReviewAsync("field",From,Through,source.Source.TransactionId)).ReturnsAsync(new RevenueDeliveryAcceptanceSavedReview(current,[]));using var state=new DeliveryAcceptanceWorkspaceState(company);state.Attach("field",From,Through,source.Source.TransactionId);await state.LoadAsync(client.Object);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>mode=="state-prepare"?state.PrepareAsync(client.Object,preparation.Facts):state.DecideAsync(client.Object,RevenueDeliveryAcceptanceDecision.Approve,decision.Reason));Assert.False(state.HasPendingIntent);client.Verify(value=>value.SaveDeliveryIntentAsync(It.IsAny<RevenueDeliveryAcceptanceIntent>()),Times.Never());
        }
        Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory][InlineData("partial-approve")][InlineData("partial-reject")][InlineData("partial-form")][InlineData("full-approve")][InlineData("lost-prepare")][InlineData("lost-decision")][InlineData("legacy-approve")][InlineData("legacy-reject")]
    public async Task DeliveryQuantityHttpNativeAndRecoveredScreensPreservePartialFactsAndNeverAlterMeasurement(string mode)
    {
        var f=new Fixture();f.Native.Connection="production";var preparation=DeliveryPreparation();preparation=preparation with{Facts=preparation.Facts with{AcceptedQuantity=mode=="full-approve"?7m:mode.StartsWith("legacy",StringComparison.Ordinal)?null:4m,AcceptedQuantityUnit=mode.StartsWith("legacy",StringComparison.Ordinal)?null:"BBL"}};var original=preparation.Source;
        f.Native.Rows.RemoveAll(row=>row is REVENUE_TRANSACTION or SALES_CONTRACT);f.Native.Rows.AddRange(original.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Contract));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Allocation));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Ticket));
        var actor=preparation.ActorUserId;var businessUncertain=false;var businessWrites=0;f.Native.Source.Setup(value=>value.UpdateEntity("RUN_TICKET",It.IsAny<object>())).Returns((string table,object value)=>{++f.Native.Writes;++businessWrites;f.Native.Rows.RemoveAll(row=>row is RUN_TICKET);f.Native.Rows.Add(CompanyInventorySourceReview.Copy((RUN_TICKET)value));f.Native.FailCommit=businessUncertain;return new ErrorsInfo{Flag=Errors.Ok};});
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD",It.IsAny<string>(),null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});var permissions=new[]{"Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve"};
        await using var host=await AccountingSourceHttpTests.Start(permissions:permissions,configure:(services,context)=>{context.Actor=actor;services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));});using var company=new CompanyClientContext();
        void Actor(string account){actor=account;host.Context.Actor=account;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",account);company.TrySelect(company.Clear(),account,new(preparation.Company,["ConfiguredCompanyRole"],permissions));}Actor(actor);
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);RevenueDeliveryAcceptanceRecord prepared;
        if(mode.StartsWith("legacy",StringComparison.Ordinal))
        {
            prepared=RevenueDeliveryAcceptanceRecord.Create(preparation,new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc));var row=f.Native.Rows.OfType<RUN_TICKET>().Single();row.ROW_CHANGED_BY=preparation.ActorUserId;row.ROW_CHANGED_DATE=prepared.PreparedAtUtc;row.DELIVERY_WORKFLOW_JSON=RevenueDeliveryAcceptanceProof.Create(prepared,row);
        }
        else if(mode=="lost-prepare")
        {
            businessUncertain=true;await Assert.ThrowsAnyAsync<Exception>(()=>client.PrepareDeliveryAcceptanceAsync(preparation));businessUncertain=false;f.Native.FailCommit=false;
            var saved=await client.GetDeliverySavedIntentAsync(preparation.IntentId);Assert.Equal("RECORDED",saved.Status);Assert.Equal(4m,saved.Entry.Restore(preparation.Company,actor,saved.Entry.SCOPE_KEY).Original.Facts.AcceptedQuantity);prepared=saved.Current!.Recorded!;
        }
        else if(mode=="partial-form")
        {
            await using var formServices=DeliveryServices(company,client);await using var formRenderer=new HtmlRenderer(formServices,formServices.GetRequiredService<ILoggerFactory>());RevenueDeliveryAcceptanceRecord? formPrepared=null;
            await formRenderer.Dispatcher.InvokeAsync(async()=>{
                var root=await formRenderer.RenderComponentAsync<DeliveryAcceptanceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId",original.Source.TransactionId}}));var panel=formServices.GetRequiredService<DeliveryComponents>().Panel!;var type=typeof(DeliveryAcceptanceWorkbench);
                Assert.Contains("Actual accepted receiving quantity",root.ToHtmlString());Assert.Null(type.GetField("_quantity",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel));
                foreach(var input in new Dictionary<string,object?>{{"_dateUtc",preparation.Facts.AcceptedAtUtc.Date},{"_timeUtc",preparation.Facts.AcceptedAtUtc.TimeOfDay},{"_quantity",4m},{"_buyer","buyer"},{"_reference",preparation.Facts.EvidenceReference},{"_notes",preparation.Facts.Notes}})type.GetField(input.Key,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,input.Value);
                typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null);await root.QuiescenceTask;await (Task)type.GetMethod("Prepare",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await root.QuiescenceTask;
                formPrepared=formServices.GetRequiredService<DeliveryAcceptanceWorkspaceState>().Review?.Recorded;Assert.NotNull(formPrepared);Assert.Equal(4m,formPrepared!.Preparation.Facts.AcceptedQuantity);Assert.Equal("BBL",formPrepared.Preparation.Facts.AcceptedQuantityUnit);Assert.Contains("Recorded accepted quantity: 4",root.ToHtmlString());
            });
            prepared=formPrepared!;preparation=prepared.Preparation;
        }
        else prepared=await client.PrepareDeliveryAcceptanceAsync(preparation);
        Actor("delivery-reviewer");var reject=mode.EndsWith("reject",StringComparison.Ordinal);var command=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,actor,prepared.Revision,reject?RevenueDeliveryAcceptanceDecision.Reject:RevenueDeliveryAcceptanceDecision.Approve,"Independent explicit quantity review");var submission=new RevenueDeliveryAcceptanceSubmission(prepared,command);
        if(mode=="legacy-approve")
        {
            var before=f.Native.Writes;await Assert.ThrowsAsync<InvalidOperationException>(()=>client.DecideDeliveryAcceptanceAsync(submission));var response=await host.Client.PostAsJsonAsync("api/fields/field/accounting/revenue-source/delivery/acceptance/decide",submission);Assert.Equal(HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(before,f.Native.Writes);Assert.Empty(f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>());
        }
        else if(mode=="lost-decision"){businessUncertain=true;await Assert.ThrowsAnyAsync<Exception>(()=>client.DecideDeliveryAcceptanceAsync(submission));businessUncertain=false;f.Native.FailCommit=false;}
        else await client.DecideDeliveryAcceptanceAsync(submission);
        var current=await client.GetDeliveryAcceptanceAsync("field",From,Through,original.Source.TransactionId);current.Require();Assert.Equal(7m,current.Source.Ticket.NET_VOLUME);Assert.Equal("BBL",current.Source.Ticket.VOLUME_OUOM);Assert.Equal(JsonSerializer.Serialize(original.Allocation),JsonSerializer.Serialize(current.Source.Allocation));Assert.Equal(JsonSerializer.Serialize(original.Source.Contract),JsonSerializer.Serialize(current.Source.Source.Contract));
        if(mode=="legacy-approve" || reject){Assert.Null(current.ApprovedAcceptedQuantity);Assert.Null(current.MeasuredQuantityOutsideApprovedAcceptance);}else{Assert.Equal(mode=="full-approve"?7m:4m,current.ApprovedAcceptedQuantity);Assert.Equal(mode=="full-approve"?0m:3m,current.MeasuredQuantityOutsideApprovedAcceptance);}
        var writes=f.Native.Writes;await using var services=DeliveryServices(company,client);await using var renderer=new HtmlRenderer(services,services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<DeliveryAcceptanceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId",original.Source.TransactionId}}));
            var html=root.ToHtmlString();if(mode.StartsWith("legacy",StringComparison.Ordinal)){Assert.Contains("Recorded accepted quantity: Unavailable",html);Assert.Contains("certified measurement is not substituted",html);}else Assert.Contains("Recorded accepted quantity: "+(mode=="full-approve"?"7":"4"),html);
            if(!reject && mode!="legacy-approve")Assert.Contains("Approved "+(mode=="full-approve"?"full":"partial")+" receiving quantity",html);else Assert.DoesNotContain("Approved partial receiving quantity",html);
            if(mode=="legacy-approve")
            {
                var panel=services.GetRequiredService<DeliveryComponents>().Panel!;Assert.False((bool)typeof(DeliveryAcceptanceWorkbench).GetField("_canApprove",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel)!);
                typeof(DeliveryAcceptanceWorkbench).GetField("_reason",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,"Unknown original quantity must not receive a new approval");
                await (Task)typeof(DeliveryAcceptanceWorkbench).GetMethod("Decide",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,new object[]{RevenueDeliveryAcceptanceDecision.Approve})!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null);await root.QuiescenceTask;Assert.Contains("Delivery decision was not confirmed",root.ToHtmlString());
            }
            Assert.False(services.GetRequiredService<DeliveryAcceptanceWorkspaceState>().HasPendingIntent);Assert.Equal(writes,f.Native.Writes);
        });
        Assert.Equal(mode=="legacy-approve"?0:mode=="legacy-reject"?1:2,businessWrites);
    }
    [Theory][InlineData("preparation-unsent")][InlineData("preparation-recorded")][InlineData("approve-unsent")][InlineData("approve-recorded")][InlineData("reject-unsent")][InlineData("reject-recorded")]
    public async Task DeliveryQuantityLegacyNativeAndJournalRemainReadableWithExactUnknownCommandAndNoInference(string mode)
    {
        var f=new Fixture();f.Native.Connection="production";var preparation=DeliveryPreparation();preparation=preparation with{Facts=preparation.Facts with{AcceptedQuantity=null,AcceptedQuantityUnit=null}};var original=preparation.Source;var deciding=!mode.StartsWith("preparation",StringComparison.Ordinal);var actor=deciding?"delivery-reviewer":preparation.ActorUserId;var record=RevenueDeliveryAcceptanceRecord.Create(preparation,new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc));
        var decision=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,actor,record.Revision,mode.StartsWith("reject",StringComparison.Ordinal)?RevenueDeliveryAcceptanceDecision.Reject:RevenueDeliveryAcceptanceDecision.Approve,"Preserved original historic decision");var command=deciding?new RevenueDeliveryAcceptanceIntent(null,new(record,decision)):new(preparation,null);var body=JsonSerializer.Serialize(command);Assert.DoesNotContain("AcceptedQuantity",body);
        f.Native.Rows.RemoveAll(row=>row is REVENUE_TRANSACTION or SALES_CONTRACT);f.Native.Rows.AddRange(original.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Contract));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Allocation));var ticket=CompanyInventorySourceReview.Copy(original.Ticket);
        if(deciding || mode.EndsWith("recorded",StringComparison.Ordinal))
        {
            if(deciding && mode.EndsWith("recorded",StringComparison.Ordinal))record=record.Decide(decision,original,new DateTime(2026,2,3,0,0,0,DateTimeKind.Utc));ticket.ROW_CHANGED_BY=record.Decision?.ActorUserId ??preparation.ActorUserId;ticket.ROW_CHANGED_DATE=record.DecidedAtUtc ??record.PreparedAtUtc;ticket.DELIVERY_WORKFLOW_JSON=RevenueDeliveryAcceptanceProof.Create(record,ticket);
        }
        f.Native.Rows.Add(ticket);var entry=DELIVERY_ACCEPTANCE_INTENT.Create(command,new DateTime(2026,2,4,0,0,0,DateTimeKind.Utc));f.Native.Rows.Add(entry);var nativeBefore=JsonSerializer.Serialize(ticket);var journalBefore=JsonSerializer.Serialize(entry);
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(actor,"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View"],configure:(services,context)=>{context.Actor=actor;services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));});host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);var review=await client.GetDeliverySavedIntentAsync(command.IntentId);var recorded=mode.EndsWith("recorded",StringComparison.Ordinal);
        Assert.Equal(recorded?"RECORDED":mode.StartsWith("reject",StringComparison.Ordinal)?"SAVED_PENDING":"RECONCILIATION_REQUIRED",review.Status);Assert.Equal(body,JsonSerializer.Serialize(review.Entry.Restore(preparation.Company,actor,entry.SCOPE_KEY)));Assert.NotNull(review.Current);Assert.Null(review.Current.ApprovedAcceptedQuantity);Assert.Null(review.Current.MeasuredQuantityOutsideApprovedAcceptance);
        using var state=new DeliveryAcceptanceWorkspaceState(company);state.Attach("field",From,Through,original.Source.TransactionId);state.RecoverSaved(review.Entry);await state.LoadAsync(client);Assert.Equal(!recorded,state.HasPendingIntent);Assert.Equal(!recorded && mode.StartsWith("reject",StringComparison.Ordinal),state.CanRetryPending);
        if(!recorded)Assert.Equal(body,JsonSerializer.Serialize(state.PendingOriginal));Assert.Equal(nativeBefore,JsonSerializer.Serialize(f.Native.Rows.OfType<RUN_TICKET>().Single()));Assert.Equal(journalBefore,JsonSerializer.Serialize(f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Single()));Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory][InlineData("commercial")][InlineData("accounting")][InlineData("assigned-unavailable")][InlineData("other-workspace")][InlineData("wrong-screen")][InlineData("foreign-account")][InlineData("permission")]
    public async Task DeliveryRecoveryPersonaInboxUsesConfiguredWorkspaceAndIndependentSavedQueue(string mode)
    {
        var preparation=DeliveryPreparation();using var company=new CompanyClientContext();var actor=preparation.ActorUserId;company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],mode=="permission"?[]:["Accounting.View"]));
        var route=mode=="commercial"?"/accounting/revenue-workbench":mode=="other-workspace"?"/production/operations":"/accounting/dashboard";
        var profile=new AppUserPersona{UserId=mode=="foreign-account"?"other":actor,PersonaCode="CUSTOM_WORKSPACE"};var persona=new AppPersona{Code="CUSTOM_WORKSPACE",Name="Configured company workspace",DefaultRoute=route};
        var personas=new Mock<IPersonaContextService>();personas.SetupGet(value=>value.CurrentProfile).Returns(profile);personas.SetupGet(value=>value.CurrentPersona).Returns(persona);personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(value=>value.GetInboxAsync("CUSTOM_WORKSPACE")).ReturnsAsync(new UnifiedInbox{Company=preparation.Company,ActorUserId=actor,PersonaCode="CUSTOM_WORKSPACE",CriticalTasks=[],Counts=new()});
        if(mode=="assigned-unavailable")inbox.Setup(value=>value.GetInboxAsync("CUSTOM_WORKSPACE")).ThrowsAsync(new IOException("private assigned provider detail"));
        var api=new Mock<IAccountingServiceClient>();api.Setup(value=>value.GetDeliverySavedPageAsync(1,8)).ReturnsAsync(CompanyDeliverySavedWorkPage.Create(preparation.Company,actor,"binding",[],1,8,0));
        await using var services=DeliveryServices(company,api.Object,configure:values=>{values.AddSingleton(personas.Object);values.AddSingleton(inbox.Object);values.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));});await using var renderer=new HtmlRenderer(services,services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<PersonaTaskInbox>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"WorkspaceRoutes",new[]{mode=="wrong-screen"?"/procurement":route}}}));var visible=mode is "commercial" or "accounting" or "assigned-unavailable";
            if(visible){Assert.Contains("My saved delivery work",root.ToHtmlString());Assert.Contains("Saved delivery entries: 0",root.ToHtmlString());}else Assert.DoesNotContain("My saved delivery work",root.ToHtmlString());
            api.Verify(value=>value.GetDeliverySavedPageAsync(1,8),visible?Times.Once():Times.Never());Assert.DoesNotContain("private assigned provider detail",root.ToHtmlString());
        });
    }
    [Theory][InlineData("queue-company")][InlineData("queue-auth")][InlineData("route-company")][InlineData("route-auth")]
    public async Task DeliveryRecoveryScreensRejectLateCompanyAndAuthenticationResponses(string mode)
    {
        var preparation=DeliveryPreparation();var entry=DELIVERY_ACCEPTANCE_INTENT.Create(new(preparation,null),DateTime.UtcNow);var item=CompanyDeliverySavedWorkReview.Create(preparation.Company,preparation.ActorUserId,"binding",entry,new(preparation.Source,null));var page=CompanyDeliverySavedWorkPage.Create(preparation.Company,preparation.ActorUserId,"binding",[item],1,8,1);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),preparation.ActorUserId,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));var api=new Mock<IAccountingServiceClient>();DeliveryAuth? authentication=null;
        void Late(){if(mode.EndsWith("company",StringComparison.Ordinal))company.Clear();else authentication!.Revoke();}
        api.Setup(value=>value.GetDeliverySavedPageAsync(1,8)).ReturnsAsync(()=>{Late();return page;});api.Setup(value=>value.GetDeliverySavedIntentAsync(entry.DELIVERY_ACCEPTANCE_INTENT_ID)).ReturnsAsync(()=>{Late();return item;});
        await using var services=DeliveryServices(company,api.Object);authentication=(DeliveryAuth)services.GetRequiredService<AuthenticationStateProvider>();await using var renderer=new HtmlRenderer(services,services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=mode.StartsWith("queue",StringComparison.Ordinal)?await renderer.RenderComponentAsync<DeliverySavedWorkQueue>():await renderer.RenderComponentAsync<DeliverySavedIntent>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"IntentId",entry.DELIVERY_ACCEPTANCE_INTENT_ID}}));
            Assert.DoesNotContain(entry.DELIVERY_ACCEPTANCE_INTENT_ID,root.ToHtmlString());Assert.DoesNotContain("Saved delivery entries: 1",root.ToHtmlString());Assert.Null(services.GetRequiredService<DeliveryComponents>().Panel);
        });
        api.Verify(value=>value.SaveDeliveryIntentAsync(It.IsAny<RevenueDeliveryAcceptanceIntent>()),Times.Never());api.Verify(value=>value.PrepareDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptancePreparation>()),Times.Never());api.Verify(value=>value.DecideDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptanceSubmission>()),Times.Never());
    }
    [Fact]
    public async Task DeliveryRecoveryRenderedQueuePagesUseConfirmedMembershipRatherThanAssignedTaskCounts()
    {
        var entries=new[]{DeliveryJournalEntry("ticket-a"),DeliveryJournalEntry("ticket-b")}.OrderBy(row=>row.SCOPE_KEY,StringComparer.Ordinal).ToArray();var scope=new CompanyScope("tenant","company");var actor=entries[0].ROW_CREATED_BY;
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(scope,["ConfiguredCompanyRole"],["Accounting.View"]));var api=new Mock<IAccountingServiceClient>();
        api.Setup(value=>value.GetDeliverySavedPageAsync(It.IsAny<int>(),It.IsAny<int>())).ReturnsAsync((int number,int size)=>CompanyDeliverySavedWorkPage.Create(scope,actor,"binding",entries.Skip((number-1)*size).Take(size).Select(row=>CompanyDeliverySavedWorkReview.Create(scope,actor,"binding",row,null)).ToArray(),number,size,entries.Length));
        await using var services=DeliveryServices(company,api.Object);await using var renderer=new HtmlRenderer(services,services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<DeliverySavedWorkQueue>();var component=services.GetRequiredService<DeliveryComponents>().Queue!;
            await (Task)typeof(DeliverySavedWorkQueue).GetMethod("ChangeSize",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,new object[]{1})!;
            await (Task)typeof(ComponentBase).GetMethod("InvokeAsync",BindingFlags.Instance|BindingFlags.NonPublic,null,[typeof(Action)],null)!.Invoke(component,new object[]{(Action)(()=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,null))})!;
            Assert.Contains("Saved delivery entries: 2",root.ToHtmlString());Assert.Contains(entries[0].DELIVERY_ACCEPTANCE_INTENT_ID,root.ToHtmlString());Assert.DoesNotContain(entries[1].DELIVERY_ACCEPTANCE_INTENT_ID,root.ToHtmlString());
            await (Task)typeof(DeliverySavedWorkQueue).GetMethod("LoadPage",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,new object[]{2})!;
            await (Task)typeof(ComponentBase).GetMethod("InvokeAsync",BindingFlags.Instance|BindingFlags.NonPublic,null,[typeof(Action)],null)!.Invoke(component,new object[]{(Action)(()=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,null))})!;
            Assert.Contains(entries[1].DELIVERY_ACCEPTANCE_INTENT_ID,root.ToHtmlString());Assert.DoesNotContain(entries[0].DELIVERY_ACCEPTANCE_INTENT_ID,root.ToHtmlString());Assert.Contains("Page 2 of 2",root.ToHtmlString());Assert.Contains("Reconciliation required",root.ToHtmlString());
        });
        api.Verify(value=>value.GetDeliverySavedPageAsync(1,1),Times.Once());api.Verify(value=>value.GetDeliverySavedPageAsync(2,1),Times.Once());
    }
    [Theory][InlineData("pending")][InlineData("recorded")][InlineData("changed")][InlineData("unavailable")]
    public async Task DeliveryRecoveryQueueAndUuidRouteRestoreBothOriginalCommandsWithoutGlobalFieldOrAutomaticWrite(string mode)
    {
        foreach(var deciding in new[]{false,true})
        {
            var f=new Fixture();f.Native.Connection="production";var preparation=DeliveryPreparation();var original=preparation.Source;var actor=deciding?"delivery-reviewer":preparation.ActorUserId;
            f.Native.Rows.RemoveAll(row=>row is REVENUE_TRANSACTION or SALES_CONTRACT);
            f.Native.Rows.AddRange(original.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Contract));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Allocation));
            var record=RevenueDeliveryAcceptanceRecord.Create(preparation,new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc));var ticket=CompanyInventorySourceReview.Copy(original.Ticket);
            var decision=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,actor,record.Revision,RevenueDeliveryAcceptanceDecision.Reject,"Original independent rejection evidence");
            var command=deciding?new RevenueDeliveryAcceptanceIntent(null,new(record,decision)):new(preparation,null);
            if(deciding || mode=="recorded")
            {
                if(deciding && mode=="recorded")
                {
                    var field=original.Source.Source;var reviewer=RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(field.Company,actor,field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows),original.Source.TransactionId,original.Source.Contract),original.Allocation,original.Ticket);
                    record=record.Decide(decision,reviewer,new DateTime(2026,2,3,0,0,0,DateTimeKind.Utc));
                }
                ticket.ROW_CHANGED_BY=deciding && mode=="recorded"?actor:preparation.ActorUserId;ticket.ROW_CHANGED_DATE=record.DecidedAtUtc ??record.PreparedAtUtc;ticket.DELIVERY_WORKFLOW_JSON=RevenueDeliveryAcceptanceProof.Create(record,ticket);
            }
            f.Native.Rows.Add(ticket);var entry=DELIVERY_ACCEPTANCE_INTENT.Create(command,DateTime.UtcNow);f.Native.Rows.Add(entry);
            f.Native.Source.Setup(value=>value.UpdateEntity("RUN_TICKET",It.IsAny<object>())).Returns((string table,object value)=>{++f.Native.Writes;f.Native.Rows.RemoveAll(row=>row is RUN_TICKET);f.Native.Rows.Add(CompanyInventorySourceReview.Copy((RUN_TICKET)value));return new ErrorsInfo{Flag=Errors.Ok};});
            if(mode=="changed")ticket.NET_VOLUME=8;if(mode=="unavailable")f.Fail=true;
            var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(actor,"field","FIELD",It.IsAny<string>(),null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
            var permissions=new[]{"Accounting.View",deciding?"ProductionAccounting.Approve":"ProductionAccounting.Adjust"};
            await using var host=await AccountingSourceHttpTests.Start(permissions:permissions,configure:(services,context)=>{context.Actor=actor;services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));});
            host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],permissions));
            var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
            var item=await client.GetDeliverySavedIntentAsync(command.IntentId);var expected=mode=="recorded"?"RECORDED":mode=="pending"?"SAVED_PENDING":"RECONCILIATION_REQUIRED";
            Assert.Equal(expected,item.Status);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(item.Entry.Restore(preparation.Company,actor,entry.SCOPE_KEY)));
            var page=await client.GetDeliverySavedPageAsync(1,8);Assert.Equal(1,page.TotalRecords);Assert.Equal(expected,Assert.Single(page.Rows).Status);
            var fields=new Mock<IFieldSelectionContext>(MockBehavior.Strict);fields.Setup(value=>value.GetCurrentFieldIdAsync()).ThrowsAsync(new InvalidOperationException("Global FIELD must not be read for original recovery"));
            await using var provider=DeliveryServices(company,client,fields);await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async()=>{
                var queue=await renderer.RenderComponentAsync<DeliverySavedWorkQueue>();var queueComponent=provider.GetRequiredService<DeliveryComponents>().Queue!;
                if(mode=="recorded"){Assert.DoesNotContain(command.IntentId,queue.ToHtmlString());typeof(DeliverySavedWorkQueue).GetField("_all",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(queueComponent,true);await (Task)typeof(ComponentBase).GetMethod("InvokeAsync",BindingFlags.Instance|BindingFlags.NonPublic,null,[typeof(Action)],null)!.Invoke(queueComponent,new object[]{(Action)(()=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(queueComponent,null))})!;}
                Assert.Contains("Saved delivery entries: 1",queue.ToHtmlString());Assert.Contains("/accounting/delivery-intents/"+command.IntentId,queue.ToHtmlString());
                var root=await renderer.RenderComponentAsync<DeliverySavedIntent>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"IntentId",command.IntentId}}));var state=provider.GetRequiredService<DeliveryAcceptanceWorkspaceState>();
                Assert.Contains(command.IntentId,root.ToHtmlString());Assert.Contains("Original stored window: 2026-01-01",root.ToHtmlString());Assert.DoesNotContain("Global FIELD must not",root.ToHtmlString());
                if(mode=="recorded"){Assert.False(state.HasPendingIntent);Assert.NotNull(state.Review);}
                else{Assert.True(state.HasPendingIntent);Assert.Equal(command.IntentId,state.PendingOriginal!.IntentId);Assert.Equal(mode=="pending",state.CanRetryPending);}
                Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
                if(mode=="pending")
                {
                    await (Task)typeof(DeliveryAcceptanceWorkbench).GetMethod("Retry",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(provider.GetRequiredService<DeliveryComponents>().Panel!,null)!;await root.QuiescenceTask;
                    for(var attempt=0;attempt<200 && (state.Busy || state.HasPendingIntent);++attempt)await Task.Delay(10);
                    Assert.Equal(1,f.Native.Writes);Assert.False(state.HasPendingIntent,root.ToHtmlString());Assert.Equal(command.IntentId,deciding?state.Review!.Recorded!.Decision!.IntentId:state.Review!.Recorded!.Preparation.IntentId);
                    var refreshed=await client.GetDeliverySavedIntentAsync(command.IntentId);Assert.Equal("RECORDED",refreshed.Status);Assert.Single(f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>());
                }
                company.Clear();await root.QuiescenceTask;Assert.DoesNotContain(command.IntentId,root.ToHtmlString());Assert.DoesNotContain(command.IntentId,queue.ToHtmlString());
            });
            fields.Verify(value=>value.GetCurrentFieldIdAsync(),Times.Never());Assert.All(fields.Invocations,call=>Assert.Contains(call.Method.Name,new[]{"add_CurrentFieldChanged","remove_CurrentFieldChanged"}));Assert.Equal(mode=="pending"?1:0,f.Native.Writes);if(mode!="pending")Assert.Empty(f.Native.Transactions);
            Assert.Contains(f.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="DELIVERY_ACCEPTANCE_INTENT" && ((List<AppFilter>)call.Arguments[1]).Any(filter=>filter.FieldName=="DELIVERY_ACCEPTANCE_INTENT_ID" && filter.FilterValue==command.IntentId));
        }
    }
    [Theory][InlineData("anonymous")][InlineData("role")][InlineData("permission")][InlineData("field")][InlineData("closing")][InlineData("binding")][InlineData("missing")][InlineData("duplicate")][InlineData("wrong-id")][InlineData("foreign")][InlineData("drift")]
    public async Task DeliveryRecoveryUuidAndPageRefuseForeignAmbiguousOrChangedAuthority(string mode)
    {
        foreach(var paged in new[]{false,true})
        {
            var f=new Fixture();f.Native.Connection="production";var preparation=DeliveryPreparation();var entry=DELIVERY_ACCEPTANCE_INTENT.Create(new(preparation,null),DateTime.UtcNow);f.Native.Rows.Add(entry);var captures=0;
            if(mode=="missing")f.Native.Rows.Remove(entry);
            if(mode is "duplicate" or "wrong-id" or "foreign" or "drift")f.Native.Source.Setup(value=>value.GetEntityAsync("DELIVERY_ACCEPTANCE_INTENT",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                Assert.Contains(filters,value=>value.FieldName=="OWNER_TENANT_ID" && value.FilterValue=="tenant");Assert.Contains(filters,value=>value.FieldName=="OWNER_COMPANY_BA_ID" && value.FilterValue=="company");Assert.Contains(filters,value=>value.FieldName=="ROW_CREATED_BY" && value.FilterValue==preparation.ActorUserId);Assert.Contains(filters,value=>value.FieldName=="DELIVERY_ACCEPTANCE_INTENT_ID" && value.FilterValue==entry.DELIVERY_ACCEPTANCE_INTENT_ID);
                var returned=CompanyInventorySourceReview.Copy(entry);if(mode=="wrong-id" || mode=="drift" && ++captures>1)returned=DELIVERY_ACCEPTANCE_INTENT.Create(new(preparation with{IntentId=Guid.NewGuid().ToString("N")},null),DateTime.UtcNow);if(mode=="foreign")returned.OWNER_TENANT_ID="foreign";
                return mode=="duplicate"?new object[]{returned,CompanyInventorySourceReview.Copy(returned)}:new object[]{returned};
            });
            var checks=0;var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(preparation.ActorUserId,"field","FIELD","READ",null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=mode!="field" && (mode!="closing" || ++checks<2)});
            await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="anonymous",role:mode!="role",permissions:mode=="permission"?[]:["Accounting.View"],configure:(services,context)=>{context.Actor=preparation.ActorUserId;context.ChangeBinding=mode=="binding";services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));});
            host.Client.DefaultRequestHeaders.Add("X-Test-Actor",preparation.ActorUserId);var response=await host.Client.GetAsync(paged?"api/accounting/delivery-intents?page=1&size=8":"api/accounting/delivery-intents/"+entry.DELIVERY_ACCEPTANCE_INTENT_ID);
            if(paged && mode=="missing"){Assert.Equal(HttpStatusCode.OK,response.StatusCode);Assert.Empty((await response.Content.ReadFromJsonAsync<CompanyDeliverySavedWorkPage>())!.Rows);}
            else Assert.Equal(mode=="anonymous"?HttpStatusCode.Unauthorized:mode is "missing" or "duplicate" or "wrong-id" or "foreign" or "drift"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);
            if(!response.IsSuccessStatusCode)Assert.DoesNotContain(entry.DELIVERY_ACCEPTANCE_INTENT_ID,await response.Content.ReadAsStringAsync());Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
        }
    }
    [Fact]
    public async Task DeliveryRecoveryHttpPagesAboveThirtyTwoPreserveUnconfirmedOriginalRowsAndCompleteCounts()
    {
        var f=new Fixture();f.Native.Connection="production";var entries=Enumerable.Range(0,33).Select(index=>DeliveryJournalEntry("saved-"+index)).OrderBy(row=>row.SCOPE_KEY,StringComparer.Ordinal).ToArray();f.Native.Rows.AddRange(entries);var actor=entries[0].ROW_CREATED_BY;
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(actor,"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View"],configure:(services,context)=>{context.Actor=actor;services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));});host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View"]));var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var first=await client.GetDeliverySavedPageAsync(1,32);var last=await client.GetDeliverySavedPageAsync(2,32);Assert.Equal(33,first.TotalRecords);Assert.Equal(32,first.Rows.Count);Assert.Equal(33,last.TotalRecords);Assert.Equal(entries[^1].DELIVERY_ACCEPTANCE_INTENT_ID,Assert.Single(last.Rows).Entry.DELIVERY_ACCEPTANCE_INTENT_ID);Assert.All(first.Rows.Concat(last.Rows),row=>{Assert.Null(row.Current);Assert.Equal("RECONCILIATION_REQUIRED",row.Status);});
        Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);Assert.DoesNotContain(f.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="DELIVERY_ACCEPTANCE_INTENT" && !((List<AppFilter>)call.Arguments[1]).Any(filter=>filter.FieldName=="DELIVERY_ACCEPTANCE_INTENT_ID"));
    }
    [Theory][InlineData("matching")][InlineData("late")][InlineData("null")][InlineData("foreign")][InlineData("revision")][InlineData("body")][InlineData("page")][InlineData("count")]
    public async Task DeliveryRecoveryTypedClientRequiresCompleteOwnedFrozenItemAndPage(string mode)
    {
        foreach(var paged in new[]{false,true})
        {
            var preparation=DeliveryPreparation();var entry=DELIVERY_ACCEPTANCE_INTENT.Create(new(preparation,null),DateTime.UtcNow);var item=CompanyDeliverySavedWorkReview.Create(preparation.Company,preparation.ActorUserId,"binding",entry,new(preparation.Source,null));
            var page=CompanyDeliverySavedWorkPage.Create(preparation.Company,preparation.ActorUserId,"binding",[item],1,8,1);
            if(mode=="foreign"){item=item with{ActorUserId="other"};page=page with{ActorUserId="other"};}if(mode=="revision"){item=item with{Revision="changed"};page=page with{Revision="changed"};}if(mode=="body"){item.Entry.COMMAND_JSON="{}";page.Rows[0].Entry.COMMAND_JSON="{}";}if(mode=="page")page=page with{PageNumber=2};if(mode=="count")page=page with{TotalRecords=2};
            using var company=new CompanyClientContext();company.TrySelect(company.Clear(),preparation.ActorUserId,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
            using var http=new HttpClient(new Reply(()=>{if(mode=="late")company.Clear();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:paged?(object)page:item)};})){BaseAddress=new Uri("https://fixture/")};var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
            async Task Read(){if(paged)await client.GetDeliverySavedPageAsync(1,8);else await client.GetDeliverySavedIntentAsync(entry.DELIVERY_ACCEPTANCE_INTENT_ID);}
            if(mode=="matching" || !paged && mode is "page" or "count")await Read();else await Assert.ThrowsAnyAsync<Exception>(Read);
        }
    }
    [Theory][InlineData("unavailable")][InlineData("late-save")][InlineData("missing")][InlineData("competing")][InlineData("fresh")][InlineData("completed-account-change")]
    public async Task DeliveryJournalStateRetainsUnknownSaveAndRefusesMissingOrCompetingRecoveryBeforeReplay(string mode)
    {
        foreach(var deciding in new[]{false,true})
        {
            var preparation=DeliveryPreparation();var actor=deciding?"delivery-reviewer":preparation.ActorUserId;var source=preparation.Source;
            RevenueDeliverySourceReview ForActor(string account)
            {var field=source.Source.Source;return RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(field.Company,account,field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows),source.Source.TransactionId,source.Source.Contract),source.Allocation,source.Ticket);}
            var record=RevenueDeliveryAcceptanceRecord.Create(preparation,DateTime.UtcNow);var current=deciding?DeliveryRecordedReview(ForActor(actor),record):new RevenueDeliveryAcceptanceReadReview(source,null);
            using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
            var entries=new List<DELIVERY_ACCEPTANCE_INTENT>();var api=new Mock<IAccountingServiceClient>();RevenueDeliveryAcceptanceIntent? retained=null;
            api.Setup(value=>value.GetDeliverySavedReviewAsync("field",From,Through,source.Source.TransactionId)).ReturnsAsync(()=>new RevenueDeliveryAcceptanceSavedReview(CompanyInventorySourceReview.Copy(current),CompanyInventorySourceReview.Copy(entries)));
            api.Setup(value=>value.SaveDeliveryIntentAsync(It.IsAny<RevenueDeliveryAcceptanceIntent>())).Returns((RevenueDeliveryAcceptanceIntent original)=>{
                retained=CompanyInventorySourceReview.Copy(original);if(mode=="unavailable")throw new IOException("Journal outcome unknown before business send");
                var row=DELIVERY_ACCEPTANCE_INTENT.Create(original,DateTime.UtcNow);entries.Add(row);if(mode=="late-save")company.Clear();return Task.FromResult(CompanyInventorySourceReview.Copy(row));
            });
            api.Setup(value=>value.PrepareDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptancePreparation>())).ThrowsAsync(new IOException("Business outcome unknown"));
            api.Setup(value=>value.DecideDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptanceSubmission>())).ThrowsAsync(new IOException("Business outcome unknown"));
            using var state=new DeliveryAcceptanceWorkspaceState(company);state.Attach("field",From,Through,source.Source.TransactionId);await state.LoadAsync(api.Object);
            Task Send()=>deciding?state.DecideAsync(api.Object,RevenueDeliveryAcceptanceDecision.Reject,"Retain original independent receiving decision"):state.PrepareAsync(api.Object,preparation.Facts);
            if(mode=="late-save")await Send();else await Assert.ThrowsAsync<IOException>(Send);
            Assert.True(state.HasPendingIntent);Assert.Null(state.Review);Assert.NotNull(retained);
            api.Verify(value=>value.PrepareDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptancePreparation>()),deciding || mode is "unavailable" or "late-save"?Times.Never():Times.Once());
            api.Verify(value=>value.DecideDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptanceSubmission>()),!deciding || mode is "unavailable" or "late-save"?Times.Never():Times.Once());
            if(mode=="unavailable"){Assert.Empty(entries);continue;}
            if(mode=="late-save")company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
            if(mode=="missing")entries.Clear();
            if(mode=="competing")
            {
                var other=deciding?retained! with{Submission=retained.Submission! with{Command=retained.Submission.Command with{IntentId=Guid.NewGuid().ToString("N")}}}:retained! with{Preparation=retained.Preparation! with{IntentId=Guid.NewGuid().ToString("N")}};
                entries.Clear();entries.Add(DELIVERY_ACCEPTANCE_INTENT.Create(other,DateTime.UtcNow));
            }
            if(mode is "missing" or "competing"){await Assert.ThrowsAsync<InvalidOperationException>(()=>state.LoadAsync(api.Object));Assert.True(state.HasPendingIntent);Assert.Null(state.Review);Assert.False(state.CanRetryPending);Assert.Equal(retained!.IntentId,state.PendingDecision?.IntentId ??state.PendingPreparation!.IntentId);continue;}
            if(mode=="completed-account-change")
            {
                current=deciding?DeliveryRecordedReview(ForActor(actor),record.Decide(retained!.Submission!.Command,ForActor(actor),DateTime.UtcNow)):DeliveryRecordedReview(source,RevenueDeliveryAcceptanceRecord.Create(retained!.Preparation!,DateTime.UtcNow));
                await state.LoadAsync(api.Object);Assert.False(state.HasPendingIntent);Assert.NotNull(state.SavedIntent);
                actor="another-reader";company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));entries.Clear();
                current=DeliveryRecordedReview(ForActor(actor),current.Recorded!);await state.LoadAsync(api.Object);Assert.NotNull(state.Review);Assert.Null(state.SavedIntent);Assert.False(state.HasPendingIntent);continue;
            }
            using var fresh=new DeliveryAcceptanceWorkspaceState(company);fresh.Attach("field",From,Through,source.Source.TransactionId);await fresh.LoadAsync(api.Object);
            Assert.True(fresh.HasPendingIntent);Assert.True(fresh.CanRetryPending);Assert.Equal(retained!.IntentId,fresh.PendingDecision?.IntentId ??fresh.PendingPreparation!.IntentId);Assert.Equal(retained.IntentId,fresh.SavedIntent!.DELIVERY_ACCEPTANCE_INTENT_ID);
            // Recovery reads never increase the original mocked business send count.
            api.Verify(value=>value.PrepareDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptancePreparation>()),deciding || mode=="late-save"?Times.Never():Times.Once());
            api.Verify(value=>value.DecideDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptanceSubmission>()),!deciding || mode=="late-save"?Times.Never():Times.Once());
        }
    }
    [Theory][InlineData("matching")][InlineData("late")][InlineData("null")][InlineData("foreign-actor")][InlineData("foreign-company")][InlineData("ticket")][InlineData("tampered")][InlineData("duplicate")]
    public async Task DeliverySavedTypedReadRefusesForeignMalformedOrLateEvidence(string mode)
    {
        var preparation=DeliveryPreparation();var source=preparation.Source;var row=DELIVERY_ACCEPTANCE_INTENT.Create(new(preparation,null),DateTime.UtcNow);
        var current=new RevenueDeliveryAcceptanceReadReview(source,null);
        if(mode is "foreign-actor" or "foreign-company")current=current with{Source=source with{Source=source.Source with{Source=source.Source.Source with{ActorUserId=mode=="foreign-actor"?"other":preparation.ActorUserId,Company=mode=="foreign-company"?new("tenant","other"):preparation.Company}}}};
        if(mode=="ticket")row=DeliveryJournalEntry("foreign-ticket");if(mode=="tampered")row.COMMAND_JSON="{}";
        var result=new RevenueDeliveryAcceptanceSavedReview(current,mode=="duplicate"?[row,CompanyInventorySourceReview.Copy(row)]:[row]);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),preparation.ActorUserId,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
        using var http=new HttpClient(new Reply(()=>{if(mode=="late")company.Clear();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:result)};})){BaseAddress=new Uri("https://fixture/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="matching"){var returned=await client.GetDeliverySavedReviewAsync("field",From,Through,source.Source.TransactionId);Assert.Equal(row.RECORDED_REVISION,Assert.Single(returned.Entries).RECORDED_REVISION);returned.Entries[0].COMMAND_JSON="changed";Assert.NotEqual("changed",row.COMMAND_JSON);}
        else await Assert.ThrowsAnyAsync<Exception>(()=>client.GetDeliverySavedReviewAsync("field",From,Through,source.Source.TransactionId));
    }
    [Theory][InlineData("matching")][InlineData("late")][InlineData("null")][InlineData("foreign")][InlineData("different")][InlineData("audit")][InlineData("input-change")]
    public async Task DeliverySavedTypedWriteRequiresOriginalFrozenCommandAndCompleteReadback(string mode)
    {
        var preparation=DeliveryPreparation();var command=new RevenueDeliveryAcceptanceIntent(preparation,null);var saved=DELIVERY_ACCEPTANCE_INTENT.Create(command,DateTime.UtcNow);
        if(mode=="foreign")saved.OWNER_COMPANY_BA_ID="other";
        if(mode=="different")saved=DELIVERY_ACCEPTANCE_INTENT.Create(new(preparation with{IntentId=Guid.NewGuid().ToString("N")},null),DateTime.UtcNow);
        if(mode=="audit")saved.ROW_CREATED_BY="other";
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),preparation.ActorUserId,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
        using var http=new HttpClient(new Reply(()=>{if(mode=="late")company.Clear();if(mode=="input-change")preparation.Source.Ticket.NET_VOLUME=8;return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:saved)};})){BaseAddress=new Uri("https://fixture/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode is "matching" or "input-change"){var returned=await client.SaveDeliveryIntentAsync(command);Assert.Equal(command.IntentId,returned.DELIVERY_ACCEPTANCE_INTENT_ID);Assert.Equal(7,returned.Restore(preparation.Company,preparation.ActorUserId,saved.SCOPE_KEY).Original.Source.NetVolume);}
        else await Assert.ThrowsAnyAsync<Exception>(()=>client.SaveDeliveryIntentAsync(command));
    }
    [Theory][InlineData("saved-unsent")][InlineData("lost-journal")][InlineData("lost-business")][InlineData("changed")][InlineData("grant")][InlineData("field")][InlineData("foreign-account")][InlineData("duplicate")][InlineData("bulk")][InlineData("window")]
    public async Task DeliveryJournalFreshHttpAndRenderedSessionsRecoverBothOriginalCommandsWithoutAutomaticReplay(string mode)
    {
        foreach(var deciding in new[]{false,true})
        {
            var f=new Fixture();f.Native.Connection="production";var preparation=DeliveryPreparation();var original=preparation.Source;var actor=deciding?"delivery-reviewer":preparation.ActorUserId;
            f.Native.Rows.RemoveAll(row=>row is REVENUE_TRANSACTION or SALES_CONTRACT);
            f.Native.Rows.AddRange(original.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Contract));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Allocation));
            var record=RevenueDeliveryAcceptanceRecord.Create(preparation,new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc));var ticket=CompanyInventorySourceReview.Copy(original.Ticket);
            if(deciding){ticket.ROW_CHANGED_BY=preparation.ActorUserId;ticket.ROW_CHANGED_DATE=record.PreparedAtUtc;ticket.DELIVERY_WORKFLOW_JSON=RevenueDeliveryAcceptanceProof.Create(record,ticket);}
            f.Native.Rows.Add(ticket);var businessUncertain=false;var businessWrites=0;
            f.Native.Source.Setup(value=>value.UpdateEntity("RUN_TICKET",It.IsAny<object>())).Returns((string table,object value)=>{++f.Native.Writes;++businessWrites;f.Native.Rows.RemoveAll(row=>row is RUN_TICKET);f.Native.Rows.Add(CompanyInventorySourceReview.Copy((RUN_TICKET)value));f.Native.FailCommit=businessUncertain;return new ErrorsInfo{Flag=Errors.Ok};});
            var allowed=true;var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD",It.IsAny<string>(),null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=allowed});
            var permissions=new[]{"Accounting.View",deciding?"ProductionAccounting.Approve":"ProductionAccounting.Adjust"};
            await using var host=await AccountingSourceHttpTests.Start(permissions:permissions,configure:(services,context)=>{context.Actor=actor;services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));});
            host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],permissions));
            var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
            var decision=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,actor,record.Revision,RevenueDeliveryAcceptanceDecision.Reject,"Original independently reviewed receiving rejection");
            var command=deciding?new RevenueDeliveryAcceptanceIntent(null,new(record,decision)):new(preparation,null);var before=JsonSerializer.Serialize(ticket);
            f.Native.FailCommit=mode=="lost-journal";
            if(mode=="lost-journal")await Assert.ThrowsAnyAsync<Exception>(()=>client.SaveDeliveryIntentAsync(command));else{var saved=await client.SaveDeliveryIntentAsync(command);Assert.Equal(command.IntentId,saved.DELIVERY_ACCEPTANCE_INTENT_ID);}
            f.Native.FailCommit=false;Assert.Equal(1,f.Native.Writes);Assert.Equal(0,businessWrites);Assert.Equal(before,JsonSerializer.Serialize(f.Native.Rows.OfType<RUN_TICKET>().Single()));
            if(mode=="lost-business")
            {
                businessUncertain=true;
                if(deciding)await Assert.ThrowsAnyAsync<Exception>(()=>client.DecideDeliveryAcceptanceAsync(command.Submission!));else await Assert.ThrowsAnyAsync<Exception>(()=>client.PrepareDeliveryAcceptanceAsync(command.Preparation!));
                f.Native.FailCommit=false;businessUncertain=false;Assert.Equal(1,businessWrites);
            }
            if(mode=="changed")f.Native.Rows.OfType<RUN_TICKET>().Single().NET_VOLUME=8;
            if(mode=="grant"){host.Context.Permissions=["Accounting.View"];company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));}
            if(mode=="field")allowed=false;
            if(mode=="foreign-account"){actor="different-account";host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],permissions));}
            if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Single()));
            if(mode=="bulk")
            {
                for(var index=0;index<33;index++)
                {
                    var otherTicket=CompanyInventorySourceReview.Copy(original.Ticket);otherTicket.RUN_TICKET_ID="unrelated-"+index;var allocation=CompanyInventorySourceReview.Copy(original.Allocation);allocation.ALLOCATION_REQUEST_ID=otherTicket.RUN_TICKET_ID;
                    var field=original.Source.Source;var source=RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(field.Company,actor,field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows),original.Source.TransactionId,original.Source.Contract);
                    var other=preparation with{IntentId=Guid.NewGuid().ToString("N"),ActorUserId=actor,Source=RevenueDeliverySourceReview.Create(source,allocation,otherTicket)};
                    f.Native.Rows.Add(DELIVERY_ACCEPTANCE_INTENT.Create(new(other,null),DateTime.UtcNow));
                }
            }
            await using var provider=DeliveryServices(company,client);await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async()=>{
                var root=await renderer.RenderComponentAsync<DeliveryAcceptanceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",mode=="window"?From.AddDays(-1):From},{"Through",Through},{"TransactionId",original.Source.TransactionId}}));var state=provider.GetRequiredService<DeliveryAcceptanceWorkspaceState>();var panel=provider.GetRequiredService<DeliveryComponents>().Panel!;
                if(mode is "field" or "duplicate"){Assert.Null(state.Review);Assert.Contains("could not be confirmed",root.ToHtmlString());Assert.Equal(0,businessWrites);return;}
                if(mode=="foreign-account"){Assert.NotNull(state.Review);Assert.False(state.HasPendingIntent);Assert.DoesNotContain(command.IntentId,root.ToHtmlString());Assert.Equal(0,businessWrites);return;}
                if(mode is "changed" or "window" && deciding){Assert.Null(state.Review);Assert.Null(state.SavedIntent);Assert.Contains("could not be confirmed",root.ToHtmlString());Assert.Equal(command.IntentId,f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Single().DELIVERY_ACCEPTANCE_INTENT_ID);Assert.Equal(0,businessWrites);return;}
                Assert.Equal(command.IntentId,state.SavedIntent!.DELIVERY_ACCEPTANCE_INTENT_ID);Assert.Contains("Confirmed saved delivery intent",root.ToHtmlString());
                if(mode=="changed"){Assert.True(state.HasPendingIntent);Assert.Null(state.Review);Assert.False(state.CanRetryPending);Assert.Equal(command.IntentId,state.PendingDecision?.IntentId ??state.PendingPreparation!.IntentId);Assert.Equal(0,businessWrites);return;}
                if(mode=="window"){Assert.True(state.HasPendingIntent);Assert.Null(state.Review);Assert.False(state.CanRetryPending);Assert.Equal(command.IntentId,state.PendingOriginal!.IntentId);Assert.Null(state.PendingPreparation);Assert.Contains("Original recovery source",root.ToHtmlString());Assert.Contains("2026-01-01",root.ToHtmlString());Assert.Equal(0,businessWrites);return;}
                if(mode=="lost-business"){Assert.False(state.HasPendingIntent);Assert.Equal(command.IntentId,deciding?state.Review!.Recorded!.Decision!.IntentId:state.Review!.Recorded!.Preparation.IntentId);Assert.Equal(1,businessWrites);return;}
                Assert.True(state.HasPendingIntent);Assert.Equal(command.IntentId,state.PendingDecision?.IntentId ??state.PendingPreparation!.IntentId);Assert.Equal(0,businessWrites);
                await (Task)typeof(DeliveryAcceptanceWorkbench).GetMethod("Retry",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;await root.QuiescenceTask;
                if(mode=="grant"){Assert.True(state.HasPendingIntent);Assert.Equal(0,businessWrites);Assert.Contains("remains unconfirmed",root.ToHtmlString());return;}
                Assert.False(state.HasPendingIntent);Assert.Equal(1,businessWrites);Assert.Equal(command.IntentId,deciding?state.Review!.Recorded!.Decision!.IntentId:state.Review!.Recorded!.Preparation.IntentId);
                await (Task)typeof(DeliveryAcceptanceWorkbench).GetMethod("Reload",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;Assert.Equal(1,businessWrites);
            });
            if(mode=="grant")
            {if(deciding)await Assert.ThrowsAnyAsync<Exception>(()=>client.DecideDeliveryAcceptanceAsync(command.Submission!));else await Assert.ThrowsAnyAsync<Exception>(()=>client.PrepareDeliveryAcceptanceAsync(command.Preparation!));Assert.Equal(0,businessWrites);}
            Assert.Equal(mode is "saved-unsent" or "lost-journal" or "lost-business" or "bulk"?2:1,f.Native.Writes);
            if(mode=="field" || mode is "changed" or "window" && deciding)Assert.DoesNotContain(f.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="DELIVERY_ACCEPTANCE_INTENT" && ((List<AppFilter>)call.Arguments[1]).Any(filter=>filter.FieldName=="RUN_TICKET_ID"));
            else Assert.Contains(f.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="DELIVERY_ACCEPTANCE_INTENT" && ((List<AppFilter>)call.Arguments[1]).Any(filter=>filter.FieldName=="RUN_TICKET_ID" && filter.FilterValue==original.TicketId));
        }
    }
    [Theory][InlineData("anonymous")][InlineData("role")][InlineData("permission")][InlineData("field")][InlineData("closing")][InlineData("binding")][InlineData("journal-drift")][InlineData("foreign-row")]
    public async Task DeliverySavedHttpRequiresIndependentCurrentAuthorityAndStableOwnedJournal(string mode)
    {
        var f=new Fixture();f.Native.Connection="production";var preparation=DeliveryPreparation();var original=preparation.Source;
        f.Native.Rows.RemoveAll(row=>row is REVENUE_TRANSACTION or SALES_CONTRACT);
        f.Native.Rows.AddRange(original.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Contract));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Allocation));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Ticket));
        var entry=DELIVERY_ACCEPTANCE_INTENT.Create(new(preparation,null),DateTime.UtcNow);f.Native.Rows.Add(entry);var checks=0;var journalReads=0;
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=mode!="field" && (mode!="closing" || ++checks<4)});
        if(mode is "journal-drift" or "foreign-row")
        {
            f.Native.Source.Setup(value=>value.GetEntityAsync("DELIVERY_ACCEPTANCE_INTENT",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                Assert.Contains(filters,row=>row.FieldName=="OWNER_TENANT_ID" && row.FilterValue=="tenant");Assert.Contains(filters,row=>row.FieldName=="OWNER_COMPANY_BA_ID" && row.FilterValue=="company");
                Assert.Contains(filters,row=>row.FieldName=="ROW_CREATED_BY" && row.FilterValue==preparation.ActorUserId);Assert.Contains(filters,row=>row.FieldName=="RUN_TICKET_ID" && row.FilterValue==original.TicketId);
                var returned=CompanyInventorySourceReview.Copy(entry);if(mode=="foreign-row")returned.OWNER_TENANT_ID="other";
                else if(++journalReads>1)returned=DELIVERY_ACCEPTANCE_INTENT.Create(new(preparation with{IntentId=Guid.NewGuid().ToString("N")},null),DateTime.UtcNow);
                return new object[]{returned};
            });
        }
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="anonymous",role:mode!="role",permissions:mode=="permission"?[]:["Accounting.View"],configure:(services,context)=>{
            context.Actor=preparation.ActorUserId;context.ChangeBinding=mode=="binding";services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor",preparation.ActorUserId);
        var response=await host.Client.GetAsync("api/fields/field/accounting/revenue-source/delivery/acceptance/saved?from=2026-01-01&through=2026-01-31&transactionId="+Uri.EscapeDataString(original.Source.TransactionId));
        Assert.Equal(mode=="anonymous"?HttpStatusCode.Unauthorized:mode is "journal-drift" or "foreign-row"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);
        Assert.DoesNotContain(entry.DELIVERY_ACCEPTANCE_INTENT_ID,await response.Content.ReadAsStringAsync());Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
        if(mode is "anonymous" or "role" or "permission" or "field")Assert.DoesNotContain(f.Native.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && (string)call.Arguments[0]=="DELIVERY_ACCEPTANCE_INTENT");
    }
    private static DELIVERY_ACCEPTANCE_INTENT DeliveryJournalEntry(string ticketId)
    {
        var original=DeliveryPreparation();var ticket=CompanyInventorySourceReview.Copy(original.Source.Ticket);ticket.RUN_TICKET_ID=ticketId;
        var allocation=CompanyInventorySourceReview.Copy(original.Source.Allocation);allocation.ALLOCATION_REQUEST_ID=ticketId;
        var preparation=original with{Source=RevenueDeliverySourceReview.Create(original.Source.Source,allocation,ticket)};
        return DELIVERY_ACCEPTANCE_INTENT.Create(new(preparation,null),new(2026,2,2,0,0,0,DateTimeKind.Utc));
    }
    [Theory][InlineData("first")][InlineData("last")][InlineData("page")][InlineData("size")][InlineData("total")][InlineData("next")][InlineData("previous")]
    [InlineData("count")][InlineData("foreign")][InlineData("duplicate")][InlineData("inactive")][InlineData("provider")][InlineData("drift")][InlineData("null")][InlineData("invalid")]
    public async Task DeliveryJournalPagesRefuseIncompleteNativeMetadataOrMembership(string mode)
    {
        var f=new Fixture();var one=DeliveryJournalEntry("ticket-a");var two=DeliveryJournalEntry("ticket-b");f.Native.Rows.Add(one);f.Native.Rows.Add(two);var captures=0;
        var company=new CompanyScope(one.OWNER_TENANT_ID,one.OWNER_COMPANY_BA_ID);var actor=one.ROW_CREATED_BY;
        if(mode=="provider")f.Native.Source.SetupGet(value=>value.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Failed});
        f.Native.Source.Setup(value=>value.GetEntity("DELIVERY_ACCEPTANCE_INTENT",It.IsAny<List<AppFilter>>(),It.IsAny<int>(),It.IsAny<int>())).Returns((string table,List<AppFilter> filters,int page,int size)=>{
            Assert.Contains(filters,value=>value.FieldName=="OWNER_TENANT_ID" && value.FilterValue==company.TenantId);Assert.Contains(filters,value=>value.FieldName=="OWNER_COMPANY_BA_ID" && value.FilterValue==company.BusinessAssociateId);Assert.Contains(filters,value=>value.FieldName=="ROW_CREATED_BY" && value.FilterValue==actor);
            if(mode=="null")return null!;
            var entries=new[]{CompanyInventorySourceReview.Copy(one),CompanyInventorySourceReview.Copy(two)}.OrderBy(row=>row.SCOPE_KEY,StringComparer.Ordinal).ToArray();
            if(mode=="foreign")entries[0].OWNER_TENANT_ID="foreign";if(mode=="inactive")entries[0].ACTIVE_IND="N";if(mode=="duplicate")entries[1]=CompanyInventorySourceReview.Copy(entries[0]);
            var selected=mode=="count"?entries.Take(1).ToArray():entries.Skip((page-1)*size).Take(size).ToArray();
            var result=new PagedResult(selected,page,size,mode=="drift" && ++captures>1?3:2);
            if(mode=="page")result.PageNumber=2;if(mode=="size")result.PageSize=1;if(mode=="total")result.TotalPages=0;if(mode=="next")result.HasNextPage=true;if(mode=="previous")result.HasPreviousPage=true;return result;
        });
        var number=mode=="last"?2:mode=="invalid"?0:1;var count=mode is "first" or "last"?1:2;
        Task<(IReadOnlyList<DELIVERY_ACCEPTANCE_INTENT> Entries,int Total)> Read()=>f.Service.GetDeliveryAcceptanceIntentPageAsync(company,actor,f.Native.Connection,number,count,()=>Task.CompletedTask);
        if(mode is "first" or "last")
        {var result=await Read();var expected=new[]{one,two}.OrderBy(row=>row.SCOPE_KEY,StringComparer.Ordinal).ElementAt(number-1);Assert.Equal(2,result.Total);Assert.Equal(expected.RECORDED_REVISION,Assert.Single(result.Entries).RECORDED_REVISION);}
        else await Assert.ThrowsAnyAsync<Exception>(()=>Read());Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Fact]
    public async Task DeliveryJournalAboveThirtyTwoKeepsCompleteNativePagingAndRefusesTruncatedWholeSet()
    {
        var f=new Fixture();var entries=Enumerable.Range(0,33).Select(index=>DeliveryJournalEntry("ticket-"+index)).OrderBy(row=>row.SCOPE_KEY,StringComparer.Ordinal).ToArray();f.Native.Rows.AddRange(entries);
        var company=new CompanyScope(entries[0].OWNER_TENANT_ID,entries[0].OWNER_COMPANY_BA_ID);var actor=entries[0].ROW_CREATED_BY;
        f.Native.Source.Setup(value=>value.GetEntity("DELIVERY_ACCEPTANCE_INTENT",It.IsAny<List<AppFilter>>(),It.IsAny<int>(),It.IsAny<int>())).Returns((string table,List<AppFilter> filters,int page,int size)=>new PagedResult(entries.Skip((page-1)*size).Take(size).Select(CompanyInventorySourceReview.Copy).ToArray(),page,size,entries.Length));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Service.GetDeliveryAcceptanceIntentsAsync(company,actor,f.Native.Connection,()=>Task.CompletedTask));
        var first=await f.Service.GetDeliveryAcceptanceIntentPageAsync(company,actor,f.Native.Connection,1,32,()=>Task.CompletedTask);Assert.Equal(33,first.Total);Assert.Equal(32,first.Entries.Count);
        var last=await f.Service.GetDeliveryAcceptanceIntentPageAsync(company,actor,f.Native.Connection,2,32,()=>Task.CompletedTask);Assert.Equal(33,last.Total);Assert.Equal(entries[^1].RECORDED_REVISION,Assert.Single(last.Entries).RECORDED_REVISION);
        first.Entries[0].COMMAND_JSON="{}";Assert.Equal(entries[0].RECORDED_REVISION,(await f.Service.GetDeliveryAcceptanceIntentAsync(company,actor,entries[0].SCOPE_KEY,f.Native.Connection,()=>Task.CompletedTask))!.RECORDED_REVISION);
        Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory][InlineData("valid")][InlineData("uncertain")][InlineData("competing")][InlineData("source")][InlineData("authority")][InlineData("revoke")]
    [InlineData("schema")][InlineData("write")][InlineData("readback")][InlineData("duplicate")][InlineData("tamper")][InlineData("foreign")]
    [InlineData("collision")][InlineData("borrowed")][InlineData("isolation")][InlineData("closing-source")][InlineData("closing-authority")][InlineData("reader-actor")]
    public async Task DurableDeliveryJournalRetainsOriginalPreparationAndDecisionWithoutChangingBusinessEvidence(string mode)
    {
        foreach(var deciding in new[]{false,true})
        {
            var f=new Fixture();f.Native.Connection="production";var preparation=DeliveryPreparation();var actor=deciding?"delivery-reviewer":preparation.ActorUserId;
            var at=new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc);var prepared=RevenueDeliveryAcceptanceRecord.Create(preparation,at);
            var decision=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,actor,prepared.Revision,RevenueDeliveryAcceptanceDecision.Reject,"Reviewed receiving rejection to retain before writing");
            var command=deciding?new RevenueDeliveryAcceptanceIntent(null,new(prepared,decision)):new(preparation,null);
            var ticket=CompanyInventorySourceReview.Copy(preparation.Source.Ticket);
            if(deciding){ticket.ROW_CHANGED_BY=preparation.ActorUserId;ticket.ROW_CHANGED_DATE=at;ticket.DELIVERY_WORKFLOW_JSON=RevenueDeliveryAcceptanceProof.Create(prepared,ticket);}
            f.Native.Rows.Add(ticket);var business=JsonSerializer.Serialize(ticket);var revoked=false;var reads=0;
            Task Guard(){if(revoked || mode=="authority")throw new UnauthorizedAccessException();return Task.CompletedTask;}
            Task<RevenueDeliveryAcceptanceReadReview> Current()
            {
                ++reads;var row=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<RUN_TICKET>().Single());if(mode=="source" || mode=="closing-source" && reads>1)row.NET_VOLUME=8;
                var original=preparation.Source.Source.Source;var source=FieldRevenueSourceReview.Create(original.Company,mode=="reader-actor"?"other-account":actor,original.FieldId,original.From,original.Through,original.BindingRevision,original.Rows);
                var contract=RevenueSalesContractReview.Create(source,preparation.Source.Source.TransactionId,preparation.Source.Source.Contract);
                var delivery=RevenueDeliverySourceReview.Create(contract,preparation.Source.Allocation,row);
                return Task.FromResult(new RevenueDeliveryAcceptanceReadReview(delivery,deciding?prepared:null));
            }
            f.MissingDeliveryIntentSchema=mode=="schema";f.Native.FailCommit=mode=="uncertain";f.Native.FailWrite=mode=="write"?1:0;f.Native.Borrowed=mode=="borrowed";
            if(mode=="isolation")f.Native.Isolation=System.Data.IsolationLevel.ReadCommitted;
            f.AfterIntentWrite=()=>{if(mode=="revoke")revoked=true;if(mode=="readback")f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Single().RECORDED_REVISION="";};
            f.Native.AfterCommit=()=>{if(mode=="closing-authority")revoked=true;};
            if(mode=="collision"){var foreign=DELIVERY_ACCEPTANCE_INTENT.Create(command,DateTime.UtcNow);foreign.OWNER_TENANT_ID="foreign";foreign.RECORDED_REVISION=DELIVERY_ACCEPTANCE_INTENT.Revision(foreign);f.Native.Rows.Add(foreign);}
            if(mode is "source" or "authority" or "revoke" or "schema" or "write" or "readback" or "borrowed" or "isolation" or "closing-source" or "reader-actor" or "collision")
            {
                await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SaveDeliveryAcceptanceIntentAsync(command,"production",Guard,Current));
                Assert.Equal(mode=="collision"?1:0,f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Count());Assert.Equal(business,JsonSerializer.Serialize(f.Native.Rows.OfType<RUN_TICKET>().Single()));continue;
            }
            if(mode is "uncertain" or "closing-authority")
            {
                await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SaveDeliveryAcceptanceIntentAsync(command,"production",Guard,Current));Assert.Single(f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>());Assert.Equal(1,f.Native.Writes);
                f.Native.FailCommit=false;revoked=false;f.Native.AfterCommit=null;
            }
            var saved=await f.Service.SaveDeliveryAcceptanceIntentAsync(command,"production",Guard,Current);Assert.Equal(1,f.Native.Writes);
            Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(saved.Restore(command.Company,actor,command.ScopeKey)));Assert.Equal(business,JsonSerializer.Serialize(f.Native.Rows.OfType<RUN_TICKET>().Single()));
            Assert.Equal(saved.RECORDED_REVISION,(await f.Service.SaveDeliveryAcceptanceIntentAsync(command,"production",Guard,Current)).RECORDED_REVISION);Assert.Equal(1,f.Native.Writes);
            var entries=await f.Service.GetDeliveryAcceptanceIntentsAsync(command.Company,actor,"production",Guard);Assert.Equal(saved.RECORDED_REVISION,Assert.Single(entries).RECORDED_REVISION);
            entries[0].COMMAND_JSON="{}";Assert.Equal(saved.RECORDED_REVISION,(await f.Service.GetDeliveryAcceptanceIntentAsync(command.Company,actor,command.ScopeKey,"production",Guard))!.RECORDED_REVISION);
            if(mode=="competing")
            {
                var other=deciding?command with{Submission=new(prepared,decision with{IntentId=Guid.NewGuid().ToString("N"),Reason="Competing reason"})}:command with{Preparation=preparation with{IntentId=Guid.NewGuid().ToString("N"),Facts=preparation.Facts with{Notes="Competing preparation"}}};
                Assert.Equal(command.ScopeKey,other.ScopeKey);await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SaveDeliveryAcceptanceIntentAsync(other,"production",Guard,Current));Assert.Equal(1,f.Native.Writes);
            }
            var native=f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Single();
            if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(native));
            if(mode=="tamper")native.COMMAND_JSON="{}";
            if(mode=="foreign")native.OWNER_TENANT_ID="foreign";
            if(mode is "duplicate" or "tamper")
            {await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.GetDeliveryAcceptanceIntentAsync(command.Company,actor,command.ScopeKey,"production",Guard));await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.GetDeliveryAcceptanceIntentsAsync(command.Company,actor,"production",Guard));}
            if(mode=="foreign")
            {Assert.Null(await f.Service.GetDeliveryAcceptanceIntentAsync(command.Company,actor,command.ScopeKey,"production",Guard));Assert.Empty(await f.Service.GetDeliveryAcceptanceIntentsAsync(command.Company,actor,"production",Guard));await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SaveDeliveryAcceptanceIntentAsync(command,"production",Guard,Current));}
            Assert.Equal(1,f.Native.Writes);Assert.Equal(business,JsonSerializer.Serialize(f.Native.Rows.OfType<RUN_TICKET>().Single()));
        }
    }
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("lost")][InlineData("rollback")][InlineData("other-reviewer")]
    [InlineData("maker")][InlineData("grant")][InlineData("source")][InlineData("definition")][InlineData("field")][InlineData("inbox")]
    [InlineData("screen-approve")][InlineData("screen-reject")][InlineData("screen-denied")]
    [InlineData("handoff-screen")][InlineData("handoff-lost")][InlineData("handoff-conflict")]
    [InlineData("handoff-field")][InlineData("handoff-source")][InlineData("handoff-grant")][InlineData("handoff-duplicate")]
    [InlineData("history-active")][InlineData("history-closed")]
    [InlineData("correction-api")][InlineData("correction-save-lost")][InlineData("correction-business-lost")][InlineData("correction-state")][InlineData("correction-recovery")]
    [InlineData("correction-recovery-screen")]
    [InlineData("correction-form")][InlineData("correction-form-quantity")][InlineData("correction-form-reason")][InlineData("correction-form-auth")]
    [InlineData("correction-role")][InlineData("correction-grant")][InlineData("correction-field")][InlineData("correction-source")][InlineData("correction-process")][InlineData("correction-predecessor")][InlineData("correction-quantity")]
    public async Task DeliveryCompanyHandoffRequiresRecordedDecisionAndRoutesOriginalPreparerReview(string mode)
    {
        var f=new Fixture();f.Native.Connection="production";var template=DeliveryPreparation();var original=template.Source;
        f.Native.Rows.Clear();f.Native.Rows.AddRange(original.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));
        f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Contract));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Allocation));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Ticket));
        f.Native.Source.Setup(value=>value.UpdateEntity("RUN_TICKET",It.IsAny<object>())).Returns((string table,object value)=>{++f.Native.Writes;f.Native.Rows.RemoveAll(row=>row is RUN_TICKET);f.Native.Rows.Add(CompanyInventorySourceReview.Copy((RUN_TICKET)value));return new ErrorsInfo{Flag=Errors.Ok};});
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");CompanyStepCommandTests.ConfigureUpdates(native);
        native.Definition.ENTITY_TYPE="RUN_TICKET";
        native.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new Beep.OilandGas.Models.Processes.ProcessStepDefinition{
            StepId="review",StepName="Independent delivery acceptance",SequenceNumber=1,RequiredRoles=["ConfiguredCompanyRole"],RequiresApproval=true,StepType="APPROVAL",
            StepConfiguration=new(){{CompanyAccountingStepEvidence.ConfigurationKey,mode=="definition"?CompanyAccountingSourceTypes.CommercialApproval:CompanyAccountingSourceTypes.DeliveryApproval}}},
            new(){StepId="return",StepName="Original delivery preparer rejection review",SequenceNumber=2,RequiredRoles=["ConfiguredCompanyRole"],StepType="ACTION",
                StepConfiguration=new(){{CompanyAccountingStepEvidence.ConfigurationKey,CompanyAccountingSourceTypes.DeliveryRejectionReview}}}}});
        string[] permissions=["Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve","Workflow.Start","Workflow.Approve","Workflow.Execute"];
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD",It.IsAny<string>(),null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        using var rbac=mode=="inbox" || mode.StartsWith("screen",StringComparison.Ordinal)?new WorkflowInboxHttpTests.Fixture("COMMERCIAL","ConfiguredCompanyRole","reviewer"):null;
        if(rbac is not null)native.Native.Destinations=["COMMERCIAL"];
        await using var host=await AccountingSourceHttpTests.Start(permissions:permissions,configure:(services,context)=>{
            if(rbac is not null){rbac.Configure(services,context);services.AddSingleton<IProcessService>(native.Service);services.AddSingleton<Beep.OilandGas.LifeCycle.Services.Processes.ICrossPersonaTaskRouter>(native.Native.Router);}
            context.FixedProcess=native.Service;context.StartBinding=new string('B',64);
            services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));
        });
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(template.Company,["ConfiguredCompanyRole"],permissions));
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);var accounting=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var processes=new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance);
        var fresh=await accounting.GetDeliveryAcceptanceAsync("field",From,Through,original.Source.TransactionId);
        var preparation=template with{ActorUserId="actor",Source=fresh.Source};var prepared=await accounting.PrepareDeliveryAcceptanceAsync(preparation);
        var query=new CompanyAccountingProcessStartQuery("definition","RUN_TICKET","ticket",CompanyAccountingSourceTypes.DeliveryPreparationRevision(prepared));
        if(mode=="definition"){await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCompanyAccountingStartAsync(query));Assert.Equal(0,native.Native.Writes);return;}
        var handoffQuery=new CompanyAccountingProcessStartCatalogQuery("RUN_TICKET","ticket",query.ExpectedSourceRevision!);
        if(mode.StartsWith("handoff",StringComparison.Ordinal))Assert.Null((await processes.DiscoverNativeDecisionHandoffAsync(handoffQuery)).Start);
        var catalog=await processes.GetCompanyAccountingStartCatalogAsync(handoffQuery);var start=Assert.Single(catalog.Choices);Assert.Equal("actor",start.SourcePreparerUserId);
        var intent=start.CreateIntent(Guid.NewGuid().ToString("N"));CompanyAccountingProcessStartReceipt receipt;
        if(mode is "handoff-screen" or "handoff-lost" or "handoff-conflict")
        {
            CompanyAccountingProcessStartReceipt? created=null;
            var transport=new Mock<IBusinessProcessServiceClient>(MockBehavior.Strict);
            transport.Setup(value=>value.DiscoverNativeDecisionHandoffAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyAccountingProcessStartCatalogQuery value,CancellationToken token)=>processes.DiscoverNativeDecisionHandoffAsync(value,token));
            transport.Setup(value=>value.GetCompanyAccountingStartCatalogAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyAccountingProcessStartCatalogQuery value,CancellationToken token)=>processes.GetCompanyAccountingStartCatalogAsync(value,token));
            transport.Setup(value=>value.StartCompanyAccountingAsync(It.IsAny<CompanyAccountingProcessStart>(),It.IsAny<CancellationToken>())).Returns(async(CompanyAccountingProcessStart value,CancellationToken token)=>{
                created=await processes.StartCompanyAccountingAsync(mode=="handoff-conflict"?value with{IntentId=Guid.NewGuid().ToString("N")}:value,token);
                if(mode!="handoff-screen")throw new HttpRequestException("The start acknowledgement was lost.");return created;
            });
            ServiceProvider PreparationServices(IBusinessProcessServiceClient selected)
            {
                var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");
                var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<DeliveryAcceptanceWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();
                services.AddSingleton<IBusinessProcessServiceClient>(selected);services.AddSingleton<IAccountingServiceClient>(accounting);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new DeliveryAuth(company));
                services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,CommercialProcessComponents>();return services.BuildServiceProvider();
            }
            await using(var provider=PreparationServices(transport.Object))
            await using(var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>()))
            {
                await renderer.Dispatcher.InvokeAsync(async()=>{
                    var root=await renderer.RenderComponentAsync<DeliveryAcceptanceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId",original.Source.TransactionId}}));
                    Assert.Contains("acceptance-reference",root.ToHtmlString());Assert.Contains("Review workflow choices",root.ToHtmlString());Assert.DoesNotContain("Record delivery preparation for independent review",root.ToHtmlString());
                    var panel=((CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>()).Handoff!;
                    async Task Invoke(string method,params object?[] args){var returned=panel.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,args);if(returned is Task task)await task;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null);await root.QuiescenceTask;}
                    await Invoke("Load");Assert.Single(provider.GetRequiredService<CompanyAccountingHandoffState>().Catalog!.Choices);await Invoke("Select","definition");Assert.Contains("Recorded source maker: actor",root.ToHtmlString());await Invoke("Submit");
                    var state=provider.GetRequiredService<CompanyAccountingHandoffState>();Assert.NotNull(state.Intent);intent=state.Intent!;Assert.NotNull(created);var writes=native.Native.Writes;
                    Assert.Equal(mode!="handoff-screen",state.HasUnconfirmedIntent);await Invoke("Load");Assert.Equal(writes,native.Native.Writes);
                    if(mode=="handoff-conflict"){Assert.True(state.HasUnconfirmedIntent);Assert.Same(intent,state.Intent);Assert.Null(state.Discovery);Assert.Null(state.Catalog);Assert.DoesNotContain("Open original preparation process",root.ToHtmlString());return;}
                    Assert.False(state.HasUnconfirmedIntent);Assert.Equal(intent.InstanceId(),state.Discovery!.Start!.InstanceId());Assert.Contains("Open original preparation process",root.ToHtmlString());Assert.Contains(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",intent.InstanceId(),"RUN_TICKET"),root.ToHtmlString());Assert.Null(state.Catalog);
                });
            }
            transport.Verify(value=>value.StartCompanyAccountingAsync(It.IsAny<CompanyAccountingProcessStart>(),It.IsAny<CancellationToken>()),Times.Once);
            receipt=created!;
            if(mode=="handoff-conflict"){Assert.Equal(2,f.Native.Writes);return;}
            var beforeRecovery=native.Native.Writes;
            await using(var provider=PreparationServices(processes))
            await using(var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>()))
            {
                await renderer.Dispatcher.InvokeAsync(async()=>{
                    var root=await renderer.RenderComponentAsync<DeliveryAcceptanceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId",original.Source.TransactionId}}));
                    var panel=((CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>()).Handoff!;await (Task)panel.GetType().GetMethod("Load",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;
                    typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null);await root.QuiescenceTask;
                    Assert.Contains("Open original preparation process",root.ToHtmlString());Assert.Null(provider.GetRequiredService<CompanyAccountingHandoffState>().Intent);Assert.Null(provider.GetRequiredService<CompanyAccountingHandoffState>().Catalog);
                });
            }
            Assert.Equal(beforeRecovery,native.Native.Writes);Assert.Equal(2,f.Native.Writes);
            await Assert.ThrowsAnyAsync<Exception>(()=>processes.StartCompanyAccountingAsync(intent with{IntentId=Guid.NewGuid().ToString("N")}));Assert.Equal(beforeRecovery,native.Native.Writes);return;
        }
        receipt=await processes.StartCompanyAccountingAsync(intent);
        if(mode.StartsWith("handoff",StringComparison.Ordinal))
        {
            var discovery=await processes.DiscoverNativeDecisionHandoffAsync(handoffQuery);Assert.Equal(intent.InstanceId(),discovery.Start!.InstanceId());Assert.Equal(JsonSerializer.Serialize(intent),JsonSerializer.Serialize(discovery.Start));
            var writes=native.Native.Writes;
            if(mode=="handoff-source")f.Native.Rows.OfType<REVENUE_TRANSACTION>().Single().GROSS_REVENUE=999;
            if(mode=="handoff-field")assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
            if(mode=="handoff-grant")host.Context.Permissions=["Workflow.Start"];
            if(mode=="handoff-duplicate")native.Native.Rows.Add(CompanyInventorySourceReview.Copy(native.Native.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE>().Single()));
            var response=await host.Client.PostAsJsonAsync("/api/accounting/processes/native-handoff-review",handoffQuery);
            Assert.Equal(mode is "handoff-field" or "handoff-grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,response.StatusCode);Assert.Equal(writes,native.Native.Writes);Assert.Equal(2,f.Native.Writes);return;
        }
        var startedWrites=native.Native.Writes;Assert.Equal(receipt.RecordedRevision,(await processes.StartCompanyAccountingAsync(intent)).RecordedRevision);Assert.Equal(startedWrites,native.Native.Writes);
        Assert.All(native.Native.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.CROSS_PERSONA_TASK>(),task=>Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",receipt.InstanceId,"RUN_TICKET"),task.ROUTE));
        void Actor(string actor){host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],permissions));}
        Actor("reviewer");var review=(await processes.ReviewCompanyAccountingStepAsync(receipt.InstanceId))!;Assert.False(review.Eligible);Assert.Equal("DELIVERY_APPROVAL_REQUIRED",review.Eligibility);
        if(rbac is not null)
        {
            var inbox=await new UnifiedTaskInboxService(api).GetInboxAsync("COMMERCIAL");var task=Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));
            Assert.Equal("RUN_TICKET",task.EntityType);Assert.Equal(receipt.InstanceId,task.WorkflowName);Assert.Equal("DELIVERY_APPROVAL_REQUIRED",task.AccountingStepReview!.Eligibility);
            var personas=new Mock<IPersonaContextService>();personas.SetupGet(value=>value.CurrentPersona).Returns(new AppPersona{Code="COMMERCIAL",Name="Commercial",DefaultRoute="/accounting/revenue-recognition"});
            personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId="reviewer",PersonaCode="COMMERCIAL"});personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
            using var queue=new PersonaTaskInboxState(new UnifiedTaskInboxService(api),company,personas.Object);queue.Start();await queue.LoadAsync();queue.SelectSource("RUN_TICKET");queue.SelectAccountingEvidence("DELIVERY_APPROVAL_REQUIRED");
            Assert.Single(queue.VisibleRows);Assert.Equal(1,queue.SourceCount("RUN_TICKET"));Assert.Equal(1,queue.EvidenceCount("DELIVERY_APPROVAL_REQUIRED"));
            var queueServices=new ServiceCollection();queueServices.AddLogging();queueServices.AddMudServices();queueServices.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();queueServices.AddSingleton<NavigationManager,Navigation>();
            await using var queueProvider=queueServices.BuildServiceProvider();await using var queueRenderer=new HtmlRenderer(queueProvider,queueProvider.GetRequiredService<ILoggerFactory>());
            await queueRenderer.Dispatcher.InvokeAsync(async()=>{
                var rendered=await queueRenderer.RenderComponentAsync<PersonaAssignedWorkPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",queue},{"WorkspaceName","Commercial"}}));
                Assert.Contains("Delivery acceptance assignments: 1",rendered.ToHtmlString());Assert.Contains("Delivery decision needed: 1",rendered.ToHtmlString());Assert.Contains(task.Route,rendered.ToHtmlString());
                Assert.Contains("original receiving evidence",rendered.ToHtmlString());
            });
        }
        var exactSource=await processes.GetCompanyDeliverySourceAsync(receipt.InstanceId);exactSource.Require(preparation.Company,"reviewer",receipt.InstanceId);
        Assert.Equal("ticket",exactSource.TicketId);Assert.Equal(original.Source.TransactionId,exactSource.Delivery.Source.Source.TransactionId);
        if(mode.StartsWith("screen",StringComparison.Ordinal))
        {
            var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("another-global-field");
            ServiceProvider ScreenServices(string actor)
            {
                var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<CompanyAccountingStepState>();services.AddScoped<DeliveryAcceptanceWorkspaceState>();
                services.AddSingleton<IBusinessProcessServiceClient>(processes);services.AddSingleton<IAccountingServiceClient>(accounting);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new CommercialAuth(actor));
                services.AddSingleton<NavigationManager>(new CommercialProcessNavigation(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",receipt.InstanceId,"RUN_TICKET")));services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();
                services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,CommercialProcessComponents>();return services.BuildServiceProvider();
            }
            if(mode=="screen-denied")assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
            await using(var provider=ScreenServices("reviewer"))
            await using(var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>()))
            {
                await renderer.Dispatcher.InvokeAsync(async()=>{
                    var root=await renderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",receipt.InstanceId}}));
                    if(mode=="screen-denied"){Assert.DoesNotContain("acceptance-reference",root.ToHtmlString());Assert.DoesNotContain("Approve reviewed delivery acceptance",root.ToHtmlString());return;}
                    var components=(CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>();var page=components.Page!;
                    async Task Invoke(object component,string method,params object?[] args)
                    {
                        await (Task)component.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(component,args)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);await root.QuiescenceTask;
                        var deadline=DateTime.UtcNow.AddSeconds(10);
                        while(DateTime.UtcNow<deadline && ((bool)page.GetType().GetField("_busy",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(page)! || provider.GetRequiredService<DeliveryAcceptanceWorkspaceState>().Busy ||
                            components.Delivery is {} active && (bool)active.GetType().GetField("_authorized",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(active)! == false))await Task.Delay(10);
                    }
                    Assert.Contains("acceptance-reference",root.ToHtmlString());Assert.Contains("Approve reviewed delivery acceptance",root.ToHtmlString());Assert.Contains("FIELD: field",root.ToHtmlString());Assert.DoesNotContain("Record delivery preparation for independent review",root.ToHtmlString());
                    await Invoke(page,"ReviewStep");Assert.Contains("original receiving evidence",root.ToHtmlString());
                    var panel=components.Delivery!;panel.GetType().GetField("_reason",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,"Independent persona checked original receiving evidence");
                    await Invoke(panel,"Decide",mode=="screen-reject"?RevenueDeliveryAcceptanceDecision.Reject:RevenueDeliveryAcceptanceDecision.Approve);
                    Assert.Equal(4,f.Native.Writes);Assert.Contains("Recorded independent decision",root.ToHtmlString());Assert.Null(provider.GetRequiredService<CompanyAccountingStepState>().Review);
                    await Invoke(page,"ReviewStep");Assert.Contains(mode=="screen-reject"?"Route recorded rejection to original preparer":"Complete reviewed workflow step",root.ToHtmlString());
                    page.GetType().GetField("_notes",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,"Complete this original delivery task");await Invoke(page,"CompleteStep");
                    Assert.Contains("process reload confirmed",root.ToHtmlString());Assert.NotNull(provider.GetRequiredService<CompanyAccountingStepState>().Receipt);
                });
            }
            if(mode=="screen-reject")
            {
                Actor("actor");await using var provider=ScreenServices("actor");await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
                await renderer.Dispatcher.InvokeAsync(async()=>{
                    var root=await renderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",receipt.InstanceId}}));var page=((CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>()).Page!;
                    await (Task)page.GetType().GetMethod("ReviewStep",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);await root.QuiescenceTask;
                    Assert.Contains("Delivery acceptance remains rejected",root.ToHtmlString());Assert.DoesNotContain("Approve reviewed delivery acceptance",root.ToHtmlString());
                    page.GetType().GetField("_notes",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,"Original preparer reviewed receiving rejection");
                    await (Task)page.GetType().GetMethod("CompleteStep",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;Assert.NotNull(provider.GetRequiredService<CompanyAccountingStepState>().Receipt);
                });
                Assert.Equal("REJECTION_REVIEWED",(await processes.GetCompanyAccountingInstanceAsync(receipt.InstanceId))!.CurrentState);
            }
            fields.Verify(value=>value.GetCurrentFieldIdAsync(),Times.Never);fields.Verify(value=>value.SetCurrentFieldAsync(It.IsAny<SetActiveFieldRequest>(),It.IsAny<CancellationToken>()),Times.Never);Assert.Equal(mode=="screen-denied"?2:4,f.Native.Writes);return;
        }
        if(mode=="maker")
        {
            Actor("actor");await Assert.ThrowsAnyAsync<Exception>(()=>accounting.DecideDeliveryAcceptanceAsync(new(prepared,new(Guid.NewGuid().ToString("N"),preparation.Company,"actor",prepared.Revision,RevenueDeliveryAcceptanceDecision.Approve,"Maker may not approve"))));
            Assert.Equal("ROLE_OR_INDEPENDENCE_REQUIRED",(await processes.ReviewCompanyAccountingStepAsync(receipt.InstanceId))!.Eligibility);Assert.Equal(2,f.Native.Writes);return;
        }
        var rejecting=mode is "reject" or "lost" or "rollback" or "history-active" or "history-closed" || mode.StartsWith("correction-",StringComparison.Ordinal);
        var decided=await accounting.DecideDeliveryAcceptanceAsync(new(prepared,new(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",prepared.Revision,rejecting?RevenueDeliveryAcceptanceDecision.Reject:RevenueDeliveryAcceptanceDecision.Approve,"Reviewed exact receiving evidence")));
        async Task VerifyCorrectedHistory(CompanyAccountingStepReview? retained=null)
        {
            Actor("actor");var current=await accounting.GetDeliveryAcceptanceAsync("field",From,Through,original.Source.TransactionId);var proof=RevenueDeliveryAcceptanceProof.RequireEnvelope(preparation.Company,current.Source.Ticket);
            var correctedPreparation=new RevenueDeliveryAcceptancePreparation(Guid.NewGuid().ToString("N"),preparation.Company,"actor",RevenueDeliveryAcceptanceCorrection.NormalizeSource(current.Source,decided),preparation.Facts with{AcceptedQuantity=4m,AcceptedQuantityUnit="BBL",EvidenceReference="corrected-receiving-reference",Notes="Original preparer corrected rejected receiving facts"});
            var nativeContext=new Mock<ICompanyAccountingReadContext>();nativeContext.Setup(value=>value.CaptureAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((new RepositoryCompanyAccess(preparation.Company,["ConfiguredCompanyRole"],permissions),correctedPreparation.Source.Source.Source.BindingRevision,"actor"));
            var nativeReader=new CompanyFieldRevenueReads(f.Service,nativeContext.Object,assets.Object,()=>Task.FromResult(f.Native.Connection));var maker=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"actor"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"app"));
            var corrected=await f.Service.CorrectDeliveryAcceptanceAsync(new(decided,proof.Revision,correctedPreparation,"Correct rejected original receiving evidence"),"production",async()=> (await nativeReader.ReadDeliveryAcceptanceAsync(maker,"field",From,Through,original.Source.TransactionId)).Source,()=>Task.CompletedTask);
            Assert.Null(corrected.Decision);Actor("reviewer");var graph=JsonSerializer.Serialize(native.Native.Rows);var workflowWrites=native.Native.Writes;var productionWrites=f.Native.Writes;
            var historical=await processes.GetCompanyDeliverySourceAsync(receipt.InstanceId);historical.Require(preparation.Company,"reviewer",receipt.InstanceId);Assert.True(historical.IsHistorical);Assert.Equal(JsonSerializer.Serialize(decided),JsonSerializer.Serialize(historical.Delivery.Recorded));Assert.Equal("acceptance-reference",historical.Delivery.Recorded!.Preparation.Facts.EvidenceReference);Assert.Null(historical.Delivery.ApprovedAcceptedQuantity);
            await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCompanyAccountingStepAsync(receipt.InstanceId));await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCompanyAccountingStartCatalogAsync(handoffQuery));await Assert.ThrowsAnyAsync<Exception>(()=>processes.DiscoverNativeDecisionHandoffAsync(handoffQuery));
            if(retained is not null)await Assert.ThrowsAnyAsync<Exception>(()=>processes.CompleteCompanyAccountingStepAsync(new(retained,Guid.NewGuid().ToString("N"),"Obsolete rejected receiving version must not complete",CompanyAccountingSourceTypes.RouteDeliveryRejection)));
            var latest=await accounting.GetDeliveryAcceptanceAsync("field",From,Through,original.Source.TransactionId);Assert.False(latest.IsHistorical);Assert.Equal(corrected.Revision,latest.Recorded!.Revision);Assert.Null(latest.Recorded.Decision);
            var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("different-global-field");var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<CompanyAccountingStepState>();services.AddScoped<DeliveryAcceptanceWorkspaceState>();services.AddSingleton<IBusinessProcessServiceClient>(processes);services.AddSingleton<IAccountingServiceClient>(accounting);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new CommercialAuth("reviewer"));services.AddSingleton<NavigationManager>(new CommercialProcessNavigation(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",receipt.InstanceId,"RUN_TICKET")));services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,CommercialProcessComponents>();
            await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async()=>{
                var root=await renderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",receipt.InstanceId}}));var components=(CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>();var page=components.Page!;var html=root.ToHtmlString();
                Assert.Contains("Historical delivery receiving evidence",html);Assert.Contains("acceptance-reference",html);Assert.Contains("Recorded decision: Reject",html);Assert.Contains("read-only",html);Assert.DoesNotContain("corrected-receiving-reference",html);Assert.DoesNotContain("Approve reviewed delivery acceptance",html);Assert.DoesNotContain("Review current step authority",html);Assert.DoesNotContain("Record delivery preparation for independent review",html);Assert.Null(components.Delivery);Assert.Null(provider.GetRequiredService<CompanyAccountingStepState>().Review);
                foreach(var method in new[]{"ReviewStep","CompleteStep"})await (Task)page.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
                Assert.Null(provider.GetRequiredService<CompanyAccountingStepState>().Intent);
            });
            fields.Verify(value=>value.GetCurrentFieldIdAsync(),Times.Never);fields.Verify(value=>value.SetCurrentFieldAsync(It.IsAny<SetActiveFieldRequest>(),It.IsAny<CancellationToken>()),Times.Never);Assert.Equal(graph,JsonSerializer.Serialize(native.Native.Rows));Assert.Equal(workflowWrites,native.Native.Writes);Assert.Equal(productionWrites,f.Native.Writes);
        }
        if(mode=="history-active"){var retained=(await processes.ReviewCompanyAccountingStepAsync(receipt.InstanceId))!;Assert.Equal("DELIVERY_REJECTION_RECORDED",retained.Eligibility);await VerifyCorrectedHistory(retained);return;}
        await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCompanyAccountingStartAsync(query));
        if(mode=="source"){f.Native.Rows.OfType<REVENUE_TRANSACTION>().Single().GROSS_REVENUE=999;await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCompanyAccountingStepAsync(receipt.InstanceId));await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCompanyDeliverySourceAsync(receipt.InstanceId));return;}
        if(mode=="field"){assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCompanyAccountingStepAsync(receipt.InstanceId));await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCompanyDeliverySourceAsync(receipt.InstanceId));return;}
        if(mode=="other-reviewer")Actor("other-reviewer");if(mode=="grant")host.Context.Permissions=["Accounting.View","Workflow.Approve"];
        review=(await processes.ReviewCompanyAccountingStepAsync(receipt.InstanceId))!;Assert.Equal(decided.Revision,review.AccountingEvidenceRevision);
        if(mode is "other-reviewer" or "grant"){Assert.False(review.Eligible);Assert.Equal(mode=="grant"?"DELIVERY_APPROVAL_PERMISSION_REQUIRED":"DELIVERY_REVIEWER_REQUIRED",review.Eligibility);return;}
        Assert.Equal(rejecting?"DELIVERY_REJECTION_RECORDED":"ELIGIBLE",review.Eligibility);
        const string completionNotes="Close against exact recorded delivery decision";
        var completionAction=rejecting?CompanyAccountingSourceTypes.RouteDeliveryRejection:null;
        using var completion=new CompanyAccountingStepState(company);
        var beforeCompletion=(await processes.GetCompanyAccountingInstanceAsync(receipt.InstanceId))!;
        completion.AttachProcess(receipt.InstanceId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(beforeCompletion)));completion.AcceptReview(review);
        var graph=JsonSerializer.Serialize(native.Native.Rows);var before=native.Native.Writes;
        if(mode=="rollback")host.Context.OnCapture=_=>{if(native.Native.Writes>before)host.Context.Permissions=["Accounting.View"];};
        if(mode=="lost")native.Native.FailCommit=true;
        if(mode is "lost" or "rollback")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>completion.SubmitAsync(processes,completionNotes,completionAction));
            if(mode=="rollback"){Assert.Equal(graph,JsonSerializer.Serialize(native.Native.Rows));return;}native.Native.FailCommit=false;
        }
        // The existing scoped Web owner must validate delivery receipts and terminal history as well.
        var retainedIntent=completion.Intent;
        await completion.SubmitAsync(processes,completionNotes,completionAction);
        if(mode=="lost")Assert.Equal(retainedIntent,completion.Intent);
        var completed=completion.Receipt!;Assert.NotNull(completed);var command=completion.Intent!;completed.Require(command);var completedWrites=native.Native.Writes;
        Assert.Equal(completed.RecordedRevision,(await processes.CompleteCompanyAccountingStepAsync(command)).RecordedRevision);Assert.Equal(completedWrites,native.Native.Writes);
        if(rejecting)
        {
            Assert.NotNull(completed.NextStepInstanceId);Assert.NotEmpty(completed.NextTaskIds);
            Assert.Equal("DELIVERY_PREPARER_REQUIRED",(await processes.ReviewCompanyAccountingStepAsync(receipt.InstanceId))!.Eligibility);Actor("actor");
            var originalReview=(await processes.ReviewCompanyAccountingStepAsync(receipt.InstanceId))!;Assert.True(originalReview.Eligible);Assert.False(originalReview.RequiresIndependentActor);
            var close=new CompanyAccountingStepCommand(originalReview,Guid.NewGuid().ToString("N"),"Original preparer reviewed rejected receiving evidence");
            var closed=await processes.CompleteCompanyAccountingStepAsync(close);Assert.Null(closed.NextStepInstanceId);
            using var finalDetail=new ProcessInstanceDetailState(company,_=>throw new InvalidOperationException(),_=>throw new InvalidOperationException());
            await finalDetail.LoadAccountingAsync(receipt.InstanceId,id=>processes.GetCompanyAccountingInstanceAsync(id));
            Assert.NotNull(finalDetail.Instance);
            var process=(await processes.GetCompanyAccountingInstanceAsync(receipt.InstanceId))!;Assert.Equal("REJECTION_REVIEWED",process.CurrentState);Assert.Equal("actor",process.CompletedBy);
            Assert.Contains(process.History,row=>row.Action=="DELIVERY_REJECTION_ROUTED");Assert.Contains(process.History,row=>row.Action=="DELIVERY_REJECTION_REVIEWED");
        }
        else{Assert.Null(completed.NextStepInstanceId);Assert.Equal(Beep.OilandGas.Models.Processes.ProcessStatus.COMPLETED,(await processes.GetCompanyAccountingInstanceAsync(receipt.InstanceId))!.Status);}
        Assert.Equal(4,f.Native.Writes);
        if(mode=="history-closed")await VerifyCorrectedHistory();
        if(mode.StartsWith("correction-",StringComparison.Ordinal))
        {
            var correctionReview=await accounting.GetDeliveryCorrectionReviewAsync(receipt.InstanceId);correctionReview.Require(preparation.Company,"actor",receipt.InstanceId);
            var facts=preparation.Facts with{AcceptedQuantity=4m,AcceptedQuantityUnit="BBL",EvidenceReference="corrected-explicit-evidence",Notes="Original preparer corrected rejected receiving facts"};
            var correction=new CompanyDeliveryCorrectionSubmission(correctionReview,correctionReview.CreateIntent(Guid.NewGuid().ToString("N"),facts,"Correct rejected partial receiving evidence"));correction.Require();
            var envelope=new RevenueDeliveryAcceptanceIntent(null,null){Correction=correction};var oldJournal=f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Select(row=>JsonSerializer.Serialize(row)).ToArray();
            if(mode=="correction-role")host.Context.Roles=["OtherRole"];if(mode=="correction-grant")host.Context.Permissions=["Accounting.View"];if(mode=="correction-field")assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
            if(mode=="correction-source")f.Native.Rows.OfType<REVENUE_TRANSACTION>().Single().GROSS_REVENUE=999;if(mode=="correction-process")native.Definition.PROCESS_CONFIG_JSON=native.Definition.PROCESS_CONFIG_JSON.Replace("Independent delivery acceptance","Changed native delivery approval",StringComparison.Ordinal);
            if(mode=="correction-predecessor")correction=correction with{Correction=correction.Correction with{PreviousNativeRevision=new string('a',64)}};
            if(mode=="correction-quantity")correction=correction with{Correction=correction.Correction with{Preparation=correction.Correction.Preparation with{Facts=facts with{AcceptedQuantity=8m}}}};
            if(mode is "correction-role" or "correction-grant" or "correction-field" or "correction-source" or "correction-process" or "correction-predecessor" or "correction-quantity")
            {await Assert.ThrowsAnyAsync<Exception>(()=>accounting.CorrectDeliveryAcceptanceAsync(correction));Assert.Equal(4,f.Native.Writes);return;}
            if(mode.StartsWith("correction-form",StringComparison.Ordinal))
            {
                var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("unrelated-global-field");
                var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<CompanyAccountingStepState>();services.AddScoped<DeliveryAcceptanceWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();
                services.AddSingleton<IBusinessProcessServiceClient>(processes);services.AddSingleton<IAccountingServiceClient>(accounting);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new DeliveryAuth(company));services.AddSingleton<NavigationManager>(new CommercialProcessNavigation(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",receipt.InstanceId,"RUN_TICKET")));services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,CommercialProcessComponents>();
                await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
                await renderer.Dispatcher.InvokeAsync(async()=>{
                    var root=await renderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",receipt.InstanceId}}));var components=(CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>();var page=components.Page!;
                    async Task Redraw(){typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);await root.QuiescenceTask;var deadline=DateTime.UtcNow.AddSeconds(5);while(provider.GetRequiredService<DeliveryAcceptanceWorkspaceState>().Busy && DateTime.UtcNow<deadline)await Task.Delay(10);}
                    await (Task)page.GetType().GetMethod("ReviewDeliveryCorrection",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;await Redraw();Assert.Contains("Receiving correction reason",root.ToHtmlString());Assert.Contains("Record correction for independent review",root.ToHtmlString());var panel=components.Delivery!;var type=panel.GetType();Assert.Null(type.GetField("_quantity",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));
                    if(mode=="correction-form-auth"){((DeliveryAuth)provider.GetRequiredService<AuthenticationStateProvider>()).Revoke();await Redraw();Assert.DoesNotContain("Record correction for independent review",root.ToHtmlString());Assert.DoesNotContain("acceptance-reference",root.ToHtmlString());Assert.Equal(4,f.Native.Writes);return;}
                    foreach(var entry in new Dictionary<string,object?>{{"_dateUtc",facts.AcceptedAtUtc.Date},{"_timeUtc",facts.AcceptedAtUtc.TimeOfDay},{"_quantity",mode=="correction-form-quantity"?8m:facts.AcceptedQuantity},{"_buyer",facts.ReceivingBusinessAssociateId},{"_reference",facts.EvidenceReference},{"_notes",facts.Notes},{"_correctionReason",mode=="correction-form-reason"?"":correction.Correction.Reason}})type.GetField(entry.Key,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,entry.Value);
                    typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null);await root.QuiescenceTask;await (Task)type.GetMethod("Correct",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;await Redraw();
                    if(mode!="correction-form"){Assert.Equal(4,f.Native.Writes);Assert.False(provider.GetRequiredService<DeliveryAcceptanceWorkspaceState>().HasPendingIntent);return;}
                    Assert.Equal(6,f.Native.Writes);Assert.Contains("Historical delivery receiving evidence",root.ToHtmlString());Assert.DoesNotContain("Record correction for independent review",root.ToHtmlString());Assert.DoesNotContain("Approve reviewed delivery acceptance",root.ToHtmlString());
                    // The form owns its UUID, so subsequent assertions use its exact retained journal command.
                    var saved=f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Single(row=>row.INTENT_KIND=="CORRECTION");envelope=saved.Restore(preparation.Company,"actor",saved.SCOPE_KEY);correction=envelope.Correction!;Assert.Contains("/accounting/delivery-intents/"+saved.DELIVERY_ACCEPTANCE_INTENT_ID,root.ToHtmlString());
                });
                fields.Verify(value=>value.GetCurrentFieldIdAsync(),Times.Never);fields.Verify(value=>value.SetCurrentFieldAsync(It.IsAny<SetActiveFieldRequest>(),It.IsAny<CancellationToken>()),Times.Never);if(mode!="correction-form")return;
            }
            else if(mode=="correction-recovery-screen")
            {
                var saved=await accounting.SaveDeliveryIntentAsync(envelope);Assert.Equal(5,f.Native.Writes);var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("other-global-field");
                await using var provider=DeliveryServices(company,accounting,fields,services=>{services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<IBusinessProcessServiceClient>(processes);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));});await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
                await renderer.Dispatcher.InvokeAsync(async()=>{
                    var root=await renderer.RenderComponentAsync<DeliverySavedIntent>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"IntentId",saved.DELIVERY_ACCEPTANCE_INTENT_ID}}));var html=root.ToHtmlString();Assert.Contains("CORRECTION",html);Assert.Contains("Retained correction",html);Assert.Contains(correction.Correction.Reason,html);Assert.Contains("Retry original delivery intent",html);Assert.Equal(5,f.Native.Writes);
                    var panel=provider.GetRequiredService<DeliveryComponents>().Panel!;await (Task)panel.GetType().GetMethod("Retry",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;await root.QuiescenceTask;
                    var recoveryState=provider.GetRequiredService<DeliveryAcceptanceWorkspaceState>();var deadline=DateTime.UtcNow.AddSeconds(5);while(recoveryState.Busy && DateTime.UtcNow<deadline)await Task.Delay(10);
                    Assert.False(recoveryState.Busy);Assert.False(recoveryState.HasPendingIntent,"Writes="+f.Native.Writes+"; error="+provider.GetRequiredService<DeliveryComponents>().Panel!.GetType().GetField("_error",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(provider.GetRequiredService<DeliveryComponents>().Panel));Assert.Equal(6,f.Native.Writes);
                });
                fields.Verify(value=>value.GetCurrentFieldIdAsync(),Times.Never);fields.Verify(value=>value.SetCurrentFieldAsync(It.IsAny<SetActiveFieldRequest>(),It.IsAny<CancellationToken>()),Times.Never);
            }
            else if(mode is "correction-state" or "correction-recovery")
            {
                var saved=await accounting.SaveDeliveryIntentAsync(envelope);Assert.Equal("CORRECTION",saved.INTENT_KIND);Assert.Equal(5,f.Native.Writes);
                using var state=new DeliveryAcceptanceWorkspaceState(company);state.Attach("field",From,Through,original.Source.TransactionId);state.RecoverSaved(saved);await state.LoadAsync(accounting);Assert.True(state.HasPendingIntent);Assert.True(state.CanRetryPending);Assert.Equal(JsonSerializer.Serialize(envelope),JsonSerializer.Serialize(state.PendingOriginal));Assert.Equal(5,f.Native.Writes);
                await state.SubmitPendingAsync(accounting);Assert.False(state.HasPendingIntent);Assert.Equal(6,f.Native.Writes);Assert.Equal(4m,state.Review!.Recorded!.Preparation.Facts.AcceptedQuantity);
                if(mode=="correction-recovery"){using var restored=new DeliveryAcceptanceWorkspaceState(company);restored.Attach("field",From,Through,original.Source.TransactionId);restored.RecoverSaved(saved);await restored.LoadAsync(accounting);Assert.False(restored.HasPendingIntent);Assert.Equal(6,f.Native.Writes);}
            }
            else
            {
                if(mode=="correction-save-lost"){f.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>accounting.SaveDeliveryIntentAsync(envelope));Assert.Equal(5,f.Native.Writes);f.Native.FailCommit=false;}
                if(mode=="correction-business-lost")f.Native.Source.Setup(value=>value.UpdateEntity("RUN_TICKET",It.IsAny<object>())).Returns((string table,object value)=>{++f.Native.Writes;f.Native.Rows.RemoveAll(row=>row is RUN_TICKET);f.Native.Rows.Add(CompanyInventorySourceReview.Copy((RUN_TICKET)value));f.Native.FailCommit=true;return new ErrorsInfo{Flag=Errors.Ok};});
                if(mode=="correction-business-lost"){await Assert.ThrowsAnyAsync<Exception>(()=>accounting.CorrectDeliveryAcceptanceAsync(correction));Assert.Equal(6,f.Native.Writes);f.Native.FailCommit=false;}
                var result=await accounting.CorrectDeliveryAcceptanceAsync(correction);Assert.Null(result.Decision);Assert.Equal(JsonSerializer.Serialize(correction.Correction.Preparation),JsonSerializer.Serialize(result.Preparation));Assert.Equal(6,f.Native.Writes);Assert.Equal(result.Revision,(await accounting.CorrectDeliveryAcceptanceAsync(correction)).Revision);Assert.Equal(6,f.Native.Writes);
            }
            Assert.All(oldJournal,old=>Assert.Contains(f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>(),row=>JsonSerializer.Serialize(row)==old));var recorded=f.Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Single(row=>row.INTENT_KIND=="CORRECTION");Assert.Equal(JsonSerializer.Serialize(correction),JsonSerializer.Serialize(recorded.Restore(preparation.Company,"actor",recorded.SCOPE_KEY).Correction));
            var recovered=await accounting.GetDeliverySavedIntentAsync(recorded.DELIVERY_ACCEPTANCE_INTENT_ID);Assert.Equal("RECORDED",recovered.Status);Assert.True((await processes.GetCompanyDeliverySourceAsync(receipt.InstanceId)).IsHistorical);await Assert.ThrowsAnyAsync<Exception>(()=>accounting.GetDeliveryCorrectionReviewAsync(receipt.InstanceId));
            Assert.Equal(7m,f.Native.Rows.OfType<RUN_TICKET>().Single().NET_VOLUME);Assert.Equal(6,f.Native.Writes);
            if(mode=="correction-form")
            {
                var current=await accounting.GetDeliveryAcceptanceAsync("field",From,Through,original.Source.TransactionId);Assert.Null(current.Recorded!.Decision);Assert.Equal(4m,current.Recorded.Preparation.Facts.AcceptedQuantity);
                var nextQuery=new CompanyAccountingProcessStartQuery("definition","RUN_TICKET","ticket",CompanyAccountingSourceTypes.DeliveryPreparationRevision(current.Recorded));var nextReview=await processes.ReviewCompanyAccountingStartAsync(nextQuery);var nextIntent=nextReview!.CreateIntent(Guid.NewGuid().ToString("N"));var next=await processes.StartCompanyAccountingAsync(nextIntent);Assert.NotEqual(receipt.InstanceId,next.InstanceId);
                Assert.False((await processes.GetCompanyDeliverySourceAsync(next.InstanceId)).IsHistorical);Assert.True((await processes.GetCompanyDeliverySourceAsync(receipt.InstanceId)).IsHistorical);Assert.Equal(6,f.Native.Writes);
                await Assert.ThrowsAnyAsync<Exception>(()=>accounting.DecideDeliveryAcceptanceAsync(new(current.Recorded,new(Guid.NewGuid().ToString("N"),preparation.Company,"actor",current.Recorded.Revision,RevenueDeliveryAcceptanceDecision.Approve,"Maker cannot approve corrected evidence"))));Assert.Equal(6,f.Native.Writes);
                Actor("reviewer");Assert.Equal("DELIVERY_APPROVAL_REQUIRED",(await processes.ReviewCompanyAccountingStepAsync(next.InstanceId))!.Eligibility);
                current=await accounting.GetDeliveryAcceptanceAsync("field",From,Through,original.Source.TransactionId);var approval=await accounting.DecideDeliveryAcceptanceAsync(new(current.Recorded!,new(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",current.Recorded!.Revision,RevenueDeliveryAcceptanceDecision.Approve,"Independent review of the corrected partial receiving quantity")));Assert.Equal(8,f.Native.Writes);Assert.Equal(4m,approval.Preparation.Facts.AcceptedQuantity);
                var eligible=(await processes.ReviewCompanyAccountingStepAsync(next.InstanceId))!;Assert.True(eligible.Eligible);var finished=await processes.CompleteCompanyAccountingStepAsync(new(eligible,Guid.NewGuid().ToString("N"),"Completed independent corrected receiving review"));Assert.Null(finished.NextStepInstanceId);
                Assert.Equal(Beep.OilandGas.Models.Processes.ProcessStatus.COMPLETED,(await processes.GetCompanyAccountingInstanceAsync(next.InstanceId))!.Status);var historical=await processes.GetCompanyDeliverySourceAsync(receipt.InstanceId);Assert.True(historical.IsHistorical);Assert.Equal(RevenueDeliveryAcceptanceDecision.Reject,historical.Delivery.Recorded!.Decision!.Decision);Assert.Equal(preparation.Facts.AcceptedQuantity,historical.Delivery.Recorded.Preparation.Facts.AcceptedQuantity);Assert.Equal(7m,f.Native.Rows.OfType<RUN_TICKET>().Single().NET_VOLUME);
            }
        }
    }
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("other-reviewer")][InlineData("revoked")][InlineData("source-drift")][InlineData("unsupported-definition")][InlineData("persona-screen")]
    [InlineData("source-company")][InlineData("source-field")][InlineData("source-permission")][InlineData("source-role")]
    [InlineData("reject-route")][InlineData("approve-route")][InlineData("lost-route")][InlineData("rollback-route")][InlineData("other-preparer-route")]
    [InlineData("historical-route")][InlineData("historical-field-route")][InlineData("historical-permission-route")][InlineData("historical-role-route")]
    [InlineData("historical-company-route")][InlineData("historical-binding-route")][InlineData("historical-native-route")]
    [InlineData("amendment-uncertain-route")][InlineData("amendment-rollback-route")][InlineData("amendment-process-route")][InlineData("amendment-predecessor-route")][InlineData("amendment-reason-route")]
    [InlineData("amendment-definition-route")][InlineData("amendment-role-route")][InlineData("amendment-grant-route")][InlineData("amendment-source-route")][InlineData("amendment-audit-route")]
    [InlineData("amendment-form-route")][InlineData("amendment-form-lost-route")]
    [InlineData("amendment-form-multiversion-route")]
    [InlineData("amendment-intent-route")][InlineData("amendment-intent-uncertain-route")][InlineData("amendment-intent-role-route")][InlineData("amendment-intent-grant-route")][InlineData("amendment-intent-field-route")][InlineData("amendment-intent-process-route")][InlineData("amendment-intent-source-route")]
    public async Task CommercialCompanyHandoffRequiresOriginalNativeApprovalByCurrentIndependentReviewer(string mode)
    {
        var f=new Fixture();f.Native.Connection="production";f.Row.ROW_CREATED_BY="production-maker";f.Contract.ROW_CREATED_BY="contract-maker";
        var native=new CompanyProcessStartTests.Fixture("ConfiguredCompanyRole");CompanyStepCommandTests.ConfigureUpdates(native);
        var multiVersion=mode=="amendment-form-multiversion-route";
        var formJourney=mode is "amendment-form-route" or "amendment-form-lost-route" or "amendment-form-multiversion-route";
        using var rbac=mode=="persona-screen" || formJourney?new WorkflowInboxHttpTests.Fixture("COMMERCIAL","ConfiguredCompanyRole",formJourney?"actor":"reviewer"):null;
        if(rbac is not null)native.Native.Destinations=["COMMERCIAL"];
        if(formJourney)
        {
            rbac!.Repository.Users.Add(new OilGasUser{Id="new-reviewer",IsActive=true});
            rbac.Repository.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>{UserId="new-reviewer",RoleId="role"});
            rbac.Repository.Set<AppCompanyMembership>().Add(new(){TenantId="tenant",BusinessAssociateId="company",UserId="new-reviewer",GrantedByUserId="actor",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
            rbac.Repository.Set<AppUserRoleExtension>().Add(new(){TenantId="tenant",BusinessAssociateId="company",UserId="new-reviewer",RoleId="role",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
            rbac.Repository.Set<AppUserPersona>().Add(new(){UserId="new-reviewer",TenantId="tenant",BusinessAssociateId="company",PersonaCode="COMMERCIAL"});rbac.Repository.SaveChanges();
        }
        native.Definition.ENTITY_TYPE="SALES_CONTRACT";
        native.Definition.PROCESS_CONFIG_JSON=System.Text.Json.JsonSerializer.Serialize(new {Steps=new[]{new Beep.OilandGas.Models.Processes.ProcessStepDefinition{
            StepId="review",StepName="Commercial terms approval",SequenceNumber=1,RequiredRoles=["ConfiguredCompanyRole"],RequiresApproval=true,StepType="APPROVAL",
            StepConfiguration=new Dictionary<string,object>{{CompanyAccountingStepEvidence.ConfigurationKey,mode=="unsupported-definition"?"OTHER":CompanyAccountingSourceTypes.CommercialApproval}}}}});
        var twoStep=mode.EndsWith("-route",StringComparison.Ordinal);
        if(twoStep)
        {
            var steps=System.Text.Json.JsonSerializer.Deserialize<Beep.OilandGas.Models.Processes.ProcessDefinition>(System.Text.Json.JsonSerializer.Serialize(new {Steps=System.Text.Json.JsonDocument.Parse(native.Definition.PROCESS_CONFIG_JSON).RootElement.GetProperty("Steps")}))!.Steps;
            steps.Add(new(){StepId="rejection-review",StepName="Original preparer rejection review",SequenceNumber=2,RequiredRoles=["ConfiguredCompanyRole"],StepType="ACTION",StepConfiguration=new Dictionary<string,object>{{CompanyAccountingStepEvidence.ConfigurationKey,CompanyAccountingSourceTypes.CommercialRejectionReview}}});
            native.Definition.PROCESS_CONFIG_JSON=System.Text.Json.JsonSerializer.Serialize(new {Steps=steps});
        }
        string[] permissions=["Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve","Workflow.Start","Workflow.Approve","Workflow.Execute"];
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD",It.IsAny<string>(),null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        var amendmentLog=new Mock<ILogger<Beep.OilandGas.ApiService.Controllers.Accounting.Traditional.CompanyAccountingProcessController>>();
        await using var host=await AccountingSourceHttpTests.Start(permissions:permissions,configure:(services,context)=>{
            if(rbac is not null){rbac.Configure(services,context);services.AddSingleton<IProcessService>(native.Service);services.AddSingleton<Beep.OilandGas.LifeCycle.Services.Processes.ICrossPersonaTaskRouter>(native.Native.Router);}
            context.FixedProcess=native.Service;context.StartBinding=new string('B',64);
            services.AddSingleton(amendmentLog.Object);
            services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));
        });
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],permissions));
        var api=new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);
        var accounting=new AccountingServiceClient(api,NullLogger<AccountingServiceClient>.Instance);
        var processes=new BusinessProcessServiceClient(api,NullLogger<BusinessProcessServiceClient>.Instance);
        var source=await accounting.GetSalesContractTermsAsync("field",From,Through,"revenue-a");
        var preparation=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),source.Source.Source.Company,"actor",source.Source,CommercialPreparation().Terms);
        var prepared=await accounting.PrepareSalesContractTermsAsync(preparation);
        var query=new CompanyAccountingProcessStartQuery("definition","SALES_CONTRACT","contract",CompanyAccountingSourceTypes.CommercialPreparationRevision(prepared));
        if(mode=="unsupported-definition"){await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCompanyAccountingStartAsync(query));Assert.Equal(0,native.Native.Writes);return;}
        var startReview=await processes.ReviewCompanyAccountingStartAsync(query);
        Assert.Equal("actor",startReview.SourcePreparerUserId);
        var catalog=await processes.GetCompanyAccountingStartCatalogAsync(new("SALES_CONTRACT","contract",query.ExpectedSourceRevision!));Assert.Single(catalog.Choices);
        var started=await processes.StartCompanyAccountingAsync(startReview.CreateIntent(Guid.NewGuid().ToString("N")));
        Assert.All(native.Native.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.CROSS_PERSONA_TASK>(),task=>Assert.Equal(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",started.InstanceId,"SALES_CONTRACT"),task.ROUTE));
        void Actor(string actor){host.Context.Actor=actor;host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",actor);company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],permissions));}
        Actor("reviewer");
        if(mode.StartsWith("source-",StringComparison.Ordinal) && mode!="source-drift")
        {
            if(mode=="source-company")company.TrySelect(company.Clear(),"reviewer",new(new("tenant","second"),["ConfiguredCompanyRole"],permissions));
            if(mode=="source-field")assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
            if(mode=="source-permission")host.Context.Permissions=["Workflow.Approve"];
            if(mode=="source-role")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");
            var deniedWrites=native.Native.Writes;await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCompanyCommercialSourceAsync(started.InstanceId));Assert.Equal(deniedWrites,native.Native.Writes);Assert.Equal(1,f.Native.Writes);return;
        }
        if(mode=="persona-screen")
        {
            var inboxClient=new UnifiedTaskInboxService(api);var inbox=await inboxClient.GetInboxAsync("COMMERCIAL");
            var task=Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));
            Assert.Equal(started.InstanceId,task.WorkflowName);Assert.Equal("SALES_CONTRACT",task.EntityType);Assert.Equal("COMMERCIAL_APPROVAL_REQUIRED",task.AccountingStepReview!.Eligibility);
            var exact=await processes.GetCompanyCommercialSourceAsync(started.InstanceId);Assert.Equal(From,exact.Terms.Source.Source.From);Assert.Equal(Through,exact.Terms.Source.Source.Through);Assert.Equal("revenue-a",exact.Terms.Source.TransactionId);
            var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("another-global-field");
            var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<CompanyAccountingStepState>();services.AddScoped<CommercialTermsWorkspaceState>();services.AddScoped<CommercialAmendmentWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();
            services.AddSingleton<IBusinessProcessServiceClient>(processes);services.AddSingleton<IAccountingServiceClient>(accounting);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new CommercialAuth("reviewer"));
            services.AddSingleton<NavigationManager>(new CommercialProcessNavigation(task.Route));services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,CommercialProcessComponents>();
            await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async()=>{
                var personas=new Mock<IPersonaContextService>();personas.SetupGet(value=>value.CurrentPersona).Returns(new AppPersona{Code="COMMERCIAL",Name="Commercial",DefaultRoute="/accounting/revenue-recognition"});personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId="reviewer",PersonaCode="COMMERCIAL"});personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
                using var queue=new PersonaTaskInboxState(inboxClient,company,personas.Object);queue.Start();await queue.LoadAsync();queue.SelectSource("SALES_CONTRACT");queue.SelectAccountingEvidence("COMMERCIAL_APPROVAL_REQUIRED");
                var assigned=await renderer.RenderComponentAsync<PersonaAssignedWorkPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",queue},{"WorkspaceName","Commercial"}}));
                Assert.Contains(task.Route,assigned.ToHtmlString());Assert.Contains("Commercial approval needed: 1",assigned.ToHtmlString());Assert.Contains("Open the process to review the recorded commercial terms",assigned.ToHtmlString());Assert.Single(queue.VisibleRows);
                var root=await renderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",started.InstanceId}}));
                var components=(CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>();var page=components.Page!;
                async Task Invoke(object component,string method,params object?[] args){await (Task)component.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(component,args)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);await root.QuiescenceTask;var deadline=DateTime.UtcNow.AddSeconds(5);while(components.Terms is {} active && DateTime.UtcNow<deadline && ((bool)active.GetType().GetField("_authorized",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(active)! == false || provider.GetRequiredService<CommercialTermsWorkspaceState>().Busy)){await Task.Delay(10);} }
                Assert.Contains("Revenue transaction: revenue-a",root.ToHtmlString());Assert.Contains("Approve reviewed terms",root.ToHtmlString());Assert.DoesNotContain("Hand off this source",root.ToHtmlString());
                await Invoke(page,"ReviewStep");Assert.Contains("Record an independent approval",root.ToHtmlString());
                var panel=components.Terms!;panel.GetType().GetField("_reason",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,"Independent commercial persona reviewed the exact recorded source");
                await Invoke(panel,"Decide",SalesContractTermsDecision.Approve);
                Assert.Contains("Recorded decision: Approve",root.ToHtmlString());Assert.Null(provider.GetRequiredService<CompanyAccountingStepState>().Review);
                await Invoke(page,"ReviewStep");Assert.Contains("Completion reason",root.ToHtmlString());
                page.GetType().GetField("_notes",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,"Complete against the recorded commercial approval");
                await Invoke(page,"CompleteStep");Assert.Contains("process reload confirmed",root.ToHtmlString());
                Assert.NotNull(provider.GetRequiredService<CompanyAccountingStepState>().Receipt);
                await queue.LoadAsync();Assert.Empty(queue.VisibleRows);Assert.Equal(0,queue.SourceCount("SALES_CONTRACT"));
            });
            var empty=await inboxClient.GetInboxAsync("COMMERCIAL");Assert.Empty(empty.CriticalTasks.Concat(empty.HighPriorityTasks).Concat(empty.NormalTasks));
            Assert.Equal(2,f.Native.Writes);fields.Verify(value=>value.SetCurrentFieldAsync(It.IsAny<SetActiveFieldRequest>(),It.IsAny<CancellationToken>()),Times.Never);return;
        }
        var before=await processes.ReviewCompanyAccountingStepAsync(started.InstanceId);Assert.False(before!.Eligible);Assert.Equal("COMMERCIAL_APPROVAL_REQUIRED",before.Eligibility);
        if(twoStep)await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCommercialAmendmentAsync(started.InstanceId));
        var decision=new SalesContractTermsDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",prepared.Revision,mode=="reject" || twoStep && mode!="approve-route"?SalesContractTermsDecision.Reject:SalesContractTermsDecision.Approve,"Independently verified recorded terms");
        var approved=await accounting.DecideSalesContractTermsAsync(new(prepared,decision));
        Assert.Equal(query.ExpectedSourceRevision,CompanyAccountingSourceTypes.CommercialPreparationRevision(approved));
        await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCompanyAccountingStartAsync(query));
        if(mode=="other-reviewer")Actor("other-reviewer");
        if(mode=="revoked")host.Context.Permissions=["Accounting.View","Workflow.Approve"];
        if(mode=="source-drift"){f.Row.GROSS_REVENUE=999;await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCompanyAccountingStepAsync(started.InstanceId));return;}
        var reviewed=await processes.ReviewCompanyAccountingStepAsync(started.InstanceId);
        Assert.Equal(approved.Revision,reviewed!.AccountingEvidenceRevision);
        if(twoStep && mode!="approve-route")
        {
            Assert.False(reviewed.Eligible);Assert.Equal("COMMERCIAL_REJECTION_RECORDED",reviewed.Eligibility);
            var routing=new CompanyAccountingStepCommand(reviewed,Guid.NewGuid().ToString("N"),"Return the recorded rejection for original preparer review",CompanyAccountingSourceTypes.RouteCommercialRejection);
            var originalGraph=System.Text.Json.JsonSerializer.Serialize(native.Native.Rows);var routeWrites=native.Native.Writes;
            if(mode=="rollback-route")host.Context.OnCapture=_=>{if(native.Native.Writes>routeWrites)host.Context.Permissions=["Accounting.View"];};
            if(mode=="lost-route")native.Native.FailCommit=true;
            if(mode is "rollback-route" or "lost-route")
            {
                await Assert.ThrowsAnyAsync<Exception>(()=>processes.CompleteCompanyAccountingStepAsync(routing));
                if(mode=="rollback-route"){Assert.Equal(originalGraph,System.Text.Json.JsonSerializer.Serialize(native.Native.Rows));Assert.Equal("rollback",native.Native.Transactions.Last());return;}
                native.Native.FailCommit=false;
            }
            CompanyAccountingStepReceipt routed;
            if(mode=="reject-route")
            {
                var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("another-global-field");
                var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<CompanyAccountingStepState>();services.AddScoped<CommercialTermsWorkspaceState>();services.AddScoped<CommercialAmendmentWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<IBusinessProcessServiceClient>(processes);services.AddSingleton<IAccountingServiceClient>(accounting);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new CommercialAuth("reviewer"));services.AddSingleton<NavigationManager>(new CommercialProcessNavigation(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",started.InstanceId,"SALES_CONTRACT")));services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,CommercialProcessComponents>();
                await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
                await renderer.Dispatcher.InvokeAsync(async()=>{
                    var root=await renderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",started.InstanceId}}));var page=((CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>()).Page!;
                    await (Task)page.GetType().GetMethod("ReviewStep",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
                    Assert.Contains("Route recorded rejection to original preparer",root.ToHtmlString());Assert.Contains("Recorded decision: Reject",root.ToHtmlString());
                    page.GetType().GetField("_notes",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,routing.Notes);
                    await (Task)page.GetType().GetMethod("CompleteStep",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
                });
                var state=provider.GetRequiredService<CompanyAccountingStepState>();Assert.NotNull(state.Receipt);routed=state.Receipt!;routing=state.Intent!;
            }
            else routed=await processes.CompleteCompanyAccountingStepAsync(routing);
            routed.Require(routing);Assert.NotNull(routed.NextStepInstanceId);Assert.NotEmpty(routed.NextTaskIds);
            var routedWrites=native.Native.Writes;Assert.Equal(routed.RecordedRevision,(await processes.CompleteCompanyAccountingStepAsync(routing)).RecordedRevision);Assert.Equal(routedWrites,native.Native.Writes);
            Assert.Equal("REJECTED",Assert.Single(native.Native.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_APPROVAL>()).APPROVAL_STATUS);
            var other=await processes.ReviewCompanyAccountingStepAsync(started.InstanceId);Assert.Equal("COMMERCIAL_PREPARER_REQUIRED",other!.Eligibility);
            if(mode=="other-preparer-route")return;
            Actor("actor");host.Context.Permissions=["Accounting.View","Workflow.Execute"];
            Assert.Equal("COMMERCIAL_PREPARATION_PERMISSION_REQUIRED",(await processes.ReviewCompanyAccountingStepAsync(started.InstanceId))!.Eligibility);host.Context.Permissions=permissions;
            var preparer=(await processes.ReviewCompanyAccountingStepAsync(started.InstanceId))!;Assert.True(preparer.Eligible);Assert.False(preparer.RequiresIndependentActor);
            using var completion=new CompanyAccountingStepState(company);var graph=(await processes.GetCompanyAccountingInstanceAsync(started.InstanceId))!;completion.AttachProcess(started.InstanceId,ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(graph)));completion.AcceptReview(preparer);
            if(formJourney)
            {
                var inboxClient=new UnifiedTaskInboxService(api);var inbox=await inboxClient.GetInboxAsync("COMMERCIAL");var task=Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));
                Assert.Equal(started.InstanceId,task.WorkflowName);Assert.Equal("ELIGIBLE",task.AccountingStepReview!.Eligibility);Assert.False(task.AccountingStepReview.RequiresIndependentActor);Assert.Equal("actor",task.AccountingStepReview.SourceMakerUserId);
                WorkflowInboxReview.RequireTask(task,["ConfiguredCompanyRole"]);
                var foreignMaker=CompanyInventorySourceReview.Copy(task);foreignMaker.AccountingStepReview=foreignMaker.AccountingStepReview! with{SourceMakerUserId="other-preparer"};Assert.Throws<InvalidOperationException>(()=>WorkflowInboxReview.RequireTask(foreignMaker,["ConfiguredCompanyRole"]));
                var foreignType=CompanyInventorySourceReview.Copy(task);foreignType.EntityType="AP_INVOICE";foreignType.AccountingStepReview=foreignType.AccountingStepReview! with{EntityType="AP_INVOICE"};Assert.Throws<InvalidOperationException>(()=>WorkflowInboxReview.RequireTask(foreignType,["ConfiguredCompanyRole"]));
                var assignedFields=new Mock<IFieldSelectionContext>();assignedFields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("another-global-field");
                var assignedServices=new ServiceCollection();assignedServices.AddLogging();assignedServices.AddMudServices();assignedServices.AddSingleton(company);assignedServices.AddScoped<CompanyAccountingStepState>();assignedServices.AddScoped<CommercialTermsWorkspaceState>();assignedServices.AddScoped<CommercialAmendmentWorkspaceState>();assignedServices.AddScoped<CompanyAccountingHandoffState>();assignedServices.AddSingleton<IBusinessProcessServiceClient>(processes);assignedServices.AddSingleton<IAccountingServiceClient>(accounting);assignedServices.AddSingleton(assignedFields.Object);assignedServices.AddSingleton<AuthenticationStateProvider>(new CommercialAuth("actor"));assignedServices.AddSingleton<NavigationManager>(new CommercialProcessNavigation(task.Route));assignedServices.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();assignedServices.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));assignedServices.AddSingleton<IComponentActivator,CommercialProcessComponents>();
                await using var assignedProvider=assignedServices.BuildServiceProvider();await using var assignedRenderer=new HtmlRenderer(assignedProvider,assignedProvider.GetRequiredService<ILoggerFactory>());
                await assignedRenderer.Dispatcher.InvokeAsync(async()=>{
                    var personas=new Mock<IPersonaContextService>();personas.SetupGet(value=>value.CurrentPersona).Returns(new AppPersona{Code="COMMERCIAL",Name="Commercial",DefaultRoute="/accounting/revenue-recognition"});personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId="actor",PersonaCode="COMMERCIAL"});personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
                    using var queue=new PersonaTaskInboxState(inboxClient,company,personas.Object);queue.Start();await queue.LoadAsync();queue.SelectSource("SALES_CONTRACT");
                    var panel=await assignedRenderer.RenderComponentAsync<PersonaAssignedWorkPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",queue},{"WorkspaceName","Commercial"}}));Assert.Contains(task.Route,panel.ToHtmlString());Assert.Single(queue.VisibleRows);
                    var root=await assignedRenderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",started.InstanceId}}));var page=((CommercialProcessComponents)assignedProvider.GetRequiredService<IComponentActivator>()).Page!;
                    await (Task)page.GetType().GetMethod("ReviewStep",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
                    Assert.Contains("current original-preparer authority permit rejection-review completion",root.ToHtmlString());
                    page.GetType().GetField("_notes",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,"Reviewed the rejected version; amendment must be proposed separately");
                    await (Task)page.GetType().GetMethod("CompleteStep",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;await root.QuiescenceTask;
                    Assert.Equal("REJECTION_REVIEWED",(await processes.GetCompanyAccountingInstanceAsync(started.InstanceId))!.CurrentState);Assert.False(assignedProvider.GetRequiredService<CompanyAccountingStepState>().HasUnconfirmedIntent);await queue.LoadAsync();Assert.Empty(queue.VisibleRows);
                });
                assignedFields.Verify(value=>value.SetCurrentFieldAsync(It.IsAny<SetActiveFieldRequest>(),It.IsAny<CancellationToken>()),Times.Never);
            }
            else{await completion.SubmitAsync(processes,"Reviewed the rejected version; amendment must be proposed separately");Assert.NotNull(completion.Receipt);}
            var closed=(await processes.GetCompanyAccountingInstanceAsync(started.InstanceId))!;Assert.Equal("REJECTION_REVIEWED",closed.CurrentState);Assert.Contains(closed.History,row=>row.Action=="COMMERCIAL_REJECTION_ROUTED");Assert.Contains(closed.History,row=>row.Action=="COMMERCIAL_REJECTION_REVIEWED");
            using var terminalReview=new ProcessInstanceDetailState(company,_=>throw new Exception(),_=>throw new Exception());
            var falseApproval=CompanyInventorySourceReview.Copy(closed);falseApproval.CurrentState="COMPLETED";
            await Assert.ThrowsAsync<InvalidOperationException>(()=>terminalReview.LoadAccountingAsync(started.InstanceId,_=>Task.FromResult<Beep.OilandGas.Models.Processes.ProcessInstance?>(falseApproval)));Assert.Null(terminalReview.Instance);
            Assert.Equal(SalesContractTermsDecision.Reject,(await accounting.GetSalesContractTermsAsync("field",From,Through,"revenue-a")).Recorded!.Decision!.Decision);Assert.Equal(2,f.Native.Writes);
            if(mode.StartsWith("historical",StringComparison.Ordinal) || mode.StartsWith("amendment",StringComparison.Ordinal))
            {
                var current=await accounting.GetSalesContractTermsAsync("field",From,Through,"revenue-a");
                var predecessor=SalesContractTermsNativeProof.Require(preparation.Company,current.Source.Contract);
                Actor("other-preparer");await Assert.ThrowsAnyAsync<Exception>(()=>processes.ReviewCommercialAmendmentAsync(started.InstanceId));Actor("actor");
                f.Row.GROSS_REVENUE=999; // An explicit amendment can review fresh revenue after rejection.
                var capturedRejection=await processes.GetCompanyCommercialSourceAsync(started.InstanceId);Assert.True(capturedRejection.IsHistorical);Assert.Equal(20m,capturedRejection.Terms.Source.Source.Rows.Single().GROSS_REVENUE);
                var amendmentReview=await processes.ReviewCommercialAmendmentAsync(started.InstanceId);Assert.Equal(999m,amendmentReview.Source.Source.Rows.Single().GROSS_REVENUE);
                var amendment=amendmentReview.CreateIntent(Guid.NewGuid().ToString("N"),preparation.Terms with{Version="amended-v2",Price=30.125m},"Resolve the reviewed rejection");
                var submission=new CompanyCommercialAmendmentSubmission(amendmentReview,amendment);
                if(mode.StartsWith("amendment-intent",StringComparison.Ordinal))
                {
                    Assert.Empty((await processes.GetCommercialAmendmentQueueAsync()).Rows);
                    Assert.Empty((await processes.GetCommercialAmendmentPageAsync(1,8)).Queue.Rows);
                    Assert.Equal(HttpStatusCode.BadRequest,(await host.Client.GetAsync("/api/accounting/processes/commercial-amendment-queue/pages/0/8")).StatusCode);
                    Assert.Equal(HttpStatusCode.BadRequest,(await host.Client.GetAsync("/api/accounting/processes/commercial-amendment-queue/pages/1/33")).StatusCode);
                    Assert.Null(await processes.GetCommercialAmendmentIntentAsync(started.InstanceId));
                    var beforeSavedProcess=System.Text.Json.JsonSerializer.Serialize(native.Native.Rows);var originalContract=System.Text.Json.JsonSerializer.Serialize(f.Native.Rows.OfType<SALES_CONTRACT>().Single());
                    if(mode=="amendment-intent-role-route")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");
                    if(mode=="amendment-intent-grant-route")host.Context.Permissions=["Accounting.View"];
                    if(mode=="amendment-intent-field-route")assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD",It.IsAny<string>(),null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
                    if(mode=="amendment-intent-process-route")native.Native.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_STEP_INSTANCE>().Single(row=>row.STEP_ID=="rejection-review").OUTCOME="SUCCESS";
                    if(mode=="amendment-intent-source-route")f.Row.GROSS_REVENUE=998;
                    if(mode is not ("amendment-intent-route" or "amendment-intent-uncertain-route"))
                    {await Assert.ThrowsAnyAsync<Exception>(()=>processes.SaveCommercialAmendmentIntentAsync(submission));Assert.Equal(2,f.Native.Writes);Assert.Empty(f.Native.Rows.OfType<COMMERCIAL_TERMS_INTENT>());return;}
                    if(mode=="amendment-intent-uncertain-route"){f.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>processes.SaveCommercialAmendmentIntentAsync(submission));f.Native.FailCommit=false;}
                    var saved=await processes.SaveCommercialAmendmentIntentAsync(submission);Assert.Equal(3,f.Native.Writes);Assert.Equal(beforeSavedProcess,System.Text.Json.JsonSerializer.Serialize(native.Native.Rows));Assert.Equal(originalContract,System.Text.Json.JsonSerializer.Serialize(f.Native.Rows.OfType<SALES_CONTRACT>().Single()));
                    var recovered=await processes.GetCommercialAmendmentIntentAsync(started.InstanceId);Assert.Equal(saved.RECORDED_REVISION,recovered!.RECORDED_REVISION);Assert.Equal(submission.Amendment.Preparation.IntentId,recovered.Restore(preparation.Company,"actor",started.InstanceId).Amendment.Preparation.IntentId);
                    var pendingQueue=await processes.GetCommercialAmendmentQueueAsync();Assert.Equal("SAVED_PENDING",Assert.Single(pendingQueue.Rows).Status);
                    var pendingPage=await processes.GetCommercialAmendmentPageAsync(1,8);Assert.Equal(1,pendingPage.TotalRecords);Assert.Equal("SAVED_PENDING",Assert.Single(pendingPage.Queue.Rows).Status);
                    async Task RenderQueue(bool pending)
                    {
                        var queueServices=new ServiceCollection();queueServices.AddLogging();queueServices.AddMudServices();queueServices.AddSingleton(company);queueServices.AddSingleton<IBusinessProcessServiceClient>(processes);queueServices.AddSingleton<AuthenticationStateProvider>(new CommercialAuth("actor"));queueServices.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();queueServices.AddSingleton<NavigationManager>(new CommercialProcessNavigation("/accounting/revenue-workbench"));
                        await using var queueProvider=queueServices.BuildServiceProvider();await using var queueRenderer=new HtmlRenderer(queueProvider,queueProvider.GetRequiredService<ILoggerFactory>());
                        await queueRenderer.Dispatcher.InvokeAsync(async()=>{
                            var root=await queueRenderer.RenderComponentAsync<CommercialSavedProposalQueue>();var html=root.ToHtmlString();Assert.Contains("My saved commercial proposals",html);
                            if(pending){Assert.Contains("Saved; native amendment not recorded",html);Assert.Contains(saved.COMMERCIAL_TERMS_INTENT_ID,html);Assert.Contains("/accounting/processes/"+Uri.EscapeDataString(started.InstanceId),html);}
                            else{Assert.Contains("No unresolved saved commercial proposals",html);Assert.DoesNotContain("Saved; native amendment not recorded",html);}
                        });
                    }
                    await RenderQueue(true);Assert.Equal(3,f.Native.Writes);
                    var competing=new CompanyCommercialAmendmentSubmission(amendmentReview,amendmentReview.CreateIntent(Guid.NewGuid().ToString("N"),preparation.Terms with{Version="different-v2",Price=33m},"Different reviewed proposal"));await Assert.ThrowsAnyAsync<Exception>(()=>processes.SaveCommercialAmendmentIntentAsync(competing));await Assert.ThrowsAnyAsync<Exception>(()=>processes.SubmitCommercialAmendmentAsync(competing));Assert.Equal(3,f.Native.Writes);
                    f.Row.GROSS_REVENUE=998;Assert.Equal(saved.RECORDED_REVISION,(await processes.GetCommercialAmendmentIntentAsync(started.InstanceId))!.RECORDED_REVISION);f.Row.GROSS_REVENUE=999;
                    await processes.SubmitCommercialAmendmentAsync(recovered.Restore(preparation.Company,"actor",started.InstanceId));Assert.Equal(4,f.Native.Writes);
                    var recordedQueue=await processes.GetCommercialAmendmentQueueAsync();Assert.Equal("RECORDED",Assert.Single(recordedQueue.Rows).Status);Assert.Equal(amendment.Preparation.IntentId,recordedQueue.Rows.Single().Recorded()!.Preparation.IntentId);
                    Assert.Equal("RECORDED",Assert.Single((await processes.GetCommercialAmendmentPageAsync(1,8)).Queue.Rows).Status);
                    await RenderQueue(false);Assert.Equal(4,f.Native.Writes);
                    host.Client.DefaultRequestHeaders.Remove("X-Test-Role");await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCommercialAmendmentQueueAsync());await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCommercialAmendmentPageAsync(1,8));host.Client.DefaultRequestHeaders.Add("X-Test-Role","yes");
                    var oldPermissions=host.Context.Permissions;host.Context.Permissions=["Accounting.View"];await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCommercialAmendmentQueueAsync());await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCommercialAmendmentPageAsync(1,8));host.Context.Permissions=oldPermissions;
                    assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD",It.IsAny<string>(),null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCommercialAmendmentQueueAsync());await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCommercialAmendmentPageAsync(1,8));
                    assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD",It.IsAny<string>(),null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
                    Assert.Equal(HttpStatusCode.Conflict,(await host.Client.GetAsync("/api/accounting/processes/commercial-amendment-queue/pages/2/8")).StatusCode);
                    Assert.Equal(saved.RECORDED_REVISION,(await processes.GetCommercialAmendmentIntentAsync(started.InstanceId))!.RECORDED_REVISION);Assert.Equal(beforeSavedProcess,System.Text.Json.JsonSerializer.Serialize(native.Native.Rows));return;
                }
                if(mode=="amendment-process-route")submission=submission with{Review=amendmentReview with{ProcessRevision=new string('A',64)},Amendment=amendment with{Process=new(started.InstanceId,new string('A',64))}};
                if(mode=="amendment-predecessor-route")submission=submission with{Amendment=amendment with{PreviousNativeRevision=new string('A',64)}};
                if(mode is "amendment-process-route" or "amendment-predecessor-route"){await Assert.ThrowsAnyAsync<Exception>(()=>processes.SubmitCommercialAmendmentAsync(submission));Assert.Equal(2,f.Native.Writes);return;}
                if(mode is "amendment-definition-route" or "amendment-role-route" or "amendment-grant-route" or "amendment-source-route" or "amendment-audit-route")
                {
                    if(mode=="amendment-definition-route")native.Definition.DESCRIPTION="Changed after frozen amendment review";
                    if(mode=="amendment-role-route")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");
                    if(mode=="amendment-grant-route")host.Context.Permissions=["Accounting.View"];
                    if(mode=="amendment-source-route")f.Row.GROSS_REVENUE=998;
                    if(mode=="amendment-audit-route")native.Native.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_STEP_INSTANCE>().Single(row=>row.STEP_ID=="rejection-review").OUTCOME="SUCCESS";
                    await Assert.ThrowsAnyAsync<Exception>(()=>processes.SubmitCommercialAmendmentAsync(submission));Assert.Equal(2,f.Native.Writes);return;
                }
                if(mode=="amendment-rollback-route")f.AfterContractWrite=()=>host.Context.Permissions=["Accounting.View"];
                if(mode=="amendment-uncertain-route")f.Native.FailCommit=true;
                if(mode is "amendment-rollback-route" or "amendment-uncertain-route")
                {
                    var originalNative=System.Text.Json.JsonSerializer.Serialize(f.Native.Rows.OfType<SALES_CONTRACT>().Single());
                    await Assert.ThrowsAnyAsync<Exception>(()=>processes.SubmitCommercialAmendmentAsync(submission));
                    if(mode=="amendment-rollback-route"){Assert.Equal(originalNative,System.Text.Json.JsonSerializer.Serialize(f.Native.Rows.OfType<SALES_CONTRACT>().Single()));Assert.Single(f.Native.Rows.OfType<COMMERCIAL_TERMS_INTENT>());Assert.Equal("rollback",f.Native.Transactions.Last());return;}
                    f.Native.FailCommit=false;
                }
                SalesContractTermsRecord amended;
                CompanyAccountingProcessStartReceipt? formHandoff=null;
                if(formJourney)
                {
                    var formFields=new Mock<IFieldSelectionContext>();formFields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("another-global-field");
                    var formServices=new ServiceCollection();formServices.AddLogging();formServices.AddMudServices();formServices.AddSingleton(company);formServices.AddScoped<CompanyAccountingStepState>();formServices.AddScoped<CommercialTermsWorkspaceState>();formServices.AddScoped<CommercialAmendmentWorkspaceState>();formServices.AddScoped<CompanyAccountingHandoffState>();formServices.AddSingleton<IBusinessProcessServiceClient>(processes);formServices.AddSingleton<IAccountingServiceClient>(accounting);formServices.AddSingleton(formFields.Object);formServices.AddSingleton<AuthenticationStateProvider>(new CommercialAuth("actor"));formServices.AddSingleton<NavigationManager>(new CommercialProcessNavigation(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",started.InstanceId,"SALES_CONTRACT")));formServices.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();formServices.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));formServices.AddSingleton<IComponentActivator,CommercialProcessComponents>();
                    await using var formProvider=formServices.BuildServiceProvider();await using var formRenderer=new HtmlRenderer(formProvider,formProvider.GetRequiredService<ILoggerFactory>());
                    await formRenderer.Dispatcher.InvokeAsync(async()=>{
                        var root=await formRenderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",started.InstanceId}}));
                        var components=(CommercialProcessComponents)formProvider.GetRequiredService<IComponentActivator>();var panel=components.Amendment!;
                        Assert.Contains("New terms version",root.ToHtmlString());Assert.Contains("Fresh gross revenue: 999",root.ToHtmlString());Assert.DoesNotContain("Review current step authority",root.ToHtmlString());
                        void Draft(string name,object? value)=>panel.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,value);
                        Draft("_version","amended-v2");Draft("_from",preparation.Terms.EffectiveFrom);Draft("_through",preparation.Terms.EffectiveThrough);Draft("_currency",preparation.Terms.CurrencyCode);Draft("_price",30.125m);Draft("_basis",preparation.Terms.PriceUnit);Draft("_method",preparation.Terms.PricingMethod);Draft("_notes",preparation.Terms.Notes);Draft("_reason","Resolve the reviewed rejection");
                        typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null);await root.QuiescenceTask;
                        f.Native.FailCommit=mode=="amendment-form-lost-route";
                        await (Task)panel.GetType().GetMethod("Prepare",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;
                        var state=formProvider.GetRequiredService<CommercialAmendmentWorkspaceState>();
                        async Task Settle(){typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(components.Page,null);await root.QuiescenceTask;var deadline=DateTime.UtcNow.AddSeconds(5);while((state.Recorded is null || state.Handoff is null || state.Busy) && DateTime.UtcNow<deadline)await Task.Delay(10);typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(components.Amendment,null);}
                        if(mode=="amendment-form-lost-route")
                        {
                            Assert.True(state.HasPendingIntent);var retained=state.Pending!;Assert.Equal(3,f.Native.Writes);Assert.Null(state.Recorded);
                            await Assert.ThrowsAnyAsync<Exception>(()=>state.PrepareAsync(processes,preparation.Terms,"Replacement must refuse"));
                            f.Native.FailCommit=false;f.Row.GROSS_REVENUE=998;
                            await using(var recoveryProvider=formServices.BuildServiceProvider())
                            await using(var recoveryRenderer=new HtmlRenderer(recoveryProvider,recoveryProvider.GetRequiredService<ILoggerFactory>()))
                            {
                                await recoveryRenderer.Dispatcher.InvokeAsync(async()=>{
                                    var recoveryRoot=await recoveryRenderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",started.InstanceId}}));
                                    var recoveryComponents=(CommercialProcessComponents)recoveryProvider.GetRequiredService<IComponentActivator>();var recoveryPanel=recoveryComponents.Amendment!;var freshCircuit=recoveryProvider.GetRequiredService<CommercialAmendmentWorkspaceState>();
                                    Assert.Equal(retained.Amendment.Preparation.IntentId,freshCircuit.Pending!.Amendment.Preparation.IntentId);Assert.NotNull(freshCircuit.SavedIntent);Assert.False(freshCircuit.CanRetry);Assert.Null(freshCircuit.Recorded);
                                    Assert.Contains("Original amendment proposal saved",recoveryRoot.ToHtmlString());Assert.Contains(retained.Amendment.Preparation.IntentId,recoveryRoot.ToHtmlString());Assert.DoesNotContain("New terms version",recoveryRoot.ToHtmlString());
                                    await (Task)recoveryPanel.GetType().GetMethod("Retry",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(recoveryPanel,null)!;Assert.Equal(3,f.Native.Writes);Assert.Null(freshCircuit.Recorded);
                                    f.Row.GROSS_REVENUE=999;await (Task)recoveryPanel.GetType().GetMethod("Reload",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(recoveryPanel,null)!;Assert.True(freshCircuit.CanRetry);
                                    await (Task)recoveryPanel.GetType().GetMethod("Retry",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(recoveryPanel,null)!;
                                    var deadline=DateTime.UtcNow.AddSeconds(5);while((freshCircuit.Recorded is null || freshCircuit.Busy) && DateTime.UtcNow<deadline)await Task.Delay(10);
                                    Assert.Equal(retained.Amendment.Preparation.IntentId,freshCircuit.Recorded!.Preparation.IntentId);Assert.False(freshCircuit.HasPendingIntent);Assert.Equal(4,f.Native.Writes);
                                });
                            }
                            await (Task)panel.GetType().GetMethod("Reload",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;
                            await Settle();
                            Assert.Equal(retained.Amendment.Preparation.IntentId,state.Recorded!.Preparation.IntentId);Assert.False(state.HasPendingIntent);Assert.Equal(4,f.Native.Writes);
                        }
                        else await Settle();
                        Assert.NotNull(state.Recorded);
                        amendment=amendmentReview.CreateIntent(state.Recorded!.Preparation.IntentId,state.Recorded.Preparation.Terms,"Resolve the reviewed rejection");submission=new(amendmentReview,amendment);
                        typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(components.Page,null);await root.QuiescenceTask;
                        Assert.Contains("Recorded amendment and original process history confirmed",root.ToHtmlString());
                        Assert.Contains("Open predecessor rejection review",root.ToHtmlString());
                        var handoff=components.Handoff!;
                        await (Task)handoff.GetType().GetMethod("Load",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(handoff,null)!;
                        var handoffState=formProvider.GetRequiredService<CompanyAccountingHandoffState>();Assert.Single(handoffState.Catalog!.Choices);
                        handoff.GetType().GetMethod("Select",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(handoff,["definition"]);
                        await (Task)handoff.GetType().GetMethod("Submit",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(handoff,null)!;
                        formHandoff=handoffState.Receipt;Assert.NotNull(formHandoff);Assert.NotEqual(started.InstanceId,formHandoff!.InstanceId);
                        typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(handoff,null);
                        Assert.Contains("Recorded handoff confirmed",root.ToHtmlString());
                        await state.LoadAsync(processes);Assert.Equal(formHandoff.InstanceId,state.Handoff!.Start!.InstanceId());
                        var discoveryQuery=new CompanyAccountingProcessStartCatalogQuery("SALES_CONTRACT","contract",CompanyAccountingSourceTypes.CommercialPreparationRevision(state.Recorded!));
                        var discovery=await processes.DiscoverCommercialHandoffAsync(discoveryQuery);Assert.Equal("IN_PROGRESS",discovery.Status);Assert.Equal(formHandoff.InstanceId,discovery.Start!.InstanceId());
                        var processRow=native.Native.Rows.OfType<Beep.OilandGas.LifeCycle.Data.Tables.PROCESS_INSTANCE>().Single(row=>row.PROCESS_INSTANCE_ID==formHandoff.InstanceId);
                        native.Native.Rows.Add(CompanyInventorySourceReview.Copy(processRow));await Assert.ThrowsAnyAsync<Exception>(()=>processes.DiscoverCommercialHandoffAsync(discoveryQuery));native.Native.Rows.RemoveAt(native.Native.Rows.Count-1);
                        var originalPayload=processRow.INSTANCE_DATA_JSON;processRow.INSTANCE_DATA_JSON="{}";await Assert.ThrowsAnyAsync<Exception>(()=>processes.DiscoverCommercialHandoffAsync(discoveryQuery));processRow.INSTANCE_DATA_JSON=originalPayload;
                        using var recovered=new CommercialAmendmentWorkspaceState(company);recovered.AttachOwner(new object(),started.InstanceId);await recovered.LoadAsync(processes);Assert.Equal(formHandoff.InstanceId,recovered.Handoff!.Start!.InstanceId());Assert.False(recovered.HasPendingIntent);
                        var writes=native.Native.Writes;await (Task)components.Amendment!.GetType().GetMethod("Reload",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(components.Amendment,null)!;await Settle();
                        typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(components.Page,null);await root.QuiescenceTask;
                        Assert.Contains("Open recorded amendment handoff",root.ToHtmlString());Assert.Equal(writes,native.Native.Writes);
                    });
                    amended=formProvider.GetRequiredService<CommercialAmendmentWorkspaceState>().Recorded!;
                    formFields.Verify(value=>value.SetCurrentFieldAsync(It.IsAny<SetActiveFieldRequest>(),It.IsAny<CancellationToken>()),Times.Never);
                }
                else try{amended=await processes.SubmitCommercialAmendmentAsync(submission);}catch(Exception failure){throw new InvalidOperationException("Amendment API failed",amendmentLog.Invocations.LastOrDefault()?.Arguments.ElementAtOrDefault(3) as Exception ?? failure);}Assert.Equal(4,f.Native.Writes);
                Assert.Equal(amended.Revision,(await processes.SubmitCommercialAmendmentAsync(submission)).Revision);Assert.Equal(4,f.Native.Writes);
                if(mode=="amendment-reason-route"){await Assert.ThrowsAnyAsync<Exception>(()=>processes.SubmitCommercialAmendmentAsync(submission with{Amendment=amendment with{Reason="Changed retained reason"}}));Assert.Equal(4,f.Native.Writes);return;}
                f.Row.GROSS_REVENUE=998; // Historic capture remains available even when current revenue changes again.
                var historical=await processes.GetCompanyCommercialSourceAsync(started.InstanceId);historical.Require(preparation.Company,"actor",started.InstanceId);
                Assert.True(historical.IsHistorical);Assert.Equal(approved.Revision,historical.Terms.Recorded!.Revision);Assert.Equal(20m,historical.Terms.Source.Source.Rows.Single().GROSS_REVENUE);Assert.Equal(preparation.Terms,historical.Terms.Recorded.Preparation.Terms);
                Assert.Equal(amended.Revision,SalesContractTermsNativeProof.Require(preparation.Company,historical.CurrentNativeContract!).Record.Revision);
                Assert.Equal(amendment.Process,SalesContractTermsNativeProof.Require(preparation.Company,historical.CurrentNativeContract!).Amendment!.Process);
                Assert.Equal("REJECTION_REVIEWED",(await processes.GetCompanyAccountingInstanceAsync(started.InstanceId))!.CurrentState);
                var changed=CompanyInventorySourceReview.Copy(historical);changed.Terms.Source.Source.Rows[0].GROSS_REVENUE=998;Assert.ThrowsAny<Exception>(()=>changed.Require(preparation.Company,"actor",started.InstanceId));
                var badEnvelope=CompanyInventorySourceReview.Copy(historical.CurrentNativeContract!);badEnvelope.DESCRIPTION="Unreviewed native facts";
                Assert.ThrowsAny<Exception>(()=>(historical with{CurrentNativeContract=badEnvelope}).Require(preparation.Company,"actor",started.InstanceId));
                if(mode.StartsWith("historical-",StringComparison.Ordinal) && mode!="historical-route")
                {
                    if(mode=="historical-field-route")assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
                    if(mode=="historical-permission-route")host.Context.Permissions=["Workflow.Execute"];
                    if(mode=="historical-role-route")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");
                    if(mode=="historical-company-route")company.TrySelect(company.Clear(),"actor",new(new("tenant","foreign"),["ConfiguredCompanyRole"],permissions));
                    var capture=host.Context.Calls;
                    if(mode=="historical-binding-route")host.Context.OnCapture=count=>{if(count>capture+5)host.Context.PolicyBinding="changed-production-binding";};
                    if(mode=="historical-native-route")host.Context.OnCapture=count=>{if(count>capture+5)f.Native.Rows.OfType<SALES_CONTRACT>().Single().DESCRIPTION="Changed native facts while opening historical source";};
                    await Assert.ThrowsAnyAsync<Exception>(()=>processes.GetCompanyCommercialSourceAsync(started.InstanceId));Assert.Equal(4,f.Native.Writes);return;
                }
                var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("another-global-field");
                var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<CompanyAccountingStepState>();services.AddScoped<CommercialTermsWorkspaceState>();services.AddScoped<CommercialAmendmentWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton<IBusinessProcessServiceClient>(processes);services.AddSingleton<IAccountingServiceClient>(accounting);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new CommercialAuth("actor"));services.AddSingleton<NavigationManager>(new CommercialProcessNavigation(WorkflowTaskRoute.For("COMPANY_ACCOUNTING",started.InstanceId,"SALES_CONTRACT")));services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,CommercialProcessComponents>();
                await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
                await renderer.Dispatcher.InvokeAsync(async()=>{
                    var root=await renderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",started.InstanceId}}));var html=root.ToHtmlString();
                    Assert.Contains("Historical commercial terms",html);Assert.Contains("captured evidence",html);Assert.Contains("Recorded decision: Reject",html);Assert.Contains(preparation.Terms.Version,html);Assert.Contains("Recorded amendment and original process history confirmed",html);Assert.Contains("amended-v2",html);Assert.DoesNotContain("Approve reviewed terms",html);Assert.DoesNotContain("Review current step authority",html);Assert.Null(((CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>()).Terms);
                });
                f.Row.GROSS_REVENUE=999;
                var newProcess=formHandoff;
                if(newProcess is null){var newStart=await processes.ReviewCompanyAccountingStartAsync(new("definition","SALES_CONTRACT","contract",CompanyAccountingSourceTypes.CommercialPreparationRevision(amended)));newProcess=await processes.StartCompanyAccountingAsync(newStart.CreateIntent(Guid.NewGuid().ToString("N")));}Assert.NotEqual(started.InstanceId,newProcess.InstanceId);
                var live=await processes.GetCompanyCommercialSourceAsync(newProcess.InstanceId);Assert.False(live.IsHistorical);Assert.Equal("amended-v2",live.Terms.Recorded!.Preparation.Terms.Version);
                Actor("new-reviewer");SalesContractTermsRecord amendedDecision;
                if(formJourney)
                {
                    var inboxClient=new UnifiedTaskInboxService(api);var inbox=await inboxClient.GetInboxAsync("COMMERCIAL");var task=Assert.Single(inbox.CriticalTasks.Concat(inbox.HighPriorityTasks).Concat(inbox.NormalTasks));
                    Assert.Equal(newProcess.InstanceId,task.WorkflowName);Assert.Equal("COMMERCIAL_APPROVAL_REQUIRED",task.AccountingStepReview!.Eligibility);Assert.Equal("actor",task.AccountingStepReview.SourceMakerUserId);Assert.True(task.AccountingStepReview.RequiresIndependentActor);
                    var reviewerServices=new ServiceCollection();reviewerServices.AddLogging();reviewerServices.AddMudServices();reviewerServices.AddSingleton(company);reviewerServices.AddScoped<CompanyAccountingStepState>();reviewerServices.AddScoped<CommercialTermsWorkspaceState>();reviewerServices.AddScoped<CommercialAmendmentWorkspaceState>();reviewerServices.AddScoped<CompanyAccountingHandoffState>();reviewerServices.AddSingleton<IBusinessProcessServiceClient>(processes);reviewerServices.AddSingleton<IAccountingServiceClient>(accounting);reviewerServices.AddSingleton(fields.Object);reviewerServices.AddSingleton<AuthenticationStateProvider>(new CommercialAuth("new-reviewer"));reviewerServices.AddSingleton<NavigationManager>(new CommercialProcessNavigation(task.Route));reviewerServices.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();reviewerServices.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));reviewerServices.AddSingleton<IComponentActivator,CommercialProcessComponents>();
                    await using var reviewerProvider=reviewerServices.BuildServiceProvider();await using var reviewerRenderer=new HtmlRenderer(reviewerProvider,reviewerProvider.GetRequiredService<ILoggerFactory>());
                    await reviewerRenderer.Dispatcher.InvokeAsync(async()=>{
                        var personas=new Mock<IPersonaContextService>();personas.SetupGet(value=>value.CurrentPersona).Returns(new AppPersona{Code="COMMERCIAL",Name="Commercial",DefaultRoute="/accounting/revenue-recognition"});personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId="new-reviewer",PersonaCode="COMMERCIAL"});personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
                        using var queue=new PersonaTaskInboxState(inboxClient,company,personas.Object);queue.Start();await queue.LoadAsync();queue.SelectSource("SALES_CONTRACT");queue.SelectAccountingEvidence("COMMERCIAL_APPROVAL_REQUIRED");
                        var assigned=await reviewerRenderer.RenderComponentAsync<PersonaAssignedWorkPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Review",queue},{"WorkspaceName","Commercial"}}));Assert.Contains(task.Route,assigned.ToHtmlString());Assert.Contains("Commercial approval needed: 1",assigned.ToHtmlString());Assert.Single(queue.VisibleRows);
                        var root=await reviewerRenderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",newProcess.InstanceId}}));var components=(CommercialProcessComponents)reviewerProvider.GetRequiredService<IComponentActivator>();var page=components.Page!;
                        async Task Invoke(object component,string method,params object?[] args){await (Task)component.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(component,args)!;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);await root.QuiescenceTask;var deadline=DateTime.UtcNow.AddSeconds(5);while(components.Terms is {} active && DateTime.UtcNow<deadline && ((bool)active.GetType().GetField("_authorized",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(active)! == false || reviewerProvider.GetRequiredService<CommercialTermsWorkspaceState>().Busy))await Task.Delay(10);}
                        Assert.Contains("amended-v2",root.ToHtmlString());Assert.Contains("Approve reviewed terms",root.ToHtmlString());Assert.DoesNotContain("Amend rejected commercial terms",root.ToHtmlString());
                        await Invoke(page,"ReviewStep");var panel=components.Terms!;panel.GetType().GetField("_reason",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,multiVersion?"Independent second rejection":"Independent new-version approval");await Invoke(panel,"Decide",multiVersion?SalesContractTermsDecision.Reject:SalesContractTermsDecision.Approve);
                        Assert.Contains(multiVersion?"Recorded decision: Reject":"Recorded decision: Approve",root.ToHtmlString());await Invoke(page,"ReviewStep");page.GetType().GetField("_notes",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,multiVersion?"Return this rejected amendment to its original preparer":"Complete only this independently approved amendment");await Invoke(page,"CompleteStep");
                        Assert.Equal(multiVersion?Beep.OilandGas.Models.Processes.ProcessStatus.IN_PROGRESS:Beep.OilandGas.Models.Processes.ProcessStatus.COMPLETED,(await processes.GetCompanyAccountingInstanceAsync(newProcess.InstanceId))!.Status);
                        await queue.LoadAsync();
                        if(multiVersion)
                        {
                            Assert.Equal("COMMERCIAL_PREPARER_REQUIRED",Assert.Single(queue.VisibleRows).AccountingStepReview!.Eligibility);Assert.False(queue.VisibleRows.Single().AccountingStepReview!.Eligible);
                            var denied=await processes.ReviewCompanyAccountingStepAsync(newProcess.InstanceId);Assert.Equal("COMMERCIAL_PREPARER_REQUIRED",denied!.Eligibility);var writes=native.Native.Writes;
                            await Assert.ThrowsAnyAsync<Exception>(()=>processes.CompleteCompanyAccountingStepAsync(new(denied,Guid.NewGuid().ToString("N"),"Reviewer cannot adopt original-preparer work")));Assert.Equal(writes,native.Native.Writes);Assert.Equal(5,f.Native.Writes);
                        }
                        else{Assert.Empty(queue.VisibleRows);Assert.Equal(0,queue.SourceCount("SALES_CONTRACT"));}
                    });
                    var completedInbox=await inboxClient.GetInboxAsync("COMMERCIAL");var completedRows=completedInbox.CriticalTasks.Concat(completedInbox.HighPriorityTasks).Concat(completedInbox.NormalTasks);
                    if(multiVersion)Assert.Equal("COMMERCIAL_PREPARER_REQUIRED",Assert.Single(completedRows).AccountingStepReview!.Eligibility);else Assert.Empty(completedRows);
                    amendedDecision=(await processes.GetCompanyCommercialSourceAsync(newProcess.InstanceId)).Terms.Recorded!;Assert.Equal("new-reviewer",amendedDecision.Decision!.ActorUserId);
                }
                else
                {
                    amendedDecision=await accounting.DecideSalesContractTermsAsync(new(amended,new(Guid.NewGuid().ToString("N"),preparation.Company,"new-reviewer",amended.Revision,SalesContractTermsDecision.Approve,"Independent new-version approval")));
                    var newStep=(await processes.ReviewCompanyAccountingStepAsync(newProcess.InstanceId))!;Assert.True(newStep.Eligible);await processes.CompleteCompanyAccountingStepAsync(new(newStep,Guid.NewGuid().ToString("N"),"Complete only this independently approved amendment"));
                }
                Assert.Equal(approved.Revision,(await processes.GetCompanyCommercialSourceAsync(started.InstanceId)).Terms.Recorded!.Revision);Assert.Equal(amendedDecision.Revision,(await processes.GetCompanyCommercialSourceAsync(newProcess.InstanceId)).Terms.Recorded!.Revision);Assert.Equal(5,f.Native.Writes);
                if(multiVersion)
                {
                    ServiceCollection JourneyServices(string currentActor,string route)
                    {
                        var result=new ServiceCollection();result.AddLogging();result.AddMudServices();result.AddSingleton(company);result.AddScoped<CompanyAccountingStepState>();result.AddScoped<CommercialTermsWorkspaceState>();result.AddScoped<CommercialAmendmentWorkspaceState>();result.AddScoped<CompanyAccountingHandoffState>();result.AddSingleton<IBusinessProcessServiceClient>(processes);result.AddSingleton<IAccountingServiceClient>(accounting);result.AddSingleton(fields.Object);result.AddSingleton<AuthenticationStateProvider>(new CommercialAuth(currentActor));result.AddSingleton<NavigationManager>(new CommercialProcessNavigation(route));result.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();result.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));result.AddSingleton<IComponentActivator,CommercialProcessComponents>();return result;
                    }
                    async Task Handler(object component,string method,params object?[] args)=>await (Task)component.GetType().GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(component,args)!;
                    void Redraw(object component)=>typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(component,null);
                    var inboxClient=new UnifiedTaskInboxService(api);Actor("actor");
                    var returnedInbox=await inboxClient.GetInboxAsync("COMMERCIAL");var returnedTask=Assert.Single(returnedInbox.CriticalTasks.Concat(returnedInbox.HighPriorityTasks).Concat(returnedInbox.NormalTasks));Assert.Equal(newProcess.InstanceId,returnedTask.WorkflowName);Assert.Equal("ELIGIBLE",returnedTask.AccountingStepReview!.Eligibility);
                    CompanyAccountingProcessStartReceipt? thirdHandoff=null;SalesContractTermsRecord? third=null;
                    await using(var preparerProvider=JourneyServices("actor",returnedTask.Route).BuildServiceProvider())
                    await using(var preparerRenderer=new HtmlRenderer(preparerProvider,preparerProvider.GetRequiredService<ILoggerFactory>()))
                    {
                        await preparerRenderer.Dispatcher.InvokeAsync(async()=>{
                            var root=await preparerRenderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",newProcess.InstanceId}}));var components=(CommercialProcessComponents)preparerProvider.GetRequiredService<IComponentActivator>();var page=components.Page!;
                            await Handler(page,"ReviewStep");page.GetType().GetField("_notes",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,"Reviewed the independently rejected second version");await Handler(page,"CompleteStep");Redraw(page);await root.QuiescenceTask;
                            Assert.Equal("REJECTION_REVIEWED",(await processes.GetCompanyAccountingInstanceAsync(newProcess.InstanceId))!.CurrentState);
                            var state=preparerProvider.GetRequiredService<CommercialAmendmentWorkspaceState>();var deadline=DateTime.UtcNow.AddSeconds(5);while((components.Amendment is null || state.Review is null || state.Busy) && DateTime.UtcNow<deadline)await Task.Delay(10);var editor=components.Amendment!;Assert.NotNull(state.Review);
                            void Draft(string name,object? value)=>editor.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(editor,value);
                            Draft("_version","amended-v3");Draft("_from",preparation.Terms.EffectiveFrom);Draft("_through",preparation.Terms.EffectiveThrough);Draft("_currency",preparation.Terms.CurrencyCode);Draft("_price",40.125m);Draft("_basis",preparation.Terms.PriceUnit);Draft("_method",preparation.Terms.PricingMethod);Draft("_notes",preparation.Terms.Notes);Draft("_reason","Resolve the independent second rejection");Redraw(editor);await root.QuiescenceTask;await Handler(editor,"Prepare");Redraw(page);await root.QuiescenceTask;
                            deadline=DateTime.UtcNow.AddSeconds(5);while((state.Recorded is null || state.Busy || components.Handoff is null) && DateTime.UtcNow<deadline)await Task.Delay(10);third=state.Recorded;Assert.Equal("amended-v3",third!.Preparation.Terms.Version);Assert.Equal(7,f.Native.Writes);Assert.False(state.HasPendingIntent);
                            var handoff=components.Handoff!;await Handler(handoff,"Load");handoff.GetType().GetMethod("Select",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(handoff,["definition"]);await Handler(handoff,"Submit");thirdHandoff=preparerProvider.GetRequiredService<CompanyAccountingHandoffState>().Receipt;Assert.NotNull(thirdHandoff);Assert.NotEqual(newProcess.InstanceId,thirdHandoff!.InstanceId);Assert.NotEqual(started.InstanceId,thirdHandoff.InstanceId);
                        });
                    }
                    var closedSecond=(await processes.GetCompanyAccountingInstanceAsync(newProcess.InstanceId))!;Assert.Contains(closedSecond.History,row=>row.Action=="COMMERCIAL_REJECTION_ROUTED");Assert.Contains(closedSecond.History,row=>row.Action=="COMMERCIAL_REJECTION_REVIEWED");
                    Actor("new-reviewer");var thirdInbox=await inboxClient.GetInboxAsync("COMMERCIAL");var thirdTask=Assert.Single(thirdInbox.CriticalTasks.Concat(thirdInbox.HighPriorityTasks).Concat(thirdInbox.NormalTasks));Assert.Equal(thirdHandoff!.InstanceId,thirdTask.WorkflowName);Assert.Equal("COMMERCIAL_APPROVAL_REQUIRED",thirdTask.AccountingStepReview!.Eligibility);
                    await using(var thirdProvider=JourneyServices("new-reviewer",thirdTask.Route).BuildServiceProvider())
                    await using(var thirdRenderer=new HtmlRenderer(thirdProvider,thirdProvider.GetRequiredService<ILoggerFactory>()))
                    {
                        await thirdRenderer.Dispatcher.InvokeAsync(async()=>{
                            var root=await thirdRenderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",thirdHandoff.InstanceId}}));var components=(CommercialProcessComponents)thirdProvider.GetRequiredService<IComponentActivator>();var page=components.Page!;Assert.Contains("amended-v3",root.ToHtmlString());Assert.Contains("Approve reviewed terms",root.ToHtmlString());
                            await Handler(page,"ReviewStep");components.Terms!.GetType().GetField("_reason",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(components.Terms,"Independent third-version approval");await Handler(components.Terms,"Decide",SalesContractTermsDecision.Approve);Redraw(page);await root.QuiescenceTask;
                            await Handler(page,"ReviewStep");page.GetType().GetField("_notes",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(page,"Complete the independently approved third version");await Handler(page,"CompleteStep");Redraw(page);await root.QuiescenceTask;
                            Assert.Equal(Beep.OilandGas.Models.Processes.ProcessStatus.COMPLETED,(await processes.GetCompanyAccountingInstanceAsync(thirdHandoff.InstanceId))!.Status);Assert.Equal(8,f.Native.Writes);
                        });
                    }
                    var finalReviewerInbox=await inboxClient.GetInboxAsync("COMMERCIAL");Assert.Empty(finalReviewerInbox.CriticalTasks.Concat(finalReviewerInbox.HighPriorityTasks).Concat(finalReviewerInbox.NormalTasks));
                    Actor("actor");var businessBefore=System.Text.Json.JsonSerializer.Serialize(f.Native.Rows);var lifecycleBefore=System.Text.Json.JsonSerializer.Serialize(native.Native.Rows);
                    var savedPage=await processes.GetCommercialAmendmentPageAsync(1,8);Assert.Equal(2,savedPage.TotalRecords);Assert.All(savedPage.Queue.Rows,row=>Assert.Equal("RECORDED",row.Status));Assert.Equal(new[]{started.InstanceId,newProcess.InstanceId}.Order(StringComparer.Ordinal),savedPage.Queue.Rows.Select(row=>row.Entry.PROCESS_INSTANCE_ID));
                    string? firstLink=null,secondLink=null;
                    await using(var landingProvider=JourneyServices("actor","/accounting/revenue-workbench").BuildServiceProvider())
                    await using(var landingRenderer=new HtmlRenderer(landingProvider,landingProvider.GetRequiredService<ILoggerFactory>()))
                    {
                        await landingRenderer.Dispatcher.InvokeAsync(async()=>{
                            var root=await landingRenderer.RenderComponentAsync<CommercialSavedProposalQueue>();Assert.Contains("No unresolved saved commercial proposals",root.ToHtmlString());var queue=((CommercialProcessComponents)landingProvider.GetRequiredService<IComponentActivator>()).Queue!;queue.GetType().GetField("_showRecorded",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(queue,true);Redraw(queue);Assert.Contains("Independent decision: Reject",root.ToHtmlString());Assert.Contains("Independent decision: Approve",root.ToHtmlString());
                            var links=System.Text.RegularExpressions.Regex.Matches(root.ToHtmlString(),"href=\"(/accounting/processes/[^\"]+)\"").Select(match=>WebUtility.HtmlDecode(match.Groups[1].Value)).ToArray();Assert.Equal(2,links.Length);firstLink=Assert.Single(links,link=>Uri.UnescapeDataString(link.Split('/').Last())==started.InstanceId);secondLink=Assert.Single(links,link=>Uri.UnescapeDataString(link.Split('/').Last())==newProcess.InstanceId);
                        });
                    }
                    f.Row.GROSS_REVENUE=997;
                    await using(var recoveredProvider=JourneyServices("actor",firstLink!).BuildServiceProvider())
                    await using(var recoveredRenderer=new HtmlRenderer(recoveredProvider,recoveredProvider.GetRequiredService<ILoggerFactory>()))
                    {
                        await recoveredRenderer.Dispatcher.InvokeAsync(async()=>{
                            var id=Uri.UnescapeDataString(firstLink!.Split('/').Last());var root=await recoveredRenderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",id}}));var state=recoveredProvider.GetRequiredService<CommercialAmendmentWorkspaceState>();
                            Assert.Equal("amended-v2",state.Recorded!.Preparation.Terms.Version);Assert.Equal(SalesContractTermsDecision.Reject,state.Recorded.Decision!.Decision);Assert.Equal(newProcess.InstanceId,state.Handoff!.Start!.InstanceId());Assert.Equal("COMPLETED",state.Handoff.Status);Assert.False(state.CanRetry);Assert.False(state.HasPendingIntent);
                            Assert.Contains("Recorded amendment and original process history confirmed",root.ToHtmlString());Assert.Contains("Open recorded amendment handoff",root.ToHtmlString());Assert.DoesNotContain("New terms version",root.ToHtmlString());Assert.DoesNotContain("Approve reviewed terms",root.ToHtmlString());
                        });
                    }
                    f.Row.GROSS_REVENUE=999;
                    await using(var latestProvider=JourneyServices("actor",secondLink!).BuildServiceProvider())
                    await using(var latestRenderer=new HtmlRenderer(latestProvider,latestProvider.GetRequiredService<ILoggerFactory>()))
                    {
                        await latestRenderer.Dispatcher.InvokeAsync(async()=>{
                            var id=Uri.UnescapeDataString(secondLink!.Split('/').Last());var root=await latestRenderer.RenderComponentAsync<CompanyAccountingProcessDetail>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Id",id}}));var state=latestProvider.GetRequiredService<CommercialAmendmentWorkspaceState>();
                            Assert.Equal(third!.Preparation.IntentId,state.Recorded!.Preparation.IntentId);Assert.Equal("amended-v3",state.Recorded.Preparation.Terms.Version);Assert.Equal(SalesContractTermsDecision.Approve,state.Recorded.Decision!.Decision);Assert.Equal(thirdHandoff.InstanceId,state.Handoff!.Start!.InstanceId());Assert.Equal("COMPLETED",state.Handoff.Status);Assert.False(state.CanRetry);Assert.False(state.HasPendingIntent);
                            Assert.Contains("Recorded amendment and original process history confirmed",root.ToHtmlString());Assert.Contains("Open recorded amendment handoff",root.ToHtmlString());Assert.DoesNotContain("New terms version",root.ToHtmlString());Assert.DoesNotContain("Approve reviewed terms",root.ToHtmlString());
                        });
                    }
                    Assert.Equal(businessBefore,System.Text.Json.JsonSerializer.Serialize(f.Native.Rows));Assert.Equal(lifecycleBefore,System.Text.Json.JsonSerializer.Serialize(native.Native.Rows));Assert.Equal(8,f.Native.Writes);
                    var proof=SalesContractTermsNativeProof.Require(preparation.Company,(await processes.GetCompanyCommercialSourceAsync(started.InstanceId)).CurrentNativeContract!);Assert.Equal(2,proof.PriorVersions!.Count);Assert.Equal(approved.Revision,proof.PriorVersions[0].Record.Revision);Assert.Equal(amendedDecision.Revision,proof.PriorVersions[1].Record.Revision);Assert.Equal(SalesContractTermsDecision.Approve,proof.Record.Decision!.Decision);Assert.Equal(third!.Preparation.IntentId,proof.Record.Preparation.IntentId);
                }
                fields.Verify(value=>value.SetCurrentFieldAsync(It.IsAny<SetActiveFieldRequest>(),It.IsAny<CancellationToken>()),Times.Never);
            }
            return;
        }
        if(mode is not ("approve" or "approve-route")){Assert.False(reviewed.Eligible);Assert.Equal(mode=="reject"?"COMMERCIAL_RETURN_CONFIGURATION_REQUIRED":mode=="revoked"?"COMMERCIAL_APPROVAL_PERMISSION_REQUIRED":"COMMERCIAL_REVIEWER_REQUIRED",reviewed.Eligibility);return;}
        Assert.True(reviewed.Eligible);
        var command=new CompanyAccountingStepCommand(reviewed,Guid.NewGuid().ToString("N"),"Complete task against the recorded independent approval");
        var receipt=await processes.CompleteCompanyAccountingStepAsync(command);receipt.Require(command);Assert.Null(receipt.NextStepInstanceId);
        var writes=native.Native.Writes;var retry=await processes.CompleteCompanyAccountingStepAsync(command);Assert.Equal(receipt.RecordedRevision,retry.RecordedRevision);Assert.Equal(writes,native.Native.Writes);
        Assert.Equal(Beep.OilandGas.Models.Processes.ProcessStatus.COMPLETED,(await processes.GetCompanyAccountingInstanceAsync(started.InstanceId))!.Status);
        if(twoStep)Assert.Contains((await processes.GetCompanyAccountingInstanceAsync(started.InstanceId))!.StepInstances,row=>row.StepId=="rejection-review" && row.Status==Beep.OilandGas.Models.Processes.StepStatus.SKIPPED);
        if(twoStep){using var terminal=new ProcessInstanceDetailState(company,_=>throw new Exception(),_=>throw new Exception());await terminal.LoadAccountingAsync(started.InstanceId,id=>processes.GetCompanyAccountingInstanceAsync(id));Assert.NotNull(terminal.Instance);}
        Assert.Equal(2,f.Native.Writes);
    }
    private sealed class CommercialProcessNavigation:NavigationManager
    {public CommercialProcessNavigation(string path)=>Initialize("http://fixture/","http://fixture"+path);protected override void NavigateToCore(string uri,bool forceLoad){}}
    private sealed class CommercialProcessComponents(IServiceProvider services):IComponentActivator
    {public CompanyAccountingProcessDetail? Page;public DeliveryAcceptanceWorkbench? Delivery;public CommercialTermsWorkbench? Terms;public CommercialAmendmentWorkbench? Amendment;public CommercialSavedProposalQueue? Queue;public Beep.OilandGas.Web.Components.BusinessProcess.CompanyAccountingHandoffPanel? Handoff;public IComponent CreateInstance(Type type){var component=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(component is CompanyAccountingProcessDetail page)Page=page;if(component is DeliveryAcceptanceWorkbench delivery)Delivery=delivery;if(component is CommercialTermsWorkbench terms)Terms=terms;if(component is CommercialAmendmentWorkbench amendment)Amendment=amendment;if(component is CommercialSavedProposalQueue queue)Queue=queue;if(component is Beep.OilandGas.Web.Components.BusinessProcess.CompanyAccountingHandoffPanel handoff)Handoff=handoff;return component;}}
    private static readonly DateTime From=new(2026,1,1),Through=new(2026,1,31);
    [Theory][InlineData("amend")][InlineData("approve")][InlineData("second")][InlineData("uncertain")][InlineData("predecessor")][InlineData("maker")][InlineData("version")][InlineData("approved")][InlineData("reason")][InlineData("history")][InlineData("revoke")]
    public async Task NativeCommercialAmendmentPreservesRejectedVersionsAndRequiresOriginalIntent(string mode)
    {
        var f=new Fixture();f.Contract.ROW_CREATED_BY="contract-maker";f.Row.ROW_CREATED_BY="production-maker";var actor="preparer";
        async Task<RevenueSalesContractReview> Read()=>RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(new("tenant","company"),actor,"field",From,Through,"binding",await f.Service.GetRecordedFieldRevenueAsync("field",From,Through,f.Native.Connection)),"revenue-a",await f.Service.GetRecordedSalesContractAsync("contract",new("tenant","company"),f.Native.Connection));
        var initial=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),new("tenant","company"),actor,await Read(),CommercialPreparation().Terms);
        var prepared=await f.Service.PrepareSalesContractTermsAsync(initial,f.Native.Connection,Read,()=>Task.CompletedTask);actor="reviewer";
        var decision=new SalesContractTermsDecisionCommand(Guid.NewGuid().ToString("N"),initial.Company,actor,prepared.Revision,mode=="approved"?SalesContractTermsDecision.Approve:SalesContractTermsDecision.Reject,"Independent decision on original terms");
        var rejected=await f.Service.DecideSalesContractTermsAsync(prepared,decision,f.Native.Connection,Read,()=>Task.CompletedTask);actor="preparer";
        var native=await f.Service.GetRecordedSalesContractAsync("contract",initial.Company,f.Native.Connection);var previous=SalesContractTermsNativeProof.Require(initial.Company,native);
        var source=SalesContractTermsAmendment.NormalizeSource(await Read(),rejected);
        var proposal=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),initial.Company,actor,source,initial.Terms with{Version="v2",Price=20.125m,Notes="Corrected explicit proposed price and basis"});
        var amendment=new SalesContractTermsAmendment(rejected,previous.Revision,proposal,"Correct rejected version after independent feedback");
        if(mode=="predecessor")amendment=amendment with{PreviousNativeRevision=new string('A',64)};
        if(mode=="maker")amendment=amendment with{Preparation=proposal with{ActorUserId="other"}};
        if(mode=="version")amendment=amendment with{Preparation=proposal with{Terms=proposal.Terms with{Version=initial.Terms.Version}}};
        if(mode is "predecessor" or "maker" or "version" or "approved")
        {await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.AmendSalesContractTermsAsync(amendment,f.Native.Connection,Read,()=>Task.CompletedTask));Assert.Equal(2,f.Native.Writes);return;}
        if(mode=="revoke")f.AfterContractWrite=()=>actor="revoked";
        async Task Authority(){if(actor!="preparer")throw new UnauthorizedAccessException();await Task.CompletedTask;}
        if(mode=="revoke")
        {var before=System.Text.Json.JsonSerializer.Serialize(f.Native.Rows);await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.AmendSalesContractTermsAsync(amendment,f.Native.Connection,Read,Authority));Assert.Equal(before,System.Text.Json.JsonSerializer.Serialize(f.Native.Rows));Assert.Equal("rollback",f.Native.Transactions.Last());return;}
        f.Native.FailCommit=mode=="uncertain";
        if(mode=="uncertain"){await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.AmendSalesContractTermsAsync(amendment,f.Native.Connection,Read,Authority));Assert.Equal(3,f.Native.Writes);f.Native.FailCommit=false;}
        var amended=await f.Service.AmendSalesContractTermsAsync(amendment,f.Native.Connection,Read,Authority);Assert.Null(amended.Decision);Assert.Equal("v2",amended.Preparation.Terms.Version);Assert.Equal(3,f.Native.Writes);
        Assert.Equal(amended.Revision,(await f.Service.AmendSalesContractTermsAsync(amendment,f.Native.Connection,Read,Authority)).Revision);Assert.Equal(3,f.Native.Writes);
        native=await f.Service.GetRecordedSalesContractAsync("contract",initial.Company,f.Native.Connection);var proof=SalesContractTermsNativeProof.Require(initial.Company,native);
        var preserved=Assert.Single(proof.PriorVersions!);Assert.Equal(System.Text.Json.JsonSerializer.Serialize(previous.Record),System.Text.Json.JsonSerializer.Serialize(preserved.Record));Assert.Equal(previous.NativeRowJson,preserved.NativeRowJson);Assert.Equal(previous.Revision,preserved.Revision);Assert.Equal(amendment.Reason,proof.Amendment!.Reason);Assert.Null(native.BASE_PRICE);
        if(mode=="reason"){await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.AmendSalesContractTermsAsync(amendment with{Reason="different reason"},f.Native.Connection,Read,Authority));Assert.Equal(3,f.Native.Writes);return;}
        if(mode=="history")
        {var bad=proof with{PriorVersions=[preserved with{Record=preserved.Record with{Preparation=preserved.Record.Preparation with{Terms=preserved.Record.Preparation.Terms with{Price=999}}}}]};native.TERMS_WORKFLOW_JSON=System.Text.Json.JsonSerializer.Serialize(bad);Assert.ThrowsAny<Exception>(()=>SalesContractTermsNativeProof.Require(initial.Company,native));return;}
        if(mode is "approve" or "second")
        {
            actor="new-reviewer";var nextDecision=new SalesContractTermsDecisionCommand(Guid.NewGuid().ToString("N"),initial.Company,actor,amended.Revision,mode=="second"?SalesContractTermsDecision.Reject:SalesContractTermsDecision.Approve,"Independent decision for amended version");
            var decided=await f.Service.DecideSalesContractTermsAsync(amended,nextDecision,f.Native.Connection,Read,()=>Task.CompletedTask);Assert.Equal(4,f.Native.Writes);
            Assert.Equal(nextDecision,decided.Decision);native=await f.Service.GetRecordedSalesContractAsync("contract",initial.Company,f.Native.Connection);proof=SalesContractTermsNativeProof.Require(initial.Company,native);Assert.Equal(previous.Revision,Assert.Single(proof.PriorVersions!).Revision);
            if(mode=="second")
            {
                actor="preparer";var thirdPreparation=proposal with{IntentId=Guid.NewGuid().ToString("N"),Source=SalesContractTermsAmendment.NormalizeSource(await Read(),decided),Terms=proposal.Terms with{Version="v3",Price=30.5m}};
                var third=await f.Service.AmendSalesContractTermsAsync(new(decided,proof.Revision,thirdPreparation,"Second independent correction"),f.Native.Connection,Read,Authority);
                native=await f.Service.GetRecordedSalesContractAsync("contract",initial.Company,f.Native.Connection);proof=SalesContractTermsNativeProof.Require(initial.Company,native);Assert.Equal(2,proof.PriorVersions!.Count);Assert.Equal(previous.Revision,proof.PriorVersions[0].Revision);Assert.Equal(decided.Revision,proof.PriorVersions[1].Record.Revision);Assert.Equal(third.Revision,proof.Record.Revision);Assert.Equal(5,f.Native.Writes);
            }
        }
    }
    [Theory][InlineData("before")][InlineData("after")][InlineData("late")][InlineData("readback")][InlineData("source")][InlineData("origin")][InlineData("owner")]
    public async Task AmendmentStateRetainsOriginalReviewedIntentUntilExactNativeHistoryReadBack(string mode)
    {
        var preparation=CommercialPreparation();var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);var prepared=SalesContractTermsRecord.Create(preparation,at);
        var rejected=prepared.Decide(new(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",prepared.Revision,SalesContractTermsDecision.Reject,"Independent rejection"),preparation.Source,at.AddMinutes(1));
        var previous=CommercialReview(preparation,rejected,preparation.ActorUserId);var predecessor=SalesContractTermsNativeProof.Require(preparation.Company,previous.Source.Contract);
        var review=new CompanyCommercialAmendmentReview(preparation.Company,preparation.ActorUserId,"process",new string('A',64),new string('B',64),previous.Source.Contract,rejected,SalesContractTermsAmendment.NormalizeSource(previous.Source,rejected));
        using var company=new CompanyClientContext();void Select()=>company.TrySelect(company.Clear(),preparation.ActorUserId,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust"]));Select();
        using var state=new CommercialAmendmentWorkspaceState(company);var owner=new object();state.AttachOwner(owner,"process");
        var client=new Mock<IBusinessProcessServiceClient>();SalesContractTermsRecord? stored=null;CompanyCommercialAmendmentSubmission? original=null;var submissions=new List<string>();var failRead=false;var wrongOrigin=false;
        CompanyCommercialProcessSourceReview Source()
        {
            if(failRead)throw new InvalidOperationException("Read-back unavailable");
            var native=CompanyInventorySourceReview.Copy(previous.Source.Contract);
            if(stored is not null)
            {
                native.ROW_CHANGED_BY=preparation.ActorUserId;native.ROW_CHANGED_DATE=stored.PreparedAtUtc;
                var command=original!.Amendment;
                native.TERMS_WORKFLOW_JSON=SalesContractTermsNativeProof.Create(stored,native,[new(predecessor.Record,predecessor.NativeRowJson,predecessor.Revision)],new(command.Previous.Revision,command.PreviousNativeRevision,wrongOrigin?"Changed origin":command.Reason,command.Process));
            }
            var terms=SalesContractTermsNativeProof.HistoricalReview(preparation.Company,preparation.ActorUserId,native,CompanyAccountingSourceTypes.CommercialPreparationRevision(rejected));
            return new(preparation.Company,preparation.ActorUserId,"process","contract",CompanyAccountingSourceTypes.CommercialPreparationRevision(rejected),review.ProcessRevision,terms,native);
        }
        client.Setup(value=>value.GetCompanyCommercialSourceAsync("process",It.IsAny<CancellationToken>())).ReturnsAsync(()=>Source());
        client.Setup(value=>value.DiscoverCommercialHandoffAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync((CompanyAccountingProcessStartCatalogQuery query,CancellationToken token)=>new CompanyCommercialHandoffReview(preparation.Company,preparation.ActorUserId,query.EntityId,query.ExpectedSourceRevision!,new string('B',64),new string('C',64),null,null,null));
        client.Setup(value=>value.ReviewCommercialAmendmentAsync("process",It.IsAny<CancellationToken>())).ReturnsAsync(()=>CompanyInventorySourceReview.Copy(review));
        COMMERCIAL_TERMS_INTENT? savedIntent=null;
        client.Setup(value=>value.GetCommercialAmendmentIntentAsync("process",It.IsAny<CancellationToken>())).ReturnsAsync(()=>savedIntent is null?null:CompanyInventorySourceReview.Copy(savedIntent));
        client.Setup(value=>value.SaveCommercialAmendmentIntentAsync(It.IsAny<CompanyCommercialAmendmentSubmission>(),It.IsAny<CancellationToken>())).ReturnsAsync((CompanyCommercialAmendmentSubmission command,CancellationToken token)=>{
            savedIntent ??=COMMERCIAL_TERMS_INTENT.Create(command,at.AddMinutes(2));return CompanyInventorySourceReview.Copy(savedIntent);
        });
        client.Setup(value=>value.SubmitCommercialAmendmentAsync(It.IsAny<CompanyCommercialAmendmentSubmission>(),It.IsAny<CancellationToken>())).Returns(async(CompanyCommercialAmendmentSubmission command,CancellationToken token)=>{
            original ??=CompanyInventorySourceReview.Copy(command);submissions.Add(command.Amendment.Preparation.IntentId);command.Require();
            if(submissions.Count==1 && mode is "before" or "source" or "owner")throw new InvalidOperationException("Failure before write");
            stored=SalesContractTermsRecord.Create(command.Amendment.Preparation,at.AddMinutes(5));
            if(mode=="origin")wrongOrigin=true;
            if(mode=="readback")failRead=true;
            if(mode=="late")company.Clear();
            if(mode=="after")throw new InvalidOperationException("Response lost after write");
            await Task.CompletedTask;return stored;
        });
        await state.LoadAsync(client.Object);
        var terms=preparation.Terms with{Version="amended-v2",Price=30.125m};
        if(mode=="late")await state.PrepareAsync(client.Object,terms,"Explicit rejected-version correction");
        else await Assert.ThrowsAnyAsync<Exception>(()=>state.PrepareAsync(client.Object,terms,"Explicit rejected-version correction"));
        Assert.True(state.HasPendingIntent);Assert.Null(state.Recorded);Assert.Single(submissions);
        if(mode=="late"){Assert.Null(state.Pending);Select();}
        var retained=state.Pending!;Assert.Equal(original!.Amendment.Preparation.IntentId,retained.Amendment.Preparation.IntentId);
        using(var recoveredState=new CommercialAmendmentWorkspaceState(company))
        {
            recoveredState.AttachOwner(new object(),"process");
            if(mode is "readback" or "origin"){await Assert.ThrowsAnyAsync<Exception>(()=>recoveredState.LoadAsync(client.Object));Assert.True(recoveredState.HasPendingIntent);Assert.False(recoveredState.CanRetry);}
            else{await recoveredState.LoadAsync(client.Object);if(stored is null){Assert.True(recoveredState.CanRetry);Assert.Equal(retained.Amendment.Preparation.IntentId,recoveredState.Pending!.Amendment.Preparation.IntentId);}else Assert.Equal(retained.Amendment.Preparation.IntentId,recoveredState.Recorded!.Preparation.IntentId);}
            Assert.Single(submissions);var savedCopy=recoveredState.SavedIntent!;savedCopy.COMMAND_JSON="{}";Assert.NotEqual("{}",recoveredState.SavedIntent!.COMMAND_JSON);
        }
        retained.Amendment.Preparation.Source.Source.Rows[0].GROSS_REVENUE=1234;Assert.NotEqual(1234m,state.Pending!.Amendment.Preparation.Source.Source.Rows[0].GROSS_REVENUE);
        await Assert.ThrowsAnyAsync<Exception>(()=>state.PrepareAsync(client.Object,terms,"Replacement refuses"));
        failRead=false;
        if(mode=="origin"){await Assert.ThrowsAnyAsync<Exception>(()=>state.LoadAsync(client.Object));Assert.True(state.HasPendingIntent);Assert.False(state.CanRetry);wrongOrigin=false;}
        if(mode=="source")
        {
            var changed=CompanyInventorySourceReview.Copy(review.Source.Source.Rows.ToArray());changed[0].GROSS_REVENUE=888;
            var fresh=FieldRevenueSourceReview.Create(preparation.Company,preparation.ActorUserId,"field",From,Through,"binding",changed);
            review=review with{Source=RevenueSalesContractReview.Create(fresh,"rev",review.Source.Contract)};
            await Assert.ThrowsAnyAsync<Exception>(()=>state.LoadAsync(client.Object));Assert.True(state.HasPendingIntent);Assert.False(state.CanRetry);Assert.Single(submissions);return;
        }
        if(mode=="owner"){var next=new object();state.AttachOwner(next,"process");state.DetachOwner(owner);}
        await state.LoadAsync(client.Object);
        if(mode is "before" or "owner"){Assert.True(state.CanRetry);await state.SubmitPendingAsync(client.Object);Assert.Equal(2,submissions.Count);Assert.Single(submissions.Distinct());}
        Assert.False(state.HasPendingIntent);Assert.NotNull(state.Recorded);Assert.Equal(original.Amendment.Preparation.IntentId,state.Recorded!.Preparation.IntentId);
    }
    [Theory][InlineData("save-late")][InlineData("saved-missing")][InlineData("saved-competing")]
    public async Task SavedAmendmentContextAndIdentityFailuresNeverReplaceTheOriginalCommand(string mode)
    {
        var preparation=CommercialPreparation();var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);var prepared=SalesContractTermsRecord.Create(preparation,at);
        var rejected=prepared.Decide(new(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",prepared.Revision,SalesContractTermsDecision.Reject,"Independent rejection"),preparation.Source,at.AddMinutes(1));
        var previous=CommercialReview(preparation,rejected,preparation.ActorUserId);
        var review=new CompanyCommercialAmendmentReview(preparation.Company,preparation.ActorUserId,"process",new string('A',64),new string('B',64),previous.Source.Contract,rejected,SalesContractTermsAmendment.NormalizeSource(previous.Source,rejected));
        using var company=new CompanyClientContext();void Select()=>company.TrySelect(company.Clear(),preparation.ActorUserId,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust"]));Select();
        using var state=new CommercialAmendmentWorkspaceState(company);state.AttachOwner(new object(),"process");var client=new Mock<IBusinessProcessServiceClient>();COMMERCIAL_TERMS_INTENT? saved=null;
        client.Setup(value=>value.GetCommercialAmendmentIntentAsync("process",It.IsAny<CancellationToken>())).ReturnsAsync(()=>saved is null?null:CompanyInventorySourceReview.Copy(saved));
        client.Setup(value=>value.GetCompanyCommercialSourceAsync("process",It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyCommercialProcessSourceReview(preparation.Company,preparation.ActorUserId,"process","contract",CompanyAccountingSourceTypes.CommercialPreparationRevision(rejected),review.ProcessRevision,previous,previous.Source.Contract));
        client.Setup(value=>value.ReviewCommercialAmendmentAsync("process",It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(value=>value.SaveCommercialAmendmentIntentAsync(It.IsAny<CompanyCommercialAmendmentSubmission>(),It.IsAny<CancellationToken>())).ReturnsAsync((CompanyCommercialAmendmentSubmission command,CancellationToken token)=>{saved=COMMERCIAL_TERMS_INTENT.Create(command,at.AddMinutes(2));if(mode=="save-late")company.Clear();return CompanyInventorySourceReview.Copy(saved);});
        client.Setup(value=>value.SubmitCommercialAmendmentAsync(It.IsAny<CompanyCommercialAmendmentSubmission>(),It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Execution unavailable"));
        await state.LoadAsync(client.Object);
        if(mode=="save-late")await state.PrepareAsync(client.Object,preparation.Terms with{Version="amended-v2"},"Reviewed correction");
        else await Assert.ThrowsAnyAsync<Exception>(()=>state.PrepareAsync(client.Object,preparation.Terms with{Version="amended-v2"},"Reviewed correction"));
        var original=saved!.Restore(preparation.Company,preparation.ActorUserId,"process");Assert.True(state.HasPendingIntent);Assert.Null(state.SavedIntent);Assert.Null(state.Recorded);
        if(mode=="save-late")
        {client.Verify(value=>value.SubmitCommercialAmendmentAsync(It.IsAny<CompanyCommercialAmendmentSubmission>(),It.IsAny<CancellationToken>()),Times.Never);Select();await state.LoadAsync(client.Object);Assert.True(state.CanRetry);Assert.Equal(original.Amendment.Preparation.IntentId,state.Pending!.Amendment.Preparation.IntentId);}
        else
        {
            saved=mode=="saved-missing"?null:COMMERCIAL_TERMS_INTENT.Create(new(review,review.CreateIntent(Guid.NewGuid().ToString("N"),preparation.Terms with{Version="different-v2"},"Other proposal")),at.AddMinutes(3));
            await Assert.ThrowsAnyAsync<Exception>(()=>state.LoadAsync(client.Object));Assert.False(state.CanRetry);Assert.Null(state.SavedIntent);Assert.Equal(original.Amendment.Preparation.IntentId,state.Pending!.Amendment.Preparation.IntentId);
        }
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("process")][InlineData("source")][InlineData("predecessor")][InlineData("late")][InlineData("null")]
    public async Task TypedAmendmentReviewRefusesForeignChangedAndLateEvidence(string mode)
    {
        var preparation=CommercialPreparation();var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);
        var prepared=SalesContractTermsRecord.Create(preparation,at);
        var rejected=prepared.Decide(new(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",prepared.Revision,SalesContractTermsDecision.Reject,"Independent recorded rejection"),preparation.Source,at.AddMinutes(1));
        var current=CommercialReview(preparation,rejected,preparation.ActorUserId);
        var result=new CompanyCommercialAmendmentReview(preparation.Company,preparation.ActorUserId,"process",new string('A',64),new string('B',64),current.Source.Contract,rejected,SalesContractTermsAmendment.NormalizeSource(current.Source,rejected));
        result.Require(preparation.Company,preparation.ActorUserId,"process");
        if(mode=="company")result=result with{Company=new("tenant","foreign")};if(mode=="actor")result=result with{ActorUserId="other"};if(mode=="process")result=result with{InstanceId="other"};
        if(mode=="source")result=result with{Source=result.Source with{TransactionId="foreign"}};
        if(mode=="predecessor"){result=CompanyInventorySourceReview.Copy(result);result.PreviousNativeContract.DESCRIPTION="Altered predecessor";}
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),preparation.ActorUserId,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust"]));
        using var http=new HttpClient(new Reply(()=>{if(mode=="late")company.Clear();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:result)};})){BaseAddress=new Uri("https://fixture/")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        await Assert.ThrowsAnyAsync<Exception>(()=>client.ReviewCommercialAmendmentAsync("process"));
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("process")][InlineData("contract")][InlineData("preparation")][InlineData("late")][InlineData("null")]
    public async Task TypedAssignedCommercialSourceRefusesForeignChangedAndLateEvidence(string mode)
    {
        var preparation=CommercialPreparation();var record=SalesContractTermsRecord.Create(preparation,new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc));
        var terms=CommercialReview(preparation,record,"reviewer");
        var result=new CompanyCommercialProcessSourceReview(preparation.Company,"reviewer","process","contract",CompanyAccountingSourceTypes.CommercialPreparationRevision(record),new string('A',64),terms);
        if(mode=="company")result=result with{Company=new("tenant","foreign")};if(mode=="actor")result=result with{ActorUserId="other"};if(mode=="process")result=result with{InstanceId="other"};
        if(mode=="contract")result=result with{ContractId="other"};if(mode=="preparation")result=result with{PreparationRevision=new string('B',64)};
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"reviewer",new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
        using var http=new HttpClient(new Reply(()=>{if(mode=="late")company.Clear();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:result)};})){BaseAddress=new Uri("https://fixture/")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        await Assert.ThrowsAnyAsync<Exception>(()=>client.GetCompanyCommercialSourceAsync("process"));
    }
    [Fact]
    public async Task OldCommercialPanelDisposalCannotDetachNewPanelSource()
    {
        var preparation=CommercialPreparation();var recorded=SalesContractTermsRecord.Create(preparation,new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc));
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"reviewer",new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
        using var state=new CommercialTermsWorkspaceState(company);var oldOwner=new object();var newOwner=new object();
        state.AttachOwner(oldOwner,"field",From,Through,"rev");state.AttachOwner(newOwner,"field",From,Through,"rev");state.DetachOwner(oldOwner);
        var api=new Mock<IAccountingServiceClient>();api.Setup(value=>value.GetSalesContractTermsAsync("field",From,Through,"rev")).ReturnsAsync(CommercialReview(preparation,recorded,"reviewer"));
        await state.LoadAsync(api.Object);Assert.NotNull(state.Review);state.DetachOwner(newOwner);Assert.Null(state.Review);
    }
    [Fact]
    public void BeepClassCreatorDeclaresCanonicalCommercialEvidenceColumn()
    {
        var schema=new ClassCreator(Mock.Of<IDMEEditor>()).ConvertToEntityStructure(typeof(SALES_CONTRACT));
        var column=Assert.Single(schema.Fields,value=>value.FieldName=="TERMS_WORKFLOW_JSON");
        Assert.Equal(typeof(string).FullName,column.Fieldtype);Assert.True(column.AllowDBNull);Assert.True(column.Size>=SalesContractTermsNativeProof.MaximumLength);
    }
    private static SalesContractTermsPreparation CommercialPreparation()
    {
        var source=FieldRevenueSourceReview.Create(new("tenant","company"),"commercial-preparer","field",From,Through,"binding",[new(){REVENUE_TRANSACTION_ID="rev",CONTRACT_ID="contract",FIELD_ID="field",ACTIVE_IND="Y",TRANSACTION_DATE=From,CURRENCY_CODE="KWD",ROW_CREATED_BY="production-maker"}]);
        var review=RevenueSalesContractReview.Create(source,"rev",new(){SALES_CONTRACT_ID="contract",SELLER_BA_ID="company",BUYER_BA_ID="buyer",ACTIVE_IND="Y",ROW_CREATED_BY="contract-maker",CURRENCY_CODE="KWD",BASE_PRICE=null});
        return new(Guid.NewGuid().ToString("N"),source.Company,"commercial-preparer",review,new("version-1",From,Through,"KWD",12.345m,"KWD per recorded barrel","Fixed contract price","Proposed terms for independent review"));
    }
    [Theory][InlineData("valid")][InlineData("uncertain")][InlineData("competing")][InlineData("source")][InlineData("authority")][InlineData("revoke")][InlineData("schema")][InlineData("write")][InlineData("duplicate")][InlineData("tamper")][InlineData("foreign")][InlineData("borrowed")][InlineData("isolation")]
    public async Task DurableCommercialIntentRetainsExactReviewedCommandBeforeBusinessMutation(string mode)
    {
        var f=new Fixture();f.Native.Connection="production";f.Contract.ROW_CREATED_BY="contract-maker";f.Row.ROW_CREATED_BY="production-maker";
        var original=FieldRevenueSourceReview.Create(new("tenant","company"),"preparer","field",From,Through,"binding",[f.Row]);
        var source=RevenueSalesContractReview.Create(original,"revenue-a",f.Contract);
        var preparation=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),original.Company,"preparer",source,CommercialPreparation().Terms);
        var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);var prepared=SalesContractTermsRecord.Create(preparation,at);
        var rejected=prepared.Decide(new(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",prepared.Revision,SalesContractTermsDecision.Reject,"Independent rejection"),source,at.AddMinutes(1));
        f.Contract.ROW_CHANGED_BY="reviewer";f.Contract.ROW_CHANGED_DATE=at.AddMinutes(1);f.Contract.TERMS_WORKFLOW_JSON=SalesContractTermsNativeProof.Create(rejected,f.Contract);
        var fresh=RevenueSalesContractReview.Create(original,"revenue-a",f.Contract);
        var review=new CompanyCommercialAmendmentReview(preparation.Company,"preparer","process",new string('A',64),new string('B',64),f.Contract,rejected,SalesContractTermsAmendment.NormalizeSource(fresh,rejected));
        var command=new CompanyCommercialAmendmentSubmission(review,review.CreateIntent(Guid.NewGuid().ToString("N"),preparation.Terms with{Version="amended-v2",Price=30.125m},"Correct the rejected terms"));
        var before=System.Text.Json.JsonSerializer.Serialize(f.Contract);var revoked=false;
        Task Guard(){if(revoked || mode=="authority")throw new UnauthorizedAccessException();return Task.CompletedTask;}
        Task<RevenueSalesContractReview> Current()=>Task.FromResult(RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(preparation.Company,"preparer","field",From,Through,"binding",[f.Row]),"revenue-a",f.Native.Rows.OfType<SALES_CONTRACT>().Single()));
        f.MissingIntentSchema=mode=="schema";f.Native.FailCommit=mode=="uncertain";f.Native.FailWrite=mode=="write"?1:0;f.Native.Borrowed=mode=="borrowed";
        if(mode=="isolation")f.Native.Isolation=System.Data.IsolationLevel.ReadCommitted;
        if(mode=="source")f.Row.GROSS_REVENUE=999;
        f.AfterIntentWrite=()=>{if(mode=="revoke")revoked=true;};
        if(mode is "source" or "authority" or "revoke" or "schema" or "write" or "borrowed" or "isolation")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SaveCommercialAmendmentIntentAsync(command,"production",Guard,Current));Assert.Empty(f.Native.Rows.OfType<COMMERCIAL_TERMS_INTENT>());Assert.Equal(before,System.Text.Json.JsonSerializer.Serialize(f.Native.Rows.OfType<SALES_CONTRACT>().Single()));return;
        }
        if(mode=="uncertain"){await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SaveCommercialAmendmentIntentAsync(command,"production",Guard,Current));f.Native.FailCommit=false;}
        var entry=await f.Service.SaveCommercialAmendmentIntentAsync(command,"production",Guard,Current);Assert.Equal(1,f.Native.Writes);Assert.Single(f.Native.Rows.OfType<COMMERCIAL_TERMS_INTENT>());
        var index=await f.Service.GetCommercialAmendmentIntentsAsync(preparation.Company,"preparer","production",Guard);Assert.Equal(entry.RECORDED_REVISION,Assert.Single(index).RECORDED_REVISION);index[0].COMMAND_JSON="{}";Assert.Equal(entry.RECORDED_REVISION,(await f.Service.GetCommercialAmendmentIntentsAsync(preparation.Company,"preparer","production",Guard)).Single().RECORDED_REVISION);
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(command),System.Text.Json.JsonSerializer.Serialize(entry.Restore(preparation.Company,"preparer","process")));
        Assert.Equal(before,System.Text.Json.JsonSerializer.Serialize(f.Native.Rows.OfType<SALES_CONTRACT>().Single()));
        var stored=f.Native.Rows.OfType<COMMERCIAL_TERMS_INTENT>().Single();
        if(mode=="competing"){var other=new CompanyCommercialAmendmentSubmission(review,review.CreateIntent(Guid.NewGuid().ToString("N"),preparation.Terms with{Version="another",Price=33m},"Competing proposal"));await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SaveCommercialAmendmentIntentAsync(other,"production",Guard,Current));Assert.Equal(1,f.Native.Writes);return;}
        if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(stored));
        if(mode=="tamper")stored.COMMAND_JSON="{}";
        if(mode=="foreign")stored.OWNER_COMPANY_BA_ID="foreign";
        if(mode is "duplicate" or "tamper"){await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.GetCommercialAmendmentIntentAsync(preparation.Company,"preparer","process","production",Guard));await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.GetCommercialAmendmentIntentsAsync(preparation.Company,"preparer","production",Guard));}
        else if(mode=="foreign"){Assert.Null(await f.Service.GetCommercialAmendmentIntentAsync(preparation.Company,"preparer","process","production",Guard));await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.SaveCommercialAmendmentIntentAsync(command,"production",Guard,Current));}
        else{var recovered=await f.Service.GetCommercialAmendmentIntentAsync(preparation.Company,"preparer","process","production",Guard);Assert.Equal(entry.RECORDED_REVISION,recovered!.RECORDED_REVISION);f.Row.GROSS_REVENUE=999;Assert.Equal(entry.RECORDED_REVISION,(await f.Service.GetCommercialAmendmentIntentAsync(preparation.Company,"preparer","process","production",Guard))!.RECORDED_REVISION);}
        Assert.Equal(1,f.Native.Writes);
    }
    [Theory][InlineData("valid")][InlineData("company")][InlineData("actor")][InlineData("binding")][InlineData("revision")][InlineData("null")][InlineData("late")]
    public async Task CommercialProposalQueueTypedClientRequiresOriginalEvidence(string mode)
    {
        var scope=new CompanyScope("tenant","company");var binding=new string('B',64);CompanyCommercialAmendmentQueueRow[] rows=[];
        var queue=new CompanyCommercialAmendmentQueue(scope,"actor",binding,rows,ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(new{Company=scope,ActorUserId="actor",BindingRevision=binding,Rows=rows})));
        if(mode=="company")queue=queue with{Company=new("tenant","foreign")};if(mode=="actor")queue=queue with{ActorUserId="other"};if(mode=="binding")queue=queue with{BindingRevision="unconfirmed"};if(mode=="revision")queue=queue with{Revision=new string('A',64)};
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(scope,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust"]));
        using var http=new HttpClient(new Reply(()=>{if(mode=="late")company.Clear();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:queue)};})){BaseAddress=new Uri("https://fixture/")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        if(mode=="valid")Assert.Empty((await client.GetCommercialAmendmentQueueAsync()).Rows);else await Assert.ThrowsAnyAsync<Exception>(()=>client.GetCommercialAmendmentQueueAsync());
    }
    [Theory][InlineData("empty")][InlineData("denied")][InlineData("failure")][InlineData("late")]
    public async Task CommercialProposalQueueScreenClearsUnconfirmedAndChangedAccountEvidence(string mode)
    {
        var scope=new CompanyScope("tenant","company");var binding=new string('B',64);CompanyCommercialAmendmentQueueRow[] rows=[];
        var queue=new CompanyCommercialAmendmentQueue(scope,"actor",binding,rows,ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(new{Company=scope,ActorUserId="actor",BindingRevision=binding,Rows=rows})));
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(scope,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust"]));
        var client=new Mock<IBusinessProcessServiceClient>();client.Setup(value=>value.GetCommercialAmendmentPageAsync(It.IsAny<int>(),It.IsAny<int>(),It.IsAny<CancellationToken>())).ReturnsAsync((int page,int size,CancellationToken token)=>{if(mode=="failure")throw new InvalidOperationException("Private provider details");if(mode=="late")company.Clear();return CompanyCommercialAmendmentPage.Create(queue,page,size,0);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new CommercialAuth(mode=="denied"?"other":"actor"));services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager>(new CommercialProcessNavigation("/accounting/revenue-workbench"));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<CommercialSavedProposalQueue>();var html=root.ToHtmlString();Assert.DoesNotContain("Private provider details",html);
            if(mode=="empty")Assert.Contains("No unresolved saved commercial proposals",html);
            if(mode is "denied" or "failure")Assert.Contains("could not be confirmed",html);
            if(mode=="late"){Assert.Contains("Select a company",html);Assert.DoesNotContain("No unresolved saved",html);Assert.DoesNotContain("Retrieved UTC:",html);}
        });
        if(mode=="denied")client.Verify(value=>value.GetCommercialAmendmentPageAsync(It.IsAny<int>(),It.IsAny<int>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("valid")][InlineData("duplicate")][InlineData("overflow")][InlineData("tamper")][InlineData("drift")][InlineData("revoked")][InlineData("foreign")][InlineData("schema")]
    public async Task SavedCommercialQueueRequiresCompleteStableOwnedSet(string mode)
    {
        var f=new Fixture();var preparation=CommercialPreparation();var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);
        var prepared=SalesContractTermsRecord.Create(preparation,at);var rejected=prepared.Decide(new(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",prepared.Revision,SalesContractTermsDecision.Reject,"Rejected for correction"),preparation.Source,at.AddMinutes(1));
        var previous=CommercialReview(preparation,rejected,preparation.ActorUserId);var review=new CompanyCommercialAmendmentReview(preparation.Company,preparation.ActorUserId,"process",new string('A',64),new string('B',64),previous.Source.Contract,rejected,SalesContractTermsAmendment.NormalizeSource(previous.Source,rejected));
        var command=new CompanyCommercialAmendmentSubmission(review,review.CreateIntent(Guid.NewGuid().ToString("N"),preparation.Terms with{Version="v2"},"Correction"));var entry=COMMERCIAL_TERMS_INTENT.Create(command,at.AddMinutes(2));
        f.Native.Rows.Add(entry);if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(entry));if(mode=="overflow")for(var index=0;index<CompanyCommercialAmendmentQueue.MaximumEntries;index++)f.Native.Rows.Add(CompanyInventorySourceReview.Copy(entry));
        if(mode=="tamper")entry.COMMAND_JSON="{}";if(mode=="foreign")entry.OWNER_COMPANY_BA_ID="foreign";if(mode=="schema")f.MissingIntentSchema=true;
        var guards=0;Task Guard(){++guards;if(guards==3 && mode=="drift")f.Native.Rows.Remove(entry);if(guards==3 && mode=="revoked")throw new UnauthorizedAccessException();return Task.CompletedTask;}
        if(mode is "valid" or "foreign")
        {
            var rows=await f.Service.GetCommercialAmendmentIntentsAsync(preparation.Company,preparation.ActorUserId,f.Native.Connection,Guard);
            if(mode=="foreign")Assert.Empty(rows);else Assert.Equal(entry.RECORDED_REVISION,Assert.Single(rows).RECORDED_REVISION);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.GetCommercialAmendmentIntentsAsync(preparation.Company,preparation.ActorUserId,f.Native.Connection,Guard));
        Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    private static CompanyCommercialAmendmentSubmission CommercialQueueSubmission(string process)
    {
        var preparation=CommercialPreparation();var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);var prepared=SalesContractTermsRecord.Create(preparation,at);
        var rejected=prepared.Decide(new(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",prepared.Revision,SalesContractTermsDecision.Reject,"Independent rejection"),preparation.Source,at.AddMinutes(1));
        var previous=CommercialReview(preparation,rejected,preparation.ActorUserId);var review=new CompanyCommercialAmendmentReview(preparation.Company,preparation.ActorUserId,process,new string('A',64),new string('B',64),previous.Source.Contract,rejected,SalesContractTermsAmendment.NormalizeSource(previous.Source,rejected));
        return new(review,review.CreateIntent(Guid.NewGuid().ToString("N"),preparation.Terms with{Version="v2"},"Explicit reviewed correction"));
    }
    private static CompanyCommercialAmendmentQueue CommercialQueue(params CompanyCommercialAmendmentSubmission[] commands)
    {
        var company=commands[0].Review.Company;var actor=commands[0].Review.ActorUserId;var binding=commands[0].Review.BindingRevision;
        var rows=commands.OrderBy(command=>command.Review.InstanceId,StringComparer.Ordinal).Select(command=>new CompanyCommercialAmendmentQueueRow(COMMERCIAL_TERMS_INTENT.Create(command,new DateTime(2026,2,1,0,2,0,DateTimeKind.Utc)),new(company,actor,command.Review.InstanceId,"contract",CompanyAccountingSourceTypes.CommercialPreparationRevision(command.Review.Previous),command.Review.ProcessRevision,SalesContractTermsNativeProof.HistoricalReview(company,actor,command.Review.PreviousNativeContract,CompanyAccountingSourceTypes.CommercialPreparationRevision(command.Review.Previous)),command.Review.PreviousNativeContract))).ToArray();
        return new(company,actor,binding,rows,ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(new{Company=company,ActorUserId=actor,BindingRevision=binding,Rows=rows})));
    }
    [Theory][InlineData("valid")][InlineData("company")][InlineData("actor")][InlineData("page")][InlineData("size")][InlineData("count")][InlineData("revision")][InlineData("null")][InlineData("late")][InlineData("invalid")]
    public async Task CommercialProposalPageTypedClientRequiresExactRequestAndCurrentAccount(string mode)
    {
        var command=CommercialQueueSubmission("process");var queue=CommercialQueue(command);var result=CompanyCommercialAmendmentPage.Create(queue,1,1,2);
        if(mode=="company")result=result with{Queue=queue with{Company=new("tenant","foreign")}};if(mode=="actor")result=result with{Queue=queue with{ActorUserId="other"}};
        if(mode=="page")result=result with{PageNumber=2};if(mode=="size")result=result with{PageSize=2};if(mode=="count")result=result with{TotalRecords=0};if(mode=="revision")result=result with{Revision=new string('C',64)};
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),queue.ActorUserId,new(queue.Company,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust"]));var reads=0;
        using var http=new HttpClient(new Reply(()=>{++reads;if(mode=="late")company.Clear();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:result)};})){BaseAddress=new Uri("https://fixture/")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        if(mode=="valid"){var page=await client.GetCommercialAmendmentPageAsync(1,1);Assert.Equal(2,page.TotalRecords);Assert.True(page.HasNextPage);Assert.False(page.HasPreviousPage);Assert.Equal(command.Amendment.Preparation.IntentId,page.Queue.Rows.Single().Entry.COMMERCIAL_TERMS_INTENT_ID);}
        else await Assert.ThrowsAnyAsync<Exception>(()=>client.GetCommercialAmendmentPageAsync(mode=="invalid"?0:1,1));if(mode=="invalid")Assert.Equal(0,reads);
    }
    [Theory][InlineData("navigate")][InlineData("failure")][InlineData("late")]
    public async Task CommercialProposalScreenPagesReviewIndependentCountsAndClearFailedNavigation(string mode)
    {
        var commands=new[]{CommercialQueueSubmission("process-a"),CommercialQueueSubmission("process-b")};var full=CommercialQueue(commands);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),full.ActorUserId,new(full.Company,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust"]));
        var client=new Mock<IBusinessProcessServiceClient>();var requests=new List<(int Page,int Size)>();
        client.Setup(value=>value.GetCommercialAmendmentPageAsync(It.IsAny<int>(),It.IsAny<int>(),It.IsAny<CancellationToken>())).ReturnsAsync((int page,int size,CancellationToken token)=>{
            requests.Add((page,size));if(page==2 && mode=="failure")throw new InvalidOperationException("Private provider details");if(page==2 && mode=="late")company.Clear();
            return CompanyCommercialAmendmentPage.Create(CommercialQueue(commands.Skip((page-1)*size).Take(size).ToArray()),page,size,2);
        });
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new CommercialAuth(full.ActorUserId));services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<NavigationManager>(new CommercialProcessNavigation("/accounting/revenue-workbench"));services.AddSingleton<IComponentActivator,CommercialProcessComponents>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<CommercialSavedProposalQueue>();var component=((CommercialProcessComponents)provider.GetRequiredService<IComponentActivator>()).Queue!;
            Assert.Contains("Saved proposals: 2",root.ToHtmlString());
            async Task Invoke(string name,int value){await (Task)component.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(component,[value])!;component.GetType().GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(component,[]);}
            await Invoke("ChangePageSize",1);Assert.Contains("Page 1 of 2",root.ToHtmlString());Assert.Contains("Unresolved on this page (1)",root.ToHtmlString());Assert.Contains(commands[0].Amendment.Preparation.IntentId,root.ToHtmlString());Assert.DoesNotContain(commands[1].Amendment.Preparation.IntentId,root.ToHtmlString());
            await Invoke("LoadPage",2);var html=root.ToHtmlString();Assert.DoesNotContain(commands[0].Amendment.Preparation.IntentId,html);Assert.DoesNotContain("Private provider details",html);
            if(mode=="navigate"){Assert.Contains("Page 2 of 2",html);Assert.Contains(commands[1].Amendment.Preparation.IntentId,html);Assert.Contains("/accounting/processes/process-b",html);}
            if(mode=="failure"){Assert.Contains("could not be confirmed",html);Assert.DoesNotContain(commands[1].Amendment.Preparation.IntentId,html);}
            if(mode=="late"){Assert.Contains("Select a company",html);Assert.DoesNotContain(commands[1].Amendment.Preparation.IntentId,html);}
            Assert.Equal(new[]{(1,8),(1,1),(2,1)},requests);
        });
        client.Verify(value=>value.GetCommercialAmendmentQueueAsync(It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("first")][InlineData("last")][InlineData("page")][InlineData("size")][InlineData("total")][InlineData("next")][InlineData("previous")][InlineData("count")][InlineData("foreign")][InlineData("duplicate")][InlineData("inactive")][InlineData("provider")][InlineData("drift")][InlineData("null")][InlineData("invalid")]
    public async Task CommercialBeepProposalPagesRefuseIncompleteProviderMetadataAndMembership(string mode)
    {
        var f=new Fixture();var first=CommercialQueueSubmission("process-a");var second=CommercialQueueSubmission("process-b");var at=new DateTime(2026,2,1,0,2,0,DateTimeKind.Utc);
        var one=COMMERCIAL_TERMS_INTENT.Create(first,at);var two=COMMERCIAL_TERMS_INTENT.Create(second,at);f.Native.Rows.Add(one);f.Native.Rows.Add(two);var captures=0;
        if(mode=="provider")f.Native.Source.SetupGet(value=>value.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Failed});
        if(mode is not ("first" or "last" or "provider" or "invalid"))f.Native.Source.Setup(value=>value.GetEntity("COMMERCIAL_TERMS_INTENT",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>(),It.IsAny<int>(),It.IsAny<int>())).Returns((string table,List<TheTechIdea.Beep.Report.AppFilter> filters,int page,int size)=>{
            if(mode=="null")return null!;
            var entries=new[]{CompanyInventorySourceReview.Copy(one),CompanyInventorySourceReview.Copy(two)};
            if(mode=="foreign")entries[0].OWNER_COMPANY_BA_ID="foreign";if(mode=="inactive")entries[0].ACTIVE_IND="N";if(mode=="duplicate")entries[1]=CompanyInventorySourceReview.Copy(entries[0]);
            var result=new PagedResult(mode=="count"?entries.Take(1).ToArray():entries,page,size,mode=="drift" && ++captures>1?3:2);
            if(mode=="page")result.PageNumber=2;if(mode=="size")result.PageSize=1;if(mode=="total")result.TotalPages=0;if(mode=="next")result.HasNextPage=true;if(mode=="previous")result.HasPreviousPage=true;return result;
        });
        var number=mode=="last"?2:mode=="invalid"?0:1;var count=mode is "first" or "last"?1:2;
        Task<(IReadOnlyList<COMMERCIAL_TERMS_INTENT> Entries,int Total)> Read()=>f.Service.GetCommercialAmendmentIntentPageAsync(first.Review.Company,first.Review.ActorUserId,f.Native.Connection,number,count,()=>Task.CompletedTask);
        if(mode is "first" or "last"){var result=await Read();Assert.Equal(2,result.Total);Assert.Equal(mode=="first"?one.RECORDED_REVISION:two.RECORDED_REVISION,Assert.Single(result.Entries).RECORDED_REVISION);}
        else await Assert.ThrowsAnyAsync<Exception>(()=>Read());Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Fact]
    public async Task ActualCommercialIntentWriterAboveThirtyTwoCanReadFinalNativePageWithoutWholeQueue()
    {
        var f=new Fixture();f.Contract.ROW_CREATED_BY="contract-maker";f.Row.ROW_CREATED_BY="production-maker";var scope=new CompanyScope("tenant","company");var actor="preparer";
        async Task<RevenueSalesContractReview> Read()=>RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(scope,actor,"field",From,Through,"binding",await f.Service.GetRecordedFieldRevenueAsync("field",From,Through,f.Native.Connection)),"revenue-a",await f.Service.GetRecordedSalesContractAsync("contract",scope,f.Native.Connection));
        var preparation=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),scope,actor,await Read(),CommercialPreparation().Terms);
        var prepared=await f.Service.PrepareSalesContractTermsAsync(preparation,f.Native.Connection,Read,()=>Task.CompletedTask);actor="reviewer";
        var rejected=await f.Service.DecideSalesContractTermsAsync(prepared,new(Guid.NewGuid().ToString("N"),scope,actor,prepared.Revision,SalesContractTermsDecision.Reject,"Independent rejection"),f.Native.Connection,Read,()=>Task.CompletedTask);actor="preparer";
        var native=await f.Service.GetRecordedSalesContractAsync("contract",scope,f.Native.Connection);var source=SalesContractTermsAmendment.NormalizeSource(await Read(),rejected);COMMERCIAL_TERMS_INTENT? last=null;
        for(var index=0;index<65;index++)
        {
            var review=new CompanyCommercialAmendmentReview(scope,actor,"process-"+index.ToString("D3"),new string('A',64),new string('B',64),native,rejected,source);
            var command=new CompanyCommercialAmendmentSubmission(review,review.CreateIntent(Guid.NewGuid().ToString("N"),preparation.Terms with{Version="v2-"+index},"Saved proposal "+index));
            last=await f.Service.SaveCommercialAmendmentIntentAsync(command,f.Native.Connection,()=>Task.CompletedTask,Read);
        }
        Assert.Equal(67,f.Native.Writes);Assert.Equal(65,f.Native.Rows.OfType<COMMERCIAL_TERMS_INTENT>().Count());
        var page=await f.Service.GetCommercialAmendmentIntentPageAsync(scope,actor,f.Native.Connection,3,32,()=>Task.CompletedTask);Assert.Equal(65,page.Total);Assert.Equal(last!.RECORDED_REVISION,Assert.Single(page.Entries).RECORDED_REVISION);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.GetCommercialAmendmentIntentsAsync(scope,actor,f.Native.Connection,()=>Task.CompletedTask));Assert.Equal(67,f.Native.Writes);Assert.Equal(rejected.Revision,SalesContractTermsNativeProof.Require(scope,await f.Service.GetRecordedSalesContractAsync("contract",scope,f.Native.Connection)).Record.Revision);
    }
    [Fact]
    public async Task SavedCommercialQueueAndFreshWorkspaceRecoverOlderRecordedAmendmentAfterAnotherVersion()
    {
        var f=new Fixture();f.Contract.ROW_CREATED_BY="contract-maker";f.Row.ROW_CREATED_BY="production-maker";var companyScope=new CompanyScope("tenant","company");var actor="preparer";
        async Task<RevenueSalesContractReview> Read()=>RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(companyScope,actor,"field",From,Through,"binding",await f.Service.GetRecordedFieldRevenueAsync("field",From,Through,f.Native.Connection)),"revenue-a",await f.Service.GetRecordedSalesContractAsync("contract",companyScope,f.Native.Connection));
        var firstPreparation=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),companyScope,actor,await Read(),CommercialPreparation().Terms);
        var first=await f.Service.PrepareSalesContractTermsAsync(firstPreparation,f.Native.Connection,Read,()=>Task.CompletedTask);actor="reviewer";
        first=await f.Service.DecideSalesContractTermsAsync(first,new(Guid.NewGuid().ToString("N"),companyScope,actor,first.Revision,SalesContractTermsDecision.Reject,"Independent first rejection"),f.Native.Connection,Read,()=>Task.CompletedTask);actor="preparer";
        var commands=new List<CompanyCommercialAmendmentSubmission>();var saved=new List<COMMERCIAL_TERMS_INTENT>();var previous=first;
        for(var version=2;version<=3;version++)
        {
            var native=await f.Service.GetRecordedSalesContractAsync("contract",companyScope,f.Native.Connection);
            var review=new CompanyCommercialAmendmentReview(companyScope,actor,"process-"+version,new string((char)('A'+version),64),new string('B',64),native,previous,SalesContractTermsAmendment.NormalizeSource(await Read(),previous));
            var command=new CompanyCommercialAmendmentSubmission(review,review.CreateIntent(Guid.NewGuid().ToString("N"),firstPreparation.Terms with{Version="v"+version,Price=version*10m},"Independent correction "+version));commands.Add(command);
            saved.Add(await f.Service.SaveCommercialAmendmentIntentAsync(command,f.Native.Connection,()=>Task.CompletedTask,Read));
            previous=await f.Service.AmendSalesContractTermsAsync(command.Amendment,f.Native.Connection,Read,()=>Task.CompletedTask);
            if(version==2){actor="new-reviewer";previous=await f.Service.DecideSalesContractTermsAsync(previous,new(Guid.NewGuid().ToString("N"),companyScope,actor,previous.Revision,SalesContractTermsDecision.Reject,"Independent second rejection"),f.Native.Connection,Read,()=>Task.CompletedTask);actor="preparer";}
        }
        var current=await f.Service.GetRecordedSalesContractAsync("contract",companyScope,f.Native.Connection);Assert.Equal(2,SalesContractTermsNativeProof.Require(companyScope,current).PriorVersions!.Count);Assert.Equal(7,f.Native.Writes);
        CompanyCommercialProcessSourceReview Source(CompanyCommercialAmendmentSubmission command)=>new(companyScope,actor,command.Review.InstanceId,"contract",CompanyAccountingSourceTypes.CommercialPreparationRevision(command.Review.Previous),command.Review.ProcessRevision,SalesContractTermsNativeProof.HistoricalReview(companyScope,actor,current,CompanyAccountingSourceTypes.CommercialPreparationRevision(command.Review.Previous)),current);
        var entries=await f.Service.GetCommercialAmendmentIntentsAsync(companyScope,actor,f.Native.Connection,()=>Task.CompletedTask);Assert.Equal(2,entries.Count);
        var rows=entries.Select(entry=>new CompanyCommercialAmendmentQueueRow(entry,Source(commands.Single(command=>command.Review.InstanceId==entry.PROCESS_INSTANCE_ID)))).ToArray();
        var queue=new CompanyCommercialAmendmentQueue(companyScope,actor,new string('B',64),rows,ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(new{Company=companyScope,ActorUserId=actor,BindingRevision=new string('B',64),Rows=rows})));
        queue.Require(companyScope,actor);Assert.All(queue.Rows,row=>Assert.Equal("RECORDED",row.Status));Assert.Equal("v2",queue.Rows[0].Recorded()!.Preparation.Terms.Version);Assert.Equal(SalesContractTermsDecision.Reject,queue.Rows[0].Recorded()!.Decision!.Decision);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(companyScope,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust"]));using var workspace=new CommercialAmendmentWorkspaceState(company);workspace.AttachOwner(new object(),commands[0].Review.InstanceId);
        var client=new Mock<IBusinessProcessServiceClient>();client.Setup(value=>value.GetCommercialAmendmentIntentAsync(commands[0].Review.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(saved[0]);client.Setup(value=>value.GetCompanyCommercialSourceAsync(commands[0].Review.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync(Source(commands[0]));
        client.Setup(value=>value.DiscoverCommercialHandoffAsync(It.IsAny<CompanyAccountingProcessStartCatalogQuery>(),It.IsAny<CancellationToken>())).ReturnsAsync((CompanyAccountingProcessStartCatalogQuery query,CancellationToken token)=>new CompanyCommercialHandoffReview(companyScope,actor,"contract",query.ExpectedSourceRevision!,new string('B',64),new string('E',64),null,null,null));
        await workspace.LoadAsync(client.Object);Assert.Null(workspace.Pending);Assert.Equal("v2",workspace.Recorded!.Preparation.Terms.Version);Assert.False(workspace.CanRetry);Assert.Equal(7,f.Native.Writes);
        client.Verify(value=>value.SubmitCommercialAmendmentAsync(It.IsAny<CompanyCommercialAmendmentSubmission>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("approved")][InlineData("first")][InlineData("last")][InlineData("open-end")][InlineData("unprepared")][InlineData("undecided")][InlineData("rejected")][InlineData("before")][InlineData("after")][InlineData("currency-missing")][InlineData("currency-mismatch")][InlineData("utc")][InlineData("local")][InlineData("source-tamper")][InlineData("native-tamper")]
    public async Task CommercialApplicabilityUsesExactApprovedStoredDateAndCurrencyWithoutFinancialDefaults(string mode)
    {
        var preparation=CommercialPreparation();var rows=CompanyInventorySourceReview.Copy(preparation.Source.Source.Rows.ToArray());
        rows[0].TRANSACTION_DATE=mode is "first" or "before"?From:Through.AddHours(23).AddMinutes(59);
        if(mode is "utc" or "local"){var stored=rows[0].TRANSACTION_DATE!.Value;rows[0].TRANSACTION_DATE=null;rows[0].TRANSACTION_DATE=DateTime.SpecifyKind(stored,mode=="utc"?DateTimeKind.Utc:DateTimeKind.Local);}
        if(mode=="currency-missing")rows[0].CURRENCY_CODE="";
        var source=RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(preparation.Company,preparation.ActorUserId,"field",From,Through,"binding",rows),"rev",preparation.Source.Contract);
        var terms=preparation.Terms;if(mode=="before")terms=terms with{EffectiveFrom=From.AddDays(1)};if(mode=="after")terms=terms with{EffectiveThrough=Through.AddDays(-1)};if(mode=="open-end")terms=terms with{EffectiveThrough=null};if(mode=="currency-mismatch")terms=terms with{CurrencyCode="EUR"};
        preparation=preparation with{Source=source,Terms=terms};var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);SalesContractTermsRecord? record=mode=="unprepared"?null:SalesContractTermsRecord.Create(preparation,at);
        if(record is not null && mode!="undecided")record=record.Decide(new(Guid.NewGuid().ToString("N"),preparation.Company,"reviewer",record.Revision,mode=="rejected"?SalesContractTermsDecision.Reject:SalesContractTermsDecision.Approve,"Independent applicability review"),preparation.Source,at.AddMinutes(1));
        var review=CommercialReview(preparation,record,preparation.ActorUserId);
        if(mode=="source-tamper"){review.Source.Source.Rows[0].GROSS_REVENUE=998;Assert.ThrowsAny<Exception>(()=>review.ReviewApplicability());return;}
        if(mode=="native-tamper"){review.Source.Contract.TERMS_WORKFLOW_JSON="{}";Assert.ThrowsAny<Exception>(()=>review.ReviewApplicability());return;}
        var expected=mode switch{"unprepared"=>SalesContractTermApplicability.PreparationRequired,"undecided"=>SalesContractTermApplicability.IndependentApprovalRequired,"rejected"=>SalesContractTermApplicability.Rejected,"before" or "after"=>SalesContractTermApplicability.OutsideEffectiveDates,"currency-missing"=>SalesContractTermApplicability.CurrencyRequired,"currency-mismatch"=>SalesContractTermApplicability.CurrencyMismatch,"utc" or "local"=>SalesContractTermApplicability.DateInterpretationRequired,_=>SalesContractTermApplicability.ApprovedStoredDateAndCurrency};
        var result=review.ReviewApplicability();result.Require(review);Assert.Equal(expected,result.Status);Assert.Equal(rows[0].TRANSACTION_DATE,result.StoredTransactionDate);Assert.Equal(rows[0].CURRENCY_CODE,result.RecordedCurrency);Assert.Equal(record?.Revision,result.TermsRevision);Assert.Equal(review.Source.Revision,result.SourceRevision);
        CompanyInventorySourceReview.Copy(result).Require(review);Assert.ThrowsAny<Exception>(()=>(result with{Status=(SalesContractTermApplicability)999}).Require(review));Assert.ThrowsAny<Exception>(()=>(result with{SourceRevision=new string('A',64)}).Require(review));
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),preparation.ActorUserId,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));var client=new Mock<IAccountingServiceClient>();client.Setup(value=>value.GetSalesContractTermsAsync("field",From,Through,"rev")).ReturnsAsync(review);
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<CommercialTermsWorkspaceState>();services.AddScoped<CompanyAccountingHandoffState>();services.AddSingleton(client.Object);services.AddSingleton(Mock.Of<IBusinessProcessServiceClient>());services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new CommercialAuth(preparation.ActorUserId));services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<CommercialTermsWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId","rev"}}));var html=root.ToHtmlString();Assert.Contains("Commercial terms applicability",html);Assert.Contains("measured quantity and units",html);Assert.Contains(result.SourceRevision,html);
            var message=expected switch{SalesContractTermApplicability.PreparationRequired=>"Record explicit commercial terms",SalesContractTermApplicability.IndependentApprovalRequired=>"An independent recorded approval is required",SalesContractTermApplicability.Rejected=>"These terms were independently rejected",SalesContractTermApplicability.OutsideEffectiveDates=>"outside the independently approved effective dates",SalesContractTermApplicability.CurrencyRequired=>"No currency is substituted",SalesContractTermApplicability.CurrencyMismatch=>"Currency conversion requires separate reviewed evidence",SalesContractTermApplicability.DateInterpretationRequired=>"explicit company calendar and timezone interpretation",_=>"approved terms cover this recorded calendar date and currency"};Assert.Contains(message,html);
        });
        client.Verify(value=>value.PrepareSalesContractTermsAsync(It.IsAny<SalesContractTermsPreparation>()),Times.Never);client.Verify(value=>value.DecideSalesContractTermsAsync(It.IsAny<SalesContractTermsDecisionSubmission>()),Times.Never);
    }
    private static SalesContractTermsReadReview CommercialReview(SalesContractTermsPreparation original,SalesContractTermsRecord? record,string actor)
    {
        var source=FieldRevenueSourceReview.Create(original.Company,actor,"field",From,Through,original.Source.Source.BindingRevision,original.Source.Source.Rows);
        var row=CompanyInventorySourceReview.Copy(original.Source.Contract);
        if(record is not null){row.ROW_CHANGED_BY=record.Decision?.ActorUserId ??record.Preparation.ActorUserId;row.ROW_CHANGED_DATE=(record.DecidedAtUtc ??record.PreparedAtUtc).AddMinutes(1);row.TERMS_WORKFLOW_JSON=SalesContractTermsNativeProof.Create(record,row);}
        var review=new SalesContractTermsReadReview(RevenueSalesContractReview.Create(source,"rev",row),record);review.Require(original.Company,actor,"field",From,Through,"rev");return review;
    }
    [Theory][InlineData("prepare")][InlineData("before")][InlineData("after")][InlineData("late")][InlineData("different")]
    [InlineData("decision")][InlineData("decision-after")][InlineData("readback-failure")]
    public async Task CommercialStateRetainsOneIntentAndRequiresExactNativeOutcomeAcrossFailures(string mode)
    {
        var original=CommercialPreparation();var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);var actor=mode.StartsWith("decision",StringComparison.Ordinal)?"reviewer":"commercial-preparer";
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(original.Company,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve"]));
        using var state=new CommercialTermsWorkspaceState(company);state.Attach("field",From,Through,"rev");
        SalesContractTermsRecord? stored=actor=="reviewer"?SalesContractTermsRecord.Create(original,at):null;
        var api=new Mock<IAccountingServiceClient>();var calls=0;var reads=0;var intents=new List<string>();
        api.Setup(value=>value.GetSalesContractTermsAsync("field",From,Through,"rev")).ReturnsAsync(()=>{++reads;if(mode=="readback-failure" && reads==2)throw new IOException("lost read-back");return CommercialReview(original,stored,actor);});
        api.Setup(value=>value.PrepareSalesContractTermsAsync(It.IsAny<SalesContractTermsPreparation>())).ReturnsAsync((SalesContractTermsPreparation intent)=>{
            ++calls;intents.Add(intent.IntentId);if(mode=="before" && calls==1)throw new IOException("not recorded");stored=SalesContractTermsRecord.Create(intent,at);
            if(mode is "after" or "different" && calls==1)throw new IOException("response lost");if(mode=="late")company.Clear();return stored;
        });
        api.Setup(value=>value.DecideSalesContractTermsAsync(It.IsAny<SalesContractTermsDecisionSubmission>())).ReturnsAsync((SalesContractTermsDecisionSubmission command)=>{
            ++calls;intents.Add(command.Command.IntentId);stored=stored!.Decide(command.Command,CommercialReview(original,stored,actor).Source is {} fresh?SalesContractTermsNativeProof.RestoreOriginalReview(fresh,stored):throw new Exception(),at.AddMinutes(5));
            if(mode=="decision-after")throw new IOException("decision response lost");return stored;
        });
        await state.LoadAsync(api.Object);
        async Task Submit(){if(actor=="reviewer")await state.DecideAsync(api.Object,SalesContractTermsDecision.Approve,"Independent explicit review");else await state.PrepareAsync(api.Object,original.Terms);}
        if(mode is "prepare" or "decision"){await Submit();Assert.False(state.HasPendingIntent);Assert.NotNull(state.Review?.Recorded);Assert.Equal(1,calls);return;}
        if(mode=="late")await Submit();else await Assert.ThrowsAnyAsync<Exception>(Submit);
        Assert.True(state.HasPendingIntent);Assert.Null(state.Review);
        await Assert.ThrowsAnyAsync<Exception>(()=>state.PrepareAsync(api.Object,original.Terms));Assert.Equal(1,calls);
        if(mode=="late")company.TrySelect(company.Clear(),actor,new(original.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
        if(mode=="different")stored=SalesContractTermsRecord.Create(original with{IntentId=Guid.NewGuid().ToString("N")},at);
        if(mode=="different"){await Assert.ThrowsAnyAsync<Exception>(()=>state.LoadAsync(api.Object));Assert.True(state.HasPendingIntent);Assert.Null(state.Review);return;}
        await state.LoadAsync(api.Object);
        if(mode=="before"){Assert.True(state.HasPendingIntent);await state.SubmitPendingAsync(api.Object);Assert.Equal(2,calls);Assert.Equal(intents[0],intents[1]);}
        else Assert.Equal(1,calls);
        Assert.False(state.HasPendingIntent);Assert.NotNull(state.Review?.Recorded);
    }
    private sealed class CommercialAuth(string actor):AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"app"))));}
    [Theory][InlineData("prepare")][InlineData("reviewer")][InlineData("same-maker")][InlineData("recorded")][InlineData("denied")][InlineData("failure")][InlineData("handoff")]
    public async Task CommercialPersonaPanelShowsExplicitTermsAndGatesIndependentDecisions(string mode)
    {
        var original=CommercialPreparation();var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);var actor=mode is "reviewer" or "recorded"?"reviewer":"commercial-preparer";
        SalesContractTermsRecord? record=mode is "reviewer" or "same-maker" or "recorded" or "handoff"?SalesContractTermsRecord.Create(original,at):null;
        if(mode=="recorded")record=record!.Decide(new(Guid.NewGuid().ToString("N"),original.Company,actor,record.Revision,SalesContractTermsDecision.Approve,"Independent approval"),original.Source,at.AddMinutes(5));
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(original.Company,["ConfiguredCompanyRole"],mode=="denied"?[]:mode=="handoff"?["Accounting.View","Workflow.Start"]:["Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve"]));
        var api=new Mock<IAccountingServiceClient>();api.Setup(value=>value.GetSalesContractTermsAsync("field",From,Through,"rev")).ReturnsAsync(()=>{if(mode=="failure")throw new IOException("lost");return CommercialReview(original,record,actor);});
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton<CommercialTermsWorkspaceState>();services.AddScoped<DeliveryAcceptanceWorkspaceState>();services.AddSingleton<CompanyAccountingHandoffState>();services.AddSingleton(Mock.Of<IBusinessProcessServiceClient>());services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton(api.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new CommercialAuth(actor));services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<CommercialTermsWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId","rev"}}));var html=root.ToHtmlString();
            if(mode=="prepare"){Assert.Contains("Explicit price unit and basis",html);Assert.Contains("Proposed terms version",html);Assert.Contains("Record preparation for independent review",html);}
            if(mode=="reviewer"){Assert.Contains("Approve reviewed terms",html);Assert.Contains("Reject reviewed terms",html);Assert.Contains("commercial-preparer",html);Assert.Contains("12.345",html);}
            if(mode=="same-maker"){Assert.DoesNotContain("Approve reviewed terms",html);Assert.Contains("Independent decision access",html);}
            if(mode=="handoff"){Assert.Contains("Hand off this source to a workflow",html);Assert.Contains("Review workflow choices",html);}
            if(mode=="recorded"){Assert.Contains("Recorded decision: Approve",html);Assert.Contains("Independent approval",html);Assert.DoesNotContain("Approve reviewed terms",html);}
            if(mode is "denied" or "failure"){Assert.Contains("could not be confirmed",html);Assert.DoesNotContain("Recorded proposal",html);Assert.DoesNotContain("Record preparation for independent review",html);}
        });
        if(mode=="denied")api.Verify(value=>value.GetSalesContractTermsAsync(It.IsAny<string>(),It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<string>()),Times.Never);
    }
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("preparer")][InlineData("contract-maker")][InlineData("production-maker")]
    [InlineData("unknown-maker")][InlineData("company")][InlineData("revision")][InlineData("reason")][InlineData("invalid-decision")]
    [InlineData("revenue")][InlineData("contract")][InlineData("binding")][InlineData("time")]
    public void CommercialTermsDecisionRequiresFrozenSourceExplicitTermsAndIndependentMaker(string mode)
    {
        var proposed=CommercialPreparation();var preparedAt=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);
        if(mode=="unknown-maker")proposed=proposed with{Source=RevenueSalesContractReview.Create(proposed.Source.Source,"rev",new(){SALES_CONTRACT_ID="contract",SELLER_BA_ID="company",ACTIVE_IND="Y"})};
        var record=SalesContractTermsRecord.Create(proposed,preparedAt);
        // The source reader changes actor provenance for the independent reviewer, while source facts remain frozen.
        var freshSource=FieldRevenueSourceReview.Create(record.Preparation.Company,"reviewer","field",From,Through,mode=="binding"?"changed":"binding",record.Preparation.Source.Source.Rows);
        var freshContract=CompanyInventorySourceReview.Copy(record.Preparation.Source.Contract);
        if(mode=="revenue"){freshSource.Rows.Single().GROSS_REVENUE=99;freshSource=FieldRevenueSourceReview.Create(freshSource.Company,"reviewer","field",From,Through,"binding",freshSource.Rows);}
        if(mode=="contract")freshContract.BUYER_BA_ID="other";
        var fresh=RevenueSalesContractReview.Create(freshSource,"rev",freshContract);
        var actor=mode switch{"preparer"=>"commercial-preparer","contract-maker"=>"contract-maker","production-maker"=>"production-maker",_=>"reviewer"};
        var command=new SalesContractTermsDecisionCommand(Guid.NewGuid().ToString("N"),mode=="company"?new("tenant","other"):record.Preparation.Company,actor,mode=="revision"?new string('A',64):record.Revision,
            mode=="invalid-decision"?(SalesContractTermsDecision)99:mode=="reject"?SalesContractTermsDecision.Reject:SalesContractTermsDecision.Approve,mode=="reason"?"":"Independent recorded decision");
        if(mode is "approve" or "reject")
        {
            var result=record.Decide(command,fresh,preparedAt.AddMinutes(5));result.Require();Assert.Equal(command,result.Decision);Assert.NotEqual(record.Revision,result.Revision);Assert.Equal(12.345m,result.Preparation.Terms.Price);Assert.Null(result.Preparation.Source.Contract.BASE_PRICE);
            var transported=CompanyInventorySourceReview.Copy(result);transported.Require();Assert.Equal(result.Revision,transported.Revision);
            Assert.ThrowsAny<Exception>(()=>result.Decide(command,fresh,preparedAt.AddMinutes(10)));
            proposed.Source.Contract.BUYER_BA_ID="mutated";Assert.Equal("buyer",result.Preparation.Source.Contract.BUYER_BA_ID);
            transported.Preparation.Source.Contract.DESCRIPTION="altered";Assert.ThrowsAny<Exception>(transported.Require);
        }
        else Assert.ThrowsAny<Exception>(()=>record.Decide(command,fresh,mode=="time"?preparedAt.AddMinutes(-1):preparedAt.AddMinutes(5)));
    }
    [Theory][InlineData("unit")][InlineData("price")][InlineData("date-kind")][InlineData("range")][InlineData("version")][InlineData("missing-date")][InlineData("intent")]
    public void CommercialPreparationNeverSuppliesMissingUnitDatesVersionOrIntent(string mode)
    {
        var preparation=CommercialPreparation();var terms=preparation.Terms;
        terms=mode switch{"unit"=>terms with{PriceUnit=""},"price"=>terms with{Price=-1},"date-kind"=>terms with{EffectiveFrom=DateTime.SpecifyKind(From,DateTimeKind.Utc)},"range"=>terms with{EffectiveThrough=From.AddDays(-1)},"version"=>terms with{Version=""},"missing-date"=>terms with{EffectiveFrom=default},_=>terms};
        preparation=preparation with{Terms=terms,IntentId=mode=="intent"?Guid.Empty.ToString("N"):preparation.IntentId};
        Assert.ThrowsAny<Exception>(()=>SalesContractTermsRecord.Create(preparation,new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc)));
    }
    [Theory][InlineData("valid")][InlineData("missing-net")][InlineData("missing-unit")][InlineData("open")][InlineData("zero")]
    [InlineData("missing-allocation")][InlineData("duplicate-allocation")][InlineData("wrong-link")][InlineData("foreign-field")][InlineData("inactive")][InlineData("duplicate-ticket")][InlineData("drift")][InlineData("grant")][InlineData("asset")]
    public async Task DeliverySourceUsesExactAllocationTicketIdentityAndCurrentCompanyFieldAuthority(string mode)
    {
        var f=new Fixture();f.Row.ALLOCATION_RESULT_ID="allocation";
        var allocation=new ALLOCATION_RESULT{ALLOCATION_RESULT_ID="allocation",ALLOCATION_REQUEST_ID=mode=="wrong-link"?"other":"ticket",ACTIVE_IND="Y",TOTAL_VOLUME=7};
        var ticket=new RUN_TICKET{RUN_TICKET_ID="ticket",RUN_TICKET_NUMBER="same-number-is-not-identity",FIELD_ID=mode=="foreign-field"?"other":"field",ACTIVE_IND=mode=="inactive"?"N":"Y",NET_VOLUME=mode=="missing-net"?null:mode=="zero"?0:7,VOLUME_OUOM=mode=="missing-unit"?null:"BBL",TICKET_STATUS=mode=="open"?"OPEN":"CERTIFIED",TICKET_DATE_TIME=From};
        f.Native.Source.Setup(value=>value.GetEntityAsync("ALLOCATION_RESULT",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>{
            Assert.Contains(filters,p=>p.FieldName=="ALLOCATION_RESULT_ID" && p.FilterValue=="allocation");
            return mode=="missing-allocation"?Array.Empty<object>():mode=="duplicate-allocation"?new object[]{allocation,allocation}:new object[]{allocation};
        });
        var ticketReads=0;
        f.Native.Source.Setup(value=>value.GetEntityAsync("RUN_TICKET",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table,List<TheTechIdea.Beep.Report.AppFilter> filters)=>{
            Assert.Contains(filters,p=>p.FieldName=="RUN_TICKET_ID" && p.FilterValue==allocation.ALLOCATION_REQUEST_ID);Assert.Contains(filters,p=>p.FieldName=="FIELD_ID" && p.FilterValue=="field");
            Assert.DoesNotContain(filters,p=>p.FieldName is "ALLOCATION_REQUEST_ID" or "RUN_TICKET_NUMBER");
            if(++ticketReads==2 && mode=="drift")ticket.NET_VOLUME=8;
            return mode=="duplicate-ticket"?new object[]{ticket,ticket}:new object[]{ticket};
        });
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync("actor","field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=mode!="asset"});
        await using var host=await AccountingSourceHttpTests.Start(permissions:mode=="grant"?[]:["Accounting.View"],configure:(services,context)=>services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection))));
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View"]));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var source=FieldRevenueSourceReview.Create(new("tenant","company"),"actor","field",From,Through,"binding",[f.Row]);
        if(mode=="valid")
        {
            var native=await f.Service.GetRecordedRevenueDeliveryAsync(RevenueSalesContractReview.Create(source,"revenue-a",f.Contract),f.Native.Connection);
            var webOptions=new JsonSerializerOptions(JsonSerializerDefaults.Web);
            var webRoundTrip=JsonSerializer.Deserialize<RevenueDeliverySourceReview>(JsonSerializer.Serialize(native,webOptions),webOptions)!;webRoundTrip.Require(native.Source);
        }
        if(mode is "valid" or "missing-net" or "missing-unit" or "open" or "zero")
        {
            var result=await client.GetRevenueDeliveryAsync(source,"revenue-a");result.Require(result.Source);Assert.Equal("ticket",result.TicketId);Assert.Equal(mode=="valid",result.HasCertifiedMeasuredQuantity);
            Assert.Equal(ticket.NET_VOLUME,result.NetVolume);Assert.Equal(ticket.VOLUME_OUOM,result.VolumeUnit);
            var copy=CompanyInventorySourceReview.Copy(result);copy.Require(result.Source);
            Assert.ThrowsAny<Exception>(()=>(copy with{Revision=new string('A',64)}).Require(result.Source));
            ticket.NET_VOLUME=99;Assert.NotEqual(ticket.NET_VOLUME,result.NetVolume);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>client.GetRevenueDeliveryAsync(source,"revenue-a"));
        if(mode is "grant" or "asset")Assert.Equal(0,ticketReads);Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("full")][InlineData("partial")][InlineData("opaque")][InlineData("unit")][InlineData("unit-case")]
    [InlineData("zero")][InlineData("overflow")][InlineData("precision")][InlineData("underflow")][InlineData("legacy")]
    [InlineData("receiving-none")][InlineData("receiving-pending")][InlineData("receiving-rejected")]
    [InlineData("terms-none")][InlineData("terms-pending")][InlineData("terms-rejected")][InlineData("currency")]
    [InlineData("tamper")][InlineData("source-change")][InlineData("legacy-json")][InlineData("historical")]
    public async Task BillingPriceBasisRequiresCurrentApprovedExplicitCompatibleNativeEvidence(string mode)
    {
        RevenueDeliveryAcceptanceReadReview original;
        if(mode=="historical")
        {
            var fixture=new DeliveryCorrectionFixture();var command=await fixture.Command();await fixture.Data.Service.CorrectDeliveryAcceptanceAsync(command,"production",fixture.Read,fixture.Guard);var current=await fixture.Read();
            original=RevenueDeliveryAcceptanceProof.HistoricalReview(fixture.Original.Company,fixture.Original.ActorUserId,current,CompanyAccountingSourceTypes.DeliveryPreparationRevision(fixture.Previous)) with{CurrentSource=current};
        }
        else original=BillingDeliveryReview(mode);
        var before=JsonSerializer.Serialize(original);var review=original.ReviewBillingBasis();review.Require(original);Assert.Equal(before,JsonSerializer.Serialize(original));
        var expected=mode switch{
            "opaque"=>RevenueDeliveryBillingBasisStatus.PricingMethodUnresolved,"unit" or "unit-case"=>RevenueDeliveryBillingBasisStatus.QuantityUnitMismatch,
            "zero"=>RevenueDeliveryBillingBasisStatus.ZeroPricePolicyRequired,"overflow" or "precision" or "underflow"=>RevenueDeliveryBillingBasisStatus.AmountOutOfRange,
            "legacy"=>RevenueDeliveryBillingBasisStatus.AcceptedQuantityRequired,"receiving-none"=>RevenueDeliveryBillingBasisStatus.ReceivingPreparationRequired,
            "receiving-pending"=>RevenueDeliveryBillingBasisStatus.ReceivingApprovalRequired,"receiving-rejected"=>RevenueDeliveryBillingBasisStatus.ReceivingRejected,
            "terms-none"=>RevenueDeliveryBillingBasisStatus.CommercialTermsRequired,"terms-pending"=>RevenueDeliveryBillingBasisStatus.CommercialApprovalRequired,
            "terms-rejected"=>RevenueDeliveryBillingBasisStatus.CommercialTermsRejected,"currency"=>RevenueDeliveryBillingBasisStatus.CommercialApplicabilityRequired,
            "historical"=>RevenueDeliveryBillingBasisStatus.HistoricalReceivingEvidence,_=>RevenueDeliveryBillingBasisStatus.CompatibleFixedUnitPrice};
        Assert.Equal(expected,review.Status);
        if(review.HasCompatiblePriceBasis){review.RequireCompatible(original);Assert.Equal(mode=="partial"?49.380m:86.415m,review.UnroundedAmount);Assert.Equal("KWD",review.CurrencyCode);Assert.Equal("BBL",review.QuantityUnit);Assert.Equal(original.Recorded!.Revision,review.DeliveryRevision);}
        else{Assert.Null(review.UnroundedAmount);Assert.Throws<InvalidOperationException>(()=>review.RequireCompatible(original));}
        if(mode=="tamper")Assert.Throws<InvalidOperationException>(()=>(review with{UnroundedAmount=999m}).Require(original));
        if(mode=="source-change")Assert.Throws<InvalidOperationException>(()=>review.Require(BillingDeliveryReview("partial")));
        if(mode=="legacy-json")
        {
            var terms=CommercialPreparation().Terms;Assert.DoesNotContain("FixedUnitPriceMethod",JsonSerializer.Serialize(terms));Assert.DoesNotContain("BillingBasis",JsonSerializer.Serialize(original));
            Assert.Equal(8,typeof(SalesContractEffectiveTerms).GetConstructors().Single(value=>value.IsPublic).GetParameters().Length);
        }
    }
    internal static RevenueDeliveryAcceptanceReadReview BillingDeliveryReview(string mode,string? binding=null)
    {
        var template=DeliveryPreparation();var previous=SalesContractTermsNativeProof.Require(template.Company,template.Source.Source.Contract).Record;
        var price=mode switch{"zero"=>0m,"overflow"=>decimal.MaxValue,"precision"=>1.0000000000000000000000000001m,"underflow"=>0.0000000000000000000000000001m,_=>12.345m};
        var preparation=previous.Preparation with{Terms=previous.Preparation.Terms with{Price=price,PriceUnit=mode=="unit"?"M3":mode=="unit-case"?"bbl":"BBL",PricingMethod=mode=="opaque"?"Fixed contract price":SalesContractEffectiveTerms.FixedUnitPriceMethod}};
        if(binding is not null)
        {var field=preparation.Source.Source;preparation=preparation with{Source=RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(field.Company,field.ActorUserId,field.FieldId,field.From,field.Through,binding,field.Rows),preparation.Source.TransactionId,preparation.Source.Contract)};}
        if(mode=="currency"){var rows=CompanyInventorySourceReview.Copy(preparation.Source.Source.Rows.ToArray());rows[0].CURRENCY_CODE="USD";var field=preparation.Source.Source;preparation=preparation with{Source=RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(field.Company,field.ActorUserId,field.FieldId,field.From,field.Through,field.BindingRevision,rows),preparation.Source.TransactionId,preparation.Source.Contract)};}
        var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);SalesContractTermsRecord? terms=mode=="terms-none"?null:SalesContractTermsRecord.Create(preparation,at);
        if(terms is not null && mode!="terms-pending")terms=terms.Decide(new(Guid.NewGuid().ToString("N"),template.Company,"terms-reviewer",terms.Revision,mode=="terms-rejected"?SalesContractTermsDecision.Reject:SalesContractTermsDecision.Approve,"Independent explicit price basis review"),preparation.Source,at.AddMinutes(1));
        var source=RevenueDeliverySourceReview.Create(CommercialReview(preparation,terms,preparation.ActorUserId).Source,template.Source.Allocation,template.Source.Ticket);
        if(mode.StartsWith("terms-",StringComparison.Ordinal) || mode is "currency" or "receiving-none")return new(source,null);
        var quantity=mode switch{"partial"=>4m,"precision"=>1.0000000000000000000000000001m,"underflow"=>0.0000000000000000000000000001m,_=>7m};
        var intent=template with{Source=source,Facts=template.Facts with{AcceptedQuantity=mode=="legacy"?null:quantity,AcceptedQuantityUnit=mode=="legacy"?null:"BBL"}};
        var receiving=RevenueDeliveryAcceptanceRecord.Create(intent,at.AddDays(1));
        if(mode!="receiving-pending")receiving=receiving.Decide(new(Guid.NewGuid().ToString("N"),template.Company,"receiving-reviewer",receiving.Revision,mode=="receiving-rejected"?RevenueDeliveryAcceptanceDecision.Reject:RevenueDeliveryAcceptanceDecision.Approve,"Independent actual receiving review"),source,at.AddDays(2));
        return DeliveryRecordedReview(source,receiving);
    }
    [Theory][InlineData("full")][InlineData("partial")][InlineData("opaque")][InlineData("unit")][InlineData("receiving-rejected")][InlineData("legacy")][InlineData("account")]
    public async Task BillingPriceBasisActualApiClientAndReceivingScreenShowApprovedInputsWithoutFinancialWrites(string mode)
    {
        var original=BillingDeliveryReview(mode=="account"?"partial":mode);var f=new Fixture();f.Native.Connection="production";f.Native.Rows.Clear();f.Native.Rows.AddRange(original.Source.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Source.Contract));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Allocation));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Ticket));
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View"],configure:(services,context)=>{context.Actor=original.Source.Source.Source.ActorUserId;services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult("production")));});host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",original.Source.Source.Source.ActorUserId);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),original.Source.Source.Source.ActorUserId,new(original.Source.Source.Source.Company,["ConfiguredCompanyRole"],["Accounting.View"]));var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var current=await client.GetDeliveryAcceptanceAsync("field",From,Through,"rev");current.Require();Assert.Equal(original.ReviewBillingBasis().Status,current.ReviewBillingBasis().Status);
        await using var provider=DeliveryServices(company,client);await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{var root=await renderer.RenderComponentAsync<DeliveryAcceptanceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId","rev"}}));var html=root.ToHtmlString();Assert.Contains("Receiving price basis for billing review",html);Assert.Contains("Terms revision:",html);Assert.Contains("Receiving revision:",html);
            if(mode is "full" or "partial" or "account"){Assert.Contains("before rounding, tax and charges",html);Assert.Contains((mode=="full"?86.415m:49.38m).ToString(System.Globalization.CultureInfo.CurrentCulture),html);}else Assert.DoesNotContain("before rounding, tax and charges:",html);
            if(mode=="opaque")Assert.Contains("free-text descriptions are not interpreted",html);if(mode=="unit")Assert.Contains("No unit or currency conversion is assumed",html);
            if(mode=="account"){((DeliveryAuth)provider.GetRequiredService<AuthenticationStateProvider>()).Revoke();await root.QuiescenceTask;Assert.DoesNotContain("Receiving price basis for billing review",root.ToHtmlString());}
        });Assert.Equal(0,f.Native.Writes);
    }
    private static RevenueDeliveryAcceptancePreparation DeliveryPreparation()
    {
        var terms=CommercialPreparation();var rows=CompanyInventorySourceReview.Copy(terms.Source.Source.Rows.ToArray());rows[0].ALLOCATION_RESULT_ID="allocation";
        var source=terms.Source.Source;var current=FieldRevenueSourceReview.Create(source.Company,source.ActorUserId,source.FieldId,source.From,source.Through,source.BindingRevision,rows);
        terms=terms with{Source=RevenueSalesContractReview.Create(current,terms.Source.TransactionId,terms.Source.Contract)};
        var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);var prepared=SalesContractTermsRecord.Create(terms,at);
        var approved=prepared.Decide(new(Guid.NewGuid().ToString("N"),terms.Company,"terms-reviewer",prepared.Revision,SalesContractTermsDecision.Approve,"Reviewed explicit terms"),terms.Source,at.AddMinutes(1));
        var contract=CommercialReview(terms,approved,terms.ActorUserId).Source;
        var delivery=RevenueDeliverySourceReview.Create(contract,new(){ALLOCATION_RESULT_ID="allocation",ALLOCATION_REQUEST_ID="ticket",ACTIVE_IND="Y",ROW_CREATED_BY="allocation-maker"},new(){RUN_TICKET_ID="ticket",FIELD_ID="field",ACTIVE_IND="Y",NET_VOLUME=7,VOLUME_OUOM="BBL",TICKET_STATUS="CERTIFIED",TICKET_DATE_TIME=From,ROW_CREATED_BY="measurement-maker"});
        return new(Guid.NewGuid().ToString("N"),terms.Company,terms.ActorUserId,delivery,new(at,"buyer","acceptance-reference","Actual receiving evidence reviewed"){AcceptedQuantity=7,AcceptedQuantityUnit="BBL"});
    }
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("maker")][InlineData("missing-maker")][InlineData("wrong-intent")][InlineData("future")][InlineData("local")][InlineData("buyer")][InlineData("reference")][InlineData("notes")][InlineData("quantity")][InlineData("unit")][InlineData("status")][InlineData("changed-source")][InlineData("tamper")]
    public void DeliveryAcceptanceBindsExplicitReceivingEvidenceToCertifiedMeasurementAndIndependentReviewer(string mode)
    {
        var preparation=DeliveryPreparation();var at=new DateTime(2026,2,2,0,0,0,DateTimeKind.Utc);
        if(mode=="future")preparation=preparation with{Facts=preparation.Facts with{AcceptedAtUtc=at.AddDays(1)}};
        if(mode=="local")preparation=preparation with{Facts=preparation.Facts with{AcceptedAtUtc=DateTime.SpecifyKind(preparation.Facts.AcceptedAtUtc,DateTimeKind.Local)}};
        if(mode=="buyer")preparation=preparation with{Facts=preparation.Facts with{ReceivingBusinessAssociateId="other"}};
        if(mode=="reference")preparation=preparation with{Facts=preparation.Facts with{EvidenceReference=""}};
        if(mode=="notes")preparation=preparation with{Facts=preparation.Facts with{Notes=""}};
        if(mode is "quantity" or "unit" or "status" or "missing-maker")
        {
            var original=preparation.Source;var ticket=CompanyInventorySourceReview.Copy(original.Ticket);
            if(mode=="quantity")ticket.NET_VOLUME=null;if(mode=="unit")ticket.VOLUME_OUOM="";if(mode=="status")ticket.TICKET_STATUS="OPEN";if(mode=="missing-maker")ticket.ROW_CREATED_BY="";
            preparation=preparation with{Source=RevenueDeliverySourceReview.Create(original.Source,original.Allocation,ticket)};
        }
        if(mode is "future" or "local" or "buyer" or "reference" or "notes" or "quantity" or "unit" or "status"){Assert.ThrowsAny<Exception>(()=>RevenueDeliveryAcceptanceRecord.Create(preparation,at));return;}
        var record=RevenueDeliveryAcceptanceRecord.Create(preparation,at);var command=new RevenueDeliveryAcceptanceDecisionCommand(mode=="wrong-intent"?Guid.Empty.ToString("N"):Guid.NewGuid().ToString("N"),preparation.Company,mode=="maker"?"measurement-maker":"delivery-reviewer",record.Revision,mode=="reject"?RevenueDeliveryAcceptanceDecision.Reject:RevenueDeliveryAcceptanceDecision.Approve,"Independent receiving review");
        var current=preparation.Source;
        if(mode=="changed-source"){var ticket=CompanyInventorySourceReview.Copy(current.Ticket);ticket.NET_VOLUME=8;current=RevenueDeliverySourceReview.Create(current.Source,current.Allocation,ticket);}
        if(mode is "maker" or "missing-maker" or "wrong-intent" or "changed-source"){Assert.ThrowsAny<Exception>(()=>record.Decide(command,current,at.AddMinutes(1)));return;}
        var decided=record.Decide(command,current,at.AddMinutes(1));decided.Require();Assert.Equal(command,decided.Decision);Assert.NotEqual(record.Revision,decided.Revision);
        var native=CompanyInventorySourceReview.Copy(current.Ticket);native.ROW_CHANGED_BY=command.ActorUserId;native.ROW_CHANGED_DATE=at.AddMinutes(2);native.DELIVERY_WORKFLOW_JSON=RevenueDeliveryAcceptanceProof.Create(decided,native);
        Assert.Equal(decided.Revision,RevenueDeliveryAcceptanceProof.Require(preparation.Company,native).Revision);
        if(mode=="tamper"){native.NET_VOLUME=8;Assert.ThrowsAny<Exception>(()=>RevenueDeliveryAcceptanceProof.Require(preparation.Company,native));}
        else{var web=new JsonSerializerOptions(JsonSerializerDefaults.Web);JsonSerializer.Deserialize<RevenueDeliveryAcceptanceRecord>(JsonSerializer.Serialize(decided,web),web)!.Require();}
    }
    [Theory][InlineData("prepare")][InlineData("approve")][InlineData("reject")][InlineData("uncertain")][InlineData("schema")][InlineData("authority")][InlineData("revoke")][InlineData("source")][InlineData("write")][InlineData("readback")][InlineData("borrowed")][InlineData("isolation")]
    public async Task NativeDeliveryAcceptanceOwnsSerializableWriteReadbackAndExactRetry(string mode)
    {
        var f=new Fixture();var preparation=DeliveryPreparation();f.Native.Rows.Add(CompanyInventorySourceReview.Copy(preparation.Source.Ticket));
        f.MissingDeliverySchema=mode=="schema";f.Native.Borrowed=mode=="borrowed";if(mode=="isolation")f.Native.Isolation=System.Data.IsolationLevel.ReadCommitted;
        var revoked=false;
        f.Native.Source.Setup(value=>value.UpdateEntity("RUN_TICKET",It.IsAny<object>())).Returns((string table,object value)=>{
            ++f.Native.Writes;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};
            var ticket=CompanyInventorySourceReview.Copy((RUN_TICKET)value);if(mode=="readback")ticket.NET_VOLUME=8;
            f.Native.Rows.RemoveAll(row=>row is RUN_TICKET);f.Native.Rows.Add(ticket);if(mode=="revoke")revoked=true;return new ErrorsInfo{Flag=Errors.Ok};
        });
        Task Guard(){if(mode=="authority" || revoked)throw new UnauthorizedAccessException();return Task.CompletedTask;}
        Task<RevenueDeliverySourceReview> Read()
        {
            var ticket=CompanyInventorySourceReview.Copy(f.Native.Rows.OfType<RUN_TICKET>().Single());if(mode=="source")ticket.NET_VOLUME=8;
            return Task.FromResult(RevenueDeliverySourceReview.Create(preparation.Source.Source,preparation.Source.Allocation,ticket));
        }
        if(mode is "schema" or "authority" or "revoke" or "source" or "write" or "readback" or "borrowed" or "isolation")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.PrepareDeliveryAcceptanceAsync(preparation,f.Native.Connection,Read,Guard));Assert.Null(f.Native.Rows.OfType<RUN_TICKET>().Single().DELIVERY_WORKFLOW_JSON);
            if(mode is "schema" or "authority" or "source" or "borrowed" or "isolation")Assert.Equal(0,f.Native.Writes);return;
        }
        f.Native.FailCommit=mode=="uncertain";
        if(mode=="uncertain"){await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.PrepareDeliveryAcceptanceAsync(preparation,f.Native.Connection,Read,Guard));Assert.NotNull(f.Native.Rows.OfType<RUN_TICKET>().Single().DELIVERY_WORKFLOW_JSON);Assert.DoesNotContain("rollback",f.Native.Transactions);f.Native.FailCommit=false;}
        var prepared=await f.Service.PrepareDeliveryAcceptanceAsync(preparation,f.Native.Connection,Read,Guard);Assert.Equal(1,f.Native.Writes);
        Assert.Equal(prepared.Revision,(await f.Service.PrepareDeliveryAcceptanceAsync(preparation,f.Native.Connection,Read,Guard)).Revision);Assert.Equal(1,f.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.PrepareDeliveryAcceptanceAsync(preparation with{IntentId=Guid.NewGuid().ToString("N")},f.Native.Connection,Read,Guard));Assert.Equal(1,f.Native.Writes);
        if(mode is "approve" or "reject")
        {
            var command=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,"delivery-reviewer",prepared.Revision,mode=="approve"?RevenueDeliveryAcceptanceDecision.Approve:RevenueDeliveryAcceptanceDecision.Reject,"Independent delivery review");
            var decided=await f.Service.DecideDeliveryAcceptanceAsync(prepared,command,f.Native.Connection,Read,Guard);Assert.Equal(command,decided.Decision);Assert.Equal(2,f.Native.Writes);
            Assert.Equal(decided.Revision,(await f.Service.DecideDeliveryAcceptanceAsync(prepared,command,f.Native.Connection,Read,Guard)).Revision);Assert.Equal(2,f.Native.Writes);
            var review=new RevenueDeliveryAcceptanceReadReview(await Read(),decided);review.Require();
            Assert.Equal(preparation.Facts.EvidenceReference,review.Recorded!.Preparation.Facts.EvidenceReference);
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.DecideDeliveryAcceptanceAsync(prepared,command with{IntentId=Guid.NewGuid().ToString("N")},f.Native.Connection,Read,Guard));Assert.Equal(2,f.Native.Writes);
        }
        Assert.Contains("rollback",f.Native.Transactions); // A deliberately competing intent is refused in its own transaction.
    }
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("uncertain")][InlineData("grant")][InlineData("maker")][InlineData("source")]
    [InlineData("form-approve")][InlineData("form-reject")][InlineData("form-uncertain")]
    public async Task DeliveryAcceptanceRealHttpClientPreservesOriginalIntentAndIndependentCurrentAuthority(string mode)
    {
        var f=new Fixture();var preparation=DeliveryPreparation();var original=preparation.Source;
        f.Native.Rows.RemoveAll(row=>row is REVENUE_TRANSACTION or SALES_CONTRACT);
        f.Native.Rows.AddRange(original.Source.Source.Rows.Select(CompanyInventorySourceReview.Copy));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Source.Contract));
        f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Allocation));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(original.Ticket));
        f.Native.Source.Setup(value=>value.UpdateEntity("RUN_TICKET",It.IsAny<object>())).Returns((string table,object value)=>{++f.Native.Writes;f.Native.FailCommit=mode is "uncertain" or "form-uncertain" && f.Native.Writes==2;f.Native.Rows.RemoveAll(row=>row is RUN_TICKET);f.Native.Rows.Add(CompanyInventorySourceReview.Copy((RUN_TICKET)value));return new ErrorsInfo{Flag=Errors.Ok};});
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),"field","FIELD",It.IsAny<string>(),null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        await using var host=await AccountingSourceHttpTests.Start(permissions:mode=="grant"?["Accounting.View"]:["Accounting.View","ProductionAccounting.Adjust"],configure:(services,context)=>{context.Actor=preparation.ActorUserId;services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));});
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor",preparation.ActorUserId);using var company=new CompanyClientContext();company.TrySelect(company.Clear(),preparation.ActorUserId,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust"]));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var first=await client.GetDeliveryAcceptanceAsync("field",From,Through,original.Source.TransactionId);Assert.Null(first.Recorded);
        if(mode=="grant"){await Assert.ThrowsAnyAsync<Exception>(()=>client.PrepareDeliveryAcceptanceAsync(preparation));Assert.Equal(0,f.Native.Writes);return;}
        f.Native.FailCommit=false;
        RevenueDeliveryAcceptanceRecord? formPrepared=null;
        if(mode.StartsWith("form-",StringComparison.Ordinal))
        {
            await using var services=DeliveryServices(company,client);await using var renderer=new HtmlRenderer(services,services.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async()=>{
                var root=await renderer.RenderComponentAsync<DeliveryAcceptanceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId",original.Source.TransactionId}}));
                var panel=services.GetRequiredService<DeliveryComponents>().Panel!;var type=typeof(DeliveryAcceptanceWorkbench);
                Assert.Contains("Record delivery preparation for independent review",root.ToHtmlString());
                foreach(var entry in new Dictionary<string,object?>{{"_dateUtc",preparation.Facts.AcceptedAtUtc.Date},{"_timeUtc",preparation.Facts.AcceptedAtUtc.TimeOfDay},{"_quantity",preparation.Facts.AcceptedQuantity},{"_buyer",preparation.Facts.ReceivingBusinessAssociateId},{"_reference",preparation.Facts.EvidenceReference},{"_notes",preparation.Facts.Notes}})type.GetField(entry.Key,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,entry.Value);
                typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null);await root.QuiescenceTask;
                await (Task)type.GetMethod("Prepare",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;await root.QuiescenceTask;
                var state=services.GetRequiredService<DeliveryAcceptanceWorkspaceState>();
                if(mode=="form-uncertain")
                {
                    Assert.True(state.HasPendingIntent);var identity=state.PendingPreparation!.IntentId;Assert.Contains("remains unconfirmed",root.ToHtmlString());Assert.Equal(2,f.Native.Writes);
                    f.Native.FailCommit=false;await (Task)type.GetMethod("Reload",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!;await root.QuiescenceTask;Assert.False(state.HasPendingIntent);Assert.Equal(identity,state.Review!.Recorded!.Preparation.IntentId);Assert.Equal(2,f.Native.Writes);
                }
                formPrepared=state.Review?.Recorded;Assert.NotNull(formPrepared);Assert.Contains("Recorded acceptance preparation",root.ToHtmlString());Assert.DoesNotContain("Approve reviewed delivery acceptance",root.ToHtmlString());Assert.Equal(preparation.Facts,formPrepared!.Preparation.Facts);
            });
            preparation=formPrepared!.Preparation;
        }
        if(mode=="uncertain"){await Assert.ThrowsAnyAsync<Exception>(()=>client.PrepareDeliveryAcceptanceAsync(preparation));Assert.NotNull(f.Native.Rows.OfType<RUN_TICKET>().Single().DELIVERY_WORKFLOW_JSON);f.Native.FailCommit=false;}
        if(mode=="approve"){var diagnostic=await host.Client.PostAsJsonAsync("api/fields/field/accounting/revenue-source/delivery/acceptance/prepare",preparation);Assert.True(diagnostic.IsSuccessStatusCode,await diagnostic.Content.ReadAsStringAsync());}
        var prepared=formPrepared ??await client.PrepareDeliveryAcceptanceAsync(preparation);Assert.Equal(2,f.Native.Writes);
        Assert.Equal(prepared.Revision,(await client.PrepareDeliveryAcceptanceAsync(preparation)).Revision);Assert.Equal(2,f.Native.Writes);
        var reviewer=mode=="maker"?"measurement-maker":"delivery-reviewer";host.Context.Actor=reviewer;host.Context.Permissions=["Accounting.View","ProductionAccounting.Approve"];
        host.Client.DefaultRequestHeaders.Remove("X-Test-Actor");host.Client.DefaultRequestHeaders.Add("X-Test-Actor",reviewer);company.TrySelect(company.Clear(),reviewer,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Approve"]));
        var command=new RevenueDeliveryAcceptanceDecisionCommand(Guid.NewGuid().ToString("N"),preparation.Company,reviewer,prepared.Revision,mode.EndsWith("reject",StringComparison.Ordinal)?RevenueDeliveryAcceptanceDecision.Reject:RevenueDeliveryAcceptanceDecision.Approve,"Independent receiving evidence review");
        if(mode=="source")f.Native.Rows.OfType<RUN_TICKET>().Single().NET_VOLUME=8;
        if(mode is "maker" or "source")
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>client.DecideDeliveryAcceptanceAsync(new(prepared,command)));
            var refusal=await host.Client.PostAsJsonAsync("api/fields/field/accounting/revenue-source/delivery/acceptance/decide",new RevenueDeliveryAcceptanceSubmission(prepared,command));
            Assert.Equal(HttpStatusCode.Conflict,refusal.StatusCode);Assert.Equal(2,f.Native.Writes);return;
        }
        RevenueDeliveryAcceptanceRecord? formDecided=null;
        if(mode.StartsWith("form-",StringComparison.Ordinal))
        {
            await using var services=DeliveryServices(company,client);await using var renderer=new HtmlRenderer(services,services.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async()=>{
                var root=await renderer.RenderComponentAsync<DeliveryAcceptanceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId",original.Source.TransactionId}}));var panel=services.GetRequiredService<DeliveryComponents>().Panel!;
                Assert.Contains("Approve reviewed delivery acceptance",root.ToHtmlString());typeof(DeliveryAcceptanceWorkbench).GetField("_reason",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,command.Reason);
                await (Task)typeof(DeliveryAcceptanceWorkbench).GetMethod("Decide",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,[command.Decision])!;await root.QuiescenceTask;
                formDecided=services.GetRequiredService<DeliveryAcceptanceWorkspaceState>().Review?.Recorded;Assert.NotNull(formDecided?.Decision);Assert.Contains("Recorded independent decision",root.ToHtmlString());Assert.DoesNotContain("Approve reviewed delivery acceptance",root.ToHtmlString());
            });
            command=formDecided!.Decision!;
        }
        var decided=formDecided ??await client.DecideDeliveryAcceptanceAsync(new(prepared,command));Assert.Equal(command,decided.Decision);Assert.Equal(4,f.Native.Writes);
        Assert.Equal(decided.Revision,(await client.DecideDeliveryAcceptanceAsync(new(prepared,command))).Revision);Assert.Equal(4,f.Native.Writes);
        var closing=await client.GetDeliveryAcceptanceAsync("field",From,Through,original.Source.TransactionId);closing.Require();Assert.Equal(decided.Revision,closing.Recorded!.Revision);Assert.Equal(preparation.IntentId,closing.Recorded.Preparation.IntentId);
        host.Context.Permissions=["Accounting.View"];var response=await host.Client.PostAsJsonAsync("api/fields/field/accounting/revenue-source/delivery/acceptance/decide",new RevenueDeliveryAcceptanceSubmission(prepared,command));Assert.Equal(HttpStatusCode.Forbidden,response.StatusCode);Assert.Equal(4,f.Native.Writes);
    }
    private static RevenueDeliveryAcceptanceReadReview DeliveryRecordedReview(RevenueDeliverySourceReview source,RevenueDeliveryAcceptanceRecord record)
    {
        var ticket=CompanyInventorySourceReview.Copy(source.Ticket);ticket.ROW_CHANGED_BY=record.Decision?.ActorUserId ??record.Preparation.ActorUserId;ticket.ROW_CHANGED_DATE=(record.DecidedAtUtc ??record.PreparedAtUtc).AddSeconds(1);ticket.DELIVERY_WORKFLOW_JSON=RevenueDeliveryAcceptanceProof.Create(record,ticket);
        return new(RevenueDeliverySourceReview.Create(source.Source,source.Allocation,ticket),record);
    }
    [Theory][InlineData("prepared")][InlineData("approved")][InlineData("rejected")][InlineData("wrong-revision")]
    [InlineData("role")][InlineData("grant")][InlineData("asset")][InlineData("foreign-company")][InlineData("closing-company")]
    [InlineData("closing-actor")][InlineData("closing-binding")][InlineData("closing-grant")][InlineData("closing-asset")]
    [InlineData("missing")][InlineData("duplicate")][InlineData("schema")][InlineData("native-tamper")][InlineData("source-drift")][InlineData("closing-native")]
    [InlineData("same-company-other-tenant")][InlineData("closing-source")][InlineData("ticket-number")][InlineData("metadata")]
    public async Task DeliveryProcessSourceReopensExactPreparationWithCurrentCompanyFieldAuthority(string mode)
    {
        var f=new Fixture();var preparation=DeliveryPreparation();var requestedId=mode=="ticket-number"?"human-ticket-number":"ticket";
        if(mode=="ticket-number")
        {
            var originalTicket=CompanyInventorySourceReview.Copy(preparation.Source.Ticket);originalTicket.RUN_TICKET_NUMBER=requestedId;
            preparation=preparation with{Source=RevenueDeliverySourceReview.Create(preparation.Source.Source,preparation.Source.Allocation,originalTicket)};
        }
        var record=RevenueDeliveryAcceptanceRecord.Create(preparation,DateTime.UtcNow);
        var revision=CompanyAccountingSourceTypes.DeliveryPreparationRevision(record);
        if(mode is "approved" or "rejected")record=record.Decide(new(Guid.NewGuid().ToString("N"),preparation.Company,"delivery-reviewer",record.Revision,
            mode=="approved"?RevenueDeliveryAcceptanceDecision.Approve:RevenueDeliveryAcceptanceDecision.Reject,"Independent receiving review"),preparation.Source,DateTime.UtcNow);
        Assert.Equal(revision,CompanyAccountingSourceTypes.DeliveryPreparationRevision(record));
        var stored=DeliveryRecordedReview(preparation.Source,record);var source=preparation.Source.Source.Source;
        f.Native.Rows.Clear();f.Native.Rows.AddRange(source.Rows.Select(CompanyInventorySourceReview.Copy));
        f.Native.Rows.Add(CompanyInventorySourceReview.Copy(preparation.Source.Source.Contract));f.Native.Rows.Add(CompanyInventorySourceReview.Copy(preparation.Source.Allocation));
        var ticket=CompanyInventorySourceReview.Copy(stored.Source.Ticket);if(mode!="missing")f.Native.Rows.Add(ticket);if(mode=="duplicate")f.Native.Rows.Add(CompanyInventorySourceReview.Copy(ticket));
        if(mode=="native-tamper")ticket.NET_VOLUME=99;if(mode=="source-drift")f.Native.Rows.OfType<REVENUE_TRANSACTION>().Single().GROSS_REVENUE=99;
        f.MissingDeliverySchema=mode=="schema";f.WrongDeliveryKey=mode=="metadata";var ticketReads=0;
        f.Native.Source.Setup(value=>value.GetEntityAsync("RUN_TICKET",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
            ++ticketReads;Assert.Contains(filters,value=>value.FieldName=="RUN_TICKET_ID" && value.FilterValue==requestedId);
            var rows=f.Native.Rows.OfType<RUN_TICKET>().Where(row=>filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).Select(CompanyInventorySourceReview.Copy).Cast<object>().ToArray();
            if(mode=="closing-native" && ticketReads==3)ticket.NET_VOLUME=99;
            if(mode=="closing-source" && ticketReads==4)f.Native.Rows.OfType<REVENUE_TRANSACTION>().Single().GROSS_REVENUE=99;return rows;
        });
        var context=new Mock<ICompanyAccountingReadContext>();var captures=0;var actor="delivery-reviewer";
        context.Setup(value=>value.CaptureAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(()=>{
            var closing=++captures>1;var company=mode=="foreign-company" || closing && mode=="closing-company"?new CompanyScope("other-tenant","other-company"):preparation.Company;
            if(mode=="same-company-other-tenant")company=new("other-tenant",preparation.Company.BusinessAssociateId);
            return(new RepositoryCompanyAccess(company,[mode=="role"?"Other":"ConfiguredCompanyRole"],mode=="grant" || closing && mode=="closing-grant"?[]:["Accounting.View"]),
                closing && mode=="closing-binding"?"changed":source.BindingRevision,closing && mode=="closing-actor"?"other-reviewer":actor);
        });
        var assets=new Mock<IAccessControlService>();var assetReads=0;
        assets.Setup(value=>value.CheckAssetAccessAsync(actor,"field","FIELD","READ",null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=mode!="asset" && (mode!="closing-asset" || ++assetReads<3)});
        var reads=new CompanyFieldRevenueReads(f.Service,context.Object,assets.Object,()=>Task.FromResult(f.Native.Connection));
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,actor),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"app"));
        if(mode is "prepared" or "approved" or "rejected")
        {
            var result=await reads.ReadProcessDeliveryAsync(user,"ticket",revision);result.Require();
            Assert.Equal(actor,result.Source.Source.Source.ActorUserId);Assert.Equal(source.FieldId,result.Source.Source.Source.FieldId);
            Assert.Equal(preparation.IntentId,result.Recorded!.Preparation.IntentId);Assert.Equal(record.Revision,result.Recorded.Revision);
            result.Source.Ticket.NET_VOLUME=999;Assert.Equal(7m,ticket.NET_VOLUME);
        }
        else
        {
            var failure=await Assert.ThrowsAnyAsync<Exception>(()=>reads.ReadProcessDeliveryAsync(user,requestedId,mode=="wrong-revision"?new string('a',64):revision));
            Assert.False(failure is Xunit.Sdk.XunitException,"The source owner must refuse; a fixture assertion is not refusal evidence.");
        }
        if(mode is "role" or "grant")Assert.Equal(0,ticketReads);
        if(mode is "asset" or "foreign-company")Assert.Equal(0,f.Reads);
        Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("retry")][InlineData("confirmed")][InlineData("changed")][InlineData("competing")][InlineData("other-company")][InlineData("late")][InlineData("decision-confirmed")][InlineData("decision-competing")]
    public async Task DeliveryAcceptanceWorkspaceRetainsOriginalIntentAndRefusesChangedOrCompetingRecovery(string mode)
    {
        var preparation=DeliveryPreparation();var decisionMode=mode.StartsWith("decision",StringComparison.Ordinal);var actor=decisionMode?"delivery-reviewer":preparation.ActorUserId;
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
        var source=preparation.Source;
        if(decisionMode){var field=source.Source.Source;source=RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(field.Company,actor,field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows),source.Source.TransactionId,source.Source.Contract),source.Allocation,source.Ticket);}
        var record=RevenueDeliveryAcceptanceRecord.Create(preparation,DateTime.UtcNow);RevenueDeliveryAcceptanceReadReview current=decisionMode?DeliveryRecordedReview(source,record):new(source,null);
        var journal=new List<DELIVERY_ACCEPTANCE_INTENT>();var api=new Mock<IAccountingServiceClient>();
        api.Setup(value=>value.GetDeliverySavedReviewAsync("field",From,Through,source.Source.TransactionId)).ReturnsAsync(()=>new RevenueDeliveryAcceptanceSavedReview(CompanyInventorySourceReview.Copy(current),CompanyInventorySourceReview.Copy(journal)));
        api.Setup(value=>value.SaveDeliveryIntentAsync(It.IsAny<RevenueDeliveryAcceptanceIntent>())).ReturnsAsync((RevenueDeliveryAcceptanceIntent command)=>{var entry=DELIVERY_ACCEPTANCE_INTENT.Create(command,DateTime.UtcNow);journal.Add(entry);return CompanyInventorySourceReview.Copy(entry);});
        api.Setup(value=>value.PrepareDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptancePreparation>())).Returns((RevenueDeliveryAcceptancePreparation original)=>{
            if(mode=="late"){company.Clear();return Task.FromResult(RevenueDeliveryAcceptanceRecord.Create(original,DateTime.UtcNow));}throw new IOException("lost delivery response");
        });
        api.Setup(value=>value.DecideDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptanceSubmission>())).ThrowsAsync(new IOException("lost decision response"));
        using var state=new DeliveryAcceptanceWorkspaceState(company);state.Attach("field",From,Through,source.Source.TransactionId);await state.LoadAsync(api.Object);
        if(decisionMode)await Assert.ThrowsAsync<IOException>(()=>state.DecideAsync(api.Object,RevenueDeliveryAcceptanceDecision.Approve,"Reviewed receiving evidence"));
        else if(mode=="late")await state.PrepareAsync(api.Object,preparation.Facts);else await Assert.ThrowsAsync<IOException>(()=>state.PrepareAsync(api.Object,preparation.Facts));
        Assert.True(state.HasPendingIntent);Assert.Null(state.Review);
        if(mode=="late")company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
        var pending=state.PendingPreparation;var decision=state.PendingDecision;var identity=decision?.IntentId ??pending!.IntentId;
        await Assert.ThrowsAnyAsync<Exception>(()=>state.PrepareAsync(api.Object,preparation.Facts));
        if(mode=="other-company")
        {company.TrySelect(company.Clear(),actor,new(new("tenant","other"),["ConfiguredCompanyRole"],["Accounting.View"]));Assert.Null(state.PendingPreparation);Assert.False(state.CanRetryPending);await Assert.ThrowsAnyAsync<Exception>(()=>state.SubmitPendingAsync(api.Object));Assert.True(state.HasPendingIntent);return;}
        if(mode=="changed"){var ticket=CompanyInventorySourceReview.Copy(source.Ticket);ticket.NET_VOLUME=8;current=new(RevenueDeliverySourceReview.Create(source.Source,source.Allocation,ticket),null);}
        if(mode is "confirmed" or "competing")current=DeliveryRecordedReview(source,RevenueDeliveryAcceptanceRecord.Create(mode=="confirmed"?pending!:pending! with{IntentId=Guid.NewGuid().ToString("N")},DateTime.UtcNow));
        if(decisionMode)current=DeliveryRecordedReview(source,record.Decide(mode=="decision-confirmed"?decision!:decision! with{IntentId=Guid.NewGuid().ToString("N")},source,DateTime.UtcNow));
        if(mode is "changed" or "competing" or "decision-competing")
        {await Assert.ThrowsAnyAsync<Exception>(()=>state.LoadAsync(api.Object));Assert.True(state.HasPendingIntent);Assert.Null(state.Review);Assert.False(state.CanRetryPending);Assert.Equal(identity,state.PendingDecision?.IntentId ??state.PendingPreparation!.IntentId);}
        else
        {await state.LoadAsync(api.Object);Assert.NotNull(state.Review);if(mode is "confirmed" or "decision-confirmed"){Assert.False(state.HasPendingIntent);Assert.False(state.CanRetryPending);}else{Assert.True(state.CanRetryPending);Assert.Equal(identity,state.PendingPreparation!.IntentId);}}
        api.Verify(value=>value.PrepareDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptancePreparation>()),decisionMode?Times.Never():Times.Once());api.Verify(value=>value.DecideDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptanceSubmission>()),decisionMode?Times.Once():Times.Never());
    }
    [Theory][InlineData("preparer")][InlineData("reviewer")][InlineData("maker")][InlineData("denied")][InlineData("failed")][InlineData("quantity")]
    public async Task RenderedDeliveryAcceptanceGatesFormsByCurrentRolesAndNativeMakerEvidence(string mode)
    {
        var preparation=DeliveryPreparation();var actor=mode=="reviewer"?"delivery-reviewer":preparation.ActorUserId;
        var source=preparation.Source;
        if(mode=="quantity"){var ticket=CompanyInventorySourceReview.Copy(source.Ticket);ticket.NET_VOLUME=null;source=RevenueDeliverySourceReview.Create(source.Source,source.Allocation,ticket);}
        if(mode=="reviewer"){var field=source.Source.Source;source=RevenueDeliverySourceReview.Create(RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(field.Company,actor,field.FieldId,field.From,field.Through,field.BindingRevision,field.Rows),source.Source.TransactionId,source.Source.Contract),source.Allocation,source.Ticket);}
        var review=mode is "reviewer" or "maker"?DeliveryRecordedReview(source,RevenueDeliveryAcceptanceRecord.Create(preparation,DateTime.UtcNow)):new RevenueDeliveryAcceptanceReadReview(source,null);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),actor,new(preparation.Company,["ConfiguredCompanyRole"],mode=="denied"?[]:["Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve"]));
        var api=new Mock<IAccountingServiceClient>();api.Setup(value=>value.GetDeliverySavedReviewAsync("field",From,Through,source.Source.TransactionId)).ReturnsAsync(new RevenueDeliveryAcceptanceSavedReview(review,[]));if(mode=="failed")api.Setup(value=>value.GetDeliverySavedReviewAsync("field",From,Through,source.Source.TransactionId)).ThrowsAsync(new IOException("private provider details"));
        await using var services=DeliveryServices(company,api.Object);await using var renderer=new HtmlRenderer(services,services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=await renderer.RenderComponentAsync<DeliveryAcceptanceWorkbench>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"FieldId","field"},{"From",From},{"Through",Through},{"TransactionId",source.Source.TransactionId}}));var html=root.ToHtmlString();
            Assert.DoesNotContain("private provider details",html);
            if(mode=="preparer"){Assert.Contains("Actual acceptance date in UTC",html);Assert.Contains("Receiving evidence reference",html);Assert.Contains("Record delivery preparation for independent review",html);}
            else Assert.DoesNotContain("Record delivery preparation for independent review",html);
            if(mode=="reviewer")Assert.Contains("Approve reviewed delivery acceptance",html);else Assert.DoesNotContain("Approve reviewed delivery acceptance",html);
            if(mode=="quantity"){Assert.Contains("Net measured quantity: Unavailable",html);Assert.Contains("certified measured quantity",html);}
            if(mode=="maker")Assert.Contains("Independent decision access",html);
            if(mode is "denied" or "failed")Assert.Contains("Delivery acceptance could not be confirmed",html);
            company.Clear();await root.QuiescenceTask;Assert.DoesNotContain("Recorded acceptance preparation",root.ToHtmlString());Assert.DoesNotContain("Receiving evidence reference",root.ToHtmlString());
        });
        api.Verify(value=>value.PrepareDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptancePreparation>()),Times.Never());api.Verify(value=>value.DecideDeliveryAcceptanceAsync(It.IsAny<RevenueDeliveryAcceptanceSubmission>()),Times.Never());
    }
    private sealed class DeliveryAuth(CompanyClientContext company):AuthenticationStateProvider
    {private bool revoked;public void Revoke(){revoked=true;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,revoked?"other":company.Selection?.UserId ??""),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"app"))));}
    private sealed class DeliveryComponents(IServiceProvider services):IComponentActivator
    {public DeliveryAcceptanceWorkbench? Panel;public DeliverySavedWorkQueue? Queue;public IComponent CreateInstance(Type type){var value=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(value is DeliveryAcceptanceWorkbench panel)Panel=panel;if(value is DeliverySavedWorkQueue queue)Queue=queue;return value;}}
    private static ServiceProvider DeliveryServices(CompanyClientContext company,IAccountingServiceClient client,Mock<IFieldSelectionContext>? selection=null,Action<ServiceCollection>? configure=null)
    {
        var fields=selection ??new Mock<IFieldSelectionContext>();if(selection is null)fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(client);services.AddScoped<DeliveryAcceptanceWorkspaceState>();services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new DeliveryAuth(company));services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<DeliveryComponents>();services.AddSingleton<IComponentActivator>(provider=>provider.GetRequiredService<DeliveryComponents>());configure?.Invoke(services);return services.BuildServiceProvider();
    }
    private sealed class Fixture
    {
        public readonly PersonaTaskRoutingTests.Fixture Native=new();
        public readonly REVENUE_TRANSACTION Row=new(){REVENUE_TRANSACTION_ID="revenue-a",PROPERTY_ID="field",FIELD_ID="field",TRANSACTION_DATE=new DateTime(2026,1,31,23,30,0),ACTIVE_IND="Y",CURRENCY_CODE="KWD",GROSS_REVENUE=20,NET_REVENUE=null,DESCRIPTION="Recorded<source>",CONTRACT_ID="contract",OIL_PRICE=null};
        public readonly ProductionAccountingService Service;
        public readonly SALES_CONTRACT Contract=new(){SALES_CONTRACT_ID="contract",SELLER_BA_ID="company",BUYER_BA_ID="buyer",ACTIVE_IND="Y",EFFECTIVE_DATE=new(2026,1,1),CURRENCY_CODE="KWD",BASE_PRICE=null,DESCRIPTION="Recorded<terms>"};
        public int ContractReads;
        public bool ContractMissing,ContractDuplicate,ContractChanged;
        public bool MissingTermsSchema;
        public bool MissingIntentSchema;
        public bool MissingDeliveryIntentSchema;
        public bool MissingDeliverySchema;
        public bool WrongDeliveryKey;
        public Action? AfterIntentWrite;
        public Action? AfterContractWrite;
        public bool Fail,Duplicate,Malformed;
        public int Reads;
        public Fixture()
        {
            Native.Source.Setup(value=>value.GetEntity("DELIVERY_ACCEPTANCE_INTENT",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>(),It.IsAny<int>(),It.IsAny<int>())).Returns((string table,List<TheTechIdea.Beep.Report.AppFilter> filters,int page,int size)=>{
                var entries=Native.Rows.OfType<DELIVERY_ACCEPTANCE_INTENT>().Where(row=>filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).OrderBy(row=>row.SCOPE_KEY,StringComparer.Ordinal).Select(CompanyInventorySourceReview.Copy).ToArray();
                return new PagedResult(entries.Skip((page-1)*size).Take(size).ToArray(),page,size,entries.Length);
            });
            Native.Source.Setup(value=>value.GetEntity("COMMERCIAL_TERMS_INTENT",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>(),It.IsAny<int>(),It.IsAny<int>())).Returns((string table,List<TheTechIdea.Beep.Report.AppFilter> filters,int page,int size)=>{
                var entries=Native.Rows.OfType<COMMERCIAL_TERMS_INTENT>().Where(row=>filters.All(filter=>row.GetType().GetProperty(filter.FieldName)?.GetValue(row)?.ToString()==filter.FilterValue)).OrderBy(row=>row.PROCESS_INSTANCE_ID,StringComparer.Ordinal).Select(CompanyInventorySourceReview.Copy).ToArray();
                return new PagedResult(entries.Skip((page-1)*size).Take(size).ToArray(),page,size,entries.Length);
            });
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(It.IsAny<string>())).ReturnsAsync((string table)=>new List<string>{table=="RUN_TICKET" && WrongDeliveryKey?"TICKET_NUMBER":table+"_ID"});
            Native.Source.SetupGet(value=>value.ErrorObject).Returns(new ErrorsInfo{Flag=Errors.Ok});
            Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure entity,bool refresh)=>{
                var schema=new ClassCreator(Native.Editor.Object).ConvertToEntityStructure(entity.EntityName=="RUN_TICKET"?typeof(RUN_TICKET):entity.EntityName=="DELIVERY_ACCEPTANCE_INTENT"?typeof(DELIVERY_ACCEPTANCE_INTENT):entity.EntityName=="COMMERCIAL_TERMS_INTENT"?typeof(COMMERCIAL_TERMS_INTENT):entity.EntityName=="SALES_CONTRACT"?typeof(SALES_CONTRACT):typeof(REVENUE_TRANSACTION));
                if(entity.EntityName=="DELIVERY_ACCEPTANCE_INTENT" && MissingDeliveryIntentSchema)schema.Fields.RemoveAll(field=>field.FieldName=="COMMAND_JSON");
                if(entity.EntityName=="RUN_TICKET"){var column=schema.Fields.Single(value=>value.FieldName=="DELIVERY_WORKFLOW_JSON");column.AllowDBNull=true;column.Size=MissingDeliverySchema?10:RevenueDeliveryAcceptanceProof.MaximumLength;}
                if(entity.EntityName=="COMMERCIAL_TERMS_INTENT"){foreach(var field in schema.Fields.Where(field=>field.IsKey || field.FieldName is "OWNER_TENANT_ID" or "OWNER_COMPANY_BA_ID" or "PROCESS_INSTANCE_ID" or "SALES_CONTRACT_ID" or "COMMAND_JSON" or "RECORDED_REVISION"))field.AllowDBNull=false;if(MissingIntentSchema)schema.Fields.RemoveAll(field=>field.FieldName=="COMMAND_JSON");}
                if(entity.EntityName=="SALES_CONTRACT"){var column=schema.Fields.Single(value=>value.FieldName=="TERMS_WORKFLOW_JSON");column.AllowDBNull=true;column.Size=MissingTermsSchema?10:SalesContractTermsNativeProof.MaximumLength;}return schema;
            });
            Native.Source.Setup(value=>value.InsertEntity("COMMERCIAL_TERMS_INTENT",It.IsAny<object>())).Returns((string table,object value)=>{
                ++Native.Writes;if(Native.Writes==Native.FailWrite)return new ErrorsInfo{Flag=Errors.Failed};Native.Rows.Add(CompanyInventorySourceReview.Copy((COMMERCIAL_TERMS_INTENT)value));AfterIntentWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};
            });
            Native.Source.Setup(value=>value.InsertEntity("DELIVERY_ACCEPTANCE_INTENT",It.IsAny<object>())).Returns((string table,object value)=>{
                ++Native.Writes;if(Native.Writes==Native.FailWrite)return new ErrorsInfo{Flag=Errors.Failed};Native.Rows.Add(CompanyInventorySourceReview.Copy((DELIVERY_ACCEPTANCE_INTENT)value));AfterIntentWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};
            });
            Native.Source.Setup(value=>value.GetEntityAsync("SALES_CONTRACT",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                ++ContractReads;Assert.Contains(filters,value=>value.FieldName=="SELLER_BA_ID" && value.FilterValue=="company");
                Assert.Contains(filters,value=>value.FieldName=="SALES_CONTRACT_ID" && value.FilterValue=="contract");
                if(ContractChanged && ContractReads>1)Contract.BASE_PRICE=50;
                var row=Native.Rows.OfType<SALES_CONTRACT>().Single();
                return ContractMissing?Array.Empty<object>():ContractDuplicate?new object[]{row,row}:new object[]{CompanyInventorySourceReview.Copy(row)};
            });
            Native.Source.Setup(value=>value.UpdateEntity("SALES_CONTRACT",It.IsAny<object>())).Returns((string table,object value)=>{
                ++Native.Writes;if(Native.FailWrite==Native.Writes)return new ErrorsInfo{Flag=Errors.Failed};
                var row=CompanyInventorySourceReview.Copy((SALES_CONTRACT)value);if(Native.CorruptReadback)row.DESCRIPTION="corrupted";
                Native.Rows.RemoveAll(value=>value is SALES_CONTRACT);Native.Rows.Add(row);AfterContractWrite?.Invoke();return new ErrorsInfo{Flag=Errors.Ok};
            });
            Native.Source.Setup(value=>value.GetEntityAsync("REVENUE_TRANSACTION",It.IsAny<List<AppFilter>>())).ReturnsAsync((string table,List<AppFilter> filters)=>{
                ++Reads;if(Fail)throw new InvalidOperationException("private provider detail");
                Assert.Contains(filters,value=>value.FieldName=="TRANSACTION_DATE" && value.Operator=="<" && value.FilterValue=="2026-02-01");
                if(Malformed)return new object[]{new object()};
                var column=filters.Single(value=>value.FieldName is "FIELD_ID" or "PROPERTY_ID");
                var result=Native.Rows.OfType<REVENUE_TRANSACTION>().Where(row=>row.GetType().GetProperty(column.FieldName)!.GetValue(row)?.ToString()==column.FilterValue).Select(CompanyInventorySourceReview.Copy).Cast<object>().ToList();
                if(Duplicate && result.Count>0)result.Add(result[0]);return result.ToArray();
            });
            Native.Rows.Add(Row);Native.Rows.Add(Contract);Service=new(Native.Editor.Object,new CommonColumnHandler(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        }
    }
    [Theory][InlineData("valid")][InlineData("property-only")][InlineData("field-only")][InlineData("foreign")][InlineData("date")][InlineData("duplicate")][InlineData("malformed")][InlineData("provider")]
    public async Task NativeRecordedSourcePreservesUnknownValuesAndCompleteDualMembership(string mode)
    {
        var f=new Fixture();if(mode=="property-only")f.Row.FIELD_ID="";if(mode=="field-only")f.Row.PROPERTY_ID="";
        if(mode=="foreign")f.Row.FIELD_ID="other";if(mode=="date")f.Row.TRANSACTION_DATE=From.AddDays(-1);f.Duplicate=mode=="duplicate";f.Malformed=mode=="malformed";f.Fail=mode=="provider";
        if(mode is "valid" or "property-only" or "field-only")
        {
            var rows=await f.Service.GetRecordedFieldRevenueAsync("field",From,Through,f.Native.Connection);var row=Assert.Single(rows);Assert.Null(row.NET_REVENUE);Assert.Null(row.OIL_PRICE);Assert.Equal("KWD",row.CURRENCY_CODE);Assert.Equal(23,row.TRANSACTION_DATE!.Value.Hour);Assert.Equal(2,f.Reads);
            var review=FieldRevenueSourceReview.Create(new("tenant","company"),"actor","field",From,Through,"binding",rows);row.GROSS_REVENUE=999;Assert.Equal(20m,review.Rows.Single().GROSS_REVENUE);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.GetRecordedFieldRevenueAsync("field",From,Through,f.Native.Connection));
        Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("valid")][InlineData("role")][InlineData("grant")][InlineData("asset")][InlineData("closing")][InlineData("drift")]
    public async Task CurrentCompanyRolesFieldAndClosingEvidenceGovernRevenueSource(string mode)
    {
        var f=new Fixture();var context=new Mock<ICompanyAccountingReadContext>();var captures=0;
        context.Setup(value=>value.CaptureAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(()=>{
            if(++captures==2 && mode=="drift")f.Row.GROSS_REVENUE=25;
            return(new RepositoryCompanyAccess(new("tenant","company"),[mode=="role"?"Other":"ConfiguredCompanyRole"],mode=="grant"?[]:["Accounting.View"]),"binding","actor");
        });
        var assets=new Mock<IAccessControlService>();var assetReads=0;assets.Setup(value=>value.CheckAssetAccessAsync("actor","field","FIELD","READ",null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=mode!="asset" && (mode!="closing" || ++assetReads<3)});
        var reads=new CompanyFieldRevenueReads(f.Service,context.Object,assets.Object,()=>Task.FromResult(f.Native.Connection));
        var user=new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"actor"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"app"));
        if(mode=="valid"){var result=await reads.GetAsync(user,"field",From,Through);result.Require(new("tenant","company"),"actor","field",From,Through);Assert.Single(result.Rows);}
        else await Assert.ThrowsAnyAsync<Exception>(()=>reads.GetAsync(user,"field",From,Through));
        if(mode is "role" or "grant" or "asset")Assert.Equal(0,f.Reads);Assert.Equal(0,f.Native.Writes);
    }
    private sealed class Auth:AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"actor"),new(ClaimTypes.Role,"ConfiguredCompanyRole")],"app"))));}
    [Theory][InlineData("valid")][InlineData("schema")][InlineData("authority")][InlineData("revoke")][InlineData("write")]
    [InlineData("readback")][InlineData("borrowed")][InlineData("isolation")][InlineData("source")][InlineData("binding")]
    public async Task NativeCommercialPreparationRequiresOwnedTransactionSchemaFreshSourceAndDurableAudit(string mode)
    {
        var f=new Fixture();f.Contract.ROW_CREATED_BY="contract-maker";f.Row.ROW_CREATED_BY="production-maker";var revoked=false;
        async Task<RevenueSalesContractReview> Read()=>RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(new("tenant","company"),"preparer","field",From,Through,"binding",await f.Service.GetRecordedFieldRevenueAsync("field",From,Through,f.Native.Connection)),"revenue-a",await f.Service.GetRecordedSalesContractAsync("contract",new("tenant","company"),f.Native.Connection));
        var original=await Read();var preparation=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),original.Source.Company,"preparer",original,CommercialPreparation().Terms);
        f.MissingTermsSchema=mode=="schema";f.Native.FailWrite=mode=="write"?1:0;f.Native.CorruptReadback=mode=="readback";f.Native.Borrowed=mode=="borrowed";
        if(mode=="isolation")f.Native.Isolation=System.Data.IsolationLevel.ReadCommitted;if(mode=="source")f.Row.GROSS_REVENUE=99;
        f.AfterContractWrite=()=>{if(mode=="revoke")revoked=true;if(mode=="binding")f.Native.Connection="changed";};
        Task Guard(){if(revoked || mode=="authority" || f.Native.Connection!="lifecycle")throw new UnauthorizedAccessException();return Task.CompletedTask;}
        if(mode=="valid")
        {
            var result=await f.Service.PrepareSalesContractTermsAsync(preparation,"lifecycle",Read,Guard);result.Require();Assert.Null(result.Decision);Assert.Equal(1,f.Native.Writes);
            var stored=f.Native.Rows.OfType<SALES_CONTRACT>().Single();Assert.Equal("contract-maker",stored.ROW_CREATED_BY);Assert.Equal("preparer",stored.ROW_CHANGED_BY);Assert.Null(stored.BASE_PRICE);
            Assert.Equal(result.Revision,SalesContractTermsNativeProof.Require(preparation.Company,stored).Record.Revision);
            Assert.Equal(result.Revision,(await f.Service.PrepareSalesContractTermsAsync(preparation,"lifecycle",Read,Guard)).Revision);Assert.Equal(1,f.Native.Writes);
            var replacement=preparation with{IntentId=Guid.NewGuid().ToString("N")};await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.PrepareSalesContractTermsAsync(replacement,"lifecycle",Read,Guard));Assert.Equal(1,f.Native.Writes);
        }
        else
        {
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.PrepareSalesContractTermsAsync(preparation,"lifecycle",Read,Guard));
            Assert.Null(f.Native.Rows.OfType<SALES_CONTRACT>().Single().TERMS_WORKFLOW_JSON);
            if(mode is "schema" or "authority" or "borrowed" or "isolation" or "source")Assert.Equal(0,f.Native.Writes);
        }
    }
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("uncertain")][InlineData("changed")][InlineData("tampered")]
    public async Task NativeCommercialDecisionRetainsPreparationAndReconcilesExactIntent(string mode)
    {
        var f=new Fixture();f.Contract.ROW_CREATED_BY="contract-maker";f.Row.ROW_CREATED_BY="production-maker";var actor="preparer";
        async Task<RevenueSalesContractReview> Read()=>RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(new("tenant","company"),actor,"field",From,Through,"binding",await f.Service.GetRecordedFieldRevenueAsync("field",From,Through,f.Native.Connection)),"revenue-a",await f.Service.GetRecordedSalesContractAsync("contract",new("tenant","company"),f.Native.Connection));
        var original=await Read();var intent=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),original.Source.Company,actor,original,CommercialPreparation().Terms);
        var prepared=await f.Service.PrepareSalesContractTermsAsync(intent,f.Native.Connection,Read,()=>Task.CompletedTask);actor="reviewer";
        var command=new SalesContractTermsDecisionCommand(Guid.NewGuid().ToString("N"),intent.Company,actor,prepared.Revision,mode=="reject"?SalesContractTermsDecision.Reject:SalesContractTermsDecision.Approve,"Independent reviewed decision");
        var native=f.Native.Rows.OfType<SALES_CONTRACT>().Single();if(mode=="changed")native.BUYER_BA_ID="other";if(mode=="tampered")native.TERMS_WORKFLOW_JSON=native.TERMS_WORKFLOW_JSON!.Replace("Proposed terms","Altered terms");
        if(mode is "changed" or "tampered")
        {await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.DecideSalesContractTermsAsync(prepared,command,f.Native.Connection,Read,()=>Task.CompletedTask));Assert.Equal(1,f.Native.Writes);return;}
        f.Native.FailCommit=mode=="uncertain";
        if(mode=="uncertain"){await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.DecideSalesContractTermsAsync(prepared,command,f.Native.Connection,Read,()=>Task.CompletedTask));Assert.Equal(2,f.Native.Writes);f.Native.FailCommit=false;}
        var decided=await f.Service.DecideSalesContractTermsAsync(prepared,command,f.Native.Connection,Read,()=>Task.CompletedTask);decided.Require();Assert.Equal(command,decided.Decision);Assert.Equal(prepared.PreparedAtUtc,decided.PreparedAtUtc);Assert.Equal(2,f.Native.Writes);
        var retry=await f.Service.DecideSalesContractTermsAsync(prepared,command,f.Native.Connection,Read,()=>Task.CompletedTask);Assert.Equal(decided.Revision,retry.Revision);Assert.Equal(2,f.Native.Writes);
        var stored=f.Native.Rows.OfType<SALES_CONTRACT>().Single();Assert.Equal("contract-maker",stored.ROW_CREATED_BY);Assert.Equal("reviewer",stored.ROW_CHANGED_BY);Assert.Null(stored.BASE_PRICE);
        Assert.Equal(command,SalesContractTermsNativeProof.Require(intent.Company,stored).Record.Decision);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Service.DecideSalesContractTermsAsync(prepared,command with{IntentId=Guid.NewGuid().ToString("N")},f.Native.Connection,Read,()=>Task.CompletedTask));Assert.Equal(2,f.Native.Writes);
    }
    [Theory]
    [InlineData("allowed")][InlineData("escaped")][InlineData("anonymous")][InlineData("role")][InlineData("grant")]
    [InlineData("asset")][InlineData("closing")][InlineData("binding")][InlineData("drift")][InlineData("provider")][InlineData("window")]
    public async Task RealHttpAndTypedClientRequireCurrentCompanyFieldAuthorityAndCompleteSource(string mode)
    {
        var f=new Fixture();var field=mode=="escaped"?"field/α%2F":"field";f.Row.FIELD_ID=field;f.Row.PROPERTY_ID=field;f.Fail=mode=="provider";
        var assets=new Mock<IAccessControlService>();var checks=0;
        assets.Setup(value=>value.CheckAssetAccessAsync("actor",field,"FIELD","READ",null)).ReturnsAsync(()=>new AccessCheckResponse{HasAccess=mode!="asset" && (mode!="closing" || ++checks<3)});
        await using var host=await AccountingSourceHttpTests.Start(authenticate:mode!="anonymous",role:mode!="role",permissions:mode=="grant"?[]:["Accounting.View"],configure:(services,context)=>{
            context.ChangeBinding=mode=="binding";
            context.OnCapture=count=>{if(count==2 && mode=="drift")f.Row.GROSS_REVENUE=25;};
            services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection)));
        });
        if(mode is "allowed" or "escaped")
        {
            using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View"]));
            var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
            var review=await client.GetFieldRevenueSourceAsync(field,From,Through);review.Require(new("tenant","company"),"actor",field,From,Through);
            Assert.Equal("KWD",Assert.Single(review.Rows).CURRENCY_CODE);Assert.Null(review.Rows.Single().NET_REVENUE);Assert.Equal(4,f.Reads);
        }
        else
        {
            var query=mode=="window"?"from=2026-01-31&through=2026-01-01":"from=2026-01-01&through=2026-01-31";
            var response=await host.Client.GetAsync("/api/fields/"+Uri.EscapeDataString(field)+"/accounting/revenue-source?"+query);
            var expected=mode=="anonymous"?HttpStatusCode.Unauthorized:mode=="window"?HttpStatusCode.BadRequest:mode is "provider" or "drift"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden;
            Assert.Equal(expected,response.StatusCode);Assert.DoesNotContain("private provider detail",await response.Content.ReadAsStringAsync());
            if(mode is "anonymous" or "role" or "grant" or "asset" or "window")Assert.Equal(0,f.Reads);
        }
        Assert.Equal(0,f.Native.Writes);
    }
    private sealed class Reply(Func<HttpResponseMessage> reply):HttpMessageHandler
    {protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)=>Task.FromResult(reply());}
    [Theory][InlineData("approve")][InlineData("reject")][InlineData("uncertain")][InlineData("escaped")]
    [InlineData("state")]
    [InlineData("read-schema")]
    [InlineData("anonymous")][InlineData("role")][InlineData("grant")][InlineData("asset")][InlineData("actor")][InlineData("company")]
    [InlineData("route")][InlineData("source")][InlineData("revoke")][InlineData("approval-grant")][InlineData("same-maker")]
    public async Task CommercialTermsRealHttpTypedClientJourneyEnforcesSeparateGrantsAndOriginalCompanyEvidence(string mode)
    {
        var f=new Fixture();var field=mode=="escaped"?"field/α%2F":"field";f.Row.FIELD_ID=field;f.Row.PROPERTY_ID=field;f.Row.ROW_CREATED_BY="production-maker";f.Contract.ROW_CREATED_BY="contract-maker";
        var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),field,"FIELD",It.IsAny<string>(),null)).ReturnsAsync((string actor,string id,string type,string level,string? connection)=>new AccessCheckResponse{HasAccess=level!="WRITE" || mode!="asset"});
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve"],configure:(services,context)=>services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection))));
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Adjust","ProductionAccounting.Approve"]));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(mode=="read-schema"){f.MissingTermsSchema=true;await Assert.ThrowsAnyAsync<Exception>(()=>client.GetSalesContractTermsAsync(field,From,Through,"revenue-a"));Assert.Equal(0,f.Native.Writes);return;}
        var review=await client.GetSalesContractTermsAsync(field,From,Through,"revenue-a");Assert.Null(review.Recorded);
        var intent=new SalesContractTermsPreparation(Guid.NewGuid().ToString("N"),review.Source.Source.Company,"actor",review.Source,CommercialPreparation().Terms);
        if(mode=="anonymous")host.Client.DefaultRequestHeaders.Remove("X-Test-Auth");if(mode=="role")host.Client.DefaultRequestHeaders.Remove("X-Test-Role");if(mode=="grant")host.Context.Permissions=["Accounting.View"];
        if(mode=="actor")intent=intent with{ActorUserId="forged",Source=RevenueSalesContractReview.Create(FieldRevenueSourceReview.Create(intent.Company,"forged",field,From,Through,"binding",intent.Source.Source.Rows),"revenue-a",intent.Source.Contract)};
        if(mode=="company")host.Client.DefaultRequestHeaders.Add("X-Test-Company","foreign");if(mode=="source")f.Row.GROSS_REVENUE=999;
        if(mode=="revoke")f.AfterContractWrite=()=>host.Context.Permissions=[];
        if(mode is "anonymous" or "role" or "grant" or "asset" or "actor" or "company" or "route" or "source" or "revoke")
        {
            var route="api/fields/"+Uri.EscapeDataString(mode=="route"?"other":field)+"/accounting/commercial-terms/prepare";
            var response=await host.Client.PostAsJsonAsync(route,intent);
            Assert.Equal(mode=="anonymous"?HttpStatusCode.Unauthorized:mode=="route"?HttpStatusCode.BadRequest:mode=="source"?HttpStatusCode.Conflict:HttpStatusCode.Forbidden,response.StatusCode);
            Assert.Null(f.Native.Rows.OfType<SALES_CONTRACT>().Single().TERMS_WORKFLOW_JSON);if(mode!="revoke")Assert.Equal(0,f.Native.Writes);return;
        }
        using var workspace=mode=="state"?new CommercialTermsWorkspaceState(company):null;
        SalesContractTermsRecord prepared;
        if(workspace is not null){workspace.Attach(field,From,Through,"revenue-a");await workspace.LoadAsync(client);await workspace.PrepareAsync(client,intent.Terms);Assert.False(workspace.HasPendingIntent);prepared=workspace.Review!.Recorded!;intent=prepared.Preparation;}
        else prepared=await client.PrepareSalesContractTermsAsync(intent);
        Assert.Equal(1,f.Native.Writes);
        Assert.Equal(prepared.Revision,(await client.PrepareSalesContractTermsAsync(intent)).Revision);Assert.Equal(1,f.Native.Writes);
        var readback=await client.GetSalesContractTermsAsync(field,From,Through,"revenue-a");Assert.Equal(prepared.Revision,readback.Recorded!.Revision);
        var reviewer=mode=="same-maker"?"actor":"reviewer";host.Context.Actor=reviewer;host.Client.DefaultRequestHeaders.Add("X-Test-Actor",reviewer);
        company.TrySelect(company.Clear(),reviewer,new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View","ProductionAccounting.Approve"]));
        if(mode=="approval-grant")host.Context.Permissions=["Accounting.View","ProductionAccounting.Adjust"];
        var command=new SalesContractTermsDecisionCommand(Guid.NewGuid().ToString("N"),intent.Company,reviewer,prepared.Revision,mode=="reject"?SalesContractTermsDecision.Reject:SalesContractTermsDecision.Approve,"Independent commercial approval review");
        var submission=new SalesContractTermsDecisionSubmission(prepared,command);
        if(mode is "approval-grant" or "same-maker")
        {var denied=await host.Client.PostAsJsonAsync("api/fields/"+Uri.EscapeDataString(field)+"/accounting/commercial-terms/decide",submission);Assert.Equal(mode=="approval-grant"?HttpStatusCode.Forbidden:HttpStatusCode.Conflict,denied.StatusCode);Assert.Equal(1,f.Native.Writes);return;}
        f.Native.FailCommit=mode=="uncertain";
        if(mode=="uncertain"){await Assert.ThrowsAnyAsync<Exception>(()=>client.DecideSalesContractTermsAsync(submission));Assert.Equal(2,f.Native.Writes);f.Native.FailCommit=false;}
        SalesContractTermsRecord decided;
        if(workspace is not null){await workspace.LoadAsync(client);await workspace.DecideAsync(client,SalesContractTermsDecision.Approve,"Independent commercial approval review");Assert.False(workspace.HasPendingIntent);decided=workspace.Review!.Recorded!;command=decided.Decision!;submission=new(prepared,command);}
        else decided=await client.DecideSalesContractTermsAsync(submission);
        Assert.Equal(command,decided.Decision);Assert.Equal(2,f.Native.Writes);
        Assert.Equal(decided.Revision,(await client.DecideSalesContractTermsAsync(submission)).Revision);Assert.Equal(2,f.Native.Writes);
        var recorded=(await client.GetSalesContractTermsAsync(field,From,Through,"revenue-a")).Recorded!;Assert.Equal(decided.Revision,recorded.Revision);Assert.Equal(command,recorded.Decision);
        Assert.Equal("actor",recorded.Preparation.ActorUserId);Assert.Equal(prepared.PreparedAtUtc,recorded.PreparedAtUtc);Assert.Null(f.Native.Rows.OfType<SALES_CONTRACT>().Single().BASE_PRICE);
    }
    [Theory][InlineData("valid")][InlineData("foreign")][InlineData("missing")][InlineData("duplicate")][InlineData("inactive")]
    [InlineData("changed")][InlineData("stale")][InlineData("transaction")][InlineData("reference")][InlineData("revoke")]
    public async Task RevenueContractHttpReviewPreservesExactReferenceAndNativeSellerOwnership(string mode)
    {
        var f=new Fixture();var assets=new Mock<IAccessControlService>();assets.Setup(value=>value.CheckAssetAccessAsync("actor","field","FIELD","READ",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=true});
        await using var host=await AccountingSourceHttpTests.Start(permissions:["Accounting.View"],configure:(services,context)=>services.AddSingleton(new CompanyFieldRevenueReads(f.Service,context,assets.Object,()=>Task.FromResult(f.Native.Connection))));
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View"]));
        var client=new AccountingServiceClient(new ApiClient(host.Client,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        var source=await client.GetFieldRevenueSourceAsync("field",From,Through);
        if(mode=="foreign")f.Contract.SELLER_BA_ID="other";if(mode=="inactive")f.Contract.ACTIVE_IND="N";
        f.ContractMissing=mode=="missing";f.ContractDuplicate=mode=="duplicate";f.ContractChanged=mode=="changed";
        if(mode=="stale")f.Row.GROSS_REVENUE=25;if(mode=="reference")f.Row.CONTRACT_ID="";
        if(mode=="revoke")host.Context.OnCapture=count=>{if(count>=6)host.Context.Permissions=[];};
        if(mode=="valid")
        {
            var result=await client.GetRevenueSalesContractAsync(source,"revenue-a");result.Require(source.Company,source.ActorUserId,"field",From,Through,"revenue-a",source.Revision);
            Assert.Equal("contract",result.Contract.SALES_CONTRACT_ID);Assert.Equal("buyer",result.Contract.BUYER_BA_ID);Assert.Null(result.Contract.BASE_PRICE);Assert.Equal(From,result.Contract.EFFECTIVE_DATE);Assert.Null(result.Contract.EXPIRY_DATE);Assert.Equal(2,f.ContractReads);
            f.Contract.DESCRIPTION="mutated";Assert.Equal("Recorded<terms>",result.Contract.DESCRIPTION);
        }
        else await Assert.ThrowsAnyAsync<Exception>(()=>client.GetRevenueSalesContractAsync(source,mode=="transaction"?"other":"revenue-a"));
        if(mode is "stale" or "reference" or "transaction")Assert.Equal(0,f.ContractReads);Assert.Equal(0,f.Native.Writes);
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("field")][InlineData("window")]
    [InlineData("row")][InlineData("revision")][InlineData("late")][InlineData("null")]
    public async Task TypedClientRefusesForeignMalformedAndLateRecordedSource(string mode)
    {
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View"]));
        var review=FieldRevenueSourceReview.Create(new("tenant","company"),"actor","field",From,Through,"binding",[new(){REVENUE_TRANSACTION_ID="rev",FIELD_ID="field",ACTIVE_IND="Y",TRANSACTION_DATE=From}]);
        if(mode=="company")review=review with{Company=new("tenant","foreign")};if(mode=="actor")review=review with{ActorUserId="other"};
        if(mode=="field")review=review with{FieldId="other"};if(mode=="window")review=review with{Through=Through.AddDays(1)};
        if(mode=="row")review.Rows.Single().GROSS_REVENUE=99;if(mode=="revision")review=review with{Revision="altered"};
        using var http=new HttpClient(new Reply(()=>{if(mode=="late")company.Clear();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:review)};})){BaseAddress=new Uri("https://fixture/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        await Assert.ThrowsAnyAsync<Exception>(()=>client.GetFieldRevenueSourceAsync("field",From,Through));
    }
    [Theory][InlineData(false,"null")][InlineData(false,"foreign")][InlineData(false,"changed")][InlineData(false,"late")]
    [InlineData(true,"null")][InlineData(true,"foreign")][InlineData(true,"changed")][InlineData(true,"late")]
    public async Task CommercialTypedCommandsRefuseDifferentRecordedIntentAndLateContext(bool decision,string mode)
    {
        var intent=CommercialPreparation();var at=new DateTime(2026,2,1,0,0,0,DateTimeKind.Utc);var prepared=SalesContractTermsRecord.Create(intent,at);
        var command=new SalesContractTermsDecisionCommand(Guid.NewGuid().ToString("N"),intent.Company,"reviewer",prepared.Revision,SalesContractTermsDecision.Approve,"Reviewed source and explicit terms");
        var submission=new SalesContractTermsDecisionSubmission(prepared,command);
        var result=decision?prepared.Decide(command,intent.Source,at.AddMinutes(5)):prepared;
        if(mode=="foreign")
        {
            var other=intent with{IntentId=Guid.NewGuid().ToString("N")};var otherRecord=SalesContractTermsRecord.Create(other,at);
            result=decision?otherRecord.Decide(command with{PreparationRevision=otherRecord.Revision},other.Source,at.AddMinutes(5)):otherRecord;
        }
        if(mode=="changed")result=decision?prepared.Decide(command with{IntentId=Guid.NewGuid().ToString("N")},intent.Source,at.AddMinutes(5)):SalesContractTermsRecord.Create(intent with{Terms=intent.Terms with{Price=99}},at);
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),decision?"reviewer":"commercial-preparer",new(intent.Company,["ConfiguredCompanyRole"],["Accounting.View"]));
        using var http=new HttpClient(new Reply(()=>{if(mode=="late")company.Clear();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="null"?null:result)};})){BaseAddress=new Uri("https://fixture/")};
        var client=new AccountingServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<AccountingServiceClient>.Instance);
        if(decision)await Assert.ThrowsAnyAsync<Exception>(()=>client.DecideSalesContractTermsAsync(submission));else await Assert.ThrowsAnyAsync<Exception>(()=>client.PrepareSalesContractTermsAsync(intent));
        Assert.Null(prepared.Decision);Assert.Equal(12.345m,intent.Terms.Price);
    }
    private sealed class Navigation:NavigationManager{public Navigation()=>Initialize("https://fixture/","https://fixture/ppdm39/accounting/revenue-recognition");protected override void NavigateToCore(string uri,bool forceLoad){}}
    private sealed class Components(IServiceProvider services):IComponentActivator
    {public RevenueRecognition? Page;public IComponent CreateInstance(Type type){var value=(IComponent)ActivatorUtilities.CreateInstance(services,type);if(value is RevenueRecognition page)Page=page;return value;}}
    [Theory][InlineData("records")][InlineData("empty")][InlineData("failure")][InlineData("denied")][InlineData("filter")][InlineData("late-company")]
    [InlineData("contract")][InlineData("contract-failure")][InlineData("contract-late")]
    [InlineData("terms-open")]
    [InlineData("acceptance-open")]
    [InlineData("delivery")][InlineData("delivery-missing")][InlineData("delivery-failure")][InlineData("delivery-late")]
    public async Task RenderedRevenueSourceNeverReplacesUnknownOrFailedEvidenceWithZeroMetrics(string mode)
    {
        using var company=new CompanyClientContext();company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],mode=="denied"?[]:["Accounting.View"]));
        var api=new Mock<IAccountingServiceClient>();var pending=new TaskCompletionSource<FieldRevenueSourceReview>();
        api.Setup(value=>value.GetSalesContractTermsAsync("field",It.IsAny<DateTime>(),It.IsAny<DateTime>(),"rev-kwd")).Returns(async(string field,DateTime from,DateTime through,string transaction)=>new SalesContractTermsReadReview(RevenueSalesContractReview.Create(await api.Object.GetFieldRevenueSourceAsync(field,from,through),transaction,new(){SALES_CONTRACT_ID="contract",SELLER_BA_ID="company",ACTIVE_IND="Y"}),null));
        var contractPending=new TaskCompletionSource<RevenueSalesContractReview>();
        api.Setup(value=>value.GetDeliverySavedReviewAsync("field",It.IsAny<DateTime>(),It.IsAny<DateTime>(),"rev-kwd")).Returns(async(string field,DateTime from,DateTime through,string transaction)=>{
            var source=await api.Object.GetFieldRevenueSourceAsync(field,from,through);var contract=RevenueSalesContractReview.Create(source,transaction,new(){SALES_CONTRACT_ID="contract",SELLER_BA_ID="company",BUYER_BA_ID="buyer",ACTIVE_IND="Y"});
            return new RevenueDeliveryAcceptanceSavedReview(new(RevenueDeliverySourceReview.Create(contract,new(){ALLOCATION_RESULT_ID="allocation",ALLOCATION_REQUEST_ID="ticket",ACTIVE_IND="Y"},new(){RUN_TICKET_ID="ticket",FIELD_ID=field,ACTIVE_IND="Y",NET_VOLUME=7,VOLUME_OUOM="BBL",TICKET_STATUS="CERTIFIED",TICKET_DATE_TIME=from}),null),[]);
        });
        var deliveryPending=new TaskCompletionSource<RevenueDeliverySourceReview>();
        api.Setup(value=>value.GetRevenueDeliveryAsync(It.IsAny<FieldRevenueSourceReview>(),"rev-kwd")).Returns((FieldRevenueSourceReview source,string transaction)=>{
            if(mode=="delivery-late")return deliveryPending.Task;if(mode=="delivery-failure")throw new IOException("lost delivery");
            var contract=RevenueSalesContractReview.Create(source,transaction,new(){SALES_CONTRACT_ID="contract",SELLER_BA_ID="company",ACTIVE_IND="Y"});
            return Task.FromResult(RevenueDeliverySourceReview.Create(contract,new ALLOCATION_RESULT{ALLOCATION_RESULT_ID="allocation",ALLOCATION_REQUEST_ID="ticket",ACTIVE_IND="Y"},new RUN_TICKET{RUN_TICKET_ID="ticket",FIELD_ID="field",ACTIVE_IND="Y",NET_VOLUME=mode=="delivery-missing"?null:7,VOLUME_OUOM=mode=="delivery-missing"?null:"BBL",TICKET_STATUS="CERTIFIED",TICKET_DATE_TIME=source.From}));
        });
        api.Setup(value=>value.GetRevenueSalesContractAsync(It.IsAny<FieldRevenueSourceReview>(),"rev-kwd")).Returns((FieldRevenueSourceReview source,string transaction)=>{
            if(mode=="contract-late")return contractPending.Task;if(mode=="contract-failure")throw new IOException("lost contract");
            return Task.FromResult(RevenueSalesContractReview.Create(source,transaction,new(){SALES_CONTRACT_ID="contract",SELLER_BA_ID="company",BUYER_BA_ID="buyer",ACTIVE_IND="Y",CURRENCY_CODE="KWD",DESCRIPTION="Terms<recorded>"}));
        });
        api.Setup(value=>value.GetFieldRevenueSourceAsync("field",It.IsAny<DateTime>(),It.IsAny<DateTime>())).Returns((string field,DateTime from,DateTime through)=>{
            if(mode=="late-company")return pending.Task;if(mode=="failure")throw new IOException("lost");
            var rows=mode=="empty"?Array.Empty<REVENUE_TRANSACTION>():new[]{new REVENUE_TRANSACTION{REVENUE_TRANSACTION_ID="rev-kwd",FIELD_ID=field,ACTIVE_IND="Y",TRANSACTION_DATE=from,CURRENCY_CODE="KWD",GROSS_REVENUE=15,NET_REVENUE=null,CONTRACT_ID="contract",ALLOCATION_RESULT_ID="allocation"},new REVENUE_TRANSACTION{REVENUE_TRANSACTION_ID="rev-eur",FIELD_ID=field,ACTIVE_IND="Y",TRANSACTION_DATE=from,CURRENCY_CODE="EUR",GROSS_REVENUE=20}};
            return Task.FromResult(FieldRevenueSourceReview.Create(new("tenant","company"),"actor",field,from,through,"binding",rows));
        });
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton<CommercialTermsWorkspaceState>();services.AddScoped<DeliveryAcceptanceWorkspaceState>();services.AddSingleton(Mock.Of<IBusinessProcessServiceClient>());services.AddSingleton(api.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider,Auth>();services.AddSingleton<NavigationManager,Navigation>();services.AddSingleton<IJSRuntime,APInvoiceFinancialHttpTests.NoBrowser>();services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>{
            var root=renderer.BeginRenderingComponent<RevenueRecognition>();
            if(mode=="late-company"){company.Clear();pending.SetException(new IOException("old"));await root.QuiescenceTask;Assert.DoesNotContain("rev-kwd",root.ToHtmlString());Assert.Contains("Select a company",root.ToHtmlString());return;}
            await root.QuiescenceTask;var page=((Components)provider.GetRequiredService<IComponentActivator>()).Page!;
            if(mode=="acceptance-open")
            {
                typeof(RevenueRecognition).GetMethod("SelectAcceptance",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,["rev-kwd"]);typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);await root.QuiescenceTask;
                Assert.Contains("Delivery acceptance preparation and independent review",root.ToHtmlString());Assert.Contains("Ticket: ticket",root.ToHtmlString());Assert.Contains("independently approved applicable terms are unavailable",root.ToHtmlString());
                api.Verify(value=>value.GetDeliverySavedReviewAsync("field",It.IsAny<DateTime>(),It.IsAny<DateTime>(),"rev-kwd"),Times.Once);
            }
            if(mode.StartsWith("delivery",StringComparison.Ordinal))
            {
                var task=(Task)typeof(RevenueRecognition).GetMethod("ReviewDelivery",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,["rev-kwd"])!;
                if(mode=="delivery-late"){company.Clear();deliveryPending.SetException(new IOException("old delivery"));}
                await task;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);await root.QuiescenceTask;
                var current=root.ToHtmlString();
                if(mode is "delivery" or "delivery-missing")
                {
                    Assert.Contains("Recorded allocation and measured ticket",current);Assert.Contains("Ticket: ticket",current);Assert.Contains("Delivery acceptance, independent approval",current);
                    if(mode=="delivery")Assert.Contains("The recorded ticket is certified",current);else{Assert.Contains("not all available",current);Assert.Contains("Net measured quantity: Unavailable",current);}
                    await (Task)typeof(RevenueRecognition).GetMethod("Reload",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null)!;
                    typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);Assert.DoesNotContain("Ticket: ticket",root.ToHtmlString());
                }
                if(mode=="delivery-failure"){Assert.Contains("measured ticket could not be confirmed",current);Assert.DoesNotContain("Ticket: ticket",current);}
                if(mode=="delivery-late"){Assert.Contains("Select a company",current);Assert.DoesNotContain("Ticket: ticket",current);Assert.DoesNotContain("measured ticket could not be confirmed",current);}
            }
            if(mode=="terms-open")
            {
                typeof(RevenueRecognition).GetMethod("SelectTerms",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,["rev-kwd"]);
                typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);await root.QuiescenceTask;
                Assert.Contains("Commercial terms preparation and independent review",root.ToHtmlString());Assert.Contains("Revenue transaction: rev-kwd",root.ToHtmlString());
                api.Verify(value=>value.GetSalesContractTermsAsync("field",It.IsAny<DateTime>(),It.IsAny<DateTime>(),"rev-kwd"),Times.Once);
            }
            if(mode.StartsWith("contract",StringComparison.Ordinal))
            {
                var reviewTask=(Task)typeof(RevenueRecognition).GetMethod("ReviewContract",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,["rev-kwd"])!;
                if(mode=="contract-late"){company.Clear();contractPending.SetException(new IOException("old contract"));}
                await reviewTask;typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);
                var current=root.ToHtmlString();
                if(mode=="contract"){Assert.Contains("Recorded sales contract contract",current);Assert.Contains("Terms&lt;recorded&gt;",current);Assert.Contains("Buyer: buyer",current);Assert.Contains("Approval, price units",current);}
                if(mode=="contract-failure"){Assert.Contains("sales contract could not be confirmed",current);Assert.DoesNotContain("Recorded sales contract contract",current);}
                if(mode=="contract-late"){Assert.Contains("Select a company",current);Assert.DoesNotContain("Terms&lt;recorded&gt;",current);Assert.DoesNotContain("sales contract could not be confirmed",current);}
            }
            if(mode=="filter"){typeof(RevenueRecognition).GetMethod("SelectCurrency",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,["KWD",true]);typeof(ComponentBase).GetMethod("StateHasChanged",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(page,null);Assert.Contains("rev-kwd",root.ToHtmlString());Assert.DoesNotContain("rev-eur",root.ToHtmlString());}
            var html=root.ToHtmlString();Assert.DoesNotContain("Recalculate",html);Assert.DoesNotContain("Total Revenue (YTD)",html);
            if(mode is "records" or "filter"){Assert.Contains("Unavailable",html);Assert.Contains("KWD",html);Assert.Contains("Pricing unit/basis unavailable",html);Assert.Contains("Retrieved UTC",html);}
            if(mode=="empty")Assert.Contains("No recorded revenue transactions",html);
            if(mode is "failure" or "denied"){Assert.Contains("could not be confirmed",html);Assert.DoesNotContain("All recorded transactions (0)",html);}
        });
    }
}
}
// Focused composition provides constructor dependencies only; the native reader above is the exact production partial.
namespace Beep.OilandGas.ProductionAccounting.Services
{
public partial class ProductionAccountingService(IDMEEditor editor,ICommonColumnHandler columns,IPPDM39DefaultsRepository defaults,IPPDMMetadataRepository metadata)
{private readonly IDMEEditor _editor=editor;private readonly ICommonColumnHandler _commonColumnHandler=columns;private readonly IPPDM39DefaultsRepository _defaults=defaults;private readonly IPPDMMetadataRepository _metadata=metadata;}
}
#endif
