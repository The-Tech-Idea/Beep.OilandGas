using Beep.OilandGas.PermitsAndApplications.Services;
using Xunit;

namespace Beep.OilandGas.PermitsAndApplications.Tests;

public class PermitStatusTransitionRulesTests
{
    [Theory]
    [InlineData("DRAFT", "SUBMITTED", true)]
    [InlineData("DRAFT", "WITHDRAWN", true)]
    [InlineData("SUBMITTED", "UNDER_REVIEW", true)]
    [InlineData("SUBMITTED", "APPROVED", true)]
    [InlineData("SUBMITTED", "REJECTED", true)]
    [InlineData("SUBMITTED", "ADDITIONAL_INFO_REQUIRED", true)]
    [InlineData("ADDITIONAL_INFO_REQUIRED", "SUBMITTED", true)]
    [InlineData("APPROVED", "EXPIRED", true)]
    [InlineData("EXPIRED", "RENEWED", true)]
    [InlineData("DRAFT", "APPROVED", false)]
    [InlineData("REJECTED", "APPROVED", false)]
    [InlineData("WITHDRAWN", "SUBMITTED", false)]
    public void IsTransitionAllowed_matches_regulatory_model(string current, string next, bool expected)
    {
        Assert.Equal(expected, PermitStatusTransitionRules.IsTransitionAllowed(current, next));
    }

    [Fact]
    public void Normalize_blank_is_draft()
    {
        Assert.Equal("DRAFT", PermitStatusTransitionRules.Normalize(null));
        Assert.Equal("DRAFT", PermitStatusTransitionRules.Normalize(""));
        Assert.Equal("DRAFT", PermitStatusTransitionRules.Normalize("   "));
    }

    [Fact]
    public void Same_status_is_not_allowed_transition()
    {
        Assert.False(PermitStatusTransitionRules.IsTransitionAllowed("DRAFT", "DRAFT"));
    }
}
