using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class ProductionGlPostingTests
{
    [Theory]
    [InlineData("PRODUCTION", true, true)]
    [InlineData("PRODUCTION", false, true)]
    [InlineData("PRODUCTION", false, false)]
    [InlineData("REVENUE", true, true)]
    [InlineData("REVENUE", false, true)]
    [InlineData("REVENUE", false, false)]
    public async Task PostingUsesBalancedDatedJournalAndRequiresPostSuccess(string source, bool cash, bool posted)
    {
        var journal = new Mock<IJournalEntryService>(MockBehavior.Strict);
        var date = new DateTime(2026, 9, 1);
        List<JOURNAL_ENTRY_LINE>? captured = null;
        journal.Setup(x => x.CreateEntryAsync(date, It.IsAny<string>(), It.IsAny<List<JOURNAL_ENTRY_LINE>>(),
                "local-user", "ticket", source, null))
            .Callback<DateTime, string, List<JOURNAL_ENTRY_LINE>, string, string?, string?, string?>(
                (_, _, lines, _, _, _, _) => captured = lines)
            .ReturnsAsync(new JOURNAL_ENTRY { JOURNAL_ENTRY_ID = "persisted-journal" });
        journal.Setup(x => x.PostEntryAsync("persisted-journal", "local-user")).ReturnsAsync(posted);
        var service = new GLIntegrationService(journal.Object,
            new GLAccountMappingService(journal.Object, NullLogger<GLAccountMappingService>.Instance),
            NullLogger<GLIntegrationService>.Instance);

        Task<string> Post() => source == "PRODUCTION"
            ? service.PostProductionToGL("ticket", 100m, cash, date, "local-user")
            : service.PostRevenueToGL("ticket", 100m, cash, date, "local-user");
        if (posted)
            Assert.Equal("persisted-journal", await Post());
        else
            await Assert.ThrowsAsync<InvalidOperationException>(Post);

        Assert.NotNull(captured);
        Assert.Equal(2, captured.Count);
        Assert.Equal(cash ? "1000" : "1110", captured[0].GL_ACCOUNT_ID);
        Assert.Equal("4001", captured[1].GL_ACCOUNT_ID);
        Assert.Equal(100m, captured.Sum(x => x.DEBIT_AMOUNT ?? 0m));
        Assert.Equal(100m, captured.Sum(x => x.CREDIT_AMOUNT ?? 0m));
        journal.VerifyAll();
    }

    [Theory]
    [InlineData("", 100, "local-user")]
    [InlineData("transaction", 0, "local-user")]
    [InlineData("transaction", -1, "local-user")]
    [InlineData("transaction", 100, " ")]
    public async Task RevenueRejectsInvalidInputsBeforeStorage(string reference, decimal amount, string actor)
    {
        var journal = new Mock<IJournalEntryService>(MockBehavior.Strict);
        var service = new GLIntegrationService(journal.Object,
            new GLAccountMappingService(journal.Object, NullLogger<GLAccountMappingService>.Instance),
            NullLogger<GLIntegrationService>.Instance);

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.PostRevenueToGL(reference, amount, true, new DateTime(2026, 9, 1), actor));

        journal.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("PRODUCTION")]
    [InlineData("REVENUE")]
    public async Task MissingPersistedIdCannotBePosted(string source)
    {
        var journal = new Mock<IJournalEntryService>(MockBehavior.Strict);
        var date = new DateTime(2026, 9, 1);
        journal.Setup(x => x.CreateEntryAsync(date, It.IsAny<string>(), It.IsAny<List<JOURNAL_ENTRY_LINE>>(),
                "local-user", "reference", source, null))
            .ReturnsAsync(new JOURNAL_ENTRY { JOURNAL_ENTRY_ID = " " });
        var service = new GLIntegrationService(journal.Object,
            new GLAccountMappingService(journal.Object, NullLogger<GLAccountMappingService>.Instance),
            NullLogger<GLIntegrationService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => source == "PRODUCTION"
            ? service.PostProductionToGL("reference", 100m, true, date, "local-user")
            : service.PostRevenueToGL("reference", 100m, true, date, "local-user"));

        journal.VerifyAll();
        journal.Verify(x => x.PostEntryAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
