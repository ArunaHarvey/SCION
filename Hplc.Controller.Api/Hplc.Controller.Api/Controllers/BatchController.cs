using Microsoft.AspNetCore.Mvc;
using Hplc.Controller.Api.Services;
using Hplc.Controller.Api.Models;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/batch")]
public class BatchController : ControllerBase
{
    private readonly BatchFileService _svc;

    public BatchController(BatchFileService svc)
    {
        _svc = svc;
    }

    /* =========================
       Batch Definitions (READ ONLY)
       ========================= */

    // GET /api/batch
    [HttpGet]
    public IActionResult GetBatches()
    {
        var batches = _svc.GetAvailableBatchNames().ToList();
        return Ok(batches);
    }

    // GET /api/batch/definition/{batchName}
    [HttpGet("definition/{batchName}")]
    public IActionResult GetBatchDefinition(string batchName)
    {
        try
        {
            var batch = _svc.LoadBatchDefinition(batchName);
            return Ok(batch);
        }
        catch (FileNotFoundException)
        {
            return NotFound($"Batch definition '{batchName}' not found");
        }
    }

    /* =========================
       Batch Save (NO ENQUEUE)
       ========================= */

    // POST /api/batch/save
    [HttpPost("save")]
    public IActionResult Save([FromBody] Batch batch)
    {
        _svc.SaveBatchDefinition(batch);
        return Ok();
    }

    /* =========================
       Run Queue
       ========================= */

    // GET /api/batch/queue
    [HttpGet("queue")]
    public IActionResult GetRunQueue()
    {
        return Ok(_svc.GetBatchRunQueue());
    }

    // POST /api/batch/enqueue
    [HttpPost("enqueue")]
    public IActionResult Enqueue([FromBody] Batch batch)
    {
        _svc.EnqueueBatch(batch);
        return Ok();
    }

    // POST /api/batch/start/{batchName}
    [HttpPost("start/{batchName}")]
    public IActionResult StartBatch(string batchName)
    {
        _svc.StartBatch(batchName);
        return Ok();
    }

    // DELETE /api/batch/queue
    [HttpDelete("queue")]
    public IActionResult ClearQueue()
    {
        _svc.ClearRunQueue();
        return Ok();
    }
}