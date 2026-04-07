using Microsoft.AspNetCore.Mvc;
using Hplc.Controller.Api.Data;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/status")]
public class InstrumentStatusController : ControllerBase
{
    [HttpGet("lcs")]
    public IActionResult GetLcStatus()
    {
        return Ok(InstrumentStatusStore.Lcs);
    }

    [HttpGet("ms")]
    public IActionResult GetMsStatus()
    {
        return Ok(InstrumentStatusStore.Ms);
    }
}