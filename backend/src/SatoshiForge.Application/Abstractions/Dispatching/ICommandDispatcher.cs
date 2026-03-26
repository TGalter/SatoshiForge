using SatoshiForge.Application.Abstractions.CQRS;

namespace SatoshiForge.Application.Abstractions.Dispatching;

public interface ICommandDispatcher
{
    Task<TResult> DispatchAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default);
}