using CommunityToolkit.Mvvm.ComponentModel;
using CyclingIntervalsGui.Models;
using CyclingIntervalsGui.Repositories;
using System.ComponentModel;

namespace CyclingIntervalsGui.ViewModels;

public partial class ConfigViewModel : ViewModelBase
{
    private readonly DataRepository _repository;

    [ObservableProperty]
    private AnalyzeConfig? _analyzeConfig;

    public ConfigViewModel(DataRepository repository)
    {
        _repository = repository;
        _repository.PropertyChanged += OnRepositoryPropertyChanged;
    }

    /// <summary>
    /// Maneja cambios en el repositorio y actualiza las propiedades observables del ViewModel.
    /// Esto asegura que los cambios en los datos se reflejen automáticamente en la vista.
    /// </summary>
    private void OnRepositoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(DataRepository.Configuration):
                AnalyzeConfig = _repository.Configuration;
                break;
        }
    }
}