namespace Hplc.Controller.Api.Models;

public class BatchRunInfo
{
    public string BatchName { get; set; } = "";
    public BatchRunStatus Status { get; set; } = BatchRunStatus.Queued;
    public int QueuePosition { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }


    // ✅ Batch-centric execution
    public List<SampleExecutionInfo> Samples { get; set; } = new();
}