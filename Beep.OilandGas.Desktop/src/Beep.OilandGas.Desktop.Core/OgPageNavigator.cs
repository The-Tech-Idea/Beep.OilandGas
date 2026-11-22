namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>What a page asks the workbench to open (DAT-03 AC-3 to DAT-04).</summary>
/// <param name="TableName">The table to show.</param>
/// <param name="Filter">The filter the tree had active, carried into the workbench.</param>
public sealed record OgTableOpenRequest(string TableName, string Filter);

/// <summary>
/// Lets a page ask the shell to show another page. The shell installs the handler; a headless run has none, in
/// which case <see cref="Navigate"/> reports false and the caller says so instead of pretending to navigate.
/// </summary>
public sealed class OgPageNavigator
{
    /// <summary>Set by the shell. Null in a headless run or in a test that only records the intent.</summary>
    public Action<string>? Handler { get; set; }

    /// <summary>The routes requested, in order - the smoke run and tests read this.</summary>
    public List<string> Requested { get; } = [];

    public bool Navigate(string route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            throw new ArgumentException("A navigation needs a route.", nameof(route));
        }

        Requested.Add(route);

        if (Handler is null)
        {
            return false;
        }

        Handler(route);
        return true;
    }
}

/// <summary>
/// The request the tree leaves for the workbench. A page instance is created per navigation, so the request has
/// to outlive the page that recorded it.
/// </summary>
public sealed class OgWorkbenchNavigation
{
    public OgTableOpenRequest? Pending { get; set; }

    /// <summary>Takes the pending request, so a second visit does not silently reuse the first table.</summary>
    public OgTableOpenRequest? Take()
    {
        var pending = Pending;
        Pending = null;
        return pending;
    }
}
