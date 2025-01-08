
namespace Api8.Models;

public class AppSettingsOptions : IValidatableObject
{
    public const string SectionName = "AppSettings";
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string SomeKey { get; set; }
    
    [Required]
    [RegularExpression(@"^\d{1,3}(\.\d{1,3}){3}$", ErrorMessage = "Invalid IP address format.")]
    public required string SmtpIp { get; set; }
    
    [Range(1, 65535, ErrorMessage = "Port number must be between 1 and 65535.")]
    public int SmtpPort { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SmtpIp == "127.0.0.1" && SmtpPort != 25)
        {
            yield return new ValidationResult(
                "Invalid port number for localhost SMTP server.",
                [nameof(SmtpPort)]);
        }

        yield return ValidationResult.Success!;
    }
}

// public class SmtpOptions
// {
//     public required string Ip { get; set; }
//     public int Port { get; set; }
// }