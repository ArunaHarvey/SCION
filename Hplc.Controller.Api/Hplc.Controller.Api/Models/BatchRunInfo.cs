namespace Hplc.Controller.Api.Models;

public class BatchRunInfo
{
    public string BatchName { get; set; } = string.Empty;

    // ✅ Expose status as STRING for Angular
    public string Status { get; set; } = "Queued";

    public int QueuePosition { get; set; }

    public DateTime? BatchStarted { get; set; }

    public DateTime? BatchCompleted { get; set; }
}