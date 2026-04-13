namespace Hplc.Controller.Api.Models;

public class Batch
{
    public string BatchName { get; set; } = "";
    public List<BatchSample> Samples { get; set; } = new();
}