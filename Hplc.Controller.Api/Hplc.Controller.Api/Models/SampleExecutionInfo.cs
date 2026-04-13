namespace Hplc.Controller.Api.Models;

public class SampleExecutionInfo
{
    public string SampleName { get; set; } = "";
    public string MethodId { get; set; } = "";
    public string? LcId { get; set; }
    public SampleExecutionState State { get; set; }
}