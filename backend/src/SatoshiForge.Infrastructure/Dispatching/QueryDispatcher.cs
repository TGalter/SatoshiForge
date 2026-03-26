using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using SatoshiForge.Application.Abstractions.CQRS;
using SatoshiForge.Application.Abstractions.Dispatching;
using SatoshiForge.Application.Exceptions;

namespace SatoshiForge.Infrastructure.Dispatching;

public sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public async Task<TResult> DispatchAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        await ValidateAsync(query, cancellationToken);

        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        dynamic handler = serviceProvider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)query, cancellationToken);
    }

    private async Task ValidateAsync<TResult>(
        IQuery<TResult> query,
        CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(query.GetType());
        var validators = serviceProvider.GetServices(validatorType).Cast<IValidator>();

        if (!validators.Any())
            return;

        var context = new ValidationContext<object>(query);
        var failures = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            if (!result.IsValid)
                failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
            return;

        var errors = failures
            .Where(x => x is not null)
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).Distinct().ToArray());

        throw new ApplicationValidationException(errors);
    }
}