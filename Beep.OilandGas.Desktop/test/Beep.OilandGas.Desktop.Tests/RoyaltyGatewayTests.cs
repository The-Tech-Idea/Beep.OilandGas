using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.ProductionAccounting.Calculations;
using Beep.OilandGas.ProductionAccounting.Constants;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-07 against a real SQLite database: a royalty accrues and pays at the configured accounts and is traceable to its
/// volume (AC-1, AC-6, SG-10-7); a lease with no ownership or a division that does not sum to 100 % is refused by name
/// (AC-2); a disputed royalty cannot be paid and the refusal carries the reference (AC-3); a joint-interest statement nets
/// to its charges less its credits and posts once (AC-4, SG-10-7); and a cost allocation commits exactly the shares the
/// preview showed (AC-5). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class RoyaltyGatewayTests
{
    private const string ConnectionName = "acc07-royalties";

    [IntegrationFact]
    public async Task The_royalty_and_jib_journey_posts_at_the_configured_accounts()
    {        var root = Path.Combine(OgPaths.DataRoot, "acc07-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasAcc07"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "royalties.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "royalties.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgRoyaltiesService(() => source, reporter);
        var context = new OgRoyaltyContext("SMOKE", "MAIN");

        // ---- the workspace ----------------------------------------------------
        var loaded = await service.LoadAsync(context);

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Royalties);
        Assert.Empty(loaded.Value.Jibs);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains("ProRataWorkingInterest", loaded.Value.AllocationMethods);
        Assert.Contains("PROPERTY", loaded.Value.TargetTypes);

        // ---- AC-2: a lease with no ownership fraction is refused by name ------
        var noOwnership = await service.CalculateAsync(Inputs() with { LeaseId = "LSE-9" }, context, "sara");

        Assert.False(noOwnership.Success);
        Assert.Equal(OwnershipFractions.Codes.MissingFraction, noOwnership.Reference);
        Assert.Contains("LSE-9", noOwnership.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "ROYALTY_CALCULATION"));

        // ... and so is one whose division does not reach 100 %.
        var shortDivision = await service.CalculateAsync(Inputs() with { LeaseId = "LSE-3" }, context, "sara");

        Assert.False(shortDivision.Success);
        Assert.Equal(OwnershipFractions.Codes.FractionsDoNotSum, shortDivision.Reference);
        Assert.Contains("totals 60 %", shortDivision.Message!, StringComparison.Ordinal);
        Assert.Equal(0, Count(database, "ROYALTY_CALCULATION"));

        // ---- AC-1 and SG-10-7: the royalty accrues at the configured accounts --
        var calculated = await service.CalculateAsync(Inputs(), context, "sara");

        Assert.True(calculated.Success, calculated.Message + " :: " + string.Join(" | ", reporter.Failures));

        var royalty = calculated.Value!.SelectedRoyalty!;

        // 50 bbl x 70 = 3,500 gross; less 350 deductions = 3,150 net; 12.5 % = 393.75.
        Assert.Equal(3_500m, royalty.GrossValue);
        Assert.Equal(350m, royalty.Deductions.Total);
        Assert.Equal(3_150m, royalty.NetRevenue);
        Assert.Equal(393.75m, royalty.Amount);
        Assert.True(royalty.NetsAsStated);
        Assert.Equal(RoyaltyStatus.Accrued, royalty.Status);
        Assert.True(royalty.CanPay);
        Assert.Equal("USD", royalty.Currency);

        // AC-6: the calculation carries the volume, its unit and the allocation behind it.
        Assert.Contains("volume 50 bbl at 70.00", royalty.TraceText, StringComparison.Ordinal);
        Assert.Contains("allocation AL-1", royalty.TraceText, StringComparison.Ordinal);

        Assert.Equal(1, Count(database,
            "ROYALTY_CALCULATION WHERE ROYALTY_AMOUNT = 393.75 AND NET_REVENUE = 3150 AND GROSS_REVENUE = 3500 " +
            "AND GROSS_VOLUME = 50 AND PRICE_PER_UNIT = 70 AND ROYALTY_STATUS = 'ACCRUED'"));
        Assert.Equal(1, Count(database,
            "ROYALTY_CALCULATION WHERE ALLOCATION_DETAIL_ID = 'AL-1' AND ALLOCATION_RESULT_ID = 'AR-1'"));
        Assert.Equal(1, Count(database, "ROYALTY_DEDUCTIONS WHERE TOTAL_DEDUCTIONS = 350"));

        // The accrual: royalty expense against the accrued liability.
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY WHERE SOURCE_MODULE = 'ROYALTY_JIB' AND TOTAL_DEBIT = 393.75 " +
            "AND REFERENCE_NUMBER LIKE 'ROYALTY-%'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 393.75 AND GL_ACCOUNT_ID = 'A-6100'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 393.75 AND GL_ACCOUNT_ID = 'A-2200'"));

        // ---- the pay command's segregation of duties --------------------------
        var byAuthor = await service.PayAsync(royalty.Id, context, "sara");

        Assert.False(byAuthor.Success);
        Assert.Equal(RoyaltyRules.CodeAuthorCannotAct, byAuthor.Reference);
        Assert.Contains("sara", byAuthor.Message!, StringComparison.Ordinal);

        var paid = await service.PayAsync(royalty.Id, context, "nadia");

        Assert.True(paid.Success, paid.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal(RoyaltyStatus.Paid, paid.Value!.SelectedRoyalty!.Status);
        // ROYALTY_PAYMENT declares STATUS and Status, so the payment is recorded on the calculation instead - see the
        // evidence's engine findings. The journal plus this state is the payment.
        Assert.Equal(1, Count(database,
            $"ROYALTY_CALCULATION WHERE ROYALTY_CALCULATION_ID = '{royalty.Id}' AND PAYMENT_STATUS = 'PAID' " +
            "AND PAID_BY = 'nadia' AND PAID_AMOUNT = 393.75 AND PAYMENT_JOURNAL_ENTRY_ID IS NOT NULL"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 393.75 AND GL_ACCOUNT_ID = 'A-2200' AND JOURNAL_ENTRY_ID IN " +
            "(SELECT JOURNAL_ENTRY_ID FROM JOURNAL_ENTRY WHERE REFERENCE_NUMBER LIKE 'ROYALTY-PAY-%')"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 393.75 AND GL_ACCOUNT_ID = 'A-1000'"));

        // A paid royalty is paid once.
        Assert.Equal(
            RoyaltyRules.CodeAlreadyPaid,
            (await service.PayAsync(royalty.Id, context, "omar")).Reference);

        // ---- AC-3: a disputed royalty cannot be paid -------------------------
        var second = (await service.CalculateAsync(Inputs(), context, "sara")).Value!.SelectedRoyalty!;

        var disputed = await service.DisputeAsync(second.Id, "Owner contests the severance deduction", "sara");

        Assert.True(disputed.Success, disputed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var disputedRoyalty = disputed.Value!.SelectedRoyalty!;

        Assert.True(disputedRoyalty.IsDisputed);
        Assert.False(disputedRoyalty.CanPay);
        Assert.StartsWith("DIS-", disputedRoyalty.DisputeReference ?? string.Empty, StringComparison.Ordinal);

        var blocked = await service.PayAsync(second.Id, context, "nadia");

        Assert.False(blocked.Success);
        Assert.Equal(RoyaltyRules.CodeDisputed, blocked.Reference);
        Assert.Contains(disputedRoyalty.DisputeReference!, blocked.Message!, StringComparison.Ordinal);
        Assert.Contains("severance", blocked.Message!, StringComparison.Ordinal);
        Assert.Equal(1, Count(database, "ROYALTY_CALCULATION WHERE PAYMENT_STATUS = 'PAID'"));

        // Resolving it releases the payment.
        var resolved = await service.ResolveDisputeAsync(second.Id, "deduction agreed", "nadia");

        Assert.True(resolved.Success, resolved.Message);
        Assert.True((await service.PayAsync(second.Id, context, "omar")).Success);
        Assert.Equal(2, Count(database, "ROYALTY_CALCULATION WHERE PAYMENT_STATUS = 'PAID'"));

        // ---- AC-4 and AC-6: a joint-interest statement -----------------------
        var noSource = await service.RaiseJibAsync(
            "PARTNER-1", new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), "  ", "USD", JibLines(), context, "sara");

        Assert.False(noSource.Success);
        Assert.Equal(RoyaltyRules.CodeSourceVolumeRequired, noSource.Reference);
        Assert.Equal(0, Count(database, "JOINT_INTEREST_STATEMENT"));

        var raised = await service.RaiseJibAsync(
            "PARTNER-1", new DateTime(2026, 5, 1), new DateTime(2026, 5, 31), "ALLOC-1", "USD", JibLines(),
            context, "sara");

        Assert.True(raised.Success, raised.Message);

        var jib = raised.Value!.SelectedJib!;

        Assert.Equal(2_500m, jib.Charges);
        Assert.Equal(500m, jib.Credits);
        Assert.Equal(2_000m, jib.Net);
        Assert.True(jib.NetsAsStated);
        Assert.Equal(JibStatus.Draft, jib.State);
        Assert.Equal(3, jib.Lines.Count);
        Assert.Contains("traceable to ALLOC-1", jib.TraceText, StringComparison.Ordinal);

        Assert.Equal(1, Count(database,
            "JOINT_INTEREST_STATEMENT WHERE TOTAL_CHARGES = 2500 AND TOTAL_CREDITS = 500 AND NET_AMOUNT = 2000 " +
            "AND SOURCE_VOLUME_REFERENCE = 'ALLOC-1' AND STATUS = 'DRAFT'"));
        Assert.Equal(2, Count(database, "JIB_CHARGE"));
        Assert.Equal(1, Count(database, "JIB_CREDIT"));

        // ---- the author cannot approve, and a draft cannot be posted ----------
        Assert.Equal(
            RoyaltyRules.CodeNotApproved,
            (await service.PostJibAsync(jib.Id, context, "nadia")).Reference);

        var selfApproved = await service.ReviewJibAsync(jib.Id, approve: true, reason: string.Empty, actor: "sara");

        Assert.False(selfApproved.Success);
        Assert.Equal(RoyaltyRules.CodeAuthorCannotAct, selfApproved.Reference);

        var approved = await service.ReviewJibAsync(jib.Id, approve: true, reason: string.Empty, actor: "nadia");

        Assert.True(approved.Success, approved.Message);
        Assert.Equal(JibStatus.Approved, approved.Value!.SelectedJib!.State);

        // ---- SG-10-7: the bill posts once, at the configured accounts --------
        var posted = await service.PostJibAsync(jib.Id, context, "nadia");

        Assert.True(posted.Success, posted.Message);

        var postedJib = posted.Value!.SelectedJib!;

        Assert.Equal(JibStatus.Posted, postedJib.State);
        Assert.True(postedJib.HasJournal);
        Assert.Equal("ALLOC-1", postedJib.SourceVolumeReference);

        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY WHERE REFERENCE_NUMBER LIKE 'JIB-%' AND TOTAL_DEBIT = 2000 AND TOTAL_CREDIT = 2000"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE DEBIT_AMOUNT = 2000 AND GL_ACCOUNT_ID = 'A-1110'"));
        Assert.Equal(1, Count(database,
            "JOURNAL_ENTRY_LINE WHERE CREDIT_AMOUNT = 2000 AND GL_ACCOUNT_ID = 'A-6001'"));

        // Posting again writes no second journal.
        Assert.True((await service.PostJibAsync(jib.Id, context, "nadia")).Success);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE REFERENCE_NUMBER LIKE 'JIB-%'"));

        // A lost acknowledgement adopts the entry it already wrote.
        Execute(database,
            $"UPDATE JOINT_INTEREST_STATEMENT SET JOURNAL_ENTRY_ID = NULL, STATUS = 'APPROVED', POSTED_BY = NULL, " +
            $"POSTED_AT = NULL WHERE JOINT_INTEREST_STATEMENT_ID = '{jib.Id}';");

        Assert.True((await service.PostJibAsync(jib.Id, context, "nadia")).Success);
        Assert.Equal(1, Count(database, "JOURNAL_ENTRY WHERE REFERENCE_NUMBER LIKE 'JIB-%'"));

        // ---- AC-5: the allocation commits exactly what it previewed ----------
        var targets = new[]
        {
            new CostAllocationMath.AllocationTarget(
                "PR-1", "PROPERTY", 60m),
            new CostAllocationMath.AllocationTarget(
                "PR-2", "PROPERTY", 40m),
        };

        var preview = service.PreviewAllocation(1_000m, "USD", targets, "ProRataWorkingInterest");

        Assert.True(preview.Success, preview.Message);
        Assert.True(preview.Value!.AddsBack);
        Assert.Equal(600m, preview.Value.Shares.Single(share => share.TargetId == "PR-1").Amount);
        Assert.Equal(400m, preview.Value.Shares.Single(share => share.TargetId == "PR-2").Amount);

        // A duplicate target is refused before anything is written.
        var duplicate = service.PreviewAllocation(
            1_000m, "USD",
            [
                targets[0],
                new CostAllocationMath.AllocationTarget(
                    "PR-1", "PROPERTY", 40m),
            ],
            "ProRataWorkingInterest");

        Assert.False(duplicate.Success);
        Assert.Equal(RoyaltyRules.CodeInputInvalid, duplicate.Reference);

        var committed = await service.CommitAllocationAsync(
            1_000m, "USD", targets, "ProRataWorkingInterest", "POOL-1", "sara");

        Assert.True(committed.Success, committed.Message);
        Assert.Equal(2, committed.Value!.CommittedAllocation.Count);

        // The committed shares are the previewed shares, to the cent, and they add back to the pool.
        foreach (var share in preview.Value.Shares)
        {
            Assert.Equal(share.Amount,
                committed.Value.CommittedAllocation.Single(row => row.TargetId == share.TargetId).Amount);
        }

        Assert.Equal(1_000m, committed.Value.CommittedAllocation.Sum(row => row.Amount));
        Assert.Equal(1, Count(database,
            "COST_ALLOCATION WHERE COST_TRANSACTION_ID = 'POOL-1' AND ALLOCATED_AMOUNT = 600 AND PROPERTY_ID = 'PR-1'"));
        Assert.Equal(1, Count(database,
            "COST_ALLOCATION WHERE COST_TRANSACTION_ID = 'POOL-1' AND ALLOCATED_AMOUNT = 400 AND PROPERTY_ID = 'PR-2'"));
        Assert.Equal(1_000m, Sum(database,
            "SELECT SUM(ALLOCATED_AMOUNT) FROM COST_ALLOCATION WHERE COST_TRANSACTION_ID = 'POOL-1'"));

        // ---- the whole register, reloaded ------------------------------------
        var reloaded = await service.LoadAsync(context, allocationReference: "POOL-1");

        Assert.True(reloaded.Success, reloaded.Message);
        Assert.Equal(2, reloaded.Value!.Royalties.Count);
        Assert.Single(reloaded.Value.Jibs);
        Assert.Equal(2, reloaded.Value.CommittedAllocation.Count);

        // ---- an unavailable register is a state ------------------------------
        Execute(database, "DROP TABLE ROYALTY_CALCULATION;");
        Execute(database, "DROP TABLE JOINT_INTEREST_STATEMENT;");

        var unavailable = await service.LoadAsync(context);

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Royalties);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database,
            """
            CREATE TABLE ROYALTY_CALCULATION (
                DISPUTED_IND TEXT,
                DISPUTE_REFERENCE TEXT,
                DISPUTE_REASON TEXT,
                DISPUTED_BY TEXT,
                DISPUTED_AT TEXT,
                DISPUTE_RESOLVED_BY TEXT,
                DISPUTE_RESOLVED_AT TEXT,
                PAID_AT TEXT,
                PAID_BY TEXT,
                PAID_AMOUNT REAL,
                PAYMENT_JOURNAL_ENTRY_ID TEXT,
                AUTHOR TEXT,
                CURRENCY_CODE TEXT,
                JOURNAL_ENTRY_ID TEXT,
                ROYALTY_CALCULATION_ID TEXT PRIMARY KEY,
                CALCULATION_DATE TEXT,
                PROPERTY_OR_LEASE_ID TEXT,
                ALLOCATION_RESULT_ID TEXT,
                ROYALTY_INTEREST_ID TEXT,
                ROYALTY_OWNER_ID TEXT,
                GROSS_REVENUE REAL,
                ROYALTY_DEDUCTIONS_ID TEXT,
                NET_REVENUE REAL,
                ROYALTY_INTEREST REAL,
                ROYALTY_AMOUNT REAL,
                ALLOCATION_DETAIL_ID TEXT,
                TRANSPORTATION_COST REAL,
                AD_VALOREM_TAX REAL,
                SEVERANCE_TAX REAL,
                ROYALTY_STATUS TEXT,
                ROYALTY_TYPE TEXT,
                ROYALTY_BASIS TEXT,
                FLUID_TYPE TEXT,
                PRODUCTION_PERIOD_START TEXT,
                GROSS_VOLUME REAL,
                GROSS_VOLUME_OUOM TEXT,
                PRICE_PER_UNIT REAL,
                PAYMENT_STATUS TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE ROYALTY_DEDUCTIONS (
                ROYALTY_DEDUCTIONS_ID TEXT PRIMARY KEY,
                ROW_ID TEXT,
                PRODUCTION_TAXES REAL,
                TRANSPORTATION_COSTS REAL,
                PROCESSING_COSTS REAL,
                MARKETING_COSTS REAL,
                OTHER_DEDUCTIONS REAL,
                TOTAL_DEDUCTIONS REAL,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE OWNERSHIP_INTEREST (
                OWNERSHIP_ID TEXT,
                OWNER_ID TEXT,
                PROPERTY_OR_LEASE_ID TEXT,
                WORKING_INTEREST REAL,
                NET_REVENUE_INTEREST REAL,
                ROYALTY_INTEREST REAL,
                OVERRIDING_ROYALTY_INTEREST REAL,
                EFFECTIVE_START_DATE TEXT,
                EFFECTIVE_END_DATE TEXT,
                DIVISION_ORDER_ID TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JOINT_INTEREST_STATEMENT (
                STATUS TEXT,
                PARTICIPANT_ID TEXT,
                AUTHOR TEXT,
                APPROVED_BY TEXT,
                APPROVED_AT TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                RETURN_REASON TEXT,
                JOURNAL_ENTRY_ID TEXT,
                SOURCE_VOLUME_REFERENCE TEXT,
                CURRENCY_CODE TEXT,
                JOINT_INTEREST_STATEMENT_ID TEXT PRIMARY KEY,
                REPORT_TYPE TEXT,
                REPORT_PERIOD_START TEXT,
                REPORT_PERIOD_END TEXT,
                GENERATION_DATE TEXT,
                GENERATED_BY TEXT,
                JIB_ID TEXT,
                OPERATOR TEXT,
                TOTAL_CHARGES REAL,
                TOTAL_CREDITS REAL,
                NET_AMOUNT REAL,
                ROW_ID TEXT,
                STATEMENT_ID TEXT,
                PROPERTY_OR_LEASE_ID TEXT,
                STATEMENT_PERIOD_START TEXT,
                STATEMENT_PERIOD_END TEXT,
                GENERATED_DATE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JIB_CHARGE (
                JIB_CHARGE_ID TEXT PRIMARY KEY,
                JOINT_INTEREST_STATEMENT_ID TEXT,
                DESCRIPTION TEXT,
                AMOUNT REAL,
                CATEGORY TEXT,
                ROW_ID TEXT,
                CHARGE_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JIB_CREDIT (
                JIB_CREDIT_ID TEXT PRIMARY KEY,
                JOINT_INTEREST_STATEMENT_ID TEXT,
                DESCRIPTION TEXT,
                AMOUNT REAL,
                CATEGORY TEXT,
                ROW_ID TEXT,
                CREDIT_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE COST_ALLOCATION (
                COST_ALLOCATION_ID TEXT PRIMARY KEY,
                COST_TRANSACTION_ID TEXT,
                PROPERTY_ID TEXT,
                WELL_ID TEXT,
                COST_CENTER_ID TEXT,
                ALLOCATED_AMOUNT REAL,
                ALLOCATION_PERCENTAGE REAL,
                ALLOCATION_METHOD TEXT,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE GL_ACCOUNT (
                CONFIGURATION_COMMAND_JSON TEXT,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                BOOK_ID TEXT,
                CURRENCY_CODE TEXT,
                GL_ACCOUNT_ID TEXT PRIMARY KEY,
                ACCOUNT_NUMBER TEXT,
                ACCOUNT_NAME TEXT,
                ACCOUNT_TYPE TEXT,
                PARENT_ACCOUNT_ID TEXT,
                NORMAL_BALANCE TEXT,
                OPENING_BALANCE REAL,
                CURRENT_BALANCE REAL,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JOURNAL_ENTRY (
                AUTHOR TEXT,
                POSTED_BY TEXT,
                POSTED_AT TEXT,
                PERIOD_CODE TEXT,
                REVERSAL_OF_ENTRY_ID TEXT,
                REVERSED_AT TEXT,
                JOURNAL_ENTRY_ID TEXT PRIMARY KEY,
                ENTRY_NUMBER TEXT,
                ENTRY_DATE TEXT,
                ENTRY_TYPE TEXT,
                STATUS TEXT,
                DESCRIPTION TEXT,
                REFERENCE_NUMBER TEXT,
                SOURCE_MODULE TEXT,
                TOTAL_DEBIT REAL,
                TOTAL_CREDIT REAL,
                ROW_ID TEXT,
                JOURNAL_DATE TEXT,
                JOURNAL_TYPE TEXT,
                JOURNAL_DESCRIPTION TEXT,
                SOURCE_SYSTEM TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE JOURNAL_ENTRY_LINE (
                JOURNAL_ENTRY_LINE_ID TEXT PRIMARY KEY,
                JOURNAL_ENTRY_ID TEXT,
                GL_ACCOUNT_ID TEXT,
                LINE_NUMBER INTEGER,
                DEBIT_AMOUNT REAL,
                CREDIT_AMOUNT REAL,
                DESCRIPTION TEXT,
                ROW_ID TEXT,
                COST_CENTER_ID TEXT,
                CURRENCY_CODE TEXT,
                EXCHANGE_RATE REAL,
                SHADOW_DEBIT_AMOUNT REAL,
                SHADOW_CREDIT_AMOUNT REAL,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            CREATE TABLE COMPANY_LEDGER_PERIOD (
                COMPANY_LEDGER_PERIOD_ID TEXT PRIMARY KEY,
                OWNER_TENANT_ID TEXT,
                OWNER_COMPANY_BA_ID TEXT,
                BOOK_ID TEXT,
                CURRENCY_CODE TEXT,
                CALENDAR_ID TEXT,
                PERIOD_CODE TEXT,
                START_DATE TEXT,
                END_DATE TEXT,
                STATUS TEXT,
                AUTHORITY_JSON TEXT,
                STATE_HISTORY_JSON TEXT,
                CLOSED_BY TEXT,
                CLOSED_AT TEXT,
                REOPENED_BY TEXT,
                REOPENED_AT TEXT,
                REOPEN_REASON TEXT,
                CLOSE_NOTE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                EXPIRY_DATE TEXT,
                EFFECTIVE_DATE TEXT,
                REMARK TEXT,
                SOURCE TEXT);

            INSERT INTO GL_ACCOUNT (GL_ACCOUNT_ID, ACCOUNT_NUMBER, ACCOUNT_NAME, ACCOUNT_TYPE, NORMAL_BALANCE,
                CURRENCY_CODE, ACTIVE_IND) VALUES
                ('A-6100', '6100', 'Royalty expense', 'EXPENSE', 'DEBIT', 'USD', 'Y'),
                ('A-2200', '2200', 'Accrued royalties', 'LIABILITY', 'CREDIT', 'USD', 'Y'),
                ('A-1000', '1000', 'Cash', 'ASSET', 'DEBIT', 'USD', 'Y'),
                ('A-1110', '1110', 'Accounts receivable', 'ASSET', 'DEBIT', 'USD', 'Y'),
                ('A-6001', '6001', 'Operating expense', 'EXPENSE', 'DEBIT', 'USD', 'Y');

            INSERT INTO OWNERSHIP_INTEREST (OWNERSHIP_ID, OWNER_ID, PROPERTY_OR_LEASE_ID, WORKING_INTEREST,
                NET_REVENUE_INTEREST, ROYALTY_INTEREST, ACTIVE_IND) VALUES
                ('O-1', 'OWNER-A', 'LSE-1', 80, 75, 12.5, 'Y'),
                ('O-2', 'OWNER-B', 'LSE-1', 20, 25, 12.5, 'Y'),
                ('O-3', 'OWNER-C', 'LSE-3', 60, 60, 12.5, 'Y');
            """);

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static long Count(string database, string fromAndWhere)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM " + fromAndWhere + ";";

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static decimal Sum(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var value = command.ExecuteScalar();

        return value is null or DBNull ? 0m : Convert.ToDecimal(value);
    }

    private static OgRoyaltyInputs Inputs() =>
        new(
            "LSE-1",
            "OWNER-A",
            "AL-1",
            "AR-1",
            GrossVolume: 50m,
            VolumeUnit: "bbl",
            PricePerUnit: 70m,
            RatePercent: 12.5m,
            DeclaredResidualPercent: 0m,
            Currency: "USD",
            Deductions: new OgRoyaltyDeductions(200m, 100m, 50m, 0m, 0m));

    private static IReadOnlyList<OgJibLine> JibLines() =>
    [
        new OgJibLine("CHARGE", "Casing", "MATERIAL", 2_000m),
        new OgJibLine("CHARGE", "Rig day rate", "SERVICE", 500m),
        new OgJibLine("CREDIT", "Cost recovery", "RECOVERY", 500m),
    ];

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
