using CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Models;

namespace CyclingIntervalsGui.Models;

public class AnalyzeConfig
{
    public IntervalGroupThresholds Thresholds { get; set; } = new();
    public List<Zone> Zones { get; set; } = new();
}