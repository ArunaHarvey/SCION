using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Hplc.Controller.Api.Controllers
{
    [ApiController]
    [Route("api/methods")]
    public class MethodController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public MethodController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        [HttpGet]
        public IActionResult GetMethods()
        {
            string folder = _configuration["HplcConfig:MethodsFolder"]!;
            string file = _configuration["HplcConfig:MethodsFile"]!;

            var path = Path.Combine(_env.ContentRootPath, folder, file);

            if (!System.IO.File.Exists(path))
                return NotFound("methods.json not found");

            var json = System.IO.File.ReadAllText(path);

            // ✅ Deserialize ROOT ARRAY
            var methods = JsonSerializer.Deserialize<List<object>>(json);

            return Ok(methods);
        }
    }
}