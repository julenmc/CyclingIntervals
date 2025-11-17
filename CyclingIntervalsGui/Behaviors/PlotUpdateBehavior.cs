using Avalonia;
using Avalonia.Media;
using CyclingIntervalsGui.Models;
using ScottPlot;
using ScottPlot.Avalonia;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaColors = Avalonia.Media.Colors;

namespace CyclingIntervalsGui.Behaviors;

/// <summary>
/// AttachedBehavior que actualiza automáticamente el gráfico ScottPlot cuando los datos cambian.
/// Se vincula a la propiedad AltitudeData del ViewModel y redibuja el gráfico reactivamente.
/// Soporta zoom sincronizado y colores personalizados.
/// </summary>
public class PlotUpdateBehavior
{
    public static readonly AttachedProperty<GraphData?> DataSourceProperty =
        AvaloniaProperty.RegisterAttached<PlotUpdateBehavior, AvaPlot, GraphData?>(
            "DataSource",
            null,
            false);

    public static readonly AttachedProperty<AvaloniaColor> LineColorProperty =
        AvaloniaProperty.RegisterAttached<PlotUpdateBehavior, AvaPlot, AvaloniaColor>(
            "LineColor",
            AvaloniaColors.Blue,
            false);

    // Diccionario para almacenar los límites originales de cada gráfico
    private static readonly Dictionary<AvaPlot, (double minX, double maxX, double minY, double maxY)> _originalLimits = new();

    static PlotUpdateBehavior()
    {
        DataSourceProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnDataSourceChanged(plot, e));
        LineColorProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnColorChanged(plot, e));
    }

    public static GraphData? GetDataSource(AvaPlot plot)
    {
        return plot.GetValue(DataSourceProperty);
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

    public static (double minX, double maxX, double minY, double maxY)? GetOriginalLimits(AvaPlot plot)
    {
        if (_originalLimits.TryGetValue(plot, out var limits))
        {
            return limits;
        }
        return null;
    }

    private static void OnDataSourceChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        var graphData = GetDataSource(plot);
        if (graphData != null)
        {
            UpdatePlot(plot, graphData, GetLineColor(plot));
        }
    }

    private static void OnColorChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        var graphData = GetDataSource(plot);
        if (graphData != null)
        {
            UpdatePlot(plot, graphData, GetLineColor(plot));
        }
    }

    public static void UpdatePlot(AvaPlot plot, GraphData? graphData, AvaloniaColor lineColor)
    {
        if (plot?.Plot == null || graphData?.Values == null || graphData.Values.Count == 0)
        {
            return;
        }

        try
        {
            plot.Plot.Clear();

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

            plot.Refresh();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error actualizando el gráfico: {ex.Message}");
        }
    }
}



