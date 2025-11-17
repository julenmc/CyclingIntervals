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
    private AnalyzeConfig _configuration;

    [ObservableProperty]
    private UIConfiguration? _uiConfiguration;

    /// <summary>
    /// Repository's constructor, used to initialize configuration with default values
    /// </summary>
    public DataRepository()
    {
        _uiConfiguration = new UIConfiguration
        {
            ShowClimbs = true,
            ShowIntervals = true
        };

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
                    Id = 1, LowLimit = 0, HighLimit = 180,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 2, LowLimit = 181, HighLimit = 240,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 3, LowLimit = 241, HighLimit = 290,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 4, LowLimit = 291, HighLimit = 340,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 5, LowLimit = 341, HighLimit = 390,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 6, LowLimit = 391, HighLimit = 460,
                },
                new CyclingTrainer.Core.Models.Zone
                {
                    Id = 7, LowLimit = 461, HighLimit = 2000,
                },
            }
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