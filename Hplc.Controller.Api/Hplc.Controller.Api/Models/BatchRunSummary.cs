namespace Hplc.Controller.Api.Models;

public class BatchRunSummary
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public TimeSpan MultiplexedDuration { get; set; }
    public TimeSpan SequentialDuration { get; set; }

    public TimeSpan TimeSaved =>
        SequentialDuration - MultiplexedDuration;

    public double PercentImprovement =>
        SequentialDuration.TotalSeconds == 0
            ? 0
            : (TimeSaved.TotalSeconds / SequentialDuration.TotalSeconds) * 100;
}