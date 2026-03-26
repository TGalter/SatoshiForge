using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using SatoshiForge.Application.Abstractions.CQRS;
using SatoshiForge.Application.Abstractions.Dispatching;
using SatoshiForge.Application.Exceptions;

namespace SatoshiForge.Infrastructure.Dispatching;

public sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task<TResult> DispatchAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        await ValidateAsync(command, cancellationToken);

        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = serviceProvider.GetRequiredService(handlerType);

        return await handler.HandleAsync((dynamic)command, cancellationToken);
    }

    private async Task ValidateAsync<TResult>(
        ICommand<TResult> command,
        CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(command.GetType());
        var validators = serviceProvider.GetServices(validatorType).Cast<IValidator>();

        if (!validators.Any())
            return;

        var context = new ValidationContext<object>(command);
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