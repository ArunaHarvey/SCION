namespace Hplc.Controller.Api.Models;

public class Sample
{
    public int SerialNo { get; set; }
    public string SampleName { get; set; } = "";
    public string InjectionLocation { get; set; } = "";
    public int InjectionVolume { get; set; }
    public string MethodId { get; set; } = "";
    public int AssignedLC { get; set; }
}