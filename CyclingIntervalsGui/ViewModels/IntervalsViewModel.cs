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
        Logger.Info("Rectangle clicked");
    }
}