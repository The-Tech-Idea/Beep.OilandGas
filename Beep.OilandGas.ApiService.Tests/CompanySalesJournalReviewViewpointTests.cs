#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
[Collection("CompanySalesJournalSource")]
public sealed class CompanySalesJournalReviewViewpointTests
{
    [Fact] public async Task ActualPeerReadsProduceSameFrozenMakerViewWithoutChangingActualReaderAuthority()
    {
        using var validation=CompanySalesInvoiceValidation.Enter();var fixture=new CompanySalesJournalSourceTests.Fixture();await fixture.Prepare();var maker=await fixture.Read();var actor="independent-reviewer";fixture.Receivable.Actor=actor;var calls=0;
        Task Guard(){calls++;Assert.Equal(actor,fixture.Receivable.Actor);return fixture.Guard();}
        var peer=await fixture.Journal.GetCompanySalesJournalSourceAsync(maker.Company,actor,maker.ReceivableId,maker.BookId,maker.CurrencyCode,maker.CalendarId,maker.PostingDate,maker.Selection,fixture.Receivable.Data.Owner,fixture.Mappings,fixture.Periods,fixture.Receivable.Current,Guard,_=>Guard(),maker.ActorUserId);
        Assert.Equal(JsonSerializer.Serialize(maker),JsonSerializer.Serialize(peer));Assert.True(calls>0);Assert.Equal(actor,fixture.Receivable.Actor);fixture.NoWrites();
    }
}
#endif
