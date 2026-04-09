using Microsoft.AspNetCore.Mvc;
using Hplc.Controller.Api.Models;
using Hplc.Controller.Api.Services;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/batch")]
public class BatchController : ControllerBase
{
    private readonly BatchFileService _batchFileService;

    public BatchController(BatchFileService batchFileService)
    {
        _batchFileService = batchFileService;
    }

    // =====================================================
    // ✅ AVAILABLE BATCH DEFINITIONS (NEW & PERMANENT)
    // =====================================================

    // This is what Angular should call:
    // GET /api/batch
    [HttpGet]
    public IActionResult GetAvailableBatches()
    {
        // For now, static list.
        // Later this can come from DB, batch files, LMS, etc.
        return Ok(new[]
        {
            "Batch_Impurity_Run",
            "Batch_Mixed_Run",
            "d",
            "eee"
        });
    }

    // =====================================================
    // RUN QUEUE APIs (EXISTING – UNCHANGED)
    // =====================================================

    [HttpGet("queue")]
    public IActionResult GetBatchRunQueue()
    {
        return Ok(_batchFileService.GetBatchRunQueue());
    }

    [HttpPost("enqueue/{batchName}")]
    public IActionResult EnqueueBatch(string batchName)
    {
        _batchFileService.EnqueueBatch(batchName);
        return Ok();
    }

    [HttpPost("start/{batchName}")]
    public IActionResult StartBatch(string batchName)
    {
        _batchFileService.StartBatch(batchName);
        return Ok();
    }

    [HttpDelete("{batchName}")]
    public IActionResult RemoveFromQueue(string batchName)
    {
        _batchFileService.RemoveFromQueue(batchName);
        return Ok();
    }

    [HttpPost("clear")]
    public IActionResult ClearRunQueue()
    {
        _batchFileService.ClearRunQueue();
        return Ok();
    }

    // =====================================================
    // SAVE BATCH DEFINITION → EXECUTION RUN
    // =====================================================

    [HttpPost("save")]
    public IActionResult SaveBatch([FromBody] Batch batch)
    {
        if (batch == null || string.IsNullOrWhiteSpace(batch.BatchName))
            return BadRequest("Invalid batch");

        var runInfo = MapBatchToBatchRunInfo(batch);
        _batchFileService.SaveBatch(runInfo);

        return Ok();
    }

    // =====================================================
    // SAMPLE EXECUTION
    // =====================================================

    [HttpGet("{batchName}/samples")]
    public IActionResult GetSamples(string batchName)
    {
        return Ok(_batchFileService.GetSamples(batchName));
    }

    // =====================================================
    // PRIVATE MAPPER
    // =====================================================

    private static BatchRunInfo MapBatchToBatchRunInfo(Batch batch)
    {
        return new BatchRunInfo
        {
            BatchName = batch.BatchName,
            Status = BatchRunStatus.Queued,
            OwnsMS = false,
            QueuePosition = null,
            Samples = batch.Samples.Select(sample => new SampleExecutionInfo
            {
                BatchName = batch.BatchName,
                SampleName = sample.SampleName,
                State = SampleExecutionState.Queued
            }).ToList()
        };
    }
}
