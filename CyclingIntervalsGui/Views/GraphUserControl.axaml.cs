using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CyclingIntervalsGui.Views;

public partial class GraphUserControl : UserControl
{
    public GraphUserControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly StyledProperty<bool> ShowClimbsProperty =
        AvaloniaProperty.Register<GraphUserControl, bool>(nameof(ShowClimbs));

    public bool ShowClimbs
    {
        get => GetValue(ShowClimbsProperty);
        set => SetValue(ShowClimbsProperty, value);
    }

    public static readonly StyledProperty<bool> ShowIntervalsProperty =
        AvaloniaProperty.Register<GraphUserControl, bool>(nameof(ShowIntervals));

    public bool ShowIntervals
    {
        get => GetValue(ShowIntervalsProperty);
        set => SetValue(ShowIntervalsProperty, value);
    }
}
