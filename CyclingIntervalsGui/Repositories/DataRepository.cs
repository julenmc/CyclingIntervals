using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CyclingIntervalsGui.Models;
using CyclingTrainer.SessionAnalyzer.Models;
using static CyclingTrainer.SessionAnalyzer.Constants.IntervalSearchValues;

namespace CyclingIntervalsGui.Repositories;

public partial class DataRepository : ObservableObject
{
    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private GraphData? _altitudeData;

    [ObservableProperty]
    private GraphData? _powerData;

    [ObservableProperty]
    private GraphData? _hrData;

    [ObservableProperty]
    private List<ClimbData>? _climbsList;

    [ObservableProperty]
    private List<Interval>? _intervalsList;

    [ObservableProperty]
    private ObservableCollection<Interval> _plottableIntervals;

    [ObservableProperty]
    private AnalyzeConfig _configuration;

    [ObservableProperty]
    private bool _showClimbs;

    /// <summary>
    /// Repository's constructor, used to initialize configuration with default values
    /// </summary>
    public DataRepository()
    {
        _showClimbs = false;

        _configuration = new AnalyzeConfig
        {
            Thresholds = new IntervalGroupThresholds
            {
                Short = ShortIntervals.Default.Copy(),
                Medium = MediumIntervals.Default.Copy(),
                Long = LongIntervals.Default.Copy(),
            },
            Zones = new List<CyclingTrainer.Core.Models.Zone>
            {
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 1, LowLimit = 0, HighLimit = 136,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 2, LowLimit = 137, HighLimit = 186,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 3, LowLimit = 187, HighLimit = 223,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 4, LowLimit = 224, HighLimit = 260,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 5, LowLimit = 261, HighLimit = 298,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 6, LowLimit = 299, HighLimit = 373,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 7, LowLimit = 374, HighLimit = 2000,
                },
            }
        };

        _plottableIntervals = new ObservableCollection<Interval>();
        
        // Escuchar cambios en la colección para disparar PropertyChanged
        _plottableIntervals.CollectionChanged += (sender, e) =>
        {
            OnPropertyChanged(nameof(PlottableIntervals));
        };
    }

    public AnalyzeConfig CopyConfig()
    {
        AnalyzeConfig config = new AnalyzeConfig();
        config.Zones = new List<CyclingTrainer.Core.Models.Zone>();
        foreach (CyclingTrainer.Core.Models.Zone zone in Configuration.Zones)
        {
            config.Zones.Add(zone);
        }

        config.Thresholds.Short = Configuration.Thresholds.Short.Copy();
        config.Thresholds.Medium = Configuration.Thresholds.Medium.Copy();
        config.Thresholds.Long = Configuration.Thresholds.Long.Copy();

        return config;
    }
}