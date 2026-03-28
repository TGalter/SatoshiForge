using SatoshiForge.Application.Abstractions.CQRS;

namespace SatoshiForge.Application.Abstractions.Dispatching;

public interface IQueryDispatcher
{
    Task<TResult> DispatchAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default);
}