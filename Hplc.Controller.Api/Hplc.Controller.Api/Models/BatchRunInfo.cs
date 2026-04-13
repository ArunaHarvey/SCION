namespace Hplc.Controller.Api.Models;

public class BatchRunInfo
{
    public string BatchName { get; set; } = "";
    public BatchRunStatus Status { get; set; }
    public int QueuePosition { get; set; }
    public List<SampleExecutionInfo> Samples { get; set; } = new();
}