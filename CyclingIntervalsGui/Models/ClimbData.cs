using CyclingTrainer.SessionAnalyzer.Models;

namespace CyclingIntervalsGui.Models;

public class ClimbData
{
    public int StartLength { get; set; }
    public int EndLength { get; set; }
    public int TotalLength => EndLength - StartLength;
    public int TotalClimb { get; set; }
    public float AverageSlope { get; set; } // cant be climb/length cuz might have some downhill
    public float MaxSlope { get; set; }
    public Interval Interval { get; set; } = new();
}