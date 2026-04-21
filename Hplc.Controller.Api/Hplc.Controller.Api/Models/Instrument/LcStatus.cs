using System.Net.NetworkInformation;

namespace Hplc.Controller.Api.Models.Instrument;



public class LcStatus
{
    public int LcId { get; set; }
    public string State { get; set; } = "";
    public string Sample { get; set; } = "";
}
