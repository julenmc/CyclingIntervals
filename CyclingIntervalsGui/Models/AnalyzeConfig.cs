using CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Models;

namespace CyclingIntervalsGui.Models;

public class AnalyzeConfig
{
    public IntervalGroupThresholds Thresholds { get; set; } = new();
    public List<Zone> Zones { get; set; } = new();

    public AnalyzeConfig() {}

    public AnalyzeConfig Copy()
    {
        AnalyzeConfig ret = new();
        ret.Thresholds.Short = Thresholds.Short.Copy();
        ret.Thresholds.Medium = Thresholds.Medium.Copy();
        ret.Thresholds.Long = Thresholds.Long.Copy();

        ret.Zones.AddRange(Zones);

        return ret;
    }
}