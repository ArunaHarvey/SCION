using Microsoft.AspNetCore.Mvc;
using Hplc.Controller.Api.Services;
using Hplc.Controller.Api.Models;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/batch")]
public class BatchController : ControllerBase
{
    private readonly BatchFileService _batchService;

    public BatchController(BatchFileService batchService)
    {
        _batchService = batchService;
    }

    // ✅ GET /api/batch
    [HttpGet]
    public IActionResult GetBatches()
    {
        try
        {
            var batches = _batchService.GetAvailableBatchNames();
            return Ok(batches);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Failed to load batches",
                detail: ex.Message
            );
        }
    }

    // ✅ POST /api/batch/enqueue/{batchName}
    [HttpPost("enqueue/{batchName}")]
    public IActionResult Enqueue(string batchName)
    {
        try
        {
            var batch = _batchService.LoadBatchDefinition(batchName);
            batch.BatchName = batchName;
            _batchService.EnqueueBatch(batch);
            return Ok();
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Enqueue failed",
                detail: ex.Message
            );
        }
    }

    // ✅ GET /api/batch/queue
    [HttpGet("queue")]
    public IActionResult GetRunQueue()
    {
        try
        {
            var queue = _batchService.GetBatchRunQueue();
            return Ok(queue);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Failed to load run queue",
                detail: ex.Message
            );
        }
    }

    [HttpDelete("queue")]
    public IActionResult ClearRunQueue()
    {
        try
        {
            _batchService.ClearRunQueue();
            return NoContent(); // ✅ Best REST practice
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Failed to clear run queue",
                detail: ex.Message
            );
        }
    }

    // ✅ GET /api/batch/{batchName}
    [HttpGet("{batchName}")]
    public IActionResult GetBatch(string batchName)
    {
        try
        {
            var batch = _batchService.LoadBatchDefinition(batchName);
            batch.BatchName = batchName;
            return Ok(batch);
        }
        catch (FileNotFoundException)
        {
            return NotFound($"Batch '{batchName}' not found");
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Failed to load batch definition",
                detail: ex.Message
            );
        }
    }


    // ✅ POST /api/batch/start/{batchName}  ← THIS FIXES YOUR ERROR
    [HttpPost("start/{batchName}")]
    public IActionResult StartBatch(string batchName)
    {
        try
        {
            _batchService.StartBatch(batchName);
            return Ok();
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Failed to start batch",
                detail: ex.Message
            );
        }
    }
    // ✅ GET /api/batch/summary
    [HttpGet("summary")]
    public IActionResult GetBatchRunSummary()
    {
        try
        {
            var summary = _batchService.GetLatestBatchRunSummary();

            if (summary == null)
                return NotFound("No completed batch run found.");

            return Ok(summary);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Failed to compute batch run summary",
                detail: ex.Message
            );
        }
    }
}
