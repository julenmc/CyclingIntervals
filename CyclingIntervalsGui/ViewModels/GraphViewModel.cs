using CommunityToolkit.Mvvm.ComponentModel;
using CyclingIntervalsGui.Models;
using CyclingIntervalsGui.Repositories;

namespace CyclingIntervalsGui.ViewModels;

public partial class GraphViewModel : ViewModelBase
{
    private readonly DataRepository _repository;

    [ObservableProperty]
    private GraphData _altitudeData = new();

    public GraphViewModel(DataRepository repository)
    {
        _repository = repository;
    }

    private bool CanShowClimbs() => _repository.UiConfiguration != null ? _repository.UiConfiguration.ShowClimbs : false;
    private bool CanShowIntervals() => _repository.UiConfiguration != null ? _repository.UiConfiguration.ShowIntervals : false;
}