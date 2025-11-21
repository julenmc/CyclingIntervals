using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaColors = Avalonia.Media.Colors;
using CyclingIntervalsGui.Models;
using CyclingIntervalsGui.ViewModels;
using Avalonia.Input;
using ScottPlot;
using CyclingTrainer.SessionAnalyzer.Models;
using ScottPlot.Avalonia;

namespace CyclingIntervalsGui.Views;

public partial class GraphUserControl : UserControl
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public GraphUserControl()
    {
        InitializeComponent();

        this.Loaded += (s, e) =>
        {
            var graph = this.Find<AvaPlot>("Graph");
            if (graph != null)
            {
                graph.PointerPressed += Graph_PointerPressed;
            }
            else
            {
                Logger.Error("No se pudo encontrar el control Graph");
            }
        };
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

    private void Graph_PointerPressed(object? sender, PointerEventArgs e)
    {
        var graph = this.Find<AvaPlot>("Graph");
        if (graph == null)
        {
            Logger.Error("No se pudo encontrar el control Graph");
            return;
        }

        // Hide if open
        var intervalInfoPopup = this.Find<Avalonia.Controls.Primitives.Popup>("IntervalInfoPopup");
        if (intervalInfoPopup != null)
        {
            if (intervalInfoPopup.IsOpen)
            {
                Logger.Debug("Closing interval popup");
                intervalInfoPopup.IsOpen = false;
            }
        }
        

        var climbInfoPopup = this.Find<Avalonia.Controls.Primitives.Popup>("ClimbInfoPopup");
        if (climbInfoPopup != null)
        {
            if (climbInfoPopup.IsOpen)
            {
                Logger.Debug("Closing climb popup");
                climbInfoPopup.IsOpen = false;
            }
        }

        var mouse = e.GetPosition(graph);
        var coordinates = graph.Plot.GetCoordinates(new Pixel((float)mouse.X, (float)mouse.Y));

        bool intervalFound = false;
        bool climbFound = false;
        // Obtener spans
        foreach (var span in graph.Plot.GetPlottables().OfType<ScottPlot.Plottables.HorizontalSpan>())
        {
            if (coordinates.X > span.X1 && coordinates.X < span.X2)
            {
                if (!intervalFound) intervalFound = CheckIntervalSpan(span, graph);
                if (!climbFound) climbFound = CheckClimbSpan(span, graph);
            }
            if (intervalFound && climbFound) return;
        }
    }

    private bool CheckIntervalSpan(ScottPlot.Plottables.HorizontalSpan span, AvaPlot graph)
    {
        var vm = this.DataContext as GraphViewModel;
        if (vm == null)
        {
            Logger.Error("No se pudo encontrar el ViewModel");
            return false;
        }
        Interval? interval = FindInterval(vm.IntervalsList, span);
        if (interval == null)
        {
            return false;   // Interval not found, could be a climb
        }
        
        ShowIntervalTooltip(interval, graph);
        return true;
    }

    private void ShowIntervalTooltip(Interval info, AvaPlot graph)
    {
        var popUpTitle = this.Find<TextBlock>("IntervalPopupTitle");
        var infoPopup = this.Find<Avalonia.Controls.Primitives.Popup>("IntervalInfoPopup");
        var popupDetails = this.Find<TextBlock>("IntervalPopupDetails");
        if (popUpTitle == null || popupDetails == null || infoPopup == null) return;

        popUpTitle.Text = "Interval";
        TimeSpan time = TimeSpan.FromSeconds(info.TimeDiff);
        popupDetails.Text = $"Time: {time.ToString(@"mm\:ss")} ({info.StartTime.TimeOfDay}-{info.EndTime.TimeOfDay})\r\nAvrPower: {info.AveragePower.ToString("0.0")} W";
        infoPopup.PlacementTarget = graph;
        infoPopup.IsOpen = true;
        Logger.Debug("Showing interval popup");
    }

    private static Interval? FindInterval(IEnumerable<Interval> intervals, ScottPlot.Plottables.HorizontalSpan span)
    {
        foreach (Interval interval in intervals)
        {
            if (IsInterval(span, interval)) return interval;
            if (interval.Intervals.Count != 0)
            {
                Interval? subInterval = FindInterval(interval.Intervals, span);
                if (subInterval != null) return subInterval;
            }
        }
        return null;
    }

    private static bool IsInterval(ScottPlot.Plottables.HorizontalSpan span, Interval interval)
    {
        return interval.StartTime == DateTime.FromOADate(span.X1) &&
                interval.EndTime == DateTime.FromOADate(span.X2);
    }

    private bool CheckClimbSpan(ScottPlot.Plottables.HorizontalSpan span, AvaPlot graph)
    {
        var vm = this.DataContext as GraphViewModel;
        if (vm == null || vm.ClimbsList == null)
        {
            Logger.Error("No se pudo encontrar el ViewModel");
            return false;
        }
        ClimbData? climb = FindClimb(vm.ClimbsList, span);
        if (climb == null)
        {
            return false;   // Climb not found, could be an interval
        }

        ShowClimbTooltip(climb, graph);
        return true;
    }

    private void ShowClimbTooltip(ClimbData info, AvaPlot graph)
    {
        var popUpTitle = this.Find<TextBlock>("ClimbPopupTitle");
        var infoPopup = this.Find<Avalonia.Controls.Primitives.Popup>("ClimbInfoPopup");
        var popupDetails = this.Find<TextBlock>("ClimbPopupDetails");
        if (popUpTitle == null || popupDetails == null || infoPopup == null) return;

        popUpTitle.Text = "Climb";
        TimeSpan time = TimeSpan.FromSeconds(info.Interval.TimeDiff);
        popupDetails.Text = $"Lenght: {info.TotalLength} m ({((float)info.StartLength / 1000).ToString("0.00")}-{((float)info.EndLength / 1000).ToString("0.00")})\r\n" +
                            $"Slope: {info.AverageSlope}%, Max: {info.MaxSlope}%\r\n" +
                            $"Total climb: {info.TotalClimb} m\r\n" +
                            $"Time: {time.ToString(@"mm\:ss")} ({info.Interval.StartTime.TimeOfDay}-{info.Interval.EndTime.TimeOfDay})\r\n" +
                            $"AvrPower: {info.Interval.AveragePower.ToString("0.0")} W";
        infoPopup.PlacementTarget = graph;
        infoPopup.IsOpen = true;
        Logger.Debug("Showing climb popup");
    }

    private static ClimbData? FindClimb(IEnumerable<ClimbData> climbs, ScottPlot.Plottables.HorizontalSpan span)
    {
        foreach (ClimbData climb in climbs)
        {
            if (IsInterval(span, climb.Interval)) return climb;
        }
        return null;
    }
}
