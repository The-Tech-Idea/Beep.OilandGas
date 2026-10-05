using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
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

    // The services pass a status member's name (status.ToString()); upper-casing it alone gave UNDERREVIEW, a key the
    // rules do not have, so an application under review could never be approved.
    [Theory]
    [InlineData(nameof(PermitApplicationStatus.UnderReview), "UNDER_REVIEW")]
    [InlineData(nameof(PermitApplicationStatus.AdditionalInformationRequired), "ADDITIONAL_INFO_REQUIRED")]
    [InlineData(nameof(PermitApplicationStatus.Submitted), "SUBMITTED")]
    [InlineData("under_review", "UNDER_REVIEW")]
    public void Normalize_reads_a_member_name_as_its_storage_key(string status, string expected)
    {
        Assert.Equal(expected, PermitStatusTransitionRules.Normalize(status));
    }

    [Fact]
    public void Normalize_keeps_an_unknown_status_unknown()
    {
        Assert.Equal("NOT_A_STATUS", PermitStatusTransitionRules.Normalize("not_a_status"));
        Assert.False(PermitStatusTransitionRules.IsTransitionAllowed("not_a_status", "SUBMITTED"));
    }

    [Fact]
    public void An_application_under_review_can_be_approved_by_member_name()
    {
        Assert.True(PermitStatusTransitionRules.IsTransitionAllowed(
            nameof(PermitApplicationStatus.UnderReview), nameof(PermitApplicationStatus.Approved)));
    }

    [Fact]
    public void A_refused_transition_is_a_conflict_in_words_for_the_person()
    {
        var refusal = PermitStatusTransitionRules.RefuseTransition("DRAFT", nameof(PermitApplicationStatus.Approved));

        Assert.Equal(RefusalKind.Conflict, refusal.Kind);
        Assert.Equal("A permit application that is draft cannot be moved to approved.", refusal.Sentence);
    }

    [Fact]
    public void Moving_to_the_status_it_has_is_refused_as_already_there()
    {
        var refusal = PermitStatusTransitionRules.RefuseTransition("SUBMITTED", "Submitted");

        Assert.Equal(RefusalKind.Conflict, refusal.Kind);
        Assert.Equal("The permit application is already submitted.", refusal.Sentence);
    }
}
