using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BugscapeMVC.Services.Validation
{
    public class RequiredHtmlCleanAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string str)
            {
                var withoutHtml = Regex.Replace(str, "<.*?>", string.Empty);
                if (string.IsNullOrWhiteSpace(withoutHtml))
                {
                    return new ValidationResult("Message is required.");
                }
            }

            return ValidationResult.Success!;
        }
    }
}