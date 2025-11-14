using Avalonia;
using CyclingIntervalsGui.Models;
using ScottPlot.Avalonia;

namespace CyclingIntervalsGui.Behaviors;

/// <summary>
/// AttachedBehavior que actualiza automáticamente el gráfico ScottPlot cuando los datos cambian.
/// Se vincula a la propiedad AltitudeData del ViewModel y redibuja el gráfico reactivamente.
/// 
/// Patrón MVVM: Cuando el ViewModel notifica que AltitudeData ha cambiado,
/// este behavior reacciona y actualiza la visualización automáticamente.
/// </summary>
public class PlotUpdateBehavior
{
    public static readonly AttachedProperty<GraphData?> DataSourceProperty =
        AvaloniaProperty.RegisterAttached<PlotUpdateBehavior, AvaPlot, GraphData?>(
            "DataSource",
            null,
            false);

    static PlotUpdateBehavior()
    {
        DataSourceProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnDataSourceChanged(plot, e));
    }

    public static GraphData? GetDataSource(AvaPlot plot)
    {
        return plot.GetValue(DataSourceProperty);
    }

    public static void SetDataSource(AvaPlot plot, GraphData? value)
    {
        plot.SetValue(DataSourceProperty, value);
    }

    private static void OnDataSourceChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is GraphData graphData)
        {
            UpdatePlot(plot, graphData);
        }
    }

    public static void UpdatePlot(AvaPlot plot, GraphData? graphData)
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

            plot.Plot.Add.Scatter(xs, ys);
            plot.Plot.Axes.AutoScale();
            plot.Refresh();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error actualizando el gráfico: {ex.Message}");
        }
    }
}



