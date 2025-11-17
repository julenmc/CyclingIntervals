using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CyclingIntervalsGui.Views;

public partial class ZonesUserControl : UserControl
{
    public ZonesUserControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
