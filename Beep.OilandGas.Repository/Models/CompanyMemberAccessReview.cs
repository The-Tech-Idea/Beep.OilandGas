using Beep.OilandGas.Models.Data.AccessControl;
namespace Beep.OilandGas.Repository.Models;

/// <summary>Current access observed for one account and company; null access means no usable company access.</summary>
public sealed record CompanyMemberAccessReview(CompanyScope Company, string UserId, RepositoryCompanyAccess? Access)
{
    /// <summary>The recorded membership interval, including expired or future membership; it does not itself grant access.</summary>
    public CompanyMembershipObservation? Membership { get; init; }
}

public sealed record CompanyMembershipObservation(DateTime EffectiveFromUtc, DateTime? EffectiveToUtc, string GrantedByUserId, DateTime ChangedUtc, string Revision);
