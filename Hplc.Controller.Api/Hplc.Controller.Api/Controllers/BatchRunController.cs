using Microsoft.AspNetCore.Mvc;
using Hplc.Controller.Api.Services;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/batch/run")]
public class BatchRunController : ControllerBase
{
    private readonly BatchFileService _svc;
    public BatchRunController(BatchFileService svc) => _svc = svc;

    [HttpGet("queue")]
    public IActionResult Queue()
        => Ok(_svc.GetBatchRunQueue());

    [HttpPost("start/{name}")]
    public IActionResult Start(string name)
    {
        _svc.StartBatch(name);
        return Ok();
    }

    [HttpDelete("queue/{name}")]
    public IActionResult Remove(string name)
    {
        _svc.RemoveFromQueue(name);
        return Ok();
    }

    [HttpPost("clear")]
    public IActionResult Clear()
    {
        _svc.ClearRunQueue();
        return Ok();
    }
}