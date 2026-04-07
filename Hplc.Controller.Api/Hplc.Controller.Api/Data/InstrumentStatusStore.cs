using Hplc.Controller.Api.Models.Instrument;

namespace Hplc.Controller.Api.Data;

public static class InstrumentStatusStore
{
    public static List<LcStatus> Lcs = new()
    {
        new() { LcId = "HPLC1", State = LcState.Running, CurrentSample = "Assay_S01" },
        new() { LcId = "HPLC2", State = LcState.Injecting, CurrentSample = "Assay_S02" },
        new() { LcId = "HPLC3", State = LcState.Idle },
        new() { LcId = "HPLC4", State = LcState.Idle }
    };

    public static MsStatus Ms = new()
    {
        IsAcquiring = true,
        ActiveLcId = "HPLC1",
        BatchName = "Batch_Assay_Run"
    };
}