using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SatoshiForge.IntegrationTests.Common;

namespace SatoshiForge.IntegrationTests.Features.Auth;

public sealed class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_Should_Return_Created()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = "integration@test.com",
            Password = "123456"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Login_Should_Return_Ok_With_Token()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = "login@test.com",
            Password = "123456"
        });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = "login@test.com",
            Password = "123456"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        payload.Should().NotBeNull();
        payload!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Me_Should_Return_Ok_When_Token_Is_Valid()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = "me@test.com",
            Password = "123456"
        });

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            Email = "me@test.com",
            Password = "123456"
        });

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginPayload.Should().NotBeNull();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginPayload!.AccessToken);

        var response = await _client.GetAsync("/api/v1/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record LoginResponse(string AccessToken);
}