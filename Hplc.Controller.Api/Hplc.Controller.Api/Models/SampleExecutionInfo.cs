namespace Hplc.Controller.Api.Models;

public class SampleExecutionInfo
{
    public string SampleName { get; set; } = "";
    public string MethodId { get; set; } = "";

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? LcStartTime { get; set; }
    public DateTime? MsStartTime { get; set; }
    public DateTime? MsEndTime { get; set; }
    public DateTime? LcEndTime { get; set; }

    // Execution state
    public SampleExecutionState State { get; set; } = SampleExecutionState.Queued;

    // ✅ LC–MS simulation fields
    public int AssignedLC { get; set; } = 0;    // LC-1..LC-4
    public bool OwnsMS { get; set; } = false;
}