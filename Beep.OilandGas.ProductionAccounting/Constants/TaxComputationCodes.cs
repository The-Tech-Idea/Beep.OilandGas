namespace Beep.OilandGas.ProductionAccounting.Constants;

/// <summary>
/// The tax-computation vocabulary the owner did not have (ACC-08).
/// </summary>
/// <remarks>
/// The production-tax kinds are the owner's own (<see cref="ProductionTaxAssessmentCodes"/>); what did not exist is a
/// vocabulary for a tax <b>provision</b>, and a status for a computation before and after it is posted. Both are stated
/// here so the screen and the service cannot disagree.
/// </remarks>
public static class TaxComputationCodes
{
    /// <summary>A current-tax provision on the period's taxable income (IAS 12).</summary>
    public const string ProvisionCurrent = "PROVISION_CURRENT";

    /// <summary>A deferred-tax provision, the movement in the temporary differences.</summary>
    public const string ProvisionDeferred = "PROVISION_DEFERRED";

    /// <summary>Every computation kind this workbench writes.</summary>
    public static readonly string[] Kinds =
    [
        ProductionTaxAssessmentCodes.Severance,
        ProductionTaxAssessmentCodes.AdValorem,
        ProvisionCurrent,
        ProvisionDeferred,
    ];

    /// <summary>Computed and not yet posted; the computation may still be reviewed.</summary>
    public const string StatusComputed = "COMPUTED";

    /// <summary>Posted: the journal exists and the computation is read-only.</summary>
    public const string StatusPosted = "POSTED";

    /// <summary>Every computation state.</summary>
    public static readonly string[] Statuses = [StatusComputed, StatusPosted];

    /// <summary>Whether a kind is a provision rather than a production tax - the ones the fiscal-year gate applies to.</summary>
    public static bool IsProvision(string? kind) =>
        string.Equals(kind?.Trim(), ProvisionCurrent, System.StringComparison.OrdinalIgnoreCase)
        || string.Equals(kind?.Trim(), ProvisionDeferred, System.StringComparison.OrdinalIgnoreCase);

    /// <summary>A kind's name for a person.</summary>
    public static string Describe(string? kind) => kind?.Trim().ToUpperInvariant() switch
    {
        ProductionTaxAssessmentCodes.Severance => "severance tax",
        ProductionTaxAssessmentCodes.AdValorem => "ad valorem tax",
        ProvisionCurrent => "current tax provision",
        ProvisionDeferred => "deferred tax provision",
        _ => kind ?? "(no kind)",
    };
}
