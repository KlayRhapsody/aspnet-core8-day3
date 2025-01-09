
using Api8.Services;

namespace Api8.Models.Validations;

public class AppSettingsValidation : IValidateOptions<AppSettingsOptions>
{
    private readonly IAllowedIpService _allowedIpService;

    public AppSettingsValidation(IAllowedIpService allowedIpService)
    {
        _allowedIpService = allowedIpService;
    }

    public ValidateOptionsResult Validate(string? name, AppSettingsOptions options)
    {
        var errors = new List<string>();

        Console.WriteLine(options.SomeKey);

        if (!_allowedIpService.IsAllowedIp(options.SmtpIp))
        {
            errors.Add($"IP address {options.SmtpIp} is not allowed.");
        }

        if (options.SmtpPort == 25 && !options.SomeKey.Contains("Admin"))
        {
            errors.Add("When SmtpPort is 25, SomeKey must contain the word 'Admin'.");
        }

        if (errors.Any())
        {
            return ValidateOptionsResult.Fail(string.Join("; ", errors));
        }

        return ValidateOptionsResult.Success;
    }
}