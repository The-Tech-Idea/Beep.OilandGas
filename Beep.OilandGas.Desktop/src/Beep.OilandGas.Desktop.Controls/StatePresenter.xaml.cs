using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TheTechIdea.Beep.OilandGas.Desktop.Controls;

/// <summary>
/// Renders one of the five page states around the page content. The failed state always shows the
/// failure reference and never shows exception text.
/// </summary>
public partial class StatePresenter : UserControl
{
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(OgViewState), typeof(StatePresenter),
            new PropertyMetadata(OgViewState.Ready, OnStateChanged));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(StatePresenter),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ReferenceProperty =
        DependencyProperty.Register(nameof(Reference), typeof(string), typeof(StatePresenter),
            new PropertyMetadata(null, OnStateChanged));

    public static readonly DependencyProperty ActionTextProperty =
        DependencyProperty.Register(nameof(ActionText), typeof(string), typeof(StatePresenter),
            new PropertyMetadata(string.Empty, OnStateChanged));

    public static readonly DependencyProperty ActionCommandProperty =
        DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(StatePresenter),
            new PropertyMetadata(null));

    public StatePresenter()
    {
        InitializeComponent();
        Apply();
    }

    public OgViewState State
    {
        get => (OgViewState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>The failure reference shown in the failed state. Never exception text.</summary>
    public string? Reference
    {
        get => (string?)GetValue(ReferenceProperty);
        set => SetValue(ReferenceProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((StatePresenter)d).Apply();

    private void Apply()
    {
        if (ContentHost is null || StateHost is null)
        {
            return;
        }

        ContentHost.Visibility = State == OgViewState.Ready ? Visibility.Visible : Visibility.Collapsed;
        StateHost.Visibility = State == OgViewState.Ready ? Visibility.Collapsed : Visibility.Visible;

        if (State == OgViewState.Ready)
        {
            return;
        }

        StateTitle.Text = TitleForState(State);
        StateMessage.Text = Message ?? string.Empty;
        StateMessage.Visibility = string.IsNullOrWhiteSpace(Message) ? Visibility.Collapsed : Visibility.Visible;

        StateReference.Text = string.IsNullOrWhiteSpace(Reference) ? string.Empty : $"Reference: {Reference}";
        StateReference.Visibility = string.IsNullOrWhiteSpace(Reference) ? Visibility.Collapsed : Visibility.Visible;

        StateAction.Visibility = string.IsNullOrWhiteSpace(ActionText) ? Visibility.Collapsed : Visibility.Visible;

        StateGlyph.Text = GlyphForState(State);
        StateGlyph.Foreground = BrushForState(State);
    }

    private static string TitleForState(OgViewState state) => state switch
    {
        OgViewState.Ready => string.Empty,
        OgViewState.Empty => "Nothing to show",
        OgViewState.Loading => "Loading",
        OgViewState.Failed => "Something went wrong",
        OgViewState.Denied => "Not available to you",
        OgViewState.Unsaved => "Unsaved changes",
    };

    private static string GlyphForState(OgViewState state) => state switch
    {
        OgViewState.Ready => "\uE73E",
        OgViewState.Empty => "\uE7BA",
        OgViewState.Loading => "\uE895",
        OgViewState.Failed => "\uEA39",
        OgViewState.Denied => "\uE72E",
        OgViewState.Unsaved => "\uE7BA",
    };

    private System.Windows.Media.Brush BrushForState(OgViewState state)
    {
        var key = state switch
        {
            OgViewState.Ready => "BeepTextPrimaryBrush",
            OgViewState.Empty => "BeepTextSecondaryBrush",
            OgViewState.Loading => "BeepInfoBrush",
            OgViewState.Failed => "BeepDangerBrush",
            OgViewState.Denied => "BeepWarningBrush",
            OgViewState.Unsaved => "BeepWarningBrush",
        };

        return TryFindResource(key) as System.Windows.Media.Brush
               ?? System.Windows.Media.Brushes.Gray;
    }

    private void OnActionClick(object sender, RoutedEventArgs e)
    {
        if (ActionCommand?.CanExecute(null) == true)
        {
            ActionCommand.Execute(null);
        }
    }
}
