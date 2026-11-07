using Application.Features.Users.Commands.CreateUser;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.Users;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidUser_HasNoErrors()
    {
        var result = _validator.TestValidate(new CreateUserCommand("amina@example.com", "Amina", "secret1"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void Validate_InvalidEmail_HasError(string email)
    {
        var result = _validator.TestValidate(new CreateUserCommand(email, "Amina", "secret1"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_HasError(string name)
    {
        var result = _validator.TestValidate(new CreateUserCommand("amina@example.com", name, "secret1"));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Validate_EmptyOrShortPassword_HasError(string password)
    {
        var result = _validator.TestValidate(new CreateUserCommand("amina@example.com", "Amina", password));
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
