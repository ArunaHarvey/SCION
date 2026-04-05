namespace Hplc.Controller.Api.Models;

public class Batch
{
    public string BatchName { get; set; } = "";
    public List<Sample> Samples { get; set; } = new();
}