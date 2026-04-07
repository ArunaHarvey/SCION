namespace Hplc.Controller.Api.Models;

public class BatchRunInfo
{
    public string BatchName { get; set; } = "";

    // ✅ REMOVE JsonIgnore — UI MUST see this
    public BatchRunStatus Status { get; set; } = BatchRunStatus.Queued;

    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;

    // Optional runtime info
    public DateTime? InjectionStartTime { get; set; }
}