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
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetBatches()
    {
        return Ok(_svc.GetAvailableBatchNames().ToList());
    }

    // GET /api/batch/definition/{batchName}
    [HttpGet("definition/{batchName}")]
    [ProducesResponseType(typeof(Batch), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Batch> GetBatchDefinition(string batchName)
    {
        try
        {
            return Ok(_svc.LoadBatchDefinition(batchName));
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
       ✅ RUN QUEUE (FIXED)
       ========================= */

    // GET /api/batch/queue
    // ✅ STRONGLY TYPED RESPONSE SO SAMPLES + ASSIGNEDLC ARE NOT LOST
    [HttpGet("queue")]
    [ProducesResponseType(typeof(List<BatchRunInfo>), StatusCodes.Status200OK)]
    public ActionResult<List<BatchRunInfo>> GetRunQueue()
    {
        var runs = _svc.GetBatchRunQueue();
        return Ok(runs);
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