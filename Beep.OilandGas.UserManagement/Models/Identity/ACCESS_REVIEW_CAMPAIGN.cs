using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.UserManagement.Models.Identity;

/// <summary>
/// Represents a periodic access review campaign (quarterly certification).
/// Managers must certify that their team members' access is still appropriate.
/// Required by SOX ITGC and ISO 27001.
/// Part of Phase 4 governance & compliance.
/// </summary>
public class ACCESS_REVIEW_CAMPAIGN : ModelEntityBase
{
    [Key]
    public string CAMPAIGN_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Campaign name, e.g. "Q3 2026 Access Review".</summary>
    public string CAMPAIGN_NAME { get; set; } = string.Empty;

    /// <summary>When the campaign starts.</summary>
    public DateTime START_DATE { get; set; } = DateTime.UtcNow;

    /// <summary>Deadline for all reviews (typically 30 days).</summary>
    public DateTime DUE_DATE { get; set; } = DateTime.UtcNow.AddDays(30);

    /// <summary>Status: ACTIVE, COMPLETED, OVERDUE.</summary>
    public string STATUS { get; set; } = "ACTIVE";

    /// <summary>Who initiated the campaign.</summary>
    public string INITIATED_BY { get; set; } = string.Empty;

    /// <summary>Optional notes about the campaign scope.</summary>
    public string? DESCRIPTION { get; set; }
}

/// <summary>
/// Individual access review item within a campaign.
/// A manager reviews a specific user's roles and either certifies or revokes them.
/// </summary>
public class ACCESS_REVIEW_ITEM : ModelEntityBase
{
    [Key]
    public string REVIEW_ITEM_ID { get; set; } = Guid.NewGuid().ToString();

    /// <summary>FK → ACCESS_REVIEW_CAMPAIGN.CAMPAIGN_ID.</summary>
    public string CAMPAIGN_ID { get; set; } = string.Empty;

    /// <summary>FK → USER.USER_ID — the user being reviewed.</summary>
    public string USER_ID { get; set; } = string.Empty;

    /// <summary>FK → USER.USER_ID — the manager performing the review.</summary>
    public string REVIEWER_ID { get; set; } = string.Empty;

    /// <summary>JSON snapshot of the user's roles at campaign start.</summary>
    public string? CURRENT_ROLES_JSON { get; set; }

    /// <summary>JSON snapshot of the user's effective permissions at campaign start.</summary>
    public string? CURRENT_PERMISSIONS_JSON { get; set; }

    /// <summary>Decision: CERTIFIED, REVOKED, MODIFIED, PENDING.</summary>
    public string DECISION { get; set; } = "PENDING";

    /// <summary>Reviewer comments.</summary>
    public string? COMMENTS { get; set; }

    /// <summary>When the review was completed.</summary>
    public DateTime? REVIEWED_DATE { get; set; }
}
