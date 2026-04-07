using System.Net.NetworkInformation;

namespace Hplc.Controller.Api.Models.Instrument;

public class LcStatus
{
    public string LcId { get; set; } = "";       // HPLC1, HPLC2, etc.
    public LcState State { get; set; }            // Idle, Injecting, Running
    public string? CurrentSample { get; set; }    // Assay_S01
}
