using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using SatoshiForge.Api.Contracts.Auth;
using SatoshiForge.Application.Abstractions.Dispatching;
using SatoshiForge.Application.Features.Auth.Commands.RegisterUser;

namespace SatoshiForge.Api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password);

        var response = await commandDispatcher.DispatchAsync(command, cancellationToken);

        return Created(string.Empty, response);
    }
}