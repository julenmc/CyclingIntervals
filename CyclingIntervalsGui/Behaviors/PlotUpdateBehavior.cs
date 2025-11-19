using Avalonia;
using CyclingIntervalsGui.Models;
using ScottPlot.Avalonia;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaColors = Avalonia.Media.Colors;
using NLog;
using CyclingTrainer.SessionAnalyzer.Models;
using System.Collections.ObjectModel;

namespace CyclingIntervalsGui.Behaviors;

/// <summary>
/// AttachedBehavior que actualiza automáticamente el gráfico ScottPlot cuando los datos cambian.
/// Se vincula a la propiedad AltitudeData del ViewModel y redibuja el gráfico reactivamente.
/// Soporta zoom sincronizado y colores personalizados.
/// </summary>
public class PlotUpdateBehavior
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static readonly AttachedProperty<GraphData?> DataSourceProperty =
        AvaloniaProperty.RegisterAttached<PlotUpdateBehavior, AvaPlot, GraphData?>(
            "DataSource",
            null,
            false);

    public static readonly AttachedProperty<List<ClimbData>?> ClimbSourceProperty =
        AvaloniaProperty.RegisterAttached<PlotUpdateBehavior, AvaPlot, List<ClimbData>?>(
            "ClimbSource",
            null,
            false);

    public static readonly AttachedProperty<ObservableCollection<Interval>?> IntervalSourceProperty =
        AvaloniaProperty.RegisterAttached<PlotUpdateBehavior, AvaPlot, ObservableCollection<Interval>?>(
            "IntervalSource",
            null,
            false);

    public static readonly AttachedProperty<bool> ShowClimbsProperty =
        AvaloniaProperty.RegisterAttached<PlotUpdateBehavior, AvaPlot, bool>(
            "ShowClimbs",
            true,
            false);

    public static readonly AttachedProperty<AvaloniaColor> LineColorProperty =
        AvaloniaProperty.RegisterAttached<PlotUpdateBehavior, AvaPlot, AvaloniaColor>(
            "LineColor",
            AvaloniaColors.Black,
            false);

    public static readonly AttachedProperty<bool> ShowXAxisProperty =
        AvaloniaProperty.RegisterAttached<PlotUpdateBehavior, AvaPlot, bool>(
            "ShowXAxis",
            true,
            false);

    // Diccionario para almacenar los límites originales de cada gráfico
    private static readonly Dictionary<AvaPlot, (double minX, double maxX, double minY, double maxY)> _originalLimits = new();
    
    // Diccionario para almacenar los spans de cada gráfico
    private static readonly Dictionary<AvaPlot, List<ScottPlot.Plottables.HorizontalSpan>> _climbSpans = new();
    private static readonly Dictionary<AvaPlot, List<ScottPlot.Plottables.HorizontalSpan>> _intervalSpans = new();
    
    // Diccionario para rastrear los listeners de CollectionChanged de intervals
    private static readonly Dictionary<AvaPlot, System.Collections.Specialized.NotifyCollectionChangedEventHandler> _intervalCollectionListeners = new();

    static PlotUpdateBehavior()
    {
        DataSourceProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnDataSourceChanged(plot, e));
        ClimbSourceProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnClimbsChanged(plot, e));
        IntervalSourceProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnIntervalsChanged(plot, e));
        LineColorProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnColorChanged(plot, e));
        ShowClimbsProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnShowClimbsChanged(plot, e));
        ShowXAxisProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnShowXAxisChanged(plot, e));
    }

    // public static void HandleIntervalClick(ScottPlot.Plottables.Rectangle rectangle)
    // {
    //     foreach ()
    // }

    public static GraphData? GetDataSource(AvaPlot plot)
    {
        return plot.GetValue(DataSourceProperty);
    }

    public static List<ClimbData>? GetClimbSource(AvaPlot plot)
    {
        return plot.GetValue(ClimbSourceProperty);
    }

    public static void SetClimbSource(AvaPlot plot, List<ClimbData>? value)
    {
        plot.SetValue(ClimbSourceProperty, value);
    }

    public static ObservableCollection<Interval>? GetIntervalSource(AvaPlot plot)
    {
        return plot.GetValue(IntervalSourceProperty);
    }

    public static void SetIntervalSource(AvaPlot plot, ObservableCollection<Interval>? value)
    {
        plot.SetValue(IntervalSourceProperty, value);
    }

    public static void SetDataSource(AvaPlot plot, GraphData? value)
    {
        plot.SetValue(DataSourceProperty, value);
    }

    public static AvaloniaColor GetLineColor(AvaPlot plot)
    {
        return plot.GetValue(LineColorProperty);
    }

    public static void SetLineColor(AvaPlot plot, AvaloniaColor value)
    {
        plot.SetValue(LineColorProperty, value);
    }

    public static bool GetShowClimbs(AvaPlot plot)
    {
        return plot.GetValue(ShowClimbsProperty);
    }

    public static void SetShowClimbs(AvaPlot plot, bool value)
    {
        plot.SetValue(ShowClimbsProperty, value);
    }

    public static bool GetShowXAxis(AvaPlot plot)
    {
        return plot.GetValue(ShowXAxisProperty);
    }

    public static void SetShowXAxis(AvaPlot plot, bool value)
    {
        plot.SetValue(ShowXAxisProperty, value);
    }

    public static (double minX, double maxX, double minY, double maxY)? GetOriginalLimits(AvaPlot plot)
    {
        if (_originalLimits.TryGetValue(plot, out var limits))
        {
            return limits;
        }
        return null;
    }

    public static void SetOriginalLimitsForIntervals(AvaPlot plot, double minX, double maxX, double minY, double maxY)
    {
        _originalLimits[plot] = (minX, maxX, minY, maxY);
    }

    private static void OnDataSourceChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        var graphData = GetDataSource(plot);
        if (graphData != null)
        {
            UpdatePlot(plot, graphData, GetLineColor(plot));
        }
    }

    private static void OnClimbsChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        UpdateClimbHighlight(plot);
    }

    private static void OnIntervalsChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        // Remover listener anterior si existe
        if (_intervalCollectionListeners.TryGetValue(plot, out var oldListener))
        {
            var oldCollection = e.OldValue as ObservableCollection<Interval>;
            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= oldListener;
            }
        }

        var newCollection = e.NewValue as ObservableCollection<Interval>;
        if (newCollection != null)
        {
            // Crear nuevo listener
            System.Collections.Specialized.NotifyCollectionChangedEventHandler newListener = 
                (sender, args) => UpdateIntervalHighlight(plot);
            
            // Guardar listener para poder removerlo después
            _intervalCollectionListeners[plot] = newListener;
            
            // Suscribirse a cambios en la colección
            newCollection.CollectionChanged += newListener;
        }

        UpdateIntervalHighlight(plot);
    }

    private static void OnColorChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        var graphData = GetDataSource(plot);
        if (graphData != null)
        {
            UpdatePlot(plot, graphData, GetLineColor(plot));
        }
    }

    private static void OnShowClimbsChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        UpdateClimbHighlight(plot);
    }
    
    private static void OnShowXAxisChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        // Hide axis label and tick
        if (!GetShowXAxis(plot))
        {
            plot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
            plot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
            plot.Plot.Axes.Bottom.MinorTickStyle.Length = 0;
        }
    }

    private static void UpdatePlot(AvaPlot plot, GraphData? graphData, AvaloniaColor lineColor)
    {
        if (plot?.Plot == null || graphData?.Values == null || graphData.Values.Count == 0)
        {
            return;
        }

        Logger.Info($"Updating plot {plot.Name}");
        try
        {
            plot.Plot.Clear();

            // Limpiar los diccionarios de spans al hacer Clear
            ClearSpans(plot);

            double[] xs = graphData.Values.Select(d => d.Date.ToOADate()).ToArray();
            double[] ys = graphData.Values.Select(d => (double)d.Value).ToArray();

            // Convertir Color de Avalonia a ScottPlot.Color
            var plotColor = new ScottPlot.Color(lineColor.R, lineColor.G, lineColor.B, lineColor.A);

            // Agregar scatter plot (puntos pequeños conectados como línea visual)
            var scatter = plot.Plot.Add.Scatter(xs, ys);
            scatter.Color = plotColor;
            scatter.LineWidth = 2;
            scatter.MarkerSize = 0; // Sin marcadores, solo línea

            // Mantener los ejes verticales fijos (izquierda)
            if (graphData.MaxValue > graphData.MinValue)
            {
                plot.Plot.Axes.Left.Min = graphData.MinValue;
                plot.Plot.Axes.Left.Max = graphData.MaxValue;
            }

            // Auto-scale solo el eje horizontal
            if (xs.Length > 0)
            {
                plot.Plot.Axes.Bottom.Min = xs.Min();
                plot.Plot.Axes.Bottom.Max = xs.Max();
            }
            plot.Plot.Axes.DateTimeTicksBottom();

            // Guardar los límites originales para que SyncZoomBehavior los use
            if (xs.Length > 0)
            {
                _originalLimits[plot] = (
                    xs.Min(),
                    xs.Max(),
                    graphData.MinValue,
                    graphData.MaxValue
                );
            }

            // Hide axis label and tick
            if (!GetShowXAxis(plot))
            {
                plot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
                plot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
                plot.Plot.Axes.Bottom.MinorTickStyle.Length = 0;
            }

            plot.Refresh();
        }
        catch (Exception ex)
        {
            Logger.Error($"Error actualizando el gráfico: {ex.Message}");
        }
    }

    private static void UpdateClimbHighlight(AvaPlot plot)
    {
        if (plot?.Plot == null)
        {
            return;
        }

        // Limpiar spans existentes de climbs
        ClearClimbSpans(plot);

        var climbs = GetClimbSource(plot);
        var showClimbs = GetShowClimbs(plot);
        var plotColor = GetLineColor(plot);
        var highlightColor = new ScottPlot.Color(plotColor.R, plotColor.G, plotColor.B, plotColor.A);

        if (!showClimbs || climbs == null || climbs.Count == 0)
        {
            plot.Refresh();
            return;
        }

        Logger.Debug($"Updating {climbs.Count} climb highlights");

        foreach (ClimbData climb in climbs)
        {          
            double start = climb.Interval.StartTime.ToOADate();
            double end = climb.Interval.EndTime.ToOADate();

            var span = plot.Plot.Add.HorizontalSpan(
                x1: start,
                x2: end,
                color: highlightColor.WithAlpha(0.3)
            );

            // Guardar referencia al span
            if (!_climbSpans.ContainsKey(plot))
            {
                _climbSpans[plot] = new List<ScottPlot.Plottables.HorizontalSpan>();
            }
            _climbSpans[plot].Add(span);
        }

        plot.Refresh();
    }

    private static void UpdateIntervalHighlight(AvaPlot plot)
    {
        if (plot?.Plot == null)
        {
            return;
        }

        // Limpiar spans existentes de intervalos
        ClearIntervalSpans(plot);

        var intervals = GetIntervalSource(plot);
        var plotColor = GetLineColor(plot);
        var highlightColor = new ScottPlot.Color(plotColor.R, plotColor.G, plotColor.B, plotColor.A);

        if (intervals == null || intervals.Count == 0)
        {
            plot.Refresh();
            return;
        }

        Logger.Debug($"Updating {intervals.Count} interval highlights");

        foreach (Interval interval in intervals)
        {
            double start = interval.StartTime.ToOADate();
            double end = interval.EndTime.ToOADate();

            var span = plot.Plot.Add.HorizontalSpan(
                x1: start,
                x2: end,
                color: highlightColor.WithAlpha(0.3)
            );

            // Guardar referencia al span
            if (!_intervalSpans.ContainsKey(plot))
            {
                _intervalSpans[plot] = new List<ScottPlot.Plottables.HorizontalSpan>();
            }
            _intervalSpans[plot].Add(span);
        }

        plot.Refresh();
    }

    /// <summary>
    /// Limpia los spans de climbs del gráfico
    /// </summary>
    private static void ClearClimbSpans(AvaPlot plot)
    {
        if (_climbSpans.TryGetValue(plot, out var spans))
        {
            foreach (var span in spans)
            {
                plot.Plot.Remove(span);
            }
            spans.Clear();
        }
    }

    /// <summary>
    /// Limpia los spans de intervalos del gráfico
    /// </summary>
    private static void ClearIntervalSpans(AvaPlot plot)
    {
        if (_intervalSpans.TryGetValue(plot, out var spans))
        {
            foreach (var span in spans)
            {
                plot.Plot.Remove(span);
            }
            spans.Clear();
        }
    }

    /// <summary>
    /// Limpia todos los spans del gráfico
    /// </summary>
    private static void ClearSpans(AvaPlot plot)
    {
        ClearClimbSpans(plot);
        ClearIntervalSpans(plot);
    }

    /// <summary>
    /// Limpia todos los datos asociados al plot (útil cuando se destruye el control)
    /// </summary>
    public static void CleanupPlot(AvaPlot plot)
    {
        ClearSpans(plot);
        _climbSpans.Remove(plot);
        _intervalSpans.Remove(plot);
        _originalLimits.Remove(plot);
    }
}