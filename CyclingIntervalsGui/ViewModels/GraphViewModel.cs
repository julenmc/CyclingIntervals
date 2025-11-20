using AvaloniaColor = Avalonia.Media.Color;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CyclingIntervalsGui.Repositories;
using CyclingIntervalsGui.Services;
using CyclingTrainer.SessionAnalyzer.Models;
using System.ComponentModel;
using System.Collections.ObjectModel;
using CyclingIntervalsGui.Models;

namespace CyclingIntervalsGui.ViewModels;

public partial class GraphViewModel : ViewModelBase
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly DataRepository _repository;

    [ObservableProperty]
    private GraphData? _altitudeData;

    [ObservableProperty]
    private GraphData? _hrData;

    [ObservableProperty]
    private GraphData? _powerData;

    [ObservableProperty]
    private List<ClimbData>? _climbsList;

    [ObservableProperty]
    private ObservableCollection<Interval> _intervalsList;

    private readonly MainWindowViewModel _parent;

    [ObservableProperty]
    private bool _showClimbs;

    public GraphViewModel(DataRepository repository,
                            MainWindowViewModel parent)
    {
        _repository = repository;
        _parent = parent;

        // Inicializar propiedades observables desde el padre
        ShowClimbs = _parent.ShowClimbs;

        // Suscribirse a cambios del repositorio para mantener el ViewModel sincronizado
        _repository.PropertyChanged += OnRepositoryPropertyChanged;

        // Suscribirse a cambios del padre para mantener sincronizadas las propiedades
        _parent.PropertyChanged += OnParentPropertyChanged;

        _intervalsList = new ObservableCollection<Interval>();
        _intervalsList.CollectionChanged += (sender, e) =>
        {
            OnPropertyChanged(nameof(IntervalsList));
        };
    }

    private void OnParentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainWindowViewModel.ShowClimbs):
                ShowClimbs = _parent.ShowClimbs;
                break;
        }
    }

    private void OnRepositoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DataRepository.AltitudeData):
                AltitudeData = _repository.AltitudeData;
                break;
            case nameof(DataRepository.PowerData):
                PowerData = _repository.PowerData;
                break;
            case nameof(DataRepository.HrData):
                HrData = _repository.HrData;
                break;
            case nameof(DataRepository.ClimbsList):
                ClimbsList = _repository.ClimbsList;
                break;
            case nameof(DataRepository.PlottableIntervals):
                // Sincronizar contenido sin reemplazar la referencia
                SyncIntervals();
                break;
        }
    }

    private void SyncIntervals()
    {
        var repositoryIntervals = _repository.PlottableIntervals;
        
        // Si es la primera vez, inicializar la colección
        if (IntervalsList == null)
        {
            IntervalsList = new ObservableCollection<Interval>(repositoryIntervals);
            return;
        }

        // Sincronizar el contenido sin reemplazar la referencia
        // Esto mantiene viva la suscripción a CollectionChanged
        IntervalsList.Clear();
        foreach (var interval in repositoryIntervals)
        {
            IntervalsList.Add(interval);
        }
    }
}