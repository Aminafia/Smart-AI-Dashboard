using Application.Features.Auth.Commands.Login;

using FluentValidation.TestHelper;

namespace Application.Tests.Features.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCredentials_HasNoErrors()
    {
        var result = _validator.TestValidate(new LoginCommand { Email = "amina@example.com", Password = "secret1" });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmail_HasRequiredError(string email)
    {
        var result = _validator.TestValidate(new LoginCommand { Email = email, Password = "secret1" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("amina@")]
    [InlineData("@example.com")]
    public void Validate_InvalidEmail_HasFormatError(string email)
    {
        var result = _validator.TestValidate(new LoginCommand { Email = email, Password = "secret1" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void Validate_EmptyOrShortPassword_HasError(string password)
    {
        var result = _validator.TestValidate(new LoginCommand { Email = "amina@example.com", Password = password });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_SixCharacterPassword_IsAccepted()
    {
        var result = _validator.TestValidate(new LoginCommand { Email = "amina@example.com", Password = "123456" });
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
