using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TheTechIdea.Beep.OilandGas.Desktop.Controls.Notifications;

/// <summary>
/// The live toast surface: bottom-right, worst on top, at most five at once. Sticky failures stay until
/// the person dismisses them.
/// </summary>
public partial class ToastHost : UserControl
{
    private const int MaxVisible = 5;

    private readonly ObservableCollection<ToastItem> _items = [];

    public ToastHost()
    {
        InitializeComponent();
        ToastList.ItemsSource = _items;
    }

    /// <summary>Adds a toast to the surface, collapsing an identical one shown within the last five seconds.</summary>
    public void Add(ToastItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var duplicate = _items.FirstOrDefault(existing =>
            !existing.IsDismissed
            && string.Equals(existing.Text, item.Text, StringComparison.Ordinal)
            && string.Equals(existing.Reference, item.Reference, StringComparison.Ordinal));

        if (duplicate is not null)
        {
            _items.Remove(duplicate);
        }

        _items.Insert(0, item);

        while (_items.Count > MaxVisible)
        {
            _items.RemoveAt(_items.Count - 1);
        }

        if (!item.IsSticky)
        {
            var timer = new DispatcherTimer { Interval = item.Duration };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                _items.Remove(item);
            };
            timer.Start();
        }
    }

    public int VisibleCount => _items.Count;

    private void OnDismissClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ToastItem item })
        {
            _items.Remove(item);
        }
    }
}
