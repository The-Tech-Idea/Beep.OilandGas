namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// The current actor and company, so a failure or audit row records who and where without every caller
/// passing them. Set once at sign-in and on a company/field switch.
/// </summary>
public sealed class OgRuntimeContext
{
    public string? UserId { get; set; }

    public string? CompanyId { get; set; }

    public string? FieldId { get; set; }
}
