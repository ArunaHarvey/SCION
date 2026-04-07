namespace Hplc.Controller.Api.Models;

public class SampleExecutionInfo
{
    public string BatchName { get; set; } = "";
    public string SampleName { get; set; } = "";
    public string LcId { get; set; } = "";
    public bool UsesMS { get; set; }
    public string State { get; set; } = "Running";
}