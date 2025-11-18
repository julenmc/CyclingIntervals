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

public partial class GraphViewModel : ViewModelBase
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly DataRepository _repository;

    [ObservableProperty]
    private GraphData? _data;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private AvaloniaColor _color;

    [ObservableProperty]
    private bool _showAxis;

    [ObservableProperty]
    private List<ClimbData>? _climbsList;

    [ObservableProperty]
    private List<Interval>? _intervalsList;

    private readonly MainWindowViewModel _parent;

    [ObservableProperty]
    private bool _showClimbs;

    [ObservableProperty]
    private bool _showIntervals;

    public GraphViewModel(DataRepository repository,
                            string name,
                            AvaloniaColor color,
                            bool showAxis,
                            MainWindowViewModel parent)
    {
        _repository = repository;
        Name = name;
        Color = color;
        ShowAxis = showAxis;
        _parent = parent;

        // Inicializar propiedades observables desde el padre
        ShowClimbs = _parent.ShowClimbs;
        ShowIntervals = _parent.ShowIntervals;

        // Suscribirse a cambios del repositorio para mantener el ViewModel sincronizado
        _repository.PropertyChanged += OnRepositoryPropertyChanged;

        // Suscribirse a cambios del padre para mantener sincronizadas las propiedades
        _parent.PropertyChanged += OnParentPropertyChanged;
    }

    private void OnParentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainWindowViewModel.ShowClimbs):
                ShowClimbs = _parent.ShowClimbs;
                break;
            case nameof(MainWindowViewModel.ShowIntervals):
                ShowIntervals = _parent.ShowIntervals;
                break;
        }
    }

    private void OnRepositoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == Name)
        {
            Data = _repository.GetDataByName(Name);
        }
        else
        {
            switch (e.PropertyName)
            {
                case nameof(DataRepository.ClimbsList):
                    ClimbsList = _repository.ClimbsList;
                    break;
                case nameof(DataRepository.IntervalsList):
                    IntervalsList = _repository.IntervalsList;
                    break;
            }
        }
    }
}