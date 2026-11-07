using Application.Features.AI.Commands.Summarize;
using FluentValidation.TestHelper;

namespace Application.Tests.Features.AI;

public class SummarizeCommandValidatorTests
{
    private readonly SummarizeCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidText_HasNoErrors()
    {
        var result = _validator.TestValidate(new SummarizeCommand { Text = "A valid document text." });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyText_HasRequiredError(string text)
    {
        var result = _validator.TestValidate(new SummarizeCommand { Text = text });
        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("abcd")]
    public void Validate_TextShorterThanFiveCharacters_HasLengthError(string text)
    {
        var result = _validator.TestValidate(new SummarizeCommand { Text = text });
        result.ShouldHaveValidationErrorFor(x => x.Text);
    }

    [Fact]
    public void Validate_FiveCharacterText_IsAccepted()
    {
        var result = _validator.TestValidate(new SummarizeCommand { Text = "12345" });
        result.ShouldNotHaveValidationErrorFor(x => x.Text);
    }
}
