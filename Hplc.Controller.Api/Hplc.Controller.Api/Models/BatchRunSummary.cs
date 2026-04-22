public class BatchRunSummary
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int TotalSamples { get; set; }
    public int CompletedSamples { get; set; }

    public TimeSpan MultiplexedDuration { get; set; }
    public TimeSpan SequentialDuration { get; set; }

    public int ParallelChannelsUsed { get; set; }
    public string ExecutionMode { get; set; }  // "Parallel-4LC"

    public TimeSpan TimeSaved =>
        SequentialDuration > MultiplexedDuration
            ? SequentialDuration - MultiplexedDuration
            : TimeSpan.Zero;

    public double PercentImprovement =>
        SequentialDuration.TotalSeconds == 0
            ? 0
            : (TimeSaved.TotalSeconds / SequentialDuration.TotalSeconds) * 100;
}
