using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Application.Common.Behaviors;

/// <summary>
///  This class intercepts every request going through MediatR
/// - TRequest- LoginCommand, CreateUserCommand, etc.
/// - TResponse- LoginResponse, etc.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// DI injects all validators for the given TRequest. For example, if TRequest is LoginCommand, it will inject LoginCommandValidator.
    /// </summary>
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(e => e != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}