using Hplc.Controller.Api.Models.Instrument;

namespace Hplc.Controller.Api.Stores;

public class InstrumentStatusStore
{
    private readonly object _lock = new();

    public bool MsBusy { get; private set; }
    public int? ActiveLc { get; private set; }

    public List<LcStatus> Lcs { get; } = new();

    public ChromatogramStatus? CurrentChrom { get; private set; }

    public void SetMsBusy(bool busy, int? activeLc)
    {
        lock (_lock)
        {
            MsBusy = busy;
            ActiveLc = activeLc;
        }
    }

    public void SetCurrentChrom(ChromatogramStatus? chrom)
    {
        lock (_lock)
        {
            CurrentChrom = chrom;
        }
    }

    public ChromatogramStatus? GetCurrentChrom()
    {
        lock (_lock)
        {
            return CurrentChrom;
        }
    }
}

public class ChromatogramStatus
{
    public string BatchName { get; set; } = "";
    public string SampleName { get; set; } = "";
    public DateTime StartTime { get; set; }
}
