using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Hplc.Controller.Api.Controllers;

[ApiController]
[Route("api/batch-definitions")]
public class BatchDefinitionsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public BatchDefinitionsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Returns all available batch definitions by reading files
    /// from the Data/Batches folder.
    ///
    /// Angular calls: GET /api/batch-definitions
    /// </summary>
    [HttpGet]
    public IActionResult GetAllBatchDefinitions()
    {
        var batchesFolder = Path.Combine(
            _env.ContentRootPath,
            "Data",
            "Batches"
        );

        if (!Directory.Exists(batchesFolder))
        {
            return Ok(Array.Empty<string>());
        }

        var batchNames = Directory
            .EnumerateFiles(batchesFolder, "*.*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name)
            .ToList();

        return Ok(batchNames);
    }
}