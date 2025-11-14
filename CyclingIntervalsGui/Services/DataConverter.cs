using CyclingIntervalsGui.Models;
using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Models;

namespace CyclingIntervalsGui.Services;

/// <summary>
/// Class <c>DataConverter</c> has methods to convert the full read data 
/// from the activity file into graphable models
/// </summary>
public class DataConverter
{
    private List<FitnessData> _points;
    private List<CoreModels.Climb> _climbs;

    /// <summary>
    /// Constructor for class <c>ConvertGraph</c>.
    /// </summary>
    /// <param name="points">the full points list to convert.</param>
    /// <param name="climbs">list of climbs found in the activity.</param>
    public DataConverter(List<FitnessData> points, List<CoreModels.Climb>? climbs = null)
    {
        // argument check
        if (points.Count == 0) throw new ArgumentException(nameof(points), "No points given.");

        _points = new List<FitnessData>();
        _points.AddRange(points);
        _climbs = new List<CoreModels.Climb>();
        if (climbs != null) _climbs.AddRange(climbs);
    }

    /// <summary>
    /// Method <c>ConvertGraph</c> converts the full read data from the activity file 
    /// into a graphable model (<see cref="GraphData"/>).
    /// </summary>
    /// <param name="selector">the variable that is to be put in the output data.</param>
    /// <returns>
    /// The class <see cref="GraphData"/> with the selected value ready for visualization.
    /// </returns>
    public GraphData GetGraphData(Func<FitnessData, float?> selector)
    {
        GraphData ret = new GraphData();
        ret.MaxValue = (int)(selector(_points.First()) ?? 0);
        ret.MinValue = (int)(selector(_points.First()) ?? 0);
        foreach (FitnessData point in _points)
        {
            float? value = selector(point);
            if (value == null) continue; //throw new ArgumentException(nameof(_points), $"Null value at point {point.Timestamp.GetDateTime().TimeOfDay}");
            ret.Values.Add(new GraphData.SingularGraphData
            {
                Date = point.Timestamp.GetDateTime(),
                Value = (float)value,
            });
            if (value > ret.MaxValue) ret.MaxValue = (int)value;
            if (value < ret.MinValue) ret.MinValue = (int)value;
        }
        return ret;
    }
    
    /// <summary>
    /// A list of <see cref="ClimbData"/> with the climbs' data ready for visualization.
    /// </summary>
    public List<ClimbData> Climbs
    {
        get
        {
            return ConvertClimbs();
        }
    }

    /// <summary>
    /// Method <c>ConvertClimb</c> converts the detected climb's data
    /// into a graphable model (<see cref="ClimbData"/>).
    /// </summary>
    /// <returns>
    /// A list of <see cref="ClimbData"/> with the climbs' data ready for visualization.
    /// </returns>
    private List<ClimbData> ConvertClimbs()
    {
        List<ClimbData> ret = new List<ClimbData>();
        foreach (CoreModels.Climb climb in _climbs)
        {
            // search for start and end dates
            int startIndex = 0;
            for (startIndex = 0; startIndex < _points.Count; startIndex++)
            {
                if (_points[startIndex].Position.Distance >= climb.InitRouteDistance)
                {
                    break;
                }
            }
            int endIndex = startIndex;
            for (endIndex = startIndex; endIndex < _points.Count; endIndex++)
            {
                if (_points[endIndex].Position.Distance >= climb.EndRouteDistance)
                {
                    break;
                }
            }
            startIndex = (startIndex >= _points.Count) ? _points.Count - 1 : startIndex;
            endIndex = (endIndex >= _points.Count) ? _points.Count - 1 : endIndex;

            ret.Add(new ClimbData
            {
                StartLength = (int)climb.InitRouteDistance,
                EndLength = (int)climb.EndRouteDistance,
                TotalClimb = (int)climb.HeightDiff,
                AverageSlope = (float)climb.AverageSlope,
                MaxSlope = (float)climb.MaxSlope,
                Interval = GenerateInterval(_points[startIndex].Timestamp.GetDateTime(), _points[endIndex].Timestamp.GetDateTime())
            });
        }
        return ret;
    }

    private Interval GenerateInterval(DateTime startTime, DateTime endTime)
    {
        var points = _points
            .Where(p =>
            {
                var timestamp = p.Timestamp.GetDateTime();
                return timestamp >= startTime && timestamp <= endTime;
            })
            .ToList();

        Interval interval = new Interval
        {
            StartTime = startTime,
            EndTime = endTime,
            TimeDiff = (int)(endTime - startTime).TotalSeconds + 1,
            AveragePower = (float)points.Average(p => p.Stats.Power ?? 0),
        };
        interval.Intervals = new List<Interval>();

        return interval;
    }
}