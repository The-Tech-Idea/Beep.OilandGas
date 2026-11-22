namespace TheTechIdea.Beep.OilandGas.Desktop.Controls;

/// <summary>The five states every feature page must implement.</summary>
public enum OgViewState
{
    /// <summary>Content is shown normally.</summary>
    Ready,

    /// <summary>Nothing exists yet for this context.</summary>
    Empty,

    /// <summary>A read is in progress.</summary>
    Loading,

    /// <summary>A read or write failed. The message carries the failure reference.</summary>
    Failed,

    /// <summary>The person may not see this.</summary>
    Denied,

    /// <summary>There are unsaved changes that the person must resolve.</summary>
    Unsaved,
}
