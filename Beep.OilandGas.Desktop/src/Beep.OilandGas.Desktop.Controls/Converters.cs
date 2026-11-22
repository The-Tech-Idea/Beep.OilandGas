using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TheTechIdea.Beep.OilandGas.Desktop.Controls;

/// <summary>
/// Turns a theme token key into the brush it names. Views bind a token name, never a colour, so the
/// single colour authority stays in <c>Themes/Tokens.xaml</c> and a skin change is picked up on redraw.
/// </summary>
public sealed class TokenBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string key && Application.Current is { } app)
        {
            if (app.TryFindResource(key) is Brush brush)
            {
                return brush;
            }
        }

        return Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>True → Visible, false → Collapsed.</summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
