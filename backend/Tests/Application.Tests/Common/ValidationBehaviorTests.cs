using Application.Common.Behaviors;

using FluentValidation;

using MediatR;

namespace Application.Tests.Common;

public class ValidationBehaviorTests
{
    private sealed record TestRequest(string Name) : IRequest<string>;

    [Fact]
    public async Task Handle_NoValidators_ExecutesNextHandler()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () =>
        {
            nextCalled = true;
            return Task.FromResult("handled");
        };

        var result = await behavior.Handle(new TestRequest("valid"), next, CancellationToken.None);

        Assert.Equal("handled", result);
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task Handle_ValidRequest_ExecutesNextHandler()
    {
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(request => request.Name).NotEmpty();
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);

        var result = await behavior.Handle(
            new TestRequest("Amina"),
            () => Task.FromResult("handled"),
            CancellationToken.None);

        Assert.Equal("handled", result);
    }

    [Fact]
    public async Task Handle_MultipleValidationFailures_AggregatesErrorsAndSkipsHandler()
    {
        var first = new InlineValidator<TestRequest>();
        first.RuleFor(request => request.Name).NotEmpty().WithMessage("Name is required.");
        var second = new InlineValidator<TestRequest>();
        second.RuleFor(request => request.Name).MinimumLength(5).WithMessage("Name is too short.");
        var behavior = new ValidationBehavior<TestRequest, string>([first, second]);
        var nextCalled = false;

        RequestHandlerDelegate<string> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult("handled");
        };

        var exception = await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(new TestRequest(""), next, CancellationToken.None));

        Assert.Equal(2, exception.Errors.Count());
        Assert.Contains(exception.Errors, error => error.ErrorMessage == "Name is required.");
        Assert.Contains(exception.Errors, error => error.ErrorMessage == "Name is too short.");
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToValidators()
    {
        var token = new CancellationTokenSource().Token;
        CancellationToken validatorToken = default;
        var validator = new InlineValidator<TestRequest>();
        validator.RuleFor(request => request.Name).MustAsync((_, cancellationToken) =>
        {
            validatorToken = cancellationToken;
            return Task.FromResult(true);
        });
        var behavior = new ValidationBehavior<TestRequest, string>([validator]);

        await behavior.Handle(
            new TestRequest("Amina"),
            () => Task.FromResult("handled"),
            token);

        Assert.Equal(token, validatorToken);
    }
}
