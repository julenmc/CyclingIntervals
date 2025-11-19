using AvaloniaColor = Avalonia.Media.Color;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CyclingIntervalsGui.Repositories;
using CyclingIntervalsGui.Services;
using CyclingTrainer.SessionAnalyzer.Models;
using System.ComponentModel;
using System.Collections.ObjectModel;
using CyclingIntervalsGui.Models;
using CyclingIntervalsGui.Behaviors;

namespace CyclingIntervalsGui.ViewModels;

public partial class IntervalsViewModel : ViewModelBase
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly DataRepository _repository;

    [ObservableProperty]
    private List<Interval>? _intervalsList;

    public IntervalsViewModel(DataRepository repository)
    {
        _repository = repository;
        _repository.PropertyChanged += OnRepositoryPropertyChanged;
    }

    private void OnRepositoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DataRepository.IntervalsList))
        {
            IntervalsList = _repository.IntervalsList;
        }
    }

    [RelayCommand]
    private void HandleRectangleClick(ScottPlot.Plottables.Rectangle rect)
    {
        // Logger.Info($"Rectangle clicked: {DateTime.FromOADate(rect.X1).TimeOfDay},{DateTime.FromOADate(rect.X2).TimeOfDay}");
        if (IntervalsList == null) return;

        Interval? found = FindInterval(IntervalsList, rect);
        if (found == null) return;

        if (!_repository.PlottableIntervals.Contains(found))
        {
            Logger.Debug("Show new interval");
            _repository.PlottableIntervals.Add(found);
        }
        else
        {
            Logger.Debug("Hide interval");
            _repository.PlottableIntervals.Remove(found);
        }
    }

    private static Interval? FindInterval(List<Interval> intervals, ScottPlot.Plottables.Rectangle rect)
    {
        foreach (Interval interval in intervals)
        {
            if (IsRectangleInterval(rect, interval)) return interval;
            if (interval.Intervals.Count != 0)
            {
                Interval? subInterval = FindInterval(interval.Intervals, rect);
                if (subInterval != null) return subInterval;
            }
        }
        return null;
    }

    private static bool IsRectangleInterval(ScottPlot.Plottables.Rectangle rect, Interval interval)
    {
        return interval.StartTime == DateTime.FromOADate(rect.X1) &&
                interval.EndTime == DateTime.FromOADate(rect.X2);
    }
}