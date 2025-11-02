using FluentValidation;

namespace TaskManager.Application.Validators
{
    public static class ValidationRules
    {
        public const string EmailPattern = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        public const string UsernamePattern = @"^[a-zA-Z0-9]+$";
        public const string NamePattern = @"^[a-zA-Z]+$";
        public const string PhonePattern = @"^\d+$";
        public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"\d").WithMessage("Password must contain at least one digit.")
                .Matches(@"[][""!@$%^&*(){}:;<>,.?/+_=|'~\\-]").WithMessage("Password must contain at least one special character.");
        }
        public static IRuleBuilderOptions<T, string> ApplyEmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Please enter a valid email address.")
                .Matches(EmailPattern).WithMessage("Please enter a valid email format.");
        }
        public static IRuleBuilderOptions<T, string> ApplyUsernameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("Username is required.")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters.")
                .Matches(UsernamePattern).WithMessage("Username can only contain letters, numbers, dots, hyphens, and underscores.");
        }
        public static IRuleBuilderOptions<T, string> ApplyNameRules<T>(this IRuleBuilder<T, string> ruleBuilder, string fieldName)
        {
            return ruleBuilder
                .NotEmpty().WithMessage($"{fieldName} is required.")
                .Length(1, 250).WithMessage($"{fieldName} must be between 1 and 250 characters.")
                .Matches(NamePattern).WithMessage($"{fieldName} can only contain letters, spaces, hyphens, and apostrophes.");
        }
        public static IRuleBuilderOptions<T, string?> ApplyPhoneRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Length(9, 15).WithMessage("Phone number must be between 9 and 15 digits.")
                .Matches(PhonePattern).WithMessage("Phone number can only contain digits.")
                .When(x => !string.IsNullOrEmpty(ruleBuilder.ToString()));
        }
    }
}
