namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// The outcome of an operation the desktop performed. A failed result always carries the failure
/// reference that was written to the diagnostics store; it never carries exception text.
/// </summary>
public class OgResult
{
    protected OgResult(bool success, string? message, string? reference)
    {
        Success = success;
        Message = message;
        Reference = reference;
    }

    public bool Success { get; }

    /// <summary>A localisation key or an already-localised sentence safe to show.</summary>
    public string? Message { get; }

    /// <summary>The failure reference, when this result is a failure. Never exception text.</summary>
    public string? Reference { get; }

    public static OgResult Ok(string? message = null) => new(true, message, null);

    public static OgResult Failed(string message, string reference) => new(false, message, reference);

    public static OgResult<T> Ok<T>(T value) => OgResult<T>.Ok(value);

    public static OgResult<T> Failed<T>(string message, string reference) => OgResult<T>.Failed(message, reference);
}

/// <summary>An outcome that carries a value when it succeeded.</summary>
public sealed class OgResult<T> : OgResult
{
    private OgResult(bool success, T? value, string? message, string? reference)
        : base(success, message, reference)
    {
        Value = value;
    }

    public T? Value { get; }

    public static OgResult<T> Ok(T value) => new(true, value, null, null);

    public static new OgResult<T> Failed(string message, string reference) =>
        new(false, default, message, reference);
}
