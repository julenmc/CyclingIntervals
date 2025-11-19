using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ScottPlot;

namespace CyclingIntervalsGui.Views;

public partial class IntervalsUserControl : UserControl
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public static readonly StyledProperty<ICommand?> RectangleClickedCommandProperty =
        AvaloniaProperty.Register<IntervalsUserControl, ICommand?>(nameof(RectangleClickedCommand));

    public ICommand? RectangleClickedCommand
    {
        get => GetValue(RectangleClickedCommandProperty);
        set => SetValue(RectangleClickedCommandProperty, value);
    }

    public IntervalsUserControl()
    {
        InitializeComponent();

        this.Loaded += (s, e) =>
        {
            var intervals = this.Find<ScottPlot.Avalonia.AvaPlot>("Intervals");
            if (intervals != null)
            {
                intervals.PointerPressed += Plot_PointerPressed;
            }
            else
            {
                Logger.Error("No se pudo encontrar el control Intervals");
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Plot_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        try
        {
            var intervals = this.Find<ScottPlot.Avalonia.AvaPlot>("Intervals");
            if (intervals?.Plot == null) return;
            
            var point = e.GetPosition(intervals);
            var coordinates = intervals.Plot.GetCoordinates(new Pixel((float)point.X, (float)point.Y));
            
            CheckClickOnRectangles(intervals, coordinates.X, coordinates.Y);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Error on pointer press: {ex.Message}");
        }   
    }

    private void CheckClickOnRectangles(ScottPlot.Avalonia.AvaPlot intervals, double x, double y)
    {
        if (intervals?.Plot == null)
            return;
            
        var rects = intervals.Plot.GetPlottables<ScottPlot.Plottables.Rectangle>();

        foreach (var rect in rects)
        {
            if (IsInsideRectangle(rect, x, y))
            {
                OnRectangleClicked(rect);
                break;
            }
        }
    }

    private bool IsInsideRectangle(ScottPlot.Plottables.Rectangle rect, double x, double y)
    {
        double minX = Math.Min(rect.X1, rect.X2);
        double maxX = Math.Max(rect.X1, rect.X2);
        double minY = Math.Min(rect.Y1, rect.Y2);
        double maxY = Math.Max(rect.Y1, rect.Y2);

        return x >= minX && x <= maxX && y >= minY && y <= maxY;
    }
    
    private void OnRectangleClicked(ScottPlot.Plottables.Rectangle rect)
    {
        if (RectangleClickedCommand?.CanExecute(rect) == true)
        {
            RectangleClickedCommand.Execute(rect);
        }
    }
}