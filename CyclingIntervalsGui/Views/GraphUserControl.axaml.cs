using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaColors = Avalonia.Media.Colors;
using CyclingIntervalsGui.Models;

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

    public static readonly StyledProperty<GraphData?> DataProperty =
        AvaloniaProperty.Register<GraphUserControl, GraphData?>(nameof(Data));

    public GraphData? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly StyledProperty<AvaloniaColor> PlotColorProperty =
        AvaloniaProperty.Register<GraphUserControl, AvaloniaColor>(
            nameof(PlotColor),
            defaultValue: AvaloniaColors.DodgerBlue);

    public AvaloniaColor PlotColor
    {
        get => GetValue(PlotColorProperty);
        set => SetValue(PlotColorProperty, value);
    }

    public static readonly StyledProperty<bool> ShowAxisProperty =
        AvaloniaProperty.Register<GraphUserControl, bool>(
            nameof(ShowAxis));

    public bool ShowAxis
    {
        get => GetValue(ShowAxisProperty);
        set => SetValue(ShowAxisProperty, value);
    }

    // public static readonly StyledProperty<bool> ShowClimbsProperty =
    //     AvaloniaProperty.Register<GraphUserControl, bool>(nameof(ShowClimbs));

    // public bool ShowClimbs
    // {
    //     get => GetValue(ShowClimbsProperty);
    //     set => SetValue(ShowClimbsProperty, value);
    // }

    // public static readonly StyledProperty<bool> ShowIntervalsProperty =
    //     AvaloniaProperty.Register<GraphUserControl, bool>(nameof(ShowIntervals));

    // public bool ShowIntervals
    // {
    //     get => GetValue(ShowIntervalsProperty);
    //     set => SetValue(ShowIntervalsProperty, value);
    // }
}
