using CyclingIntervalsGui.Repositories;
using System.ComponentModel;
using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionReader.Services;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using CyclingTrainer.Core.Models;

namespace CyclingIntervalsGui.Services;

public class AnalyzeService
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly DataRepository _repository;
    private SessionContainer? _sessionContainer;
    private DataConverter? _converter;

    public AnalyzeService(DataRepository repository)
    {
        _repository = repository;
        _repository.PropertyChanged += OnChange;
    }

    private void OnChange(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            if (e.PropertyName == nameof(DataRepository.FilePath))
            {
                Logger.Debug("File path change detected");
                ReadActivity();

                AnalyzeActivity();
            }
            else if (e.PropertyName == nameof(DataRepository.Configuration))
            {
                Logger.Info("Configuration change detected");
                AnalyzeActivity();
            }
        }
        catch (Exception ex)
        {
            Logger.Warn($"Error on event: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads the activity using the SessionReader module, converts the data into 
    /// graphable models and saves them in the repository.
    /// </summary>
    private void ReadActivity()
    {
        // Read
        if (_repository.FilePath == null || !File.Exists(_repository.FilePath))
        {
            throw new ArgumentException(nameof(_repository.FilePath), "Invalid file path!");
        }
        _sessionContainer = SessionReaderService.ReadRoute(_repository.FilePath);

        // Convert and save
        _converter = new DataConverter(_sessionContainer.FitnessDataContainer.FitnessData, _sessionContainer.RouteSections.Climbs);
        _repository.AltitudeData = _converter.GetGraphData(x => x.Position.Altitude);
        _repository.PowerData = _converter.GetGraphData(x => x.Stats.Power);
        _repository.HrData = _converter.GetGraphData(x => x.Stats.HeartRate);
    }

    /// <summary>
    /// Analyzes the activity using the SessionAnalyzer module, converts the data into 
    /// graphable models and saves them in the repository.
    /// </summary>
    private void AnalyzeActivity()
    {
        if (_repository.FilePath == null || !File.Exists(_repository.FilePath))
        {
            throw new ArgumentException(nameof(_repository.FilePath), "Invalid file path!");
        }
        if (_sessionContainer == null || _converter == null)
        {
            throw new ArgumentException("File wasn't read succesfully. Can't analyze the activity");
        }
        if (_repository.Configuration == null)
        {
            throw new ArgumentException(nameof(_repository.Configuration), "Invalid configuration");
        }
        // Analyze
        List<Zone> zones = new List<Zone>();
        foreach (Zone zone in _repository.Configuration.Zones) zones.Add(zone);
        IntervalsService intervalService = new IntervalsService(_sessionContainer.FitnessDataContainer.FitnessData, zones, _repository.Configuration.Thresholds);
        IntervalContainer intervalContainer = intervalService.Search();
        // Convert and save
        _repository.ClimbsList = _converter.Climbs;
        _repository.IntervalsList = intervalContainer.Intervals;
    }
}