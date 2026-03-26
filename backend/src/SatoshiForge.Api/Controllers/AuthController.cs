using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatoshiForge.Api.Contracts.Auth;
using SatoshiForge.Application.Abstractions.Dispatching;
using SatoshiForge.Application.Features.Auth.Commands.LoginUser;
using SatoshiForge.Application.Features.Auth.Commands.RegisterUser;
using SatoshiForge.Application.Features.Auth.Queries.GetCurrentUser;

namespace SatoshiForge.Api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher) : ControllerBase
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

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(
    [FromBody] LoginUserRequest request,
    CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(
            request.Email,
            request.Password);

        var response = await commandDispatcher.DispatchAsync(command, cancellationToken);

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(GetCurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var query = new GetCurrentUserQuery();

        var response = await queryDispatcher.DispatchAsync(query, cancellationToken);

        return Ok(response);
    }
}