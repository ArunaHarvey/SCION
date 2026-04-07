using Microsoft.AspNetCore.Mvc;
using Hplc.Controller.Api.Services;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/batch/run")]
public class BatchRunController : ControllerBase
{
    private readonly BatchFileService _service;

    public BatchRunController(BatchFileService service)
    {
        _service = service;
    }

    [HttpGet("queue")]
    public IActionResult GetQueue()
        => Ok(_service.GetBatchRunQueue());

    [HttpPost("queue/{batchName}")]
    public IActionResult Enqueue(string batchName)
    {
        _service.EnqueueBatch(batchName);
        return Ok();
    }

    [HttpDelete("queue/{batchName}")]
    public IActionResult Delete(string batchName)
    {
        _service.RemoveFromQueue(batchName);
        return Ok();
    }

    [HttpPost("start/{batchName}")]
    public IActionResult Start(string batchName)
    {
        _service.StartBatch(batchName);
        return Ok();
    }

    [HttpPost("clear")]
    public IActionResult Clear()
    {
        _service.ClearRunQueue();
        return Ok();
    }

    [HttpGet("execution")]
    public IActionResult Execution()
        => Ok(BatchExecutionStore.Executions);
}