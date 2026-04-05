using Hplc.Controller.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/batch/run")]
public class BatchRunController : ControllerBase
{
    private readonly BatchFileService _batchService;

    public BatchRunController(BatchFileService batchService)
    {
        _batchService = batchService;
    }

    [HttpGet("queue")]
    public IActionResult GetQueue()
    {
        return Ok(_batchService.GetBatchRunQueue());
    }

    [HttpPost("queue/{batchName}")]
    public IActionResult AddToQueue(string batchName)
    {
        _batchService.EnqueueBatch(batchName);
        return Ok();
    }
}