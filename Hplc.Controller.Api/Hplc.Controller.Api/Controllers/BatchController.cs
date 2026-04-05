using Hplc.Controller.Api.Services;
using Hplc.Controller.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/batch")]
public class BatchController : ControllerBase
{
    private readonly BatchFileService _service;

    public BatchController(BatchFileService service)
    {
        _service = service;
    }

    // ✅ REQUIRED: GET /api/batch
    [HttpGet]
    public IActionResult GetAllBatches()
    {
        return Ok(_service.GetAllBatches());
    }

    // GET /api/batch/{batchName}
    [HttpGet("{batchName}")]
    public IActionResult GetBatch(string batchName)
    {
        return Ok(_service.GetBatch(batchName));
    }

    // GET /api/batch/{batchName}/samples
    [HttpGet("{batchName}/samples")]
    public IActionResult GetSamples(string batchName)
    {
        return Ok(_service.GetSamples(batchName));
    }

    // POST /api/batch
    [HttpPost]
    public IActionResult SaveBatch([FromBody] Batch batch)
    {
        _service.SaveBatch(batch);
        return Ok();
    }
}
