using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class CompanyGhgSelectionTests
{
    private static CompanyGhgSelectionRequest Request() => new("KW", new string('A', 64), new string('B', 64),
        "Reviewed the catalog for this company's activities", "", Guid.NewGuid().ToString("N"));

    [Fact]
    public void SelectionIsCompanyAndActorBoundAndRequiresExactRetry()
    {
        var company = new CompanyScope("tenant", "company"); var other = new CompanyScope("tenant", "other");
        var request = Request(); var saved = CompanyGhgSelectionIdentity.Prepare(company, "reviewer", request, null);
        Assert.Equal(saved, CompanyGhgSelectionIdentity.Prepare(company, "reviewer", request, saved));
        Assert.NotEqual(CompanyGhgSelectionIdentity.ForCompany(company), CompanyGhgSelectionIdentity.ForCompany(other));
        Assert.Throws<InvalidOperationException>(() => CompanyGhgSelectionIdentity.RequireValid(saved, other));
        Assert.Throws<InvalidOperationException>(() => CompanyGhgSelectionIdentity.Prepare(company, "another-reviewer", request, saved));
        Assert.Throws<InvalidOperationException>(() => CompanyGhgSelectionIdentity.Prepare(company, "reviewer", request with { CatalogRevision = new string('C', 64) }, saved));
        var next = request with { ExpectedRevision = saved.Revision, IntentId = Guid.NewGuid().ToString("N") };
        Assert.Equal(saved.Revision, CompanyGhgSelectionIdentity.Prepare(company, "reviewer", next, saved).PreviousRevision);
    }

    [Fact]
    public void InvalidEvidenceAndUnboundedSchemaAreRejected()
    {
        var company = new CompanyScope("tenant", "company"); var request = Request();
        Assert.Throws<ArgumentException>(() => CompanyGhgSelectionIdentity.Prepare(company, "reviewer", request with { Rationale = "" }, null));
        Assert.Throws<ArgumentException>(() => CompanyGhgSelectionIdentity.Prepare(company, "reviewer", request with { EnvironmentRevision = "stale" }, null));
        Assert.Throws<ArgumentException>(() => CompanyGhgSelectionIdentity.Prepare(company, "reviewer", request with { Jurisdiction = "kw" }, null));
        var entity = typeof(COMPANY_GHG_CATALOG_SELECTION);
        Assert.Equal("COMPANY_GHG_CATALOG_SELECTION_ID", Assert.Single(entity.GetProperties(), p => p.GetCustomAttribute<KeyAttribute>() is not null).Name);
        Assert.Equal(64, entity.GetProperty("CATALOG_REVISION")!.GetCustomAttribute<MaxLengthAttribute>()!.Length);
        Assert.Equal(2000, entity.GetProperty("RATIONALE")!.GetCustomAttribute<MaxLengthAttribute>()!.Length);
    }
}
