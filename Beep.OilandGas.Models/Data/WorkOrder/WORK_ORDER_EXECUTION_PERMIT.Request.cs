using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.WorkOrder;

/// <summary>
/// The request columns of an execution permit (WRK-04).
/// </summary>
/// <remarks>
/// The owner's permit records who issued it (<c>ISSUED_BY</c>) and its window, but not who asked for it - so
/// <b>AC-3</b> (only a different person may issue a permit they did not request) had nothing to compare against. The four
/// columns are additive: a permit issued by the owner's own flow simply records no requester.
/// </remarks>
public partial class WORK_ORDER_EXECUTION_PERMIT
{
    /// <summary>Who asked for the permit.</summary>
    [MaxLength(128)]
    public string? REQUESTED_BY
    {
        get => RequestedByValue;
        set => SetProperty(ref RequestedByValue, value);
    }

    private string? RequestedByValue;

    public DateTime? REQUESTED_AT_UTC
    {
        get => RequestedAtValue;
        set => SetProperty(ref RequestedAtValue, value);
    }

    private DateTime? RequestedAtValue;

    /// <summary>The conditions the permit is issued under, recorded in the issuer's own words.</summary>
    [MaxLength(1000)]
    public string? CONDITIONS
    {
        get => ConditionsValue;
        set => SetProperty(ref ConditionsValue, value);
    }

    private string? ConditionsValue;

    /// <summary>
    /// The key the request was made with, so a retried request lands on the permit it already wrote instead of issuing a
    /// second one (AC-6).
    /// </summary>
    [MaxLength(128)]
    public string? REQUEST_INTENT_ID
    {
        get => RequestIntentValue;
        set => SetProperty(ref RequestIntentValue, value);
    }

    private string? RequestIntentValue;
}
