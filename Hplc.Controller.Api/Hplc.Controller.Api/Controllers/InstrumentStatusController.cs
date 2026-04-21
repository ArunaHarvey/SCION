using Hplc.Controller.Api.Stores;
using Microsoft.AspNetCore.Mvc;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/status")]
public class InstrumentStatusController : ControllerBase
{
    private readonly InstrumentStatusStore _status;

    public InstrumentStatusController(InstrumentStatusStore status)
    {
        _status = status;
    }

    // GET /api/status/ms
    [HttpGet("ms")]
    public IActionResult Ms()
    {
        return Ok(new
        {
            busy = _status.MsBusy,
            activeLc = _status.ActiveLc
        });
    }

    // GET /api/status/lcs
    [HttpGet("lcs")]
    public IActionResult Lcs()
    {
        return Ok(_status.Lcs);
    }

    // GET /api/status/chromatogram-meta
    [HttpGet("chromatogram-meta")]
    public IActionResult ChromMeta()
    {
        var chrom = _status.GetCurrentChrom();

        if (chrom == null)
        {
            return Ok(new { state = "Idle" });
        }

        return Ok(new
        {
            state = "Running",
            batch = chrom.BatchName,
            sample = chrom.SampleName,
            startTime = chrom.StartTime
        });
    }

    // GET /api/status/chromatogram/{batch}/{sample}
    [HttpGet("chromatogram/{batch}/{sample}")]
    public IActionResult Chromatogram(string batch, string sample)
    {
        var points = BatchExecutionStore.Pull(batch, sample);
        return Ok(points);
    }
}
