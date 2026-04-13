namespace Hplc.Controller.Api.Models;

public class SampleExecutionInfo
{
    public string SampleName { get; set; } = "";
    public string MethodId { get; set; } = "";

    // Execution state
    public SampleExecutionState State { get; set; } = SampleExecutionState.Queued;

    // ✅ LC–MS simulation fields
    public int AssignedLC { get; set; } = 0;    // LC-1..LC-4
    public bool OwnsMS { get; set; } = false;
}