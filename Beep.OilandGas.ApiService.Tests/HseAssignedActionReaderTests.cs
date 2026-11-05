using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.HSE;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class HseAssignedActionReaderTests
{
    private static readonly FieldSelectionReview Context = new(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field");
    private static readonly string Revision = new('A', 64);

    [Theory]
    [InlineData("valid")]
    [InlineData("empty")]
    [InlineData("missing-link")]
    [InlineData("expired-link")]
    [InlineData("foreign-catalog")]
    [InlineData("foreign-field")]
    [InlineData("reassigned")]
    [InlineData("changed-link")]
    [InlineData("revoked")]
    [InlineData("unknown-state")]
    [InlineData("excess-population")]
    [InlineData("unknown-date")]
    public async Task AssignedActionsUseCurrentRecordedPersonAndRefuseUnconfirmedPopulations(string mode)
        => await VerifyPopulation(mode, IncidentState.PendingCorrectiveAction);

    [Theory]
    [InlineData(IncidentState.Reported)]
    [InlineData(IncidentState.UnderInvestigation)]
    [InlineData(IncidentState.PendingRCA)]
    [InlineData(IncidentState.PendingCorrectiveAction)]
    [InlineData(IncidentState.PendingCloseOut)]
    [InlineData(IncidentState.Closed)]
    [InlineData(IncidentState.Cancelled)]
    public async Task AssignedActionsIncludeRecordedPlansInEveryOpenIncidentStage(string state)
        => await VerifyPopulation("valid", state);

    private static async Task VerifyPopulation(string mode, string state)
    {
        var now = DateTime.UtcNow;
        var identityContext = new CompanyPersonnelPolicyContext(Context.Company, Context.Database, "actor", Revision);
        var link = new CompanyPersonnelIdentityVersion(Context.Company, Context.Database,
            CompanyPersonnelIdentityCommand.RecordIdFor(Context.Company, "actor", 1), "actor", "person", "EMPLOYEE", 1,
            now.AddDays(-2), mode == "expired-link" ? now.AddDays(-1) : now.AddDays(2), "Recorded employee identity", "administrator", now.AddDays(-3), Revision);
        var identities = new Mock<ICompanyPersonnelIdentityOwner>(MockBehavior.Strict);
        var catalogReads = 0;
        identities.Setup(x => x.ReadPersonnelIdentitiesAsync(It.IsAny<CompanyPersonnelPolicyContext>(), It.IsAny<Func<Task>>()))
            .Returns((CompanyPersonnelPolicyContext c, Func<Task> guard) =>
            {
                ++catalogReads;
                var context = mode == "foreign-catalog" ? c with { Database = new("foreign", "foreign-binding") } : c;
                var row = link with { Database = context.Database, Revision = mode == "changed-link" && catalogReads > 1 ? new('B', 64) : Revision };
                return Task.FromResult(CompanyPersonnelIdentityCatalog.Create(context, mode == "missing-link" ? [] : [row], DateTime.UtcNow));
            });
        var person = new CompanyNativePersonFacts(Context.Company, "person", "A display name shared by many accounts", "EMPLOYEE",
            now.AddDays(-5), now.AddDays(5), new("company", "person", 1, "Technician", "ACTIVE", now.AddDays(-5), now.AddDays(5), Revision), Revision);
        identities.Setup(x => x.ReadIdentityPersonAsync(identityContext, "person", It.IsAny<Func<Task>>())).ReturnsAsync(person);
        var hse = new Mock<IFieldHSEService>(MockBehavior.Strict); var incidentReads = 0; var actionReads = 0; var guards = 0;
        hse.Setup(x => x.GetIncidentsAsync(null)).Returns(() =>
        {
            ++incidentReads;
            var row = new HSEIncidentRecord { IncidentId = "case/شركة?2", FieldId = mode == "foreign-field" ? "other-field" : "field",
                CurrentState = mode == "unknown-state" ? null! : state };
            return Task.FromResult(mode == "empty" ? new List<HSEIncidentRecord>() : mode == "excess-population"
                ? Enumerable.Range(0, 65).Select(i => new HSEIncidentRecord { IncidentId = "case-" + i, FieldId = "field", CurrentState = IncidentState.PendingCorrectiveAction }).ToList()
                : new List<HSEIncidentRecord> { row });
        });
        hse.Setup(x => x.GetCorrectiveActionsAsync("case/شركة?2")).Returns(() =>
        {
            ++actionReads;
            return Task.FromResult(new List<CAStatus>
            {
                new(1, "My overdue repair", "CORRECTIVE", "OPEN", mode == "unknown-date" ? DateTime.SpecifyKind(now.AddHours(-2), DateTimeKind.Unspecified) : now.AddHours(-2), true,
                    mode == "reassigned" && actionReads > 1 ? "another-person" : "person", null),
                new(2, "Another person's repair", "CORRECTIVE", "OPEN", now.AddHours(-3), true, "another-person", null),
                new(3, "My future repair", "CORRECTIVE", "OPEN", now.AddHours(3), false, "person", null),
                new(4, "My repair completed late", "CORRECTIVE", "COMPLETED", now.AddHours(-3), true, "person", now.AddHours(-1))
            });
        });
        Task Guard() { if (mode == "revoked" && ++guards > 3) throw new UnauthorizedAccessException("Current access revoked"); return Task.CompletedTask; }
        Task<CompanyHseAssignedActions> Read() => CompanyHseAssignedActionReader.ReadPopulationAsync(Context, Revision, identities.Object, hse.Object, Guard, default);
        if (mode is "valid" or "empty")
        {
            var result = await Read(); result.Require(); Assert.Equal(Context, result.Context);
            if (mode == "valid" && state is not (IncidentState.Closed or IncidentState.Cancelled)) { var row = Assert.Single(result.Actions); Assert.Equal(1, row.StepSequence); Assert.Equal("My overdue repair", row.Description); Assert.Equal(1, result.ReturnedIncidentCount); Assert.Equal(2, actionReads); }
            else if (mode == "valid") { Assert.Empty(result.Actions); Assert.Equal(1, result.ReturnedIncidentCount); Assert.Equal(0, actionReads); }
            else { Assert.Empty(result.Actions); Assert.Equal(0, result.ReturnedIncidentCount); }
            Assert.Equal(2, incidentReads);
        }
        else
        {
            if (mode == "revoked") await Assert.ThrowsAsync<UnauthorizedAccessException>(Read);
            else await Assert.ThrowsAsync<RefusalException>(Read);
            if (mode is "missing-link" or "expired-link" or "foreign-catalog") Assert.Equal(0, incidentReads);
        }
        identities.Verify(x => x.RecordPersonnelIdentityAsync(It.IsAny<CompanyPersonnelIdentityCommand>(), It.IsAny<Func<Task>>(), It.IsAny<Func<Task>>(), It.IsAny<bool>()), Times.Never);
    }
}
