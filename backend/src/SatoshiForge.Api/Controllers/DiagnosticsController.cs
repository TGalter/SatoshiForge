using Microsoft.AspNetCore.Mvc;

namespace SatoshiForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DiagnosticsController : ControllerBase
{
    private readonly ILogger<DiagnosticsController> _logger;

    public DiagnosticsController(ILogger<DiagnosticsController> logger)
    {
        _logger = logger;
    }

    [HttpGet("logs-test")]
    public IActionResult LogsTest()
    {
        _logger.LogInformation(
            "Executando teste de logs para o endpoint {Endpoint} com trace local.",
            HttpContext.Request.Path);

        return Ok(new { message = "logs ok" });
    }
}