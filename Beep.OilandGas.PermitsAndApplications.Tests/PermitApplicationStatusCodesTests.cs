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

    [Fact]
    public void Every_status_has_a_distinct_canonical_history_token()
    {
        var statuses=Enum.GetValues<PermitApplicationStatus>();
        var keys=statuses.Select(status=>PermitApplicationStatusCodes.ToStorageKey(status)).ToArray();
        Assert.Equal(statuses.Length,keys.Distinct(StringComparer.Ordinal).Count());
        Assert.All(keys,key=>Assert.Matches("^[A-Z]+(?:_[A-Z]+)*$",key));
    }

    [Theory][InlineData(-1)][InlineData(int.MaxValue)]
    public void Undefined_status_is_refused_instead_of_becoming_a_draft(int value)
    {
        Assert.Throws<System.Runtime.CompilerServices.SwitchExpressionException>(()=>PermitApplicationStatusCodes.ToStorageKey((PermitApplicationStatus)value));
    }
}
