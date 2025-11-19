using Avalonia;
using CyclingIntervalsGui.Models;
using ScottPlot.Avalonia;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaColors = Avalonia.Media.Colors;
using CyclingTrainer.SessionAnalyzer.Models;
using NLog;

namespace CyclingIntervalsGui.Behaviors;

/// <summary>
/// AttachedBehavior que actualiza automáticamente el gráfico de intervalos.
/// Sincroniza su zoom con el resto de los gráficos mediante PlotSyncBehavior.
/// </summary>
public class IntervalsUpdateBehavior
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static readonly AttachedProperty<List<Interval>?> IntervalsSourceProperty =
        AvaloniaProperty.RegisterAttached<IntervalsUpdateBehavior, AvaPlot, List<Interval>?>(
            "IntervalsSource",
            null,
            false);

    static IntervalsUpdateBehavior()
    {
        IntervalsSourceProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnIntervalsChanged(plot, e));
    }

    private static readonly Dictionary<AvaPlot, List<ScottPlot.Plottables.Rectangle>> _intervalRectangles = new();

    private static readonly AvaloniaColor _baseColor = AvaloniaColors.LightGreen;
    public static List<Interval>? GetIntervalsSource(AvaPlot plot)
    {
        return plot.GetValue(IntervalsSourceProperty);
    }

    public static void SetIntervalsSource(AvaPlot plot, List<Interval>? value)
    {
        plot.SetValue(IntervalsSourceProperty, value);
    }

    private static void OnIntervalsChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        UpdateIntervalHighlight(plot);
    }

    private static void UpdateIntervalHighlight(AvaPlot plot)
    {
        if (plot?.Plot == null)
        {
            return;
        }

        // Limpiar spans existentes de intervalos
        ClearIntervalRectangles(plot);

        var intervals = GetIntervalsSource(plot);

        if (intervals == null || intervals.Count == 0)
        {
            plot.Refresh();
            return;
        }

        Logger.Debug($"Updating {intervals.Count} intervals");

        // Calcular el rango de tiempo para los intervalos
        double minTime = double.MaxValue;
        double maxTime = double.MinValue;

        // Obtener el rango de niveles
        int maxLevel = GetMaxLevel(intervals);

        AddRectangles(intervals, plot, 0, maxLevel, ref minTime, ref maxTime);

        // Configurar ejes
        // Eje Y: de 0 al número de niveles
        plot.Plot.Axes.Left.Min = 0;
        plot.Plot.Axes.Left.Max = maxLevel;

        // Eje X: rango de tiempo de los intervalos
        if (minTime < maxTime && minTime != double.MaxValue)
        {
            plot.Plot.Axes.Bottom.Min = minTime;
            plot.Plot.Axes.Bottom.Max = maxTime;

            // Guardar los límites originales para sincronización de zoom
            PlotUpdateBehavior.SetOriginalLimitsForIntervals(plot, minTime, maxTime, 0, maxLevel);
        }

        plot.Plot.Axes.DateTimeTicksBottom();

        // Hide axis label and tick del eje X
        plot.Plot.Axes.Bottom.TickLabelStyle.IsVisible = false;
        plot.Plot.Axes.Bottom.MajorTickStyle.Length = 0;
        plot.Plot.Axes.Bottom.MinorTickStyle.Length = 0;

        plot.Refresh();
    }

    private static int GetMaxLevel(List<Interval> intervals)
    {
        int maxLevel = 0;
        foreach (Interval interval in intervals)
        {
            int levels = GetIntervalsDepth(interval.Intervals) + 1;
            if (levels > maxLevel) maxLevel = levels;
        }
        return maxLevel;
    }

    private static int GetIntervalsDepth(List<Interval> intervals)
    {
        int maxLevel = 0;
        foreach (Interval interval in intervals)
        {
            int levels = GetIntervalsDepth(interval.Intervals) + 1;
            if (levels > maxLevel) maxLevel = levels;
        }
        return maxLevel;
    }

    private static void AddRectangles(List<Interval> intervals, AvaPlot plot, int currentLevel, int maxLevel, ref double minTime, ref double maxTime)
    {
        foreach (Interval interval in intervals)
        {
            double start = interval.StartTime.ToOADate();
            double end = interval.EndTime.ToOADate();

            if (start < minTime) minTime = start;
            if (end > maxTime) maxTime = end;

            var rectangle = plot.Plot.Add.Rectangle(left: start,
                                                    right: end,
                                                    bottom: currentLevel,
                                                    top: currentLevel + 1);
            ScottPlot.Color color = GetRectangleColor((double)currentLevel / (double)maxLevel);
            rectangle.FillColor = color;
            rectangle.LineColor = ScottPlot.Colors.Black;

            // Guardar referencia al rectángulo
            if (!_intervalRectangles.ContainsKey(plot))
            {
                _intervalRectangles[plot] = new List<ScottPlot.Plottables.Rectangle>();
            }
            _intervalRectangles[plot].Add(rectangle);
            AddRectangles(interval.Intervals, plot, currentLevel + 1, maxLevel, ref minTime, ref maxTime);
        }
    }

    private static ScottPlot.Color GetRectangleColor(double amount)
    {
        // amount: 0 = sin cambio, 1 = negro total
        return new ScottPlot.Color(
            (byte)(_baseColor.R * (1 - amount)),
            (byte)(_baseColor.G * (1 - amount)),
            (byte)(_baseColor.B * (1 - amount)),
            _baseColor.A
        );
    }

    private static void ClearIntervalRectangles(AvaPlot plot)
    {
        if (_intervalRectangles.TryGetValue(plot, out var rectangles))
        {
            foreach (var rectangle in rectangles)
            {
                plot.Plot.Remove(rectangle);
            }
            rectangles.Clear();
        }
    }
}