using Avalonia;
using ScottPlot.Avalonia;

namespace CyclingIntervalsGui.Behaviors;

/// <summary>
/// Behavior que sincroniza el zoom horizontal entre múltiples gráficos.
/// Mantiene los ejes verticales fijos mientras permite pan/zoom horizontal sincronizado.
/// Bloquea zoom vertical y establece límites máximos de zoom-out.
/// </summary>
public class SyncZoomBehavior
{
    private static readonly List<AvaPlot> _linkedPlots = new();
    private static bool _isUpdating = false;

    public static readonly AttachedProperty<bool> IsSyncedProperty =
        AvaloniaProperty.RegisterAttached<SyncZoomBehavior, AvaPlot, bool>(
            "IsSynced",
            false,
            false);

    static SyncZoomBehavior()
    {
        IsSyncedProperty.Changed.AddClassHandler<AvaPlot>((plot, e) => OnSyncedChanged(plot, e));
    }

    public static bool GetIsSynced(AvaPlot plot)
    {
        return plot.GetValue(IsSyncedProperty);
    }

    public static void SetIsSynced(AvaPlot plot, bool value)
    {
        plot.SetValue(IsSyncedProperty, value);
    }

    private static void OnSyncedChanged(AvaPlot plot, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            _linkedPlots.Add(plot);

            // Suscribirse SOLO a eventos de zoom (rueda) y pan (click+arrastre)
            // NO incluir PointerMoved para evitar actualizaciones continuas
            plot.PointerWheelChanged += (s, e2) => OnPlotInteraction(plot);
            plot.PointerPressed += (s, e2) => OnPlotInteraction(plot);
            plot.PointerReleased += (s, e2) => OnPlotInteraction(plot);
        }
        else if (e.NewValue is false)
        {
            _linkedPlots.Remove(plot);
        }
    }

    private static void OnPlotInteraction(AvaPlot sourcePlot)
    {
        if (_isUpdating || sourcePlot?.Plot == null)
            return;

        try
        {
            _isUpdating = true;

            // Obtener los límites originales del gráfico fuente
            var sourceOriginalLimits = PlotUpdateBehavior.GetOriginalLimits(sourcePlot);
            if (sourceOriginalLimits == null)
                return; // Los límites no están listos todavía

            var originalLimits = sourceOriginalLimits.Value;

            // Obtener el rango horizontal del gráfico fuente
            var sourceXMin = sourcePlot.Plot.Axes.Bottom.Min;
            var sourceXMax = sourcePlot.Plot.Axes.Bottom.Max;

            // Bloquear zoom vertical: restaurar a los valores originales
            sourcePlot.Plot.Axes.Left.Min = originalLimits.minY;
            sourcePlot.Plot.Axes.Left.Max = originalLimits.maxY;

            // Aplicar límites máximos de zoom-out (no alejarse más que los datos originales)
            var constrainedXMin = Math.Max(sourceXMin, originalLimits.minX);
            var constrainedXMax = Math.Min(sourceXMax, originalLimits.maxX);

            // Asegurar que el rango mínimo sea válido
            if (constrainedXMax <= constrainedXMin)
            {
                constrainedXMin = originalLimits.minX;
                constrainedXMax = originalLimits.maxX;
            }

            sourcePlot.Plot.Axes.Bottom.Min = constrainedXMin;
            sourcePlot.Plot.Axes.Bottom.Max = constrainedXMax;
            sourcePlot.Refresh();

            // Aplicar el mismo rango a todos los otros gráficos
            foreach (var plot in _linkedPlots)
            {
                if (plot != sourcePlot && plot?.Plot != null)
                {
                    // Obtener límites originales del gráfico actual
                    var plotOriginalLimits = PlotUpdateBehavior.GetOriginalLimits(plot);
                    if (plotOriginalLimits == null)
                        continue;

                    var originalLimitsForPlot = plotOriginalLimits.Value;

                    // Bloquear zoom vertical en todos los gráficos
                    plot.Plot.Axes.Left.Min = originalLimitsForPlot.minY;
                    plot.Plot.Axes.Left.Max = originalLimitsForPlot.maxY;

                    // Aplicar mismo zoom horizontal sincronizado
                    plot.Plot.Axes.Bottom.Min = constrainedXMin;
                    plot.Plot.Axes.Bottom.Max = constrainedXMax;
                    plot.Refresh();
                }
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }
}

