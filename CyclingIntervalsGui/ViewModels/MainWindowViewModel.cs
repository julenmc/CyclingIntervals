using CyclingIntervalsGui.Repositories;
using CyclingIntervalsGui.Services;
using CyclingTrainer.SessionAnalyzer.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using Avalonia.Controls;
using System.Windows.Input;

namespace CyclingIntervalsGui.ViewModels;

/// <summary>
/// ViewModel principal que gestiona la sincronización entre el repositorio y la vista.
/// Utiliza propiedades observables para mantener la vista actualizada automáticamente.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly DataRepository _repository;
    private readonly AnalyzeService _analyzer;
    private Window? _mainWindow;

    [ObservableProperty]
    private bool _showClimbs;

    [ObservableProperty]
    private bool _showIntervals;

    [ObservableProperty]
    private List<Interval>? _intervalsList;

    [ObservableProperty]
    private string? _filePath;

    public string? FileName => string.IsNullOrEmpty(FilePath)
        ? null
        : Path.GetFileName(FilePath);

    [ObservableProperty]
    private bool _isLoading;

    public ZonesViewModel ZonesViewModel { get; }
    public GraphViewModel AltitudeViewModel { get; }
    public GraphViewModel PowerViewModel { get; }
    public GraphViewModel HrViewModel { get; }
    public IntervalsViewModel IntervalsViewModel { get; }
    
    public MainWindowViewModel()
    {
        _repository = new DataRepository();
        _analyzer = new AnalyzeService(_repository);
        ZonesViewModel = new ZonesViewModel(_repository);
        AltitudeViewModel = new GraphViewModel(_repository, nameof(DataRepository.AltitudeData), Avalonia.Media.Colors.Black, false, this);
        PowerViewModel = new GraphViewModel(_repository, nameof(DataRepository.PowerData), Avalonia.Media.Colors.DodgerBlue, false, this);
        HrViewModel = new GraphViewModel(_repository, nameof(DataRepository.HrData), Avalonia.Media.Colors.Red, true, this);
        IntervalsViewModel = new IntervalsViewModel(_repository);

        // Suscribirse a cambios del repositorio para mantener el ViewModel sincronizado
        _repository.PropertyChanged += OnRepositoryPropertyChanged;
    }

    /// <summary>
    /// Inicializa el FileManager con la ventana principal.
    /// Debe llamarse desde el code-behind después de que la ventana esté cargada.
    /// </summary>
    public void SetMainWindow(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    /// <summary>
    /// Maneja cambios en el repositorio y actualiza las propiedades observables del ViewModel.
    /// Esto asegura que los cambios en los datos se reflejen automáticamente en la vista.
    /// </summary>
    private void OnRepositoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DataRepository.IntervalsList):
                IntervalsList = _repository.IntervalsList;
                break;
        }
    }

    /// <summary>
    /// Cambia la ruta del archivo a analizar. Desencadena automáticamente:
    /// 1. Lectura del archivo
    /// 2. Análisis de datos
    /// 3. Actualización de propiedades observables
    /// 4. Actualización automática de la vista via bindings
    /// </summary>
    public void ChangeFilePath(string filePath)
    {
        FilePath = filePath;
        _repository.FilePath = filePath;
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        if (_mainWindow == null)
        {
            System.Diagnostics.Debug.WriteLine("MainWindow no inicializado");
            return;
        }

        var fileManager = new FileManager(_mainWindow);
        var selectedPath = await fileManager.PickFileAsync();

        if (!string.IsNullOrEmpty(selectedPath))
        {
            ChangeFilePath(selectedPath);
        }
    }
    
    partial void OnFilePathChanged(string? value)
    {
        OnPropertyChanged(nameof(FileName));
    }
}
