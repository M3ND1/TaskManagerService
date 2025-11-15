using FluentValidation;

namespace TaskManager.Application.Validators;
/// <summary>
/// Contains validation rules and patterns for common data validation scenarios.
/// Provides extension methods for FluentValidation rule builders.
/// </summary>
public static class ValidationRules
{
    /// <summary>
    /// Regex for validating usernames (letters, numbers, dots, hyphens, underscores).
    /// </summary>
    public const string UsernamePattern = @"^[a-zA-Z0-9_.-]+$";

    /// <summary>
    /// Regex for validating names (letters, spaces, hyphens, apostrophes, and accented characters).
    /// </summary>
    public const string NamePattern = @"^[a-zA-ZÀ-ÿ\s'-]+$";

    /// <summary>
    /// Regex pattern for validating phone numbers (digits only).
    /// </summary>
    public const string PhonePattern = @"^\d+$";

    /// <summary>
    /// Regex pattern for special characters used in password validation.
    /// </summary>
    public const string SpecialCharacterPattern = @"[][""!@$%^&*(){}:;<>,.?/+_=|'~\\-]";

    /// <summary>
    /// Applies password validation rules to a string property.
    /// Validates minimum length, uppercase, lowercase, digit, and special character requirements.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to apply the rules to.</param>
    /// <returns>A configured rule builder with password validation rules applied.</returns>
    public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one digit.")
            .Matches(SpecialCharacterPattern).WithMessage("Password must contain at least one special character.")
            .Must(ValidationHelpers.BeAStrongPassword).WithMessage("Please provide strong password for your account security.");
    }

    /// <summary>
    /// Applies email validation rules to a string property.
    /// Validates that the email is not empty and follows proper email format.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to apply the rules to.</param>
    /// <returns>A configured rule builder with email validation rules applied.</returns>
    public static IRuleBuilderOptions<T, string> ApplyEmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please enter a valid email format.");
    }

    /// <summary>
    /// Applies username validation rules to a string property.
    /// Validates that the username is not empty, has proper length, and contains only allowed characters.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to apply the rules to.</param>
    /// <returns>A configured rule builder with username validation rules applied.</returns>
    public static IRuleBuilderOptions<T, string> ApplyUsernameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.")
            .Matches(UsernamePattern).WithMessage("Username can only contain letters, numbers, dots, hyphens, and underscores.");
    }

    /// <summary>
    /// Applies name validation rules to a string property.
    /// Validates that the name is not empty, has proper length, and contains only allowed characters.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to apply the rules to.</param>
    /// <param name="fieldName">The name of the field being validated for customized error messages.</param>
    /// <returns>A configured rule builder with name validation rules applied.</returns>
    public static IRuleBuilderOptions<T, string> ApplyNameRules<T>(this IRuleBuilder<T, string> ruleBuilder, string fieldName)
    {
        return ruleBuilder
            .NotEmpty().WithMessage($"{fieldName} is required.")
            .Length(1, 250).WithMessage($"{fieldName} must be between 1 and 250 characters.")
            .Matches(NamePattern).WithMessage($"{fieldName} can only contain letters, spaces, hyphens, and apostrophes.");
    }

    /// <summary>
    /// Applies phone number validation rules to a nullable string property.
    /// Validates that the phone number has proper length and contains only digits.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder to apply the rules to.</param>
    /// <returns>A configured rule builder with phone number validation rules applied.</returns>
    public static IRuleBuilderOptions<T, string?> ApplyPhoneRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Length(9, 15).WithMessage("Phone number must be between 9 and 15 digits.")
            .Matches(PhonePattern).WithMessage("Phone number can only contain digits.");
    }
}

