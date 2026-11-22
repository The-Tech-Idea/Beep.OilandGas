using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TheTechIdea.Beep.OilandGas.Desktop.Controls;

/// <summary>
/// REL-03 AC-1: what "reachable and named" means, as a check over a real element tree.
/// </summary>
/// <remarks>
/// <para>
/// A control passes when UI Automation can name it: an explicit <c>AutomationProperties.Name</c>, a
/// <c>LabeledBy</c> reference, or — for controls whose content is the label, such as a button — a string
/// content or header. Requiring an explicit name on a button whose text says "Export" would be noise; requiring
/// one on a text box that has only a neighbouring caption is not, because a screen reader does not read
/// neighbours.
/// </para>
/// <para>
/// The audit walks the <b>logical</b> tree, which is what page constructors build. A templated control's
/// internals are not audited here; that is the control library's contract, and the limit is recorded.
/// </para>
/// </remarks>
public static class OgAccessibilityAudit
{
    private static readonly HashSet<Type> NamedByContent =
    [
        typeof(Button),
        typeof(CheckBox),
        typeof(RadioButton),
        typeof(TabItem),
        typeof(MenuItem),
        typeof(Label),
    ];

    private static readonly HashSet<Type> RequiresName =
    [
        typeof(TextBox),
        typeof(PasswordBox),
        typeof(ComboBox),
        typeof(DataGrid),
        typeof(ListBox),
        typeof(TabControl),
        typeof(Slider),
        typeof(ProgressBar),
    ];

    /// <summary>Interactive controls without an accessible name, each described by control type and text.</summary>
    public static IReadOnlyList<string> UnnamedInteractive(DependencyObject root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var problems = new List<string>();

        foreach (var element in Descendants(root))
        {
            var type = element.GetType();

            if (RequiresName.Contains(type))
            {
                if (string.IsNullOrWhiteSpace(NameOf(element)))
                {
                    problems.Add($"{type.Name} has no accessible name");
                }

                continue;
            }

            if (NamedByContent.Any(candidate => candidate.IsInstanceOfType(element)))
            {
                if (string.IsNullOrWhiteSpace(NameOf(element)) && !HasTextContent(element))
                {
                    problems.Add($"{type.Name} has neither an accessible name nor text content");
                }
            }
        }

        return problems;
    }

    /// <summary>Controls a person cannot reach with Tab, because they are focusable but excluded from the tab order.</summary>
    public static IReadOnlyList<string> NotInTabOrder(DependencyObject root)
    {
        ArgumentNullException.ThrowIfNull(root);

        return Descendants(root)
            .OfType<Control>()
            .Where(control => control.Focusable && !control.IsTabStop)
            .Select(control => control.GetType().Name)
            .ToList();
    }

    /// <summary>The name UI Automation would announce, or an empty string when there is none.</summary>
    public static string NameOf(DependencyObject element)
    {
        var name = AutomationProperties.GetName(element);

        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        return AutomationProperties.GetLabeledBy(element) is not null
               && AutomationProperties.GetLabeledBy(element) is FrameworkElement label
            ? (label as TextBlock)?.Text ?? string.Empty
            : string.Empty;
    }

    private static bool HasTextContent(DependencyObject element) => element switch
    {
        ContentControl { Content: string text } => !string.IsNullOrWhiteSpace(text),
        HeaderedContentControl { Header: string header } => !string.IsNullOrWhiteSpace(header),
        HeaderedItemsControl { Header: string header } => !string.IsNullOrWhiteSpace(header),
        _ => false,
    };

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
        {
            yield return child;

            foreach (var descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }
}
