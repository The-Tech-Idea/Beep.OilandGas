using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Web.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CompanyEnvironmentSourceReviewValidatorTests
{
    private static readonly CompanyScope Company = new("tenant", "company");
    private static readonly string Revision = new('A', 64);
    private static readonly string Effective = new('B', 64);
    private static CompanyEnvironmentSourceReview Review() => new(Company, Revision,
        [new CompanyEnvironmentSource("wells", "PPDM_CORE", "ppdm39", 1, 0, "Bound")
        { MappingStatus = "ProfileChecked", CodeMappings = [new("string", "kind", "A", "Production", "Company")] }], false, Effective);

    [Fact]
    public void AcceptsMatchingBindingReview() => CompanyEnvironmentSourceReviewValidator.RequireValid(Review(), Company, Revision, Effective);

    [Theory]
    [InlineData("company")][InlineData("revision")][InlineData("effective")][InlineData("tested")]
    [InlineData("duplicates")][InlineData("mapping")][InlineData("status")]
    public void RejectsMismatchedOrInconsistentEvidence(string fault)
    {
        var value = Review(); var item = value.Sources[0];
        value = fault switch
        {
            "company" => value with { Company = new("tenant", "other") },
            "revision" => value with { Revision = new string('C', 64) },
            "effective" => value with { EffectiveRevision = new string('C', 64) },
            "tested" => value with { DataTested = true },
            "duplicates" => value with { Sources = [item, item with { Name = "WELLS" }] },
            "mapping" => value with { Sources = [item with { CodeMappings = [] }] },
            _ => value with { Sources = [item with { MappingStatus = "ProfileChecked", MappingIssues = ["issue"] }] }
        };
        Assert.Throws<InvalidOperationException>(() => CompanyEnvironmentSourceReviewValidator.RequireValid(value, Company, Revision, Effective));
    }
}
#endif
