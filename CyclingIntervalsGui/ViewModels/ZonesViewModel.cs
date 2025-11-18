using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CyclingIntervalsGui.Repositories;
using CyclingTrainer.Core.Models;
using System.ComponentModel;
using System.Collections.ObjectModel;
using NLog;
using CyclingIntervalsGui.Models;

namespace CyclingIntervalsGui.ViewModels;

public partial class ZonesViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly DataRepository _repository;

    [ObservableProperty]
    private ObservableCollection<Zone> _zones;

    public ZonesViewModel(DataRepository repository)
    {
        _repository = repository;
        Zones = new ObservableCollection<Zone>();
        foreach (Zone zone in _repository.Configuration.Zones)
        {
            Zones.Add(zone);
        }
    }

    [RelayCommand]
    private void SaveZones()
    {
        AnalyzeConfig newConfig = _repository.CopyConfig();
        newConfig.Zones = new List<Zone>();
        foreach (var zone in Zones)
        {
            newConfig.Zones.Add(zone);
        }
        _repository.Configuration = newConfig;
    }
}