namespace Hplc.Controller.Api.Models;

public class BatchSample
{
    public int SerialNo { get; set; }
    public string SampleName { get; set; } = "";
    public double InjectionVolume { get; set; }
    public string MethodId { get; set; } = "";
}