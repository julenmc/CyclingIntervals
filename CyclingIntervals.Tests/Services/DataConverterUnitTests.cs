using CyclingTrainer.SessionReader.Models;
using CyclingIntervalsGui.Models;
using CyclingIntervalsGui.Services;
using CyclingTrainer.Core.Models;

namespace CyclingIntervalsGui.Test;

/// <summary>
/// Contains the unit test of the <see cref="DataConverter"/> class.
/// </summary>
/// <remarks>
/// The test follow the convention:
/// <c>Method_Scenario_ExpectedResult</c>.
/// </remarks>
[TestClass]
public sealed class DataConverterUnitTests
{
    private List<FitnessData> GetDefaultSampleData()
    {
        DateTime defaultStartDate = new DateTime(2025, 07, 14, 12, 00, 00);
        return new List<FitnessData>
        {
            new FitnessData{
                Timestamp = new Dynastream.Fit.DateTime(defaultStartDate),
                Stats = new PointStats { Power = 150, HeartRate = 120},
                Position = new PointPosition { Distance = 0, Altitude = 200}
            },
            new FitnessData{
                Timestamp = new Dynastream.Fit.DateTime(defaultStartDate.AddSeconds(1)),
                Stats = new PointStats { Power = 200, HeartRate = 130},
                Position = new PointPosition { Distance = 10, Altitude = 202}
            },
            new FitnessData{
                Timestamp = new Dynastream.Fit.DateTime(defaultStartDate.AddSeconds(2)),
                Stats = new PointStats { Power = 250, HeartRate = 140},
                Position = new PointPosition { Distance = 20, Altitude = 204}
            },
            new FitnessData{
                Timestamp = new Dynastream.Fit.DateTime(defaultStartDate.AddSeconds(3)),
                Stats = new PointStats { Power = 300, HeartRate = 150},
                Position = new PointPosition { Distance = 30, Altitude = 198}
            },
            new FitnessData{
                Timestamp = new Dynastream.Fit.DateTime(defaultStartDate.AddSeconds(4)),
                Stats = new PointStats { Power = 0, HeartRate = 155},
                Position = new PointPosition { Distance = 40, Altitude = 199}
            },
        };
    }

    /// <summary>
    /// Verifies that the method returns the correct number of points
    /// </summary>
    [TestMethod]
    public void GetGraphData_WithAltitude_ReturnsCorrectNumberOfPoints()
    {
        DataConverter dataConverter = new DataConverter(GetDefaultSampleData());
        GraphData data = dataConverter.GetGraphData(x => x.Position.Altitude);
        Assert.AreEqual(5, data.Values.Count);
    }

    /// <summary>
    /// Verifies that the method returns the correct max value
    /// </summary>
    [TestMethod]
    public void GetGraphData_WithAltitude_ReturnsCorrectMaxValue()
    {
        DataConverter dataConverter = new DataConverter(GetDefaultSampleData());
        GraphData data = dataConverter.GetGraphData(x => x.Position.Altitude);
        Assert.AreEqual(204, data.MaxValue);
    }

    /// <summary>
    /// Verifies that the method returns the correct min value
    /// </summary>
    [TestMethod]
    public void GetGraphData_WithAltitude_ReturnsCorrectMinValue()
    {
        DataConverter dataConverter = new DataConverter(GetDefaultSampleData());
        GraphData data = dataConverter.GetGraphData(x => x.Position.Altitude);
        Assert.AreEqual(198, data.MinValue);
    }

    /// <summary>
    /// Verifies that the method returns the correct date times
    /// </summary>
    [TestMethod]
    public void GetGraphData_WithAltitude_ReturnsCorrectDateTimes()
    {
        DataConverter dataConverter = new DataConverter(GetDefaultSampleData());
        GraphData data = dataConverter.GetGraphData(x => x.Position.Altitude);
        Assert.AreEqual(new DateTime(2025, 07, 14, 12, 00, 00), data.Values[0].Date);
        Assert.AreEqual(new DateTime(2025, 07, 14, 12, 00, 00).AddSeconds(1), data.Values[1].Date);
        Assert.AreEqual(new DateTime(2025, 07, 14, 12, 00, 00).AddSeconds(2), data.Values[2].Date);
        Assert.AreEqual(new DateTime(2025, 07, 14, 12, 00, 00).AddSeconds(3), data.Values[3].Date);
        Assert.AreEqual(new DateTime(2025, 07, 14, 12, 00, 00).AddSeconds(4), data.Values[4].Date);
    }

    /// <summary>
    /// Verifies that the method returns the correct values whan asking for altitude
    /// </summary>
    [TestMethod]
    public void GetGraphData_WithAltitude_ReturnsCorrectValues()
    {
        DataConverter dataConverter = new DataConverter(GetDefaultSampleData());
        GraphData data = dataConverter.GetGraphData(x => x.Position.Altitude);
        Assert.AreEqual(200, data.Values[0].Value);
        Assert.AreEqual(202, data.Values[1].Value);
        Assert.AreEqual(204, data.Values[2].Value);
        Assert.AreEqual(198, data.Values[3].Value);
        Assert.AreEqual(199, data.Values[4].Value);
    }

    /// <summary>
    /// Verifies that the method returns the correct values whan asking for power
    /// </summary>
    [TestMethod]
    public void GetGraphData_WithPower_ReturnsCorrectValues()
    {
        DataConverter dataConverter = new DataConverter(GetDefaultSampleData());
        GraphData data = dataConverter.GetGraphData(x => x.Stats.Power);
        Assert.AreEqual(150, data.Values[0].Value);
        Assert.AreEqual(200, data.Values[1].Value);
        Assert.AreEqual(250, data.Values[2].Value);
        Assert.AreEqual(300, data.Values[3].Value);
        Assert.AreEqual(0, data.Values[4].Value);
    }

    /// <summary>
    /// Verifies that the method returns the correct values whan asking for HR
    /// </summary>
    [TestMethod]
    public void GetGraphData_WithHR_ReturnsCorrectValues()
    {
        DataConverter dataConverter = new DataConverter(GetDefaultSampleData());
        GraphData data = dataConverter.GetGraphData(x => x.Stats.HeartRate);
        Assert.AreEqual(120, data.Values[0].Value);
        Assert.AreEqual(130, data.Values[1].Value);
        Assert.AreEqual(140, data.Values[2].Value);
        Assert.AreEqual(150, data.Values[3].Value);
        Assert.AreEqual(155, data.Values[4].Value);
    }

    /// <summary>
    /// Verifies that the method throws an exception when the input list has no data
    /// </summary>
    [TestMethod]
    public void GetGraphData_NoData_ExceptionThrown()
    {
        void AuxMethod()
        {
            List<FitnessData> fitnessData = new List<FitnessData>();
            DataConverter dataConverter = new DataConverter(fitnessData);
        }
        
        Assert.ThrowsException<ArgumentException>(() => AuxMethod());
    }

    /// <summary>
    /// Verifies that the method throws an exception when the input list 
    /// has an element with null value
    /// </summary>
    [TestMethod]
    public void GetGraphData_NullValue_ExceptionThrown()
    {
        List<FitnessData> fitnessData = new List<FitnessData>
        {
            new FitnessData{
                Timestamp = new Dynastream.Fit.DateTime(new DateTime(2025, 07, 14, 12, 00, 00)),
                Stats = new PointStats { Power = 150, HeartRate = 120},
                Position = new PointPosition { Distance = 0, Altitude = null}
            }
        };
        DataConverter dataConverter = new DataConverter(fitnessData);
        Assert.ThrowsException<ArgumentException>(() => dataConverter.GetGraphData(x => x.Position.Altitude));
    }

    private List<FitnessData> GetFlatFitnessData(DateTime startDate, int startDistance, float startAltitude)
    {
        List<FitnessData> ret = new List<FitnessData>();
        for (int i = 0; i < 100; i++)
        {
            ret.Add(new FitnessData
            {
                Timestamp = new Dynastream.Fit.DateTime(startDate),
                Stats = new PointStats { Power = 200, HeartRate = 130 },
                Position = new PointPosition { Distance = startDistance, Altitude = startAltitude }
            });
            startDate = startDate.AddSeconds(1);
            startDistance += 5;
        }
        return ret;
    }

    /// <summary>
    /// Verifies that the method returns no climb when the input has no climbs
    /// </summary>
    [TestMethod]
    public void Climbs_NoClimbs_ReturnsNoClimbs()
    {
        List<Climb> climbs = new List<Climb>();
        DataConverter dataConverter = new DataConverter(GetFlatFitnessData(new DateTime(2025, 07, 14, 12, 00, 00), 0, 100), climbs);
        Assert.AreEqual(0, dataConverter.Climbs.Count);
    }

    private List<FitnessData> GetClimbFitnessData(DateTime startDate, int startDistance, float startAltitude)
    {
        List<FitnessData> ret = new List<FitnessData>();
        for (int i = 0; i < 100; i++)
        {
            ret.Add(new FitnessData
            {
                Timestamp = new Dynastream.Fit.DateTime(startDate),
                Stats = new PointStats { Power = 200, HeartRate = 130 },
                Position = new PointPosition { Distance = startDistance, Altitude = startAltitude }
            });
            startDate = startDate.AddSeconds(1);
            startDistance += 5;
            startAltitude += 0.5f;
        }
        return ret;
    }

    /// <summary>
    /// Verifies that the method returns the correct number of climbs
    /// </summary>
    [TestMethod]
    public void Climbs_TwoClimbs_ReturnsCorrectNumberOfClimbs()
    {
        List<FitnessData> fitnessData = new List<FitnessData>();
        DateTime startDate = new DateTime(2025, 07, 14, 12, 00, 00);
        fitnessData.AddRange(GetClimbFitnessData(startDate, 0, 100));
        fitnessData.AddRange(GetFlatFitnessData(startDate.AddSeconds(100), 500, 150));
        fitnessData.AddRange(GetClimbFitnessData(startDate.AddSeconds(200), 1000, 150));
        List<Climb> climbs = new List<Climb>
        {
            new Climb
            {
                InitRouteDistance = 0,
                EndRouteDistance = 500,
                Distance = 500,
                AverageSlope = 10,
                MaxSlope = 10,
                HeightDiff = 50
            },
            new Climb
            {
                InitRouteDistance = 1000,
                EndRouteDistance = 1500,
                Distance = 500,
                AverageSlope = 10,
                MaxSlope = 10,
                HeightDiff = 50
            }
        };
        DataConverter dataConverter = new DataConverter(fitnessData, climbs);
        Assert.AreEqual(2, dataConverter.Climbs.Count);
    }

    /// <summary>
    /// Verifies that the method returns the correct climb values
    /// </summary>
    [TestMethod]
    public void Climbs_OneClimb_ReturnsCorrectValues()
    {
        List<FitnessData> fitnessData = new List<FitnessData>();
        DateTime startDate = new DateTime(2025, 07, 14, 12, 00, 00);
        fitnessData.AddRange(GetClimbFitnessData(startDate, 0, 100));
        List<Climb> climbs = new List<Climb>
        {
            new Climb
            {
                InitRouteDistance = 0,
                EndRouteDistance = 500,
                Distance = 500,
                AverageSlope = 10,
                MaxSlope = 10,
                HeightDiff = 50
            }
        };
        DataConverter dataConverter = new DataConverter(fitnessData, climbs);
        ClimbData climb = dataConverter.Climbs.First();
        Assert.AreEqual(0, climb.StartLength);
        Assert.AreEqual(500, climb.EndLength);
        Assert.AreEqual(500, climb.TotalLength);
        Assert.AreEqual(50, climb.TotalClimb);
        Assert.AreEqual(10, climb.AverageSlope);
        Assert.AreEqual(10, climb.MaxSlope);
    }

    /// <summary>
    /// Verifies that the method returns the correct interval values
    /// </summary>
    [TestMethod]
    public void Climbs_OneClimb_ReturnsCorrectInterval()
    {
        List<FitnessData> fitnessData = new List<FitnessData>();
        DateTime startDate = new DateTime(2025, 07, 14, 12, 00, 00);
        fitnessData.AddRange(GetClimbFitnessData(startDate, 0, 100));
        List<Climb> climbs = new List<Climb>
        {
            new Climb
            {
                InitRouteDistance = 0,
                EndRouteDistance = 500,
                Distance = 500,
                AverageSlope = 10,
                MaxSlope = 10,
                HeightDiff = 50
            }
        };
        DataConverter dataConverter = new DataConverter(fitnessData, climbs);
        CyclingTrainer.SessionAnalyzer.Models.Interval interval = dataConverter.Climbs.First().Interval;
        Assert.AreEqual(new DateTime(2025, 07, 14, 12, 00, 00), interval.StartTime);
        Assert.AreEqual(new DateTime(2025, 07, 14, 12, 01, 39), interval.EndTime);
        Assert.AreEqual(100, interval.TimeDiff);
        Assert.AreEqual(200, interval.AveragePower);
    }
}