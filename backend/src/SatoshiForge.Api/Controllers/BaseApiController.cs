using Microsoft.AspNetCore.Mvc;
using SatoshiForge.Shared.Responses;

namespace SatoshiForge.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.Ok(data, message));
    }

    protected ActionResult<ApiResponse> Success(string? message = null)
    {
        return Ok(ApiResponse.Ok(message));
    }
}