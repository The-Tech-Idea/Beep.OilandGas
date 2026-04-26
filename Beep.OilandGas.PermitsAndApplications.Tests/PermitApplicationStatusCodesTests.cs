using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Constants;
using Xunit;

namespace Beep.OilandGas.PermitsAndApplications.Tests;

public class PermitApplicationStatusCodesTests
{
    [Theory]
    [InlineData(PermitApplicationStatus.Draft, "DRAFT")]
    [InlineData(PermitApplicationStatus.Submitted, "SUBMITTED")]
    [InlineData(PermitApplicationStatus.UnderReview, "UNDER_REVIEW")]
    [InlineData(PermitApplicationStatus.AdditionalInformationRequired, "ADDITIONAL_INFO_REQUIRED")]
    [InlineData(PermitApplicationStatus.Approved, "APPROVED")]
    [InlineData(PermitApplicationStatus.Rejected, "REJECTED")]
    [InlineData(PermitApplicationStatus.Withdrawn, "WITHDRAWN")]
    [InlineData(PermitApplicationStatus.Expired, "EXPIRED")]
    [InlineData(PermitApplicationStatus.Renewed, "RENEWED")]
    public void ToStorageKey_is_upper_snake(PermitApplicationStatus status, string expected)
    {
        Assert.Equal(expected, PermitApplicationStatusCodes.ToStorageKey(status));
    }

    [Fact]
    public void ToStorageKey_null_is_draft()
    {
        Assert.Equal(PermitApplicationStatusCodes.Draft, PermitApplicationStatusCodes.ToStorageKey(null));
    }

    [Theory]
    [InlineData("DRAFT", PermitApplicationStatus.Draft)]
    [InlineData("draft", PermitApplicationStatus.Draft)]
    [InlineData("SUBMITTED", PermitApplicationStatus.Submitted)]
    [InlineData("Submitted", PermitApplicationStatus.Submitted)]
    public void FromStorageKey_parses_variants(string persisted, PermitApplicationStatus expected)
    {
        Assert.Equal(expected, PermitApplicationStatusCodes.FromStorageKey(persisted));
    }

    [Fact]
    public void FromStorageKey_round_trips_ToStorageKey_for_all_statuses()
    {
        foreach (PermitApplicationStatus status in Enum.GetValues<PermitApplicationStatus>())
        {
            var key = PermitApplicationStatusCodes.ToStorageKey(status);
            Assert.Equal(status, PermitApplicationStatusCodes.FromStorageKey(key));
        }
    }
}
