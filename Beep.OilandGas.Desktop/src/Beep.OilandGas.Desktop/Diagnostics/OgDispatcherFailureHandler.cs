using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>
/// FND-06 AC-7: what the application does when an exception escapes a user action on the UI thread.
/// </summary>
/// <remarks>
/// <para>
/// The exception is reported like any other, the action is marked handled so the process survives, and the
/// person is shown <b>only the reference</b> — never the exception's text.
/// </para>
/// <para>
/// The "show" step is a callback rather than a <c>MessageBox</c> call so the rule can be tested without a
/// window: the product keeps exactly one <c>MessageBox</c>, in startup, and that is enforced by a guard.
/// </para>
/// </remarks>
public sealed class OgDispatcherFailureHandler
{
    private readonly OgDiagnosticsPipeline _pipeline;
    private readonly Action<string> _showReference;

    public OgDispatcherFailureHandler(OgDiagnosticsPipeline pipeline, Action<string> showReference)
    {
        _pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _showReference = showReference ?? throw new ArgumentNullException(nameof(showReference));
    }

    /// <summary>
    /// Reports <paramref name="exception"/> and tells the person the reference. Always returns true: a
    /// dispatcher failure that is not marked handled ends the process, and losing the session over one
    /// failed action is worse than letting the person carry on.
    /// </summary>
    public bool Handle(Exception exception, out string reference)
    {
        ArgumentNullException.ThrowIfNull(exception);

        reference = _pipeline.ReportHandled(
            exception,
            "dispatching a user action",
            "the action did not complete",
            FailureSeverity.Error);

        _showReference(reference);
        return true;
    }
}
