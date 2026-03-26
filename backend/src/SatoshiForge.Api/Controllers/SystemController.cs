using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SatoshiForge.Application.Common.Responses;
using SatoshiForge.Shared.Responses;

namespace SatoshiForge.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/system")]
public class SystemController : BaseApiController
{
    [HttpGet("ping")]
    [ProducesResponseType(typeof(ApiResponse<SystemPingResponse>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<SystemPingResponse>> Ping()
    {
        var response = new SystemPingResponse
        {
            Message = "pong",
            Version = "1.0"
        };

        return Success(response);
    }
}