using System.Windows;
using System.Windows.Controls;

namespace TheTechIdea.Beep.OilandGas.Desktop.Controls;

/// <summary>
/// Hosts one feature page and draws the chrome every page shares: title, context line, status badge,
/// command bar and content area. A feature never draws its own header.
/// </summary>
public class PageFrame : ContentControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageFrame),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ContextTextProperty =
        DependencyProperty.Register(nameof(ContextText), typeof(string), typeof(PageFrame),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(PageFrame),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ToolbarProperty =
        DependencyProperty.Register(nameof(Toolbar), typeof(object), typeof(PageFrame),
            new PropertyMetadata(null));

    static PageFrame()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PageFrame),
            new FrameworkPropertyMetadata(typeof(PageFrame)));
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string ContextText
    {
        get => (string)GetValue(ContextTextProperty);
        set => SetValue(ContextTextProperty, value);
    }

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    /// <summary>The command bar, supplied by the page.</summary>
    public object? Toolbar
    {
        get => GetValue(ToolbarProperty);
        set => SetValue(ToolbarProperty, value);
    }
}
