using SatoshiForge.Domain.Entities;

namespace SatoshiForge.Application.Abstractions.Auth;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}