namespace Hplc.Controller.Api.Models.Instrument;

public class MsStatus
{
    public bool IsAcquiring { get; set; }
    public string? ActiveLcId { get; set; }
    public string? BatchName { get; set; }
}
