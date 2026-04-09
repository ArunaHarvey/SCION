namespace Hplc.Controller.Api.Models;

public class BatchRunInfo
{
    public string BatchName { get; set; } = "";
    public string LcId { get; set; } = "";

    public BatchRunStatus Status { get; set; } = BatchRunStatus.Queued;

    public bool OwnsMS { get; set; }
    public int? QueuePosition { get; set; }

    public DateTime? InjectionStartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public string? ActualDuration { get; set; }

    // ✅ INTERNAL – UI does NOT need to render this
    public List<SampleExecutionInfo> Samples { get; set; } = new();
}