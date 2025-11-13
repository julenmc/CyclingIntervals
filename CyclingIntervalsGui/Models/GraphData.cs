using CyclingTrainer;

namespace CyclingIntervalsGui.Models;

public class GraphData
{
    public int MaxValue { get; set; }
    public int MinValue { get; set; }
    public List<SingularGraphData> Values { get; set; } = new();

    public class SingularGraphData
    {
        public DateTime Date { get; set; }
        public float Value { get; set; }
    }
}