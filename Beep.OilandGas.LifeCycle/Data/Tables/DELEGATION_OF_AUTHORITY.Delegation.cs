using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.LifeCycle.Data.Tables;

/// <summary>
/// The person-to-person columns of a delegation of authority (WFL-06, AC-3).
/// </summary>
/// <remarks>
/// The owner's table models the authority ladder - an entity type, a threshold and the role that may approve at that level - and
/// has no way to say that one named person is acting for another for a window of time. Those four facts are added here,
/// additively, so an existing threshold row stays valid and simply records no person delegation.
/// </remarks>
public partial class DELEGATION_OF_AUTHORITY
{
    /// <summary>The person whose authority is being exercised.</summary>
    [MaxLength(128)]
    public string? DELEGATOR_USER_ID
    {
        get => DelegatorValue;
        set => SetProperty(ref DelegatorValue, value);
    }

    private string? DelegatorValue;

    /// <summary>The person acting for them.</summary>
    [MaxLength(128)]
    public string? DELEGATE_USER_ID
    {
        get => DelegateValue;
        set => SetProperty(ref DelegateValue, value);
    }

    private string? DelegateValue;

    /// <summary>The first moment the delegation may be used.</summary>
    public DateTime? VALID_FROM_UTC
    {
        get => FromValue;
        set => SetProperty(ref FromValue, value);
    }

    private DateTime? FromValue;

    /// <summary>The last moment it may be used; outside this window the delegation is inactive.</summary>
    public DateTime? VALID_TO_UTC
    {
        get => ToValue;
        set => SetProperty(ref ToValue, value);
    }

    private DateTime? ToValue;

    /// <summary>Y while the administrator keeps it in force; N when it is withdrawn.</summary>
    [MaxLength(1)]
    public string? IS_ACTIVE
    {
        get => ActiveValue;
        set => SetProperty(ref ActiveValue, value);
    }

    private string? ActiveValue;
}
